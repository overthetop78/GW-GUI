namespace GWGUI.Emulation.Exceptions;

public sealed class EmulationMessageException : Exception
{
    public EmulationMessageException(EmulationMessage message, Exception? innerException = null)
        : base(null, innerException)
    {
        MessageData = message ?? throw new ArgumentNullException(nameof(message));
    }

    public EmulationMessage MessageData { get; }
}
