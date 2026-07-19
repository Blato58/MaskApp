using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MaskApp.Core.Features.Preflight;

public sealed class PreflightPageOption : INotifyPropertyChanged
{
    private bool isSelected;

    public PreflightPageOption(string pageId, string title)
    {
        PageId = pageId;
        Title = title;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string PageId { get; }

    public string Title { get; }

    public string DisplayName => $"{Title} Page";

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value)
            {
                return;
            }

            isSelected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectionStatusText));
        }
    }

    public string SelectionStatusText => IsSelected
        ? $"{DisplayName} is selected for Preflight."
        : $"{DisplayName} is not selected for Preflight.";

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
