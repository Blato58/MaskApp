using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Gallery;

public sealed class PageAddItemViewModel : INotifyPropertyChanged
{
    private static readonly string[] ColorOptions =
    [
        "#52E3FF",
        "#A78BFA",
        "#FF3D8B",
        "#FACC15",
        "#22C55E",
        "#FFFFFF"
    ];

    private readonly GalleryCatalogBuilder catalogBuilder;
    private readonly ITextPresetStore textPresetStore;
    private readonly IBuiltInAssetArchiveStore builtInArchiveStore;
    private readonly IGalleryLayoutStore layoutStore;
    private GalleryLayoutState layoutState = new();
    private IReadOnlyList<GalleryItem> allItems = [];
    private GalleryPageLayout selectedPage = new();
    private IReadOnlyList<PageAddItemCandidateCard> availableItems = [];
    private IReadOnlyList<PageAddItemPackCard> iconPacks = [];
    private IReadOnlyList<PageAddItemIconCard> icons = [];
    private IReadOnlyList<PageAddItemColorCard> colors = [];
    private string pageId = string.Empty;
    private string searchText = string.Empty;
    private string selectedIconPack = "Mask";
    private GalleryItem? selectedItem;
    private string draftLabel = string.Empty;
    private string selectedIconKey = "txt";
    private string selectedColorHex = "#52E3FF";
    private string statusText = "Choose a Gallery item.";

    public PageAddItemViewModel(
        QuickActionCatalog quickActionCatalog,
        ITextPresetStore textPresetStore,
        IBuiltInAssetArchiveStore builtInArchiveStore,
        IGalleryLayoutStore layoutStore)
    {
        catalogBuilder = new GalleryCatalogBuilder(quickActionCatalog);
        this.textPresetStore = textPresetStore;
        this.builtInArchiveStore = builtInArchiveStore;
        this.layoutStore = layoutStore;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<PageAddItemCandidateCard> AvailableItems
    {
        get => availableItems;
        private set => SetField(ref availableItems, value);
    }

    public IReadOnlyList<PageAddItemPackCard> IconPacks
    {
        get => iconPacks;
        private set => SetField(ref iconPacks, value);
    }

    public IReadOnlyList<PageAddItemIconCard> Icons
    {
        get => icons;
        private set => SetField(ref icons, value);
    }

    public IReadOnlyList<PageAddItemColorCard> Colors
    {
        get => colors;
        private set => SetField(ref colors, value);
    }

    public string PageTitle => selectedPage.Title;

    public string SearchText
    {
        get => searchText;
        set
        {
            if (SetField(ref searchText, value ?? string.Empty))
            {
                RebuildAvailableItems();
            }
        }
    }

    public string SelectedIconPack
    {
        get => selectedIconPack;
        private set
        {
            if (SetField(ref selectedIconPack, value))
            {
                RebuildIconPacks();
                RebuildIcons();
            }
        }
    }

    public GalleryItem? SelectedItem
    {
        get => selectedItem;
        private set
        {
            if (SetField(ref selectedItem, value))
            {
                OnPropertyChanged(nameof(SelectedItemTitle));
                OnPropertyChanged(nameof(SelectedItemSubtitle));
                OnPropertyChanged(nameof(HasSelectedItem));
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(SaveSummaryText));
            }
        }
    }

    public string SelectedItemTitle => SelectedItem?.Title ?? "No item selected";

    public string SelectedItemSubtitle => SelectedItem is null
        ? "Choose one Library item for this page shortcut."
        : $"{SelectedItem.TypeLabel} / {SelectedItem.GroupName}";

    public bool HasSelectedItem => SelectedItem is not null;

    public string DraftLabel
    {
        get => draftLabel;
        set
        {
            if (SetField(ref draftLabel, value ?? string.Empty))
            {
                OnPropertyChanged(nameof(PreviewLabel));
                OnPropertyChanged(nameof(SaveSummaryText));
            }
        }
    }

    public string SelectedIconKey
    {
        get => selectedIconKey;
        private set
        {
            if (SetField(ref selectedIconKey, value))
            {
                OnPropertyChanged(nameof(PreviewIconLabel));
                OnPropertyChanged(nameof(PreviewIconAsset));
                RebuildIcons();
            }
        }
    }

    public string SelectedColorHex
    {
        get => selectedColorHex;
        private set
        {
            if (SetField(ref selectedColorHex, value))
            {
                RebuildColors();
            }
        }
    }

    public string PreviewLabel => string.IsNullOrWhiteSpace(DraftLabel)
        ? SelectedItem?.Title ?? "Shortcut"
        : DraftLabel.Trim();

    public string PreviewIconLabel =>
        GalleryIconOption.Defaults.FirstOrDefault(icon => icon.IconKey == SelectedIconKey)?.Label ?? "ITEM";

    public string PreviewIconAsset =>
        GalleryIconOption.Defaults.FirstOrDefault(icon => icon.IconKey == SelectedIconKey)?.PreviewAsset ?? string.Empty;

    public bool CanSave => SelectedItem is not null;

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public string SaveSummaryText => CanSave
        ? "Ready to add shortcut."
        : "Choose a Gallery item before saving.";

    public async Task InitializeAsync(string requestedPageId, CancellationToken cancellationToken = default)
    {
        pageId = requestedPageId ?? string.Empty;
        layoutState = (await layoutStore.LoadAsync(cancellationToken)).Normalize();
        var textState = await textPresetStore.LoadAsync(cancellationToken);
        var archive = await builtInArchiveStore.LoadAsync(cancellationToken);
        allItems = catalogBuilder.Build(textState, archive, layoutState.Order);
        selectedPage = layoutState.Pages.FirstOrDefault(page => page.PageId == pageId)
            ?? layoutState.Pages.First();
        pageId = selectedPage.PageId;
        OnPropertyChanged(nameof(PageTitle));
        RebuildAll();
    }

