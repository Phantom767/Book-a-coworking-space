using AutoMapper;
using Coworking.Application.DTOs;

namespace Coworking.Application.common.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Из сущности в DTO
        CreateMap<Domain.Entity.Booking, BookingDto>(); 
        
        // Из DTO создания в сущность
        CreateMap<CreateBookingDto, Domain.Entity.Booking>();
    }
}