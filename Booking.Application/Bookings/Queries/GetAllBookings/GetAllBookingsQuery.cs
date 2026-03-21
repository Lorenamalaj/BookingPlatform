using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Bookings.Queries.GetAllBookings
{
    public class GetAllBookingsQuery

    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
