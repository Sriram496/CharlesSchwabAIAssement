using CourseRegistrationApplication.Eligibility;
using CourseRegistrationApplication.Storage;
using CourseRegistrationApplication.Waitlist;

namespace CourseRegistrationApplication.Test.Waitlist;

public class WaitlistQueueManagerTests
{
    private static readonly DateTimeOffset Joined = new(2026, 10, 25, 9, 0, 0, TimeSpan.Zero);

    private sealed record Setup(WaitlistQueueManager Queue, InMemoryStore Store, EligibilityProvider Eligibility);

    private static Setup CreateSetup(int capacity = 2)
    {
        var store = new InMemoryStore(new Offering("CS101", capacity));
        var eligibility = new EligibilityProvider();
        return new Setup(new WaitlistQueueManager(store, eligibility), store, eligibility);
    }

    private static WaitlistQueueManager CreateQueue() => CreateSetup().Queue;

    [Fact]
    public void Join_ReturnsOneBasedPositionInJoiningOrder()
    {
        var queue = CreateQueue();

        Assert.Equal(1, queue.Join("L1", Joined));
        Assert.Equal(2, queue.Join("L2", Joined));
    }

    [Fact]
    public void Contains_ReflectsWaitlistMembership()
    {
        var queue = CreateQueue();
        queue.Join("L1", Joined);

        Assert.True(queue.Contains("L1"));
        Assert.False(queue.Contains("L2"));
    }

    [Fact]
    public void PositionOf_UnknownLearner_ReturnsNull()
    {
        var queue = CreateQueue();

        Assert.Null(queue.PositionOf("L9"));
    }

    [Fact]
    public void PositionOf_MovesUpWhenEarlierLearnerLeaves()
    {
        // REQ-FUN-016
        var queue = CreateQueue();
        queue.Join("L1", Joined);
        queue.Join("L2", Joined);
        queue.Join("L3", Joined);

        queue.Remove("L1");

        Assert.Equal<int?>(1, queue.PositionOf("L2"));
        Assert.Equal<int?>(2, queue.PositionOf("L3"));
    }

    [Fact]
    public void Join_AfterLeavingAndReturning_GoesToEndOfQueue()
    {
        // REQ-FUN-006
        var queue = CreateQueue();
        queue.Join("L1", Joined);
        queue.Join("L2", Joined);
        queue.Remove("L1");

        int position = queue.Join("L1", Joined);

        Assert.Equal(2, position);
        Assert.Equal<int?>(1, queue.PositionOf("L2"));
    }

    [Fact]
    public void Join_WhenAlreadyWaiting_Throws()
    {
        var queue = CreateQueue();
        queue.Join("L1", Joined);

        Assert.Throws<InvalidOperationException>(() => queue.Join("L1", Joined));
    }

    [Fact]
    public void Remove_ReturnsFalseWhenNotWaiting()
    {
        var queue = CreateQueue();

        Assert.False(queue.Remove("L1"));
    }

    [Fact]
    public void PromoteNext_EligibleEarliestLearner_IsEnrolledAndRemoved()
    {
        // REQ-FUN-012
        var (queue, store, eligibility) = CreateSetup(capacity: 1);
        eligibility.AddLearner("L1", prerequisiteMet: true);
        eligibility.AddLearner("L2", prerequisiteMet: true);
        queue.Join("L1", Joined);
        queue.Join("L2", Joined);

        string? promoted = queue.PromoteNext(Joined);

        Assert.Equal("L1", promoted);
        Assert.True(store.IsEnrolled("L1"));
        Assert.False(queue.Contains("L1"));
        Assert.Equal<int?>(1, queue.PositionOf("L2"));
    }

    [Fact]
    public void PromoteNext_SkipsIneligibleLearnerAndKeepsPlace()
    {
        // REQ-FUN-014
        var (queue, store, eligibility) = CreateSetup(capacity: 1);
        eligibility.AddLearner("L1", prerequisiteMet: false);
        eligibility.AddLearner("L2", prerequisiteMet: true);
        eligibility.AddLearner("L3", prerequisiteMet: true);
        queue.Join("L1", Joined);
        queue.Join("L2", Joined);
        queue.Join("L3", Joined);

        string? promoted = queue.PromoteNext(Joined);

        Assert.Equal("L2", promoted);
        Assert.False(store.IsEnrolled("L1"));
        Assert.Equal<int?>(1, queue.PositionOf("L1"));
        Assert.Equal<int?>(2, queue.PositionOf("L3"));
    }

    [Fact]
    public void PromoteNext_NobodyEligible_ReturnsNullAndChangesNothing()
    {
        // REQ-FUN-015
        var (queue, store, eligibility) = CreateSetup(capacity: 1);
        eligibility.AddLearner("L1", prerequisiteMet: false);
        queue.Join("L1", Joined);

        string? promoted = queue.PromoteNext(Joined);

        Assert.Null(promoted);
        Assert.Equal(0, store.EnrolledCount);
        Assert.Equal<int?>(1, queue.PositionOf("L1"));
    }

    [Fact]
    public void PromoteNext_RechecksEligibilityAtPromotionTime()
    {
        // REQ-FUN-013
        var (queue, _, eligibility) = CreateSetup(capacity: 1);
        eligibility.AddLearner("L1", prerequisiteMet: true);
        queue.Join("L1", Joined);
        eligibility.SetPrerequisiteMet("L1", prerequisiteMet: false);

        Assert.Null(queue.PromoteNext(Joined));

        eligibility.SetPrerequisiteMet("L1", prerequisiteMet: true);

        Assert.Equal("L1", queue.PromoteNext(Joined));
    }

    [Fact]
    public void PromoteNext_PromotesAtMostOneLearner()
    {
        var (queue, store, eligibility) = CreateSetup(capacity: 2);
        eligibility.AddLearner("L1", prerequisiteMet: true);
        eligibility.AddLearner("L2", prerequisiteMet: true);
        queue.Join("L1", Joined);
        queue.Join("L2", Joined);

        queue.PromoteNext(Joined);

        Assert.Equal(1, store.EnrolledCount);
        Assert.True(queue.Contains("L2"));
    }

    [Fact]
    public void PromoteNext_UnknownLearnerIsSkipped()
    {
        var (queue, _, eligibility) = CreateSetup(capacity: 1);
        queue.Join("L1", Joined);
        eligibility.AddLearner("L2", prerequisiteMet: true);
        queue.Join("L2", Joined);

        Assert.Equal("L2", queue.PromoteNext(Joined));
        Assert.Equal<int?>(1, queue.PositionOf("L1"));
    }

    [Fact]
    public void PromoteNext_WhenOfferingFull_ReturnsNullAndChangesNothing()
    {
        var (queue, store, eligibility) = CreateSetup(capacity: 1);
        store.AddEnrolled("E1", Joined);
        eligibility.AddLearner("L2", prerequisiteMet: true);
        queue.Join("L2", Joined);

        Assert.Null(queue.PromoteNext(Joined));
        Assert.True(queue.Contains("L2"));
        Assert.Equal(1, store.EnrolledCount);
    }
}
