using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaskApp.Core.Features.Connect;

namespace MaskApp.Core.Features.AnimationPacks;

public sealed class MaskPackConflictChoice : INotifyPropertyChanged
{
    private MaskPackConflictResolution resolution = MaskPackConflictResolution.Merge;

    public MaskPackConflictChoice(MaskPackConflict conflict)
    {
        Conflict = conflict;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MaskPackConflict Conflict { get; }

    public string Key => Conflict.Key;

    public string Title => $"{Conflict.Type}: {Conflict.Name}";

    public string Detail => Conflict.IsExactMatch
        ? "Exact match already exists. Merge safely keeps the local copy."
        : $"ID {Conflict.Id} already exists. Rename suggestion: {Conflict.SuggestedId}.";

    public IReadOnlyList<MaskPackConflictResolution> ResolutionOptions { get; } =
        Enum.GetValues<MaskPackConflictResolution>();

    public MaskPackConflictResolution Resolution
    {
        get => resolution;
        set
        {
            if (resolution == value)
            {
                return;
            }

            resolution = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Resolution)));
        }
    }
}

public sealed class MaskPackViewModel : INotifyPropertyChanged
{
    private readonly MaskPackArchiveService archiveService;
    private MaskPackInspection? inspection;
    private IReadOnlyList<MaskPackConflictChoice> conflicts = [];
    private string packName = "MaskApp Show";
    private string author = "MaskApp";
    private string inspectionSummary = "Choose a .maskpack.zip file to inspect it before import.";
    private string detailsText = "MaskPack v2 uses 46x58 art and verified 44x58 text geometry.";
    private string statusText = "MaskPack import and export ready.";
    private bool confirmReplace;
    private bool isBusy;

