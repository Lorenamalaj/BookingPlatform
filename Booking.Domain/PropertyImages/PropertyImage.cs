namespace Booking.Domain.PropertyImages;

public class PropertyImage
{
    public Guid Id { get; private set; }
    public Guid PropertyId { get; private set; }
    public byte[] ImageData { get; private set; }
    public string ContentType { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private PropertyImage() { }
    public PropertyImage(Guid propertyId, byte[] imageData, string contentType, bool isPrimary = false)
    {
        Id = Guid.NewGuid();
        PropertyId = propertyId;
        ImageData = imageData ?? throw new ArgumentNullException(nameof(imageData));
        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        IsPrimary = isPrimary;
        UploadedAt = DateTime.UtcNow;
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
    }

    public void UnsetAsPrimary()
    {
        IsPrimary = false;
    }
}