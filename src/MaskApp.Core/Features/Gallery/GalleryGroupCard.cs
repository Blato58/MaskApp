using MaskApp.Core.Features.Connect;

using System.Collections;

namespace MaskApp.Core.Features.Gallery;

public sealed class GalleryGroupCard : IReadOnlyList<GalleryItemCard>
{
    public GalleryGroupCard(
        string key,
        string title,
        bool isEditMode,
        IReadOnlyList<GalleryItemCard> items,
        AsyncRelayCommand moveEarlierCommand,
        AsyncRelayCommand moveLaterCommand)
    {
        Key = key;
        Title = title;
        IsEditMode = isEditMode;
        Items = items;
        MoveEarlierCommand = moveEarlierCommand;
        MoveLaterCommand = moveLaterCommand;
    }

    public string Key { get; }

    public string Title { get; }

    public bool IsEditMode { get; }

    public IReadOnlyList<GalleryItemCard> Items { get; }

    public int Count => Items.Count;

    public GalleryItemCard this[int index] => Items[index];

    public AsyncRelayCommand MoveEarlierCommand { get; }

    public AsyncRelayCommand MoveLaterCommand { get; }

    public IEnumerator<GalleryItemCard> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