    public MaskPackViewModel(MaskPackArchiveService archiveService)
    {
        this.archiveService = archiveService;
        ImportCommand = new AsyncRelayCommand(ImportAsync, CanImport, SetCommandError);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public AsyncRelayCommand ImportCommand { get; }

    public string PackName
    {
        get => packName;
        set => SetField(ref packName, value ?? string.Empty);
    }

    public string Author
    {
        get => author;
        set => SetField(ref author, value ?? string.Empty);
    }

    public string InspectionSummary
    {
        get => inspectionSummary;
        private set => SetField(ref inspectionSummary, value);
    }

    public string DetailsText
    {
        get => detailsText;
        private set => SetField(ref detailsText, value);
    }

    public string StatusText
    {
        get => statusText;
        private set => SetField(ref statusText, value);
    }

    public IReadOnlyList<MaskPackConflictChoice> Conflicts
    {
        get => conflicts;
        private set
        {
            foreach (var conflict in conflicts)
            {
                conflict.PropertyChanged -= OnConflictChoiceChanged;
            }

            if (SetField(ref conflicts, value))
            {
                foreach (var conflict in conflicts)
                {
                    conflict.PropertyChanged += OnConflictChoiceChanged;
                }

                OnPropertyChanged(nameof(HasConflicts));
                OnPropertyChanged(nameof(RequiresReplaceConfirmation));
            }
        }
    }

    public bool HasConflicts => Conflicts.Count > 0;

    public bool HasValidInspection => inspection?.IsValid == true;

    public bool RequiresReplaceConfirmation =>
        Conflicts.Any(conflict => conflict.Resolution == MaskPackConflictResolution.Replace);

    public bool ConfirmReplace
    {
        get => confirmReplace;
        set
        {
            if (SetField(ref confirmReplace, value))
            {
                ImportCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetField(ref isBusy, value))
            {
                ImportCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var recovered = await archiveService.RecoverInterruptedImportAsync(cancellationToken);
            StatusText = recovered
                ? "Recovered all local content from an interrupted MaskPack import."
                : "MaskPack import and export ready.";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or InvalidDataException or InvalidOperationException)
        {
            StatusText = $"Recovery needs attention: {ShortMessage(exception)}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task InspectAsync(Stream source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        StatusText = "Inspecting archive without extracting files...";
        try
        {
            inspection = await archiveService.InspectAsync(source, cancellationToken);
            ApplyInspection(inspection);
        }
        finally
        {
            IsBusy = false;
            ImportCommand.RaiseCanExecuteChanged();
        }
    }

    public async Task<MaskPackExportResult> ExportAsync(
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (IsBusy)
        {
            return new MaskPackExportResult(false, "Another MaskPack operation is already running.", 0, 0);
        }

        IsBusy = true;
        StatusText = "Building a bounded MaskPack v2 archive...";
        try
        {
            var result = await archiveService.ExportAsync(
                destination,
                new MaskPackExportRequest { PackName = PackName, Author = Author },
                cancellationToken);
            StatusText = result.Message;
            return result;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                           or InvalidDataException or InvalidOperationException)
        {
            var result = new MaskPackExportResult(false, $"Export failed: {ShortMessage(exception)}", 0, 0);
            StatusText = result.Message;
            return result;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ImportAsync(CancellationToken cancellationToken)
    {
        if (inspection is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var resolutions = Conflicts.ToDictionary(
                choice => choice.Key,
                choice => choice.Resolution,
                StringComparer.Ordinal);
            var result = await archiveService.ImportAsync(
                new MaskPackImportRequest
                {
                    Inspection = inspection,
                    ConflictResolutions = resolutions,
                    ConfirmReplace = ConfirmReplace
                },
                cancellationToken);
            StatusText = result.Message;
            if (result.Succeeded)
            {
                InspectionSummary = $"Import complete · {result.ImportedCount} imported · {result.RenamedCount} renamed · {result.SkippedCount} skipped · {result.ReplacedCount} replaced";
                inspection = null;
                Conflicts = [];
                ConfirmReplace = false;
                OnPropertyChanged(nameof(HasValidInspection));
            }
        }
        finally
        {
            IsBusy = false;
            ImportCommand.RaiseCanExecuteChanged();
        }
    }

    private void ApplyInspection(MaskPackInspection value)
    {
        Conflicts = value.Conflicts.Select(conflict => new MaskPackConflictChoice(conflict)).ToArray();
        ConfirmReplace = false;
        OnPropertyChanged(nameof(HasValidInspection));

        if (!value.IsValid || value.Package is null)
        {
            InspectionSummary = "This archive is not safe to import.";
            DetailsText = value.Errors.Count == 0
                ? "The archive could not be decoded."
                : string.Join(Environment.NewLine, value.Errors.Select(error => $"ERROR · {error}"));
            StatusText = "Inspection failed. Local content was not changed.";
            return;
        }

        var package = value.Package;
        InspectionSummary = $"{package.Manifest.PackName} · v{package.Manifest.SchemaVersion} · {package.Entries.Count} item(s) · {Conflicts.Count} conflict(s)";
        var details = new List<string>
        {
            $"Author: {package.Manifest.Author}",
            $"Contents: {package.Faces.Count} faces, {package.Animations.Count} animations, {package.TextPresets.Count} text presets, {package.Pages.Count} Pages, {package.Scenes.Count} Scenes, {package.Setlists.Count} setlists",
            package.MigratedFromV1
                ? "Migration: legacy MaskPack v1 will be converted to native 46x58 art during import."
                : "Geometry: 46x58 art and 44x58 text."
        };
        details.AddRange(value.Warnings.Select(warning => $"WARNING · {warning}"));
        DetailsText = string.Join(Environment.NewLine, details);
        StatusText = Conflicts.Count == 0
            ? "Inspection passed. Ready to import."
            : "Inspection passed. Review every conflict before importing.";
    }

    private bool CanImport() => HasValidInspection
        && !IsBusy
        && (!RequiresReplaceConfirmation || ConfirmReplace);

    private void OnConflictChoiceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MaskPackConflictChoice.Resolution))
        {
            return;
        }

        if (!RequiresReplaceConfirmation)
        {
            ConfirmReplace = false;
        }

        OnPropertyChanged(nameof(RequiresReplaceConfirmation));
        ImportCommand.RaiseCanExecuteChanged();
    }

    private void SetCommandError(Exception exception) =>
        StatusText = $"MaskPack operation failed: {ShortMessage(exception)}";

    private static string ShortMessage(Exception exception)
    {
        var message = string.IsNullOrWhiteSpace(exception.Message) ? exception.GetType().Name : exception.Message;
        return message.Length <= 180 ? message : string.Concat(message.AsSpan(0, 180), "...");
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
