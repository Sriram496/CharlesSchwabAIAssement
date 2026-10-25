namespace CourseRegistrationApplication.Storage;

public sealed class Offering
{
    public Offering(string offeringId, int capacity)
    {
        if (string.IsNullOrWhiteSpace(offeringId))
        {
            throw new ArgumentException("Offering identifier is required.", nameof(offeringId));
        }

        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be greater than 0.");
        }

        OfferingId = offeringId;
        Capacity = capacity;
    }

    public string OfferingId { get; }

    public int Capacity { get; }
}
