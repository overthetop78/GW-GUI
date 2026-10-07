namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Dictionaries;

public static class ControllerCatalog
{
    public static IReadOnlyList<ControllerType> Types(Model model) => model.ControllerTypes;

    public static ControllerType Default(Model model) => model.ControllerTypes[BufferConstants.FirstCollectionIndex];

    public static ControllerType Normalize(Model model, ControllerType type) =>
        type != ControllerType.Automatic && Types(model).Contains(type) ? type : Default(model);

    public static IReadOnlyList<ControllerType> ParallelPortTypes { get; } =
        [ControllerType.Joystick, ControllerType.None];
}
