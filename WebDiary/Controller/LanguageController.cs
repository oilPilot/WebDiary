using System;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
namespace WebDiary.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class LanguageController : ControllerBase
{
    [NonAction]
    public IActionResult Set(string culture, string redirectUri)
    {
        if (culture != null)
        {
            var requestCulture = new RequestCulture(culture, culture);
            var cookieName = CookieRequestCultureProvider.DefaultCookieName;
            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(requestCulture);

            HttpContext.Response.Cookies.Append(cookieName, cookieValue);
        }

        return Redirect(redirectUri);
    }
}
