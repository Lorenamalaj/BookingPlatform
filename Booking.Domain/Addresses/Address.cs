using System;

namespace Booking.Domain.Addresses
{
    public class Address
    {
        public int Id { get; private set; }
        public string Country { get; private set; }
        public string City { get; private set; }
        public string Street { get; private set; }
        public string PostalCode { get; private set; }

        private Address() { }

        public Address(string country, string city, string street, string postalCode = null)
        {
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country is required", nameof(country));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required", nameof(city));

            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street is required", nameof(street));

            Country = country;
            City = city;
            Street = street;
            PostalCode = postalCode;
        }

        public void Update(string country, string city, string street, string postalCode)
        {
            if (!string.IsNullOrWhiteSpace(country))
                Country = country;

            if (!string.IsNullOrWhiteSpace(city))
                City = city;

            if (!string.IsNullOrWhiteSpace(street))
                Street = street;

            PostalCode = postalCode;
        }

        public string GetFullAddress()
        {
            return $"{Street}, {City}, {Country}" +
                   (string.IsNullOrWhiteSpace(PostalCode) ? "" : $" {PostalCode}");
        }
    }
}