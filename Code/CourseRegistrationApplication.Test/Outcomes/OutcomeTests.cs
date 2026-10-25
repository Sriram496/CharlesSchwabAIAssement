using CourseRegistrationApplication.Outcomes;

namespace CourseRegistrationApplication.Test.Outcomes;

public class OutcomeTests
{
    [Fact]
    public void Waitlisted_WithSamePosition_AreEqual()
    {
        EnrollOutcome first = new EnrollOutcome.Waitlisted(2);
        EnrollOutcome second = new EnrollOutcome.Waitlisted(2);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Waitlisted_WithDifferentPosition_AreNotEqual()
    {
        EnrollOutcome first = new EnrollOutcome.Waitlisted(1);
        EnrollOutcome second = new EnrollOutcome.Waitlisted(2);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Dropped_CarriesPromotedLearnerId()
    {
        DropOutcome outcome = new DropOutcome.Dropped("learner-7");

        Assert.Equal("learner-7", Assert.IsType<DropOutcome.Dropped>(outcome).PromotedLearnerId);
    }

    [Fact]
    public void Dropped_WithNoPromotion_HasNullPromotedLearnerId()
    {
        DropOutcome outcome = new DropOutcome.Dropped(null);

        Assert.Null(Assert.IsType<DropOutcome.Dropped>(outcome).PromotedLearnerId);
    }

    [Fact]
    public void StatusEnrolled_IsDistinctFromEnrollEnrolled()
    {
        StatusOutcome status = new StatusOutcome.Enrolled();
        EnrollOutcome enroll = new EnrollOutcome.Enrolled();

        Assert.IsType<StatusOutcome.Enrolled>(status);
        Assert.IsType<EnrollOutcome.Enrolled>(enroll);
    }
}
