namespace GWGUI.Emulation.Atari.Exceptions;



public sealed class AtariEmulationException : Exception
{
    public AtariEmulationException(AtariErrorCategory category, AtariErrorCode code, string message,
        IReadOnlyDictionary<string, string>? context = null, Exception? innerException = null)
        : base(message, innerException)
    {
        Category = category;
        Code = code;
        Context = context ?? new Dictionary<string, string>();
    }

    public AtariErrorCategory Category { get; }
    public AtariErrorCode Code { get; }
    public IReadOnlyDictionary<string, string> Context { get; }
}
