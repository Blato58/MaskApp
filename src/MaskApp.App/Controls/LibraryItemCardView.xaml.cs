using MaskApp.Core.Features.Gallery;

namespace MaskApp.App.Controls;

public partial class LibraryItemCardView : ContentView
{
    public static readonly BindableProperty ItemProperty = BindableProperty.Create(
        nameof(Item), typeof(GalleryItemCard), typeof(LibraryItemCardView));

    public LibraryItemCardView()
    {
        InitializeComponent();
    }

    public event EventHandler? EditRequested;

    public GalleryItemCard? Item
    {
        get => (GalleryItemCard?)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    private void OnEditClicked(object? sender, EventArgs e) => EditRequested?.Invoke(this, EventArgs.Empty);
}
