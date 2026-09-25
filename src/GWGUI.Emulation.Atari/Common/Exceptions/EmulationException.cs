namespace GWGUI.Emulation.Atari.Common.Exceptions;



public sealed class EmulationException : Exception
{
    public EmulationException(ErrorCategory category, ErrorCode code, string message,
        IReadOnlyDictionary<string, string>? context = null, Exception? innerException = null)
        : base(message, innerException)
    {
        Category = category;
        Code = code;
        Context = context ?? new Dictionary<string, string>();
    }

    public ErrorCategory Category { get; }
    public ErrorCode Code { get; }
    public IReadOnlyDictionary<string, string> Context { get; }
}
