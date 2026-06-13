using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Coworking.Application.Service;
using Coworking.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Coworking.WebApi.Pages.Admin;

[Authorize(Roles = "Admin")]
public class Rooms(
    IAdminRoomService adminRoomService,
    IRoomService roomService,
    IAdminBookingService bookingService,
    IStringLocalizer<Rooms> localizer) : PageModel
{
    // ── Данные для страницы ─────────────────────────────────────
    public List<RoomDto> RoomDtos { get; private set; } = [];
    public List<BookingAdminDto> Bookings { get; private set; } = [];
    public (int Total, int Pending, int Confirmed, int Cancelled) BookingStats { get; private set; }

    // ── Фильтр бронирований ─────────────────────────────────────
    [BindProperty(SupportsGet = true)] public string? BookingSearch   { get; set; }
    [BindProperty(SupportsGet = true)] public string? BookingStatus   { get; set; }
    [BindProperty(SupportsGet = true)] public Guid?   BookingRoomId   { get; set; }
    [BindProperty(SupportsGet = true)] public string? BookingDateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public string? BookingDateTo   { get; set; }
    [BindProperty(SupportsGet = true)] public string  ActiveTab       { get; set; } = "rooms";

    // ── Input для создания / редактирования комнаты ─────────────
    [BindProperty] public RoomInputModel Input  { get; set; } = new();
    [BindProperty] public IFormFile?      Photo { get; set; }

    // ───────────────────────────────────────────────────────────
    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    // ── Создание комнаты ────────────────────────────────────────
    public async Task<IActionResult> OnPostAsync()
    {
        var dto = new CreateRoomDto
        {
            Name         = Input.Name,
            Description  = Input.Description,
            PricePerHour = Input.Price,
            Capacity     = Input.Capacity,
            Photo        = Photo
        };

        var result = await adminRoomService.CreateRoomAsync(dto);
        if (result.IsError)
            ModelState.AddModelError("", result.FirstError.Description);

        return RedirectToPage(new { ActiveTab = "rooms" });
    }

    // ── Редактирование комнаты ──────────────────────────────────
    public async Task<IActionResult> OnPostEditAsync(Guid roomId)
    {
        var dto = new UpdateRoomDto
        {
            Name         = Input.Name,
            Description  = Input.Description,
            PricePerHour = Input.Price,
            Capacity     = Input.Capacity,
            Photo        = Photo
        };

        await adminRoomService.UpdateRoomAsync(roomId, dto);
        return RedirectToPage(new { ActiveTab = "rooms" });
    }

    // ── Удаление комнаты ────────────────────────────────────────
    public async Task<IActionResult> OnPostDeleteAsync(Guid roomId)
    {
        await adminRoomService.DeleteRoomAsync(roomId);
        return RedirectToPage(new { ActiveTab = "rooms" });
    }

    // ── Подтвердить бронь ───────────────────────────────────────
    public async Task<IActionResult> OnPostConfirmBookingAsync(Guid bookingId)
    {
        var result = await bookingService.ConfirmAsync(bookingId);
        if (result.IsError)
            ModelState.AddModelError("", result.FirstError.Description);

        return RedirectToPage(new { ActiveTab = "bookings" });
    }

    // ── Отменить бронь ──────────────────────────────────────────
    public async Task<IActionResult> OnPostCancelBookingAsync(Guid bookingId, string? cancelReason)
    {
        var result = await bookingService.CancelAsync(bookingId, cancelReason);
        if (result.IsError)
            ModelState.AddModelError("", result.FirstError.Description);

        return RedirectToPage(new { ActiveTab = "bookings" });
    }

    // ── Приватный помощник ──────────────────────────────────────
    private async Task LoadDataAsync()
    {
        RoomDtos = await roomService.GetAllRoomsAsync();

        BookingStatus? statusEnum = BookingStatus switch
        {
            "Pending"   => Coworking.Domain.Enums.BookingStatus.Pending,
            "Confirmed" => Coworking.Domain.Enums.BookingStatus.Confirmed,
            "Cancelled" => Coworking.Domain.Enums.BookingStatus.Cancelled,
            "Completed" => Coworking.Domain.Enums.BookingStatus.Completed,
            _ => null
        };

        DateTime? from = DateTime.TryParse(BookingDateFrom, out var df) ? df : null;
        DateTime? to   = DateTime.TryParse(BookingDateTo,   out var dt) ? dt : null;

        Bookings = await bookingService.GetAllAsync(
            status:   statusEnum,
            roomId:   BookingRoomId,
            search:   BookingSearch,
            dateFrom: from,
            dateTo:   to);

        BookingStats = await bookingService.GetStatsAsync();
    }

    // ── Input model ─────────────────────────────────────────────
    public class RoomInputModel
    {
        public string  Name        { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price       { get; set; }
        public int     Capacity    { get; set; }
    }
}