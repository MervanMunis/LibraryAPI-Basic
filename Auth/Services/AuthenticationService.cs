using LibraryAPI.Data;
using LibraryAPI.Exceptions;
using LibraryAPI.Models.DTOs.Request;
using LibraryAPI.Models.Entities;
using LibraryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Auth.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly LibraryAPIContext _context;
        private readonly IEmailService _emailService;

        public AuthenticationService(UserManager<ApplicationUser> userManager, LibraryAPIContext context, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
            _emailService = emailService;
        }

        public async Task<ServiceResult<string>> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordRequest.Email);
            if (user == null)
            {
                return ServiceResult<string>.FailureResult("User not found.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Here we need to send the token via email to the user
            // For simplicity, we're returning the token as a response (do not do this in a real application)
            return ServiceResult<string>.SuccessResult(token);
        }

        public async Task<ServiceResult<string>> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordRequest.Email);
            if (user == null)
            {
                return ServiceResult<string>.FailureResult("User not found.");
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordRequest.Token, resetPasswordRequest.NewPassword);
            if (!result.Succeeded)
            {
                return ServiceResult<string>.FailureResult("Failed to reset password.");
            }

            // Invalidate JWT tokens by adding the token to a blacklist or other logic
            await InvalidateTokensForUser(user.Id);

            return ServiceResult<string>.SuccessResult("Password reset successfully.");
        }

        private async Task InvalidateTokensForUser(string userId)
        {
            // Logic to invalidate tokens (e.g., add tokens to a blacklist)
            var userTokens = await _context.UserTokens.Where(t => t.UserId == userId).ToListAsync();
            _context.UserTokens.RemoveRange(userTokens);
            await _context.SaveChangesAsync();
        }
    }
}
