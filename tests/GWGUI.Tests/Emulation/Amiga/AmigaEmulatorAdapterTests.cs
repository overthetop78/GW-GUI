using GWGUI.Emulation.Amiga.Modules;

namespace GWGUI.Tests.Emulation.Amiga;

public sealed class AmigaEmulatorAdapterTests
{
    [Fact]
    public void PuaeUsesItsPhysicalNamespaceAndTheCommonAdapter()
    {
        var assembly = typeof(AmigaEmulationModule).Assembly;
        var adapter = assembly.GetType("GWGUI.Emulation.Amiga.Emulators.PUAE.Factories.PuaeMachineFactory");
        var common = assembly.GetType("GWGUI.Emulation.Amiga.Common.Interfaces.IEmulatorAdapter");
        Assert.NotNull(adapter);
        Assert.NotNull(common);
        Assert.Contains(common!, adapter!.GetInterfaces());
    }
}
