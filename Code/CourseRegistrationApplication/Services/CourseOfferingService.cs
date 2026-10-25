using CourseRegistrationApplication.Eligibility;
using CourseRegistrationApplication.Outcomes;
using CourseRegistrationApplication.Storage;
using CourseRegistrationApplication.Waitlist;

namespace CourseRegistrationApplication.Services;

public sealed class CourseOfferingService : ICourseOfferingService
{
    private readonly IOfferingStore _store;
    private readonly IWaitlistQueue _waitlist;
    private readonly IEligibilityProvider _eligibility;
    private readonly TimeProvider _timeProvider;
    private readonly Lock _gate = new();

    public CourseOfferingService(
        IOfferingStore store,
        IWaitlistQueue waitlist,
        IEligibilityProvider eligibility,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _waitlist = waitlist ?? throw new ArgumentNullException(nameof(waitlist));
        _eligibility = eligibility ?? throw new ArgumentNullException(nameof(eligibility));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public EnrollOutcome Enroll(string learnerId)
    {
        using Lock.Scope scope = _gate.EnterScope();
        DateTimeOffset now = _timeProvider.GetUtcNow();

        if (!_eligibility.IsKnownLearner(learnerId))
        {
            return new EnrollOutcome.RejectedUnknownLearner();
        }

        if (_store.IsEnrolled(learnerId))
        {
            return new EnrollOutcome.AlreadyEnrolled();
        }

        if (_waitlist.PositionOf(learnerId) is int existingPosition)
        {
            return new EnrollOutcome.AlreadyWaitlisted(existingPosition);
        }

        if (!_eligibility.HasMetPrerequisite(learnerId))
        {
            return new EnrollOutcome.RejectedPrerequisiteNotMet();
        }

        if (_store.EnrolledCount < _store.Capacity)
        {
            _store.AddEnrolled(learnerId, now);
            return new EnrollOutcome.Enrolled();
        }

        int position = _waitlist.Join(learnerId, now);
        return new EnrollOutcome.Waitlisted(position);
    }

    public DropOutcome Drop(string learnerId)
    {
        using Lock.Scope scope = _gate.EnterScope();
        DateTimeOffset now = _timeProvider.GetUtcNow();

        if (!_eligibility.IsKnownLearner(learnerId))
        {
            return new DropOutcome.RejectedUnknownLearner();
        }

        if (_store.IsEnrolled(learnerId))
        {
            _store.RemoveEnrolled(learnerId);
            return new DropOutcome.Dropped(_waitlist.PromoteNext(now));
        }

        if (_waitlist.Remove(learnerId))
        {
            return new DropOutcome.RemovedFromWaitlist();
        }

        return new DropOutcome.RejectedNotEnrolled();
    }

    public StatusOutcome GetStatus(string learnerId)
    {
        using Lock.Scope scope = _gate.EnterScope();

        if (!_eligibility.IsKnownLearner(learnerId))
        {
            return new StatusOutcome.RejectedUnknownLearner();
        }

        if (_store.IsEnrolled(learnerId))
        {
            return new StatusOutcome.Enrolled();
        }

        if (_waitlist.PositionOf(learnerId) is int position)
        {
            return new StatusOutcome.Waitlisted(position);
        }

        return new StatusOutcome.NotEnrolledOrWaitlisted();
    }
}
