using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using Coworking.Domain.Entity; 
using Coworking.Application.Interfaces;
using Microsoft.Extensions.Localization;

namespace Coworking.WebApi.Pages.Account;
    public class ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender, IStringLocalizer<ForgotPasswordModel> localizer) : PageModel
    {
        [BindProperty]
        public required InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(Input.Email);
        
                // Сообщение одинаковое для безопасности (чтобы не перебирать email-ы)
                var successMessage = localizer["SuccessMessage"];

                if (user == null || !await userManager.IsEmailConfirmedAsync(user))
                {
                    TempData["SuccessMessage"] = successMessage;
                    return RedirectToPage();
                }

                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Page("/Account/ResetPassword", null, new { token }, Request.Scheme);
        
                if (callbackUrl != null)
                {
                    var subject = localizer["EmailSubject"];
                    var htmlContent = $@"
                <h3>{localizer["EmailHeader"]}</h3>
                <p>{localizer["EmailBody"]}</p>
                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                   style='display:inline-block; padding:10px 20px; background:#6c5ce7; color:white; text-decoration:none; border-radius:5px;'>
                   {localizer["EmailButtonText"]}
                </a>
                <p>{localizer["EmailFooter"]}</p>";
        
                    await emailSender.SendEmailAsync(Input.Email, subject, htmlContent);
                }

                TempData["SuccessMessage"] = successMessage;
            }
            return RedirectToPage();
        }
    }