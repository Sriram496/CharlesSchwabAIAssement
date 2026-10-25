using System.Collections.Concurrent;

namespace CourseRegistrationApplication.Eligibility;

public sealed class EligibilityProvider : IEligibilityProvider
{
    private readonly ConcurrentDictionary<string, bool> _prerequisiteMet = new();

    public bool IsKnownLearner(string learnerId) => _prerequisiteMet.ContainsKey(learnerId);

    public bool HasMetPrerequisite(string learnerId) =>
        _prerequisiteMet.TryGetValue(learnerId, out bool met) && met;

    public void AddLearner(string learnerId, bool prerequisiteMet)
    {
        if (!_prerequisiteMet.TryAdd(learnerId, prerequisiteMet))
        {
            throw new InvalidOperationException($"Learner '{learnerId}' is already known.");
        }
    }

    public bool SetPrerequisiteMet(string learnerId, bool prerequisiteMet)
    {
        if (!_prerequisiteMet.ContainsKey(learnerId))
        {
            return false;
        }

        _prerequisiteMet[learnerId] = prerequisiteMet;
        return true;
    }
}
