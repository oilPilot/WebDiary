using System;
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
using WebDiary.DTO;
using Serilog;

namespace WebDiary.Controller;

[Route("auth")]
[ApiController]
public class AuthController (DiariesContext dbContext, IConfiguration config,
                            IStringLocalizer<ErrorResource> localizer) : ControllerBase
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
            Log.Information("User {Name} tries to login with correct password", user.UserName);
            return await CreatingTokens(user);
        }
        return Unauthorized(localizer["InvalidNameOrPswd"].Value);
    }

    private async Task<IActionResult> CreatingTokens(User user, bool populateExpire = true) {
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        var oldUser = user;
        user.RefreshToken = refreshToken;
        if(populateExpire)
            user.RefreshTokenDateEnd = DateTime.Now.AddDays(7);
        dbContext.users.Entry(oldUser).CurrentValues.SetValues(user);
        await dbContext.SaveChangesAsync();
        Log.Information("Created tokens for user {Name} with refresh token expiration {ExpTime}", user.UserName, user.RefreshTokenDateEnd);
        return Ok(new { token = token, refreshToken = refreshToken });
    }

    private string GenerateJwtToken(User user) {
        List<Claim> claims = new List<Claim> {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.UserData, user.Description),
            new Claim(ClaimTypes.Email, user.Email)
            };
        JwtSecurityToken securityToken = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256)
        );
        Log.Information("Generated jwt token for user {Name}", user.UserName);
        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(TokenRequestModel tokenModel) {
        var principal = GetPrincipalFromExpiredToken(tokenModel.AccessToken!);

        var user = await dbContext.users.FirstOrDefaultAsync(user => user.UserName == principal.Identity!.Name);
        if(user == null) {
                Log.Error("Upon refresh user wasn't found, name {principalName}", principal.Identity!.Name);
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

    private string GenerateRefreshToken() {
        var number = new byte[32];
        using(var random = RandomNumberGenerator.Create()) {
            random.GetBytes(number);

            return Convert.ToBase64String(number);
        }
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
                Log.Error("Upon GetPrincipalFromExpiredToken token was inwalid {Token} Algorithm: {Alg}",
                                securityToken, JwtSecurityToken.Header.Alg);
                throw new Exception("Invalid token.");
            }
            return principal;
        } catch(Exception Ex) {
            Log.Error("Catched exception at GetPrincipalFromExpiredToken: {Exception}", Ex);
        }

        return new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>() { new Claim(ClaimTypes.Name, "") }));
    }

    [HttpPost("sendEmail")]
    public async Task<IActionResult> SendEmailAsync(sendEmailForm email) {
            if(email.userId == null) return BadRequest("user id in input was null");
            var user = await dbContext.users.FindAsync(email.userId);
            if(user == null) return BadRequest("user was null");
            // Forming message to send
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("MyDiary", config["EmailFromSend"]));
            message.To.Add(new MailboxAddress("", user.Email));
            if(email.IsValidation) {
                message.Subject = "Validating email";
                message.Body = new TextPart(MimeKit.Text.TextFormat.Html) {
                    Text = $"<p>Hello, {user.UserName} to MyDiary. Before starting using app you should validate your email, for that click" +
                           $" <a href=\"{email.CallbackUrl}\">here</a> to reset." + $"After 300 minutes this url will not be usable anymore." + 
                           $"If it wasn't you, ignore this message</p>"
                };
            } else {
                message.Subject = "Resetting password";
                message.Body = new TextPart(MimeKit.Text.TextFormat.Html) {
                    Text = $"<p>Hello, {user.UserName}. We have received request to reset your password in \"MyDiary\" app, click <a href=\"{email.CallbackUrl}\">here</a>" +
                        $" to reset. After 30 minutes this url will not be usable anymore. If it wasn't you, ignore this message</p>"
                };
            }

            // Logic to send Email
            try {
                using (var client = new SmtpClient()) {
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(config["EmailFromSend"], config["AppPaswordForEmailAuth"]);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            } catch(Exception Ex) {
                Log.Error("Catched exception upon sending email: {Exception}", Ex);
            }

            Log.Information("Has sended email to {Email} isvalidation email: {Isvalidation}", user.Email, email.IsValidation);
            return Ok("Email sended successfully");
    }
    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPasswordAsync(resetPasswordForm resetPasswordForm) {
        var user = await dbContext.users.FindAsync(resetPasswordForm.UserId);
        if (user is null) {
            return NotFound(localizer["InvalidNameOrPswd"].Value);
        }
        if(user.ActionDateEnd != null && user.ActionDateEnd > DateTime.Now) {
            var Base64Token = Convert.FromBase64String( resetPasswordForm.Token.Replace('-', '+').Replace('_', '/') );
            if(user.ActionToken != null && CryptographicOperations.FixedTimeEquals(user.ActionToken, Base64Token) ) {
                var hasher = new PasswordHasher<User>();
                var newUser = user;
                newUser.Password = hasher.HashPassword(newUser, resetPasswordForm.newPassword);
                newUser.ActionDateEnd = null;
                newUser.ActionToken = null;
                
                dbContext.users.Entry(user).CurrentValues.SetValues(newUser);
                await dbContext.SaveChangesAsync();

                return Ok("Resetted successfully");
            } else {
                Log.Error("ResetPassword was unsuccessful, token {token}, base64Token {base64Token}",
                                user.ActionToken, Base64Token);
                return BadRequest(localizer["TokenNotEqual"].Value);
            }
        } else {
            Log.Information("ResetPassword was unsuccessful, actionDateEnd {DateEnd}", user.ActionDateEnd);
            return BadRequest(localizer["TokenTimeExpired"].Value);
        }
    }
    [HttpPost("ValidateEmail")]
    public async Task<IActionResult> ValidateEmailAsync(validateEmailForm ValidateEmailForm) {
        var user = await dbContext.users.FindAsync(ValidateEmailForm.UserId);
        if (user is null) {
            return NotFound(localizer["InvalidNameOrPswd"].Value);
        }
        if(user.ActionDateEnd != null && user.ActionDateEnd > DateTime.Now) {
            var Base64Token = Convert.FromBase64String( ValidateEmailForm.Token.Replace('-', '+').Replace('_', '/') );
            if(user.ActionToken != null && CryptographicOperations.FixedTimeEquals(user.ActionToken, Base64Token) ) {
                var newUser = user;
                newUser.IsValidated = true;
                newUser.ActionDateEnd = null;
                newUser.ActionToken = null;
                
                dbContext.users.Entry(user).CurrentValues.SetValues(newUser);
                await dbContext.SaveChangesAsync();

                return Ok("Validated successfully");
            } else {
                Log.Error("ValidateEmail was unsuccessful, token {token}, base64Token {base64Token}",
                                user.ActionToken, Base64Token);
                return BadRequest(localizer["TokenNotEqual"].Value);
            }
        } else {
            Log.Information("ValidateEmail was unsuccessful, actionDateEnd {DateEnd}", user.ActionDateEnd);
            return BadRequest(localizer["TokenTimeExpired"].Value);
        }
    }
    [HttpGet("password/isequal/{password}/{userId:int}")]
    public async Task<IActionResult> IsEqualPasswordsAsync(string password, int userId) {
        var user = await dbContext.users.FindAsync(userId);
        if(user == null) {
            return NotFound("Not found");
        }
        var hasher = new PasswordHasher<User>();
        var verify = hasher.VerifyHashedPassword(user, user.Password, password);
        if(verify == PasswordVerificationResult.Success) {
            return Ok("Are equal");
        }
        return BadRequest(localizer["PasswordsNotEqual"].Value);
    }
    [HttpGet("email/isunique/{email}")]
    public async Task<IActionResult> IsUniqueEmailAsync(string email) {
        var user = await dbContext.users.AnyAsync(userDb => userDb.Email.ToLower() == email.ToLower());
        return user ? Ok("Are exist") : BadRequest("Not exist");
    }

    public class passwordForm {
        public string? Password { get; set; }
        public int? UserId { get; set; }
    }
    public class TokenRequestModel {
        public string? RefreshToken { get; set; }
        public string? AccessToken { get; set; }
    }
    public class sendEmailForm {
        public int? userId { get; set; }
        public string? CallbackUrl { get; set; }
        public bool IsValidation { get; set; } = false;
    }
    public class resetPasswordForm {
        public required string Token { get; set; }
        public int UserId { get; set; }
        public required string newPassword { get; set; }
    }
    public class validateEmailForm {
        public required string Token { get; set; }
        public int UserId { get; set; }
    }
}
