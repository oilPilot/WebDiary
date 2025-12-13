using Microsoft.AspNetCore.Mvc;
using WebDiary.Entities;
using WebDiary.Model;
using Microsoft.EntityFrameworkCore;
using WebDiary.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using WebDiary.Resources;
using Microsoft.Extensions.Localization;
using Serilog;

namespace WebDiary.Controller;

[Route("auth")]
[ApiController]
public class AuthController (DiariesContext dbContext, IConfiguration config,
                            IStringLocalizer<ErrorResource> localizer,
                            IAuthService authService) : ControllerBase
{

    [HttpPost("jwttoken/login")]
    public async Task<IActionResult> LoginAsync(LoginModel model) {
        User? user = dbContext.users.FirstOrDefault(user => user.UserName == model.Username);
        if(user == null) {
            return NotFound(localizer["InvalidNameOrPswd"].Value);
        }
        var hasher = new PasswordHasher<User>();
        var verify = hasher.VerifyHashedPassword(user, user.Password, model.Password!);
        if(verify == PasswordVerificationResult.Success) {
            Log.Information("User {Name} tries to login with correct password at {currentTime}", user.UserName, DateTime.Now);
            return await CreatingTokens(user);
        }
        return Unauthorized(localizer["InvalidNameOrPswd"].Value);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(TokenRequestModel tokenModel) {
        var principal = GetPrincipalFromExpiredToken(tokenModel.AccessToken!);

        var user = await dbContext.users.FirstOrDefaultAsync(user => user.UserName == principal.Identity!.Name);
        if(user == null) {
            Log.Error<string>("Upon refresh user wasn't found, name {principalName}", principal.Identity!.Name);
            return BadRequest(localizer["RefreshTokenError"].Value);
        }
        if(user.RefreshToken != tokenModel.RefreshToken || user.RefreshTokenDateEnd <= DateTime.Now) {
            Log.Error("Refresh for user {Name} has been unsuccessful, refreshTokenExpDate: {RefreshTokenExpTime}," +
                            "user token {RefreshTokenUser}, sended token {RefreshTokenSended}",
                            user.UserName, user.RefreshTokenDateEnd, user.RefreshToken, tokenModel.RefreshToken);
            return BadRequest(localizer["RefreshTokenError"].Value);
        }

        Log.Information("Refreshing token for user name {Name}", user.UserName);
        return await CreatingTokens(user, false);
    }

    private async Task<IActionResult> CreatingTokens(User user, bool populateExpire = true) {
        var tokens = await authService.CreatingTokens(user, populateExpire);
        return Ok(new { token = tokens[0], refreshToken = tokens[1] });
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token) {
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
                Log.Error<SecurityToken, string>("Upon GetPrincipalFromExpiredToken token was inwalid {Token} Algorithm: {Alg}",
                                securityToken, JwtSecurityToken.Header.Alg);
                throw new Exception("Invalid token.");
            }
            return principal;
        } catch(Exception Ex) {
            Log.Error("Catched exception at GetPrincipalFromExpiredToken: {Exception}", Ex);
        }

        return new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>() { new Claim(ClaimTypes.Name, "") }));
    }

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
                Log.Error<byte[], byte[]>("ResetPassword was unsuccessful, token {token}, base64Token {base64Token}",
                                user.ActionToken, Base64Token);
                return BadRequest(localizer["TokenNotEqual"].Value);
            }
        } else {
            Log.Information("ResetPassword was unsuccessful, actionDateEnd {DateEnd}", user.ActionDateEnd);
            return BadRequest(localizer["TokenTimeExpired"].Value);
        }
    }
    [HttpGet("password/isequal/{password}/{userId:int}")]
    public async Task<IActionResult> IsEqualPasswordsAsync(string password, int userId)
    {
        var verify = await authService.CheckPasswordEquality(password, userId);
        if (verify == PasswordVerificationResult.Success)
            return Ok("Are equal");
        return BadRequest(localizer["PasswordsNotEqual"].Value);
    }
    [HttpGet("pincode/isequal/{pin}/{groupId:int}")]
    public async Task<IActionResult> IsEqualPinsAsync(string pin, int groupId)
    {
        var verify = await authService.CheckPinEquality(pin, groupId);
        if (verify == PasswordVerificationResult.Success)
            return Ok("Are equal");
        return BadRequest(localizer["PasswordsNotEqual"].Value);
    }
    
    // THERE ARE GOES METHODS THAT REQUIRE EMAILS
    [HttpGet("email/isunique/{email}")]
    public IActionResult IsUniqueEmailAsync(string email) {
        return Ok("NO EMAILS");
    }
    [HttpPost("sendEmail")]
    public IActionResult SendEmailAsync(sendEmailModel email)
    {
        return Ok("Email sended successfully"); // EMAIL DELETED
    }
    
    [HttpPost("ValidateEmail")]
    public IActionResult ValidateEmailAsync(validateEmailModel ValidateEmailForm) {
        return Ok("Validated successfully"); // NO VALIDATION NEEDED ANYMORE
    }

}
