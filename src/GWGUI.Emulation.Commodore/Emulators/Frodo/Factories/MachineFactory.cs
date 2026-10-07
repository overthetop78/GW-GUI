using System.IO;
using GWGUI.Emulation.Commodore.Emulators.Frodo.Constants;
using C64Models = GWGUI.Emulation.Commodore.Common.Machines.C64.Constants.ModelConstants;

namespace GWGUI.Emulation.Commodore.Emulators.Frodo.Factories;

internal sealed class MachineFactory : GWGUI.Emulation.Commodore.Emulators.Common.Interop.Factories.MachineFactory
{
    public override EmulatorCatalogEntry CatalogEntry { get; } = new(Emulator.Frodo,
        new(EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorConstants.DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { C64Models.C64 }));
    internal override CoreDefinition CoreDefinition { get; } = new(
        EmulatorConstants.DisplayName, EmulatorConstants.LibraryFile, EmulatorConstants.DownloadUrl);
    public override IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) =>
        EmulatorConstants.Roms.Select(rom => rom.Slot).ToArray();
    public override IReadOnlyList<EmulationMediaDevice> GetMediaDevices(MachineConfiguration configuration) =>
    [
        new(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, EmulatorConstants.DiskExtensions,
            RequiresMachineRecreation: true),
        new(new(EmulationMediaCategory.CassetteDrive, EmulatorConstants.PrimaryMediaIndex),
            EmulationMediaType.Cassette, EmulatorConstants.CassetteExtensions, RequiresMachineRecreation: true)
    ];
    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory)
    {
        var session = Directory.GetParent(systemDirectory)!.FullName;
        foreach (var rom in EmulatorConstants.Roms)
        {
            var path = configuration.FirmwarePath(rom.Slot.FieldId);
            if (!string.IsNullOrWhiteSpace(path)) File.Copy(path, Path.Combine(session, rom.FileName), true);
        }
    }
    internal override uint ControllerDevice(IReadOnlyList<IReadOnlyList<ControllerDevice>> ports, int port, ControllerType type) =>
        type == ControllerType.None ? EmulatorConstants.NoControllerDevice : EmulatorConstants.JoypadDevice;
    internal override IReadOnlyDictionary<string, string> NativeOptions(MachineConfiguration configuration, IReadOnlyList<MediaConfiguration> media) =>
        (configuration.Options ?? new Dictionary<string, string>())
            .Where(pair => pair.Key.StartsWith(EmulatorConstants.OptionPrefix, StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
    internal override string? PrepareContent(MachineConfiguration configuration, string session, IReadOnlyList<MediaConfiguration> media)
    {
        if (media.Count > EmulatorConstants.MaximumMountedMedia) throw new ArgumentOutOfRangeException(nameof(media));
        return media.Count == BufferConstants.EmptyCollectionCount ? null
            : Path.GetFullPath(media[EmulatorConstants.PrimaryMediaIndex].Path);
    }
}
