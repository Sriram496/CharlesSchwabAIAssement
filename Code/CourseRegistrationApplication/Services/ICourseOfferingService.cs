using CourseRegistrationApplication.Outcomes;

namespace CourseRegistrationApplication.Services;

public interface ICourseOfferingService
{
    EnrollOutcome Enroll(string learnerId);

    DropOutcome Drop(string learnerId);

    StatusOutcome GetStatus(string learnerId);
}
