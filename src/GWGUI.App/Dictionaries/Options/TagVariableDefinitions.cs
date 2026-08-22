namespace GWGUI.App.Dictionaries.Options;

internal static class TagVariableDefinitions
{
    internal static readonly (string Token, string Key)[] All =
    [
        ("{NAME}", "Options.TagVariableName"), ("{FAMILY}", "Options.TagVariableFamily"),
        ("{FORMAT}", "Options.TagVariableFormat"), ("{EXTENSION}", "Options.TagVariableExtension"),
        ("{DATE:YYYY-MM-DD}", "Options.TagVariableDateIso"),
        ("{DATE:YYYYMMDD}", "Options.TagVariableDateCompact"),
        ("{DATE:DD-MM-YYYY}", "Options.TagVariableDateLocal"),
        ("{TIME:HH-MM-SS}", "Options.TagVariableTimeFull"),
        ("{TIME:HHMMSS}", "Options.TagVariableTimeCompact"),
        ("{TIME:HH-MM}", "Options.TagVariableTimeShort")
    ];
}
