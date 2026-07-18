using System.Diagnostics;

namespace MaskApp.Core.Features.Animations;

public interface IAnimationClock
{
    long GetTimestamp();

    long Add(long timestamp, TimeSpan duration);

    TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp);

    Task DelayUntilAsync(long deadlineTimestamp, CancellationToken cancellationToken);
}

public sealed class MonotonicAnimationClock : IAnimationClock
{
    public long GetTimestamp() => Stopwatch.GetTimestamp();

    public long Add(long timestamp, TimeSpan duration) =>
        checked(timestamp + (long)Math.Round(duration.TotalSeconds * Stopwatch.Frequency));

    public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp) =>
        Stopwatch.GetElapsedTime(startingTimestamp, endingTimestamp);

    public async Task DelayUntilAsync(long deadlineTimestamp, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = GetElapsedTime(GetTimestamp(), deadlineTimestamp);
            if (remaining <= TimeSpan.Zero)
            {
                return;
            }

            await Task.Delay(remaining, cancellationToken).ConfigureAwait(false);
        }
    }
}
