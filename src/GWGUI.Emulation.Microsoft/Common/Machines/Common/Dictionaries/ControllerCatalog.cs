namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Dictionaries;

public static class ControllerCatalog
{
    public static IReadOnlyList<PeripheralCategory> Types(Model model)
    {
        var types = new List<PeripheralCategory>();
        if (model.ControllerPortCount > 0)
            types.Add(PeripheralCategory.Joystick);
        types.Add(PeripheralCategory.None);
        return types;
    }

    public static PeripheralCategory Default(Model model) => model.ControllerPortCount > 0 ? PeripheralCategory.Joystick : PeripheralCategory.None;

    public static PeripheralCategory Normalize(Model model, PeripheralCategory type) => type != PeripheralCategory.Automatic && Types(model).Contains(type) ? type : Default(model);

    public static IReadOnlyList<PeripheralCategory> ParallelPortTypes { get; } =
        [PeripheralCategory.Joystick, PeripheralCategory.None];
}
