namespace MaskApp.Core.Features.Experience;

public enum OnboardingStep
{
    Welcome,
    BluetoothRationale,
    ChooseMask
}

public sealed class OnboardingFlow(int savedStep = 0)
{
    public OnboardingStep Step { get; private set; } = (OnboardingStep)Math.Clamp(savedStep, 0, 2);

    public bool ContinueOffline { get; private set; }

    public bool IsComplete { get; private set; }

    public void Next()
    {
        if (Step < OnboardingStep.ChooseMask)
        {
            Step++;
        }
    }

    public void Back()
    {
        if (Step > OnboardingStep.Welcome)
        {
            Step--;
        }
    }

    public void Skip()
    {
        ContinueOffline = true;
        IsComplete = true;
    }

    public void CompleteAfterConnection()
    {
        ContinueOffline = false;
        IsComplete = true;
    }
}
