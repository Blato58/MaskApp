using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using MaskApp.Core.Features.Animations;
using MaskApp.Core.Features.BuiltIns;
using MaskApp.Core.Features.Faces;
using MaskApp.Core.Features.Gallery;
using MaskApp.Core.Features.HolyPriest;
using MaskApp.Core.Features.QuickActions;
using MaskApp.Core.Features.Scenes;
using MaskApp.Core.Features.TextPresets;

namespace MaskApp.Core.Features.AnimationPacks;

public sealed class MaskPackArchiveService
{
    public const long MaxArchiveBytes = 32L * 1024 * 1024;
    public const long MaxTotalUncompressedBytes = 64L * 1024 * 1024;
    public const long MaxEntryBytes = 8L * 1024 * 1024;
    public const int MaxArchiveEntries = 300;
    private const long MaxManifestBytes = 1024 * 1024;
    private const double MaxCompressionRatio = 1000;

    private readonly ITextPresetStore textPresetStore;
    private readonly IFacePatternStore facePatternStore;
    private readonly IAnimationProjectStore animationProjectStore;
    private readonly IGalleryLayoutStore galleryLayoutStore;
    private readonly ISceneShowStore sceneShowStore;
    private readonly IMaskPackImportJournalStore journalStore;
    private readonly SemaphoreSlim mutationGate = new(1, 1);

    public MaskPackArchiveService(
        ITextPresetStore textPresetStore,
        IFacePatternStore facePatternStore,
        IAnimationProjectStore animationProjectStore,
        IGalleryLayoutStore galleryLayoutStore,
        ISceneShowStore sceneShowStore,
        IMaskPackImportJournalStore journalStore)
    {
        this.textPresetStore = textPresetStore;
        this.facePatternStore = facePatternStore;
        this.animationProjectStore = animationProjectStore;
        this.galleryLayoutStore = galleryLayoutStore;
        this.sceneShowStore = sceneShowStore;
        this.journalStore = journalStore;
    }

    public async Task<MaskPackExportResult> ExportAsync(
        Stream destination,
        MaskPackExportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(request);
        if (!destination.CanWrite)
        {
            return new MaskPackExportResult(false, "The export destination is not writable.", 0, 0);
        }

        var snapshot = await LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
        EnsureWritable(snapshot);
        var payloads = BuildExportPayloads(snapshot, request);
        if (payloads.Count is < 1 or > MaskPackManifestParser.MaxContentEntries)
        {
            return new MaskPackExportResult(
                false,
                $"The export contains {payloads.Count} entries; the safe limit is {MaskPackManifestParser.MaxContentEntries}.",
                0,
                0);
        }

        var entries = payloads.Select(payload => payload.Entry).ToArray();
        var manifest = new MaskPackManifest
        {
            SchemaVersion = MaskPackManifest.CurrentSchemaVersion,
            PackName = NormalizeName(request.PackName, "MaskApp Show"),
            Author = NormalizeName(request.Author, "MaskApp"),
            Source = "maskapp-export",
            ArtDisplay = new MaskPackDisplayGeometry
            {
                Width = MaskPackManifestParser.ArtWidth,
                Height = MaskPackManifestParser.RequiredHeight
            },
            TextDisplay = new MaskPackDisplayGeometry
            {
                Width = MaskPackManifestParser.TextWidth,
                Height = MaskPackManifestParser.RequiredHeight
            },
            Contents = entries
        };
        var validation = MaskPackManifestParser.Validate(manifest);
        if (!validation.IsValid)
        {
            return new MaskPackExportResult(false, string.Join(" ", validation.Errors), 0, 0);
        }

        await using var archiveBuffer = new MemoryStream();
        using (var archive = new ZipArchive(archiveBuffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(archive, "manifest.json", Encoding.UTF8.GetBytes(MaskPackManifestParser.ToJson(manifest)));
            foreach (var payload in payloads)
            {
                cancellationToken.ThrowIfCancellationRequested();
                WriteEntry(archive, payload.Entry.Path, payload.Bytes);
            }
        }

        if (archiveBuffer.Length > MaxArchiveBytes)
        {
            return new MaskPackExportResult(
                false,
                $"The compressed MaskPack is {archiveBuffer.Length / (1024d * 1024d):0.0} MB; the safe limit is {MaxArchiveBytes / (1024 * 1024)} MB.",
                0,
                0);
        }

        archiveBuffer.Position = 0;
        await archiveBuffer.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
        await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
        return new MaskPackExportResult(
            true,
            $"Exported {payloads.Count} content entries as MaskPack v2.",
            payloads.Count,
            archiveBuffer.Length);
    }

    public async Task<MaskPackInspection> InspectAsync(
        Stream source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        try
        {
            await using var buffer = await CopyArchiveBoundedAsync(source, cancellationToken).ConfigureAwait(false);
            using var archive = new ZipArchive(buffer, ZipArchiveMode.Read, leaveOpen: true);
            var files = ValidateArchiveShape(archive);
            var manifestEntry = files.GetValueOrDefault("manifest.json")
                ?? throw new InvalidDataException("MaskPack archive must contain manifest.json at its root.");
            var manifestBytes = await ReadEntryBoundedAsync(
                manifestEntry,
                MaxManifestBytes,
                cancellationToken).ConfigureAwait(false);
            var parsed = MaskPackManifestParser.ParseJson(Encoding.UTF8.GetString(manifestBytes));
            if (!parsed.IsValid || parsed.Manifest is null)
            {
                return new MaskPackInspection
                {
                    Errors = parsed.Errors,
                    Warnings = parsed.Warnings
                };
            }

            var warnings = parsed.Warnings.ToList();
            var package = parsed.Manifest.SchemaVersion == 1
                ? await MigrateV1Async(parsed.Manifest, files, warnings, cancellationToken).ConfigureAwait(false)
                : await DecodeV2Async(parsed.Manifest, files, cancellationToken).ConfigureAwait(false);
            var snapshot = await LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
            EnsureWritable(snapshot);
            var semanticErrors = ValidateSemantics(package, snapshot);
            return new MaskPackInspection
            {
                Package = semanticErrors.Count == 0 ? package : null,
                Errors = semanticErrors,
                Warnings = warnings,
                Conflicts = semanticErrors.Count == 0 ? BuildConflicts(package, snapshot) : []
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is InvalidDataException or IOException or UnauthorizedAccessException
                or InvalidOperationException or ArgumentException or FormatException or OverflowException or CryptographicException)
        {
            return new MaskPackInspection
            {
                Errors = [ShortMessage(exception)]
            };
        }
    }

    public async Task<MaskPackImportResult> ImportAsync(
        MaskPackImportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.Inspection.IsValid || request.Inspection.Package is null)
        {
            return new MaskPackImportResult(false, "Inspect a valid MaskPack before importing it.", 0, 0, 0, 0);
        }

        await mutationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var recovered = await RecoverInterruptedImportCoreAsync(cancellationToken).ConfigureAwait(false);
            var original = await LoadSnapshotAsync(cancellationToken).ConfigureAwait(false);
            EnsureWritable(original);
            var currentConflicts = BuildConflicts(request.Inspection.Package, original);
            if (!ConflictsMatch(request.Inspection.Conflicts, currentConflicts))
            {
                return new MaskPackImportResult(
                    false,
                    "Local content changed after the conflict preview. Inspect the MaskPack again before importing.",
                    0,
                    0,
                    0,
                    0,
                    recovered);
            }

            var plan = BuildImportPlan(request, currentConflicts, original);
            if (plan.Error is not null)
            {
                return new MaskPackImportResult(false, plan.Error, 0, 0, 0, 0, recovered);
            }

            MaskPackImportSnapshot target;
            try
            {
                target = NormalizeSnapshot(ApplyPlan(request.Inspection.Package, original, plan));
            }
            catch (Exception exception) when (
                exception is ArgumentException or InvalidOperationException or FormatException or OverflowException)
            {
                return new MaskPackImportResult(
                    false,
                    $"The import would create invalid local data: {ShortMessage(exception)}",
                    0,
                    0,
                    0,
                    0,
                    recovered);
            }

            var journal = new MaskPackImportJournal
            {
                ImportId = $"maskpack-{Guid.NewGuid():N}",
                StartedAt = DateTimeOffset.UtcNow,
                Original = original
            };
            await journalStore.SaveAsync(journal, cancellationToken).ConfigureAwait(false);
            try
            {
                await SaveSnapshotAsync(target, cancellationToken).ConfigureAwait(false);
                await journalStore.ClearAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception importException) when (
                importException is IOException or UnauthorizedAccessException or InvalidOperationException
                    or ArgumentException or FormatException)
            {
                try
                {
                    await SaveSnapshotAsync(original, cancellationToken).ConfigureAwait(false);
                    await journalStore.ClearAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception rollbackException) when (
                    rollbackException is IOException or UnauthorizedAccessException or InvalidOperationException
                        or ArgumentException or FormatException)
                {
                    return new MaskPackImportResult(
                        false,
                        $"Import stopped and automatic rollback could not finish. The recovery journal was preserved: {ShortMessage(rollbackException)}",
                        0,
                        0,
                        0,
                        0,
                        recovered);
                }

                return new MaskPackImportResult(
                    false,
                    $"Import stopped and all original stores were restored: {ShortMessage(importException)}",
                    0,
                    0,
                    0,
                    0,
                    recovered);
            }

            return new MaskPackImportResult(
                true,
                $"Imported {plan.ImportedCount} item(s), renamed {plan.RenamedCount}, skipped {plan.SkippedCount}, and replaced {plan.ReplacedCount}.",
                plan.ImportedCount,
                plan.RenamedCount,
                plan.SkippedCount,
                plan.ReplacedCount,
                recovered);
        }
        finally
        {
            mutationGate.Release();
        }
    }

