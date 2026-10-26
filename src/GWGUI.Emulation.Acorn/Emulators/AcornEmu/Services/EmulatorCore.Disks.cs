using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;
using System.IO;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;
using NativeStorage = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions.StorageFunctions;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Possession et persistance des images disque ; les contrôleurs de la carte les empruntent seulement.</summary>
internal sealed partial class EmulatorCore
{
    private readonly IEmulationDiskImage?[] _floppyImages = new IEmulationDiskImage?[C.DriveCount];
    private readonly FloppyDriveSettings?[] _floppySettings = new FloppyDriveSettings?[C.DriveCount];
    private readonly bool[] _floppyConnected = new bool[C.DriveCount];
    private IReadOnlyList<string> _diskExtensions = C.Extensions;

    private void ConfigureDisks(MachineConfiguration configuration)
    {
        _diskExtensions = NativeStorage.Extensions(configuration);
        for (var drive = 0; drive < C.DriveCount; drive++)
        {
            _floppyMechanics[drive] ??= GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions.MediaProcessing.CreateDriveMechanics(S.Floppy);
            _floppyConnected[drive] = NativeStorage.Connected(configuration, drive);
            var settings = _floppySettings[drive] = NativeStorage.Settings(configuration, drive);
            _machine!.ConfigureFloppySpeed(drive, int.Parse(settings.Speed, System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    private void InsertFloppy(MediaConfiguration media)
    {
        var drive = media.Slot.Index;
        if (media.Category != MediaCategory.Floppy || drive is < 0 or >= C.DriveCount || !_floppyConnected[drive] || !_diskExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase)) throw new ArgumentException(nameof(media));
        var settings = _floppySettings[drive]!;
        var image = BbcMicroDiskFunctions.Open(media.Path, !media.IsReadOnly && !settings.WriteProtected, mechanics: _floppyMechanics[drive]);
        var transferred = false;
        try
        {
            NativeStorage.ValidateMedia(image, settings);
            EjectFloppy(drive);
            _machine!.AttachFloppy(drive, image); _floppyImages[drive] = image; transferred = true;
            _floppyMechanics[drive]!.Insert();
        }
        finally { if (!transferred) image.Dispose(); }
    }

    private void SaveFloppy(int drive) { if (_floppyImages[drive] is { HasChanges: true } image) BbcMicroDiskFunctions.Save(image); }
    private void EjectFloppy(int drive)
    {
        if (drive is < 0 or >= C.DriveCount) throw new ArgumentOutOfRangeException(nameof(drive));
        SaveFloppy(drive);
        _machine?.AttachFloppy(drive, null);
        var image = _floppyImages[drive]; _floppyImages[drive] = null;
        try { image?.Dispose(); }
        finally { if (image is not null) _floppyMechanics[drive]?.Eject(); }
    }

    /// <summary>Libère toutes les images même si une publication échoue ; le propriétaire libère ensuite la carte.</summary>
    private void DisposeDisks()
    {
        var errors = new List<Exception>();
        for (var drive = 0; drive < C.DriveCount; drive++)
        {
            var image = _floppyImages[drive]; _floppyImages[drive] = null;
            try { _machine?.AttachFloppy(drive, null); }
            finally { try { image?.Dispose(); } catch (Exception error) { errors.Add(error); } finally { _floppyMechanics[drive]?.Reset(); _floppyMechanics[drive] = null; } }
        }
        if (errors.Count > 0) throw new AggregateException(errors);
    }

    private void SaveDisks() { for (var drive = 0; drive < C.DriveCount; drive++) SaveFloppy(drive); }
    private void FlushStoppedDisks() { for (var drive = 0; drive < C.DriveCount; drive++) if (_machine?.FloppyActive(drive) != true) SaveFloppy(drive); }
}
