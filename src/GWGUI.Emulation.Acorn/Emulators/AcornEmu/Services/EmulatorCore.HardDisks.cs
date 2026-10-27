using GWGUI.Emulation.HardDisks;
using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;
using System.IO;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;
using NativeStorage = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions.StorageFunctions;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Possède les unités Winchester et publie leurs données par le même relais que les autres lecteurs.</summary>
internal sealed partial class EmulatorCore
{
    private readonly IEmulationDiskImage?[] _hardDisks = new IEmulationDiskImage?[C.HardDiskCount];
    private readonly bool[] _hardDiskConnected = new bool[C.HardDiskCount];
    private readonly (bool Protected, int ReadLatency, int WriteLatency, int Speed)[] _hardDiskSettings = new (bool, int, int, int)[C.HardDiskCount];

    private void ConfigureHardDisks(MachineConfiguration configuration)
    {
        for (var unit = 0; unit < C.HardDiskCount; unit++)
        {
            _hardDiskMechanics[unit] ??= GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions.MediaProcessing.CreateDriveMechanics(S.HardDisk);
            _hardDiskConnected[unit] = NativeStorage.HardDiskConnected(configuration, unit);
            var settings = _hardDiskSettings[unit] = NativeStorage.HardDiskSettings(configuration, unit);
            _hardDiskSoundEnabled[unit] = bool.Parse(configuration.Options.GetValueOrDefault(C.HardDiskKey(unit, C.MechanicalSoundOption), bool.TrueString));
            _hardDiskSoundVolume[unit] = EmulationMechanicalSoundSettings.ReadVolume(configuration.Options, C.HardDiskKey(unit, C.SoundVolumeOption));
            _machine!.ConfigureHardDisk(unit, settings.ReadLatency, settings.Speed, settings.WriteLatency);
        }
    }

    private void InsertHardDisk(MediaConfiguration media)
    {
        var unit = media.Slot.Index;
        if (media.Category != MediaCategory.HardDisk || unit is < 0 or >= C.HardDiskCount || !_hardDiskConnected[unit] || !C.HardDiskExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase)) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(media));
        var settings = _hardDiskSettings[unit];
        var image = BbcMicroDiskFunctions.Open(media.Path, !media.IsReadOnly && !settings.Protected, mechanics: _hardDiskMechanics[unit]);
        var transferred = false;
        try
        {
            if (image.FormatId != C.HardDiskFormat || !GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries.HardDiskFormats.All.Any(format => HardDiskImageValidation.IsSupportedCapacity(image.SectorCount * image.SectorSize, format))) throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
            EjectHardDisk(unit); _machine!.AttachHardDisk(unit, image); _hardDisks[unit] = image; transferred = true;
        }
        finally { if (!transferred) image.Dispose(); }
    }

    private void SaveHardDisk(int unit) { if (_hardDisks[unit] is { HasChanges: true } image) BbcMicroDiskFunctions.Save(image); }
    private void SaveHardDisks() { for (var unit = 0; unit < C.HardDiskCount; unit++) SaveHardDisk(unit); }
    private void FlushStoppedHardDisks() { for (var unit = 0; unit < C.HardDiskCount; unit++) if (_machine?.HardDiskActive(unit) != true) SaveHardDisk(unit); }
    private void EjectHardDisk(int unit)
    {
        if (unit is < 0 or >= C.HardDiskCount) throw new ArgumentOutOfRangeException(nameof(unit));
        SaveHardDisk(unit); _machine?.AttachHardDisk(unit, null);
        var image = _hardDisks[unit]; _hardDisks[unit] = null;
        try { image?.Dispose(); } finally { _hardDiskMechanics[unit]?.SetMotor(false); }
    }
    private void DisposeHardDisks()
    {
        var errors = new List<Exception>();
        for (var unit = 0; unit < C.HardDiskCount; unit++)
        {
            var image = _hardDisks[unit]; _hardDisks[unit] = null;
            try { _machine?.AttachHardDisk(unit, null); }
            catch (Exception error) { errors.Add(error); }
            finally { try { image?.Dispose(); } catch (Exception error) { errors.Add(error); } finally { _hardDiskMechanics[unit]?.Reset(); _hardDiskMechanics[unit] = null; } }
        }
        if (errors.Count > 0) throw new AggregateException(errors);
    }
}
