using Booking.Domain.Addresses;
using Booking.Domain.Properties;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Properties.Commands.CreateProperty;

public class CreatePropertyCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public CreatePropertyCommandHandler(BookingPlatformDbContext context) => _context = context;

    public async Task<CreatePropertyResult> Handle(CreatePropertyCommand command)
    {
        try
        {
            // Validate owner exists
            var ownerExists = await _context.Users.AnyAsync(u => u.Id == command.OwnerId);
            if (!ownerExists)
            {
                return new CreatePropertyResult { IsSuccess = false, Error = $"Owner with id {command.OwnerId} does not exist." };
            }

            // 🔍 Check if address already exists
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a =>
                    a.Country == command.Country &&
                    a.City == command.City &&
                    a.Street == command.Street &&
                    (a.PostalCode == command.PostalCode || (a.PostalCode == null && command.PostalCode == null))
                );

            int addressId;
            if (address != null)
            {
                // ✅ Use existing address
                addressId = address.Id;
            }
            else
            {
                // ✅ Create new address
                var newAddress = new Address(
                    command.Country,
                    command.City,
                    command.Street,
                    command.PostalCode
                );
                _context.Addresses.Add(newAddress);
                await _context.SaveChangesAsync();
                addressId = newAddress.Id;
            }

            // Convert time
            var checkIn = TimeSpan.Parse(command.CheckInTime);
            var checkOut = TimeSpan.Parse(command.CheckOutTime);

            // Create property
            var property = new Property(
                command.OwnerId,
                command.Name,
                command.Description,
                command.PropertyType,
                addressId,
                command.MaxGuests,
                checkIn,
                checkOut
            );

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            return new CreatePropertyResult { IsSuccess = true, PropertyId = property.Id };
        }
        catch (Exception ex)
        {
            return new CreatePropertyResult { IsSuccess = false, Error = ex.Message };
        }
    }
}

public class CreatePropertyResult
{
    public bool IsSuccess { get; set; }
    public int PropertyId { get; set; }
    public string? Error { get; set; }
}