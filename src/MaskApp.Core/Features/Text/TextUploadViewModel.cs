using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.Text;

public sealed class TextUploadViewModel : INotifyPropertyChanged
{
    public const int MaxTextLength = 64;

    private const int PreviewColumns = 48;
    private const int PreviewRows = 16;
    private const string PreviewOffColor = "#111827";
    private const int MaxDiagnosticHexLength = 1024;
    private static readonly TimeSpan PreviewDebounceDelay = TimeSpan.FromMilliseconds(180);

    private readonly ITextUploadTransport transport;
    private readonly IQuickActionTextSettingsStore quickCaptionSettingsStore;
    private readonly ITextPresetStore textPresetStore;
    private readonly SynchronizationContext? synchronizationContext;
    private string text = "HELLO";
    private string presetName = "HELLO";
    private TextColorOption selectedColor;
    private TextLayoutModeOption selectedLayoutMode;
    private TextAnimationModeOption selectedAnimationMode;
    private TextPresetCategory selectedPresetCategory = TextPresetCategory.Custom;
    private TextPresetSendProfile selectedPresetSendProfile = TextPresetSendProfile.LowStaticFlash;
    private int speed = 50;
    private string statusText;
    private string presetStatusText = "Preset library ready.";
    private string lastPayloadHex = "None";
    private string lastCommandText = "None";
    private IReadOnlyList<TextPreviewCell> previewCells = [];
    private IReadOnlyList<TextPreset> savedPresets = [];
    private TextPreset? selectedSavedPreset;
    private TextPresetId? editingPresetId;
    private int columnCount;
    private int frameCount;
    private bool isSending;
    private bool isFavorite;
    private bool showInReact = true;
    private bool showInRave;
    private bool showInControl;
    private bool useBlackBackgroundReset = true;
    private bool useCompatibilityWriteOnly;
    private bool supportsAcknowledgements;
    private TextUploadTransportState transportState;
    private bool globalColorInitialized;
    private bool selectedColorManuallyChanged;
    private CancellationTokenSource? previewRefreshCancellation;

