using MaskApp.Core.Features.Text;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Tests.Features.TextPresets;

public sealed class TextPresetDispatcherTests
{
    [Fact]
    public async Task SendAsync_UsesMaskTextAndPresetStyle()
    {
        var transport = new CapturingTextTransport();
        var store = new InMemoryTextPresetStore();
        var preset = new TextPreset
        {
            Id = TextPresetId.NewUserPreset(),
            InputText = "ČAU",
            DisplayName = "Czech hello",
            Style = TextPresetStyle.Default with
            {
                ForegroundColor = new TextLedColor(0x22, 0xC5, 0x5E),
                DisplayMode = TextDisplayMode.ScrollLeftToRight,
                LayoutMode = TextPresetLayoutMode.VariableWidthScroll,
                Speed = 77,
                SendProfile = TextPresetSendProfile.StableFlash,
                UseBlackBackgroundReset = true
            }
        }.Normalize();
        await store.UpsertAsync(preset);
        var dispatcher = new TextPresetDispatcher(transport, store);

        var result = await dispatcher.SendAsync(preset);

        Assert.True(result.Succeeded);
        Assert.Equal("CAU", transport.LastPackage?.Text);
        Assert.Equal((byte)4, transport.LastPackage!.ModeCommand.Plaintext.Span[5]);
        Assert.Equal((byte)77, transport.LastPackage.SpeedCommand.Plaintext.Span[6]);
        Assert.Equal(new TextLedColor(0x22, 0xC5, 0x5E), ReadFirstLitPayloadColor(transport.LastPackage));
        Assert.Contains("Transliteration used: CAU", result.Message);
        var saved = await store.LoadAsync();
        Assert.NotNull(saved.Presets.Single(item => item.Id == preset.Id).LastSentAt);
    }

    [Fact]
    public async Task SendAsync_NotReady_DoesNotUpload()
    {
        var transport = new CapturingTextTransport { IsReady = false };
        var dispatcher = new TextPresetDispatcher(transport);
        var preset = TextPresetSeedCatalog.CreateSeedPresets().First();

        var result = await dispatcher.SendAsync(preset);

        Assert.False(result.Succeeded);
        Assert.Equal("Text not ready", result.Message);
        Assert.Null(transport.LastPackage);
    }

    private sealed class CapturingTextTransport : ITextUploadTransport
    {
        public event EventHandler<TextUploadTransportStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Capture";

        public bool IsSimulated => true;

        public bool IsReady { get; init; } = true;

        public bool SupportsAcknowledgements => true;

        public TextUploadTransportState State => IsReady ? TextUploadTransportState.Ready : TextUploadTransportState.Disconnected;

        public string StatusText => IsReady ? "Ready." : "Disconnected.";

        public TextUploadPackage? LastPackage { get; private set; }

        public TextUploadOptions? LastOptions { get; private set; }

        public Task<TextUploadResult> UploadAsync(TextUploadPackage package, TextUploadOptions options, CancellationToken cancellationToken = default)
        {
            LastPackage = package;
            LastOptions = options;
            return Task.FromResult(TextUploadResult.Success("Uploaded.", package.Frames.Count));
        }
    }

    private static TextLedColor ReadFirstLitPayloadColor(TextUploadPackage package)
    {
        for (var column = 0; column < package.ColumnCount; column++)
        {
            var offset = column * 2;
            var columnBits = (package.LedData[offset] << 8) | package.LedData[offset + 1];
            if (columnBits != 0)
            {
                var colorOffset = package.LedData.Length + (column * 3);
                return new TextLedColor(
                    package.Payload[colorOffset],
                    package.Payload[colorOffset + 1],
                    package.Payload[colorOffset + 2]);
            }
        }

        throw new InvalidOperationException("No lit columns.");
    }
}
