using System.IO;
using GWGUI.Emulation.Sega.Emulators.BlueMSX.Constants;

namespace GWGUI.Emulation.Sega.Emulators.BlueMSX.Functions;

internal static class SystemFilesFunctions
{
    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        var profile = configuration.Model switch
        {
            ModelConstants.Sg1000 => (SystemFilesConstants.Sg1000Directory, SystemFilesConstants.Sg1000Configuration),
            ModelConstants.Sc3000 => (SystemFilesConstants.Sc3000Directory, SystemFilesConstants.Sc3000Configuration),
            ModelConstants.Sf7000 => (SystemFilesConstants.Sf7000Directory, SystemFilesConstants.Sf7000Configuration),
            _ => throw new ArgumentOutOfRangeException(nameof(configuration))
        };
        var directory = Path.Combine(systemDirectory, SystemFilesConstants.MachinesDirectory, profile.Item1);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, SystemFilesConstants.ConfigurationFileName), profile.Item2);
    }
}
