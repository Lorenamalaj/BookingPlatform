using System;
using System.Collections.Generic;
using System.Text;

namespace Booking.Application.Properties.Queries.GetAllProperties
{
    public class GetAllPropertiesQuery
    {
        public int Page { get; set; } = 1;    
        public int PageSize { get; set; } = 10;
    }
}
