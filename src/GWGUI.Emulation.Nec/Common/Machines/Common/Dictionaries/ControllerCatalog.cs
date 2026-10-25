using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngine.Constants;
using GWGUI.Emulation.Nec.Common.Machines.CoreGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.SuperGrafx.Constants;
using GWGUI.Emulation.Nec.Common.Machines.TurboExpress.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineLt.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcFx.Constants;
namespace GWGUI.Emulation.Nec.Common.Machines.Common.Dictionaries;

public static class ControllerCatalog
{
    private static readonly PeripheralCategory[] PcEngineControllers =
    [
        PeripheralCategory.PcEnginePad, PeripheralCategory.PcEngineTurboPad,
        PeripheralCategory.CoreGrafxTurboPad, PeripheralCategory.CoreGrafxIITurboPad,
        PeripheralCategory.DuoRTurboPad, PeripheralCategory.PcEngineTurboPadII,
        PeripheralCategory.PcEngineTurboStick, PeripheralCategory.AvenuePad3,
        PeripheralCategory.AvenuePad6, PeripheralCategory.ArcadePad6,
        PeripheralCategory.CordlessPad, PeripheralCategory.TurboGrafxTurboPad,
        PeripheralCategory.TurboGrafxTurboStick, PeripheralCategory.DuoPad,
        PeripheralCategory.PcEngineMouse
    ];

    public static IReadOnlyList<PeripheralCategory> Types(Model model)
    {
        var types = model.Id switch
        {
            PcFxMachineConstants.Id => new List<PeripheralCategory> { PeripheralCategory.PcFxPad, PeripheralCategory.PcFxMouse },
            TurboExpressMachineConstants.Id => new List<PeripheralCategory> { PeripheralCategory.TurboExpressControls },
            PcEngineLtMachineConstants.Id => new List<PeripheralCategory> { PeripheralCategory.PcEngineLtControls }.Concat(PcEngineControllers.Where(type => !IsMouse(type))).ToList(),
            SuperGrafxMachineConstants.Id => new List<PeripheralCategory>(PcEngineControllers.Where(type => type != PeripheralCategory.PcEngineMouse)),
            _ => new List<PeripheralCategory>(PcEngineControllers)
        };
        types.Add(PeripheralCategory.None);
        return types;
    }

    public static PeripheralCategory Default(Model model) =>
        model.Id switch
        {
            PcFxMachineConstants.Id => PeripheralCategory.PcFxPad,
            TurboExpressMachineConstants.Id => PeripheralCategory.TurboExpressControls,
            PcEngineLtMachineConstants.Id => PeripheralCategory.PcEngineLtControls,
            PcEngineDuoMachineConstants.Id => PeripheralCategory.DuoPad,
            CoreGrafxMachineConstants.Id => PeripheralCategory.CoreGrafxTurboPad,
            SuperGrafxMachineConstants.Id => PeripheralCategory.PcEngineTurboPad,
            _ => PeripheralCategory.PcEnginePad
        };

    public static PeripheralCategory Normalize(Model model, PeripheralCategory type) =>
        type switch
        {
            PeripheralCategory.Mouse when model.Id == PcFxMachineConstants.Id => PeripheralCategory.PcFxMouse,
            PeripheralCategory.Mouse when model.MouseButtonCount
                > InputSettingsFunctionsConstants.FirstPortIndex => PeripheralCategory.PcEngineMouse,
            PeripheralCategory.Joystick => Default(model),
            _ => Types(model).Contains(type) ? type : Default(model)
        };

    public static bool IsMouse(PeripheralCategory type) => type is PeripheralCategory.Mouse or PeripheralCategory.PcEngineMouse or PeripheralCategory.PcFxMouse;

    public static bool HasSixButtons(PeripheralCategory type) => type is PeripheralCategory.AvenuePad6 or PeripheralCategory.ArcadePad6 or PeripheralCategory.PcFxPad;

    public static IReadOnlyList<PeripheralCategory> ParallelPortTypes { get; } =
        [PeripheralCategory.Joystick, PeripheralCategory.None];
}
