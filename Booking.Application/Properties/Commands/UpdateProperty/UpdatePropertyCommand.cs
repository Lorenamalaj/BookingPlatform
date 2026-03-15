using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Properties.Commands.UpdateProperty
{
    public class UpdatePropertyCommand
    {
        public int PropertyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PropertyType { get; set; }
        public int MaxGuests { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
    }
}
