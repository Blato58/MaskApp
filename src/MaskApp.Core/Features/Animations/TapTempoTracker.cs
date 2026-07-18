namespace MaskApp.Core.Features.Animations;

public sealed class TapTempoTracker
{
    private static readonly TimeSpan ResetAfter = TimeSpan.FromSeconds(2);
    private const int MaximumTapCount = 8;
    private readonly Queue<TimeSpan> taps = new();

    public double? AddTap(TimeSpan timestamp)
    {
        if (taps.Count > 0 && timestamp <= taps.Last())
        {
            throw new ArgumentOutOfRangeException(nameof(timestamp), "Tap timestamps must increase monotonically.");
        }

        if (taps.Count > 0 && timestamp - taps.Last() > ResetAfter)
        {
            taps.Clear();
        }

        taps.Enqueue(timestamp);
        while (taps.Count > MaximumTapCount)
        {
            taps.Dequeue();
        }

        if (taps.Count < 2)
        {
            return null;
        }

        var values = taps.ToArray();
        var intervals = values
            .Zip(values.Skip(1), (first, second) => (second - first).TotalMilliseconds)
            .OrderBy(value => value)
            .ToArray();
        var middle = intervals.Length / 2;
        var medianMilliseconds = intervals.Length % 2 == 0
            ? (intervals[middle - 1] + intervals[middle]) / 2
            : intervals[middle];
        return Math.Clamp(60_000 / medianMilliseconds, 30, 300);
    }

    public void Reset() => taps.Clear();
}
