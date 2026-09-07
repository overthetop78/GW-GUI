using GWGUI.App.Functions.Emulation.Storage;
using GWGUI.Emulation.Contracts;

namespace GWGUI.Tests.Emulation;

public sealed class EmulationMediaDialogFunctionsTests
{
    [Fact]
    public void ClientHistoryIsStableAndIndependentPerModuleMachineAndSlot()
    {
        var cartridge = EmulationMediaDialogFunctions.ClientGuid(
            "atari", "atari-jaguar-cd", EmulationMediaSlot.Cartridge0);

        Assert.Equal(cartridge, EmulationMediaDialogFunctions.ClientGuid(
            "atari", "atari-jaguar-cd", EmulationMediaSlot.Cartridge0));
        Assert.NotEqual(cartridge, EmulationMediaDialogFunctions.ClientGuid(
            "atari", "atari-jaguar-cd", EmulationMediaSlot.Cd0));
        Assert.NotEqual(cartridge, EmulationMediaDialogFunctions.ClientGuid(
            "atari", "atari-jaguar", EmulationMediaSlot.Cartridge0));
        Assert.NotEqual(cartridge, EmulationMediaDialogFunctions.ClientGuid(
            "another-module", "atari-jaguar-cd", EmulationMediaSlot.Cartridge0));
    }
}
