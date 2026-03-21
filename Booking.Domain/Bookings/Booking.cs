using Booking.Domain.Properties;
using System;

namespace Booking.Domain.Bookings
{
    public class Booking
    {
        public Guid Id { get; private set; }
        public Guid PropertyId { get; private set; }
        public Guid GuestId { get; private set; }
        public Property? Property { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public int GuestCount { get; private set; }
        public decimal CleaningFee { get; private set; }
        public decimal AmenitiesUpCharge { get; private set; }
        public decimal PriceForPeriod { get; private set; }
        public decimal TotalPrice { get; private set; }
        public string BookingStatus { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastModifiedAt { get; private set; }
        public DateTime CreatedOnUtc { get; private set; }
        public DateTime? ConfirmedOnUtc { get; private set; }
        public DateTime? RejectedOnUtc { get; private set; }
        public DateTime? CompletedOnUtc { get; private set; }
        public DateTime? CancelledOnUtc { get; private set; }

        private Booking() { }

        public Booking(
            Guid propertyId,
            Guid guestId,
            DateTime startDate,
            DateTime endDate,
            int guestCount,
            decimal cleaningFee,
            decimal amenitiesUpCharge,
            decimal priceForPeriod)
        {
            if (propertyId == Guid.Empty)
                throw new ArgumentException("Invalid property ID", nameof(propertyId));

            if (guestId == Guid.Empty)
                throw new ArgumentException("Invalid guest ID", nameof(guestId));

            if (startDate >= endDate)
                throw new ArgumentException("End date must be after start date");

            if (startDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Cannot book in the past");

            if (guestCount <= 0)
                throw new ArgumentException("Guest count must be greater than 0", nameof(guestCount));

            if (priceForPeriod <= 0)
                throw new ArgumentException("Price must be greater than 0", nameof(priceForPeriod));

            PropertyId = propertyId;
            GuestId = guestId;
            StartDate = startDate;
            EndDate = endDate;
            GuestCount = guestCount;
            CleaningFee = cleaningFee;
            AmenitiesUpCharge = amenitiesUpCharge;
            PriceForPeriod = priceForPeriod;
            TotalPrice = priceForPeriod + cleaningFee + amenitiesUpCharge;
            BookingStatus = "Pending";
            CreatedAt = DateTime.UtcNow;
            CreatedOnUtc = DateTime.UtcNow;
        }

        // Domain methods
        public void Confirm()
        {
            if (BookingStatus != "Pending")
                throw new InvalidOperationException("Can only confirm pending bookings");

            BookingStatus = "Confirmed";
            ConfirmedOnUtc = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void Reject()
        {
            if (BookingStatus != "Pending")
                throw new InvalidOperationException("Can only reject pending bookings");

            BookingStatus = "Rejected";
            RejectedOnUtc = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            if (BookingStatus != "Confirmed")
                throw new InvalidOperationException("Can only complete confirmed bookings");

            if (DateTime.UtcNow < EndDate)
                throw new InvalidOperationException("Cannot complete booking before end date");

            BookingStatus = "Completed";
            CompletedOnUtc = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (BookingStatus == "Completed" || BookingStatus == "Cancelled")
                throw new InvalidOperationException($"Cannot cancel {BookingStatus.ToLower()} booking");

            BookingStatus = "Cancelled";
            CancelledOnUtc = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }

        public int GetNumberOfNights()
        {
            return (EndDate - StartDate).Days;
        }

        public bool CanBeCancelled()
        {
            return BookingStatus != "Completed" &&
                   BookingStatus != "Cancelled" &&
                   StartDate.AddDays(-1) > DateTime.UtcNow;
        }

        public bool CanBeReviewed()
        {
            return BookingStatus == "Completed" && CompletedOnUtc.HasValue;
        }

        public bool IsActive()
        {
            return BookingStatus == "Confirmed" &&
                   StartDate <= DateTime.UtcNow &&
                   EndDate >= DateTime.UtcNow;
        }
    }
}