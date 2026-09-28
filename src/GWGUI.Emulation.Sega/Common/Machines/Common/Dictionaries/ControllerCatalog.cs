namespace GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;

public static class ControllerCatalog
{
    public static IReadOnlyList<ControllerType> Types(Model model)
    {
        var types = model.Id switch
        {
            ModelConstants.Sg1000 => new[]
            {
                ControllerType.SegaSg1000Joystick, ControllerType.SegaSg1000IiJoypad
            },
            ModelConstants.Sc3000 => new[] { ControllerType.SegaSc3000Keyboard },
            ModelConstants.MarkIII => new[]
            {
                ControllerType.SegaMasterSystemController, ControllerType.SegaControlStick,
                ControllerType.SegaLightPhaser
            },
            ModelConstants.MasterSystem => new[]
            {
                ControllerType.SegaMasterSystemController, ControllerType.SegaControlStick,
                ControllerType.SegaLightPhaser
            },
            ModelConstants.MegaDrive => new[]
            {
                ControllerType.SegaMegaDriveThreeButton, ControllerType.SegaMegaDriveSixButton,
                ControllerType.SegaMegaMouse, ControllerType.SegaMenacer,
                ControllerType.SegaSportsPad, ControllerType.SegaPaddleControl,
                ControllerType.SegaHandleController, ControllerType.SegaArcadePowerStick,
                ControllerType.SegaArcadePowerStickSixButton, ControllerType.SegaXe1Ap,
                ControllerType.SegaActivator
            },
            ModelConstants.GameGear => new[] { ControllerType.SegaGameGearController },
            ModelConstants.Saturn => new[]
            {
                ControllerType.SegaSaturnController, ControllerType.SegaSaturnThreeDControlPad,
                ControllerType.SegaSaturnVirtuaGun, ControllerType.SegaSaturnShuttleMouse,
                ControllerType.SegaSaturnMissionStick, ControllerType.SegaSaturnArcadeRacer,
                ControllerType.SegaSaturnTwinStick, ControllerType.SegaSaturnVirtuaStick
            },
            ModelConstants.Dreamcast => new[]
            {
                ControllerType.SegaDreamcastController, ControllerType.SegaDreamcastMouse,
                ControllerType.SegaDreamcastKeyboard, ControllerType.SegaDreamcastLightGun,
                ControllerType.SegaDreamcastFishingController,
                ControllerType.SegaDreamcastArcadeStick, ControllerType.SegaDreamcastTwinStick,
                ControllerType.SegaDreamcastMaracas
            },
            _ => Array.Empty<ControllerType>()
        };
        var result = new List<ControllerType>(types);
        if (model.ControllerPortCount <= 0) result.Clear();
        result.Add(ControllerType.None);
        return result;
    }

    public static ControllerType Default(Model model) =>
        model.ControllerPortCount > 0
            ? Types(model).FirstOrDefault(type => type != ControllerType.None,
                ControllerType.None)
            : ControllerType.None;

    public static ControllerType Normalize(Model model, ControllerType type) =>
        type != ControllerType.Automatic && Types(model).Contains(type) ? type : Default(model);

    public static IReadOnlyList<ControllerType> ParallelPortTypes { get; } =
        [ControllerType.Joystick, ControllerType.None];
}
