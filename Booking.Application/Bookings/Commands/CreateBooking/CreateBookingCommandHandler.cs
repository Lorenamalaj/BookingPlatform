using Booking.Infrastructure.Data;
using Booking.Infrastructure.Email;
using Booking.Domain.Bookings; 
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Bookings.Commands.CreateBooking;

public class CreateBookingCommandHandler
{
    private readonly BookingPlatformDbContext _context;
    private readonly EmailService _emailService; // Shtojmë shërbimin këtu

    public CreateBookingCommandHandler(BookingPlatformDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

public class CreateBookingResult
{
    public bool IsSuccess { get; set; }
    public int BookingId { get; set; }
    public string? Error { get; set; }
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

        // 3. Kontrollet e tjera (Guest count & Overlap)
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
        property.MarkAsBooked(); // E bëjmë bashkë me booking

        await _context.SaveChangesAsync();

        // 5. DËRGIMI I EMAIL-IT (Pasi u ruajt me sukses në DB)
        try
        {
            await _emailService.SendEmailAsync(guest.Email, "Test", "Test Body");
        }
        catch (Exception ex)
        {
            // Kjo do të ta nxjerrë errorin te Postman në vend të "Success"
            return new CreateBookingResult { IsSuccess = false, Error = "Email Error: " + ex.Message };
        }

        return new CreateBookingResult
        {
            IsSuccess = true,
            BookingId = booking.Id
        };
    }
}