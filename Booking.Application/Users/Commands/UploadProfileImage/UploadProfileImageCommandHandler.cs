using Booking.Infrastructure.Data;

namespace Booking.Application.Users.Commands.UploadProfileImage;

public class UploadProfileImageCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public UploadProfileImageCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<UploadProfileImageResult> Handle(UploadProfileImageCommand command)
    {
        var user = await _context.Users.FindAsync(command.UserId);

        if (user == null)
        {
            return new UploadProfileImageResult
            {
                IsSuccess = false,
                Error = "User not found"
            };
        }

        try
        {
            user.UpdateProfileImage(command.ImageData, command.ContentType);
            await _context.SaveChangesAsync();

            return new UploadProfileImageResult { IsSuccess = true };
        }
        catch (Exception ex)
        {
            return new UploadProfileImageResult
            {
                IsSuccess = false,
                Error = ex.Message
            };
        }
    }
}

public class UploadProfileImageResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}