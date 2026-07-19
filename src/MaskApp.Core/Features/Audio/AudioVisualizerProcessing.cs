using System.Numerics;

namespace MaskApp.Core.Features.Audio;

public enum AudioVisualizerMode
{
    Spectrum,
    BassFace,
    VoiceMouth,
    DropDetector
}

public sealed record AudioVisualizerSettings
{
    public AudioVisualizerMode Mode { get; init; } = AudioVisualizerMode.Spectrum;

    public double Sensitivity { get; init; } = 1;

    public double Threshold { get; init; } = 0.08;

    public double Smoothing { get; init; } = 0.55;

    public AudioVisualizerSettings Normalize() => this with
    {
        Sensitivity = double.IsFinite(Sensitivity) ? Math.Clamp(Sensitivity, 0.25, 4) : 1,
        Threshold = double.IsFinite(Threshold) ? Math.Clamp(Threshold, 0, 0.75) : 0.08,
        Smoothing = double.IsFinite(Smoothing) ? Math.Clamp(Smoothing, 0, 0.9) : 0.55
    };
}

public sealed record AudioVisualizerFrame(
    IReadOnlyList<byte> Levels,
    double RootMeanSquare,
    double BassEnergy,
    bool DropTriggered);

public sealed class AudioVisualizerProcessor
{
    private const int BandCount = AudioVisualizationProtocol.RenderValueCount;
    private static readonly TimeSpan DropRefractoryPeriod = TimeSpan.FromMilliseconds(500);
    private readonly double[] priorLevels = new double[BandCount];
    private double noiseFloor = 0.01;
    private double priorBassEnergy;
    private DateTimeOffset lastDropAt = DateTimeOffset.MinValue;

    public double NoiseFloor => noiseFloor;

    public double Calibrate(IEnumerable<float[]> sampleBlocks)
    {
        ArgumentNullException.ThrowIfNull(sampleBlocks);
        var rootMeanSquares = sampleBlocks
            .Where(block => block is { Length: > 0 })
            .Select(block => CalculateRootMeanSquare(block))
            .OrderBy(value => value)
            .ToArray();
        if (rootMeanSquares.Length == 0)
        {
            throw new ArgumentException("At least one non-empty audio sample block is required.", nameof(sampleBlocks));
        }

        var percentileIndex = Math.Clamp(
            (int)Math.Floor((rootMeanSquares.Length - 1) * 0.8),
            0,
            rootMeanSquares.Length - 1);
        noiseFloor = Math.Clamp(rootMeanSquares[percentileIndex] * 1.35, 0.001, 0.35);
        ResetSmoothing();
        return noiseFloor;
    }

