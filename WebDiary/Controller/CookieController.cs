using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace WebDiary.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class CookieController : ControllerBase
{
    /// <summary>
    /// Adds a cookie to indicate that the user has agreed to cookies.
    /// </summary>
    /// <returns>An IActionResult indicating the result of the operation.</returns>
    public IActionResult AddCookieAgreement(string redirectUri)
    {
        if (string.IsNullOrEmpty(redirectUri))
        {
            return BadRequest("Redirect URI cannot be null or empty.");
        }
        HttpContext.Response.Cookies.Append("CookieAgreement", "true", new CookieOptions
        {
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            SameSite = SameSiteMode.None,
            Secure = true
        });
        return Redirect(redirectUri);
    }
}

