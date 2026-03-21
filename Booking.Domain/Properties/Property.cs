using Booking.Domain.Addresses;
using System;

namespace Booking.Domain.Properties
{
    public class Property
    {
        public Guid Id { get; private set; }
        public Guid OwnerId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string PropertyType { get; private set; }
        public Guid AddressId { get; private set; }
        public Address? Address { get; private set; }
        public int MaxGuests { get; private set; }
        public TimeSpan CheckInTime { get; private set; }
        public TimeSpan CheckOutTime { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsApproved { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastModifiedAt { get; private set; }
        public DateTime? LastBookedOnUtc { get; private set; }

        private Property() { }

        public Property(
            Guid ownerId,
            string name,
            string description,
            string propertyType,
            Guid addressId,
            int maxGuests,
            TimeSpan checkInTime,
            TimeSpan checkOutTime)
        {
            if (ownerId == Guid.Empty)
                throw new ArgumentException("Invalid owner ID", nameof(ownerId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Property name is required", nameof(name));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required", nameof(description));

            if (string.IsNullOrWhiteSpace(propertyType))
                throw new ArgumentException("Property type is required", nameof(propertyType));

            if (addressId == Guid.Empty)
                throw new ArgumentException("Invalid address ID", nameof(addressId));

            if (maxGuests <= 0)
                throw new ArgumentException("Max guests must be greater than 0", nameof(maxGuests));

            OwnerId = ownerId;
            Name = name;
            Description = description;
            PropertyType = propertyType;
            AddressId = addressId;
            MaxGuests = maxGuests;
            CheckInTime = checkInTime;
            CheckOutTime = checkOutTime;
            IsActive = true;
            IsApproved = false;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(string name, string description, int maxGuests)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;

            if (!string.IsNullOrWhiteSpace(description))
                Description = description;

            if (maxGuests > 0)
                MaxGuests = maxGuests;

            LastModifiedAt = DateTime.UtcNow;
        }

        public void UpdateCheckInCheckOut(TimeSpan checkInTime, TimeSpan checkOutTime)
        {
            CheckInTime = checkInTime;
            CheckOutTime = checkOutTime;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void Approve()
        {
            IsApproved = true;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void Reject()
        {
            IsApproved = false;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void MarkAsBooked()
        {
            LastBookedOnUtc = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }

        public bool CanBeBooked() => IsActive && IsApproved;
    }
}