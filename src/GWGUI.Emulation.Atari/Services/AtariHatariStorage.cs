namespace GWGUI.Emulation.Atari.Services;


internal sealed class AtariHatariStorage
{
    internal AtariHatariStorage(AtariMediaConfiguration configuration, AtariStorageBus bus,
        string runtimePath, IReadOnlyList<AtariHatariStorageVolume> volumes, bool ownsMarker)
    {
        Configuration = configuration;
        Bus = bus;
        RuntimePath = runtimePath;
        Volumes = volumes;
        OwnsMarker = ownsMarker;
    }

    internal AtariMediaConfiguration Configuration { get; }
    internal AtariStorageBus Bus { get; }
    internal string RuntimePath { get; }
    internal IReadOnlyList<AtariHatariStorageVolume> Volumes { get; }
    internal bool OwnsMarker { get; }

}
