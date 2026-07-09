using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.BuiltIns;

public sealed class BuiltInsViewModel : INotifyPropertyChanged
{
    private readonly IMaskCommandTransport transport;
    private readonly IBuiltInAssetArchiveStore archiveStore;
    private BuiltInAssetArchive archive = BuiltInAssetArchive.Empty;
    private BuiltInScannerMode mode = BuiltInScannerMode.StaticImage;
    private int currentId = BuiltInAssetCatalog.FirstId(BuiltInAssetType.StaticImage);
    private bool isSending;
    private bool isLoadingArchive;
    private string statusText = "Ready";
    private string lastCommandText = "None";
    private string displayName = BuiltInAssetCatalog.GetDefaultName(BuiltInAssetType.StaticImage, 0);
    private string tagsText = string.Empty;
    private string notes = string.Empty;
    private BuiltInAssetStatus assetStatus = BuiltInAssetStatus.Untested;
    private bool isFavorite;
    private DateTimeOffset? lastTestedAt;
    private DateTimeOffset? lastUpdatedAt;
    private string lastSendStatus = "Never sent";
    private IReadOnlyList<BuiltInAssetListItem> favoriteFaces = [];
    private IReadOnlyList<BuiltInAssetListItem> savedItems = [];
    private IReadOnlyList<BuiltInAssetListItem> catalogItems = [];
    private string searchText = string.Empty;
    private int catalogFirstVisibleIndex = -1;
    private int catalogLastVisibleIndex = -1;
    private bool catalogReduceMotion = true;

