using Coworking.Application.DTOs.Auth;
using Coworking.Application.Interfaces;
using Coworking.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Coworking.WebApi.Pages.Account
{
    public class LoginModel(
        IAuthService authService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
        : PageModel
    {
        [BindProperty] 
        public required LoginRequest Input { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await authService.LoginAsync(Input);

            return await result.MatchAsync<IActionResult>(
                async success =>
                {
                    var user = await userManager.FindByEmailAsync(Input.Email);
                    if (user != null)
                        await signInManager.SignInAsync(user, isPersistent: Input.RememberMe);

                    var returnUrl = Request.Query["returnUrl"].FirstOrDefault();
                    return LocalRedirect(returnUrl ?? "/Index");
                },
                errors =>
                {
                    ModelState.AddModelError(string.Empty, "Неверный логин или пароль");
                    return Task.FromResult<IActionResult>(Page());
                }
            );
        }
    }
}