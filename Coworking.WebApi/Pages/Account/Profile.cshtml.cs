using Coworking.Application.Interfaces;
using Coworking.Application.Models;
using Coworking.Domain.Entity;
using Coworking.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Coworking.WebApi.Pages.Account;

public class ProfileModel(
    IProfileService profileService,
    IBookingService bookingService,
    UserManager<ApplicationUser> userManager) : PageModel
{
    // Свойства для отображения (View Model)
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Is2FAEnabled { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public string PasswordChangedDisplay { get; set; } = "Никогда";
    public int HoursThisMonth { get; set; }
    
    public List<BookingViewModel> ActiveBookings { get; set; } = new();
    public List<BookingViewModel> PastBookings { get; set; } = new();

    // Модель ввода из Application слоя
    [BindProperty] 
    public UpdateProfileRequest ProfileInput { get; set; } = new();

    public async Task OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return;

        // Заполняем данные для отображения
        FirstName = user.FirstName ?? string.Empty;
        LastName = user.LastName ?? string.Empty;
        Email = user.Email ?? string.Empty;
        Is2FAEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        EmailNotificationsEnabled = user.EmailNotificationsEnabled;
        
        var passDate = await profileService.GetPasswordChangedDateAsync(user.Id);
        PasswordChangedDisplay = passDate?.ToString("dd MMM yyyy") ?? "Никогда";

        // Заполняем форму ввода текущими данными
        ProfileInput = new UpdateProfileRequest
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            EmailNotificationsEnabled = user.EmailNotificationsEnabled
        };

        // Логика бронирований (остается здесь или выносится в IBookingService)
        var bookings = await bookingService.GetBookingsByUserAsync(user.Id);
        var now = DateTime.Now;

    ActiveBookings = bookings
        .Where(b => b.EndTime > now && b.Status != BookingStatus.Cancelled)
        .OrderBy(b => b.StartTime)
        .Select(b => new BookingViewModel(b, now))
        .ToList();

    PastBookings = bookings
        .Where(b => b.EndTime <= now || b.Status == BookingStatus.Cancelled)
        .OrderByDescending(b => b.StartTime)
        .Take(10)
        .Select(b => new BookingViewModel(b, now))
        .ToList();

    HoursThisMonth = bookings
    .Where(b => b.StartTime.Month == now.Month
             && b.StartTime.Year  == now.Year
             && b.Status != BookingStatus.Cancelled)
    .Sum(b => (int)Math.Round((b.EndTime - b.StartTime).TotalHours));
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var success = await profileService.UpdateProfileAsync(user.Id, ProfileInput);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Не удалось обновить профиль.");
            return Page();
        }

        TempData["SuccessMessage"] = "Профиль успешно обновлен.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleNotificationsAsync(bool enableNotifications)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        await profileService.ToggleEmailNotificationsAsync(user.Id, enableNotifications);
        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostToggle2FAAsync(bool enable2fa)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        
        await userManager.SetTwoFactorEnabledAsync(user, enable2fa);
        return RedirectToPage();
    }
}