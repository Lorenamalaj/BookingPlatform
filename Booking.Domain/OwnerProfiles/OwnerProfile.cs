using System;

namespace Booking.Domain.OwnerProfiles
{
    public class OwnerProfile
    {
        public Guid UserId { get; private set; }
        public string IdentityCardNumber { get; private set; }
        public string VerificationStatus { get; private set; }
        public string BusinessName { get; private set; }
        public string CreditCard { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastModifiedAt { get; private set; }

        private OwnerProfile() { }

        public OwnerProfile(Guid userId, string identityCardNumber, string businessName = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            if (string.IsNullOrWhiteSpace(identityCardNumber))
                throw new ArgumentException("Identity card number is required", nameof(identityCardNumber));

            UserId = userId;
            IdentityCardNumber = identityCardNumber;
            BusinessName = businessName;
            VerificationStatus = "Pending";
            CreatedAt = DateTime.UtcNow;
        }
        public void Verify()
        {
            VerificationStatus = "Verified";
            LastModifiedAt = DateTime.UtcNow;
        }

        public void Reject()
        {
            VerificationStatus = "Rejected";
            LastModifiedAt = DateTime.UtcNow;
        }

        public void UpdateBusinessName(string businessName)
        {
            BusinessName = businessName;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void UpdateCreditCard(string creditCard)
        {
            // In real app, this should be encrypted/tokenized
            CreditCard = creditCard;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void UpdateIdentityCardNumber(string identityCardNumber)
        {
            if (string.IsNullOrWhiteSpace(identityCardNumber))
                throw new ArgumentException("Identity card number cannot be empty", nameof(identityCardNumber));

            IdentityCardNumber = identityCardNumber;
            VerificationStatus = "Pending"; // Reset verification status
            LastModifiedAt = DateTime.UtcNow;
        }

        public bool IsVerified() => VerificationStatus == "Verified";
        public bool IsPending() => VerificationStatus == "Pending";
        public bool IsRejected() => VerificationStatus == "Rejected";
    }
}