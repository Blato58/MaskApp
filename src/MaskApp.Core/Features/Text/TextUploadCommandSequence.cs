namespace MaskApp.Core.Features.Text;

public static class TextUploadCommandSequence
{
    public static IReadOnlyList<TextUploadCommandStep> CreatePreUploadSteps(
        TextUploadPackage package,
        TextUploadOptions options)
    {
        if (!options.PreArmModeAndSpeed)
        {
            return [];
        }

        return
        [
            new TextUploadCommandStep(package.SpeedCommand, FailSoft: false),
            new TextUploadCommandStep(package.ModeCommand, FailSoft: false)
        ];
    }

    public static IReadOnlyList<TextUploadCommandStep> CreatePostUploadSteps(
        TextUploadPackage package,
        TextUploadOptions options)
    {
        var steps = new List<TextUploadCommandStep>();

        if (options.ForceModeAndSpeed && options.ApplyModeBeforeSpeedAfterUpload)
        {
            steps.Add(new TextUploadCommandStep(package.ModeCommand, FailSoft: false));
            steps.Add(new TextUploadCommandStep(package.SpeedCommand, FailSoft: false));
            if (options.RepeatModeCommand)
            {
                steps.Add(new TextUploadCommandStep(package.ModeCommand, FailSoft: false));
            }

            AddStyleSteps(steps, package, options);
            return steps;
        }

        AddStyleSteps(steps, package, options);

        if (options.ForceModeAndSpeed)
        {
            steps.Add(new TextUploadCommandStep(package.SpeedCommand, FailSoft: false));
            steps.Add(new TextUploadCommandStep(package.ModeCommand, FailSoft: false));

            if (options.RepeatModeAndSpeed)
            {
                steps.Add(new TextUploadCommandStep(package.SpeedCommand, FailSoft: false));
                steps.Add(new TextUploadCommandStep(package.ModeCommand, FailSoft: false));
            }
            else if (options.RepeatModeCommand)
            {
                steps.Add(new TextUploadCommandStep(package.ModeCommand, FailSoft: false));
            }
        }

        return steps;
    }

    private static void AddStyleSteps(
        ICollection<TextUploadCommandStep> steps,
        TextUploadPackage package,
        TextUploadOptions options)
    {
        foreach (var styleCommand in package.StyleCommands)
        {
            steps.Add(new TextUploadCommandStep(styleCommand, options.StyleCommandsFailSoft));
        }
    }
}
