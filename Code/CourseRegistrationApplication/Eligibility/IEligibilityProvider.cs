namespace CourseRegistrationApplication.Eligibility;

public interface IEligibilityProvider
{
    bool IsKnownLearner(string learnerId);

    bool HasMetPrerequisite(string learnerId);

    void AddLearner(string learnerId, bool prerequisiteMet);

    bool SetPrerequisiteMet(string learnerId, bool prerequisiteMet);
}
