namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;

public static class ControllerCatalog
{
    public static IReadOnlyList<ControllerType> Types(Model model)
    {
        var types = new List<ControllerType>();
        if (model.ControllerPortCount > 0)
            types.Add(ControllerType.Joystick);
        types.Add(ControllerType.None);
        return types;
    }

    public static ControllerType Default(Model model) =>
        model.ControllerPortCount > 0
            ? ControllerType.Joystick
            : ControllerType.None;

    public static ControllerType Normalize(Model model, ControllerType type) =>
        type != ControllerType.Automatic && Types(model).Contains(type) ? type : Default(model);

    public static IReadOnlyList<ControllerType> ParallelPortTypes { get; } =
        [ControllerType.Joystick, ControllerType.None];
}
