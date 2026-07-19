using MaskApp.Core.Features.Audio;

namespace MaskApp.Core.Tests.Features.Audio;

public sealed class AudioVisualizerProcessingTests
{
    [Fact]
    public void Silence_ProducesAZeroSpectrum()
    {
        var processor = new AudioVisualizerProcessor();

        var frame = processor.Process(
            new float[256],
            16_000,
            new AudioVisualizerSettings { Smoothing = 0 },
            DateTimeOffset.UnixEpoch);

        Assert.All(frame.Levels, level => Assert.Equal(0, level));
        Assert.Equal(0, frame.RootMeanSquare);
    }

    [Fact]
    public void BassSine_ProducesBoundedNonzeroLevels()
    {
        var processor = new AudioVisualizerProcessor();
        var samples = CreateSine(256, 16_000, 125, 0.8);

        var frame = processor.Process(
            samples,
            16_000,
            new AudioVisualizerSettings
            {
                Mode = AudioVisualizerMode.BassFace,
                Sensitivity = 2,
                Smoothing = 0
            },
            DateTimeOffset.UnixEpoch);

        Assert.Contains(frame.Levels, level => level > 0);
        Assert.All(frame.Levels, level => Assert.InRange(level, (byte)0, (byte)9));
        Assert.True(frame.BassEnergy > 0);
    }

    [Fact]
    public void DropDetector_EnforcesHalfSecondRefractoryPeriod()
    {
        var processor = new AudioVisualizerProcessor();
        var settings = new AudioVisualizerSettings
        {
            Mode = AudioVisualizerMode.DropDetector,
            Sensitivity = 2,
            Threshold = 0.01,
            Smoothing = 0
        };
        var bass = CreateSine(256, 16_000, 125, 1);
        var silence = new float[256];

        var first = processor.Process(bass, 16_000, settings, DateTimeOffset.UnixEpoch);
        processor.Process(silence, 16_000, settings, DateTimeOffset.UnixEpoch.AddMilliseconds(100));
        var blocked = processor.Process(bass, 16_000, settings, DateTimeOffset.UnixEpoch.AddMilliseconds(200));
        processor.Process(silence, 16_000, settings, DateTimeOffset.UnixEpoch.AddMilliseconds(400));
        var later = processor.Process(bass, 16_000, settings, DateTimeOffset.UnixEpoch.AddMilliseconds(600));

        Assert.True(first.DropTriggered);
        Assert.False(blocked.DropTriggered);
        Assert.True(later.DropTriggered);
    }

    [Fact]
    public void FlashGate_SuppressesFourthFullBrightRiseInsideOneSecond()
    {
        var gate = new AudioFlashSafetyGate();
        var dark = Enumerable.Repeat((byte)0, 24).ToArray();
        var bright = Enumerable.Repeat((byte)9, 24).ToArray();
        var start = DateTimeOffset.UnixEpoch;

        Assert.True(gate.Evaluate(bright, start).CanSend);
        Assert.True(gate.Evaluate(dark, start.AddMilliseconds(100)).CanSend);
        Assert.True(gate.Evaluate(bright, start.AddMilliseconds(200)).CanSend);
        Assert.True(gate.Evaluate(dark, start.AddMilliseconds(300)).CanSend);
        Assert.True(gate.Evaluate(bright, start.AddMilliseconds(400)).CanSend);
        Assert.True(gate.Evaluate(dark, start.AddMilliseconds(500)).CanSend);

        var decision = gate.Evaluate(bright, start.AddMilliseconds(600));

        Assert.False(decision.CanSend);
        Assert.Contains("non-overridable", decision.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Calibration_UsesQuietSampleDistributionAndResetsSmoothing()
    {
        var processor = new AudioVisualizerProcessor();
        var quiet = Enumerable.Range(0, 12)
            .Select(_ => Enumerable.Repeat(0.01f, 128).ToArray())
            .ToArray();

        var floor = processor.Calibrate(quiet);

        Assert.InRange(floor, 0.013, 0.014);
        Assert.Equal(floor, processor.NoiseFloor);
    }

    private static float[] CreateSine(
        int length,
        int sampleRate,
        double frequency,
        double amplitude) =>
        Enumerable.Range(0, length)
            .Select(index => (float)(Math.Sin(2 * Math.PI * frequency * index / sampleRate) * amplitude))
            .ToArray();
}
