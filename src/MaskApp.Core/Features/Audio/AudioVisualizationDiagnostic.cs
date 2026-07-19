using System.Diagnostics;
using MaskApp.Core.Features.MaskControl;

namespace MaskApp.Core.Features.Audio;

public sealed record AudioVisualizationDiagnosticOptions
{
    public AudioVisualizationFraming Framing { get; init; } = AudioVisualizationFraming.LegacyAndroidLength;

    public AudioVisualizationPackingMode PackingMode { get; init; } = AudioVisualizationPackingMode.PaletteA;

    public TimeSpan PacketInterval { get; init; } = TimeSpan.FromMilliseconds(125);

    public int Repetitions { get; init; } = 2;

    public AudioVisualizationDiagnosticOptions Normalize() => this with
    {
        PacketInterval = PacketInterval < TimeSpan.Zero
            ? TimeSpan.Zero
            : PacketInterval > TimeSpan.FromSeconds(1)
                ? TimeSpan.FromSeconds(1)
                : PacketInterval,
        Repetitions = Math.Clamp(Repetitions, 1, 5)
    };
}

public sealed record AudioVisualizationDiagnosticResult
{
    public required AudioVisualizationEvidence Evidence { get; init; }

    public required IReadOnlyList<AudioVisualizationPacket> Packets { get; init; }
}

public sealed class AudioVisualizationDiagnostic : IDisposable
{
    private static readonly byte[][] DiagnosticPatterns =
    [
        new byte[AudioVisualizationProtocol.RenderValueCount],
        [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 1, 2, 3, 4, 5],
        [9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0, 9, 0],
        [0, 0, 3, 3, 6, 6, 9, 9, 9, 9, 6, 6, 3, 3, 0, 0, 0, 0, 3, 3, 6, 6, 9, 9],
        new byte[AudioVisualizationProtocol.RenderValueCount]
    ];
    private readonly object sync = new();
    private readonly IAudioVisualizationTransport transport;
    private readonly IVisualWorkCancellationSource? cancellationSource;
    private CancellationTokenSource? activeRunCancellation;
    private bool disposed;

    public AudioVisualizationDiagnostic(
        IAudioVisualizationTransport transport,
        IVisualWorkCancellationSource? cancellationSource = null)
    {
        this.transport = transport;
        this.cancellationSource = cancellationSource;
        if (cancellationSource is not null)
        {
            cancellationSource.VisualWorkCancelled += HandleVisualWorkCancelled;
        }
    }

    public async Task<AudioVisualizationDiagnosticResult> RunAsync(
        AudioVisualizationDiagnosticOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedOptions = (options ?? new AudioVisualizationDiagnosticOptions()).Normalize();
        using var runCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (activeRunCancellation is not null)
            {
                throw new InvalidOperationException("An audio visualization diagnostic is already running.");
            }

            activeRunCancellation = runCancellation;
        }

