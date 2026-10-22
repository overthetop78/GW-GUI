namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Dictionaries;

public static class ControllerCatalog
{
    public static IReadOnlyList<PeripheralCategory> Types(Model model) => model.ControllerTypes;

    public static PeripheralCategory Default(Model model) => model.ControllerTypes[BufferConstants.FirstCollectionIndex];

    public static PeripheralCategory Normalize(Model model, PeripheralCategory type) => type != PeripheralCategory.Automatic && Types(model).Contains(type) ? type : Default(model);

    public static IReadOnlyList<PeripheralCategory> ParallelPortTypes { get; } =
        [PeripheralCategory.Joystick, PeripheralCategory.None];
}
