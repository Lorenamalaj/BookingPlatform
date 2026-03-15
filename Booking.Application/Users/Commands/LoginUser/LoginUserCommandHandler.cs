using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Users.Commands.LoginUser
{
    public class LoginUserCommandHandler
    {
        private readonly BookingPlatformDbContext _context;

        public LoginUserCommandHandler(BookingPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<LoginUserResult> Handle(LoginUserCommand command)
        {
            // 1. Gjejmë user-in me këtë email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == command.Email);

            if (user == null)
            {
                return new LoginUserResult { IsSuccess = false, Error = "Invalid email or password." };
            }

            // 2. Hash-ojmë password-in që dha user-i tani për ta krahasuar me atë në DB
            var loginPasswordHash = HashPassword(command.Password);

            if (user.PasswordHash != loginPasswordHash)
            {
                return new LoginUserResult { IsSuccess = false, Error = "Invalid email or password." };
            }

            // 3. Login i suksesshëm
            return new LoginUserResult
            {
                IsSuccess = true,
                UserId = user.Id,
                FirstName = user.FirstName
            };
        }

        private string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    public class LoginUserResult
    {
        public bool IsSuccess { get; set; }
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? Error { get; set; }
    }
}