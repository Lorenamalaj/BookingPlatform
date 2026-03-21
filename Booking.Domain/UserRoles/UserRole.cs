using Booking.Domain.Users;
using System;

namespace Booking.Domain.UserRoles
{
    public class UserRole
    {
        public Guid UserId { get; private set; }
        public Guid RoleId { get; private set; }
        public DateTime AssignedAt { get; private set; }

        private UserRole() { }

        public UserRole(Guid userId, Guid roleId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));
            if (roleId == Guid.Empty)
                throw new ArgumentException("Invalid role ID", nameof(roleId)); 

            UserId = userId;
            RoleId = roleId;
            AssignedAt = DateTime.UtcNow;
        }
    }
}