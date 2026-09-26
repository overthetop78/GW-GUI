namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Dictionaries;

public static class ControllerCatalog
{
    public static IReadOnlyList<ControllerType> Types(Model model)
    {
        var types = new List<ControllerType>
        {
            ControllerType.Joystick,
            ControllerType.AnalogJoystick
        };
        if (model.SupportsCd32Controller)
            types.Add(ControllerType.Cd32Pad);
        types.Add(ControllerType.None);
        return types;
    }

    public static ControllerType Default(Model model) =>
        model.SupportsCd32Controller ? ControllerType.Cd32Pad : ControllerType.Joystick;

    public static ControllerType Normalize(Model model, ControllerType type) =>
        type != ControllerType.Automatic && Types(model).Contains(type) ? type : Default(model);

    public static IReadOnlyList<ControllerType> ParallelPortTypes { get; } =
        [ControllerType.Joystick, ControllerType.None];
}