    public void SelectItem(string galleryItemId)
    {
        var item = allItems.FirstOrDefault(candidate => candidate.Id == galleryItemId);
        if (item is null || selectedPage.Items.Any(existing => existing.GalleryItemId == galleryItemId))
        {
            StatusText = "Choose an item that is not already on this page.";
            return;
        }

        SelectedItem = item;
        DraftLabel = item.Title;
        SelectedColorHex = item.ColorHex;
        SelectedIconKey = item.IconKey;
        var icon = GalleryIconOption.Defaults.FirstOrDefault(option => option.IconKey == item.IconKey)
            ?? GalleryIconOption.Defaults[0];
        SelectedIconPack = icon.Pack;
        StatusText = "Customize icon, text, and color.";
        RebuildAvailableItems();
    }

    public void SelectIconPack(string pack)
    {
        if (GalleryIconOption.Packs.Contains(pack, StringComparer.Ordinal))
        {
            SelectedIconPack = pack;
        }
    }

    public void SelectIcon(string iconKey)
    {
        var icon = GalleryIconOption.Defaults.FirstOrDefault(candidate => candidate.IconKey == iconKey);
        if (icon is null)
        {
            return;
        }

        SelectedIconKey = icon.IconKey;
        SelectedIconPack = icon.Pack;
    }

    public void SelectColor(string colorHex)
    {
        if (ColorOptions.Contains(colorHex, StringComparer.OrdinalIgnoreCase))
        {
            SelectedColorHex = colorHex;
        }
    }

    public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedItem is null)
        {
            StatusText = "Choose a Gallery item before saving.";
            return false;
        }

        if (selectedPage.Items.Any(item => item.GalleryItemId == SelectedItem.Id))
        {
            StatusText = "This page already has that Library item.";
            return false;
        }

        var orderedItems = selectedPage.Items.OrderBy(item => item.SortIndex).ToArray();
        var layoutItem = new GalleryPageItemLayout
        {
            GalleryItemId = SelectedItem.Id,
            Label = PreviewLabel,
            IconKey = SelectedIconKey,
            ColorHex = SelectedColorHex,
            SortIndex = orderedItems.Length == 0 ? 0 : orderedItems.Max(item => item.SortIndex) + 1
        };
        var updatedPage = selectedPage with { Items = selectedPage.Items.Append(layoutItem).ToArray() };
        layoutState = layoutState with
        {
            Pages = layoutState.Pages
                .Select(page => page.PageId == pageId ? updatedPage : page)
                .ToArray()
        };
        await layoutStore.SaveAsync(layoutState, cancellationToken);
        selectedPage = updatedPage.Normalize(selectedPage.SortIndex);
        StatusText = "Shortcut added.";
        RebuildAll();
        return true;
    }

    private void RebuildAll()
    {
        RebuildAvailableItems();
        RebuildIconPacks();
        RebuildIcons();
        RebuildColors();
        OnPropertyChanged(nameof(SelectedItemTitle));
        OnPropertyChanged(nameof(SelectedItemSubtitle));
        OnPropertyChanged(nameof(PreviewLabel));
        OnPropertyChanged(nameof(PreviewIconLabel));
        OnPropertyChanged(nameof(PreviewIconAsset));
        OnPropertyChanged(nameof(SelectedColorHex));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(SaveSummaryText));
    }

    private void RebuildAvailableItems()
    {
        var usedIds = selectedPage.Items.Select(item => item.GalleryItemId).ToHashSet(StringComparer.Ordinal);
        var query = SearchText.Trim().ToUpperInvariant();
        AvailableItems = allItems
            .Where(item => item.CanSend && !usedIds.Contains(item.Id))
            .Where(item => query.Length == 0 || item.SearchText.Contains(query, StringComparison.Ordinal))
            .OrderByDescending(item => item.IsFavorite)
            .ThenBy(item => item.GroupName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .Select(item => new PageAddItemCandidateCard(
                item,
                item.Id == SelectedItem?.Id,
                new AsyncRelayCommand(_ =>
                {
                    SelectItem(item.Id);
                    return Task.CompletedTask;
                })))
            .ToArray();
    }

    private void RebuildIconPacks()
    {
        IconPacks = GalleryIconOption.Packs
            .Select(pack => new PageAddItemPackCard(
                pack,
                string.Equals(pack, SelectedIconPack, StringComparison.Ordinal),
                new AsyncRelayCommand(_ =>
                {
                    SelectIconPack(pack);
                    return Task.CompletedTask;
                })))
            .ToArray();
    }

    private void RebuildIcons()
    {
        Icons = GalleryIconOption.Defaults
            .Where(icon => string.Equals(icon.Pack, SelectedIconPack, StringComparison.Ordinal))
            .Select(icon => new PageAddItemIconCard(
                icon,
                icon.IconKey == SelectedIconKey,
                new AsyncRelayCommand(_ =>
                {
                    SelectIcon(icon.IconKey);
                    return Task.CompletedTask;
                })))
            .ToArray();
    }

    private void RebuildColors()
    {
        Colors = ColorOptions
            .Select(color => new PageAddItemColorCard(
                color,
                string.Equals(color, SelectedColorHex, StringComparison.OrdinalIgnoreCase),
                new AsyncRelayCommand(_ =>
                {
                    SelectColor(color);
                    return Task.CompletedTask;
                })))
            .ToArray();
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
