using GWGUI.App.Localization;
using GWGUI.Emulation.Amiga;

namespace GWGUI.App.Controls;

internal static class AmigaMouseSettingsFunctions
{
    internal static IReadOnlyList<InputBindingDefinition> Definitions(AmigaModel model)
    {
        var definitions = new List<InputBindingDefinition>
        {
            new(nameof(AmigaMouseAction.LeftButton), LocExtension.Get("Emulation.Mouse.Button.Left"), "Mouse:Left"),
            new(nameof(AmigaMouseAction.RightButton), LocExtension.Get("Emulation.Mouse.Button.Right"), "Mouse:Right")
        };
        if (model.MouseButtonCount >= 3)
            definitions.Add(new InputBindingDefinition(nameof(AmigaMouseAction.MiddleButton),
                LocExtension.Get("Emulation.Mouse.Button.Middle"), "Mouse:Middle"));
        return definitions;
    }
}
