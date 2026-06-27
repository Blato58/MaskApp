namespace MaskApp.Core.Features.Home;

public sealed class HomeViewModel
{
    public HomeViewModel()
    {
        FeatureCards =
        [
            new HomeFeatureCard(
                "Connect",
                "Ready",
                "Scan for a mask, connect over BLE, and use the current control workbench.",
                true,
                "Open"),
            new HomeFeatureCard(
                "Text",
                "MVP",
                "Compose LED text, preview deterministic mask bytes, and upload through the new text transport.",
                true,
                "Open"),
            new HomeFeatureCard(
                "Image",
                "Planned",
                "Import, crop, preview, and sync mask images after the image pipeline is mapped.",
                false,
                "Locked"),
            new HomeFeatureCard(
                "Rhythm",
                "Planned",
                "Drive effects from audio with clear permission and playback state.",
                false,
                "Locked"),
            new HomeFeatureCard(
                "Microphone",
                "Planned",
                "Use live microphone input with visible capture and permission status.",
                false,
                "Locked"),
            new HomeFeatureCard(
                "Settings",
                "Planned",
                "Group preferences, permissions, and device configuration into one operator view.",
                false,
                "Locked")
        ];
    }

    public string AppTitle => "Shining Mask";

    public string Summary => "A migration workbench for validating the mask connection and bringing features online slice by slice.";

    public IReadOnlyList<HomeFeatureCard> FeatureCards { get; }
}
