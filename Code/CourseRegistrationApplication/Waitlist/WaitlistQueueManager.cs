using CourseRegistrationApplication.Eligibility;
using CourseRegistrationApplication.Storage;

namespace CourseRegistrationApplication.Waitlist;

public sealed class WaitlistQueueManager : IWaitlistQueue
{
    private readonly IOfferingStore _store;
    private readonly IEligibilityProvider _eligibility;

    public WaitlistQueueManager(IOfferingStore store, IEligibilityProvider eligibility)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _eligibility = eligibility ?? throw new ArgumentNullException(nameof(eligibility));
    }

    public int Join(string learnerId, DateTimeOffset joinedAtUtc)
    {
        _store.AppendToWaitlist(learnerId, joinedAtUtc);
        return _store.WaitlistInOrder().Count;
    }

    public bool Contains(string learnerId) => _store.IsWaitlisted(learnerId);

    public int? PositionOf(string learnerId)
    {
        IReadOnlyList<WaitlistEntry> entries = _store.WaitlistInOrder();
        for (int index = 0; index < entries.Count; index++)
        {
            if (entries[index].LearnerId == learnerId)
            {
                return index + 1;
            }
        }

        return null;
    }

    public bool Remove(string learnerId) => _store.RemoveFromWaitlist(learnerId);

    public string? PromoteNext(DateTimeOffset enrolledAtUtc)
    {
        if (_store.EnrolledCount >= _store.Capacity)
        {
            return null;
        }

        foreach (WaitlistEntry entry in _store.WaitlistInOrder())
        {
            if (!IsEligible(entry.LearnerId))
            {
                continue;
            }

            _store.RemoveFromWaitlist(entry.LearnerId);
            _store.AddEnrolled(entry.LearnerId, enrolledAtUtc);
            return entry.LearnerId;
        }

        return null;
    }

    private bool IsEligible(string learnerId) =>
        _eligibility.IsKnownLearner(learnerId) && _eligibility.HasMetPrerequisite(learnerId);
}
