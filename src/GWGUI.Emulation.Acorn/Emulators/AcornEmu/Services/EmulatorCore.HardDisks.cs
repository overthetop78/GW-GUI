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
    private readonly (bool Protected, bool Redirect, int ReadLatency, int WriteLatency, int Speed)[] _hardDiskSettings = new (bool, bool, int, int, int)[C.HardDiskCount];

    private void ConfigureHardDisks(MachineConfiguration configuration)
    {
        for (var unit = 0; unit < C.HardDiskCount; unit++)
        {
            _hardDiskConnected[unit] = NativeStorage.HardDiskConnected(configuration, unit);
            var settings = _hardDiskSettings[unit] = NativeStorage.HardDiskSettings(configuration, unit);
            _machine!.ConfigureHardDisk(unit, settings.ReadLatency, settings.Speed, settings.WriteLatency);
        }
    }

    private void InsertHardDisk(MediaConfiguration media)
    {
        var unit = media.Slot.Index;
        if (media.Category != MediaCategory.HardDisk || unit is < 0 or >= C.HardDiskCount || !_hardDiskConnected[unit] || !C.HardDiskExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase)) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(media));
        var settings = _hardDiskSettings[unit];
        var path = settings.Redirect && !media.IsReadOnly && !settings.Protected ? BbcMicroDiskFunctions.RedirectHardDisk(media.Path, _diskSaveDirectory!, unit) : media.Path;
        var image = BbcMicroDiskFunctions.Open(path, !media.IsReadOnly && !settings.Protected);
        var transferred = false;
        try
        {
            if (image.FormatId != C.HardDiskFormat || image.SectorSize % 256 != 0 || image.SectorCount * image.SectorSize > C.MaximumHardDiskBytes) throw new InvalidDataException(ErrorMessages.IncompatibleMedia);
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
        var image = _hardDisks[unit]; _hardDisks[unit] = null; image?.Dispose();
    }
    private void DisposeHardDisks()
    {
        var errors = new List<Exception>();
        for (var unit = 0; unit < C.HardDiskCount; unit++)
        {
            var image = _hardDisks[unit]; _hardDisks[unit] = null;
            try { _machine?.AttachHardDisk(unit, null); }
            catch (Exception error) { errors.Add(error); }
            finally { try { image?.Dispose(); } catch (Exception error) { errors.Add(error); } }
        }
        if (errors.Count > 0) throw new AggregateException(errors);
    }
}
