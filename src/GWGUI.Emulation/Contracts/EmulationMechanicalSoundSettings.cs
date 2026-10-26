using System.Globalization;
using C = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;

namespace GWGUI.Emulation.Contracts;

/// <summary>Volume commun des bruitages ; le module décide d’exposer ce champ dans ses réglages.</summary>
public static class EmulationMechanicalSoundSettings
{
    public static int ReadVolume(IReadOnlyDictionary<string, string>? options, string key = C.VolumeOption)
    {
        var value = options?.TryGetValue(key, out var text) == true ? int.Parse(text, CultureInfo.InvariantCulture) : C.DefaultVolumePercent;
        if (value is < 0 or > C.MaximumVolumePercent) throw new ArgumentOutOfRangeException(nameof(options));
        return value;
    }

    public static EmulationSettingsField DescribeVolume(string id, string group, int value) => new(id, EmulationMachineTab.Storage,
        group, C.VolumeResource, EmulationSettingsEditor.Slider, value.ToString(CultureInfo.InvariantCulture),
        NumericRange: new(0, C.MaximumVolumePercent, 1, "%"));
}
