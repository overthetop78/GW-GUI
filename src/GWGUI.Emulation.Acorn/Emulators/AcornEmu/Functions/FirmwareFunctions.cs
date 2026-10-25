using System.IO;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;

/// <summary>Adaptation des champs AcornEmu aux rôles matériels et chargement des fichiers utilisateur.</summary>
/// <remarks>L'identité, la compatibilité par machine et le format des banques sont définis dans Common/Machines.
/// Les identifiants des champs déjà enregistrés restent ceux de la configuration AcornEmu.</remarks>
internal static class FirmwareFunctions
{
    internal static string? Field(FirmwareCategory category) => category switch
    {
        FirmwareCategory.OperatingSystem => EmulatorConstants.SystemFirmware,
        FirmwareCategory.Basic or FirmwareCategory.Language => EmulatorConstants.BasicFirmware,
        FirmwareCategory.CharacterGenerator => EmulatorConstants.CharacterFirmware,
        _ => null
    };

    internal static FirmwareCategory Role(string field) => field switch
    {
        EmulatorConstants.SystemFirmware => FirmwareCategory.OperatingSystem,
        EmulatorConstants.BasicFirmware => FirmwareCategory.Basic,
        EmulatorConstants.CharacterFirmware => FirmwareCategory.CharacterGenerator,
        _ => throw new ArgumentOutOfRangeException(nameof(field))
    };

    /// <summary>Lit le fichier sélectionné puis demande à Common la banque correspondant au rôle matériel.</summary>
    internal static byte[] Read(MachineConfiguration configuration, string field)
    {
        if (!configuration.Options.TryGetValue(field, out var path) || string.IsNullOrWhiteSpace(path)) throw new FileNotFoundException(field);
        return BbcMicroFirmwareFunctions.GetRomBytes(File.ReadAllBytes(path), Role(field));
    }
}
