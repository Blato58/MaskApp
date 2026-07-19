using MaskApp.Core.Features.Gallery;

namespace MaskApp.App.Controls;

public partial class LibraryRow : ContentView
{
    public static readonly BindableProperty ItemProperty = BindableProperty.Create(
        nameof(Item), typeof(GalleryItemCard), typeof(LibraryRow));

    public LibraryRow() => InitializeComponent();

    public event EventHandler? ActionsRequested;

    public GalleryItemCard? Item
    {
        get => (GalleryItemCard?)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    private void OnActionsClicked(object? sender, EventArgs e) => ActionsRequested?.Invoke(this, EventArgs.Empty);
}
