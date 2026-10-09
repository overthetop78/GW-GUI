namespace GWGUI.Emulation.Sony.Common.Machines.Common.Dictionaries;

public static class ControllerCatalog
{
    public static IReadOnlyList<ControllerType> Types(MachineConfiguration configuration) =>
        ControllerFunctions.Supported(configuration);

    public static ControllerType Default(MachineConfiguration configuration) =>
        Types(configuration).FirstOrDefault(type => type != ControllerType.None,
            ControllerType.None);

}
