using System.Security.Claims;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Coworking.WebApi.Pages.Rooms;

// ─────────────────────────────────────────────────────────────
// Query model — binds from GET query string
// ─────────────────────────────────────────────────────────────
public class RoomFilterQuery
{
    /// <summary>Поиск по названию или удобствам</summary>
    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    /// <summary>Минимальная цена за час</summary>
    [BindProperty(SupportsGet = true)]
    public decimal? PriceMin { get; set; }

    /// <summary>Максимальная цена за час</summary>
    [BindProperty(SupportsGet = true)]
    public decimal? PriceMax { get; set; }

    /// <summary>Дата заезда (комната должна быть свободна)</summary>
    [BindProperty(SupportsGet = true)]
    public DateOnly? DateFrom { get; set; }

    /// <summary>Дата выезда</summary>
    [BindProperty(SupportsGet = true)]
    public DateOnly? DateTo { get; set; }

    /// <summary>Минимальная вместимость</summary>
    [BindProperty(SupportsGet = true)]
    public int? Capacity { get; set; }

    /// <summary>
    /// Сортировка: newest | oldest | amenities | price_asc | price_desc | capacity
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; }
}


public class Room(IRoomService roomService, IBookingService bookingService)
    : PageModel
{
    [BindProperty]
    public BookingRequest Input { get; set; }
    
    // Фильтр биндится из GET параметров
    [BindProperty(SupportsGet = true)]
    public RoomFilterQuery Filter { get; set; } = new();

    public IReadOnlyList<RoomDto> Rooms { get; private set; } = [];
    public int TotalCount { get; private set; }

    public async Task OnGetAsync()
    {
        var all = await roomService.GetAllRoomsAsync(); // IEnumerable<RoomDto>

        var query = all.AsQueryable();

        // ── Поиск по названию / удобствам ──────────────────────
        if (!string.IsNullOrWhiteSpace(Filter.Q))
        {
            var q = Filter.Q.ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(q) ||
                (r.Description != null && r.Description.ToLower().Contains(q)) ||
                (r.Amenities != null && r.Amenities.Any(a => a.ToLower().Contains(q))));
        }

        // ── Цена ───────────────────────────────────────────────
        if (Filter.PriceMin.HasValue)
            query = query.Where(r => r.PricePerHour >= Filter.PriceMin.Value);

        if (Filter.PriceMax.HasValue)
            query = query.Where(r => r.PricePerHour <= Filter.PriceMax.Value);

        // ── Вместимость ────────────────────────────────────────
        if (Filter.Capacity.HasValue)
            query = query.Where(r => r.Capacity >= Filter.Capacity.Value);

        // ── Свободность по датам ───────────────────────────────
        // Комната занята, если есть Booking, пересекающийся с [DateFrom, DateTo]
        if (Filter.DateFrom.HasValue || Filter.DateTo.HasValue)
        {
            var from = Filter.DateFrom?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue;
            var to   = Filter.DateTo?.ToDateTime(TimeOnly.MaxValue)   ?? DateTime.MaxValue;

            query = query.Where(r =>
                !r.Bookings.Any(b =>
                    b.StartTime < to && b.EndTime > from));
        }

        // ── Сортировка ─────────────────────────────────────────
        query = Filter.SortBy switch
        {
            "newest"     => query.OrderByDescending(r => r.CreationTime),
            "oldest"     => query.OrderBy(r => r.CreationTime),
            "amenities"  => query.OrderByDescending(r => r.Amenities != null ? r.Amenities.Count : 0),
            "price_asc"  => query.OrderBy(r => r.PricePerHour),
            "price_desc" => query.OrderByDescending(r => r.PricePerHour),
            "capacity"   => query.OrderByDescending(r => r.Capacity),
            _            => query.OrderByDescending(r => r.CreationTime) // default
        };

        var result = query.ToList();
        TotalCount = result.Count;
        Rooms = result;
    }
    
    public class BookingRequest
    {
        public Guid RoomId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public async Task<IActionResult> OnPostCreateAsync([FromBody] BookingRequest request)
    {
        // 1. Проверка авторизации
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return new JsonResult(new { success = false, message = "Unauthorized" });

        // 2. Подготовка данных
        var dto = new CreateBookingDto
        {
            UserId = Guid.Parse(userIdClaim),
            RoomId = request.RoomId,
            StartTime = request.StartTime.ToUniversalTime(),
            EndTime = request.EndTime.ToUniversalTime()
        };

        var result = await bookingService.CreateBookingAsync(dto);

        return result.Match(
            booking => new JsonResult(new { success = true, bookingId = booking.Id }),
            errors => new JsonResult(new { 
                success = false, 
                message = string.Join("; ", errors.Select(e => e.Description)) 
            })
        );
    }
}