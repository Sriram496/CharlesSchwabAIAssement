using CourseRegistrationApplication.Eligibility;
using CourseRegistrationApplication.Outcomes;
using CourseRegistrationApplication.Services;
using CourseRegistrationApplication.Storage;
using CourseRegistrationApplication.Waitlist;

namespace CourseRegistrationApplication.Test.Services;

public class CourseOfferingServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 25, 9, 0, 0, TimeSpan.Zero);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed record Setup(CourseOfferingService Service, InMemoryStore Store, EligibilityProvider Eligibility);

    private static Setup CreateSetup(int capacity = 1)
    {
        var store = new InMemoryStore(new Offering("CS101", capacity));
        var eligibility = new EligibilityProvider();
        var waitlist = new WaitlistQueueManager(store, eligibility);
        var service = new CourseOfferingService(store, waitlist, eligibility, new FixedTimeProvider(Now));
        return new Setup(service, store, eligibility);
    }

    [Fact]
    public void Enroll_UnknownLearner_ReturnsRejectedUnknownLearner()
    {
        var (service, store, _) = CreateSetup();

        Assert.Equal(new EnrollOutcome.RejectedUnknownLearner(), service.Enroll("L9"));
        Assert.Equal(0, store.EnrolledCount);
    }

    [Fact]
    public void Enroll_PrerequisiteNotMet_ReturnsRejectedPrerequisiteNotMet()
    {
        var (service, store, eligibility) = CreateSetup();
        eligibility.AddLearner("L1", prerequisiteMet: false);

        Assert.Equal(new EnrollOutcome.RejectedPrerequisiteNotMet(), service.Enroll("L1"));
        Assert.False(store.IsEnrolled("L1"));
    }

    [Fact]
    public void Enroll_SeatFree_Enrolls()
    {
        // REQ-FUN-001
        var (service, store, eligibility) = CreateSetup();
        eligibility.AddLearner("L1", prerequisiteMet: true);

        Assert.Equal(new EnrollOutcome.Enrolled(), service.Enroll("L1"));
        Assert.True(store.IsEnrolled("L1"));
    }

    [Fact]
    public void Enroll_OfferingFull_WaitlistsWithPosition()
    {
        // REQ-FUN-002
        var (service, store, eligibility) = CreateSetup(capacity: 1);
        store.AddEnrolled("E1", Now);
        eligibility.AddLearner("L2", prerequisiteMet: true);

        Assert.Equal(new EnrollOutcome.Waitlisted(1), service.Enroll("L2"));
        Assert.Equal(Now, store.WaitlistInOrder()[0].JoinedAtUtc);
    }

    [Fact]
    public void Enroll_AlreadyEnrolled_ReturnsAlreadyEnrolled()
    {
        // REQ-FUN-005
        var (service, _, eligibility) = CreateSetup();
        eligibility.AddLearner("L1", prerequisiteMet: true);
        service.Enroll("L1");

        Assert.Equal(new EnrollOutcome.AlreadyEnrolled(), service.Enroll("L1"));
    }

    [Fact]
    public void Enroll_AlreadyWaitlisted_ReturnsPosition()
    {
        // REQ-FUN-005
        var (service, store, eligibility) = CreateSetup();
        store.AddEnrolled("E1", Now);
        eligibility.AddLearner("L2", prerequisiteMet: true);
        service.Enroll("L2");

        Assert.Equal(new EnrollOutcome.AlreadyWaitlisted(1), service.Enroll("L2"));
    }

    [Fact]
    public void Enroll_EnrolledLearnerWhosePrerequisiteLaterUnmet_KeepsSeat()
    {
        // ASM-006
        var (service, _, eligibility) = CreateSetup();
        eligibility.AddLearner("L1", prerequisiteMet: true);
        service.Enroll("L1");
        eligibility.SetPrerequisiteMet("L1", prerequisiteMet: false);

        Assert.Equal(new EnrollOutcome.AlreadyEnrolled(), service.Enroll("L1"));
    }

    [Fact]
    public void Enroll_RejectedRequests_ChangeNothing()
    {
        // REQ-NFR-007
        var (service, store, eligibility) = CreateSetup();
        eligibility.AddLearner("L1", prerequisiteMet: false);

        service.Enroll("L9");
        service.Enroll("L1");

        Assert.Equal(0, store.EnrolledCount);
        Assert.Empty(store.WaitlistInOrder());
    }

    [Fact]
    public void Drop_UnknownLearner_ReturnsRejectedUnknownLearner()
    {
        // REQ-FUN-011
        var (service, _, _) = CreateSetup();

        Assert.Equal(new DropOutcome.RejectedUnknownLearner(), service.Drop("L9"));
    }

    [Fact]
    public void Drop_NotEnrolledOrWaiting_ReturnsRejectedNotEnrolled()
    {
        // REQ-FUN-010
        var (service, _, eligibility) = CreateSetup();
        eligibility.AddLearner("L1", prerequisiteMet: true);

        Assert.Equal(new DropOutcome.RejectedNotEnrolled(), service.Drop("L1"));
    }

    [Fact]
    public void Drop_EnrolledWithEligibleWaiter_PromotesNext()
    {
        // REQ-FUN-008, REQ-FUN-012
        var (service, store, eligibility) = CreateSetup();
        eligibility.AddLearner("L1", prerequisiteMet: true);
        eligibility.AddLearner("L2", prerequisiteMet: true);
        service.Enroll("L1");
        service.Enroll("L2");

        Assert.Equal(new DropOutcome.Dropped("L2"), service.Drop("L1"));
        Assert.True(store.IsEnrolled("L2"));
        Assert.False(store.IsEnrolled("L1"));
    }

    [Fact]
    public void Drop_EnrolledNobodyEligible_ReturnsDroppedWithoutPromotion()
    {
        // REQ-FUN-015
        var (service, store, eligibility) = CreateSetup();
        eligibility.AddLearner("L1", prerequisiteMet: true);
        eligibility.AddLearner("L2", prerequisiteMet: true);
        service.Enroll("L1");
        service.Enroll("L2");
        eligibility.SetPrerequisiteMet("L2", prerequisiteMet: false);

        Assert.Equal(new DropOutcome.Dropped(null), service.Drop("L1"));
        Assert.Equal(0, store.EnrolledCount);
        Assert.True(store.IsWaitlisted("L2"));
    }

    [Fact]
    public void Drop_WaitingLearner_RemovedWithoutPromotion()
    {
        // REQ-FUN-009, ASM-008
        var (service, store, eligibility) = CreateSetup();
        store.AddEnrolled("E1", Now);
        eligibility.AddLearner("L2", prerequisiteMet: true);
        service.Enroll("L2");

        Assert.Equal(new DropOutcome.RemovedFromWaitlist(), service.Drop("L2"));
        Assert.Equal(1, store.EnrolledCount);
        Assert.False(store.IsWaitlisted("L2"));
    }

    [Fact]
    public void GetStatus_UnknownLearner_ReturnsRejectedUnknownLearner()
    {
        // REQ-FUN-011
        var (service, _, _) = CreateSetup();

        Assert.Equal(new StatusOutcome.RejectedUnknownLearner(), service.GetStatus("L9"));
    }

    [Fact]
    public void GetStatus_Enrolled_ReturnsEnrolled()
    {
        // REQ-FUN-007
        var (service, _, eligibility) = CreateSetup();
        eligibility.AddLearner("L1", prerequisiteMet: true);
        service.Enroll("L1");

        Assert.Equal(new StatusOutcome.Enrolled(), service.GetStatus("L1"));
    }

    [Fact]
    public void GetStatus_Waitlisted_ReturnsPosition()
    {
        // REQ-FUN-007
        var (service, store, eligibility) = CreateSetup();
        store.AddEnrolled("E1", Now);
        eligibility.AddLearner("L2", prerequisiteMet: true);
        service.Enroll("L2");

        Assert.Equal(new StatusOutcome.Waitlisted(1), service.GetStatus("L2"));
    }

    [Fact]
    public void GetStatus_NeitherEnrolledNorWaitlisted_ReturnsNotEnrolledOrWaitlisted()
    {
        // REQ-FUN-007
        var (service, _, eligibility) = CreateSetup();
        eligibility.AddLearner("L1", prerequisiteMet: true);

        Assert.Equal(new StatusOutcome.NotEnrolledOrWaitlisted(), service.GetStatus("L1"));
    }

    [Fact]
    public async Task Enroll_ConcurrentRequestsForOneSeat_EnrollsExactlyOne()
    {
        // REQ-NFR-003
        const int learnerCount = 50;
        var (service, store, eligibility) = CreateSetup(capacity: 1);
        var learnerIds = Enumerable.Range(1, learnerCount).Select(i => $"L{i}").ToArray();
        foreach (string id in learnerIds)
        {
            eligibility.AddLearner(id, prerequisiteMet: true);
        }

        EnrollOutcome[] results = await Task.WhenAll(
            learnerIds.Select(id => Task.Run(() => service.Enroll(id))));

        Assert.Equal(1, results.Count(r => r is EnrollOutcome.Enrolled));
        Assert.Equal(learnerCount - 1, results.Count(r => r is EnrollOutcome.Waitlisted));
        Assert.Equal(1, store.EnrolledCount);
    }
}
