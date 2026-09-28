using System.Reflection;
using System.Net.Http;
using System.IO;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Nec.Modules;
using GWGUI.Emulation.Nintendo.Modules;
using GWGUI.Emulation.Sega.Modules;
using GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Modules;
using GWGUI.Emulation.Microsoft.Modules;

namespace GWGUI.Tests.Architecture;

public sealed class ConsoleFamilyModuleTests
{
    [Fact]
    public void ConsoleFamilyModulesExposeTheSameCommonFileLayout()
    {
        var root = RepositoryRoot();
        var reference = CommonFiles(root, "Sega");
        foreach (var family in new[] { "Nintendo", "Sony", "Microsoft", "Nec" })
            Assert.Equal(reference, CommonFiles(root, family));
    }

    [Fact]
    public void ConsoleFamilyFactoriesCreateTheDeclaredModules()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-console-module-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var modules = new IEmulationModule[]
            {
                new SegaEmulationModuleFactory().Create(context),
                new NintendoEmulationModuleFactory().Create(context),
                new SonyEmulationModuleFactory().Create(context),
                new MicrosoftEmulationModuleFactory().Create(context),
                new NecEmulationModuleFactory().Create(context)
            };
            Assert.Equal(new[] { "microsoft", "nec", "nintendo", "sega", "sony" },
                modules.Select(module => module.Id).Order(StringComparer.Ordinal));
            Assert.All(modules, module => Assert.NotEmpty(module.Machines));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ConsoleFamiliesRejectMachinesWithoutAnInstalledAdapter()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-console-unsupported-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var cases = new (IEmulationModule Module, string MachineId)[]
            {
                (new NintendoEmulationModuleFactory().Create(context), "Nes"),
                (new SonyEmulationModuleFactory().Create(context), "PlayStation"),
                (new MicrosoftEmulationModuleFactory().Create(context), "Xbox"),
                (new NecEmulationModuleFactory().Create(context), "PcEngine")
            };

            foreach (var (module, machineId) in cases)
                Assert.Throws<NotSupportedException>(() => module.CreateConfiguration(machineId));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaAdaptersSelectTheMatchingCoreForSaturnAndSc3000()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-adapter-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModule(root, root, http, Path.Combine(root, "Core"));
            var saturn = Assert.IsType<MachineConfiguration>(module.CreateConfiguration("Saturn"));
            var sc3000 = Assert.IsType<MachineConfiguration>(module.CreateConfiguration("Sc3000"));
            Assert.Equal("yabause", saturn.EmulatorId);
            Assert.Equal("genesisplusgx", sc3000.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static string RepositoryRoot() => Path.GetFullPath(Path.Combine(
        Path.GetDirectoryName(typeof(ConsoleFamilyModuleTests).Assembly.Location)!,
        "..", "..", "..", "..", ".."));

    private static string[] CommonFiles(string root, string family) =>
        Directory.EnumerateFiles(Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common"),
                "*.cs", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(
                Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common"), path))
            .Order(StringComparer.Ordinal)
            .ToArray();
}
