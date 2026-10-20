namespace GWGUI.Emulation.Exceptions;

public abstract class EmulationException<TCategory, TCode> : Exception
{
    protected EmulationException(TCategory category, TCode code, string message,
        IReadOnlyDictionary<string, string>? context = null, Exception? innerException = null,
        bool isLocalized = false)
        : base(message, innerException)
    {
        Category = category;
        Code = code;
        Context = context ?? new Dictionary<string, string>();
        IsLocalized = isLocalized;
    }

    public TCategory Category { get; }
    public TCode Code { get; }
    public IReadOnlyDictionary<string, string> Context { get; }
    public bool IsLocalized { get; }
}
