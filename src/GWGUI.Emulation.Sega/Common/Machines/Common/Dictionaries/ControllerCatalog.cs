namespace GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;

public static class ControllerCatalog
{
    public static IReadOnlyList<PeripheralCategory> Types(Model model)
    {
        var types = model.Id switch
        {
            ModelConstants.Sg1000 => new[]
            {
                PeripheralCategory.SegaSg1000Joystick, PeripheralCategory.SegaSg1000IiJoypad
            },
            ModelConstants.Sc3000 or ModelConstants.Sf7000 => new[] { PeripheralCategory.SegaSc3000Keyboard },
            ModelConstants.MarkIII => new[]
            {
                PeripheralCategory.SegaMasterSystemController, PeripheralCategory.SegaControlStick,
                PeripheralCategory.SegaLightPhaser
            },
            ModelConstants.MasterSystem => new[]
            {
                PeripheralCategory.SegaMasterSystemController, PeripheralCategory.SegaControlStick,
                PeripheralCategory.SegaLightPhaser
            },
            ModelConstants.MegaDrive or ModelConstants.MegaCd or ModelConstants.ThirtyTwoX => new[]
            {
                PeripheralCategory.SegaMegaDriveThreeButton, PeripheralCategory.SegaMegaDriveSixButton,
                PeripheralCategory.SegaMegaMouse, PeripheralCategory.SegaMenacer,
                PeripheralCategory.SegaSportsPad, PeripheralCategory.SegaPaddleControl,
                PeripheralCategory.SegaHandleController, PeripheralCategory.SegaArcadePowerStick,
                PeripheralCategory.SegaArcadePowerStickSixButton, PeripheralCategory.SegaXe1Ap,
                PeripheralCategory.SegaActivator
            },
            ModelConstants.GameGear => new[] { PeripheralCategory.SegaGameGearController },
            ModelConstants.Saturn or ModelConstants.StV => new[]
            {
                PeripheralCategory.SegaSaturnController, PeripheralCategory.SegaSaturnThreeDControlPad,
                PeripheralCategory.SegaSaturnVirtuaGun, PeripheralCategory.SegaSaturnShuttleMouse,
                PeripheralCategory.SegaSaturnMissionStick, PeripheralCategory.SegaSaturnArcadeRacer,
                PeripheralCategory.SegaSaturnTwinStick, PeripheralCategory.SegaSaturnVirtuaStick
            },
            ModelConstants.Dreamcast => new[]
            {
                PeripheralCategory.SegaDreamcastController, PeripheralCategory.SegaDreamcastMouse,
                PeripheralCategory.SegaDreamcastKeyboard, PeripheralCategory.SegaDreamcastLightGun,
                PeripheralCategory.SegaDreamcastFishingController,
                PeripheralCategory.SegaDreamcastArcadeStick, PeripheralCategory.SegaDreamcastTwinStick,
                PeripheralCategory.SegaDreamcastMaracas
            },
            ModelConstants.Naomi or ModelConstants.Naomi2 or ModelConstants.Atomiswave or ModelConstants.SystemSp or ModelConstants.Model3 => new[]
            {
                PeripheralCategory.SegaDreamcastController, PeripheralCategory.SegaDreamcastLightGun,
                PeripheralCategory.SegaDreamcastArcadeStick
            },
            _ => Array.Empty<PeripheralCategory>()
        };
        var result = new List<PeripheralCategory>(types);
        if (model.ControllerPortCount <= 0) result.Clear();
        result.Add(PeripheralCategory.None);
        return result;
    }

    public static PeripheralCategory Default(Model model) => model.ControllerPortCount > 0 ? Types(model).FirstOrDefault(type => type != PeripheralCategory.None, PeripheralCategory.None) : PeripheralCategory.None;

    public static PeripheralCategory Normalize(Model model, PeripheralCategory type) => type != PeripheralCategory.Automatic && Types(model).Contains(type) ? type : Default(model);

    public static IReadOnlyList<PeripheralCategory> ParallelPortTypes { get; } =
        [PeripheralCategory.Joypad, PeripheralCategory.None];
}
