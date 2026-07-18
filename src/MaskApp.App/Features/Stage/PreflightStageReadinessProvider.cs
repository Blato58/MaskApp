using MaskApp.Core.Features.Preflight;
using MaskApp.Core.Features.Stage;

namespace MaskApp.App.Features.Stage;

public sealed class PreflightStageReadinessProvider : IStageReadinessProvider
{
    private readonly FestivalPreflightViewModel preflightViewModel;

    public PreflightStageReadinessProvider(FestivalPreflightViewModel preflightViewModel)
    {
        this.preflightViewModel = preflightViewModel;
    }

    public async Task<StageReadinessSnapshot> EvaluateAsync(
        CancellationToken cancellationToken = default)
    {
        await preflightViewModel.InitializeForStageAsync(cancellationToken);
        var report = preflightViewModel.CurrentReport;
        if (report is null)
        {
            return StageReadinessSnapshot.NotReady(preflightViewModel.SummaryText);
        }

        return new StageReadinessSnapshot(
            report.Status,
            report.StatusText,
            preflightViewModel.SummaryText,
            report.Issues.Count(issue => issue.Severity == PreflightIssueSeverity.Blocking),
            report.Issues.Count(issue => issue.Severity == PreflightIssueSeverity.Warning));
    }
}
