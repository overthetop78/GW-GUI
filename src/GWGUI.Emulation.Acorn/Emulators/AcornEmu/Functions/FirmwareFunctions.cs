using System.IO;
using GWGUI.Emulation.Acorn.Common.Constants;
using GWGUI.Emulation.Acorn.Common.Exceptions;
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
        FirmwareCategory.Network => NetworkConstants.Firmware,
        FirmwareCategory.MemoryExtension => MemoryExtensionConstants.Firmware,
        FirmwareCategory.DiscFilingSystem => StorageConstants.DfsFirmware,
        FirmwareCategory.Application => StorageConstants.DfsFirmware,
        _ => null
    };

    internal static FirmwareCategory Role(string field) => field switch
    {
        EmulatorConstants.SystemFirmware => FirmwareCategory.OperatingSystem,
        EmulatorConstants.BasicFirmware => FirmwareCategory.Basic,
        EmulatorConstants.CharacterFirmware => FirmwareCategory.CharacterGenerator,
        NetworkConstants.Firmware => FirmwareCategory.Network,
        MemoryExtensionConstants.Firmware => FirmwareCategory.MemoryExtension,
        StorageConstants.DfsFirmware or FloppyExtensionConstants.IntelRom or FloppyExtensionConstants.WdRom => FirmwareCategory.DiscFilingSystem,
        _ => throw new ArgumentOutOfRangeException(nameof(field))
    };

    /// <summary>Lit le fichier sélectionné puis demande à Common la banque correspondant au rôle matériel.</summary>
    internal static byte[] Read(MachineConfiguration configuration, string field)
    {
        if (SidewaysExpansionFunctions.RoleBank(configuration, field) is { } bank && SidewaysExpansionFunctions.ProvidesBank(configuration, bank))
            return BbcMicroFirmwareFunctions.GetRomBytes(SidewaysExpansionFunctions.ReadBank(configuration, bank), Role(field));
        var path = field is FloppyExtensionConstants.IntelRom or FloppyExtensionConstants.WdRom
            ? FloppyExtensionFunctions.RomPath(configuration, field) : configuration.Options.GetValueOrDefault(field);
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing,
                field == MemoryExtensionConstants.Firmware ? ExceptionText.Get(MemoryExtensionConstants.MissingFirmware)
                    : ExceptionText.Get(CoreErrorResourceKeys.NamedFirmwareMissing, RomName(field)), isLocalized: true);
        return BbcMicroFirmwareFunctions.GetRomBytes(File.ReadAllBytes(path), Role(field));
    }
    /// <summary>Désigne la ROM manquante sans exposer l’identifiant technique du champ à l’utilisateur.</summary>
    private static string RomName(string field) => field switch
    {
        EmulatorConstants.SystemFirmware => FirmwareRomNames.Mos,
        EmulatorConstants.BasicFirmware => FirmwareRomNames.Basic,
        EmulatorConstants.CharacterFirmware => FirmwareRomNames.Characters,
        StorageConstants.DfsFirmware => FirmwareRomNames.DiskFilingSystem,
        FloppyExtensionConstants.IntelRom => FloppyExtensionConstants.IntelRomName,
        FloppyExtensionConstants.WdRom => FloppyExtensionConstants.WdRomName,
        NetworkConstants.Firmware => FirmwareRomNames.Network,
        MemoryExtensionConstants.Firmware => ExceptionText.Get(MemoryExtensionConstants.FirmwareLabel),
        _ => throw new ArgumentOutOfRangeException(nameof(field))
    };

    /// <summary>Place chaque ROM externe dans une banque distincte selon les extensions réellement installées.</summary>
    internal static IReadOnlyDictionary<int, byte[]> OptionRoms(MachineConfiguration configuration, IReadOnlyDictionary<int, byte[]>? chipBanks = null)
    {
        var banks = (chipBanks ?? SidewaysExpansionFunctions.ChipBanks(configuration))
            .Where(bank => bank.Key != SidewaysExpansionFunctions.NativeBank(configuration, Machines.BbcMicro.Common.Constants.DiskConstants.BasicBank))
            .ToDictionary(bank => bank.Key, bank => bank.Value);
        if (StorageFunctions.NeedsDiskRom(configuration) && !banks.ContainsKey(SidewaysExpansionFunctions.NativeBank(configuration, Machines.BbcMicro.Common.Constants.DiskConstants.DfsBank))) banks[SidewaysExpansionFunctions.NativeBank(configuration, Machines.BbcMicro.Common.Constants.DiskConstants.DfsBank)] = Read(configuration, FloppyExtensionFunctions.ActiveRom(configuration));
        if (NetworkFunctions.Read(configuration).Enabled && !banks.ContainsKey(SidewaysExpansionFunctions.NativeBank(configuration, Machines.BbcMicro.Common.Constants.DiskConstants.NetworkBank))) banks[SidewaysExpansionFunctions.NativeBank(configuration, Machines.BbcMicro.Common.Constants.DiskConstants.NetworkBank)] = Read(configuration, NetworkConstants.Firmware);
        MemoryExtensionFunctions.AddFirmware(configuration, banks);
        return banks;
    }
}
