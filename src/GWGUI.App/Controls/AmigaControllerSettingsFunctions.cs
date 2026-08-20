using GWGUI.App.Localization;
using GWGUI.Emulation.Amiga;

namespace GWGUI.App.Controls;

internal static class AmigaControllerSettingsFunctions
{
    internal static string Label(AmigaControllerType type) => type switch
    {
        AmigaControllerType.Automatic => LocExtension.Get("Emulation.Controller.Automatic"),
        AmigaControllerType.Joystick => LocExtension.Get("Emulation.Amiga.Controller.Joystick"),
        AmigaControllerType.AnalogJoystick => LocExtension.Get("Emulation.Controller.AnalogJoystick"),
        AmigaControllerType.Cd32Pad => LocExtension.Get("Emulation.Amiga.Controller.Cd32"),
        AmigaControllerType.None => LocExtension.Get("Emulation.Controller.None"),
        _ => type.ToString()
    };

    internal static IReadOnlyList<InputBindingDefinition> Definitions(AmigaControllerType type)
    {
        if (type is AmigaControllerType.None or AmigaControllerType.Keyboard) return [];
        var definitions = new List<InputBindingDefinition>
        {
            new("Up", LocExtension.Get("Emulation.Controller.Action.Up"), string.Empty),
            new("Down", LocExtension.Get("Emulation.Controller.Action.Down"), string.Empty),
            new("Left", LocExtension.Get("Emulation.Controller.Action.Left"), string.Empty),
            new("Right", LocExtension.Get("Emulation.Controller.Action.Right"), string.Empty)
        };
        if (type == AmigaControllerType.Cd32Pad)
        {
            definitions.AddRange([
                new("B", LocExtension.Get("Emulation.Amiga.Controller.Cd32.Red"), string.Empty),
                new("A", LocExtension.Get("Emulation.Amiga.Controller.Cd32.Blue"), string.Empty),
                new("Y", LocExtension.Get("Emulation.Amiga.Controller.Cd32.Green"), string.Empty),
                new("X", LocExtension.Get("Emulation.Amiga.Controller.Cd32.Yellow"), string.Empty),
                new("L", LocExtension.Get("Emulation.Amiga.Controller.Cd32.Rewind"), string.Empty),
                new("R", LocExtension.Get("Emulation.Amiga.Controller.Cd32.FastForward"), string.Empty),
                new("Start", LocExtension.Get("Emulation.Amiga.Controller.Cd32.PlayPause"), string.Empty)
            ]);
        }
        else
        {
            definitions.Add(new InputBindingDefinition("B", LocExtension.Get("Emulation.Controller.Action.Fire1"), string.Empty));
            definitions.Add(new InputBindingDefinition("A", LocExtension.Get("Emulation.Controller.Action.Fire2"), string.Empty));
        }
        definitions.Add(new InputBindingDefinition("L2", LocExtension.Get("Emulation.Controller.Action.TurboFire"), string.Empty));
        return definitions;
    }
}
