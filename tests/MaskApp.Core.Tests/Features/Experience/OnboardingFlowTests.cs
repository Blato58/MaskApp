using MaskApp.Core.Features.Experience;

namespace MaskApp.Core.Tests.Features.Experience;

public sealed class OnboardingFlowTests
{
    [Fact]
    public void Skip_CompletesOfflineWithoutConnection()
    {
        var flow = new OnboardingFlow();
        flow.Next();

        flow.Skip();

        Assert.True(flow.IsComplete);
        Assert.True(flow.ContinueOffline);
    }

    [Fact]
    public void SavedStep_IsBounded()
    {
        Assert.Equal(OnboardingStep.ChooseMask, new OnboardingFlow(99).Step);
        Assert.Equal(OnboardingStep.Welcome, new OnboardingFlow(-1).Step);
    }
}
