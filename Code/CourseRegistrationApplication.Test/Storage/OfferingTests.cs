using CourseRegistrationApplication.Storage;

namespace CourseRegistrationApplication.Test.Storage;

public class OfferingTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    public void Constructor_WithPositiveCapacity_StoresCapacity(int capacity)
    {
        var offering = new Offering("CS101", capacity);

        Assert.Equal(capacity, offering.Capacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithCapacityOfZeroOrLess_Throws(int capacity)
    {
        // REQ-FUN-020
        Assert.Throws<ArgumentOutOfRangeException>(() => new Offering("CS101", capacity));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankOfferingId_Throws(string offeringId)
    {
        Assert.Throws<ArgumentException>(() => new Offering(offeringId, 1));
    }
}
