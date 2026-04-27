using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Coworking.WebApi.Pages;

public class SetLanguageModel : PageModel
{
    public IActionResult OnGet(string culture, string returnUrl = "/")
    {
        var supported = new[] { "ru", "kk", "en" };

        if (!supported.Contains(culture))
            culture = "ru";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires  = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            }
        );

        return LocalRedirect(returnUrl);
    }
}