    public AudioVisualizerFrame Process(
        ReadOnlySpan<float> samples,
        int sampleRate,
        AudioVisualizerSettings settings,
        DateTimeOffset timestamp)
    {
        if (sampleRate < 1_000 || sampleRate > 384_000)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, "Sample rate is outside the supported range.");
        }

        if (samples.Length < 64)
        {
            throw new ArgumentException("At least 64 audio samples are required.", nameof(samples));
        }

        var normalizedSettings = settings.Normalize();
        var rootMeanSquare = CalculateRootMeanSquare(samples);
        var bands = CalculateBands(samples, sampleRate);
        var bassEnergy = bands.Take(4).Average();
        var targetLevels = normalizedSettings.Mode switch
        {
            AudioVisualizerMode.Spectrum => BuildSpectrumLevels(bands, normalizedSettings),
            AudioVisualizerMode.BassFace => BuildBassFaceLevels(bassEnergy, normalizedSettings),
            AudioVisualizerMode.VoiceMouth => BuildVoiceMouthLevels(bands, rootMeanSquare, normalizedSettings),
            AudioVisualizerMode.DropDetector => BuildDropLevels(
                bassEnergy,
                normalizedSettings,
                timestamp,
                out _),
            _ => throw new ArgumentOutOfRangeException(nameof(settings), settings.Mode, "Unknown audio visualizer mode.")
        };

        var dropTriggered = normalizedSettings.Mode == AudioVisualizerMode.DropDetector
            && targetLevels.Any(level => level > 0);
        if (normalizedSettings.Mode != AudioVisualizerMode.DropDetector)
        {
            for (var index = 0; index < targetLevels.Length; index++)
            {
                var smoothed = (priorLevels[index] * normalizedSettings.Smoothing)
                    + (targetLevels[index] * (1 - normalizedSettings.Smoothing));
                priorLevels[index] = smoothed;
                targetLevels[index] = (byte)Math.Clamp((int)Math.Round(smoothed), 0, 9);
            }
        }
        else
        {
            for (var index = 0; index < targetLevels.Length; index++)
            {
                priorLevels[index] = targetLevels[index];
            }
        }

        priorBassEnergy = bassEnergy;
        return new AudioVisualizerFrame(targetLevels, rootMeanSquare, bassEnergy, dropTriggered);
    }

    public void Reset()
    {
        priorBassEnergy = 0;
        lastDropAt = DateTimeOffset.MinValue;
        ResetSmoothing();
    }

    private static double[] CalculateBands(ReadOnlySpan<float> samples, int sampleRate)
    {
        var fftLength = HighestPowerOfTwo(Math.Min(samples.Length, 512));
        var buffer = new Complex[fftLength];
        var sampleOffset = samples.Length - fftLength;
        for (var index = 0; index < fftLength; index++)
        {
            var window = 0.5 - (0.5 * Math.Cos((2 * Math.PI * index) / (fftLength - 1)));
            buffer[index] = new Complex(samples[sampleOffset + index] * window, 0);
        }

        TransformInPlace(buffer);
        var nyquist = sampleRate / 2d;
        var minimumFrequency = Math.Min(40, nyquist / 8);
        var maximumFrequency = Math.Min(8_000, nyquist * 0.98);
        var bands = new double[BandCount];
        for (var bandIndex = 0; bandIndex < BandCount; bandIndex++)
        {
            var startRatio = bandIndex / (double)BandCount;
            var endRatio = (bandIndex + 1) / (double)BandCount;
            var startFrequency = minimumFrequency * Math.Pow(maximumFrequency / minimumFrequency, startRatio);
            var endFrequency = minimumFrequency * Math.Pow(maximumFrequency / minimumFrequency, endRatio);
            var startBin = Math.Clamp((int)Math.Floor(startFrequency * fftLength / sampleRate), 1, (fftLength / 2) - 1);
            var endBin = Math.Clamp((int)Math.Ceiling(endFrequency * fftLength / sampleRate), startBin + 1, fftLength / 2);
            var total = 0d;
            for (var bin = startBin; bin < endBin; bin++)
            {
                total += buffer[bin].Magnitude * 2 / fftLength;
            }

            bands[bandIndex] = total / Math.Max(1, endBin - startBin);
        }

        return bands;
    }

    private byte[] BuildSpectrumLevels(IReadOnlyList<double> bands, AudioVisualizerSettings settings)
    {
        var levels = new byte[BandCount];
        for (var index = 0; index < levels.Length; index++)
        {
            levels[index] = Quantize(bands[index], settings);
        }

        return levels;
    }

    private byte[] BuildBassFaceLevels(double bassEnergy, AudioVisualizerSettings settings)
    {
        var peak = Quantize(bassEnergy * 1.6, settings);
        var levels = new byte[BandCount];
        for (var index = 0; index < levels.Length; index++)
        {
            var distanceFromCenter = Math.Abs(index - ((BandCount - 1) / 2d)) / (BandCount / 2d);
            var weight = 1 - (distanceFromCenter * 0.7);
            levels[index] = (byte)Math.Clamp((int)Math.Round(peak * weight), 0, 9);
        }

        return levels;
    }

    private byte[] BuildVoiceMouthLevels(
        IReadOnlyList<double> bands,
        double rootMeanSquare,
        AudioVisualizerSettings settings)
    {
        var voiceEnergy = (bands.Skip(4).Take(10).Average() * 0.75) + (rootMeanSquare * 0.25);
        var opening = Quantize(voiceEnergy * 1.4, settings);
        var levels = new byte[BandCount];
        for (var index = 0; index < levels.Length; index++)
        {
            var row = index / 6;
            var rowWeight = row is 1 or 2 ? 1d : 0.45;
            levels[index] = (byte)Math.Clamp((int)Math.Round(opening * rowWeight), 0, 9);
        }

        return levels;
    }

    private byte[] BuildDropLevels(
        double bassEnergy,
        AudioVisualizerSettings settings,
        DateTimeOffset timestamp,
        out bool dropTriggered)
    {
        var effectiveThreshold = Math.Max(noiseFloor * 1.8, settings.Threshold * 0.5);
        var increase = bassEnergy - priorBassEnergy;
        dropTriggered = bassEnergy > effectiveThreshold
            && increase > effectiveThreshold * 0.35
            && timestamp - lastDropAt >= DropRefractoryPeriod;
        if (dropTriggered)
        {
            lastDropAt = timestamp;
        }

        return Enumerable.Repeat(dropTriggered ? (byte)9 : (byte)0, BandCount).ToArray();
    }

    private byte Quantize(double energy, AudioVisualizerSettings settings)
    {
        var floor = Math.Max(noiseFloor, settings.Threshold * 0.1);
        if (energy <= floor)
        {
            return 0;
        }

        var normalized = (energy - floor) * settings.Sensitivity * 18;
        return (byte)Math.Clamp((int)Math.Round(Math.Sqrt(Math.Max(0, normalized)) * 9), 0, 9);
    }

    private static double CalculateRootMeanSquare(ReadOnlySpan<float> samples)
    {
        var sum = 0d;
        foreach (var sample in samples)
        {
            sum += sample * sample;
        }

        return Math.Sqrt(sum / samples.Length);
    }

    private static int HighestPowerOfTwo(int value)
    {
        var result = 1;
        while (result <= value / 2)
        {
            result *= 2;
        }

        return result;
    }

    private static void TransformInPlace(Complex[] values)
    {
        var length = values.Length;
        for (var index = 1; index < length; index++)
        {
            var reversed = ReverseBits(index, length);
            if (reversed > index)
            {
                (values[index], values[reversed]) = (values[reversed], values[index]);
            }
        }

        for (var size = 2; size <= length; size *= 2)
        {
            var half = size / 2;
            var phaseStep = Complex.FromPolarCoordinates(1, -2 * Math.PI / size);
            for (var offset = 0; offset < length; offset += size)
            {
                var phase = Complex.One;
                for (var index = 0; index < half; index++)
                {
                    var even = values[offset + index];
                    var odd = values[offset + index + half] * phase;
                    values[offset + index] = even + odd;
                    values[offset + index + half] = even - odd;
                    phase *= phaseStep;
                }
            }
        }
    }

    private static int ReverseBits(int value, int length)
    {
        var reversed = 0;
        for (var remaining = length; remaining > 1; remaining >>= 1)
        {
            reversed = (reversed << 1) | (value & 1);
            value >>= 1;
        }

        return reversed;
    }

    private void ResetSmoothing()
    {
        Array.Clear(priorLevels);
    }
}

