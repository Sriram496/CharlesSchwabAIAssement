namespace CourseRegistrationApplication.Storage;

public sealed class InMemoryStore : IOfferingStore
{
    private readonly Dictionary<string, EnrollmentRecord> _enrollments = new();
    private readonly List<WaitlistEntry> _waitlist = [];
    private long _nextSequence = 1;

    public InMemoryStore(Offering offering)
    {
        ArgumentNullException.ThrowIfNull(offering);

        OfferingId = offering.OfferingId;
        Capacity = offering.Capacity;
    }

    public string OfferingId { get; }

    public int Capacity { get; }

    public int EnrolledCount => _enrollments.Count;

    public bool IsEnrolled(string learnerId) => _enrollments.ContainsKey(learnerId);

    public void AddEnrolled(string learnerId, DateTimeOffset enrolledAtUtc)
    {
        if (!_enrollments.TryAdd(learnerId, new EnrollmentRecord(learnerId, enrolledAtUtc)))
        {
            throw new InvalidOperationException($"Learner '{learnerId}' is already enrolled.");
        }
    }

    public bool RemoveEnrolled(string learnerId) => _enrollments.Remove(learnerId);

    public bool IsWaitlisted(string learnerId) => FindWaitlistIndex(learnerId) >= 0;

    // The list is always sorted by Sequence: entries are only appended and removed, never reordered.
    public IReadOnlyList<WaitlistEntry> WaitlistInOrder() => _waitlist.ToList();

    public WaitlistEntry AppendToWaitlist(string learnerId, DateTimeOffset joinedAtUtc)
    {
        if (IsWaitlisted(learnerId))
        {
            throw new InvalidOperationException($"Learner '{learnerId}' is already on the waiting list.");
        }

        var entry = new WaitlistEntry(learnerId, _nextSequence++, joinedAtUtc);
        _waitlist.Add(entry);
        return entry;
    }

    public bool RemoveFromWaitlist(string learnerId)
    {
        int index = FindWaitlistIndex(learnerId);
        if (index < 0)
        {
            return false;
        }

        _waitlist.RemoveAt(index);
        return true;
    }

    private int FindWaitlistIndex(string learnerId) =>
        _waitlist.FindIndex(entry => entry.LearnerId == learnerId);
}
