using GWGUI.App.Localization;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

internal static class AmigaKeyboardSettingsFunctions
{
    internal static IReadOnlyList<InputBindingDefinition> Definitions()
    {
        var keys = new[]
        {
            EmulationKey.F1, EmulationKey.F2, EmulationKey.F3, EmulationKey.F4, EmulationKey.F5,
            EmulationKey.F6, EmulationKey.F7, EmulationKey.F8, EmulationKey.F9, EmulationKey.F10
        };
        var definitions = keys.Select(key => new InputBindingDefinition(key.ToString(), key.ToString(), key.ToString())).ToList();
        definitions.Add(new InputBindingDefinition(nameof(EmulationKey.Help), LocExtension.Get("Emulation.Key.Help"), "Insert"));
        definitions.Add(new InputBindingDefinition(nameof(EmulationKey.LeftAmiga), LocExtension.Get("Emulation.Key.LeftAmiga"), "PageUp"));
        definitions.Add(new InputBindingDefinition(nameof(EmulationKey.RightAmiga), LocExtension.Get("Emulation.Key.RightAmiga"), "PageDown"));
        return definitions;
    }
}
