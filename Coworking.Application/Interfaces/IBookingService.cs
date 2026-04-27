using Coworking.Application.DTOs;
using ErrorOr;

namespace Coworking.Application.Interfaces;

public interface IBookingService
{
    Task<ErrorOr<BookingDto>> CreateBookingAsync(CreateBookingDto dto);
    Task<List<BookingDto>> GetBookingsByUserAsync(Guid userId);
}