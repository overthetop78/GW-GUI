using GWGUI.Emulation.Atari.Emulators.Hatari.Constants;
using GWGUI.Emulation.Atari.Emulators.Hatari.Contracts;
using GWGUI.Emulation.Atari.Emulators.Hatari.Functions;
using GWGUI.Emulation.Atari.Emulators.Hatari.Services;

namespace GWGUI.Emulation.Atari.Emulators.Hatari.Services;


internal sealed class HatariStorage
{
    internal HatariStorage(MediaConfiguration configuration, StorageBus bus,
        string runtimePath, IReadOnlyList<HatariStorageVolume> volumes, bool ownsMarker)
    {
        Configuration = configuration;
        Bus = bus;
        RuntimePath = runtimePath;
        Volumes = volumes;
        OwnsMarker = ownsMarker;
    }

    internal MediaConfiguration Configuration { get; }
    internal StorageBus Bus { get; }
    internal string RuntimePath { get; }
    internal IReadOnlyList<HatariStorageVolume> Volumes { get; }
    internal bool OwnsMarker { get; }

}
