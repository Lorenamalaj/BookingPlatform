using Booking.Infrastructure.Data;
using Booking.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Bookings.Commands.CreateBooking;

public class CreateBookingCommandHandler
{
    private readonly BookingPlatformDbContext _context;
    private readonly EmailService _emailService;
    public CreateBookingCommandHandler(BookingPlatformDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<CreateBookingResult> Handle(CreateBookingCommand command)
    {
        // 1. Kontrollo nëse prona ekziston
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == command.PropertyId);

        if (property == null)
            return new CreateBookingResult { IsSuccess = false, Error = "Property not found" };

        if (!property.CanBeBooked())
            return new CreateBookingResult { IsSuccess = false, Error = "Property is not available for booking" };

        // 2. Kontrollo nëse guest-i ekziston dhe merr email-in e tij
        var guest = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.GuestId);

        if (guest == null)
            return new CreateBookingResult { IsSuccess = false, Error = "Guest not found" };

        if (command.GuestCount > property.MaxGuests)
            return new CreateBookingResult { IsSuccess = false, Error = $"Max guests: {property.MaxGuests}" };

        var hasOverlap = await _context.Bookings
            .AnyAsync(b =>
                b.PropertyId == command.PropertyId &&
                b.BookingStatus != "Cancelled" &&
                ((command.StartDate >= b.StartDate && command.StartDate < b.EndDate) ||
                 (command.EndDate > b.StartDate && command.EndDate <= b.EndDate))
            );

        if (hasOverlap)
            return new CreateBookingResult { IsSuccess = false, Error = "Dates are taken" };

        // 4. Krijo rezervimin
        var booking = new Booking.Domain.Bookings.Booking(
            command.PropertyId,
            command.GuestId,
            command.StartDate,
            command.EndDate,
            command.GuestCount,
            command.CleaningFee,
            command.AmenitiesUpCharge,
            command.PriceForPeriod
        );

        _context.Bookings.Add(booking);
        property.MarkAsBooked();

        await _context.SaveChangesAsync();

        // 5. DËRGIMI I EMAIL-IT
        try
        {
            var subject = $"Booking Confirmation #{booking.Id}";
            var body = $@"
Hello {guest.FirstName},

Your booking has been confirmed!

Booking Details:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Property ID: {property.Id}
Check-in: {command.StartDate:dddd, dd MMMM yyyy}
Check-out: {command.EndDate:dddd, dd MMMM yyyy}
Guests: {command.GuestCount}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Price Breakdown:
- Period: ${command.PriceForPeriod}
- Cleaning Fee: ${command.CleaningFee}
- Amenities: ${command.AmenitiesUpCharge}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Total: ${booking.TotalPrice}

Status: Pending (waiting for owner confirmation)

Thank you for choosing our platform!

Best regards,
Booking Platform Team
";

            await _emailService.SendEmailAsync(guest.Email, subject, body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email failed: {ex.Message}");
        }

        return new CreateBookingResult
        {
            IsSuccess = true,
            BookingId = booking.Id
        };
    }
}

public class CreateBookingResult
{
    public bool IsSuccess { get; set; }
    public Guid BookingId { get; set; }
    public string? Error { get; set; }
}