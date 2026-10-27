using System.Globalization;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;
using CommonHost = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

internal sealed partial class EmulatorCore
{
    /// <summary>Applique les seuls réglages sonores du lecteur sans toucher au matériel de la machine.</summary>
    private bool ApplyMechanicalSoundOption(string key, string value)
    {
        if (!StorageFunctions.TryGetMechanicalSoundOption(key, out var hardDisk, out var index, out var option)) return false;
        var values = new Dictionary<string, string>(StringComparer.Ordinal) { [option] = value };
        if (option == C.SoundVolumeOption) _ = EmulationMechanicalSoundSettings.ReadVolume(values);
        else if (option == C.MechanicalSoundOption) _ = bool.Parse(value);
        if (hardDisk)
        {
            if (option == C.SoundVolumeOption) _hardDiskSoundVolume[index] = int.Parse(value, CultureInfo.InvariantCulture);
            else if (option == C.MechanicalSoundOption) _hardDiskSoundEnabled[index] = bool.Parse(value);
        }
        else if (_floppySettings[index] is { } settings)
        {
            var options = new Dictionary<string, string>(settings.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal) { [option] = value };
            _floppySettings[index] = settings with { Options = options };
        }
        _runtimeOptions[key] = value;
        return true;
    }
}
