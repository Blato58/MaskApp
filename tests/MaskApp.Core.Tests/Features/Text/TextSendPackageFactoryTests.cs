using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.Text;

public sealed class TextSendPackageFactoryTests
{
    [Fact]
    public void QuickFlashLowStatic_UsesCenteredBlinkWithoutResetDelay()
    {
        var plan = TextSendPackageFactory.Create("LOL", TextSendProfile.QuickFlashLowStatic);

        Assert.Equal(44, plan.Package.ColumnCount);
        Assert.True(plan.Layout.FixedWidth);
        Assert.True(plan.Layout.Centered);
        Assert.Equal((byte)2, plan.Package.ModeCommand.Plaintext.Span[5]);
        Assert.Equal((byte)50, plan.Package.SpeedCommand.Plaintext.Span[6]);
        Assert.False(plan.Options.ResetDisplayBeforeUpload);
        Assert.True(plan.Options.PreArmModeAndSpeed);
        Assert.True(plan.Options.ApplyModeBeforeSpeedAfterUpload);
        Assert.True(plan.Options.RepeatModeCommand);
        Assert.False(plan.Options.RepeatModeAndSpeed);
        Assert.True(plan.Options.PostUploadDelay < TextSendProfile.QuickFlashStable.CreateOptions(true).PostUploadDelay);
        Assert.Empty(plan.Package.StyleCommands);
        Assert.Contains("Low-static Flash", plan.Summary);
        Assert.Contains("no background reset", plan.Summary);
    }

    [Theory]
    [InlineData("LOL")]
    [InlineData("DROP")]
    [InlineData("NOPE")]
    [InlineData("WHEEL UP")]
    [InlineData("TOO MUCH BASS")]
    public void QuickFlashStable_UsesCenteredFortyFourColumnBlinkPlan(string caption)
    {
        var plan = TextSendPackageFactory.Create(caption, TextSendProfile.QuickFlashStable);

        Assert.Equal(44, plan.Package.ColumnCount);
        Assert.True(plan.Layout.FixedWidth);
        Assert.True(plan.Layout.Centered);
        Assert.Equal((byte)2, plan.Package.ModeCommand.Plaintext.Span[5]);
        Assert.Equal((byte)50, plan.Package.SpeedCommand.Plaintext.Span[6]);
        Assert.True(plan.Options.AckRequired);
        Assert.True(plan.Options.RepeatModeAndSpeed);
        Assert.Equal(TimeSpan.FromMilliseconds(200), plan.Options.PostUploadDelay);
        Assert.Contains("Stable Flash", plan.Summary);
        Assert.Contains("black background", plan.Summary);
    }

    [Fact]
    public void QuickFlashFast_UsesSafeWriteOnlyDelaysWithoutRepeat()
    {
        var plan = TextSendPackageFactory.Create("LOL", TextSendProfile.QuickFlashFast);

        Assert.False(plan.Options.AckRequired);
        Assert.True(plan.Options.CompatibilityWriteOnly);
        Assert.Equal(TimeSpan.FromMilliseconds(20), plan.Options.InterFrameDelay);
        Assert.Equal(TimeSpan.FromMilliseconds(100), plan.Options.PostUploadDelay);
        Assert.Equal(TimeSpan.FromMilliseconds(20), plan.Options.CommandDelay);
        Assert.False(plan.Options.RepeatModeAndSpeed);
        Assert.Contains("Fast Flash unstable", plan.Summary);
    }

    [Fact]
    public void QuickFlashStable_FallsBackToWriteOnlyWhenAckIsUnavailable()
    {
        var plan = TextSendPackageFactory.Create(
            "DROP",
            TextSendProfile.QuickFlashStable,
            acknowledgementsAvailable: false);

        Assert.False(plan.Options.AckRequired);
        Assert.True(plan.Options.CompatibilityWriteOnly);
        Assert.Equal(TimeSpan.FromMilliseconds(60), plan.Options.InterFrameDelay);
        Assert.True(plan.Options.RepeatModeAndSpeed);
        Assert.Contains("write-only", plan.Summary);
    }

