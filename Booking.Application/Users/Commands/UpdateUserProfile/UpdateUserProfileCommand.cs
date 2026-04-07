namespace Booking.Application.Users.Commands.UpdateUserProfile;

public class UpdateUserProfileCommand
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    
}