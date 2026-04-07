namespace Booking.Domain.Users
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; private set; }
        public string PhoneNumber { get; private set; }
        public byte[]? ProfileImage { get; private set; }
        public string? ProfileImageContentType { get; private set; }
        public bool isActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        private User() { }
        public User(string firstName, string lastName, string email, string passwordHash)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PasswordHash = passwordHash;
            isActive = true;
            CreatedAt = DateTime.UtcNow;
            PhoneNumber = string.Empty;
        }
        public void UpdateProfile(string firstName, string lastName, string phoneNumber, string profileImageUrl)
        {
            if (!string.IsNullOrWhiteSpace(firstName))
                FirstName = firstName;

            if (!string.IsNullOrWhiteSpace(lastName))
                LastName = lastName;

            PhoneNumber = phoneNumber;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void UpdatePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            isActive = false;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            isActive = true;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void UpdateEmail(string newEmail)
        {
            if (string.IsNullOrWhiteSpace(newEmail))
                throw new ArgumentException("Email is required", nameof(newEmail));

            Email = newEmail;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void UpdatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return;

            PhoneNumber = phoneNumber;
        }
        public void UpdateProfileImage(byte[] imageData, string contentType)
        {
            if (imageData == null || imageData.Length == 0)
                throw new ArgumentException("Image data cannot be empty");

            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content type is required");

            ProfileImage = imageData;
            ProfileImageContentType = contentType;
        }

        public void RemoveProfileImage()
        {
            ProfileImage = null;
            ProfileImageContentType = null;
        }
    }
}