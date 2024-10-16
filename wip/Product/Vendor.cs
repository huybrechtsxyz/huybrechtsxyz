namespace Huybrechts.Core.Architect;

public record Vendor: Entity, IEntity
{
    
}

public record VendorContract: Entity, IEntity
{
    public Ulid VendorId { get; set; }
    
    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public TimeSpan Duration { get; set; }

    // Subscription, yearly ...
    public string LicenseType { get; set; } = string.Empty;
}