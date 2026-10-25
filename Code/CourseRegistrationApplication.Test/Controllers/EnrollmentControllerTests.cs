using CourseRegistrationApplication.Controllers;
using CourseRegistrationApplication.Eligibility;
using CourseRegistrationApplication.Outcomes;
using CourseRegistrationApplication.Services;
using CourseRegistrationApplication.Storage;
using CourseRegistrationApplication.Waitlist;

namespace CourseRegistrationApplication.Test.Controllers;

public class EnrollmentControllerTests
{
    private sealed class SpyService : ICourseOfferingService
    {
        public int CallCount { get; private set; }

        public string? LastLearnerId { get; private set; }

        public EnrollOutcome EnrollResult { get; set; } = new EnrollOutcome.Enrolled();

        public DropOutcome DropResult { get; set; } = new DropOutcome.RemovedFromWaitlist();

        public StatusOutcome StatusResult { get; set; } = new StatusOutcome.Enrolled();

        public EnrollOutcome Enroll(string learnerId)
        {
            Record(learnerId);
            return EnrollResult;
        }

        public DropOutcome Drop(string learnerId)
        {
            Record(learnerId);
            return DropResult;
        }

        public StatusOutcome GetStatus(string learnerId)
        {
            Record(learnerId);
            return StatusResult;
        }

        private void Record(string learnerId)
        {
            CallCount++;
            LastLearnerId = learnerId;
        }
    }

    public static TheoryData<EnrollOutcome, string> EnrollOutcomes => new()
    {
        { new EnrollOutcome.Enrolled(), "L1 is enrolled." },
        { new EnrollOutcome.Waitlisted(3), "L1 is on the waiting list at position 3." },
        { new EnrollOutcome.AlreadyEnrolled(), "L1 is already enrolled. No change." },
        { new EnrollOutcome.AlreadyWaitlisted(2), "L1 is already on the waiting list at position 2. No change." },
        { new EnrollOutcome.RejectedPrerequisiteNotMet(), "Rejected: L1 has not met the prerequisite." },
        { new EnrollOutcome.RejectedUnknownLearner(), "Rejected: unknown learner 'L1'." },
    };

    public static TheoryData<DropOutcome, string> DropOutcomes => new()
    {
        { new DropOutcome.Dropped(null), "L1 is dropped. No learner was promoted." },
        { new DropOutcome.Dropped("L2"), "L1 is dropped. L2 was promoted from the waiting list." },
        { new DropOutcome.RemovedFromWaitlist(), "L1 is removed from the waiting list." },
        { new DropOutcome.RejectedNotEnrolled(), "Rejected: L1 is not enrolled or on the waiting list." },
        { new DropOutcome.RejectedUnknownLearner(), "Rejected: unknown learner 'L1'." },
    };

    public static TheoryData<StatusOutcome, string> StatusOutcomes => new()
    {
        { new StatusOutcome.Enrolled(), "L1 is enrolled." },
        { new StatusOutcome.Waitlisted(4), "L1 is on the waiting list at position 4." },
        { new StatusOutcome.NotEnrolledOrWaitlisted(), "L1 is not enrolled or on the waiting list." },
        { new StatusOutcome.RejectedUnknownLearner(), "Rejected: unknown learner 'L1'." },
    };

    [Theory]
    [InlineData("enroll")]
    [InlineData("enroll   ")]
    [InlineData("drop")]
    [InlineData("drop \t")]
    [InlineData("status")]
    public void Handle_BlankLearnerId_RejectsWithoutCallingService(string commandLine)
    {
        var spy = new SpyService();
        var controller = new EnrollmentController(spy);

        string response = controller.Handle(commandLine);

        Assert.Contains("unknown learner", response);
        Assert.Equal(0, spy.CallCount);
    }

    [Fact]
    public void Handle_EnrollCommand_PassesTrimmedLearnerIdToService()
    {
        var spy = new SpyService();
        var controller = new EnrollmentController(spy);

        controller.Handle("  ENROLL   L1  ");

        Assert.Equal(1, spy.CallCount);
        Assert.Equal("L1", spy.LastLearnerId);
    }

    [Theory]
    [MemberData(nameof(EnrollOutcomes))]
    public void Handle_EnrollOutcome_ReturnsExpectedText(EnrollOutcome outcome, string expected)
    {
        var spy = new SpyService { EnrollResult = outcome };
        var controller = new EnrollmentController(spy);

        Assert.Equal(expected, controller.Handle("enroll L1"));
    }

    [Theory]
    [MemberData(nameof(DropOutcomes))]
    public void Handle_DropOutcome_ReturnsExpectedText(DropOutcome outcome, string expected)
    {
        var spy = new SpyService { DropResult = outcome };
        var controller = new EnrollmentController(spy);

        Assert.Equal(expected, controller.Handle("drop L1"));
    }

    [Theory]
    [MemberData(nameof(StatusOutcomes))]
    public void Handle_StatusOutcome_ReturnsExpectedText(StatusOutcome outcome, string expected)
    {
        var spy = new SpyService { StatusResult = outcome };
        var controller = new EnrollmentController(spy);

        Assert.Equal(expected, controller.Handle("status L1"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("register L1")]
    public void Handle_UnknownCommand_ReturnsUsageWithoutCallingService(string commandLine)
    {
        var spy = new SpyService();
        var controller = new EnrollmentController(spy);

        string response = controller.Handle(commandLine);

        Assert.StartsWith("Unknown command.", response);
        Assert.Equal(0, spy.CallCount);
    }

    [Fact]
    public void Handle_EnrollThenStatus_WithRealService_ReportsEnrolled()
    {
        var store = new InMemoryStore(new Offering("CS101", 2));
        var eligibility = new EligibilityProvider();
        eligibility.AddLearner("L1", prerequisiteMet: true);
        var waitlist = new WaitlistQueueManager(store, eligibility);
        var service = new CourseOfferingService(store, waitlist, eligibility, TimeProvider.System);
        var controller = new EnrollmentController(service);

        Assert.Equal("L1 is enrolled.", controller.Handle("enroll L1"));
        Assert.Equal("L1 is enrolled.", controller.Handle("status L1"));
    }
}