    public BuiltInsViewModel(IMaskCommandTransport transport, IBuiltInAssetArchiveStore? archiveStore = null)
    {
        this.transport = transport;
        this.archiveStore = archiveStore ?? new InMemoryBuiltInAssetArchiveStore();
        transport.TransportStateChanged += OnTransportStateChanged;

        SelectStaticImageCommand = new AsyncRelayCommand(SelectStaticImageAsync);
        SelectAnimationCommand = new AsyncRelayCommand(SelectAnimationAsync);
        PreviousCommand = new AsyncRelayCommand(PreviousAsync, CanStepPrevious);
        NextCommand = new AsyncRelayCommand(NextAsync, CanStepNext);
        SendCommand = new AsyncRelayCommand(SendAsync, CanSend);
        BlackoutCommand = new AsyncRelayCommand(BlackoutAsync, CanSend);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        ToggleFavoriteCommand = new AsyncRelayCommand(ToggleFavoriteAsync);
        MarkWorkingCommand = new AsyncRelayCommand(cancellationToken => MarkStatusAsync(BuiltInAssetStatus.Working, cancellationToken));
        MarkBadCommand = new AsyncRelayCommand(cancellationToken => MarkStatusAsync(BuiltInAssetStatus.Bad, cancellationToken));
        MarkWeirdCommand = new AsyncRelayCommand(cancellationToken => MarkStatusAsync(BuiltInAssetStatus.Weird, cancellationToken));
        LoadArchiveCommand = new AsyncRelayCommand(InitializeAsync);
        StatusValues = Enum.GetValues<BuiltInAssetStatus>();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public AsyncRelayCommand SelectStaticImageCommand { get; }

    public AsyncRelayCommand SelectAnimationCommand { get; }

    public AsyncRelayCommand PreviousCommand { get; }

    public AsyncRelayCommand NextCommand { get; }

    public AsyncRelayCommand SendCommand { get; }

    public AsyncRelayCommand BlackoutCommand { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand ToggleFavoriteCommand { get; }

    public AsyncRelayCommand MarkWorkingCommand { get; }

    public AsyncRelayCommand MarkBadCommand { get; }

    public AsyncRelayCommand MarkWeirdCommand { get; }

    public AsyncRelayCommand LoadArchiveCommand { get; }

    public IReadOnlyList<BuiltInAssetStatus> StatusValues { get; }

    public BuiltInScannerMode Mode
    {
        get => mode;
        private set
        {
            if (SetField(ref mode, value))
            {
                CurrentId = BuiltInAssetCatalog.ClampToKnownId(CurrentAssetType, CurrentId);
                LoadMetadataForCurrent();
                OnPropertyChanged(nameof(ModeText));
                OnPropertyChanged(nameof(IsStaticImageSelected));
                OnPropertyChanged(nameof(IsAnimationSelected));
                OnPropertyChanged(nameof(AvailableIds));
                OnPropertyChanged(nameof(MaxId));
                OnPropertyChanged(nameof(RangeNote));
                OnPropertyChanged(nameof(CatalogCountText));
                OnPropertyChanged(nameof(CatalogPositionText));
                OnPropertyChanged(nameof(CurrentDefinition));
                OnPropertyChanged(nameof(CurrentPreviewResourceName));
                OnPropertyChanged(nameof(CurrentPreviewIsAnimated));
                OnPropertyChanged(nameof(CurrentPreviewBadgeText));
                OnPropertyChanged(nameof(CurrentPreviewSourceText));
                OnPropertyChanged(nameof(SendButtonText));
                RefreshCatalogItems();
                RaiseCommandStates();
            }
        }
    }

    public bool IsStaticImageSelected => Mode == BuiltInScannerMode.StaticImage;

    public bool IsAnimationSelected => Mode == BuiltInScannerMode.Animation;

    public string ModeText => Mode == BuiltInScannerMode.StaticImage ? "Static Image / IMAG" : "Animation / ANIM";

    public BuiltInAssetType CurrentAssetType =>
        Mode == BuiltInScannerMode.StaticImage ? BuiltInAssetType.StaticImage : BuiltInAssetType.Animation;

    public int CurrentId
    {
        get => currentId;
        set
        {
            var clamped = BuiltInAssetCatalog.ClampToKnownId(CurrentAssetType, value);
            if (SetField(ref currentId, clamped))
            {
                LoadMetadataForCurrent();
                OnPropertyChanged(nameof(CurrentIdValue));
                OnPropertyChanged(nameof(CurrentHexId));
                OnPropertyChanged(nameof(CatalogPositionText));
                OnPropertyChanged(nameof(CurrentDefinition));
                OnPropertyChanged(nameof(CurrentPreviewResourceName));
                OnPropertyChanged(nameof(CurrentPreviewIsAnimated));
                OnPropertyChanged(nameof(CurrentPreviewBadgeText));
                OnPropertyChanged(nameof(CurrentPreviewSourceText));
                OnPropertyChanged(nameof(SendButtonText));
                RaiseCommandStates();
            }
        }
    }

    public double CurrentIdValue
    {
        get => CurrentId;
        set => CurrentId = (int)Math.Round(value);
    }

    public int MaxId => BuiltInAssetRange.GetSafeMaxId(CurrentAssetType);

    public IReadOnlyList<int> AvailableIds => BuiltInAssetCatalog.GetKnownIds(CurrentAssetType);

    public string CurrentHexId => BuiltInAssetRange.ToHexId(CurrentId);

    public BuiltInAssetDefinition CurrentDefinition =>
        BuiltInAssetCatalog.GetDefinitionOrFallback(CurrentAssetType, CurrentId);

    public string CurrentPreviewResourceName => CurrentDefinition.Preview.ResourceName;

    public bool CurrentPreviewIsAnimated => CurrentDefinition.Preview.IsAnimated;

    public string CurrentPreviewBadgeText => CurrentDefinition.Preview.BadgeText;

    public string CurrentPreviewSourceText => CurrentDefinition.Preview.Provenance;

    public string CatalogCountText => Mode == BuiltInScannerMode.StaticImage
        ? $"{BuiltInAssetCatalog.Count(BuiltInAssetType.StaticImage)} Android static images"
        : $"{BuiltInAssetCatalog.Count(BuiltInAssetType.Animation)} Android animations";

    public string CatalogPositionText =>
        BuiltInAssetCatalog.IsKnown(CurrentAssetType, CurrentId)
            ? $"{BuiltInAssetCatalog.GetPosition(CurrentAssetType, CurrentId)} of {BuiltInAssetCatalog.Count(CurrentAssetType)}"
            : "Archived unknown ID";

    public string RangeNote => Mode == BuiltInScannerMode.StaticImage
        ? "70 stock faces with exact original-app previews. IMAG command IDs run from 0 to 69."
        : "45 stock animations with exact original-app previews. ANIM command ID 4 is intentionally skipped.";

    public string ValidationLabel => "Android catalog";

    public string SuggestedSequence => "Send an ID, inspect the physical mask, then tap Favorite, Works, Bad, or Weird. Common status changes autosave.";

    public string TransportReadinessText => transport.TransportState == MaskCommandTransportState.Ready
        ? "Ready"
        : "Connect to send";

    public bool IsSending
    {
        get => isSending;
        private set
        {
            if (SetField(ref isSending, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public bool IsLoadingArchive
    {
        get => isLoadingArchive;
        private set => SetField(ref isLoadingArchive, value);
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public string LastCommandText
    {
        get => lastCommandText;
        private set => SetField(ref lastCommandText, value);
    }

    public string DisplayName
    {
        get => displayName;
        set => SetField(ref displayName, value);
    }

    public string TagsText
    {
        get => tagsText;
        set => SetField(ref tagsText, value);
    }

    public string Notes
    {
        get => notes;
        set => SetField(ref notes, value);
    }

    public BuiltInAssetStatus AssetStatus
    {
        get => assetStatus;
        set
        {
            if (SetField(ref assetStatus, value) && value == BuiltInAssetStatus.Favorite)
            {
                IsFavorite = true;
            }
        }
    }

    public bool IsFavorite
    {
        get => isFavorite;
        set
        {
            if (SetField(ref isFavorite, value))
            {
                OnPropertyChanged(nameof(FavoriteButtonText));
            }
        }
    }

    public string FavoriteButtonText => IsFavorite ? "Remove favorite" : "Add favorite";

    public string LastTestedText => lastTestedAt is null
        ? "Not tested in this archive yet."
        : $"Last tested {lastTestedAt:yyyy-MM-dd HH:mm}";

    public string LastSendStatus
    {
        get => lastSendStatus;
        private set => SetField(ref lastSendStatus, value);
    }

    public IReadOnlyList<BuiltInAssetListItem> FavoriteFaces
    {
        get => favoriteFaces;
        private set
        {
            if (SetField(ref favoriteFaces, value))
            {
                OnPropertyChanged(nameof(HasFavoriteFaces));
                OnPropertyChanged(nameof(HasNoFavoriteFaces));
                OnPropertyChanged(nameof(FavoriteFacesHintText));
            }
        }
    }

    public bool HasFavoriteFaces => FavoriteFaces.Count > 0;

    public bool HasNoFavoriteFaces => !HasFavoriteFaces;

    public string FavoriteFacesHintText => HasFavoriteFaces
        ? "Tap a face once to send its stock IMAG/ANIM command."
        : "Scan built-ins, then tap ⭐ or ✅ Works to build your deck.";

    public IReadOnlyList<BuiltInAssetListItem> SavedItems
    {
        get => savedItems;
        private set
        {
            if (SetField(ref savedItems, value))
            {
                OnPropertyChanged(nameof(HasSavedItems));
                OnPropertyChanged(nameof(ArchiveHintText));
            }
        }
    }

    public bool HasSavedItems => SavedItems.Count > 0;

    public string ArchiveHintText => HasSavedItems
        ? "Edit opens a saved ID in the scanner for names, tags, and notes."
        : "No saved archive records yet.";

    public IReadOnlyList<BuiltInAssetListItem> CatalogItems
    {
        get => catalogItems;
        private set
        {
            if (SetField(ref catalogItems, value))
            {
                OnPropertyChanged(nameof(CatalogSummaryText));
                OnPropertyChanged(nameof(IsCatalogEmpty));
            }
        }
    }

    public bool IsCatalogEmpty => CatalogItems.Count == 0;

    public string CatalogSummaryText => $"{CatalogItems.Count} {ModeText.ToLowerInvariant()} previews";

    public string SearchText
    {
        get => searchText;
        set
        {
            if (SetField(ref searchText, value))
            {
                RefreshCatalogItems();
            }
        }
    }

    public string SendButtonText => $"Send {ModeText} {CurrentId} ({CurrentHexId})";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsLoadingArchive)
        {
            return;
        }

        try
        {
            IsLoadingArchive = true;
            archive = await archiveStore.LoadAsync(cancellationToken);
            LoadMetadataForCurrent();
            RefreshItems();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            archive = BuiltInAssetArchive.Empty;
            LoadMetadataForCurrent();
            RefreshItems();
            StatusText = $"Archive unavailable; continuing with an empty archive. {ex.Message}";
        }
        finally
        {
            IsLoadingArchive = false;
        }
    }

    private Task SelectStaticImageAsync(CancellationToken cancellationToken)
    {
        Mode = BuiltInScannerMode.StaticImage;
        StatusText = "Static image scanner selected.";
        return Task.CompletedTask;
    }

    private Task SelectAnimationAsync(CancellationToken cancellationToken)
    {
        Mode = BuiltInScannerMode.Animation;
        StatusText = "Animation scanner selected.";
        return Task.CompletedTask;
    }

    private Task PreviousAsync(CancellationToken cancellationToken)
    {
        CurrentId = BuiltInAssetCatalog.GetPreviousKnownId(CurrentAssetType, CurrentId);
        return Task.CompletedTask;
    }

    private Task NextAsync(CancellationToken cancellationToken)
    {
        CurrentId = BuiltInAssetCatalog.GetNextKnownId(CurrentAssetType, CurrentId);
        return Task.CompletedTask;
    }

    private Task BlackoutAsync(CancellationToken cancellationToken) =>
        SendCommandAsync(MaskCommandBuilder.Brightness(1), cancellationToken, "BLACKOUT", updateArchive: false);

    private Task SendAsync(CancellationToken cancellationToken)
    {
        var record = BuildCurrentRecord();
        var command = BuiltInAssetCommandFactory.CreateCommand(record);
        return SendCommandAsync(command, cancellationToken, command.DisplayName, updateArchive: true);
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        lastUpdatedAt = DateTimeOffset.Now;
        var record = BuildCurrentRecord();
        archive = archive.Upsert(record);
        var saved = await SaveArchiveAsync(cancellationToken);
        RefreshItems();
        if (saved)
        {
            StatusText = "Ready";
        }
    }

    private async Task ToggleFavoriteAsync(CancellationToken cancellationToken)
    {
        var removingFavorite = IsFavorite;
        IsFavorite = !removingFavorite;
        if (removingFavorite && AssetStatus == BuiltInAssetStatus.Favorite)
        {
            AssetStatus = BuiltInAssetStatus.Untested;
        }
        lastUpdatedAt = DateTimeOffset.Now;
        var record = BuildCurrentRecord();
        archive = archive.Upsert(record);
        var saved = await SaveArchiveAsync(cancellationToken);
        RefreshItems();

        if (saved)
        {
            StatusText = "Ready";
        }
    }

    private async Task MarkStatusAsync(BuiltInAssetStatus status, CancellationToken cancellationToken)
    {
        AssetStatus = status;
        lastTestedAt = DateTimeOffset.Now;
        lastUpdatedAt = lastTestedAt;
        OnPropertyChanged(nameof(LastTestedText));

        var record = BuildCurrentRecord();
        archive = archive.Upsert(record);
        var saved = await SaveArchiveAsync(cancellationToken);
        RefreshItems();

        if (saved)
        {
            StatusText = status switch
            {
                BuiltInAssetStatus.Working => "Ready",
                BuiltInAssetStatus.Bad => "Ready",
                BuiltInAssetStatus.Weird => "Ready",
                _ => "Ready"
            };
        }
    }

    private async Task SendCommandAsync(
        MaskCommand command,
        CancellationToken cancellationToken,
        string label,
        bool updateArchive)
    {
        if (!CanSend())
        {
            StatusText = "Connect to send";
            return;
        }

        try
        {
            IsSending = true;
            LastCommandText = command.Kind == MaskCommandKind.Brightness
                ? $"{command.Kind}: {label}"
                : $"{command.Kind}: {label} ({CurrentHexId})";
            StatusText = "Needs real-mask test";
            var result = await transport.SendAsync(command, cancellationToken);
            LastSendStatus = result.Succeeded
                ? "Sent, confirm on mask"
                : "Failed";
            StatusText = LastSendStatus;

            if (updateArchive)
            {
                lastTestedAt = DateTimeOffset.Now;
                lastUpdatedAt = lastTestedAt;
                OnPropertyChanged(nameof(LastTestedText));
                archive = archive.Upsert(BuildCurrentRecord());
                await SaveArchiveAsync(cancellationToken);
                RefreshItems();
            }
        }
        finally
        {
            IsSending = false;
        }
    }

    private BuiltInAssetRecord BuildCurrentRecord() =>
        new BuiltInAssetRecord(CurrentAssetType, CurrentId)
        {
            DisplayName = DisplayName,
            Tags = ParseTags(TagsText),
            Notes = Notes,
            Status = AssetStatus,
            IsFavorite = IsFavorite,
            LastTestedAt = lastTestedAt,
            LastUpdatedAt = lastUpdatedAt,
            LastSendStatus = LastSendStatus
        }.Normalize();

    private void LoadMetadataForCurrent()
    {
        var record = archive.GetOrCreate(CurrentAssetType, CurrentId);
        DisplayName = record.DisplayName;
        TagsText = string.Join(", ", record.Tags);
        Notes = record.Notes;
        AssetStatus = record.Status;
        IsFavorite = record.IsFavorite || record.Status == BuiltInAssetStatus.Favorite;
        lastTestedAt = record.LastTestedAt;
        lastUpdatedAt = record.LastUpdatedAt;
        LastSendStatus = record.LastSendStatus;
        OnPropertyChanged(nameof(LastTestedText));
    }

    public void SelectCatalogItem(BuiltInAssetType type, int id)
    {
        if (!BuiltInAssetCatalog.IsKnown(type, id))
        {
            return;
        }

        Mode = type == BuiltInAssetType.StaticImage
            ? BuiltInScannerMode.StaticImage
            : BuiltInScannerMode.Animation;
        CurrentId = id;
        LoadMetadataForCurrent();
        StatusText = "Ready";
    }

    public void SetCatalogVisibleRange(int firstVisibleIndex, int lastVisibleIndex, bool reduceMotion)
    {
        catalogFirstVisibleIndex = firstVisibleIndex;
        catalogLastVisibleIndex = lastVisibleIndex;
        catalogReduceMotion = reduceMotion;
        ApplyCatalogAnimationState();
    }

    private void ApplyCatalogAnimationState()
    {
        for (var index = 0; index < CatalogItems.Count; index++)
        {
            var isVisible = !catalogReduceMotion &&
                index >= catalogFirstVisibleIndex &&
                index <= catalogLastVisibleIndex;
            CatalogItems[index].SetAnimationPlaying(isVisible);
        }
    }

    public void StopCatalogAnimations()
    {
        catalogFirstVisibleIndex = -1;
        catalogLastVisibleIndex = -1;
        catalogReduceMotion = true;
        foreach (var item in CatalogItems)
        {
            item.SetAnimationPlaying(false);
        }
    }

    private void RefreshItems()
    {
        FavoriteFaces = archive.FavoriteDeckRecords()
            .Select(CreateSavedItem)
            .ToArray();
        SavedItems = archive.FavoriteOrTestedRecords()
            .Select(CreateSavedItem)
            .ToArray();
        RefreshCatalogItems();
    }

    private void RefreshCatalogItems()
    {
        var query = SearchText.Trim();
        CatalogItems = BuiltInAssetCatalog.GetDefinitions(CurrentAssetType)
            .Select(definition => archive.TryGetRecord(definition.Type, definition.Id)
                ?? new BuiltInAssetRecord(definition.Type, definition.Id))
            .Where(record => MatchesSearch(record, query))
            .Select(CreateSavedItem)
            .ToArray();
        ApplyCatalogAnimationState();
    }

    private static bool MatchesSearch(BuiltInAssetRecord record, string query)
    {
        if (query.Length == 0)
        {
            return true;
        }

        var searchable = $"{record.DisplayName} {record.Id} {record.HexId} {record.Status} {string.Join(' ', record.Tags)}";
        return searchable.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private BuiltInAssetListItem CreateSavedItem(BuiltInAssetRecord record)
    {
        var preview = BuiltInAssetCatalog.GetDefinitionOrFallback(record.Type, record.Id).Preview;
        BuiltInAssetListItem? item = null;
        item = new BuiltInAssetListItem(
            record,
            record.DisplayName,
            $"{record.Status} - {record.LastSendStatus}",
            GetTypeLabel(record),
            $"ID {record.Id} / {record.HexId}",
            record.Tags.Length == 0 ? "No tags" : string.Join(", ", record.Tags),
            record.Status.ToString(),
            preview.ResourceName,
            preview.IsAnimated,
            preview.BadgeText,
            preview.Provenance,
            new AsyncRelayCommand(cancellationToken => SendSavedRecordAsync(record, cancellationToken), CanSend),
            new AsyncRelayCommand(_ =>
            {
                LoadRecord(record);
                return Task.CompletedTask;
            }),
            new AsyncRelayCommand(cancellationToken => ToggleFavoriteRecordAsync(record, cancellationToken)));
        return item;
    }

    private async Task ToggleFavoriteRecordAsync(BuiltInAssetRecord record, CancellationToken cancellationToken)
    {
        var updated = record with
        {
            IsFavorite = !(record.IsFavorite || record.Status == BuiltInAssetStatus.Favorite),
            Status = record.Status == BuiltInAssetStatus.Favorite ? BuiltInAssetStatus.Untested : record.Status,
            LastUpdatedAt = DateTimeOffset.Now
        };
        archive = archive.Upsert(updated);
        if (await SaveArchiveAsync(cancellationToken))
        {
            RefreshItems();
            if (updated.Type == CurrentAssetType && updated.Id == CurrentId)
            {
                LoadMetadataForCurrent();
            }
        }
    }

    private async Task SendSavedRecordAsync(BuiltInAssetRecord record, CancellationToken cancellationToken)
    {
        if (!CanSend())
        {
            StatusText = "Connect to send";
            return;
        }

        try
        {
            IsSending = true;
            LastCommandText = $"{GetCommandName(record)}: {record.DisplayName} ({record.HexId})";
            StatusText = "Needs real-mask test";

            var result = await transport.SendAsync(BuiltInAssetCommandFactory.CreateCommand(record), cancellationToken);
            var sendStatus = result.Succeeded
                ? "Sent, confirm on mask"
                : "Failed";
            StatusText = sendStatus;

            var updated = record with
            {
                LastTestedAt = DateTimeOffset.Now,
                LastUpdatedAt = DateTimeOffset.Now,
                LastSendStatus = sendStatus
            };
            archive = archive.Upsert(updated);
            await SaveArchiveAsync(cancellationToken);
            RefreshItems();
        }
        finally
        {
            IsSending = false;
        }
    }

    private void LoadRecord(BuiltInAssetRecord record)
    {
        Mode = record.Type == BuiltInAssetType.StaticImage
            ? BuiltInScannerMode.StaticImage
            : BuiltInScannerMode.Animation;
        LoadArchivedId(record.Id);
        LoadMetadataForCurrent();
        StatusText = "Ready";
    }

    private void LoadArchivedId(int id)
    {
        if (SetField(ref currentId, id, nameof(CurrentId)))
        {
            OnPropertyChanged(nameof(CurrentIdValue));
            OnPropertyChanged(nameof(CurrentHexId));
            OnPropertyChanged(nameof(CatalogPositionText));
            OnPropertyChanged(nameof(CurrentDefinition));
            OnPropertyChanged(nameof(CurrentPreviewResourceName));
            OnPropertyChanged(nameof(CurrentPreviewIsAnimated));
            OnPropertyChanged(nameof(CurrentPreviewBadgeText));
            OnPropertyChanged(nameof(CurrentPreviewSourceText));
            OnPropertyChanged(nameof(SendButtonText));
            RaiseCommandStates();
        }
    }

    private static string GetTypeLabel(BuiltInAssetRecord record) =>
        record.Type == BuiltInAssetType.Animation ? "Animation" : "Image";

    private static string GetCommandName(BuiltInAssetRecord record) =>
        record.Type == BuiltInAssetType.Animation ? "ANIM" : "IMAG";

    private async Task<bool> SaveArchiveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await archiveStore.SaveAsync(archive, cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            StatusText = $"Archive save failed: {ex.Message}";
            return false;
        }
    }

    private static string[] ParseTags(string value) =>
        value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(tag => tag.TrimStart('#'))
            .Where(tag => tag.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private bool CanSend() => !IsSending && transport.TransportState == MaskCommandTransportState.Ready;

    private bool CanStepPrevious() =>
        BuiltInAssetCatalog.GetPreviousKnownId(CurrentAssetType, CurrentId) != CurrentId;

    private bool CanStepNext() =>
        BuiltInAssetCatalog.GetNextKnownId(CurrentAssetType, CurrentId) != CurrentId;

    private void OnTransportStateChanged(object? sender, MaskCommandTransportStateChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TransportReadinessText));
        RaiseCommandStates();
    }

    private void RaiseCommandStates()
    {
        PreviousCommand.RaiseCanExecuteChanged();
        NextCommand.RaiseCanExecuteChanged();
        SendCommand.RaiseCanExecuteChanged();
        BlackoutCommand.RaiseCanExecuteChanged();
        SaveCommand.RaiseCanExecuteChanged();
        ToggleFavoriteCommand.RaiseCanExecuteChanged();
        MarkWorkingCommand.RaiseCanExecuteChanged();
        MarkBadCommand.RaiseCanExecuteChanged();
        MarkWeirdCommand.RaiseCanExecuteChanged();
        foreach (var item in FavoriteFaces.Concat(SavedItems).Concat(CatalogItems))
        {
            item.SendCommand.RaiseCanExecuteChanged();
        }
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
