namespace CourseRegistrationApplication.Waitlist;

public interface IWaitlistQueue
{
    int Join(string learnerId, DateTimeOffset joinedAtUtc);

    bool Contains(string learnerId);

    int? PositionOf(string learnerId);

    bool Remove(string learnerId);

    string? PromoteNext(DateTimeOffset enrolledAtUtc);
}
