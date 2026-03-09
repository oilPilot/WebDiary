using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebDiary.Data;
using WebDiary.Entities;
using WebDiary.Model;
using WebDiary.Resources;

namespace WebDiary.Controller;

[Route("auth")]
[ApiController]
public class AuthController (DiariesContext dbContext, IConfiguration config,
                            IStringLocalizer<ErrorResource> localizer,
                            IAuthService authService, IEmailSenderService emailSenderService) : ControllerBase
{

    [AllowAnonymous]
    [HttpPost("jwttoken/login")]
    public async Task<IActionResult> LoginAsync(LoginModel model) {
        if(string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrEmpty(model.Password))
        {
            return BadRequest(localizer["InvalidNameOrPswd"].Value);
        }

        User? user = dbContext.users.FirstOrDefault(user => user.UserName == model.Username);
        if(user == null) {
            return Unauthorized(localizer["InvalidNameOrPswd"].Value);
        }
        var hasher = new PasswordHasher<User>();
        var verify = hasher.VerifyHashedPassword(user, user.Password, model.Password!);
        if(verify == PasswordVerificationResult.Success) {
            Log.Information("User {Name} tries to login with correct password at {currentTime}", user.UserName, DateTime.Now);
            return await CreatingTokens(user);
        }
        return Unauthorized(localizer["InvalidNameOrPswd"].Value);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(TokenRequestModel tokenModel) {
        if(string.IsNullOrWhiteSpace(tokenModel.AccessToken) || string.IsNullOrWhiteSpace(tokenModel.RefreshToken))
        {
            return BadRequest(localizer["RefreshTokenError"].Value);
        }

        var principal = GetPrincipalFromExpiredToken(tokenModel.AccessToken);
        if(principal?.Identity?.Name == null)
        {
            return BadRequest(localizer["RefreshTokenError"].Value);
        }

        var user = await dbContext.users.FirstOrDefaultAsync(user => user.UserName == principal.Identity!.Name);
        if(user == null) {
            Log.Error<string>("Upon refresh user wasn't found, name {principalName}", principal.Identity!.Name);
            return BadRequest(localizer["RefreshTokenError"].Value);
        }
        if(user.RefreshToken != tokenModel.RefreshToken || user.RefreshTokenDateEnd <= DateTime.UtcNow) {
            Log.Warning("Refresh for user {Name} has been unsuccessful. refreshTokenExpDate: {RefreshTokenExpTime}",
                            user.UserName, user.RefreshTokenDateEnd);
            return BadRequest(localizer["RefreshTokenError"].Value);
        }

        Log.Information("Refreshing token for user name {Name}", user.UserName);
        return await CreatingTokens(user, false);
    }

    private async Task<IActionResult> CreatingTokens(User user, bool populateExpire = true) {
        var tokens = await authService.CreatingTokens(user, populateExpire);
        return Ok(new { token = tokens[0], refreshToken = tokens[1] });
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token) {
        var TokenValidationParameters = new TokenValidationParameters {
            ValidateLifetime = false,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"]
        };

        try {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, TokenValidationParameters, out var securityToken);
            var JwtSecurityToken = (JwtSecurityToken)securityToken;
            if(securityToken == null || !JwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)) {
                Log.Error("Upon GetPrincipalFromExpiredToken token was invalid. Algorithm: {Alg}",
                    JwtSecurityToken.Header.Alg);
                throw new Exception("Invalid token.");
            }
            return principal;
        } catch(Exception Ex) {
            Log.Error("Catched exception at GetPrincipalFromExpiredToken: {Exception}", Ex);
            return null;
        }
    }

    [AllowAnonymous]
    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPasswordAsync(resetPasswordModel resetPasswordForm) {
        var user = await dbContext.users.FindAsync(resetPasswordForm.UserId);
        if (user is null) {
            return NotFound(localizer["InvalidNameOrPswd"].Value);
        }
        if(user.ActionDateEnd != null && user.ActionDateEnd > DateTime.UtcNow) {
            var Base64Token = Convert.FromBase64String( resetPasswordForm.Token.Replace('-', '+').Replace('_', '/') );
            if(user.ActionToken != null && CryptographicOperations.FixedTimeEquals(user.ActionToken, Base64Token) ) {
                await authService.ResetPassword(user, resetPasswordForm.newPassword);
                return Ok("Resetted successfully");
            } else {
                Log.Warning("ResetPassword token mismatch for user {UserId}", user.Id);
                return BadRequest(localizer["TokenNotEqual"].Value);
            }
        } else {
            Log.Information("ResetPassword was unsuccessful, actionDateEnd {DateEnd}", user.ActionDateEnd);
            return BadRequest(localizer["TokenTimeExpired"].Value);
        }
    }
    [Authorize]
    [HttpPost("password/verify")]
    public async Task<IActionResult> VerifyPasswordAsync(VerifyPasswordModel passwordRequest)
    {
        var currentUserId = GetCurrentUserId();
        if(currentUserId is null)
        {
            return Unauthorized();
        }

        var verify = await authService.CheckPasswordEquality(passwordRequest.Password, currentUserId.Value);
        if (verify == PasswordVerificationResult.Success)
            return Ok("Are equal");
        return BadRequest(localizer["PasswordsNotEqual"].Value);
    }
    [Authorize]
    [HttpPost("pincode/verify")]
    public async Task<IActionResult> VerifyPinAsync(VerifyPinModel pinRequest)
    {
        var currentUserId = GetCurrentUserId();
        if(currentUserId is null)
        {
            return Unauthorized();
        }

        var group = await dbContext.diaryGroups.AsNoTracking().FirstOrDefaultAsync(group => group.Id == pinRequest.GroupId);
        if(group is null)
        {
            return NotFound();
        }
        if(!User.IsInRole("Admin") && group.UserId != currentUserId.Value)
        {
            return Forbid();
        }

        var verify = await authService.CheckPinEquality(pinRequest.Pin, pinRequest.GroupId);
        if (verify == PasswordVerificationResult.Success)
            return Ok("Are equal");
        return BadRequest(localizer["PasswordsNotEqual"].Value);
    }

    [Authorize]
    [HttpPost("masterpassword/verify")]
    public async Task<IActionResult> VerifyMasterPasswordAsync(VerifyMasterPasswordModel masterPasswordRequest)
    {
        var currentUserId = GetCurrentUserId();
        if(currentUserId is null)
        {
            return Unauthorized();
        }

        var verify = await authService.CheckMasterPasswordEquality(masterPasswordRequest.MasterPassword, currentUserId.Value);
        if (verify == PasswordVerificationResult.Success)
            return Ok("Are equal");
        return BadRequest(localizer["PasswordsNotEqual"].Value);
    }

    // administrative endpoint that allows an admin to impersonate another user by
    // providing fresh JWT tokens for that account. This is intentionally protected
    // by the "Admin" role attribute.
    [Authorize(Roles = "Admin")]
    [HttpPost("impersonate/{userId}")]
    public async Task<IActionResult> ImpersonateUser(int userId)
    {
        var user = await dbContext.users.FindAsync(userId);
        if (user == null)
            return NotFound();

        Log.Information("Administrator {Admin} impersonating user {Impersonated}",
            User.Identity?.Name, user.UserName);
        return await CreatingTokens(user);
    }

    [Authorize]
    [HttpGet("masterpassword/isset")]
    public async Task<IActionResult> IsMasterPasswordSetAsync()
    {
        var currentUserId = GetCurrentUserId();
        if(currentUserId is null)
        {
            return Unauthorized();
        }

        var user = await dbContext.users.FindAsync(currentUserId.Value);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(!string.IsNullOrEmpty(user.MasterPassword));
    }

    [Authorize]
    [HttpPost("masterpassword/set")]
    public async Task<IActionResult> SetMasterPasswordAsync(SetMasterPasswordModel masterPasswordRequest)
    {
        var currentUserId = GetCurrentUserId();
        if(currentUserId is null)
        {
            return Unauthorized();
        }

        await authService.SetMasterPassword(currentUserId.Value, masterPasswordRequest.MasterPassword);
        return Ok("Master password set");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("masterpassword/set/{userId:int}")]
    public async Task<IActionResult> SetMasterPasswordForUserAsync(int userId, SetMasterPasswordModel masterPasswordRequest)
    {
        await authService.SetMasterPassword(userId, masterPasswordRequest.MasterPassword);
        return Ok("Master password set for user");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("masterpassword/isset/{userId:int}")]
    public async Task<IActionResult> IsMasterPasswordSetForUserAsync(int userId)
    {
        var user = await dbContext.users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(!string.IsNullOrEmpty(user.MasterPassword));
    }
    
    // THERE ARE GOES METHODS THAT REQUIRE EMAILS
    [AllowAnonymous]
    [HttpGet("email/isunique/{email}")]
    public IActionResult IsUniqueEmailAsync(string email) {
        return Ok("NO EMAILS");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("sendEmail")]
    public async Task<IActionResult> SendEmailAsync(sendEmailModel email)
    {
        if (email.To == null || email.Subject == null || email.Body == null)
            return BadRequest("Invalid email data");
        var response = await emailSenderService.SendEmail(email.To, email.Subject, email.Body);
        if (!response.IsSuccessful)
            return StatusCode(500, "Failed to send email: " + response.ErrorMessage);
        return Ok("Email sended successfully");
    }
    
    [AllowAnonymous]
    [HttpPost("ValidateEmail")]
    public IActionResult ValidateEmailAsync(validateEmailModel ValidateEmailForm) {
        return Ok("Validated successfully"); // NO VALIDATION NEEDED ANYMORE
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

}
