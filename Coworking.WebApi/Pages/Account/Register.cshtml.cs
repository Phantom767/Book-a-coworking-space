using Coworking.Application.DTOs.Auth;
using Coworking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Coworking.WebApi.Pages.Account;

public class RegisterModel(IAuthService authService) : PageModel
{
    [BindProperty]
    public RegisterRequest Input { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await authService.RegisterAsync(Input);

        return result.Match<IActionResult>(
            _ => RedirectToPage("/Account/Login"),
            errors => {
                foreach (var error in errors) 
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }
        );
    }
}