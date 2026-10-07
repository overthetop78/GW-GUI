
namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Services;


internal sealed class StStorage
{
    internal StStorage(MediaConfiguration configuration, StorageBus bus,
        string runtimePath, IReadOnlyList<StStorageVolume> volumes, bool ownsMarker)
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
    internal IReadOnlyList<StStorageVolume> Volumes { get; }
    internal bool OwnsMarker { get; }

}