public sealed record AudioFlashSafetyDecision(bool CanSend, string Message);

public sealed class AudioFlashSafetyGate
{
    private static readonly TimeSpan FlashWindow = TimeSpan.FromSeconds(1);
    private readonly Queue<DateTimeOffset> brightTransitions = new();
    private double lastAcceptedAverage;

    public AudioFlashSafetyDecision Evaluate(IReadOnlyList<byte> levels, DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(levels);
        if (levels.Count != AudioVisualizationProtocol.RenderValueCount)
        {
            throw new ArgumentException("Audio visualizer safety checks require 24 render levels.", nameof(levels));
        }

        var average = levels.Average(level => (double)level);
        var isBrightRise = lastAcceptedAverage <= 2 && average >= 7;
        while (brightTransitions.TryPeek(out var transition)
            && timestamp - transition > FlashWindow)
        {
            brightTransitions.Dequeue();
        }

        if (isBrightRise && brightTransitions.Count >= 3)
        {
            return new AudioFlashSafetyDecision(
                false,
                "Frame suppressed by the non-overridable three-full-flashes-per-second live safety limit.");
        }

        if (isBrightRise)
        {
            brightTransitions.Enqueue(timestamp);
        }

        lastAcceptedAverage = average;
        return new AudioFlashSafetyDecision(true, "Frame is inside the conservative live flash limit.");
    }

    public void Reset()
    {
        brightTransitions.Clear();
        lastAcceptedAverage = 0;
    }
}
