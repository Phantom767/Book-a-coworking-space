using AutoMapper;
using Coworking.Application.DTOs;
using Coworking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Coworking.WebApi.Pages.Admin;

public class Rooms(IRoomService roomService, IAdminRoomService adminRoomService, IMapper mapper) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();
    [BindProperty] 
    public IFormFile? Photo { get; set; }
    public List<RoomDto> RoomDtos { get; set; } = new();

    public class InputModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Capacity { get; set; }
    }

    
    public async Task OnGetAsync()
    {
        RoomDtos = await roomService.GetAllRoomsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        var createDto = new CreateRoomDto()
        {
            Name = Input.Name,
            Description = Input.Description,
            PricePerHour = Input.Price,
            Capacity = Input.Capacity,
            PhotoUrl = "/images/default-room.jpg", // Дефолтное значение
            PhotoHash = "default" // Дефолтное значение
        };

        var room = mapper.Map<CreateRoomDto>(await adminRoomService.CreateRoomAsync(createDto));
        
        if (Photo is { Length: > 0 })
        {
            try
            {
                await roomService.UploadRoomPhotoAsync(room.Id, Photo);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                RoomDtos = await roomService.GetAllRoomsAsync();
                return Page();
            }
        }
        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostUploadPhotoAsync(Guid roomId)
    {
        if (Photo is not { Length: > 0 })
        {
            ModelState.AddModelError(string.Empty, "Выберите файл");
            RoomDtos = await roomService.GetAllRoomsAsync();
            return Page();
        }

        try
        {
            await roomService.UploadRoomPhotoAsync(roomId, Photo);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostEditAsync(Guid roomId)
    {
        if (!ModelState.IsValid)
        {
            RoomDtos = await roomService.GetAllRoomsAsync();
            return Page();
        }

        try
        {
            var updateDto = new UpdateRoomDto
            {
                Id = roomId,
                Name = Input.Name,
                Description = Input.Description,
                PricePerHour = Input.Price,
                Capacity = Input.Capacity
            };

            await roomService.UpdateAsync(updateDto);
            
            if (Photo is { Length: > 0 })
            {
                await roomService.UploadRoomPhotoAsync(roomId, Photo);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Ошибка обновления: {ex.Message}");
            RoomDtos = await roomService.GetAllRoomsAsync();
            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid roomId)
    {
        await adminRoomService.DeleteRoomAsync(roomId);
        return RedirectToPage();
    }
}