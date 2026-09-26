namespace GWGUI.Emulation.Atari.Emulators.Libretro.Exceptions;



public sealed class EmulationException : Exception
{
    public EmulationException(ErrorCategory category, ErrorCode code, string message,
        IReadOnlyDictionary<string, string>? context = null, Exception? innerException = null,
        bool isLocalized = false)
        : base(message, innerException)
    {
        Category = category;
        Code = code;
        Context = context ?? new Dictionary<string, string>();
        IsLocalized = isLocalized;
    }

    public ErrorCategory Category { get; }
    public ErrorCode Code { get; }
    public IReadOnlyDictionary<string, string> Context { get; }
    public bool IsLocalized { get; }
}