        try
        {
            return await RunCoreAsync(normalizedOptions, runCancellation.Token).ConfigureAwait(false);
        }
        finally
        {
            lock (sync)
            {
                if (ReferenceEquals(activeRunCancellation, runCancellation))
                {
                    activeRunCancellation = null;
                }
            }
        }
    }

    public void CancelActiveTest()
    {
        CancellationTokenSource? cancellation;
        lock (sync)
        {
            cancellation = activeRunCancellation;
        }

        try
        {
            cancellation?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public void Dispose()
    {
        CancellationTokenSource? cancellation;
        lock (sync)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            cancellation = activeRunCancellation;
        }

        try
        {
            cancellation?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        if (cancellationSource is not null)
        {
            cancellationSource.VisualWorkCancelled -= HandleVisualWorkCancelled;
        }
    }

    private async Task<AudioVisualizationDiagnosticResult> RunCoreAsync(
        AudioVisualizationDiagnosticOptions normalizedOptions,
        CancellationToken cancellationToken)
    {
        if (!transport.IsReady)
        {
            var status = transport.State == AudioVisualizationTransportState.Unsupported
                ? AudioVisualizationEvidenceStatus.Unsupported
                : AudioVisualizationEvidenceStatus.Failed;
            return CreateResult(
                normalizedOptions,
                status,
                packets: [],
                packetsSent: 0,
                failedWrites: 0,
                elapsed: TimeSpan.Zero,
                transport.StatusText);
        }

        var packets = BuildPackets(normalizedOptions).ToArray();
        var sent = 0;
        var failures = 0;
        var startedAt = Stopwatch.GetTimestamp();
        string? failureMessage = null;
        for (var index = 0; index < packets.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await transport.SendAsync(packets[index], cancellationToken).ConfigureAwait(false);
            if (result.Succeeded)
            {
                sent++;
            }
            else
            {
                failures++;
                failureMessage = result.Message;
                break;
            }

            if (index + 1 < packets.Length && normalizedOptions.PacketInterval > TimeSpan.Zero)
            {
                await Task.Delay(normalizedOptions.PacketInterval, cancellationToken).ConfigureAwait(false);
            }
        }

        var elapsed = Stopwatch.GetElapsedTime(startedAt);
        var evidenceStatus = failures == 0 && sent == packets.Length
            ? AudioVisualizationEvidenceStatus.PendingPhysicalConfirmation
            : AudioVisualizationEvidenceStatus.Failed;
        var message = evidenceStatus == AudioVisualizationEvidenceStatus.PendingPhysicalConfirmation
            ? "Deterministic writes completed without an ACK. Confirm the visible mask result before enabling microphone input."
            : $"The deterministic audio test stopped after {sent} successful write(s): {failureMessage ?? "unknown write failure"}";
        return CreateResult(
            normalizedOptions,
            evidenceStatus,
            packets,
            sent,
            failures,
            elapsed,
            message);
    }

    private void HandleVisualWorkCancelled(object? sender, VisualWorkCancelledEventArgs args) =>
        CancelActiveTest();

    public static AudioVisualizationEvidence ConfirmPhysicalResult(
        AudioVisualizationEvidence pendingEvidence,
        bool passed,
        string? note = null)
    {
        ArgumentNullException.ThrowIfNull(pendingEvidence);
        if (pendingEvidence.Status != AudioVisualizationEvidenceStatus.PendingPhysicalConfirmation)
        {
            throw new InvalidOperationException("Only a completed deterministic test can receive physical confirmation.");
        }

        var statusText = passed
            ? "Physical confirmation recorded: the deterministic sequence was visible and stable on this mask."
            : "Physical confirmation recorded: the deterministic sequence was missing, unstable, or incorrect on this mask.";
        if (!string.IsNullOrWhiteSpace(note))
        {
            statusText = $"{statusText} {note.Trim()}";
        }

        return pendingEvidence.Normalize() with
        {
            Status = passed
                ? AudioVisualizationEvidenceStatus.Passed
                : AudioVisualizationEvidenceStatus.Failed,
            StatusText = statusText,
            TestedAt = DateTimeOffset.UtcNow
        };
    }

    private static IEnumerable<AudioVisualizationPacket> BuildPackets(
        AudioVisualizationDiagnosticOptions options)
    {
        for (var repetition = 0; repetition < options.Repetitions; repetition++)
        {
            foreach (var renderPattern in DiagnosticPatterns)
            {
                yield return options.PackingMode switch
                {
                    AudioVisualizationPackingMode.PaletteA or AudioVisualizationPackingMode.PaletteB =>
                        AudioVisualizationProtocol.BuildFromLevels(
                            options.PackingMode,
                            renderPattern,
                            options.Framing),
                    AudioVisualizationPackingMode.DuplicatedPairs =>
                        AudioVisualizationProtocol.BuildFromLevels(
                            options.PackingMode,
                            CollapseDuplicatedPairs(renderPattern),
                            options.Framing),
                    AudioVisualizationPackingMode.SpacedPairs =>
                        AudioVisualizationProtocol.BuildFromLevels(
                            options.PackingMode,
                            CollapseSpacedPairs(renderPattern),
                            options.Framing),
                    _ => throw new ArgumentOutOfRangeException(nameof(options), options.PackingMode, "Unknown audio packing mode.")
                };
            }
        }
    }

    private AudioVisualizationDiagnosticResult CreateResult(
        AudioVisualizationDiagnosticOptions options,
        AudioVisualizationEvidenceStatus status,
        IReadOnlyList<AudioVisualizationPacket> packets,
        int packetsSent,
        int failedWrites,
        TimeSpan elapsed,
        string message)
    {
        var intervals = Math.Max(0, packetsSent - 1);
        var observedCadence = elapsed > TimeSpan.Zero && intervals > 0
            ? intervals / elapsed.TotalSeconds
            : (double?)null;
        var requestedCadence = options.PacketInterval > TimeSpan.Zero
            ? 1 / options.PacketInterval.TotalSeconds
            : 60;
        var evidence = new AudioVisualizationEvidence
        {
            Status = status,
            Framing = options.Framing,
            PackingMode = options.PackingMode,
            CharacteristicObserved = transport.IsReady,
            IsSimulated = transport.IsSimulated,
            PacketsAttempted = packetsSent + failedWrites,
            PacketsSent = packetsSent,
            FailedWrites = failedWrites,
            RequestedCadenceHz = requestedCadence,
            ObservedWriteCadenceHz = observedCadence,
            TestedAt = DateTimeOffset.UtcNow,
            StatusText = message
        }.Normalize();
        return new AudioVisualizationDiagnosticResult
        {
            Evidence = evidence,
            Packets = packets
        };
    }

    private static byte[] CollapseDuplicatedPairs(IReadOnlyList<byte> renderValues)
    {
        var result = new byte[12];
        for (var index = 0; index < result.Length; index++)
        {
            result[index] = renderValues[index * 2];
        }

        return result;
    }

    private static byte[] CollapseSpacedPairs(IReadOnlyList<byte> renderValues)
    {
        var result = new byte[8];
        for (var index = 0; index < 4; index++)
        {
            result[index * 2] = renderValues[index * 6];
            result[(index * 2) + 1] = renderValues[(index * 6) + 1];
        }

        return result;
    }
}
