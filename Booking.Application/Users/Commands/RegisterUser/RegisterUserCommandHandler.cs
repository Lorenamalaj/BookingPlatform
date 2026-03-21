using Booking.Domain.Users;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler
    {
        private readonly BookingPlatformDbContext _context;
        public RegisterUserCommandHandler(BookingPlatformDbContext context)
        {
            _context = context;
        }
        public async Task<RegisterUserResult> Handle(RegisterUserCommand command)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == command.Email);

            if (emailExists)
            {
                return new RegisterUserResult
                {
                    IsSuccess = false,
                    Error = "Email already exists."
                };
            }
            var passwordHash = HashPassword(command.Password);

           
            var user = new User(
                command.FirstName,
                command.LastName,
                command.Email,
                passwordHash
            );


            if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
            {
                user.UpdatePhoneNumber(command.PhoneNumber);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new RegisterUserResult
            {
                IsSuccess = true,
                UserId = user.Id
            };
        }
         private string HashPassword(string password)
        {
            return Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(password)
            );
        }
    }
    public class RegisterUserResult
    {
        public bool IsSuccess { get; set; }
        public Guid UserId { get; set; }
        public string? Error { get; set; }

    }
}
