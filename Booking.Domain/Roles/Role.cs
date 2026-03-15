using System;

namespace Booking.Domain.Roles
{
    public class Role
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        private Role() { }

        public Role(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required", nameof(name));

            Name = name;
            Description = description;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }
    }
}