using System.Collections.ObjectModel;

namespace MaskApp.Core.Features.MaskControl;

public sealed class SimulatedMaskCommandTransport : IMaskCommandTransport
{
    public event EventHandler<MaskCommandTransportStateChangedEventArgs>? TransportStateChanged
    {
        add { }
        remove { }
    }

    public MaskCommandTransportState TransportState => MaskCommandTransportState.Ready;

    public string TransportDisplayName => "Simulator";

    public bool IsSimulated => true;

    public string TransportStatusText => "Simulator ready.";

    public ObservableCollection<MaskCommand> SentCommands { get; } = [];

    public Task<MaskCommandResult> SendAsync(MaskCommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SentCommands.Add(command);
        return Task.FromResult(MaskCommandResult.Success($"Simulated {command.DisplayName}."));
    }
}
