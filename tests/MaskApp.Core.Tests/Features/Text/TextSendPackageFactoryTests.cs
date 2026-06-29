using MaskApp.Core.Features.MaskControl;
using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.Text;

public sealed class TextSendPackageFactoryTests
{
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
        Assert.Contains("Fast Flash", plan.Summary);
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
    public void BackgroundRequest_IsSkippedSoMaskBackgroundStaysBlack()
    {
        var profile = TextSendProfile.QuickFlashStable with
        {
            BackgroundEnabled = true,
            BackgroundColor = new TextLedColor(0xA8, 0x55, 0xF7),
            StyleCommandPolicy = TextStyleCommandPolicy.FailSoft
        };

        var plan = TextSendPackageFactory.Create("DROP", profile);

        Assert.Empty(plan.Package.StyleCommands);
        Assert.Contains("Style skipped", plan.Summary);
        Assert.Contains("background fixed black", plan.Summary);
    }
}
