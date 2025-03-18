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

namespace WebDiary.Controller;

[Route("auth/jwttoken")]
[ApiController]
public class AuthController (DiariesContext dbContext, IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login(LoginModel model) {
        User? userLogin = dbContext.users.FirstOrDefault(user => user.UserName == model.Username && user.Password == model.Password);
        if(userLogin != null) {
            var token = GenerateJwtToken(userLogin);
            return Ok(new { token });
        }
        return Unauthorized();
    }

    private string GenerateJwtToken(User user) {
        List<Claim> claims = new List<Claim> {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.UserData, user.Description)
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
}
