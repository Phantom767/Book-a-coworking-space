using Coworking.Application.DTOs;
using Coworking.Domain.Entity;
using ErrorOr;

namespace Coworking.Application.Interfaces;

public interface IBookingService
{
    Task<ErrorOr<BookingDto>> CreateBookingAsync(CreateBookingDto dto);
    Task<ErrorOr<List<BookingDto>>> GetBookingsByUserAsync(Guid userId);
}