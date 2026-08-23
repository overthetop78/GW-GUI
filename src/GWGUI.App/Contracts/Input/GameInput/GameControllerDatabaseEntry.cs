namespace GWGUI.App.Services.Input.GameInput;

internal sealed record GameControllerDatabaseEntry(
    string Name,
    IReadOnlyDictionary<string, string> Mappings);
