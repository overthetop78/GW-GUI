namespace GWGUI.App.Controls;

internal static class AtariControllerSettingsConstants
{
    internal static readonly IReadOnlyList<string> DirectionActions = ["Up", "Down", "Left", "Right"];
    internal static readonly IReadOnlyList<string> SingleFireActions = ["Fire1"];
    internal static readonly IReadOnlyList<string> DualFireActions = ["Fire1", "Fire2"];
    internal static readonly IReadOnlyList<string> HatariFireActions = ["Fire1", "Turbo"];
    internal static readonly IReadOnlyList<string> LynxActions = ["Fire1", "Fire2", "Option1", "Option2", "Pause"];
    internal static readonly IReadOnlyList<string> KeypadActions =
        ["Start", "Pause", "Reset", "Key0", "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "Key7", "Key8", "Key9", "Star", "Hash"];
    internal static readonly IReadOnlyList<string> JaguarActions =
        ["A", "B", "C", "Option", "Pause", "Key0", "Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "Key7", "Key8", "Key9", "Star", "Hash"];
    internal static readonly IReadOnlyDictionary<string, string> DefaultSources =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Up"] = "DPadUp", ["Down"] = "DPadDown", ["Left"] = "DPadLeft", ["Right"] = "DPadRight",
            ["Fire1"] = "ButtonA", ["Fire2"] = "ButtonB", ["Turbo"] = "ButtonX",
            ["A"] = "ButtonA", ["B"] = "ButtonB", ["C"] = "ButtonX",
            ["Option"] = "View", ["Option1"] = "LeftShoulder", ["Option2"] = "RightShoulder",
            ["Pause"] = "Menu", ["Start"] = "Menu", ["Reset"] = "View"
        };
}
