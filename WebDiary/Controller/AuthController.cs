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
using System.Security.Cryptography;

namespace WebDiary.Controller;

[Route("auth")]
[ApiController]
public class AuthController (DiariesContext dbContext, IConfiguration config) : ControllerBase
{
    [HttpPost("jwttoken/login")]
    public IActionResult Login(LoginModel model) {
        User? userLogin = dbContext.users.FirstOrDefault(user => user.UserName == model.Username && user.Password == model.Password);
        if(userLogin != null) {
            var token = GenerateJwtToken(userLogin);
            return Ok(new { token });
        }
        return Unauthorized();
    }
    [HttpPost("sendEmail")]
    public async Task SendEmail(SendEmailForm email) {
            // Forming message to send
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("MyDiary", "matveyeresko1@gmail.com"));
            message.To.Add(new MailboxAddress("", email.Email));
            message.Subject = "Resetting password";
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html) {
                Text = $"<p>Hello, {email.Username}. We have received request to reset your password in \"MyDiary\" app, click <a href=\"{email.CallbackUrl}\">here</a> to reset." +
                            "After 30 minutes this url will not be usable anymore. If it wasn't you, ignore this message</p>"
            };

            // Logic to send Email
            using (var client = new SmtpClient()) {
                await client.ConnectAsync("smtp.gmail.com", 25, false);
                await client.AuthenticateAsync("matveyeresko1@gmail.com", config["AppPaswordForEmailAuth"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
    }
    [HttpPost("ResetPassword")]
    public async Task<bool> ResetPassword(ResetPasswordForm resetPasswordForm) {
        var user = await dbContext.users.FindAsync(resetPasswordForm.UserId);
        if(user!.ResetPasswordDateEnd != null && user.ResetPasswordDateEnd > DateTime.Now) {
            var Base64Token = Convert.FromBase64String( resetPasswordForm.Token.Replace('-', '+').Replace('_', '/') );
            if(user.ResetPasswordToken != null && user.ResetPasswordToken.SequenceEqual(Base64Token) ) {
                var newUser = user;
                newUser.Password = resetPasswordForm.newPassword;
                newUser.ResetPasswordDateEnd = null;
                newUser.ResetPasswordToken = null;
                
                dbContext.users.Entry(user).CurrentValues.SetValues(newUser);
                await dbContext.SaveChangesAsync();

                return true;
            }
        }
        return false;
    }
    [HttpGet("password/isequal/{password}/{userId:int}")]
    public async Task<bool> IsEqualPasswordsAsync(string password, int userId) {
        var user = await dbContext.users.FindAsync(userId);
        if(user == null) {
            return false;
        }
        return user.Password == password ? true : false;
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

    public class PasswordForm {
        public string? Password { get; set; }
        public int? UserId { get; set; }
    }
    public class SendEmailForm {
        public string? Email { get; set; }
        public string? CallbackUrl { get; set; }
        public string? Username { get; set; }
    }
    public class ResetPasswordForm {
        public required string Token { get; set; }
        public int UserId { get; set; }
        public required string newPassword { get; set; }
    }
}
