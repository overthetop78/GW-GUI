namespace GWGUI.App.Dictionaries.Options;

internal static class LogActionDefinitions
{
    internal static readonly (string Action, string LabelKey)[] All =
    [
        ("read", "Tab.Read"), ("write", "Tab.Write"), ("convert", "Tab.Convert"),
        ("erase", "Options.LogActionErase"), ("clean", "Options.LogActionClean"),
        ("info", "Tool.Title.Info"), ("bandwidth", "Tool.Title.Bandwidth"),
        ("rpm", "Tool.Title.Rpm"), ("seek", "Tool.Title.Seek"), ("pin", "Tool.Title.Pin"),
        ("reset", "Tool.Title.Reset"), ("delays", "Tool.Title.Delays"),
        ("update", "Tool.Title.Update"), ("align", "Tool.Title.Align")
    ];
}
