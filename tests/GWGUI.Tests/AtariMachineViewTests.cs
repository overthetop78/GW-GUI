using GWGUI.App.Contracts.Machine;
using GWGUI.Emulation;

namespace GWGUI.Tests;

public sealed class AtariMachineViewTests
{
    [Fact]
    public void DeviceViewUsesTheStructuredCommonMediaSlot()
    {
        var slot = new EmulationMediaSlot(EmulationMediaCategory.FloppyDrive, 0);
        var view = new MachineViewDevice(slot.ToString(), "A:", "glyph", true, true, null, null);

        Assert.Equal(slot.ToString(), view.Key);
        Assert.Equal("A:", view.Label);
    }
}