    [Fact]
    public void ComposerScroll_AllowsVariableWidthPayload()
    {
        var plan = TextSendPackageFactory.Create("TEST SCROLL", TextSendProfile.ComposerScroll);

        Assert.True(plan.Layout.VariableWidth);
        Assert.False(plan.Layout.Centered);
        Assert.NotEqual(44, plan.Package.ColumnCount);
        Assert.Equal((byte)3, plan.Package.ModeCommand.Plaintext.Span[5]);
        Assert.Contains("Composer Scroll", plan.Summary);
    }

    [Fact]
    public void ComposerCentered_UsesFortyFourColumns()
    {
        var plan = TextSendPackageFactory.Create("TEST", TextSendProfile.ComposerCentered);

        Assert.True(plan.Layout.FixedWidth);
        Assert.Equal(44, plan.Package.ColumnCount);
        Assert.Equal((byte)2, plan.Package.ModeCommand.Plaintext.Span[5]);
        Assert.Contains("Composer Centered", plan.Summary);
    }

    [Fact]
    public void TextPlans_SendFailSoftBlackBackgroundReset()
    {
        var profile = TextSendProfile.QuickFlashStable with
        {
            BackgroundEnabled = true,
            BackgroundColor = new TextLedColor(0xA8, 0x55, 0xF7),
            StyleCommandPolicy = TextStyleCommandPolicy.FailSoft
        };

        var plan = TextSendPackageFactory.Create("DROP", profile);

        var command = Assert.Single(plan.Package.StyleCommands);
        Assert.Equal(MaskCommandKind.TextBackgroundColor, command.Kind);
        Assert.Equal(Convert.FromHexString("06424301000000000000000000000000"), command.Plaintext.ToArray());
        Assert.True(plan.Options.StyleCommandsFailSoft);
        Assert.Contains("black background", plan.Summary);
    }

    [Fact]
    public void LowStaticSequence_SendsModeBeforeAnyStyleAfterUpload()
    {
        var plan = TextSendPackageFactory.Create("DROP", TextSendProfile.QuickFlashLowStatic);

        var preUpload = TextUploadCommandSequence.CreatePreUploadSteps(plan.Package, plan.Options);
        var postUpload = TextUploadCommandSequence.CreatePostUploadSteps(plan.Package, plan.Options);

        Assert.Collection(
            preUpload,
            step => Assert.Equal(MaskCommandKind.TextSpeed, step.Command.Kind),
            step => Assert.Equal(MaskCommandKind.TextMode, step.Command.Kind));
        Assert.Collection(
            postUpload,
            step => Assert.Equal(MaskCommandKind.TextMode, step.Command.Kind),
            step => Assert.Equal(MaskCommandKind.TextSpeed, step.Command.Kind),
            step => Assert.Equal(MaskCommandKind.TextMode, step.Command.Kind));
    }

    [Fact]
    public void StableFlashSequence_KeepsBlackResetBeforeRepeatedSpeedAndMode()
    {
        var plan = TextSendPackageFactory.Create("DROP", TextSendProfile.QuickFlashStable);

        var postUpload = TextUploadCommandSequence.CreatePostUploadSteps(plan.Package, plan.Options);

        Assert.Collection(
            postUpload,
            step => Assert.Equal(MaskCommandKind.TextBackgroundColor, step.Command.Kind),
            step => Assert.Equal(MaskCommandKind.TextSpeed, step.Command.Kind),
            step => Assert.Equal(MaskCommandKind.TextMode, step.Command.Kind),
            step => Assert.Equal(MaskCommandKind.TextSpeed, step.Command.Kind),
            step => Assert.Equal(MaskCommandKind.TextMode, step.Command.Kind));
    }
}
