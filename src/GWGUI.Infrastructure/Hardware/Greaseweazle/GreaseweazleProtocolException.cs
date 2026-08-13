namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public sealed class GreaseweazleProtocolException(
    GreaseweazleCommand command,
    GreaseweazleAcknowledgement acknowledgement)
    : IOException($"Greaseweazle command {command} failed: {acknowledgement}.")
{
    public GreaseweazleCommand Command { get; } = command;

    public GreaseweazleAcknowledgement Acknowledgement { get; } = acknowledgement;
}
