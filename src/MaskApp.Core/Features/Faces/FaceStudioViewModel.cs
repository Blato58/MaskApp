using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.Faces;

public sealed class FaceStudioViewModel : INotifyPropertyChanged
{
    private const string OffColor = "#05070D";
    private const int MaxDiagnosticHexLength = 1024;

    private readonly IFacePatternStore store;
    private readonly IFaceUploadTransport transport;
    private FacePattern currentPattern = FacePatternFactory.CreateBuiltIns()[0];
    private IReadOnlyList<FacePattern> patterns = [];
    private IReadOnlyList<FacePatternCard> patternCards = [];
    private IReadOnlyList<FacePreviewCell> previewCells = [];
    private FaceColorOption selectedColor;
    private string faceName = "Happy Smiley";
    private int selectedSlot = 1;
    private bool isEraseMode;
    private bool isSending;
    private string statusText;
    private string storeStatusText = "Face library ready.";
    private string lastPayloadHex = "None";
    private string lastCommandText = "None";
    private int frameCount;
    private bool useCompatibilityWriteOnly;
    private bool supportsAcknowledgements;
    private FaceUploadTransportState transportState;

    public FaceStudioViewModel(
        IFacePatternStore store,
        IFaceUploadTransport transport)
    {
        this.store = store;
        this.transport = transport;
        selectedColor = FaceColorOptions[0];
        supportsAcknowledgements = transport.SupportsAcknowledgements;
        transportState = transport.State;
        useCompatibilityWriteOnly = ShouldDefaultToCompatibilityMode(transport.State, transport.SupportsAcknowledgements);
        statusText = transport.StatusText;
        NewBlankCommand = new AsyncRelayCommand(NewBlankAsync);
        ClearCommand = new AsyncRelayCommand(_ =>
        {
            Clear();
            return Task.CompletedTask;
        });
        MirrorCommand = new AsyncRelayCommand(_ =>
        {
            Mirror();
            return Task.CompletedTask;
        });
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        SaveAsCopyCommand = new AsyncRelayCommand(SaveAsCopyAsync, CanSave);
        UploadCommand = new AsyncRelayCommand(UploadAsync, CanUpload);
        DeleteCommand = new AsyncRelayCommand(DeleteCurrentAsync, () => !CurrentPattern.IsBuiltIn);
        transport.StateChanged += OnTransportStateChanged;
        ApplyPattern(currentPattern);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<FaceColorOption> FaceColorOptions => FaceColorOption.Defaults;

    public AsyncRelayCommand NewBlankCommand { get; }

    public AsyncRelayCommand ClearCommand { get; }

    public AsyncRelayCommand MirrorCommand { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand SaveAsCopyCommand { get; }

    public AsyncRelayCommand UploadCommand { get; }

    public AsyncRelayCommand DeleteCommand { get; }

    public IReadOnlyList<FacePattern> Patterns
    {
        get => patterns;
        private set => SetField(ref patterns, value);
    }

    public IReadOnlyList<FacePatternCard> PatternCards
    {
        get => patternCards;
        private set => SetField(ref patternCards, value);
    }

    public IReadOnlyList<FacePreviewCell> PreviewCells
    {
        get => previewCells;
        private set => SetField(ref previewCells, value);
    }

    public FacePattern CurrentPattern
    {
        get => currentPattern;
        private set
        {
            if (SetField(ref currentPattern, value.Normalize()))
            {
                OnPropertyChanged(nameof(CurrentPatternSubtitle));
                OnPropertyChanged(nameof(IsEditingBuiltIn));
                OnPropertyChanged(nameof(DeleteButtonText));
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string FaceName
    {
        get => faceName;
        set
        {
            if (SetField(ref faceName, value ?? string.Empty))
            {
                SaveCommand.RaiseCanExecuteChanged();
                SaveAsCopyCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public int SelectedSlot
    {
        get => selectedSlot;
        set
        {
            if (SetField(ref selectedSlot, Math.Clamp(value, FacePattern.MinSlot, FacePattern.MaxSlot)))
            {
                OnPropertyChanged(nameof(SlotSummaryText));
                UploadCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SlotSummaryText => $"DIY slot {SelectedSlot}/20";

    public FaceColorOption SelectedColor
    {
        get => selectedColor;
        set
        {
            if (value is not null && SetField(ref selectedColor, value))
            {
                OnPropertyChanged(nameof(SelectedColorSummary));
            }
        }
    }

    public string SelectedColorSummary => $"{SelectedColor.Name} {SelectedColor.Hex}";

    public bool IsEraseMode
    {
        get => isEraseMode;
        set
        {
            if (SetField(ref isEraseMode, value))
            {
                OnPropertyChanged(nameof(DrawModeText));
            }
        }
    }

    public string DrawModeText => IsEraseMode ? "Erase" : "Paint";

    public bool IsSending
    {
        get => isSending;
        private set
        {
            if (SetField(ref isSending, value))
            {
                UploadCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public string StoreStatusText
    {
        get => storeStatusText;
        private set => SetField(ref storeStatusText, value);
    }

    public string LastPayloadHex
    {
        get => lastPayloadHex;
        private set => SetField(ref lastPayloadHex, value);
    }

    public string LastCommandText
    {
        get => lastCommandText;
        private set => SetField(ref lastCommandText, value);
    }

    public int FrameCount
    {
        get => frameCount;
        private set => SetField(ref frameCount, value);
    }

    public bool UseCompatibilityWriteOnly
    {
        get => useCompatibilityWriteOnly;
        set
        {
            if (SetField(ref useCompatibilityWriteOnly, value))
            {
                OnPropertyChanged(nameof(AcknowledgementModeText));
                UploadCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool SupportsAcknowledgements
    {
        get => supportsAcknowledgements;
        private set
        {
            if (SetField(ref supportsAcknowledgements, value))
            {
                OnPropertyChanged(nameof(AcknowledgementModeText));
            }
        }
    }

    public FaceUploadTransportState TransportState
    {
        get => transportState;
        private set
        {
            if (SetField(ref transportState, value))
            {
                OnPropertyChanged(nameof(CanUseCompatibilityWriteOnly));
                OnPropertyChanged(nameof(AcknowledgementModeText));
                UploadCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanUseCompatibilityWriteOnly =>
        TransportState is FaceUploadTransportState.Ready
            or FaceUploadTransportState.CompatibilityReady
            or FaceUploadTransportState.Simulated;

    public string AcknowledgementModeText
    {
        get
        {
            if (UseCompatibilityWriteOnly)
            {
                return "Write-only compatibility: uploads and plays without ACK confirmation.";
            }

            return SupportsAcknowledgements
                ? "ACK required: upload waits for mask confirmation."
                : "ACK unavailable: enable write-only compatibility to upload.";
        }
    }

    public string ActiveTransportText =>
        transport.IsSimulated
            ? $"{transport.TransportDisplayName} (simulated)"
            : $"{transport.TransportDisplayName} (real)";

    public bool IsEditingBuiltIn => CurrentPattern.IsBuiltIn;

    public string CurrentPatternSubtitle => $"{CurrentPattern.SourceLabel} / {SlotSummaryText}";

    public string DeleteButtonText => CurrentPattern.IsBuiltIn ? "Built-in" : "Delete Face";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var state = (await store.LoadAsync(cancellationToken).ConfigureAwait(false)).Normalize();
        StoreStatusText = state.Status;
        ApplyState(state);
        if (Patterns.Count == 0)
        {
            ApplyPattern(FacePatternFactory.CreateBlank());
            return;
        }

        var selected = Patterns.FirstOrDefault(pattern => pattern.Id == CurrentPattern.Id)
            ?? Patterns.First();
        ApplyPattern(selected);
    }

    public void SelectColor(string colorName)
    {
        var color = FaceColorOptions.FirstOrDefault(option => string.Equals(option.Name, colorName, StringComparison.OrdinalIgnoreCase));
        if (color is not null)
        {
            SelectedColor = color;
        }
    }

    public void SetCell(int column, int row)
    {
        var pixel = IsEraseMode
            ? FacePixel.Off
            : new FacePixel(true, SelectedColor.Color);
        CurrentPattern = CurrentPattern.WithPixel(column, row, pixel) with
        {
            DisplayName = FaceName,
            Source = CurrentPattern.IsBuiltIn ? FacePatternSource.Custom : CurrentPattern.Source,
            Emotion = CurrentPattern.IsBuiltIn ? FaceEmotion.Custom : CurrentPattern.Emotion,
            PreferredSlot = SelectedSlot
        };
        RefreshPreview();
    }

    public void ImportImage(FaceSampleImage image, FacePatternSource source, string displayName)
    {
        var name = string.IsNullOrWhiteSpace(displayName)
            ? source == FacePatternSource.CapturedPhoto ? "Camera Face" : "Imported Face"
            : displayName.Trim();
        var pattern = FaceImageImport.CreatePattern(image, name, source, SelectedSlot);
        ApplyPattern(pattern);
        StatusText = "Image converted to the native 46x58 mask canvas.";
    }

    private Task NewBlankAsync(CancellationToken cancellationToken)
    {
        ApplyPattern(FacePatternFactory.CreateBlank("Custom Face", GetNextCustomSlot()));
        StatusText = "Blank 46x58 mask canvas ready.";
        return Task.CompletedTask;
    }

    private void Clear()
    {
        CurrentPattern = CurrentPattern with
        {
            Pixels = Enumerable.Repeat(FacePixel.Off, FacePattern.PixelCount).ToArray(),
            Source = CurrentPattern.IsBuiltIn ? FacePatternSource.Custom : CurrentPattern.Source,
            Emotion = CurrentPattern.IsBuiltIn ? FaceEmotion.Custom : CurrentPattern.Emotion,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        RefreshPreview();
        StatusText = "Face cleared.";
    }

    private void Mirror()
    {
        var normalized = CurrentPattern.Normalize();
        var pixels = normalized.Pixels.ToArray();
        for (var row = 0; row < FacePattern.Height; row++)
        {
            for (var column = 0; column < FacePattern.Width / 2; column++)
            {
                var left = (row * FacePattern.Width) + column;
                var right = (row * FacePattern.Width) + (FacePattern.Width - column - 1);
                pixels[right] = pixels[left];
            }
        }

        CurrentPattern = normalized with
        {
            Pixels = pixels,
            Source = normalized.IsBuiltIn ? FacePatternSource.Custom : normalized.Source,
            Emotion = normalized.IsBuiltIn ? FaceEmotion.Custom : normalized.Emotion,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        RefreshPreview();
        StatusText = "Left side mirrored to right.";
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        var pattern = BuildDraftPattern(forceNewId: CurrentPattern.IsBuiltIn);
        var state = await store.UpsertAsync(pattern, cancellationToken).ConfigureAwait(false);
        ApplyState(state);
        ApplyPattern(state.Normalize().Patterns.First(item => item.Id == pattern.Id));
        StoreStatusText = CurrentPattern.IsBuiltIn ? "Saved built-in status." : $"Saved {pattern.DisplayName}.";
    }

    private async Task SaveAsCopyAsync(CancellationToken cancellationToken)
    {
        var pattern = BuildDraftPattern(forceNewId: true) with
        {
            DisplayName = FaceName.EndsWith(" Copy", StringComparison.OrdinalIgnoreCase)
                ? FaceName
                : $"{FaceName} Copy"
        };
        var state = await store.UpsertAsync(pattern, cancellationToken).ConfigureAwait(false);
        ApplyState(state);
        ApplyPattern(state.Normalize().Patterns.First(item => item.Id == pattern.Id));
        StoreStatusText = $"Saved copy {pattern.DisplayName}.";
    }

    private async Task DeleteCurrentAsync(CancellationToken cancellationToken)
    {
        if (CurrentPattern.IsBuiltIn)
        {
            StoreStatusText = "Built-in pixel faces stay in the library.";
            return;
        }

        var deletedId = CurrentPattern.Id;
        var state = await store.DeleteAsync(deletedId, cancellationToken).ConfigureAwait(false);
        ApplyState(state);
        ApplyPattern(Patterns.FirstOrDefault() ?? FacePatternFactory.CreateBlank());
        StoreStatusText = "Face deleted.";
    }

    private async Task UploadAsync(CancellationToken cancellationToken)
    {
        if (!CanUpload())
        {
            StatusText = BuildCannotUploadStatus();
            return;
        }

        var pattern = BuildDraftPattern(forceNewId: false);
        var package = FaceUploadProtocol.CreatePackage(pattern, SelectedSlot);
        var deleteCommand = FaceUploadProtocol.BuildDeleteCommand([SelectedSlot]);
        LastCommandText = $"{deleteCommand.DisplayName}; {package.StartCommand.DisplayName}; {package.FinishCommand.DisplayName}; {package.PlayCommand.DisplayName}";
        LastPayloadHex = TruncateDiagnosticHex(Convert.ToHexString(package.Payload));
        FrameCount = package.Frames.Count;

        try
        {
            IsSending = true;
            await FaceUploadOperationLock.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                StatusText = $"Uploading {pattern.DisplayName} to slot {SelectedSlot}...";
                var options = UseCompatibilityWriteOnly || !SupportsAcknowledgements
                    ? FaceUploadOptions.WriteOnlyCompatibility
                    : FaceUploadOptions.RequireAcknowledgements;
                var slotState = (await store.LoadAsync(cancellationToken).ConfigureAwait(false))
                    .ClearSlotInstallation(SelectedSlot);
                await store.SaveAsync(slotState, cancellationToken).ConfigureAwait(false);
                var result = await transport.UploadAsync(package, options, cancellationToken).ConfigureAwait(false);
                StatusText = result.Message;
                FrameCount = result.FramesSent;

                var timestamp = DateTimeOffset.UtcNow;
                var status = result.Succeeded
                    ? $"Uploaded slot {SelectedSlot}; needs real-mask visual confirmation."
                    : result.Message;
                var state = await store.UpsertAsync(pattern with
                {
                    LastUploadedAt = result.Succeeded ? timestamp : pattern.LastUploadedAt,
                    LastUploadStatus = status
                }, cancellationToken).ConfigureAwait(false);
                state = result.Succeeded
                    ? state.MarkSlotInstalled(
                        SelectedSlot,
                        FaceContentFingerprint.Compute(pattern),
                        $"face:{pattern.Id}",
                        timestamp)
                    : state.ClearSlotInstallation(SelectedSlot);
                await store.SaveAsync(state, cancellationToken).ConfigureAwait(false);
                ApplyState(state);
                ApplyPattern(state.Normalize().Patterns.First(item => item.Id == pattern.Id));
            }
            finally
            {
                FaceUploadOperationLock.Gate.Release();
            }
        }
        catch (OperationCanceledException)
        {
            StatusText = "Face upload cancelled.";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed: {GetShortErrorMessage(ex)}";
        }
        finally
        {
            IsSending = false;
        }
    }

    private FacePattern BuildDraftPattern(bool forceNewId)
    {
        var now = DateTimeOffset.UtcNow;
        var source = CurrentPattern.IsBuiltIn || forceNewId
            ? FacePatternSource.Custom
            : CurrentPattern.Source;
        var emotion = source == FacePatternSource.Custom && !CurrentPattern.IsBuiltIn
            ? CurrentPattern.Emotion
            : FaceEmotion.Custom;
        return CurrentPattern with
        {
            Id = forceNewId ? $"face-{Guid.NewGuid():N}" : CurrentPattern.Id,
            DisplayName = string.IsNullOrWhiteSpace(FaceName) ? "Custom Face" : FaceName.Trim(),
            Source = source,
            Emotion = emotion,
            PreferredSlot = SelectedSlot,
            IsFavorite = CurrentPattern.IsFavorite && !forceNewId,
            UpdatedAt = now
        };
    }

    private void ApplyState(FacePatternStoreState state)
    {
        Patterns = state.Normalize().Patterns;
        PatternCards = Patterns
            .Select(pattern => new FacePatternCard(
                pattern,
                pattern.Id == CurrentPattern.Id,
                new AsyncRelayCommand(_ =>
                {
                    ApplyPattern(pattern);
                    return Task.CompletedTask;
                }),
                new AsyncRelayCommand(cancellationToken =>
                {
                    ApplyPattern(pattern);
                    return UploadAsync(cancellationToken);
                }, CanUpload),
                new AsyncRelayCommand(cancellationToken =>
                {
                    ApplyPattern(pattern);
                    return DeleteCurrentAsync(cancellationToken);
                }, () => !pattern.IsBuiltIn)))
            .ToArray();
    }

    private void ApplyPattern(FacePattern pattern)
    {
        CurrentPattern = pattern.Normalize();
        FaceName = CurrentPattern.DisplayName;
        SelectedSlot = CurrentPattern.PreferredSlot;
        var firstLitColor = CurrentPattern.Pixels.FirstOrDefault(pixel => pixel.IsLit).Color;
        if (firstLitColor != default)
        {
            SelectedColor = FaceColorOptions.FirstOrDefault(option => option.Color == firstLitColor)
                ?? new FaceColorOption("Picked", firstLitColor);
        }

        RefreshPreview();
        ApplyState(new FacePatternStoreState { Patterns = Patterns });
        SaveCommand.RaiseCanExecuteChanged();
        SaveAsCopyCommand.RaiseCanExecuteChanged();
        UploadCommand.RaiseCanExecuteChanged();
    }

    private void RefreshPreview()
    {
        var pattern = CurrentPattern.Normalize();
        var cells = new FacePreviewCell[FacePattern.PixelCount];
        var index = 0;
        for (var row = 0; row < FacePattern.Height; row++)
        {
            for (var column = 0; column < FacePattern.Width; column++)
            {
                var pixel = pattern.GetPixel(column, row);
                cells[index++] = new FacePreviewCell(column, row, pixel.IsLit, pixel.IsLit ? pixel.Color.Hex : OffColor);
            }
        }

        PreviewCells = cells;
    }

    private int GetNextCustomSlot()
    {
        if (Patterns.Count == 0)
        {
            return 7;
        }

        var state = new FacePatternStoreState { Patterns = Patterns };
        return state.NextCustomSlot();
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(FaceName);

    private bool CanUpload()
    {
        if (IsSending || !transport.IsReady || string.IsNullOrWhiteSpace(FaceName))
        {
            return false;
        }

        if (UseCompatibilityWriteOnly)
        {
            return CanUseCompatibilityWriteOnly;
        }

        return SupportsAcknowledgements
            && TransportState is FaceUploadTransportState.Ready or FaceUploadTransportState.Simulated;
    }

    private string BuildCannotUploadStatus()
    {
        if (transport.IsReady && !SupportsAcknowledgements && !UseCompatibilityWriteOnly)
        {
            return "ACK notifications are unavailable. Enable write-only compatibility to upload.";
        }

        return transport.StatusText;
    }

    private void OnTransportStateChanged(object? sender, FaceUploadTransportStateChangedEventArgs e)
    {
        TransportState = e.State;
        SupportsAcknowledgements = e.SupportsAcknowledgements;
        StatusText = e.Message;
        if (ShouldDefaultToCompatibilityMode(e.State, e.SupportsAcknowledgements))
        {
            UseCompatibilityWriteOnly = true;
        }

        OnPropertyChanged(nameof(ActiveTransportText));
    }

    private static bool ShouldDefaultToCompatibilityMode(
        FaceUploadTransportState state,
        bool supportsAcknowledgements) =>
        !supportsAcknowledgements && state == FaceUploadTransportState.CompatibilityReady;

    private static string TruncateDiagnosticHex(string value) =>
        value.Length <= MaxDiagnosticHexLength
            ? value
            : string.Concat(value.AsSpan(0, MaxDiagnosticHexLength), "...");

    private static string GetShortErrorMessage(Exception ex)
    {
        var message = string.IsNullOrWhiteSpace(ex.Message)
            ? ex.GetType().Name
            : ex.Message;
        return message.Length <= 96 ? message : string.Concat(message.AsSpan(0, 96), "...");
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
}