    public TextUploadViewModel(
        ITextUploadTransport transport,
        IQuickActionTextSettingsStore? quickCaptionSettingsStore = null,
        ITextPresetStore? textPresetStore = null)
    {
        this.transport = transport;
        this.quickCaptionSettingsStore = quickCaptionSettingsStore ?? new InMemoryQuickActionTextSettingsStore();
        this.textPresetStore = textPresetStore ?? new InMemoryTextPresetStore();
        synchronizationContext = SynchronizationContext.Current;
        TextColorOptions =
        [
            new TextColorOption("Cyan", 0x52, 0xE3, 0xFF, "#52E3FF"),
            new TextColorOption("White", 0xFF, 0xFF, 0xFF, "#FFFFFF"),
            new TextColorOption("Pink", 0xF4, 0x72, 0xB6, "#F472B6"),
            new TextColorOption("Amber", 0xFA, 0xCC, 0x15, "#FACC15"),
            new TextColorOption("Green", 0x22, 0xC5, 0x5E, "#22C55E"),
            new TextColorOption("Red", 0xEF, 0x44, 0x44, "#EF4444"),
            new TextColorOption("Purple", 0xA8, 0x55, 0xF7, "#A855F7")
        ];
        AnimationModes =
        [
            new TextAnimationModeOption("Off", 1),
            new TextAnimationModeOption("Blink", 2),
            new TextAnimationModeOption("Scroll right-to-left", 3),
            new TextAnimationModeOption("Scroll left-to-right", 4)
        ];
        LayoutModes =
        [
            new TextLayoutModeOption("Scroll / variable width", TextLayoutMode.VariableWidth),
            new TextLayoutModeOption("Centered 44-column", TextLayoutMode.FixedWidthCentered)
        ];
        PresetCategoryOptions =
        [
            TextPresetCategory.Custom,
            TextPresetCategory.CzechBasic,
            TextPresetCategory.CzechMeme,
            TextPresetCategory.CzechPoliticalSatire,
            TextPresetCategory.CzechRave
        ];
        PresetSendProfileOptions =
        [
            TextPresetSendProfile.LowStaticFlash,
            TextPresetSendProfile.StableFlash,
            TextPresetSendProfile.ComposerScroll
        ];

        selectedColor = TextColorOptions.Single(option => option.Name == "White");
        selectedLayoutMode = LayoutModes[1];
        selectedAnimationMode = AnimationModes[1];
        supportsAcknowledgements = transport.SupportsAcknowledgements;
        transportState = transport.State;
        useCompatibilityWriteOnly = ShouldDefaultToCompatibilityMode(transport.State, transport.SupportsAcknowledgements);
        statusText = transport.StatusText;
        SendCommand = new AsyncRelayCommand(SendAsync, CanSend);
        SaveAsPresetCommand = new AsyncRelayCommand(SaveAsPresetAsync, CanSavePreset);
        SaveChangesCommand = new AsyncRelayCommand(SaveChangesAsync, () => editingPresetId.HasValue && CanSavePreset());
        DuplicatePresetCommand = new AsyncRelayCommand(DuplicatePresetAsync, () => SelectedSavedPreset is not null);
        DeletePresetCommand = new AsyncRelayCommand(DeletePresetAsync, () => SelectedSavedPreset is not null);
        LoadPresetCommand = new AsyncRelayCommand(LoadSelectedPresetAsync, () => SelectedSavedPreset is not null);
        SaveAndSendCommand = new AsyncRelayCommand(SaveAndSendAsync, CanSavePreset);
        transport.StateChanged += OnTransportStateChanged;
        RefreshPreview();
        RefreshNormalizedTextProperties();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<TextColorOption> TextColorOptions { get; }

    public IReadOnlyList<TextLayoutModeOption> LayoutModes { get; }

    public IReadOnlyList<TextAnimationModeOption> AnimationModes { get; }

    public IReadOnlyList<TextPresetCategory> PresetCategoryOptions { get; }

    public IReadOnlyList<TextPresetSendProfile> PresetSendProfileOptions { get; }

    public AsyncRelayCommand SaveAsPresetCommand { get; }

    public AsyncRelayCommand SaveChangesCommand { get; }

    public AsyncRelayCommand DuplicatePresetCommand { get; }

    public AsyncRelayCommand DeletePresetCommand { get; }

    public AsyncRelayCommand LoadPresetCommand { get; }

    public AsyncRelayCommand SaveAndSendCommand { get; }

    public IReadOnlyList<TextPreviewCell> PreviewCells
    {
        get => previewCells;
        private set => SetField(ref previewCells, value);
    }

    public AsyncRelayCommand SendCommand { get; }

    public string PresetName
    {
        get => presetName;
        set
        {
            if (SetField(ref presetName, value ?? string.Empty))
            {
                RaisePresetCommandStates();
            }
        }
    }

    public string Text
    {
        get => text;
        set
        {
            var incomingValue = value ?? string.Empty;
            var clampedValue = incomingValue.Length > MaxTextLength
                ? incomingValue[..MaxTextLength]
                : incomingValue;

            if (SetField(ref text, clampedValue))
            {
                if (string.IsNullOrWhiteSpace(PresetName) || PresetName == MaskSafeText || PresetName == "HELLO")
                {
                    presetName = CzechTextNormalizer.Normalize(clampedValue).MaskText;
                    OnPropertyChanged(nameof(PresetName));
                }

                SchedulePreviewRefresh();
                RefreshNormalizedTextProperties();
                OnPropertyChanged(nameof(CharacterCountText));
                SendCommand.RaiseCanExecuteChanged();
                RaisePresetCommandStates();
            }
        }
    }

    public string CharacterCountText => $"{Text.Length}/{MaxTextLength}";

    public string MaskSafeText { get; private set; } = "HELLO";

    public bool HasMaskSafeDifference { get; private set; }

    public string MaskSafeTextWarning => HasMaskSafeDifference
        ? $"Mask-safe: {MaskSafeText}"
        : "Mask-safe text matches.";

    public TextColorOption SelectedColor
    {
        get => selectedColor;
        set
        {
            if (value is not null && SetField(ref selectedColor, value))
            {
                SchedulePreviewRefresh();
                OnPropertyChanged(nameof(ProfileSummary));
                OnPropertyChanged(nameof(SelectedStyleSummary));
            }
        }
    }

    public TextLayoutModeOption SelectedLayoutMode
    {
        get => selectedLayoutMode;
        set
        {
            if (value is not null && SetField(ref selectedLayoutMode, value))
            {
                SchedulePreviewRefresh();
                OnPropertyChanged(nameof(ProfileSummary));
                OnPropertyChanged(nameof(SelectedStyleSummary));
            }
        }
    }

    public TextAnimationModeOption SelectedAnimationMode
    {
        get => selectedAnimationMode;
        set
        {
            if (value is not null && SetField(ref selectedAnimationMode, value))
            {
                OnPropertyChanged(nameof(ProfileSummary));
                OnPropertyChanged(nameof(SelectedStyleSummary));
            }
        }
    }

    public TextPresetCategory SelectedPresetCategory
    {
        get => selectedPresetCategory;
        set
        {
            if (SetField(ref selectedPresetCategory, value))
            {
                ShowInRave = value == TextPresetCategory.CzechRave;
            }
        }
    }

    public TextPresetSendProfile SelectedPresetSendProfile
    {
        get => selectedPresetSendProfile;
        set
        {
            if (SetField(ref selectedPresetSendProfile, value))
            {
                OnPropertyChanged(nameof(SelectedStyleSummary));
            }
        }
    }

    public int Speed
    {
        get => speed;
        set
        {
            if (SetField(ref speed, Math.Clamp(value, 1, 100)))
            {
                OnPropertyChanged(nameof(ProfileSummary));
                OnPropertyChanged(nameof(SelectedStyleSummary));
            }
        }
    }

    public bool IsFavorite
    {
        get => isFavorite;
        set
        {
            if (SetField(ref isFavorite, value) && value)
            {
                ShowInControl = true;
            }
        }
    }

    public bool ShowInReact
    {
        get => showInReact;
        set => SetField(ref showInReact, value);
    }

    public bool ShowInRave
    {
        get => showInRave;
        set => SetField(ref showInRave, value);
    }

    public bool ShowInControl
    {
        get => showInControl;
        set => SetField(ref showInControl, value);
    }

    public bool UseBlackBackgroundReset
    {
        get => useBlackBackgroundReset;
        set
        {
            if (SetField(ref useBlackBackgroundReset, value))
            {
                OnPropertyChanged(nameof(SelectedStyleSummary));
            }
        }
    }

    public string ProfileSummary => BuildComposerProfile().Name == TextSendProfile.ComposerCentered.Name
        ? $"Centered 44 columns, {SelectedAnimationMode.Name}, Speed {Speed}, {SelectedColor.Name}"
        : $"Variable width, {SelectedAnimationMode.Name}, Speed {Speed}, {SelectedColor.Name}";

    public string SelectedStyleSummary =>
        $"{SelectedPresetSendProfile}, {SelectedLayoutMode.Name}, {SelectedAnimationMode.Name}, Speed {Speed}, {SelectedColor.Name}";

    public string PresetStatusText
    {
        get => presetStatusText;
        private set => SetField(ref presetStatusText, value);
    }

    public IReadOnlyList<TextPreset> SavedPresets
    {
        get => savedPresets;
        private set
        {
            if (SetField(ref savedPresets, value))
            {
                OnPropertyChanged(nameof(HasSavedPresets));
            }
        }
    }

    public bool HasSavedPresets => SavedPresets.Count > 0;

    public TextPreset? SelectedSavedPreset
    {
        get => selectedSavedPreset;
        set
        {
            if (SetField(ref selectedSavedPreset, value))
            {
                RaisePresetCommandStates();
            }
        }
    }

    public bool IsEditingPreset => editingPresetId.HasValue;

    public string ActiveTransportText =>
        transport.IsSimulated
            ? $"{transport.TransportDisplayName} (simulated)"
            : $"{transport.TransportDisplayName} (real)";

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

    public TextUploadTransportState TransportState
    {
        get => transportState;
        private set
        {
            if (SetField(ref transportState, value))
            {
                OnPropertyChanged(nameof(CanUseCompatibilityWriteOnly));
                OnPropertyChanged(nameof(AcknowledgementModeText));
            }
        }
    }

    public bool CanUseCompatibilityWriteOnly =>
        TransportState is TextUploadTransportState.Ready
            or TextUploadTransportState.CompatibilityReady
            or TextUploadTransportState.Simulated;

    public bool UseCompatibilityWriteOnly
    {
        get => useCompatibilityWriteOnly;
        set
        {
            if (SetField(ref useCompatibilityWriteOnly, value))
            {
                OnPropertyChanged(nameof(AcknowledgementModeText));
                SendCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string AcknowledgementModeText
    {
        get
        {
            if (UseCompatibilityWriteOnly)
            {
                return "Write-only compatibility: sends without ACK confirmation.";
            }

            return SupportsAcknowledgements
                ? "ACK required: each text step waits for mask confirmation."
                : "ACK unavailable: enable write-only compatibility to send.";
        }
    }

    public bool IsSending
    {
        get => isSending;
        private set
        {
            if (SetField(ref isSending, value))
            {
                SendCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
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

    public int ColumnCount
    {
        get => columnCount;
        private set => SetField(ref columnCount, value);
    }

    public int FrameCount
    {
        get => frameCount;
        private set => SetField(ref frameCount, value);
    }

    public void SelectColor(TextColorOption color)
    {
        selectedColorManuallyChanged = true;
        SelectedColor = color;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (globalColorInitialized || selectedColorManuallyChanged)
        {
            globalColorInitialized = true;
            await RefreshSavedPresetsAsync(cancellationToken);
            return;
        }

        var settings = (await quickCaptionSettingsStore.LoadAsync(cancellationToken)).Normalize();
        var color = FindColorOption(settings.ForegroundPreset);
        SelectedColor = color;
        globalColorInitialized = true;
        await RefreshSavedPresetsAsync(cancellationToken);
    }

    private async Task SendAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            StatusText = "Enter text before sending.";
            return;
        }

        if (!CanSend())
        {
            StatusText = BuildCannotSendStatus();
            return;
        }

        var plan = TextSendPackageFactory.Create(
            Text,
            BuildComposerProfile(),
            transport.SupportsAcknowledgements);
        var package = plan.Package;

        LastCommandText = $"{plan.Summary}; {package.StartCommand.DisplayName}; {package.SpeedCommand.DisplayName}; {package.ModeCommand.DisplayName}";
        LastPayloadHex = TruncateDiagnosticHex(Convert.ToHexString(package.Payload));
        ColumnCount = package.ColumnCount;
        FrameCount = package.Frames.Count;

        try
        {
            IsSending = true;
            StatusText = $"Sending {plan.Summary}...";

            var result = await transport.UploadAsync(package, plan.Options, cancellationToken);
            StatusText = result.Succeeded ? plan.Summary : result.Message;
            FrameCount = result.FramesSent;
        }
        catch (OperationCanceledException)
        {
            StatusText = "Text send cancelled.";
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

    private async Task SaveAsPresetAsync(CancellationToken cancellationToken)
    {
        var preset = BuildPreset(TextPresetId.NewUserPreset());
        var state = await textPresetStore.UpsertAsync(preset, cancellationToken);
        editingPresetId = preset.Id;
        OnPropertyChanged(nameof(IsEditingPreset));
        PresetStatusText = $"Saved preset {preset.DisplayName}.";
        ApplySavedState(state);
        RaisePresetCommandStates();
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        if (!editingPresetId.HasValue)
        {
            PresetStatusText = "Load a preset before saving changes.";
            return;
        }

        var preset = BuildPreset(editingPresetId.Value);
        var state = await textPresetStore.UpsertAsync(preset, cancellationToken);
        PresetStatusText = $"Saved changes to {preset.DisplayName}.";
        ApplySavedState(state);
    }

    private async Task DuplicatePresetAsync(CancellationToken cancellationToken)
    {
        if (SelectedSavedPreset is null)
        {
            return;
        }

        var duplicated = SelectedSavedPreset with
        {
            Id = TextPresetId.NewUserPreset(),
            DisplayName = $"{SelectedSavedPreset.DisplayName} Copy",
            IsSeed = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var state = await textPresetStore.UpsertAsync(duplicated, cancellationToken);
        PresetStatusText = $"Duplicated {SelectedSavedPreset.DisplayName}.";
        ApplySavedState(state);
    }

    private async Task DeletePresetAsync(CancellationToken cancellationToken)
    {
        if (SelectedSavedPreset is null)
        {
            return;
        }

        var id = SelectedSavedPreset.Id;
        var state = await textPresetStore.DeleteAsync(id, cancellationToken);
        if (editingPresetId == id)
        {
            editingPresetId = null;
            OnPropertyChanged(nameof(IsEditingPreset));
        }

        PresetStatusText = "Preset deleted.";
        ApplySavedState(state);
        RaisePresetCommandStates();
    }

    private Task LoadSelectedPresetAsync(CancellationToken cancellationToken)
    {
        if (SelectedSavedPreset is null)
        {
            return Task.CompletedTask;
        }

        LoadPreset(SelectedSavedPreset);
        return Task.CompletedTask;
    }

    private async Task SaveAndSendAsync(CancellationToken cancellationToken)
    {
        await SaveAsPresetAsync(cancellationToken);
        if (CanSend())
        {
            await SendAsync(cancellationToken);
        }
    }

    private void RefreshPreview()
    {
        try
        {
            var plan = TextSendPackageFactory.Create(
                Text,
                BuildComposerProfile(),
                transport.SupportsAcknowledgements);
            var ledData = plan.Package.LedData;
            ColumnCount = ledData.Length / 2;
            FrameCount = CalculateFrameCount(ColumnCount);

            var cells = new TextPreviewCell[PreviewColumns * PreviewRows];
            var index = 0;
            for (var row = 0; row < PreviewRows; row++)
            {
                for (var column = 0; column < PreviewColumns; column++)
                {
                    var isLit = IsLit(ledData, column, row);
                    cells[index++] = new TextPreviewCell(isLit, isLit ? SelectedColor.Hex : PreviewOffColor);
                }
            }

            PreviewCells = cells;
        }
        catch (Exception ex)
        {
            ColumnCount = 0;
            FrameCount = 0;
            PreviewCells = BuildBlankPreview();
            StatusText = $"Preview unavailable: {GetShortErrorMessage(ex)}";
        }
    }

    private void SchedulePreviewRefresh()
    {
        previewRefreshCancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        previewRefreshCancellation = cancellation;
        _ = RefreshPreviewAfterDelayAsync(cancellation);
    }

    private async Task RefreshPreviewAfterDelayAsync(CancellationTokenSource cancellation)
    {
        try
        {
            await Task.Delay(PreviewDebounceDelay, cancellation.Token).ConfigureAwait(false);
            if (cancellation.IsCancellationRequested)
            {
                return;
            }

            RunOnCapturedContext(RefreshPreview);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(previewRefreshCancellation, cancellation))
            {
                previewRefreshCancellation = null;
            }

            cancellation.Dispose();
        }
    }

    private void RunOnCapturedContext(Action action)
    {
        if (synchronizationContext is null)
        {
            action();
            return;
        }

        synchronizationContext.Post(_ => action(), null);
    }

    private static bool IsLit(byte[] ledData, int column, int row)
    {
        var byteOffset = column * 2;
        if (byteOffset + 1 >= ledData.Length)
        {
            return false;
        }

        var columnBits = (ledData[byteOffset] << 8) | ledData[byteOffset + 1];
        return (columnBits & (1 << (15 - row))) != 0;
    }

    private void OnTransportStateChanged(object? sender, TextUploadTransportStateChangedEventArgs e)
    {
        TransportState = e.State;
        SupportsAcknowledgements = e.SupportsAcknowledgements;
        StatusText = e.Message;

        if (ShouldDefaultToCompatibilityMode(e.State, e.SupportsAcknowledgements))
        {
            UseCompatibilityWriteOnly = true;
        }

        OnPropertyChanged(nameof(ActiveTransportText));
        SendCommand.RaiseCanExecuteChanged();
    }

    private bool CanSend()
    {
        if (IsSending || string.IsNullOrWhiteSpace(Text) || !transport.IsReady)
        {
            return false;
        }

        if (UseCompatibilityWriteOnly)
        {
            return CanUseCompatibilityWriteOnly;
        }

        return SupportsAcknowledgements
            && TransportState is TextUploadTransportState.Ready or TextUploadTransportState.Simulated;
    }

    private bool CanSavePreset() =>
        !string.IsNullOrWhiteSpace(Text) && !string.IsNullOrWhiteSpace(PresetName);

    private TextPreset BuildPreset(TextPresetId id)
    {
        var normalized = CzechTextNormalizer.Normalize(Text);
        var now = DateTimeOffset.UtcNow;
        return new TextPreset
        {
            Id = id,
            InputText = normalized.InputText,
            MaskText = normalized.MaskText,
            DisplayName = string.IsNullOrWhiteSpace(PresetName) ? normalized.MaskText : PresetName.Trim(),
            Category = SelectedPresetCategory,
            PackName = SelectedPresetCategory == TextPresetCategory.Custom ? "Custom" : GetPackName(SelectedPresetCategory),
            Style = new TextPresetStyle
            {
                ForegroundColor = SelectedColor.ToLedColor(),
                LayoutMode = SelectedLayoutMode.LayoutMode == TextLayoutMode.FixedWidthCentered
                    ? TextPresetLayoutMode.FixedWidthCentered
                    : TextPresetLayoutMode.VariableWidthScroll,
                DisplayMode = ToTextDisplayMode(SelectedAnimationMode.Mode),
                Speed = Speed,
                SendProfile = SelectedPresetSendProfile,
                UseBlackBackgroundReset = UseBlackBackgroundReset
            },
            IsFavorite = IsFavorite,
            Visibility = new TextPresetVisibility
            {
                ShowInReact = ShowInReact,
                ShowInRave = ShowInRave,
                ShowInControl = ShowInControl
            },
            CreatedAt = now,
            UpdatedAt = now,
            IsSeed = false
        }.Normalize(now);
    }

    private void LoadPreset(TextPreset preset)
    {
        editingPresetId = preset.Id;
        Text = preset.InputText;
        PresetName = preset.DisplayName;
        SelectedPresetCategory = preset.Category == TextPresetCategory.Legacy ? TextPresetCategory.Custom : preset.Category;
        SelectedPresetSendProfile = preset.Style.SendProfile;
        SelectedLayoutMode = preset.Style.LayoutMode == TextPresetLayoutMode.FixedWidthCentered
            ? LayoutModes.Single(option => option.LayoutMode == TextLayoutMode.FixedWidthCentered)
            : LayoutModes.Single(option => option.LayoutMode == TextLayoutMode.VariableWidth);
        SelectedAnimationMode = AnimationModes.FirstOrDefault(option => ToTextDisplayMode(option.Mode) == preset.Style.DisplayMode)
            ?? AnimationModes.Single(option => option.Mode == 2);
        Speed = preset.Style.Speed;
        SelectedColor = FindColorOption(preset.Style.ForegroundColor);
        IsFavorite = preset.IsFavorite;
        ShowInReact = preset.ShowInReact;
        ShowInRave = preset.ShowInRave;
        ShowInControl = preset.ShowInControl;
        UseBlackBackgroundReset = preset.Style.UseBlackBackgroundReset;
        PresetStatusText = $"Loaded {preset.DisplayName}.";
        OnPropertyChanged(nameof(IsEditingPreset));
        RaisePresetCommandStates();
    }

    private async Task RefreshSavedPresetsAsync(CancellationToken cancellationToken)
    {
        var state = await textPresetStore.LoadAsync(cancellationToken);
        ApplySavedState(state);
    }

    private void ApplySavedState(TextPresetStoreState state)
    {
        SavedPresets = state.Presets
            .Where(preset => preset.Category != TextPresetCategory.Legacy || preset.IsFavorite)
            .OrderBy(preset => preset.Category)
            .ThenBy(preset => preset.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        SelectedSavedPreset = SavedPresets.FirstOrDefault(preset => preset.Id == SelectedSavedPreset?.Id)
            ?? SavedPresets.FirstOrDefault(preset => preset.Id == editingPresetId)
            ?? SavedPresets.FirstOrDefault();
    }

    private void RefreshNormalizedTextProperties()
    {
        var normalized = CzechTextNormalizer.Normalize(Text);
        MaskSafeText = normalized.MaskText;
        HasMaskSafeDifference = normalized.Changed;
        OnPropertyChanged(nameof(MaskSafeText));
        OnPropertyChanged(nameof(HasMaskSafeDifference));
        OnPropertyChanged(nameof(MaskSafeTextWarning));
    }

    private void RaisePresetCommandStates()
    {
        SaveAsPresetCommand.RaiseCanExecuteChanged();
        SaveChangesCommand.RaiseCanExecuteChanged();
        DuplicatePresetCommand.RaiseCanExecuteChanged();
        DeletePresetCommand.RaiseCanExecuteChanged();
        LoadPresetCommand.RaiseCanExecuteChanged();
        SaveAndSendCommand.RaiseCanExecuteChanged();
    }

    private string BuildCannotSendStatus()
    {
        if (transport.IsReady && !SupportsAcknowledgements && !UseCompatibilityWriteOnly)
        {
            return "ACK notifications are unavailable. Enable write-only compatibility to send.";
        }

        return transport.StatusText;
    }

    private static bool ShouldDefaultToCompatibilityMode(
        TextUploadTransportState state,
        bool supportsAcknowledgements) =>
        !supportsAcknowledgements && state == TextUploadTransportState.CompatibilityReady;

    private TextSendProfile BuildComposerProfile()
    {
        var baseProfile = SelectedLayoutMode.LayoutMode == TextLayoutMode.FixedWidthCentered
            ? TextSendProfile.ComposerCentered
            : TextSendProfile.ComposerScroll;

        return baseProfile with
        {
            DisplayMode = ToTextDisplayMode(SelectedAnimationMode.Mode),
            Reliability = UseCompatibilityWriteOnly
                ? TextSendReliability.WriteOnlyCompatibility
                : TextSendReliability.ReliableAcknowledgement,
            Speed = Speed,
            TextColor = SelectedColor.ToLedColor()
        };
    }

    private TextColorOption FindColorOption(QuickCaptionForegroundPreset preset)
    {
        var color = QuickCaptionForegroundPalette.GetColor(preset);
        return TextColorOptions.FirstOrDefault(option =>
                option.Red == color.Red &&
                option.Green == color.Green &&
                option.Blue == color.Blue)
            ?? TextColorOptions.Single(option => option.Name == "White");
    }

    private TextColorOption FindColorOption(TextLedColor color) =>
        TextColorOptions.FirstOrDefault(option =>
                option.Red == color.Red &&
                option.Green == color.Green &&
                option.Blue == color.Blue)
            ?? TextColorOptions.Single(option => option.Name == "White");

    private static TextDisplayMode ToTextDisplayMode(int mode) =>
        mode switch
        {
            1 => TextDisplayMode.Off,
            2 => TextDisplayMode.Blink,
            4 => TextDisplayMode.ScrollLeftToRight,
            _ => TextDisplayMode.ScrollRightToLeft
        };

    private static string GetPackName(TextPresetCategory category) =>
        category switch
        {
            TextPresetCategory.CzechBasic => TextPresetSeedCatalog.CzechBasicPackName,
            TextPresetCategory.CzechMeme => TextPresetSeedCatalog.CzechMemePackName,
            TextPresetCategory.CzechPoliticalSatire => TextPresetSeedCatalog.CzechPoliticalPackName,
            TextPresetCategory.CzechRave => TextPresetSeedCatalog.CzechRavePackName,
            TextPresetCategory.Legacy => TextPresetSeedCatalog.LegacyPackName,
            _ => "Custom"
        };

    private static int CalculateFrameCount(int columnCount)
    {
        if (columnCount <= 0)
        {
            return 0;
        }

        var payloadLength = columnCount * 5;
        return (int)Math.Ceiling(payloadLength / (double)TextUploadProtocol.DefaultFramePayloadLength);
    }

    private static TextPreviewCell[] BuildBlankPreview()
    {
        var cells = new TextPreviewCell[PreviewColumns * PreviewRows];
        for (var i = 0; i < cells.Length; i++)
        {
            cells[i] = new TextPreviewCell(false, PreviewOffColor);
        }

        return cells;
    }

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
