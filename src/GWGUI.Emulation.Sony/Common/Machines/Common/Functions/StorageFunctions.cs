using GWGUI.Emulation.HardDisks;
using System.IO;
using PlayStationMemoryCard = GWGUI.Emulation.Sony.Common.Machines.PlayStation.Functions.MemoryCardFunctions;

namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static class StorageSettingsFunctions
{
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var adapter = EmulatorCatalog.CreateAdapters().FirstOrDefault(item => item.EmulatorId == configuration.EmulatorId);
        var mediaAdapter = adapter as IEmulatorMediaAdapter;
        var options = configuration.Options ?? new Dictionary<string, string>();
        var devices = new List<EmulationMediaDevice>();
        if (model.Id is ModelConstants.PlayStation or ModelConstants.PlayStation2
            && mediaAdapter?.HasFixedMemoryCardDirectories != true)
            for (var index = 0; index < MemoryCardCount(configuration, mediaAdapter); index++)
                devices.Add(MemoryCard(index, model.Id == ModelConstants.PlayStation2));
        if (model.Id == ModelConstants.Psp)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.MemoryCard0, EmulationMediaType.MemoryCard, [],
                RequiresMachineRecreation: true, IsPermanent: true,
                ModelResourceKey: StorageSettingsFunctionsConstants.MemoryStickModelResource,
                DisplayLabelResourceKey: StorageSettingsFunctionsConstants.MemoryStickModelResource,
                ConfigurationKind: EmulationStorageConfigurationKind.MemoryCard,
                UsesDirectory: true, DefaultImageSubdirectory: StorageSettingsFunctionsConstants.MemoryStickSubdirectory));
        if (mediaAdapter?.HasFixedMemoryCardDirectories == true)
            for (var index = 0; index < StorageSettingsFunctionsConstants.MemoryCardCount; index++)
                devices.Add(new EmulationMediaDevice(new EmulationMediaSlot(EmulationMediaCategory.MemoryCard, index),
                    EmulationMediaType.MemoryCard, [], IsRemovable: false, IsPermanent: true, UsesDirectory: true,
                    DisplayLabel: StorageSettingsFunctionsConstants.MemoryCardSlotPrefix + " " + (index + 1),
                    DisplayLabelResourceKey: StorageSettingsFunctionsConstants.MemoryCardSlotResource,
                    ModelResourceKey: StorageSettingsFunctionsConstants.MemoryCardModelResource));
        if (model.HasBuiltInMemoryCard)
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.MemoryCard0, EmulationMediaType.MemoryCard, Machines.PocketStation.Constants.MediaConstants.FlashImageExtensions, RequiresMachineRecreation: true, IsPermanent: true));
        if (model.SupportsCompactDiscDrive && adapter is not null)
        {
            var discFields = (adapter as IEmulatorSettingsAdapter)?.GetStorageDeviceConfigurationFields(configuration, EmulationMediaCategory.CompactDiscDrive) ?? [];
            devices.Add(new EmulationMediaDevice(EmulationMediaSlot.Cd0, EmulationMediaType.CompactDisc,
                mediaAdapter?.SupportedContentExtensions ?? [],
                RequiresMachineRecreation: true,
                DisplayLabel: model.Id == ModelConstants.Psp ? "UMD" : model.Id == ModelConstants.PlayStation2 ? "DVD" : StorageSettingsFunctionsConstants.CompactDiscDriveLabel,
                ModelResourceKey: model.Id == ModelConstants.Psp ? StorageSettingsFunctionsConstants.UmdModelResource : model.Id == ModelConstants.PlayStation2 ? "Emulation.Sony.Storage.DVD" : StorageSettingsFunctionsConstants.CdModelResource,
                IsPermanent: model.HasBuiltInCompactDiscDrive,
                ConfigurationKind: discFields.Count > 0 ? EmulationStorageConfigurationKind.CompactDiscDrive : EmulationStorageConfigurationKind.None,
                ConfigurationFields: discFields,
                TypeResourceKey: model.Id == ModelConstants.PlayStation2 ? "Emulation.Sony.Storage.DVD.Device" : null));
        }
        devices.AddRange(mediaAdapter?.GetStorageDevices(configuration) ?? []);
        devices = devices.OrderByDescending(device => device.Slot.Category == EmulationMediaCategory.CompactDiscDrive).ThenByDescending(device => device.IsPermanent).ThenBy(device => device.Slot.Index).ToList();
        var mounted = EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? []);
        var configured = devices.Where(device => device.Slot.Category switch
        {
            EmulationMediaCategory.MemoryCard => device.IsPermanent || model.HasBuiltInMemoryCard && device.Slot == EmulationMediaSlot.MemoryCard0
                || mounted.Any(media => media.Slot == device.Slot)
                || OptionBool(options, StorageSettingsFunctionsConstants.MemoryCardEnabledOptionPrefix + device.Slot.Index),
            EmulationMediaCategory.CompactDiscDrive => model.HasBuiltInCompactDiscDrive
                || OptionBool(options, StorageSettingsFunctionsConstants.CompactDiscDriveEnabledOption),
            EmulationMediaCategory.HardDisk => mounted.Any(media => media.Slot == device.Slot)
                || OptionBool(options, StorageSettingsFunctionsConstants.HardDiskEnabledOptionPrefix + device.Slot.Index),
            _ => false
        }).Select(device => device.Slot).ToArray();
        var deviceSettings = devices.Where(device => device.MediaType == EmulationMediaType.MemoryCard)
            .Select(device => new EmulationStorageDeviceSettings(device.Slot,
                MemoryCardPath: options.GetValueOrDefault(StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + device.Slot.Index)
                    ?? mediaAdapter?.GetMemoryCardDirectory(device.Slot.Index))).ToList();
        deviceSettings.Add(new EmulationStorageDeviceSettings(EmulationMediaSlot.Cd0, CompactDisc: new EmulationCompactDiscDriveSettings(Options: options)));
        return new EmulationStorageSettings(devices, configured, mounted, deviceSettings);
    }

    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var mediaAdapter = EmulatorCatalog.CreateAdapters().FirstOrDefault(item => item.EmulatorId == configuration.EmulatorId) as IEmulatorMediaAdapter;
        var adapterDevices = mediaAdapter?.GetStorageDevices(configuration) ?? [];
        var hardDisks = settings.MountedMedia.Where(item => item.Type == EmulationMediaType.HardDisk && settings.ConfiguredSlots.Contains(item.Slot)).ToArray();
        if (hardDisks.Select(item => item.Slot).Distinct().Count() != hardDisks.Length) throw new InvalidDataException(nameof(settings));
        foreach (var disk in hardDisks)
        {
            var device = adapterDevices.SingleOrDefault(item => item.Slot == disk.Slot && item.MediaType == disk.Type)
                ?? throw new InvalidDataException(nameof(settings));
            var format = device.HardDiskFormats?.FirstOrDefault(item => item.Extension.Equals(Path.GetExtension(disk.Path), StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidDataException(disk.Path);
            HardDiskImageValidation.ValidateExisting(disk.Path, format);
        }
        var media = settings.MountedMedia.Where(item => item.Type is not (EmulationMediaType.HardDisk or EmulationMediaType.MemoryCard) || settings.ConfiguredSlots.Contains(item.Slot)).Select((item, index) => new MediaConfiguration(
            item.Path, item.Type switch
            {
                EmulationMediaType.Floppy => MediaCategory.Floppy,
                EmulationMediaType.Cassette => MediaCategory.Cassette,
                EmulationMediaType.Cartridge => MediaCategory.Cartridge,
                EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
                EmulationMediaType.MemoryCard => MediaCategory.MemoryCard,
                EmulationMediaType.HardDisk => MediaCategory.HardDisk,
                _ => throw new ArgumentOutOfRangeException(nameof(settings), item.Type, null)
            }, IsReadOnly: item.IsReadOnly, IsInserted: item.IsInserted, MountOrder: index, SlotIndex: item.Slot.Index)).ToArray();
        var model = ModelCatalog.Get(configuration.Model);
        foreach (var item in media)
            if (!ConfigurationValidationFunctions.Supports(model, item.Category, adapterDevices))
                throw new ArgumentOutOfRangeException(nameof(settings), item.Category, null);
        var options = new Dictionary<string, string>(configuration.Options
            ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            [StorageSettingsFunctionsConstants.CompactDiscDriveEnabledOption] = settings.ConfiguredSlots.Contains(EmulationMediaSlot.Cd0).ToString()
        };
        foreach (var device in settings.AvailableDevices.Where(device => device.MediaType == EmulationMediaType.MemoryCard))
            options[StorageSettingsFunctionsConstants.MemoryCardEnabledOptionPrefix + device.Slot.Index] = settings.ConfiguredSlots.Contains(device.Slot).ToString();
        foreach (var device in adapterDevices.Where(device => device.MediaType == EmulationMediaType.HardDisk))
            options[StorageSettingsFunctionsConstants.HardDiskEnabledOptionPrefix + device.Slot.Index] = settings.ConfiguredSlots.Contains(device.Slot).ToString();
        var cdOptions = settings.DeviceSettings?.FirstOrDefault(item => item.Slot == EmulationMediaSlot.Cd0) ?.CompactDisc?.Options;
        var cdFields = settings.AvailableDevices.FirstOrDefault(device => device.Slot == EmulationMediaSlot.Cd0) ?.ConfigurationFields ?? [];
        foreach (var field in cdFields)
            if (cdOptions?.TryGetValue(field.Id, out var selected) == true) options[field.Id] = selected;
        if (model.Id is ModelConstants.PlayStation or ModelConstants.PlayStation2 or ModelConstants.Psp
            && mediaAdapter?.HasFixedMemoryCardDirectories != true)
            for (var index = 0; index < MemoryCardCount(configuration, mediaAdapter); index++)
            {
                var slot = new EmulationMediaSlot(EmulationMediaCategory.MemoryCard, index);
                if (!settings.AvailableDevices.Any(device => device.Slot == slot)) continue;
                var path = settings.DeviceSettings?.FirstOrDefault(item => item.Slot == slot)?.MemoryCardPath;
                options[StorageSettingsFunctionsConstants.MemoryCardEnabledOptionPrefix + index] = settings.ConfiguredSlots.Contains(slot).ToString();
                if (string.IsNullOrWhiteSpace(path))
                    options.Remove(StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + index);
                else
                {
                    if (model.Id == ModelConstants.Psp)
                    {
                        if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);
                    }
                    else
                    {
                        if (!File.Exists(path)) throw new FileNotFoundException(null, path);
                        var size = model.Id == ModelConstants.PlayStation2 ? StorageSettingsFunctionsConstants.Ps2MemoryCardSize : StorageSettingsFunctionsConstants.MemoryCardSize;
                        if (new FileInfo(path).Length != size) throw new InvalidDataException(nameof(settings));
                    }
                    options[StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + index] = Path.GetFullPath(path);
                }
            }
        return configuration with { Media = media, Options = options };
    }

    private static int MemoryCardCount(MachineConfiguration configuration, IEmulatorMediaAdapter? adapter) => adapter?.GetMemoryCardCount(configuration) ?? (configuration.Model == ModelConstants.Psp ? 1 : StorageSettingsFunctionsConstants.MemoryCardCount);

    private static EmulationMediaDevice MemoryCard(int index, bool ps2 = false)
    {
        var size = ps2 ? StorageSettingsFunctionsConstants.Ps2MemoryCardSize : StorageSettingsFunctionsConstants.MemoryCardSize;
        var extensions = ps2 ? new[] { StorageSettingsFunctionsConstants.Ps2MemoryCardExtension } : new[] { StorageSettingsFunctionsConstants.MemoryCardExtension, StorageSettingsFunctionsConstants.AlternativeMemoryCardExtension };
        return new(
        new EmulationMediaSlot(EmulationMediaCategory.MemoryCard, index), EmulationMediaType.MemoryCard,
        extensions,
        DisplayLabel: StorageSettingsFunctionsConstants.MemoryCardSlotPrefix + " " + (index + 1),
        DisplayLabelResourceKey: StorageSettingsFunctionsConstants.MemoryCardSlotResource,
        RequiresMachineRecreation: true,
        IsPermanent: ps2,
        ConfigurationKind: EmulationStorageConfigurationKind.MemoryCard,
        ModelResourceKey: StorageSettingsFunctionsConstants.MemoryCardModelResource,
        FixedImageSizeBytes: size,
        DefaultImageFileName: StorageSettingsFunctionsConstants.MemoryCardFilePrefix + (index + 1)
            + extensions[0],
        DialogPresentation: new EmulationStorageDialogPresentation(
            StorageSettingsFunctionsConstants.MemoryCardDeviceResource, StorageSettingsFunctionsConstants.MemoryCardDeviceResource,
            StorageSettingsFunctionsConstants.ChooseResource, StorageSettingsFunctionsConstants.CreateResource,
            StorageSettingsFunctionsConstants.AssociatedResource, StorageSettingsFunctionsConstants.FileNameResource,
            StorageSettingsFunctionsConstants.FolderResource, StorageSettingsFunctionsConstants.DeviceNameResource,
            StorageSettingsFunctionsConstants.DeviceIdResource, StorageSettingsFunctionsConstants.ModelResource,
            StorageSettingsFunctionsConstants.UseCardResource, StorageSettingsFunctionsConstants.ExistingCardHintResource,
            StorageSettingsFunctionsConstants.CardRequiredResource, StorageSettingsFunctionsConstants.InvalidCardSizeResource,
            StorageSettingsFunctionsConstants.InvalidCardFormatResource),
        HardDiskFormats: extensions.Select(extension => new HardDiskImageFormat(StorageSettingsFunctionsConstants.MemoryCardModel,
            extension, StorageSettingsFunctionsConstants.MemoryCardModel, size, size)
        {
            MinimumBytes = size,
            InitializeBlankImage = ps2 ? InitializeBlankPs2Card : PlayStationMemoryCard.InitializeBlank
        }).ToArray(),
        DefaultImageSubdirectory: Path.Combine("Memory Card", "Sony", ps2 ? ModelConstants.PlayStation2 : ModelConstants.PlayStation));
    }

    internal static void InitializeBlankPs2Card(Stream image)
    {
        // Match PCSX2's erased flash, including the sixteen spare bytes per page.
        // The console BIOS formats the filesystem on first use.
        var erased = new byte[528 * 16];
        Array.Fill(erased, (byte)0xFF);
        image.Position = 0;
        for (var written = 0; written < StorageSettingsFunctionsConstants.Ps2MemoryCardSize; written += erased.Length)
            image.Write(erased);
    }

    private static bool OptionBool(IReadOnlyDictionary<string, string> options, string key) => options.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) && parsed;
}
