namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Constants;

internal static class KeyboardActionConstants
{
    internal const string Smart1 = "Smart1", Smart2 = "Smart2", Smart3 = "Smart3";
    internal const string Smart4 = "Smart4", Smart5 = "Smart5", Smart6 = "Smart6";
    internal const string Undo = "Undo", WildCard = "WildCard", Move = "Move", Store = "Store", Clear = "Clear";
    internal const string Print = "Print";
    internal static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Smart1, Smart2, Smart3, Smart4, Smart5, Smart6, Undo, WildCard, Move, Store, Clear, Print
    };
}
