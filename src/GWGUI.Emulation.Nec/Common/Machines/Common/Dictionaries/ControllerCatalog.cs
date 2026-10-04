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
    private static readonly ControllerType[] PcEngineControllers =
    [
        ControllerType.PcEnginePad, ControllerType.PcEngineTurboPad,
        ControllerType.CoreGrafxTurboPad, ControllerType.CoreGrafxIITurboPad,
        ControllerType.DuoRTurboPad, ControllerType.PcEngineTurboPadII,
        ControllerType.PcEngineTurboStick, ControllerType.AvenuePad3,
        ControllerType.AvenuePad6, ControllerType.ArcadePad6,
        ControllerType.CordlessPad, ControllerType.TurboGrafxTurboPad,
        ControllerType.TurboGrafxTurboStick, ControllerType.DuoPad,
        ControllerType.PcEngineMouse
    ];

    public static IReadOnlyList<ControllerType> Types(Model model)
    {
        var types = model.Id switch
        {
            PcFxMachineConstants.Id => new List<ControllerType> { ControllerType.PcFxPad, ControllerType.PcFxMouse },
            TurboExpressMachineConstants.Id => new List<ControllerType> { ControllerType.TurboExpressControls },
            PcEngineLtMachineConstants.Id => new List<ControllerType> { ControllerType.PcEngineLtControls }
                .Concat(PcEngineControllers.Where(type => !IsMouse(type))).ToList(),
            SuperGrafxMachineConstants.Id => new List<ControllerType>(PcEngineControllers.Where(type =>
                type != ControllerType.PcEngineMouse)),
            _ => new List<ControllerType>(PcEngineControllers)
        };
        types.Add(ControllerType.None);
        return types;
    }

    public static ControllerType Default(Model model) =>
        model.Id switch
        {
            PcFxMachineConstants.Id => ControllerType.PcFxPad,
            TurboExpressMachineConstants.Id => ControllerType.TurboExpressControls,
            PcEngineLtMachineConstants.Id => ControllerType.PcEngineLtControls,
            PcEngineDuoMachineConstants.Id => ControllerType.DuoPad,
            CoreGrafxMachineConstants.Id => ControllerType.CoreGrafxTurboPad,
            SuperGrafxMachineConstants.Id => ControllerType.PcEngineTurboPad,
            _ => ControllerType.PcEnginePad
        };

    public static ControllerType Normalize(Model model, ControllerType type) =>
        type switch
        {
            ControllerType.Mouse when model.Id == PcFxMachineConstants.Id => ControllerType.PcFxMouse,
            ControllerType.Mouse when model.MouseButtonCount
                > InputSettingsFunctionsConstants.FirstPortIndex => ControllerType.PcEngineMouse,
            ControllerType.Joystick => Default(model),
            _ => Types(model).Contains(type) ? type : Default(model)
        };

    public static bool IsMouse(ControllerType type) =>
        type is ControllerType.Mouse or ControllerType.PcEngineMouse or ControllerType.PcFxMouse;

    public static bool HasSixButtons(ControllerType type) =>
        type is ControllerType.AvenuePad6 or ControllerType.ArcadePad6 or ControllerType.PcFxPad;

    public static IReadOnlyList<ControllerType> ParallelPortTypes { get; } =
        [ControllerType.Joystick, ControllerType.None];
}
