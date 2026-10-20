namespace GWGUI.Emulation.Atari.Emulators.Common.Exceptions;

public sealed class EmulationException : GWGUI.Emulation.Exceptions.EmulationException<ErrorCategory, ErrorCode>
{
    public EmulationException(ErrorCategory category, ErrorCode code, string message,
        IReadOnlyDictionary<string, string>? context = null, Exception? innerException = null,
        bool isLocalized = false)
        : base(category, code, message, context, innerException, isLocalized)
    {
    }
}
