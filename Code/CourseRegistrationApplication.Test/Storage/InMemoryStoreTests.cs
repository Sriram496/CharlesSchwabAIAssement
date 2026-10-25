using CourseRegistrationApplication.Storage;

namespace CourseRegistrationApplication.Test.Storage;

public class InMemoryStoreTests
{
    private static readonly DateTimeOffset Joined = new(2026, 10, 25, 9, 0, 0, TimeSpan.Zero);

    private static InMemoryStore CreateStore(int capacity = 2) => new(new Offering("CS101", capacity));

    [Fact]
    public void Constructor_ExposesOfferingIdAndCapacity()
    {
        var store = CreateStore(3);

        Assert.Equal("CS101", store.OfferingId);
        Assert.Equal(3, store.Capacity);
    }

    [Fact]
    public void AddEnrolled_IncrementsEnrolledCount()
    {
        var store = CreateStore();

        store.AddEnrolled("L1", Joined);

        Assert.Equal(1, store.EnrolledCount);
        Assert.True(store.IsEnrolled("L1"));
    }

    [Fact]
    public void AddEnrolled_WhenAlreadyEnrolled_Throws()
    {
        var store = CreateStore();
        store.AddEnrolled("L1", Joined);

        Assert.Throws<InvalidOperationException>(() => store.AddEnrolled("L1", Joined));
    }

    [Fact]
    public void RemoveEnrolled_ReturnsTrueOnceThenFalse()
    {
        var store = CreateStore();
        store.AddEnrolled("L1", Joined);

        Assert.True(store.RemoveEnrolled("L1"));
        Assert.False(store.RemoveEnrolled("L1"));
        Assert.False(store.IsEnrolled("L1"));
    }

    [Fact]
    public void AppendToWaitlist_AssignsIncreasingSequence()
    {
        var store = CreateStore();

        var first = store.AppendToWaitlist("L1", Joined);
        var second = store.AppendToWaitlist("L2", Joined);

        Assert.Equal(1L, first.Sequence);
        Assert.Equal(2L, second.Sequence);
    }

    [Fact]
    public void AppendToWaitlist_DoesNotReuseSequenceAfterRemoval()
    {
        // REQ-FUN-016
        var store = CreateStore();
        store.AppendToWaitlist("L1", Joined);
        store.AppendToWaitlist("L2", Joined);
        store.RemoveFromWaitlist("L1");

        var third = store.AppendToWaitlist("L3", Joined);

        Assert.Equal(3L, third.Sequence);
    }

    [Fact]
    public void WaitlistInOrder_ReturnsEntriesAscendingBySequence()
    {
        // REQ-FUN-016
        var store = CreateStore();
        store.AppendToWaitlist("L1", Joined);
        store.AppendToWaitlist("L2", Joined);
        store.AppendToWaitlist("L3", Joined);

        var ids = store.WaitlistInOrder().Select(entry => entry.LearnerId).ToList();

        Assert.Equal(new[] { "L1", "L2", "L3" }, ids);
    }

    [Fact]
    public void AppendToWaitlist_WhenAlreadyWaiting_Throws()
    {
        var store = CreateStore();
        store.AppendToWaitlist("L1", Joined);

        Assert.Throws<InvalidOperationException>(() => store.AppendToWaitlist("L1", Joined));
    }

    [Fact]
    public void RemoveFromWaitlist_ReturnsFalseWhenNotWaiting()
    {
        var store = CreateStore();

        Assert.False(store.RemoveFromWaitlist("L1"));
    }

    [Fact]
    public void WaitlistInOrder_SnapshotIsNotChangedByLaterUpdates()
    {
        var store = CreateStore();
        store.AppendToWaitlist("L1", Joined);
        var snapshot = store.WaitlistInOrder();

        store.AppendToWaitlist("L2", Joined);

        Assert.Single(snapshot);
    }
}
