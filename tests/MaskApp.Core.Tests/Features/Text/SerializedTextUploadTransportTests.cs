using MaskApp.Core.Features.Text;

namespace MaskApp.Core.Tests.Features.Text;

public sealed class SerializedTextUploadTransportTests
{
    [Fact]
    public async Task UploadAsync_SerializesConcurrentUploads()
    {
        var inner = new BlockingTextUploadTransport();
        var transport = new SerializedTextUploadTransport(inner);
        var options = new TextUploadOptions
        {
            ResetDisplayBeforeUpload = false,
            PostUploadQuietPeriod = TimeSpan.Zero
        };
        var firstPackage = TextUploadProtocol.CreatePackage("FIRST", new TextLedColor(1, 2, 3), mode: 2, speed: 100);
        var secondPackage = TextUploadProtocol.CreatePackage("SECOND", new TextLedColor(1, 2, 3), mode: 2, speed: 100);

        var firstUpload = transport.UploadAsync(firstPackage, options);
        await inner.WaitForStartedAsync(uploadCount: 1);

        var secondUpload = transport.UploadAsync(secondPackage, options);
        await Task.Delay(50);

        Assert.Equal(1, inner.StartedCount);

        inner.CompleteNext();
        await inner.WaitForStartedAsync(uploadCount: 2);
        inner.CompleteNext();

        var results = await Task.WhenAll(firstUpload, secondUpload);

        Assert.All(results, result => Assert.True(result.Succeeded));
        Assert.Equal(1, inner.MaxConcurrentUploads);
        Assert.Equal(new[] { "FIRST", "SECOND" }, inner.UploadedTexts);
    }

    private sealed class BlockingTextUploadTransport : ITextUploadTransport
    {
        private readonly object sync = new();
        private readonly Queue<TaskCompletionSource<TextUploadResult>> completions = [];
        private readonly TaskCompletionSource firstStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource secondStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly List<string> uploadedTexts = [];
        private int activeUploads;
        private int maxConcurrentUploads;
        private int startedCount;

        public event EventHandler<TextUploadTransportStateChangedEventArgs>? StateChanged
        {
            add { }
            remove { }
        }

        public string TransportDisplayName => "Blocking fake";

        public bool IsSimulated => true;

        public bool IsReady => true;

        public bool SupportsAcknowledgements => true;

        public TextUploadTransportState State => TextUploadTransportState.Simulated;

        public string StatusText => "Ready.";

        public int StartedCount
        {
            get
            {
                lock (sync)
                {
                    return startedCount;
                }
            }
        }

        public int MaxConcurrentUploads
        {
            get
            {
                lock (sync)
                {
                    return maxConcurrentUploads;
                }
            }
        }

        public IReadOnlyList<string> UploadedTexts
        {
            get
            {
                lock (sync)
                {
                    return [.. uploadedTexts];
                }
            }
        }

        public Task WaitForStartedAsync(int uploadCount) =>
            uploadCount switch
            {
                1 => firstStarted.Task,
                2 => secondStarted.Task,
                _ => throw new ArgumentOutOfRangeException(nameof(uploadCount), "Only the first two uploads are tracked.")
            };

        public Task<TextUploadResult> UploadAsync(
            TextUploadPackage package,
            TextUploadOptions options,
            CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<TextUploadResult> completion;
            lock (sync)
            {
                completion = new TaskCompletionSource<TextUploadResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                completions.Enqueue(completion);
                uploadedTexts.Add(package.Text);
                activeUploads++;
                maxConcurrentUploads = Math.Max(maxConcurrentUploads, activeUploads);
                startedCount++;

                if (startedCount == 1)
                {
                    firstStarted.SetResult();
                }
                else if (startedCount == 2)
                {
                    secondStarted.SetResult();
                }
            }

            return CompleteUploadAsync(completion, package.Frames.Count);
        }

        public void CompleteNext()
        {
            TaskCompletionSource<TextUploadResult> completion;
            lock (sync)
            {
                completion = completions.Dequeue();
            }

            completion.SetResult(TextUploadResult.Success("Uploaded.", framesSent: 0));
        }

        private async Task<TextUploadResult> CompleteUploadAsync(
            TaskCompletionSource<TextUploadResult> completion,
            int framesSent)
        {
            try
            {
                await completion.Task;
                return TextUploadResult.Success("Uploaded.", framesSent);
            }
            finally
            {
                lock (sync)
                {
                    activeUploads--;
                }
            }
        }
    }
}
