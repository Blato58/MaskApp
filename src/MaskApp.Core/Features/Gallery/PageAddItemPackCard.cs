using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Gallery;

public sealed class PageAddItemPackCard
{
    public PageAddItemPackCard(string label, bool isSelected, AsyncRelayCommand selectCommand)
    {
        Label = label;
        IsSelected = isSelected;
        SelectCommand = selectCommand;
    }

    public string Label { get; }

    public bool IsSelected { get; }

    public AsyncRelayCommand SelectCommand { get; }
}
