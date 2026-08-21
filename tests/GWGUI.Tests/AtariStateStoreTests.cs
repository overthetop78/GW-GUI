using System.IO;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariStateStoreTests
{
    [Fact]
    public void StateFileNameIsDerivedFromTheConfigurationIdentity()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.St);

        var path = AtariStateStoreFunctions.GetMachineDirectory(Path.GetTempPath(), configuration.Id);

        Assert.Contains(configuration.Id.ToString("N"), path, StringComparison.OrdinalIgnoreCase);
    }
}
