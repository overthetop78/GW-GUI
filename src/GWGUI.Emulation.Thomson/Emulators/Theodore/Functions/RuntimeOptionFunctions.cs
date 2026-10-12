using GWGUI.Emulation.Thomson.Emulators.Theodore.Constants;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Dictionaries;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Functions;

internal static class RuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        ConfigurationFunctions.Validate(configuration);
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal) { [RuntimeOptionConstants.Model] = ModelDictionary.NativeValues[configuration.Model] };
        var floppy = configuration.Media.FirstOrDefault(media => media.Category == MediaCategory.Floppy && media.IsInserted);
        var tape = configuration.Media.FirstOrDefault(media => media.Category == MediaCategory.Tape && media.IsInserted);
        if (floppy is not null) options[RuntimeOptionConstants.FloppyWriteProtection] = floppy.IsReadOnly ? RuntimeOptionConstants.Enabled : RuntimeOptionConstants.Disabled;
        if (tape is not null) options[RuntimeOptionConstants.TapeWriteProtection] = tape.IsReadOnly ? RuntimeOptionConstants.Enabled : RuntimeOptionConstants.Disabled;
        return options;
    }
}
