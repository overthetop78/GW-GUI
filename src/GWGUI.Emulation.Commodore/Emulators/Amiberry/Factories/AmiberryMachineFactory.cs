using GWGUI.Emulation.Commodore.Emulators.Common.Constants;
using System.IO;
using System.Globalization;
using GWGUI.Emulation.Commodore.Emulators.Amiberry.Constants;

namespace GWGUI.Emulation.Commodore.Emulators.Amiberry.Factories;

internal sealed class AmiberryMachineFactory : MachineFactory
{
    public override EmulatorCatalogEntry CatalogEntry { get; } = new(Emulator.Amiberry,
        new(EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorDescriptionConstants.AmigaDescriptionResourceKey,
            MachineCatalog.All.Select(machine => machine.Id).ToHashSet(StringComparer.Ordinal)));
    internal override CoreDefinition CoreDefinition { get; } = new(
        EmulatorConstants.DisplayName, EmulatorConstants.LibraryFile, EmulatorConstants.DownloadUrl);
    internal override string? KickstartOverrideOption => OptionConstants.Kickstart;
    internal override string ExtendedRomFileName(string model, string path) =>
        model.Equals(OptionConstants.Cd32, StringComparison.OrdinalIgnoreCase)
            ? OptionConstants.Cd32ExtendedRom : OptionConstants.CdtvExtendedRom;

    internal override IReadOnlyDictionary<string, string> NativeOptions(MachineConfiguration configuration,
        IReadOnlyList<MediaConfiguration> media)
    {
        var options = (configuration.Options ?? new Dictionary<string, string>())
            .Where(pair => pair.Key.StartsWith(OptionConstants.Prefix, StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        options[OptionConstants.Model] = configuration.Model is OptionConstants.A2000 or OptionConstants.A3000
            ? OptionConstants.A500Plus : ModelCatalog.BackendModelFor(configuration.Model);
        options[OptionConstants.Kickstart] = Path.GetFullPath(configuration.KickstartPath);
        if (configuration.Options?.TryGetValue(SettingsConstants.OptionVideoStandard, out var standard) == true)
            options[OptionConstants.VideoStandard] = standard.StartsWith(SettingsDescriptionFunctionsConstants.NTSC,
                StringComparison.OrdinalIgnoreCase) ? OptionConstants.NtSc : OptionConstants.Pal;
        return options;
    }

    internal override string? PrepareContent(MachineConfiguration configuration, string session,
        IReadOnlyList<MediaConfiguration> media)
    {
        if (media.Count == 1 && media[0].Category is MediaCategory.Configuration or MediaCategory.WhdLoad)
            return Path.GetFullPath(media[0].Path);
        if (media.Count > 0 && configuration.Model is not (OptionConstants.A2000 or OptionConstants.A3000)
            && !configuration.MountFloppiesInSeparateDrives
            && media.Select(item => item.Category).Distinct().Count() == 1)
            return ExternalCore.PrepareContentPath(configuration, session, media);

        var model = ModelCatalog.Get(configuration.Model);
        var lines = new List<string> { OptionConstants.ConfigurationVersion };
        var options = configuration.Options;
        string Value(string key, string fallback) => options?.GetValueOrDefault(key) ?? fallback;
        void Add(string key, string value)
        {
            if (value.IndexOfAny(['\r', '\n']) >= 0) throw new InvalidDataException(key);
            lines.Add($"{key}={value}");
        }
        Add(OptionConstants.Cpu, Value(SettingsConstants.OptionCpuModel, model.DefaultCpu));
        Add(OptionConstants.ChipsetConfiguration, model.Chipset.ToLowerInvariant());
        Add(OptionConstants.ChipMemory, Value(SettingsConstants.OptionChipmemSize,
            (model.ChipMemoryKib / OptionConstants.ChipMemoryUnitKib).ToString(CultureInfo.InvariantCulture)));
        Add(OptionConstants.SlowMemory, Value(SettingsConstants.OptionBogomemSize,
            (model.SlowMemoryKib / OptionConstants.SlowMemoryUnitKib).ToString(CultureInfo.InvariantCulture)));
        Add(OptionConstants.FastMemory, Value(SettingsConstants.OptionFastmemSize,
            model.FastMemoryMib.ToString(CultureInfo.InvariantCulture)));
        Add(OptionConstants.Z3Memory, Value(SettingsConstants.OptionZ3memSize,
            SettingsDescriptionFunctionsConstants.Value0));
        Add(OptionConstants.KickstartConfiguration, Path.GetFullPath(configuration.KickstartPath));
        if (!string.IsNullOrWhiteSpace(configuration.ExtendedRomPath))
            Add(OptionConstants.ExtendedRomConfiguration, Path.GetFullPath(configuration.ExtendedRomPath));
        Add(OptionConstants.NtScConfiguration,
            Value(SettingsConstants.OptionVideoStandard, SettingsDescriptionFunctionsConstants.PAL)
                .StartsWith(SettingsDescriptionFunctionsConstants.NTSC, StringComparison.OrdinalIgnoreCase)
                ? OptionConstants.Enabled : OptionConstants.Disabled);
        var floppy = 0;
        var hardDrive = 0;
        foreach (var item in media)
        {
            var path = Path.GetFullPath(item.Path);
            switch (item.Category)
            {
                case MediaCategory.Floppy:
                    if (floppy >= OptionConstants.MaximumFloppyDrives) throw new InvalidDataException(path);
                    Add(OptionConstants.FloppyPrefix + floppy++, path);
                    break;
                case MediaCategory.CompactDisc:
                    Add(OptionConstants.CdImage, path + OptionConstants.CdImageSuffix);
                    break;
                case MediaCategory.HardDrive:
                    var access = item.IsReadOnly ? OptionConstants.ReadOnly : OptionConstants.ReadWrite;
                    var drive = OptionConstants.HardDrivePrefix + hardDrive++;
                    Add(Directory.Exists(path) ? OptionConstants.FileSystemPrefix : OptionConstants.HardfilePrefix,
                        Directory.Exists(path) ? $"{access},{drive}:{drive}:{path},0"
                            : $"{access},{drive}:{path}{OptionConstants.HardfileGeometry}");
                    break;
                default: throw new InvalidDataException(path);
            }
        }
        var content = Path.Combine(session, CoreDirectoryConstants.ContentDirectoryName);
        Directory.CreateDirectory(content);
        var configurationPath = Path.Combine(content, OptionConstants.ConfigurationFile);
        File.WriteAllLines(configurationPath, lines, new System.Text.UTF8Encoding(false));
        return configurationPath;
    }
}
