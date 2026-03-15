using System;

namespace Booking.Domain.UserRoles
{
    public class UserRole
    {
        public int UserId { get; private set; }
        public int RoleId { get; private set; }
        public DateTime AssignedAt { get; private set; }

        private UserRole() { }

        public UserRole(int userId, int roleId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            if (roleId <= 0)
                throw new ArgumentException("Invalid role ID", nameof(roleId));

            UserId = userId;
            RoleId = roleId;
            AssignedAt = DateTime.UtcNow;
        }
    }
}