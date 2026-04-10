using AutoMapper;
using Coworking.Application.DTOs;
using Coworking.Domain.Entity;

namespace Coworking.Application.common.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Из сущности в DTO
        CreateMap<Booking, BookingDto>(); 
        
        // Из DTO создания в сущность
        CreateMap<CreateBookingDto, Booking>();

        // Из сущности в DTO 
        CreateMap<Room, RoomDto>();
        
        // Из DTO создания в сущность
        CreateMap<CreateRoomDto, Room>();
    }
}