using Coworking.Application.DTOs;
using Coworking.Application.Service;
using Coworking.Domain.Enums;
using ErrorOr;

namespace Coworking.Application.Interfaces;

public interface IAdminBookingService
{
    Task<List<BookingAdminDto>> GetAllAsync(
        BookingStatus? status = null,
        Guid? roomId = null,
        string? search = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null);

    Task<ErrorOr<BookingAdminDto>> ConfirmAsync(Guid bookingId);
    Task<ErrorOr<BookingAdminDto>> CancelAsync(Guid bookingId, string? reason = null);
    Task<(int Total, int Pending, int Confirmed, int Cancelled)> GetStatsAsync();
}