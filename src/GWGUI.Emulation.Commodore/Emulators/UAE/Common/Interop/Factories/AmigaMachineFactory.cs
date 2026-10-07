using System.IO;
using System.Globalization;
using System.Security.Cryptography;
using AmigaModels = GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants;
using Cd32Models = GWGUI.Emulation.Commodore.Common.Machines.AmigaCD32.Constants.ModelConstants;
using CdtvModels = GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Constants.ModelConstants;

namespace GWGUI.Emulation.Commodore.Emulators.UAE.Common.Interop.Factories;

internal abstract class AmigaMachineFactory : MachineFactory
{
    internal override MediaCategory InferMediaCategory(MachineConfiguration configuration, string path) => Directory.Exists(path)
        ? MediaCategory.HardDrive
        : Path.GetExtension(path).ToLowerInvariant() switch
    {
        StorageSettingsFunctionsConstants.Hdf or StorageSettingsFunctionsConstants.Hdz => MediaCategory.HardDrive,
        StorageSettingsFunctionsConstants.Cue or StorageSettingsFunctionsConstants.Ccd or StorageSettingsFunctionsConstants.Chd or StorageSettingsFunctionsConstants.Nrg or StorageSettingsFunctionsConstants.Mds or StorageSettingsFunctionsConstants.Iso => MediaCategory.CompactDisc,
        UaeFirmwareConstants.Lha or UaeFirmwareConstants.Slave or UaeFirmwareConstants.Info => MediaCategory.WhdLoad,
        UaeFirmwareConstants.Uae => MediaCategory.Configuration,
            _ => MediaCategory.Floppy
        };
    public override IReadOnlyList<EmulationSettingsBlock> GetSettingsBlocks(Model model, MachineConfiguration configuration) =>
        AmigaSettingsDescriptionFunctions.Create((AmigaModel)model, configuration).Where(block => block.Tab != EmulationMachineTab.Rom).ToArray();
    public override EmulationStorageSettings DescribeStorage(MachineConfiguration configuration) => AmigaStorageSettingsFunctions.Describe(configuration);
    public override MachineConfiguration ApplyStorage(MachineConfiguration configuration, EmulationStorageSettings settings) => AmigaStorageSettingsFunctions.Apply(configuration, settings);
    public override MachineConfiguration ApplyBackendSettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values) => AmigaSettingsApplicationFunctions.Apply(configuration, values);
    public override EmulationConfigurationSummary Summarize(MachineConfiguration configuration) => AmigaConfigurationSummaryFunctions.Create(configuration);
    public override void ValidateConfiguration(MachineConfiguration configuration) => AmigaConfigurationValidationFunctions.ValidateForSave(configuration);
    public override IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration) => AmigaInputSettingsFunctions.KeyboardDefinitions(configuration);
    public override MachineConfiguration ApplyInputConfiguration(MachineConfiguration configuration) => AmigaInputSettingsFunctions.Apply(configuration);
    public override IReadOnlyList<EmulationFirmwareCandidate> ScanFirmware(MachineConfiguration configuration, string directory, CancellationToken cancellationToken) =>
        AmigaFirmwareFunctions.Scan(configuration.Model, directory, cancellationToken);
    public override ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory) =>
        AmigaRuntimeMediaFunctions.PrepareMediaAsync(media, conversionDirectory);
    public override Task<MachineConfiguration> PrepareRuntimeConfigurationAsync(MachineConfiguration configuration, string conversionDirectory) =>
        AmigaRuntimeMediaFunctions.PrepareConfigurationAsync(configuration, conversionDirectory);
    public override IReadOnlySet<EmulationKey> SoftResetKeys { get; } = new HashSet<EmulationKey>
        { EmulationKey.LeftControl, EmulationKey.LeftAmiga, EmulationKey.RightAmiga };

    public override IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates)
    {
        var activity = new Dictionary<EmulationMediaSlot, bool>();
        for (var index = BufferConstants.FirstCollectionIndex; index < UaeMediaConstants.FloppyDriveCount; index++)
            activity[new(EmulationMediaCategory.FloppyDrive, index)] = ledStates.GetValueOrDefault(UaeMediaConstants.FloppyLedStart + index);
        activity[EmulationMediaSlot.HardDisk0] = ledStates.GetValueOrDefault(UaeMediaConstants.HardDiskLed);
        activity[EmulationMediaSlot.Cd0] = ledStates.GetValueOrDefault(UaeMediaConstants.CdLed);
        return activity;
    }

    protected static IReadOnlySet<string> SupportedMachines { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        AmigaModels.A500, AmigaModels.A500PLUS, AmigaModels.A600, AmigaModels.A1000,
        AmigaModels.A1200, AmigaModels.A2000, AmigaModels.A3000, AmigaModels.A4000,
        Cd32Models.CD32, CdtvModels.CDTV
    };

    public override IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) =>
    [
        new(SettingsConstants.KickstartPath, SettingsResourceKeys.FirmwareRomKickstart, true,
            SettingsHelpResourceKeys.FirmwareSystem + SettingsValueConstants.ShortHelpSuffix,
            SettingsHelpResourceKeys.FirmwareSystem + SettingsValueConstants.DetailedHelpSuffix),
        new(SettingsConstants.ExtendedRomPath, SettingsResourceKeys.FirmwareRomExtended, false,
            SettingsHelpResourceKeys.FirmwareExtendedRom + SettingsValueConstants.ShortHelpSuffix,
            SettingsHelpResourceKeys.FirmwareExtendedRom + SettingsValueConstants.DetailedHelpSuffix),
        new(SettingsConstants.RomKeyPath, SettingsResourceKeys.FirmwareRomKey, false,
            SettingsHelpResourceKeys.FirmwareRomKey + SettingsValueConstants.ShortHelpSuffix,
            SettingsHelpResourceKeys.FirmwareRomKey + SettingsValueConstants.DetailedHelpSuffix)
    ];

    public override IReadOnlyList<EmulationMediaDevice> GetMediaDevices(MachineConfiguration configuration) =>
        AmigaStorageSettingsFunctions.Describe(configuration).AvailableDevices;

    internal override int ControllerPortCount(MachineConfiguration configuration) =>
        configuration.Input?.ParallelJoystickAdapterEnabled == true
            ? ControllerPortConstants.FourControllerPorts : base.ControllerPortCount(configuration);

    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory)
    {
        var kickstart = configuration.FirmwarePath(SettingsConstants.KickstartPath);
        File.Copy(kickstart, Path.Combine(systemDirectory, ResolveKickstartFileName(configuration.Model, kickstart)), true);
        var extended = configuration.FirmwarePath(SettingsConstants.ExtendedRomPath);
        if (!string.IsNullOrWhiteSpace(extended))
            File.Copy(extended, Path.Combine(systemDirectory, ExtendedRomFileName(configuration.Model, extended)), true);
        var key = configuration.FirmwarePath(SettingsConstants.RomKeyPath);
        if (!string.IsNullOrWhiteSpace(key)) File.Copy(key, Path.Combine(systemDirectory, UaeFirmwareConstants.RomKey), true);
        foreach (var disk in ExternalCore.ResolveConfiguredMedia(configuration)
                     .Where(item => item.Category == MediaCategory.HardDrive && !Directory.Exists(item.Path)))
        {
            var format = AmigaHardDiskFormats.All.FirstOrDefault(item => string.Equals(item.Extension,
                Path.GetExtension(disk.Path), StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidDataException(CoreExceptions.UnsupportedHardDiskExtension());
            GWGUI.Emulation.HardDisks.HardDiskImageValidation.ValidateExisting(disk.Path, format);
        }
    }

    internal virtual string ExtendedRomFileName(string model, string path) => ResolveExtendedRomFileName(model, path);
    internal override string? PrepareContent(MachineConfiguration configuration, string session, IReadOnlyList<MediaConfiguration> media) =>
        PrepareContentPath(configuration, session, media);
    internal override string ConvertMediaPath(string path, string conversionDirectory) =>
        AmigaRuntimeMediaFunctions.ConvertScpPathAsync(path, conversionDirectory).GetAwaiter().GetResult();

    private static readonly IReadOnlyDictionary<string, string> KnownKickstartNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [FirmwareCatalogConstants.Hash0B8442C311CA] = UaeFirmwareConstants.Kick31034A1000,
            [FirmwareCatalogConstants.Hash1FA1F93D3D7B] = UaeFirmwareConstants.Kick32034A1000,
            [FirmwareCatalogConstants.Hash85AD74194E87] = UaeFirmwareConstants.Kick33180A500,
            [FirmwareCatalogConstants.Hash82A21C1890CA] = UaeFirmwareConstants.Kick34005A500,
            [FirmwareCatalogConstants.HashDC10D7BDD1B6] = UaeFirmwareConstants.Kick37175A500,
            [FirmwareCatalogConstants.Hash465646C9B672] = UaeFirmwareConstants.Kick37350A600,
            [FirmwareCatalogConstants.HashE40A5DFB3D01] = UaeFirmwareConstants.Kick40063A600,
            [FirmwareCatalogConstants.HashB7CC148386AA] = UaeFirmwareConstants.Kick39106A1200,
            [FirmwareCatalogConstants.Hash646773759326] = UaeFirmwareConstants.Kick40068A1200,
            [FirmwareCatalogConstants.Hash9B8BDD5A3FD3] = UaeFirmwareConstants.Kick39106A4000,
            [FirmwareCatalogConstants.Hash9BDEDDE6A4F3] = UaeFirmwareConstants.Kick40068A4000,
            [FirmwareCatalogConstants.HashF2F241BF0941] = UaeFirmwareConstants.Kick40060CD32,
            [FirmwareCatalogConstants.Hash5F8924D013DD] = UaeFirmwareConstants.Kick40060CD32
        };
    internal static string ResolveKickstartFileName(string model, string sourcePath)
    {
        using var stream = File.OpenRead(sourcePath);
        var md5 = Convert.ToHexString(MD5.HashData(stream));
        if (KnownKickstartNames.TryGetValue(md5, out var knownName)) return knownName;

        stream.Position = FirmwareCatalogConstants.StreamStart;
        Span<byte> header = stackalloc byte[FirmwareCatalogConstants.KickstartHeaderSize];
        if (stream.Read(header) == header.Length)
        {
            var version = (header[FirmwareCatalogConstants.VersionHighByteOffset] << FirmwareCatalogConstants.BitsPerByte) | header[FirmwareCatalogConstants.VersionLowByteOffset];
            var revision = (header[FirmwareCatalogConstants.RevisionHighByteOffset] << FirmwareCatalogConstants.BitsPerByte) | header[FirmwareCatalogConstants.RevisionLowByteOffset];
            var suffix = ResolveKickstartSuffix(model, version, revision);
            if (version is >= FirmwareCatalogConstants.MinimumNativeVersion and <= FirmwareCatalogConstants.MaximumNativeVersion && revision is <= FirmwareCatalogConstants.MaximumFileNameRevision)
                return string.Format(FirmwareCatalogConstants.NativeFileNameFormat, version, revision, suffix);
        }

        return model.ToUpperInvariant() switch
        {
            GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A1000 => UaeFirmwareConstants.Kick32034A1000,
            GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A500PLUS => UaeFirmwareConstants.Kick37175A500,
            GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A600 => UaeFirmwareConstants.Kick40063A600,
            GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A1200 => UaeFirmwareConstants.Kick40068A1200,
            GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A3000 or GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A4000 => UaeFirmwareConstants.Kick40068A4000,
            GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Constants.ModelConstants.CDTV => UaeFirmwareConstants.Kick34005A500,
            GWGUI.Emulation.Commodore.Common.Machines.AmigaCD32.Constants.ModelConstants.CD32 => UaeFirmwareConstants.Kick40060CD32,
            _ => UaeFirmwareConstants.Kick34005A500
        };
    }

    internal static string ResolveExtendedRomFileName(string model, string sourcePath) =>
        model.ToUpperInvariant() switch
        {
            GWGUI.Emulation.Commodore.Common.Machines.AmigaCD32.Constants.ModelConstants.CD32 => UaeFirmwareConstants.Kick40060CD32Ext,
            GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Constants.ModelConstants.CDTV => UaeFirmwareConstants.Kick34005CDTV,
            _ => Path.GetFileName(sourcePath)
        };

    private static string ResolveKickstartSuffix(string model, int version, int revision) => (version, revision) switch
    {
        ((int)NativeKickstartVersion.Kickstart11Ntsc or (int)NativeKickstartVersion.Kickstart11Pal, (int)NativeKickstartRevision.Kickstart11) => GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A1000,
        ((int)NativeKickstartVersion.Kickstart12, (int)NativeKickstartRevision.Kickstart12) or ((int)NativeKickstartVersion.Kickstart13, (int)NativeKickstartRevision.Kickstart13) or ((int)NativeKickstartVersion.Kickstart204, (int)NativeKickstartRevision.Kickstart204) => GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A500,
        ((int)NativeKickstartVersion.Kickstart204, (int)NativeKickstartRevision.Kickstart205A600) or ((int)NativeKickstartVersion.Kickstart31, (int)NativeKickstartRevision.Kickstart31A600) => GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A600,
        ((int)NativeKickstartVersion.Kickstart31, (int)NativeKickstartRevision.Kickstart31Cd32) => GWGUI.Emulation.Commodore.Common.Machines.AmigaCD32.Constants.ModelConstants.CD32,
        ((int)NativeKickstartVersion.Kickstart30, (int)NativeKickstartRevision.Kickstart30) or ((int)NativeKickstartVersion.Kickstart31, (int)NativeKickstartRevision.Kickstart31A1200) when model.Equals(GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A3000, StringComparison.OrdinalIgnoreCase)
            || model.Equals(GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A4000, StringComparison.OrdinalIgnoreCase) => GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A4000,
        ((int)NativeKickstartVersion.Kickstart30, (int)NativeKickstartRevision.Kickstart30) or ((int)NativeKickstartVersion.Kickstart31, (int)NativeKickstartRevision.Kickstart31A1200) => GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A1200,
        _ => model.ToUpperInvariant() switch
        {
            GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A1000 => GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A1000,
            GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A600 => GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A600,
            GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A1200 => GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A1200,
            GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A3000 or GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A4000 => GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A4000,
            GWGUI.Emulation.Commodore.Common.Machines.AmigaCD32.Constants.ModelConstants.CD32 => GWGUI.Emulation.Commodore.Common.Machines.AmigaCD32.Constants.ModelConstants.CD32,
            _ => GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Constants.ModelConstants.A500
        }
    };

    internal static string? PrepareContentPath(MachineConfiguration configuration, string sessionDirectory,
        IReadOnlyList<MediaConfiguration>? resolvedMedia = null, bool createHardwareConfiguration = false)
    {
        var media = resolvedMedia ?? ExternalCore.ResolveConfiguredMedia(configuration);
        if (media.Count == BufferConstants.EmptyCollectionCount && !createHardwareConfiguration) return null;
        if (media.Count == UaeContentConstants.SingleContentCount
            && (!createHardwareConfiguration || media[BufferConstants.FirstCollectionIndex].Category is MediaCategory.Configuration or MediaCategory.WhdLoad))
            return Path.GetFullPath(media[BufferConstants.FirstCollectionIndex].Path);
        var model = (AmigaModel)ModelCatalog.Get(configuration.Model);
        var lines = new List<string> { UaeConfigurationConstants.ConfigurationVersion };
        var options = configuration.Options;
        string Value(string key, string fallback) => options?.GetValueOrDefault(key) ?? fallback;
        void Add(string key, string value)
        {
            if (value.IndexOfAny(UaeConfigurationConstants.InvalidConfigurationCharacters) >= BufferConstants.FirstCollectionIndex) throw new InvalidDataException(key);
            lines.Add(string.Concat(key, UaeConfigurationConstants.AssignmentSeparator, value));
        }
        Add(UaeConfigurationConstants.Cpu, Value(SettingsConstants.OptionCpuModel, ((int)model.DefaultCpu).ToString(CultureInfo.InvariantCulture)));
        Add(UaeConfigurationConstants.ChipsetConfiguration, model.Chipset.ToLowerInvariant());
        Add(UaeConfigurationConstants.ChipMemory, Value(SettingsConstants.OptionChipmemSize,
            (model.ChipMemoryKib / UaeConfigurationConstants.ChipMemoryUnitKib).ToString(CultureInfo.InvariantCulture)));
        Add(UaeConfigurationConstants.SlowMemory, Value(SettingsConstants.OptionBogomemSize,
            (model.SlowMemoryKib / UaeConfigurationConstants.SlowMemoryUnitKib).ToString(CultureInfo.InvariantCulture)));
        Add(UaeConfigurationConstants.FastMemory, Value(SettingsConstants.OptionFastmemSize,
            model.FastMemoryMib.ToString(CultureInfo.InvariantCulture)));
        Add(UaeConfigurationConstants.Z3Memory, Value(SettingsConstants.OptionZ3memSize,
            SettingsValueConstants.DisabledMemory));
        Add(UaeConfigurationConstants.KickstartConfiguration, Path.GetFullPath(configuration.FirmwarePath(SettingsConstants.KickstartPath)));
        if (!string.IsNullOrWhiteSpace(configuration.FirmwarePath(SettingsConstants.ExtendedRomPath)))
            Add(UaeConfigurationConstants.ExtendedRomConfiguration, Path.GetFullPath(configuration.FirmwarePath(SettingsConstants.ExtendedRomPath)));
        Add(UaeConfigurationConstants.NtScConfiguration,
            Value(SettingsConstants.OptionVideoStandard, SettingsValueConstants.PAL)
                .StartsWith(SettingsValueConstants.NTSC, StringComparison.OrdinalIgnoreCase)
                ? UaeConfigurationConstants.Enabled : UaeConfigurationConstants.Disabled);
        var floppy = BufferConstants.FirstCollectionIndex;
        var hardDrive = BufferConstants.FirstCollectionIndex;
        foreach (var item in media)
        {
            var path = Path.GetFullPath(item.Path);
            switch (item.Category)
            {
                case MediaCategory.Floppy:
                    if (floppy >= UaeConfigurationConstants.MaximumFloppyDrives) throw new InvalidDataException(path);
                    var floppyIndex = item.SlotIndex ?? floppy++;
                    if (floppyIndex >= UaeConfigurationConstants.MaximumFloppyDrives) throw new InvalidDataException(path);
                    Add(UaeConfigurationConstants.FloppyPrefix + floppyIndex, path);
                    break;
                case MediaCategory.CompactDisc:
                    Add(UaeConfigurationConstants.CdImage, path + UaeConfigurationConstants.CdImageSuffix);
                    break;
                case MediaCategory.HardDrive:
                    var access = item.IsReadOnly ? UaeConfigurationConstants.ReadOnly : UaeConfigurationConstants.ReadWrite;
                    var drive = UaeConfigurationConstants.HardDrivePrefix + hardDrive++;
                    Add(Directory.Exists(path) ? UaeConfigurationConstants.FileSystemPrefix : UaeConfigurationConstants.HardfilePrefix,
                        Directory.Exists(path) ? string.Format(UaeConfigurationConstants.DirectoryMountFormat, access, drive, path)
                            : string.Format(UaeConfigurationConstants.HardfileMountFormat, access, drive, path, UaeConfigurationConstants.HardfileGeometry));
                    break;
                default: throw new InvalidDataException(path);
            }
        }
        var content = Path.Combine(sessionDirectory, CoreDirectoryConstants.ContentDirectoryName);
        Directory.CreateDirectory(content);
        var configurationPath = Path.Combine(content, UaeConfigurationConstants.ConfigurationFile);
        File.WriteAllLines(configurationPath, lines, new System.Text.UTF8Encoding(false));
        return configurationPath;
    }

}
