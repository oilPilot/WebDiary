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

namespace WebDiary.Controller;

[Route("auth")]
[ApiController]
public class AuthController (DiariesContext dbContext, IConfiguration config) : ControllerBase
{
    [HttpPost("jwttoken/login")]
    public IActionResult Login(LoginModel model) {
        User? user = dbContext.users.FirstOrDefault(user => user.UserName == model.Username);
        if(user == null) {
            return NotFound("Invalid username or password");
        }
        var hasher = new PasswordHasher<User>();
        var verify = hasher.VerifyHashedPassword(user, user.Password, model.Password!);
        if(verify == PasswordVerificationResult.Success) {
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }
        return Unauthorized("Invalid username or password");
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
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(securityToken);
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
                Console.WriteLine("Catched exception upon sending email: " + Ex);
            }

            return Ok("Email sended successfully");
    }
    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPasswordAsync(resetPasswordForm resetPasswordForm) {
        var user = await dbContext.users.FindAsync(resetPasswordForm.UserId);
        if(user!.ActionDateEnd != null && user.ActionDateEnd > DateTime.Now) {
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
                return BadRequest("Tokens weren't equal");
            }
        } else {
            return BadRequest("Time had expired");
        }
    }
    [HttpPost("ValidateEmail")]
    public async Task<IActionResult> ValidateEmailAsync(validateEmailForm ValidateEmailForm) {
        var user = await dbContext.users.FindAsync(ValidateEmailForm.UserId);
        if(user!.ActionDateEnd != null && user.ActionDateEnd > DateTime.Now) {
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
                return BadRequest("Tokens weren't equal");
            }
        } else {
            return BadRequest("Time had expired");
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
        return BadRequest("Not equal");
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
