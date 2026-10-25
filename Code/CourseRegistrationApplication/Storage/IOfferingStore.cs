namespace CourseRegistrationApplication.Storage;

public interface IOfferingStore
{
    string OfferingId { get; }

    int Capacity { get; }

    int EnrolledCount { get; }

    bool IsEnrolled(string learnerId);

    void AddEnrolled(string learnerId, DateTimeOffset enrolledAtUtc);

    bool RemoveEnrolled(string learnerId);

    bool IsWaitlisted(string learnerId);

    IReadOnlyList<WaitlistEntry> WaitlistInOrder();

    WaitlistEntry AppendToWaitlist(string learnerId, DateTimeOffset joinedAtUtc);

    bool RemoveFromWaitlist(string learnerId);
}
