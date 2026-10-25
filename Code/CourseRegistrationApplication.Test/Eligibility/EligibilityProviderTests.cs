using CourseRegistrationApplication.Eligibility;

namespace CourseRegistrationApplication.Test.Eligibility;

public class EligibilityProviderTests
{
    [Fact]
    public void AddLearner_WithPrerequisite_IsKnownAndMet()
    {
        var provider = new EligibilityProvider();

        provider.AddLearner("L1", prerequisiteMet: true);

        Assert.True(provider.IsKnownLearner("L1"));
        Assert.True(provider.HasMetPrerequisite("L1"));
    }

    [Fact]
    public void AddLearner_WithoutPrerequisite_IsKnownButNotMet()
    {
        var provider = new EligibilityProvider();

        provider.AddLearner("L1", prerequisiteMet: false);

        Assert.True(provider.IsKnownLearner("L1"));
        Assert.False(provider.HasMetPrerequisite("L1"));
    }

    [Fact]
    public void UnknownLearner_IsNeitherKnownNorMet()
    {
        var provider = new EligibilityProvider();

        Assert.False(provider.IsKnownLearner("L9"));
        Assert.False(provider.HasMetPrerequisite("L9"));
    }

    [Fact]
    public void AddLearner_WhenAlreadyKnown_Throws()
    {
        var provider = new EligibilityProvider();
        provider.AddLearner("L1", prerequisiteMet: true);

        Assert.Throws<InvalidOperationException>(() => provider.AddLearner("L1", prerequisiteMet: false));
    }

    [Fact]
    public void SetPrerequisiteMet_UpdatesStatusAndReturnsTrue()
    {
        var provider = new EligibilityProvider();
        provider.AddLearner("L1", prerequisiteMet: false);

        bool updated = provider.SetPrerequisiteMet("L1", prerequisiteMet: true);

        Assert.True(updated);
        Assert.True(provider.HasMetPrerequisite("L1"));
    }

    [Fact]
    public void SetPrerequisiteMet_UnknownLearner_ReturnsFalseAndAddsNothing()
    {
        var provider = new EligibilityProvider();

        bool updated = provider.SetPrerequisiteMet("L9", prerequisiteMet: true);

        Assert.False(updated);
        Assert.False(provider.IsKnownLearner("L9"));
    }
}
