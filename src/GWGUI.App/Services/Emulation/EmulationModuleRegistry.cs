using GWGUI.App.Services.Storage;
using System.IO;
using System.Net.Http;
using GWGUI.Emulation;
using GWGUI.Emulation.Amiga;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Services.Emulation;

internal static class EmulationModuleRegistry
{
    private static readonly HttpClient HttpClient = new();
    internal static IReadOnlyList<IEmulationModule> Modules { get; } = Create();

    private static IReadOnlyList<IEmulationModule> Create() =>
    [
        CreateAmiga(),
        CreateAtari()
    ];

    private static IEmulationModule CreateAmiga()
    {
        var root = Path.Combine(StoragePaths.EmulationDirectory, "Machines", "Amiga");
        return new AmigaEmulationModule(Path.Combine(root, "Configurations"), StoragePaths.DataDirectory,
            HttpClient, Path.Combine(root, "Core"));
    }

    private static IEmulationModule CreateAtari()
    {
        var root = Path.Combine(StoragePaths.EmulationDirectory, "Machines", "Atari");
        return new AtariEmulationModule(Path.Combine(root, "Configurations"), StoragePaths.DataDirectory,
            HttpClient, Path.Combine(root, "Core"));
    }
}
