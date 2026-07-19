using MaskApp.Core.Features.Gallery;
using MaskApp.App.Resources.Strings;

namespace MaskApp.App.Controls;

public partial class LiveTile : ContentView
{
    public static readonly BindableProperty ItemProperty = BindableProperty.Create(
        nameof(Item), typeof(GalleryPageShortcutCard), typeof(LiveTile));

    public static readonly BindableProperty RequiresHoldProperty = BindableProperty.Create(
        nameof(RequiresHold), typeof(bool), typeof(LiveTile), false, propertyChanged: OnRequiresHoldChanged);

    private CancellationTokenSource? holdCancellation;
    private bool holdCompleted;

    public LiveTile() => InitializeComponent();

    public event EventHandler? ConfirmationRequested;

    public GalleryPageShortcutCard? Item
    {
        get => (GalleryPageShortcutCard?)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public bool RequiresHold
    {
        get => (bool)GetValue(RequiresHoldProperty);
        set => SetValue(RequiresHoldProperty, value);
    }

    public string InteractionHint => AppText.Get(RequiresHold ? "HoldToSendHint" : "TapToSendHint");

    private async void OnPressed(object? sender, EventArgs e)
    {
        holdCompleted = false;
        if (!RequiresHold)
        {
            return;
        }

        holdCancellation?.Cancel();
        holdCancellation = new CancellationTokenSource();
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(700), holdCancellation.Token);
            holdCompleted = true;
            ExecuteAction();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void OnReleased(object? sender, EventArgs e)
    {
        holdCancellation?.Cancel();
        holdCancellation = null;
    }

    private void OnClicked(object? sender, EventArgs e)
    {
        if (holdCompleted)
        {
            holdCompleted = false;
            return;
        }

        if (RequiresHold)
        {
            ConfirmationRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        ExecuteAction();
    }

    public void ExecuteAction()
    {
        if (Item?.SendCommand.CanExecute(null) == true)
        {
            Item.SendCommand.Execute(null);
        }
    }

    private static void OnRequiresHoldChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((LiveTile)bindable).OnPropertyChanged(nameof(InteractionHint));
}
