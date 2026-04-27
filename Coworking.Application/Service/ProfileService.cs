using Coworking.Application.Interfaces;
using Coworking.Application.Models;
using Coworking.Domain.Entity;
using Microsoft.AspNetCore.Identity;

namespace Coworking.Application.Service;

public class ProfileService(UserManager<ApplicationUser> userManager) : IProfileService
{
    public async Task<ApplicationUser> GetUserProfileAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new InvalidOperationException("User not found");
            return user;
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            
            var emailResult = await userManager.SetEmailAsync(user, request.Email);
            if (!emailResult.Succeeded) return false;

            user.PhoneNumber = request.Phone;
            
            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task ToggleEmailNotificationsAsync(Guid userId, bool isEnabled)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null) return;

            user.EmailNotificationsEnabled = isEnabled;
            await userManager.UpdateAsync(user);
        }

        public Task<DateTime?> GetPasswordChangedDateAsync(Guid userId)
        {
            var user = userManager.Users.FirstOrDefault(u => u.Id == userId);
            return Task.FromResult(user?.PasswordChangedAt);
        }
    }