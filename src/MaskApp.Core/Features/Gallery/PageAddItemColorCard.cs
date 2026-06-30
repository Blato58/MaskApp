using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Gallery;

public sealed class PageAddItemColorCard
{
    public PageAddItemColorCard(string colorHex, bool isSelected, AsyncRelayCommand selectCommand)
    {
        ColorHex = colorHex;
        IsSelected = isSelected;
        SelectCommand = selectCommand;
    }

    public string ColorHex { get; }

    public bool IsSelected { get; }

    public string SelectionText => IsSelected ? "Selected" : "Select";

    public AsyncRelayCommand SelectCommand { get; }
}
