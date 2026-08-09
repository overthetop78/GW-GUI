namespace GWGUI.App.Options;

internal static class OptionsDefinitions
{
    internal static readonly (string Action, string LabelKey)[] LogActions =
    [
        ("read", "Tab.Read"), ("write", "Tab.Write"), ("convert", "Tab.Convert"),
        ("erase", "Options.LogActionErase"), ("clean", "Options.LogActionClean"),
        ("info", "Tool.Title.Info"), ("bandwidth", "Tool.Title.Bandwidth"), ("rpm", "Tool.Title.Rpm"),
        ("seek", "Tool.Title.Seek"), ("pin", "Tool.Title.Pin"), ("reset", "Tool.Title.Reset"),
        ("delays", "Tool.Title.Delays"), ("update", "Tool.Title.Update"), ("align", "Tool.Title.Align")
    ];

    internal static readonly (string Key, string Pattern)[] TagPresets =
    [
        ("Options.TagPresetFamily", "[{FAMILY}] "),
        ("Options.TagPresetFormat", "[{FORMAT}] "),
        ("Options.TagPresetFamilyFormat", "[{FAMILY}-{FORMAT}] "),
        ("Options.TagPresetFamilyExtension", "[{FAMILY}-{EXTENSION}] "),
        ("Options.TagPresetDetailed", "[{FAMILY}-{FORMAT}-{EXTENSION}] ")
    ];

    internal static readonly (string Token, string Key)[] TagVariables =
    [
        ("{NAME}", "Options.TagVariableName"), ("{FAMILY}", "Options.TagVariableFamily"),
        ("{FORMAT}", "Options.TagVariableFormat"), ("{EXTENSION}", "Options.TagVariableExtension"),
        ("{DATE:YYYY-MM-DD}", "Options.TagVariableDateIso"), ("{DATE:YYYYMMDD}", "Options.TagVariableDateCompact"),
        ("{DATE:DD-MM-YYYY}", "Options.TagVariableDateLocal"), ("{TIME:HH-MM-SS}", "Options.TagVariableTimeFull"),
        ("{TIME:HHMMSS}", "Options.TagVariableTimeCompact"), ("{TIME:HH-MM}", "Options.TagVariableTimeShort")
    ];
}
