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
    private int currentId = 1;
    private bool isSending;
    private bool isLoadingArchive;
    private string statusText = "Choose a built-in ID and send it to a connected mask.";
    private string lastCommandText = "None";
    private string displayName = "Image 1";
    private string tagsText = string.Empty;
    private string notes = string.Empty;
    private BuiltInAssetStatus assetStatus = BuiltInAssetStatus.Untested;
    private bool isFavorite;
    private DateTimeOffset? lastTestedAt;
    private DateTimeOffset? lastUpdatedAt;
    private string lastSendStatus = "Never sent";
    private IReadOnlyList<BuiltInAssetListItem> favoriteFaces = [];
    private IReadOnlyList<BuiltInAssetListItem> savedItems = [];

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
                CurrentId = Math.Min(CurrentId, MaxId);
                LoadMetadataForCurrent();
                OnPropertyChanged(nameof(ModeText));
                OnPropertyChanged(nameof(IsStaticImageSelected));
                OnPropertyChanged(nameof(IsAnimationSelected));
                OnPropertyChanged(nameof(MaxId));
                OnPropertyChanged(nameof(RangeNote));
                OnPropertyChanged(nameof(SendButtonText));
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
            var clamped = Math.Clamp(value, 0, MaxId);
            if (SetField(ref currentId, clamped))
            {
                LoadMetadataForCurrent();
                OnPropertyChanged(nameof(CurrentIdValue));
                OnPropertyChanged(nameof(CurrentHexId));
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

    public string CurrentHexId => BuiltInAssetRange.ToHexId(CurrentId);

    public string RangeNote => Mode == BuiltInScannerMode.StaticImage
        ? "IMAG useful range is expected up to about 0x69. Archive stores metadata only."
        : "ANIM useful range is expected up to about 0x45. Archive stores metadata only.";

    public string ValidationLabel => "Metadata only";

    public string SuggestedSequence => "Send an ID, inspect the physical mask, then tap Favorite, Works, Bad, or Weird. Common status changes autosave.";

    public string TransportReadinessText => transport.TransportState == MaskCommandTransportState.Ready
        ? $"{transport.TransportDisplayName} command transport ready."
        : $"Command transport unavailable: {transport.TransportStatusText}";

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
        set => SetField(ref isFavorite, value);
    }

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
        : "Scan built-ins, then tap Star Favorite or Works to build your deck.";

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
            archive = await archiveStore.LoadAsync(cancellationToken).ConfigureAwait(false);
            LoadMetadataForCurrent();
            RefreshSavedItems();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            archive = BuiltInAssetArchive.Empty;
            LoadMetadataForCurrent();
            RefreshSavedItems();
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
        CurrentId--;
        return SendAsync(cancellationToken);
    }

    private Task NextAsync(CancellationToken cancellationToken)
    {
        CurrentId++;
        return SendAsync(cancellationToken);
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
        var saved = await SaveArchiveAsync(cancellationToken).ConfigureAwait(false);
        RefreshSavedItems();
        if (saved)
        {
            StatusText = $"Saved {record.DisplayName} ({record.HexId}). Metadata only; no frames extracted.";
        }
    }

    private async Task ToggleFavoriteAsync(CancellationToken cancellationToken)
    {
        IsFavorite = !IsFavorite;
        lastUpdatedAt = DateTimeOffset.Now;
        var record = BuildCurrentRecord();
        archive = archive.Upsert(record);
        var saved = await SaveArchiveAsync(cancellationToken).ConfigureAwait(false);
        RefreshSavedItems();

        if (saved)
        {
            StatusText = IsFavorite
                ? $"Favorited {GetCommandName(record)} {record.Id}. Added to Favorite Faces."
                : $"Removed favorite from {GetCommandName(record)} {record.Id}.";
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
        var saved = await SaveArchiveAsync(cancellationToken).ConfigureAwait(false);
        RefreshSavedItems();

        if (saved)
        {
            StatusText = status switch
            {
                BuiltInAssetStatus.Working => $"Saved {GetCommandName(record)} {record.Id} as Working.",
                BuiltInAssetStatus.Bad => $"Marked {GetCommandName(record)} {record.Id} as Bad.",
                BuiltInAssetStatus.Weird => $"Marked {GetCommandName(record)} {record.Id} as Weird.",
                _ => $"Saved {GetCommandName(record)} {record.Id} as {status}."
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
            StatusText = transport.TransportStatusText;
            return;
        }

        try
        {
            IsSending = true;
            LastCommandText = command.Kind == MaskCommandKind.Brightness
                ? $"{command.Kind}: {label}"
                : $"{command.Kind}: {label} ({CurrentHexId})";
            StatusText = $"Sending {label}. Needs real-mask test.";
            var result = await transport.SendAsync(command, cancellationToken).ConfigureAwait(false);
            LastSendStatus = result.Succeeded
                ? $"{result.Message} Needs real-mask test."
                : result.Message;
            StatusText = LastSendStatus;

            if (updateArchive)
            {
                lastTestedAt = DateTimeOffset.Now;
                lastUpdatedAt = lastTestedAt;
                OnPropertyChanged(nameof(LastTestedText));
                archive = archive.Upsert(BuildCurrentRecord());
                await SaveArchiveAsync(cancellationToken).ConfigureAwait(false);
                RefreshSavedItems();
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

    private void RefreshSavedItems()
    {
        FavoriteFaces = archive.FavoriteDeckRecords()
            .Select(CreateSavedItem)
            .ToArray();
        SavedItems = archive.FavoriteOrTestedRecords()
            .Select(CreateSavedItem)
            .ToArray();
    }

    private BuiltInAssetListItem CreateSavedItem(BuiltInAssetRecord record)
    {
        BuiltInAssetListItem? item = null;
        item = new BuiltInAssetListItem(
            record,
            record.DisplayName,
            $"{record.Status} - {record.LastSendStatus}",
            GetTypeLabel(record),
            $"ID {record.Id} / {record.HexId}",
            record.Tags.Length == 0 ? "No tags" : string.Join(", ", record.Tags),
            record.Status.ToString(),
            record.IsFavorite || record.Status == BuiltInAssetStatus.Favorite ? "Star" : string.Empty,
            new AsyncRelayCommand(cancellationToken => SendSavedRecordAsync(record, cancellationToken), CanSend),
            new AsyncRelayCommand(_ =>
            {
                LoadRecord(record);
                return Task.CompletedTask;
            }));
        return item;
    }

    private async Task SendSavedRecordAsync(BuiltInAssetRecord record, CancellationToken cancellationToken)
    {
        if (!CanSend())
        {
            StatusText = transport.TransportStatusText;
            return;
        }

        try
        {
            IsSending = true;
            LastCommandText = $"{GetCommandName(record)}: {record.DisplayName} ({record.HexId})";
            StatusText = $"Sending {record.DisplayName}. Needs real-mask test.";

            var result = await transport.SendAsync(BuiltInAssetCommandFactory.CreateCommand(record), cancellationToken)
                .ConfigureAwait(false);
            var sendStatus = result.Succeeded
                ? $"{result.Message} Needs real-mask test."
                : result.Message;
            StatusText = sendStatus;

            var updated = record with
            {
                LastTestedAt = DateTimeOffset.Now,
                LastUpdatedAt = DateTimeOffset.Now,
                LastSendStatus = sendStatus
            };
            archive = archive.Upsert(updated);
            await SaveArchiveAsync(cancellationToken).ConfigureAwait(false);
            RefreshSavedItems();
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
        CurrentId = record.Id;
        LoadMetadataForCurrent();
        StatusText = $"Loaded {record.DisplayName}. Metadata only.";
    }

    private static string GetTypeLabel(BuiltInAssetRecord record) =>
        record.Type == BuiltInAssetType.Animation ? "Animation" : "Image";

    private static string GetCommandName(BuiltInAssetRecord record) =>
        record.Type == BuiltInAssetType.Animation ? "ANIM" : "IMAG";

    private async Task<bool> SaveArchiveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await archiveStore.SaveAsync(archive, cancellationToken).ConfigureAwait(false);
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

    private bool CanStepPrevious() => CanSend() && CurrentId > 0;

    private bool CanStepNext() => CanSend() && CurrentId < MaxId;

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
        foreach (var item in FavoriteFaces.Concat(SavedItems))
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
