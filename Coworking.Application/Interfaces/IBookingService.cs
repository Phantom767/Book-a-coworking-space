using Coworking.Application.DTOs;
using ErrorOr;

namespace Coworking.Application.Interfaces;

public interface IBookingService
{
    Task<ErrorOr<Domain.Entity.Booking>> CreateBookingAsync(CreateBookingDto dto);
}