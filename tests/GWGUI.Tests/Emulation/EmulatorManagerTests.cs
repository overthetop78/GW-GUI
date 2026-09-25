using System.IO;
using System.Net.Http;
using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Amiga.Modules;
using GWGUI.Emulation.Atari.Modules;

namespace GWGUI.Tests.Emulation;

public sealed class EmulatorManagerTests
{
    [Fact]
    public async Task FamilyManagersReturnGenericPresentationData()
    {
        var root = Path.Combine(Path.GetTempPath(), $"gwgui-emulators-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            IEmulationModule[] modules =
            [
                new AtariEmulationModule(Path.Combine(root, "atari-config"), root, http,
                    Path.Combine(root, "atari-cores")),
                new AmigaEmulationModule(Path.Combine(root, "amiga-config"), root, http,
                    Path.Combine(root, "amiga-cores"))
            ];

            foreach (var module in modules)
            {
                var manager = Assert.IsAssignableFrom<IEmulationEmulatorManager>(module);
                var configuration = module.CreateConfiguration(module.Machines[0].Id);
                var installations = await manager.GetEmulatorInstallationsAsync(configuration);
                var selected = await manager.GetEmulatorInstallationAsync(configuration);
                Assert.NotEmpty(installations);
                Assert.Contains(installations, item => item.EmulatorId == selected.EmulatorId);
                Assert.All(installations, item =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(item.DisplayName));
                    Assert.False(string.IsNullOrWhiteSpace(item.DescriptionResourceKey));
                    Assert.Contains(configuration.MachineId, item.Emulator.MachineIds);
                });
            }
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
