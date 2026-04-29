using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Coworking.Domain.Entity;
using Microsoft.Extensions.Localization;


namespace Coworking.WebApi.Pages.Account
{
    public class ResetPasswordModel(
        UserManager<ApplicationUser> userManager,
        IStringLocalizer<ResetPasswordModel> localizer)
        : PageModel
    {
        [BindProperty]
        public required InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Compare("Password")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required]
            public string Code { get; set; } = string.Empty;
        }

        public IActionResult OnGet(string? code = null, string? email = null)
        {
            code ??= Request.Query["token"].FirstOrDefault();
            email ??= Request.Query["email"].FirstOrDefault();

            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(localizer["TokenRequiredError"].Value);
            }

            Input = new InputModel
            {
                Code = code,
                Email = email ?? string.Empty
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await userManager.FindByEmailAsync(Input.Email);
            if (user == null) return RedirectToPage("./Login");

            var result = await userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            
            if (result.Succeeded)
            {
                // При обновлении пароля также обновим метку времени
                user.PasswordChangedAt = DateTime.UtcNow;
                await userManager.UpdateAsync(user);
                
                return RedirectToPage("./Login", new { status = "password-reset" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return Page();
        }
    }
}