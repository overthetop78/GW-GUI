namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Dictionaries;

internal sealed record NativeInputAction(ControllerCommand Command)
{
    internal string Id => Command.ToString();
    internal uint ButtonMask => 1u << (int)Command;
    internal string DefaultBinding => string.Empty;
    internal InputBindingDefinition Definition => new(Id, Command is ControllerCommand.Up or ControllerCommand.Down or ControllerCommand.Left or ControllerCommand.Right ? "Emulation.Controller.Action." + Id : InputMappingConstants.ResourcePrefix + Id, DefaultBinding);
}

internal static class ControllerProtocolDictionary
{
    private static readonly IReadOnlyList<NativeInputAction> Definitions = Array.AsReadOnly(Enum.GetValues<ControllerCommand>().Select(command => new NativeInputAction(command)).ToArray());

    internal static IReadOnlyList<NativeInputAction> Actions(MachineModel model, PeripheralCategory peripheral)
    {
        _ = ConfigurationFunctions.Family(model);
        return peripheral == PeripheralCategory.Intellivision ? Definitions : throw new ArgumentOutOfRangeException(nameof(peripheral));
    }
}
