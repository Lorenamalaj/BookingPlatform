using System;

namespace Booking.Domain.Reviews
{
    public class Review
    {
        public int Id { get; private set; }
        public int BookingId { get; private set; }
        public int GuestId { get; private set; }
        public int Rating { get; private set; }
        public string Comment { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Review() { }

        public Review(int bookingId, int guestId, int rating, string comment = null)
        {
            if (bookingId <= 0)
                throw new ArgumentException("Invalid booking ID", nameof(bookingId));

            if (guestId <= 0)
                throw new ArgumentException("Invalid guest ID", nameof(guestId));

            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

            BookingId = bookingId;
            GuestId = guestId;
            Rating = rating;
            Comment = comment;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateReview(int rating, string comment)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

            Rating = rating;
            Comment = comment;
        }

        public bool IsPositive() => Rating >= 4;
        public bool IsNegative() => Rating <= 2;
        public bool IsNeutral() => Rating == 3;
    }
}