    public async Task<bool> RecoverInterruptedImportAsync(CancellationToken cancellationToken = default)
    {
        await mutationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await RecoverInterruptedImportCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            mutationGate.Release();
        }
    }

    private async Task<bool> RecoverInterruptedImportCoreAsync(CancellationToken cancellationToken)
    {
        var journal = await journalStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        if (journal is null)
        {
            return false;
        }

        await SaveSnapshotAsync(journal.Original, cancellationToken).ConfigureAwait(false);
        await journalStore.ClearAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private List<ArchivePayload> BuildExportPayloads(
        MaskPackImportSnapshot snapshot,
        MaskPackExportRequest request)
    {
        var payloads = new List<ArchivePayload>();
        var referenced = snapshot.GalleryLayout.Pages
            .SelectMany(page => page.Items)
            .Select(item => item.GalleryItemId)
            .Concat(snapshot.SceneShow.Scenes.SelectMany(scene => scene.Steps).Select(step => step.GalleryItemId))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.Ordinal);
        foreach (var face in snapshot.Faces.Patterns.Where(face =>
                     !face.IsBuiltIn
                     && (request.IncludeUnreferencedUserContent || referenced.Contains($"face:{face.Id}"))))
        {
            AddPayload(payloads, MaskPackContentType.Face, face.Id, face.DisplayName, MaskPackPayloadCodec.SerializeFace(face));
        }

        foreach (var animation in snapshot.Animations.Projects.Where(animation =>
                     request.IncludeUnreferencedUserContent || referenced.Contains($"animation:{animation.Id}")))
        {
            AddPayload(payloads, MaskPackContentType.Animation, animation.Id, animation.DisplayName, MaskPackPayloadCodec.SerializeAnimation(animation));
        }

        foreach (var preset in snapshot.TextPresets.Presets.Where(preset =>
                     (!preset.IsSeed && request.IncludeUnreferencedUserContent)
                     || referenced.Contains($"text:{preset.Id.Value}")))
        {
            AddPayload(payloads, MaskPackContentType.TextPreset, preset.Id.Value, preset.DisplayName, MaskPackPayloadCodec.SerializeTextPreset(preset));
        }

        foreach (var page in snapshot.GalleryLayout.Pages)
        {
            AddPayload(payloads, MaskPackContentType.Page, page.PageId, page.Title, MaskPackPayloadCodec.SerializePage(page));
        }

        foreach (var scene in snapshot.SceneShow.Scenes)
        {
            AddPayload(payloads, MaskPackContentType.Scene, scene.Id, scene.DisplayName, MaskPackPayloadCodec.SerializeScene(scene));
        }

        foreach (var setlist in snapshot.SceneShow.Setlists)
        {
            AddPayload(payloads, MaskPackContentType.Setlist, setlist.Id, setlist.DisplayName, MaskPackPayloadCodec.SerializeSetlist(setlist));
        }

        AddPayload(
            payloads,
            MaskPackContentType.Appearance,
            "appearance",
            "Library ordering",
            MaskPackPayloadCodec.SerializeAppearance(new MaskPackAppearanceSettings
            {
                GalleryOrder = snapshot.GalleryLayout.Order
            }));
        return payloads;
    }

    private async Task<MaskPackDecodedPackage> DecodeV2Async(
        MaskPackManifest manifest,
        IReadOnlyDictionary<string, ZipArchiveEntry> files,
        CancellationToken cancellationToken)
    {
        var listedPaths = manifest.Contents.Select(content => content.Path).ToHashSet(StringComparer.Ordinal);
        var unlisted = files.Keys.Where(path => path != "manifest.json" && !listedPaths.Contains(path)).ToArray();
        if (unlisted.Length > 0)
        {
            throw new InvalidDataException($"Archive contains unlisted entries: {string.Join(", ", unlisted.Take(3))}.");
        }

        var faces = new List<MaskPackDecodedItem<FacePattern>>();
        var animations = new List<MaskPackDecodedItem<AnimationProject>>();
        var texts = new List<MaskPackDecodedItem<TextPreset>>();
        var pages = new List<MaskPackDecodedItem<GalleryPageLayout>>();
        var scenes = new List<MaskPackDecodedItem<PerformanceScene>>();
        var setlists = new List<MaskPackDecodedItem<PerformanceSetlist>>();
        MaskPackDecodedItem<MaskPackAppearanceSettings>? appearance = null;
        foreach (var content in manifest.Contents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsSafeArchivePath(content.Path) || content.Path == "manifest.json")
            {
                throw new InvalidDataException($"Content {content.Id} uses an unsafe archive path.");
            }

            var entry = files.GetValueOrDefault(content.Path)
                ?? throw new InvalidDataException($"Content entry {content.Path} is missing.");
            var bytes = await ReadEntryBoundedAsync(entry, MaxEntryBytes, cancellationToken).ConfigureAwait(false);
            var hash = Sha256(bytes);
            if (!string.Equals(hash, content.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"Content hash mismatch for {content.Type}:{content.Id}.");
            }

            switch (content.Type)
            {
                case MaskPackContentType.Face:
                {
                    var value = MaskPackPayloadCodec.DeserializeFace(bytes);
                    faces.Add(new(content, EnsureId(content, value.Id, value)));
                    break;
                }
                case MaskPackContentType.Animation:
                {
                    var value = MaskPackPayloadCodec.DeserializeAnimation(bytes);
                    animations.Add(new(content, EnsureId(content, value.Id, value)));
                    break;
                }
                case MaskPackContentType.TextPreset:
                {
                    var value = MaskPackPayloadCodec.DeserializeTextPreset(bytes);
                    if (!string.Equals(content.Id, value.Id.Value, StringComparison.Ordinal))
                    {
                        throw new InvalidDataException($"Content id {content.Id} does not match its text payload id {value.Id.Value}.");
                    }

                    texts.Add(new(content, value));
                    break;
                }
                case MaskPackContentType.Page:
                {
                    var value = MaskPackPayloadCodec.DeserializePage(bytes);
                    pages.Add(new(content, EnsureId(content, value.PageId, value)));
                    break;
                }
                case MaskPackContentType.Scene:
                {
                    var value = MaskPackPayloadCodec.DeserializeScene(bytes);
                    scenes.Add(new(content, EnsureId(content, value.Id, value)));
                    break;
                }
                case MaskPackContentType.Setlist:
                {
                    var value = MaskPackPayloadCodec.DeserializeSetlist(bytes);
                    setlists.Add(new(content, EnsureId(content, value.Id, value)));
                    break;
                }
                case MaskPackContentType.Appearance:
                    if (appearance is not null)
                    {
                        throw new InvalidDataException("MaskPack can contain only one appearance payload.");
                    }

                    appearance = new(content, MaskPackPayloadCodec.DeserializeAppearance(bytes));
                    break;
                default:
                    throw new InvalidDataException($"Unsupported MaskPack content type {content.Type}.");
            }
        }

        return new MaskPackDecodedPackage
        {
            Manifest = manifest,
            Faces = faces,
            Animations = animations,
            TextPresets = texts,
            Pages = pages,
            Scenes = scenes,
            Setlists = setlists,
            Appearance = appearance
        };
    }

    private async Task<MaskPackDecodedPackage> MigrateV1Async(
        MaskPackManifest legacy,
        IReadOnlyDictionary<string, ZipArchiveEntry> files,
        ICollection<string> warnings,
        CancellationToken cancellationToken)
    {
        var faces = new List<MaskPackDecodedItem<FacePattern>>();
        var animations = new List<MaskPackDecodedItem<AnimationProject>>();
        var payloads = new List<ArchivePayload>();
        for (var assetIndex = 0; assetIndex < legacy.Assets.Length; assetIndex++)
        {
            var asset = legacy.Assets[assetIndex];
            var frames = new List<FacePattern>();
            foreach (var frame in asset.Frames)
            {
                if (!IsSafeArchivePath(frame.Path))
                {
                    throw new InvalidDataException($"Legacy asset {asset.Id} uses an unsafe frame path.");
                }

                var entry = files.GetValueOrDefault(frame.Path)
                    ?? throw new InvalidDataException($"Legacy frame {frame.Path} is missing.");
                var bytes = await ReadEntryBoundedAsync(entry, MaxEntryBytes, cancellationToken).ConfigureAwait(false);
                frames.Add(LegacyMaskPackPngDecoder.Decode44x58(
                    bytes,
                    $"{asset.Id}-frame-{frames.Count + 1}",
                    $"{asset.Name} frame {frames.Count + 1}",
                    Math.Clamp(7 + assetIndex, FacePattern.MinSlot, FacePattern.MaxSlot)));
            }

            if (asset.Type == MaskPackAssetType.StaticImage)
            {
                var face = frames[0] with { Id = asset.Id, DisplayName = asset.Name };
                var bytes = MaskPackPayloadCodec.SerializeFace(face);
                var payload = CreatePayload(MaskPackContentType.Face, face.Id, face.DisplayName, bytes, payloads.Count);
                payloads.Add(payload);
                faces.Add(new(payload.Entry, face));
            }
            else
            {
                var project = new AnimationProject
                {
                    Id = asset.Id,
                    DisplayName = asset.Name,
                    Source = AnimationProjectSource.MaskPackImport,
                    LoopMode = asset.Loop ? AnimationLoopMode.Continuous : AnimationLoopMode.Finite,
                    FiniteLoopCount = 1,
                    Frames = frames.Select((frame, index) => new AnimationProjectFrame
                    {
                        Id = $"frame-{index + 1}",
                        Pattern = frame,
                        Duration = TimeSpan.FromMilliseconds(asset.Frames[index].DurationMs ?? asset.FrameDurationMs)
                    }).ToArray()
                }.Normalize();
                var bytes = MaskPackPayloadCodec.SerializeAnimation(project);
                var payload = CreatePayload(MaskPackContentType.Animation, project.Id, project.DisplayName, bytes, payloads.Count);
                payloads.Add(payload);
                animations.Add(new(payload.Entry, project));
            }
        }

        warnings.Add("Migrated schema-v1 44x58 PNG art to the physical 46x58 canvas by adding one off column on each side.");
        warnings.Add("Schema-v1 archives have no payload hashes; verify imported content before live use.");
        var manifest = new MaskPackManifest
        {
            SchemaVersion = MaskPackManifest.CurrentSchemaVersion,
            PackName = legacy.PackName,
            Author = legacy.Author,
            Source = "maskpack-v1-migration",
            ArtDisplay = new MaskPackDisplayGeometry { Width = 46, Height = 58 },
            TextDisplay = new MaskPackDisplayGeometry { Width = 44, Height = 58 },
            Contents = payloads.Select(payload => payload.Entry).ToArray()
        };
        return new MaskPackDecodedPackage
        {
            Manifest = manifest,
            Faces = faces,
            Animations = animations,
            MigratedFromV1 = true
        };
    }

    private IReadOnlyList<string> ValidateSemantics(
        MaskPackDecodedPackage package,
        MaskPackImportSnapshot current)
    {
        var errors = new List<string>();
        CheckCount(package.Faces.Count, 50, "faces", errors);
        CheckCount(package.Animations.Count, AnimationProject.MaxProjects, "animations", errors);
        CheckCount(package.TextPresets.Count, 150, "text presets", errors);
        CheckCount(package.Pages.Count, 50, "Pages", errors);
        CheckCount(package.Scenes.Count, SceneShowState.MaxScenes, "Scenes", errors);
        CheckCount(package.Setlists.Count, SceneShowState.MaxSetlists, "setlists", errors);

        foreach (var page in package.Pages)
        {
            if (page.Value.Items.Count > 64)
            {
                errors.Add($"Page {page.Value.Title} exceeds the 64-shortcut package limit.");
            }

            if (page.Value.Title.Length > 160 || page.Value.Items.Any(item => item.Label.Length > 160))
            {
                errors.Add($"Page {page.Value.PageId} contains text longer than the package limit.");
            }
        }

        foreach (var animation in package.Animations)
        {
            var compilation = new AnimationProjectCompiler().Compile(animation.Value);
            if (!compilation.Succeeded)
            {
                errors.Add($"Animation {animation.Value.DisplayName}: {compilation.Message}");
            }
        }

        foreach (var text in package.TextPresets)
        {
            if (text.Value.DisplayName.Length > 160
                || text.Value.InputText.Length > 2000
                || text.Value.MaskText.Length > 2000
                || text.Value.Tags.Count > 32)
            {
                errors.Add($"Text preset {text.Entry.Id} exceeds the safe text or tag limit.");
            }
        }

        var gallery = BuildSemanticCatalog(package, current);
        var galleryIds = gallery.Keys.ToHashSet(StringComparer.Ordinal);
        foreach (var page in package.Pages)
        {
            foreach (var item in page.Value.Items)
            {
                if (!galleryIds.Contains(item.GalleryItemId) && !IsKnownExternalGalleryId(item.GalleryItemId))
                {
                    errors.Add($"Page {page.Value.Title} references missing content {item.GalleryItemId}.");
                }
            }
        }

        var validator = new SceneValidator();
        foreach (var scene in package.Scenes)
        {
            var validation = validator.Validate(scene.Value, gallery);
            errors.AddRange(validation.Issues
                .Where(issue => issue.Severity == SceneValidationSeverity.Blocking)
                .Select(issue => $"Scene {scene.Value.DisplayName}: {issue.Message}"));
        }

        var sceneIds = current.SceneShow.Scenes.Select(scene => scene.Id)
            .Concat(package.Scenes.Select(scene => scene.Value.Id))
            .ToHashSet(StringComparer.Ordinal);
        foreach (var setlist in package.Setlists)
        {
            foreach (var cue in setlist.Value.Cues.Where(cue => !sceneIds.Contains(cue.SceneId)))
            {
                errors.Add($"Setlist {setlist.Value.DisplayName} cue {cue.Label} references missing Scene {cue.SceneId}.");
            }
        }

        return errors.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static Dictionary<string, GalleryItem> BuildSemanticCatalog(
        MaskPackDecodedPackage package,
        MaskPackImportSnapshot current)
    {
        var result = new Dictionary<string, GalleryItem>(StringComparer.Ordinal);
        foreach (var face in current.Faces.Patterns.Concat(package.Faces.Select(item => item.Value)))
        {
            result[$"face:{face.Id}"] = new GalleryItem
            {
                Id = $"face:{face.Id}",
                Type = GalleryItemType.CustomStaticFace,
                Title = face.DisplayName,
                FacePattern = face
            };
        }

        foreach (var animation in current.Animations.Projects.Concat(package.Animations.Select(item => item.Value)))
        {
            result[$"animation:{animation.Id}"] = new GalleryItem
            {
                Id = $"animation:{animation.Id}",
                Type = GalleryItemType.CustomAnimation,
                Title = animation.DisplayName
            };
        }

        foreach (var text in current.TextPresets.Presets.Concat(package.TextPresets.Select(item => item.Value)))
        {
            result[$"text:{text.Id.Value}"] = new GalleryItem
            {
                Id = $"text:{text.Id.Value}",
                Type = GalleryItemType.TextPreset,
                Title = text.DisplayName
            };
        }

        foreach (var animation in AppBuiltInAnimationCatalog.CreateBuiltIns())
        {
            var id = $"app-animation:{animation.Id}";
            result[id] = new GalleryItem
            {
                Id = id,
                Type = GalleryItemType.AppBuiltInAnimation,
                Title = animation.DisplayName,
                AppAnimation = animation
            };
        }

        foreach (var reference in package.Scenes.SelectMany(item => item.Value.Steps).Select(step => step.GalleryItemId))
        {
            var migratedReference = HolyPriestBuiltInCatalog.MigrateGalleryItemId(reference);
            if (result.ContainsKey(migratedReference))
            {
                continue;
            }

            if (migratedReference.StartsWith("built-in:StaticImage:", StringComparison.Ordinal))
            {
                result[migratedReference] = new GalleryItem { Id = migratedReference, Type = GalleryItemType.BuiltInStaticImage, Title = migratedReference };
            }
            else if (migratedReference.StartsWith("built-in:Animation:", StringComparison.Ordinal))
            {
                result[migratedReference] = new GalleryItem { Id = migratedReference, Type = GalleryItemType.BuiltInAnimation, Title = migratedReference };
            }
            else if (migratedReference.StartsWith("app-animation:", StringComparison.Ordinal))
            {
                result[migratedReference] = new GalleryItem { Id = migratedReference, Type = GalleryItemType.AppBuiltInAnimation, Title = migratedReference };
            }
        }

        return result;
    }

    private IReadOnlyList<MaskPackConflict> BuildConflicts(
        MaskPackDecodedPackage package,
        MaskPackImportSnapshot snapshot)
    {
        var existing = ExistingHashes(snapshot);
        var usedByType = existing.Keys
            .GroupBy(key => key.Type)
            .ToDictionary(group => group.Key, group => group.Select(key => key.Id).ToHashSet(StringComparer.Ordinal));
        return package.Entries
            .Where(entry => existing.ContainsKey((entry.Type, entry.Id)))
            .Select(entry =>
            {
                var hash = existing[(entry.Type, entry.Id)];
                return new MaskPackConflict(
                    Key(entry.Type, entry.Id),
                    entry.Type,
                    entry.Id,
                    entry.Name,
                    hash,
                    entry.Sha256,
                    string.Equals(hash, entry.Sha256, StringComparison.OrdinalIgnoreCase),
                    UniqueId(entry.Id, usedByType[entry.Type]));
            })
            .ToArray();
    }

    private ImportPlan BuildImportPlan(
        MaskPackImportRequest request,
        IReadOnlyList<MaskPackConflict> conflicts,
        MaskPackImportSnapshot current)
    {
        var conflictsByKey = conflicts.ToDictionary(conflict => conflict.Key, StringComparer.Ordinal);
        var usedByType = ExistingHashes(current).Keys
            .GroupBy(key => key.Type)
            .ToDictionary(group => group.Key, group => group.Select(key => key.Id).ToHashSet(StringComparer.Ordinal));
        foreach (var type in Enum.GetValues<MaskPackContentType>())
        {
            usedByType.TryAdd(type, []);
        }

        var actions = new List<PlannedEntry>();
        foreach (var entry in request.Inspection.Package!.Entries)
        {
            var key = Key(entry.Type, entry.Id);
            if (!conflictsByKey.TryGetValue(key, out var conflict))
            {
                usedByType[entry.Type].Add(entry.Id);
                actions.Add(new PlannedEntry(entry, ImportAction.Add, entry.Id));
                continue;
            }

            var resolution = request.ConflictResolutions.GetValueOrDefault(key, request.DefaultResolution);
            if (resolution == MaskPackConflictResolution.Replace && !request.ConfirmReplace)
            {
                return ImportPlan.Failed("Replacing existing content requires explicit confirmation.");
            }

            if (resolution == MaskPackConflictResolution.Replace
                && entry.Type == MaskPackContentType.Face
                && current.Faces.Patterns.Any(face => face.Id == entry.Id && face.IsBuiltIn))
            {
                return ImportPlan.Failed("Built-in faces cannot be replaced by a MaskPack.");
            }

            if (resolution == MaskPackConflictResolution.Skip
                || (resolution == MaskPackConflictResolution.Merge && conflict.IsExactMatch))
            {
                actions.Add(new PlannedEntry(entry, ImportAction.Skip, entry.Id));
            }
            else if (resolution == MaskPackConflictResolution.Replace)
            {
                actions.Add(new PlannedEntry(entry, ImportAction.Replace, entry.Id));
            }
            else if (resolution == MaskPackConflictResolution.Merge
                && entry.Type is MaskPackContentType.Page or MaskPackContentType.Appearance)
            {
                actions.Add(new PlannedEntry(entry, ImportAction.Merge, entry.Id));
            }
            else
            {
                var renamed = UniqueId(entry.Id, usedByType[entry.Type]);
                usedByType[entry.Type].Add(renamed);
                actions.Add(new PlannedEntry(entry, ImportAction.Rename, renamed));
            }
        }

        return new ImportPlan(actions);
    }

    private static MaskPackImportSnapshot ApplyPlan(
        MaskPackDecodedPackage package,
        MaskPackImportSnapshot original,
        ImportPlan plan)
    {
        var actions = plan.Actions.ToDictionary(action => Key(action.Entry.Type, action.Entry.Id), StringComparer.Ordinal);
        var galleryMap = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var action in plan.Actions)
        {
            switch (action.Entry.Type)
            {
                case MaskPackContentType.Face:
                    galleryMap[$"face:{action.Entry.Id}"] = $"face:{action.TargetId}";
                    break;
                case MaskPackContentType.Animation:
                    galleryMap[$"animation:{action.Entry.Id}"] = $"animation:{action.TargetId}";
                    break;
                case MaskPackContentType.TextPreset:
                    galleryMap[$"text:{action.Entry.Id}"] = $"text:{action.TargetId}";
                    break;
                case MaskPackContentType.Scene:
                    galleryMap[$"scene:{action.Entry.Id}"] = $"scene:{action.TargetId}";
                    break;
            }
        }

        var sceneMap = plan.Actions
            .Where(action => action.Entry.Type == MaskPackContentType.Scene)
            .ToDictionary(action => action.Entry.Id, action => action.TargetId, StringComparer.Ordinal);

        var faces = original.Faces.Patterns.ToList();
        foreach (var item in package.Faces)
        {
            var action = actions[Key(item.Entry.Type, item.Entry.Id)];
            if (action.Action == ImportAction.Skip)
            {
                continue;
            }

            var value = item.Value with
            {
                Id = action.TargetId,
                DisplayName = action.Action == ImportAction.Rename ? $"{item.Value.DisplayName} (Imported)" : item.Value.DisplayName,
                Source = FacePatternSource.ImportedPhoto,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LastUploadedAt = null,
                LastUploadStatus = string.Empty
            };
            faces.RemoveAll(face => face.Id == action.TargetId && !face.IsBuiltIn);
            faces.Add(value);
        }

        var animations = original.Animations.Projects.ToList();
        foreach (var item in package.Animations)
        {
            var action = actions[Key(item.Entry.Type, item.Entry.Id)];
            if (action.Action == ImportAction.Skip)
            {
                continue;
            }

            var value = item.Value with
            {
                Id = action.TargetId,
                DisplayName = action.Action == ImportAction.Rename ? $"{item.Value.DisplayName} (Imported)" : item.Value.DisplayName,
                Source = AnimationProjectSource.MaskPackImport,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            animations.RemoveAll(animation => animation.Id == action.TargetId);
            animations.Add(value);
        }

        var texts = original.TextPresets.Presets.ToList();
        foreach (var item in package.TextPresets)
        {
            var action = actions[Key(item.Entry.Type, item.Entry.Id)];
            if (action.Action == ImportAction.Skip)
            {
                continue;
            }

            var value = item.Value with
            {
                Id = new TextPresetId(action.TargetId),
                DisplayName = action.Action == ImportAction.Rename ? $"{item.Value.DisplayName} (Imported)" : item.Value.DisplayName,
                IsSeed = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LastSentAt = null,
                LastSendStatus = string.Empty
            };
            texts.RemoveAll(text => text.Id.Value == action.TargetId && !text.IsSeed);
            texts.Add(value);
        }

        var scenes = original.SceneShow.Scenes.ToList();
        foreach (var item in package.Scenes)
        {
            var action = actions[Key(item.Entry.Type, item.Entry.Id)];
            if (action.Action == ImportAction.Skip)
            {
                continue;
            }

            var value = item.Value with
            {
                Id = action.TargetId,
                DisplayName = action.Action == ImportAction.Rename ? $"{item.Value.DisplayName} (Imported)" : item.Value.DisplayName,
                Steps = item.Value.Steps.Select(step => step with
                {
                    GalleryItemId = galleryMap.GetValueOrDefault(step.GalleryItemId, step.GalleryItemId)
                }).ToArray(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            scenes.RemoveAll(scene => scene.Id == action.TargetId);
            scenes.Add(value);
        }

        var pages = original.GalleryLayout.Pages.ToList();
        foreach (var item in package.Pages)
        {
            var action = actions[Key(item.Entry.Type, item.Entry.Id)];
            if (action.Action == ImportAction.Skip)
            {
                continue;
            }

            var imported = item.Value with
            {
                PageId = action.TargetId,
                Title = action.Action == ImportAction.Rename ? $"{item.Value.Title} (Imported)" : item.Value.Title,
                Items = item.Value.Items.Select((pageItem, index) => pageItem with
                {
                    SlotId = action.Action == ImportAction.Merge ? $"imported-{Guid.NewGuid():N}" : pageItem.SlotId,
                    GalleryItemId = galleryMap.GetValueOrDefault(pageItem.GalleryItemId, pageItem.GalleryItemId),
                    SortIndex = index
                }).ToArray()
            };
            var existingIndex = pages.FindIndex(page => page.PageId == action.TargetId);
            if (action.Action == ImportAction.Merge && existingIndex >= 0)
            {
                var existing = pages[existingIndex];
                var known = existing.Items.Select(pageItem => pageItem.GalleryItemId).ToHashSet(StringComparer.Ordinal);
                var additions = imported.Items.Where(pageItem => known.Add(pageItem.GalleryItemId)).ToArray();
                pages[existingIndex] = existing with
                {
                    Items = existing.Items.Concat(additions).Select((pageItem, index) => pageItem with { SortIndex = index }).ToArray()
                };
            }
            else
            {
                if (existingIndex >= 0)
                {
                    pages.RemoveAt(existingIndex);
                }

                pages.Add(imported);
            }
        }

        var setlists = original.SceneShow.Setlists.ToList();
        foreach (var item in package.Setlists)
        {
            var action = actions[Key(item.Entry.Type, item.Entry.Id)];
            if (action.Action == ImportAction.Skip)
            {
                continue;
            }

            var value = item.Value with
            {
                Id = action.TargetId,
                DisplayName = action.Action == ImportAction.Rename ? $"{item.Value.DisplayName} (Imported)" : item.Value.DisplayName,
                Cues = item.Value.Cues.Select(cue => cue with
                {
                    Id = action.Action == ImportAction.Rename ? $"cue-{Guid.NewGuid():N}" : cue.Id,
                    SceneId = sceneMap.GetValueOrDefault(cue.SceneId, cue.SceneId)
                }).ToArray(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            setlists.RemoveAll(setlist => setlist.Id == action.TargetId);
            setlists.Add(value);
        }

        var order = original.GalleryLayout.Order;
        if (package.Appearance is not null)
        {
            var action = actions[Key(package.Appearance.Entry.Type, package.Appearance.Entry.Id)];
            if (action.Action != ImportAction.Skip)
            {
                var imported = RemapOrder(package.Appearance.Value.GalleryOrder, galleryMap);
                order = action.Action == ImportAction.Replace
                    ? imported
                    : MergeOrder(order, imported);
            }
        }

        return new MaskPackImportSnapshot
        {
            TextPresets = original.TextPresets with { Presets = texts, Status = "MaskPack imported." },
            Faces = original.Faces with { Patterns = faces, Status = "MaskPack imported." },
            Animations = original.Animations with { Projects = animations, Status = "MaskPack imported." },
            GalleryLayout = original.GalleryLayout with { Pages = pages, Order = order, Status = "MaskPack imported." },
            SceneShow = original.SceneShow with
            {
                Scenes = scenes,
                Setlists = setlists,
                Status = "MaskPack imported."
            }
        };
    }

    private async Task<MaskPackImportSnapshot> LoadSnapshotAsync(CancellationToken cancellationToken) => new()
    {
        TextPresets = await textPresetStore.LoadAsync(cancellationToken).ConfigureAwait(false),
        Faces = await facePatternStore.LoadAsync(cancellationToken).ConfigureAwait(false),
        Animations = await animationProjectStore.LoadAsync(cancellationToken).ConfigureAwait(false),
        GalleryLayout = await galleryLayoutStore.LoadAsync(cancellationToken).ConfigureAwait(false),
        SceneShow = await sceneShowStore.LoadAsync(cancellationToken).ConfigureAwait(false)
    };

    private async Task SaveSnapshotAsync(MaskPackImportSnapshot snapshot, CancellationToken cancellationToken)
    {
        await textPresetStore.SaveAsync(snapshot.TextPresets, cancellationToken).ConfigureAwait(false);
        await facePatternStore.SaveAsync(snapshot.Faces, cancellationToken).ConfigureAwait(false);
        await animationProjectStore.SaveAsync(snapshot.Animations, cancellationToken).ConfigureAwait(false);
        await galleryLayoutStore.SaveAsync(snapshot.GalleryLayout, cancellationToken).ConfigureAwait(false);
        await sceneShowStore.SaveAsync(snapshot.SceneShow, cancellationToken).ConfigureAwait(false);
    }

    private static MaskPackImportSnapshot NormalizeSnapshot(MaskPackImportSnapshot snapshot) => snapshot with
    {
        TextPresets = snapshot.TextPresets.Normalize(),
        Faces = snapshot.Faces.Normalize(),
        Animations = snapshot.Animations.Normalize(),
        GalleryLayout = snapshot.GalleryLayout.Normalize(),
        SceneShow = snapshot.SceneShow.Normalize()
    };

    private static void EnsureWritable(MaskPackImportSnapshot snapshot)
    {
        if (snapshot.TextPresets.UsedFallback || snapshot.Faces.UsedFallback
            || snapshot.Animations.UsedFallback || snapshot.GalleryLayout.UsedFallback
            || snapshot.SceneShow.UsedFallback)
        {
            throw new InvalidOperationException(
                "A content store is using a protected fallback. Export diagnostics and recover that store before MaskPack import/export.");
        }
    }

    private static Dictionary<(MaskPackContentType Type, string Id), string> ExistingHashes(
        MaskPackImportSnapshot snapshot)
    {
        var result = new Dictionary<(MaskPackContentType, string), string>();
        foreach (var face in snapshot.Faces.Patterns)
        {
            result[(MaskPackContentType.Face, face.Id)] = Sha256(MaskPackPayloadCodec.SerializeFace(face));
        }

        foreach (var animation in snapshot.Animations.Projects)
        {
            result[(MaskPackContentType.Animation, animation.Id)] = Sha256(MaskPackPayloadCodec.SerializeAnimation(animation));
        }

        foreach (var text in snapshot.TextPresets.Presets)
        {
            result[(MaskPackContentType.TextPreset, text.Id.Value)] = Sha256(MaskPackPayloadCodec.SerializeTextPreset(text));
        }

        foreach (var page in snapshot.GalleryLayout.Pages)
        {
            result[(MaskPackContentType.Page, page.PageId)] = Sha256(MaskPackPayloadCodec.SerializePage(page));
        }

        foreach (var scene in snapshot.SceneShow.Scenes)
        {
            result[(MaskPackContentType.Scene, scene.Id)] = Sha256(MaskPackPayloadCodec.SerializeScene(scene));
        }

        foreach (var setlist in snapshot.SceneShow.Setlists)
        {
            result[(MaskPackContentType.Setlist, setlist.Id)] = Sha256(MaskPackPayloadCodec.SerializeSetlist(setlist));
        }

        result[(MaskPackContentType.Appearance, "appearance")] = Sha256(
            MaskPackPayloadCodec.SerializeAppearance(new MaskPackAppearanceSettings
            {
                GalleryOrder = snapshot.GalleryLayout.Order
            }));
        return result;
    }

    private static Dictionary<string, ZipArchiveEntry> ValidateArchiveShape(ZipArchive archive)
    {
        var entries = archive.Entries.ToArray();
        if (entries.Length is < 1 or > MaxArchiveEntries)
        {
            throw new InvalidDataException($"MaskPack must contain between 1 and {MaxArchiveEntries} files.");
        }

        long total = 0;
        var result = new Dictionary<string, ZipArchiveEntry>(StringComparer.Ordinal);
        var caseInsensitivePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
            {
                throw new InvalidDataException($"Archive directory entry {entry.FullName} is not allowed.");
            }

            if (!IsSafeArchivePath(entry.FullName))
            {
                throw new InvalidDataException($"Archive entry {entry.FullName} uses an unsafe path.");
            }

            if (!result.TryAdd(entry.FullName, entry) || !caseInsensitivePaths.Add(entry.FullName))
            {
                throw new InvalidDataException($"Archive contains duplicate path {entry.FullName}.");
            }

            if (entry.Length < 0 || entry.Length > MaxEntryBytes)
            {
                throw new InvalidDataException($"Archive entry {entry.FullName} exceeds the {MaxEntryBytes / (1024 * 1024)} MB limit.");
            }

            total = checked(total + entry.Length);
            if (total > MaxTotalUncompressedBytes)
            {
                throw new InvalidDataException("MaskPack uncompressed content exceeds the safe total limit.");
            }

            if (entry.Length > 1024 * 1024)
            {
                var compressionRatio = entry.CompressedLength == 0
                    ? double.PositiveInfinity
                    : entry.Length / (double)entry.CompressedLength;
                if (compressionRatio > MaxCompressionRatio)
                {
                    throw new InvalidDataException(
                        $"Archive entry {entry.FullName} has an unsafe compression ratio ({compressionRatio:0}:1).");
                }
            }
        }

        return result;
    }

    private static async Task<MemoryStream> CopyArchiveBoundedAsync(
        Stream source,
        CancellationToken cancellationToken)
    {
        if (!source.CanRead)
        {
            throw new InvalidDataException("MaskPack source is not readable.");
        }

        if (source.CanSeek && source.Length - source.Position > MaxArchiveBytes)
        {
            throw new InvalidDataException($"MaskPack archive exceeds the {MaxArchiveBytes / (1024 * 1024)} MB limit.");
        }

        var result = new MemoryStream();
        var buffer = new byte[81920];
        long total = 0;
        while (true)
        {
            var read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            total = checked(total + read);
            if (total > MaxArchiveBytes)
            {
                await result.DisposeAsync().ConfigureAwait(false);
                throw new InvalidDataException($"MaskPack archive exceeds the {MaxArchiveBytes / (1024 * 1024)} MB limit.");
            }

            await result.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
        }

        result.Position = 0;
        return result;
    }

    private static async Task<byte[]> ReadEntryBoundedAsync(
        ZipArchiveEntry entry,
        long maxBytes,
        CancellationToken cancellationToken)
    {
        if (entry.Length > maxBytes)
        {
            throw new InvalidDataException($"Archive entry {entry.FullName} exceeds its safe size limit.");
        }

        await using var stream = entry.Open();
        using var output = new MemoryStream((int)Math.Min(entry.Length, int.MaxValue));
        var buffer = new byte[81920];
        long total = 0;
        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            total = checked(total + read);
            if (total > maxBytes)
            {
                throw new InvalidDataException($"Archive entry {entry.FullName} expanded beyond its safe size limit.");
            }

            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
        }

        if (total != entry.Length)
        {
            throw new InvalidDataException($"Archive entry {entry.FullName} length changed while reading.");
        }

        return output.ToArray();
    }

    private static void AddPayload(
        ICollection<ArchivePayload> payloads,
        MaskPackContentType type,
        string id,
        string name,
        byte[] bytes) =>
        payloads.Add(CreatePayload(type, id, name, bytes, payloads.Count));

    private static ArchivePayload CreatePayload(
        MaskPackContentType type,
        string id,
        string name,
        byte[] bytes,
        int index)
    {
        if (bytes.LongLength > MaxEntryBytes)
        {
            throw new InvalidOperationException($"{type}:{id} exceeds the per-entry MaskPack limit.");
        }

        var entry = new MaskPackContentEntry
        {
            Id = id,
            Type = type,
            Name = NormalizeName(name, id),
            Path = $"content/{type.ToString().ToLowerInvariant()}/{index:000}-{SafePathPart(id)}.json",
            Sha256 = Sha256(bytes)
        };
        return new ArchivePayload(entry, bytes);
    }

    private static void WriteEntry(ZipArchive archive, string path, byte[] bytes)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        stream.Write(bytes);
    }

    private static bool IsSafeArchivePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Length > 240 || Path.IsPathRooted(path)
            || path.Contains('\\') || path.Contains(':') || path.StartsWith("/", StringComparison.Ordinal))
        {
            return false;
        }

        var segments = path.Split('/', StringSplitOptions.None);
        return segments.All(segment => !string.IsNullOrWhiteSpace(segment) && segment is not "." and not "..");
    }

    private static string Sha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string SafePathPart(string value)
    {
        var chars = value.ToLowerInvariant()
            .Select(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_'
                ? character
                : '-')
            .Take(60)
            .ToArray();
        var result = new string(chars).Trim('-');
        return string.IsNullOrWhiteSpace(result) ? "content" : result;
    }

    private static string NormalizeName(string value, string fallback)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return normalized.Length <= 160 ? normalized : normalized[..160];
    }

    private static string Key(MaskPackContentType type, string id) => $"{type}:{id}";

    private static string UniqueId(string source, IReadOnlySet<string> used)
    {
        var stem = SafePathPart(source);
        stem = stem.Length > 110 ? stem[..110] : stem;
        var candidate = $"{stem}-imported";
        for (var suffix = 2; used.Contains(candidate); suffix++)
        {
            candidate = $"{stem}-imported-{suffix}";
        }

        return candidate;
    }

    private static bool ConflictsMatch(
        IReadOnlyList<MaskPackConflict> inspected,
        IReadOnlyList<MaskPackConflict> current)
    {
        var left = inspected.OrderBy(conflict => conflict.Key, StringComparer.Ordinal).ToArray();
        var right = current.OrderBy(conflict => conflict.Key, StringComparer.Ordinal).ToArray();
        return left.Length == right.Length && left.Zip(right).All(pair =>
            pair.First.Key == pair.Second.Key
            && string.Equals(pair.First.ExistingSha256, pair.Second.ExistingSha256, StringComparison.OrdinalIgnoreCase)
            && string.Equals(pair.First.ImportedSha256, pair.Second.ImportedSha256, StringComparison.OrdinalIgnoreCase));
    }

    private static T EnsureId<T>(MaskPackContentEntry entry, string payloadId, T value)
    {
        if (!string.Equals(entry.Id, payloadId, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Content id {entry.Id} does not match its {entry.Type} payload id {payloadId}.");
        }

        return value;
    }

    private static void CheckCount(int count, int limit, string label, ICollection<string> errors)
    {
        if (count > limit)
        {
            errors.Add($"MaskPack contains {count} {label}; the safe limit is {limit}.");
        }
    }

    private static bool IsKnownExternalGalleryId(string id)
    {
        id = HolyPriestBuiltInCatalog.MigrateGalleryItemId(id);
        var parts = id.Split(':');
        if (parts.Length == 3 && parts[0] == "built-in"
            && Enum.TryParse<BuiltInAssetType>(parts[1], out var type)
            && int.TryParse(parts[2], out var builtInId))
        {
            return BuiltInAssetCatalog.IsKnown(type, builtInId);
        }

        if (parts.Length == 2 && parts[0] == "app-animation")
        {
            return AppBuiltInAnimationCatalog.CreateBuiltIns().Any(animation => animation.Id == parts[1]);
        }

        if (parts.Length == 2 && parts[0] == "face")
        {
            return FacePatternFactory.CreateBuiltIns().Any(face => face.Id == parts[1]);
        }

        return parts.Length == 2 && parts[0] == "quick"
            && Enum.TryParse<QuickActionId>(parts[1], out var quickId)
            && new QuickActionCatalog().Actions.Any(action => action.Id == quickId);
    }

    private static GalleryOrderState RemapOrder(
        GalleryOrderState source,
        IReadOnlyDictionary<string, string> galleryMap) => source with
    {
        ItemOrders = source.ItemOrders.Select(order => order with
        {
            ItemId = galleryMap.GetValueOrDefault(order.ItemId, order.ItemId)
        }).ToArray()
    };

    private static GalleryOrderState MergeOrder(GalleryOrderState existing, GalleryOrderState imported) => new()
    {
        ItemOrders = existing.ItemOrders.Concat(imported.ItemOrders)
            .GroupBy(order => order.ItemId, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(order => order.SortIndex)
            .ToArray(),
        GroupOrders = existing.GroupOrders.Concat(imported.GroupOrders)
            .GroupBy(order => order.GroupKey, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(order => order.SortIndex)
            .ToArray()
    };

    private static string ShortMessage(Exception exception)
    {
        var message = string.IsNullOrWhiteSpace(exception.Message) ? exception.GetType().Name : exception.Message;
        return message.Length <= 240 ? message : string.Concat(message.AsSpan(0, 240), "...");
    }

    private sealed record ArchivePayload(MaskPackContentEntry Entry, byte[] Bytes);

    private enum ImportAction
    {
        Add,
        Merge,
        Rename,
        Skip,
        Replace
    }

    private sealed record PlannedEntry(MaskPackContentEntry Entry, ImportAction Action, string TargetId);

    private sealed record ImportPlan
    {
        public ImportPlan(IReadOnlyList<PlannedEntry> actions)
        {
            Actions = actions;
        }

        private ImportPlan(string error)
        {
            Error = error;
        }

        public IReadOnlyList<PlannedEntry> Actions { get; } = [];
        public string? Error { get; }
        public int ImportedCount => Actions.Count(action => action.Action is ImportAction.Add or ImportAction.Merge or ImportAction.Rename or ImportAction.Replace);
        public int RenamedCount => Actions.Count(action => action.Action == ImportAction.Rename);
        public int SkippedCount => Actions.Count(action => action.Action == ImportAction.Skip);
        public int ReplacedCount => Actions.Count(action => action.Action == ImportAction.Replace);

        public static ImportPlan Failed(string error) => new(error);
    }
}
