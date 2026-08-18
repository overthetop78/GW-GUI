using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using System.IO;

namespace GWGUI.App.Controls;

internal static class AtariStorageSettingsFunctions
{
    private const string DeviceOptionPrefix = "storage.device.";
    private const string ModelOptionPrefix = "storage.model.";
    private const string SpeedOptionPrefix = "storage.speed.";
    private const string WriteProtectedOptionPrefix = "storage.writeProtected.";
    private const string RedirectWritesOptionPrefix = "storage.redirectWrites.";

    internal static AtariStorageView Create(AtariMachineConfiguration configuration)
    {
        var allAvailable = AtariCompatibilityCatalog.Get(configuration.Model).Media
            .Where(rule => rule.Availability == AtariMediaAvailability.Available)
            .ToArray();
        var primary = PrimaryDevice(configuration.Model);
        var types = allAvailable.Select(rule => rule.Kind).Distinct()
            .Select(kind => new AtariStorageTypeChoice(kind, KindName(kind))).ToArray();
        var slots = allAvailable.GroupBy(rule => rule.Kind).ToDictionary(group => group.Key,
            group => (IReadOnlyList<AtariStorageSlotChoice>)group.SelectMany(rule => rule.Slots).Distinct()
                .Where(slot => IsPhysicalSlot(configuration.Model, group.Key, slot))
                .Select(slot => new AtariStorageSlotChoice(slot,
                    DeviceIdentifier(configuration.Model, group.Key, slot))).ToArray());
        var buses = types.ToDictionary(type => type.Kind,
            type => (IReadOnlyList<AtariStorageBusChoice>)Buses(configuration.Model, type.Kind)
                .Select(bus => new AtariStorageBusChoice(bus, bus.ToString())).ToArray());
        var primaryMedia = configuration.Media.FirstOrDefault(item => item.Slot == primary.Slot)
            ?? new AtariMediaConfiguration(string.Empty, primary.Kind, primary.Slot);
        var configuredSlots = configuration.Options
            .Where(option => option.Key.StartsWith(DeviceOptionPrefix, StringComparison.Ordinal))
            .Select(option => Enum.TryParse<EmulationMediaSlot>(option.Key[DeviceOptionPrefix.Length..], out var slot)
                && Enum.TryParse<AtariMediaKind>(option.Value, out var kind) ? (Slot: slot, Kind: kind) : default)
            .Where(item => item != default && IsAvailable(allAvailable,
                new AtariMediaConfiguration(string.Empty, item.Kind, item.Slot)))
            .ToArray();
        var extraMedia = configuration.Media
            .Where(item => item.Slot != primary.Slot && IsAvailable(allAvailable, item))
            .Select(item => (item.Slot, item.Kind));
        var devices = new[] { Device(configuration, primaryMedia, false) }
            .Concat(configuredSlots.Concat(extraMedia)
                .GroupBy(item => item.Slot)
                .Select(group => group.Last())
                .Where(item => item.Slot != primary.Slot)
                .Select(item => Device(configuration,
                    configuration.Media.FirstOrDefault(media => media.Slot == item.Slot)
                    ?? new AtariMediaConfiguration(string.Empty, item.Kind, item.Slot), true)))
            .ToArray();
        return new AtariStorageView(types, slots, buses, devices);
    }

    internal static bool CanAdd(AtariMachineModel model, AtariStorageView view) =>
        AtariCompatibilityCatalog.Get(model).Core is AtariCoreKind.Hatari or AtariCoreKind.Atari800
        && view.Types.Any(type => view.Slots[type.Kind].Any(slot => view.Devices.All(device =>
            device.Configuration.Slot != slot.Slot)));

    internal static bool IsPrimaryDevice(AtariMachineModel model, EmulationMediaSlot slot) =>
        PrimaryDevice(model).Slot == slot;

    internal static string MachineName(AtariMachineModel model) =>
        AtariConfigurationCatalogFunctions.ModelName(model);

    internal static AtariMachineFamily Family(AtariMachineModel model) => model switch
    {
        AtariMachineModel.St or AtariMachineModel.Stf or AtariMachineModel.Stfm or AtariMachineModel.MegaSt
            or AtariMachineModel.Ste or AtariMachineModel.MegaSte or AtariMachineModel.Tt
            or AtariMachineModel.Falcon => AtariMachineFamily.St,
        AtariMachineModel.Atari400 or AtariMachineModel.Atari800 or AtariMachineModel.Atari800Xl
            or AtariMachineModel.Atari130Xe or AtariMachineModel.ModernXlXe320K
            or AtariMachineModel.ModernXlXe576K or AtariMachineModel.ModernXlXe1088K
            or AtariMachineModel.Xegs => AtariMachineFamily.EightBit,
        AtariMachineModel.Atari5200 => AtariMachineFamily.Atari5200,
        AtariMachineModel.Atari2600 => AtariMachineFamily.Atari2600,
        AtariMachineModel.Atari7800 => AtariMachineFamily.Atari7800,
        AtariMachineModel.Lynx => AtariMachineFamily.Lynx,
        AtariMachineModel.Jaguar or AtariMachineModel.JaguarCd => AtariMachineFamily.Jaguar,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };

    internal static string DeviceIdentifier(AtariMachineModel model, AtariMediaKind kind,
        EmulationMediaSlot slot) => (Family(model), kind, slot) switch
    {
        (AtariMachineFamily.St, AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0) => "A:",
        (AtariMachineFamily.St, AtariMediaKind.Floppy, EmulationMediaSlot.Floppy1) => "B:",
        (AtariMachineFamily.EightBit, AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0) => "D1:",
        (AtariMachineFamily.EightBit, AtariMediaKind.Floppy, EmulationMediaSlot.Floppy1) => "D2:",
        (AtariMachineFamily.EightBit, AtariMediaKind.Floppy, EmulationMediaSlot.Floppy2) => "D3:",
        (AtariMachineFamily.EightBit, AtariMediaKind.Floppy, EmulationMediaSlot.Floppy3) => "D4:",
        (_, AtariMediaKind.HardDisk or AtariMediaKind.Directory, EmulationMediaSlot.HardDisk0) => "C:",
        (_, AtariMediaKind.Cassette, _) => "C:",
        (_, AtariMediaKind.CompactDisc, _) => "CD",
        (_, AtariMediaKind.Cartridge, _) => LocExtension.Get(AtariStorageSettingsConstants.CartridgeResource),
        _ => slot.ToString()
    };

    internal static IReadOnlyList<FloppyDriveModelChoice> FloppyModels(AtariMachineModel model)
    {
        if (Family(model) == AtariMachineFamily.St)
        {
            var models = new List<FloppyDriveModelChoice>
            {
                new("atarist.720", LocExtension.Get("Format.atarist.720"), 737_280)
            };
            if (AtariStModelCatalog.Get(model).Storage.Contains(AtariStStorageCapability.FloppyHighDensity))
                models.Add(new FloppyDriveModelChoice("atarist.1440",
                    LocExtension.Get("Format.atarist.1440"), 1_474_560));
            return models;
        }
        return
        [
            new("atari.90", LocExtension.Get("Format.atari.90"), 92_160),
            new("atari.130", LocExtension.Get("Format.atari.130"), 133_120),
            new("atari.180", LocExtension.Get("Format.atari.180"), 184_320)
        ];
    }

    internal static FloppyDriveSettings FloppySettings(AtariMachineConfiguration configuration,
        EmulationMediaSlot slot)
    {
        var models = FloppyModels(configuration.Model);
        return new FloppyDriveSettings(
            Option(configuration, ModelOptionPrefix, slot) ?? models[0].Value,
            Option(configuration, SpeedOptionPrefix, slot) ?? "100",
            bool.TryParse(Option(configuration, WriteProtectedOptionPrefix, slot), out var writeProtected)
                && writeProtected,
            bool.TryParse(Option(configuration, RedirectWritesOptionPrefix, slot), out var redirectWrites)
                && redirectWrites);
    }

    internal static AtariMachineConfiguration AddDevice(AtariMachineConfiguration source,
        AtariMediaKind kind, EmulationMediaSlot slot)
    {
        var options = new Dictionary<string, string>(source.Options)
        {
            [$"{DeviceOptionPrefix}{slot}"] = kind.ToString()
        };
        if (kind == AtariMediaKind.Floppy)
            options[$"{ModelOptionPrefix}{slot}"] = FloppyModels(source.Model)[0].Value;
        return Replace(source, source.Media, options);
    }

    internal static AtariMachineConfiguration ConfigureFloppy(AtariMachineConfiguration source,
        EmulationMediaSlot slot, FloppyDriveSettings settings)
    {
        var options = new Dictionary<string, string>(source.Options)
        {
            [$"{ModelOptionPrefix}{slot}"] = settings.Model,
            [$"{SpeedOptionPrefix}{slot}"] = settings.Speed,
            [$"{WriteProtectedOptionPrefix}{slot}"] = settings.WriteProtected.ToString(),
            [$"{RedirectWritesOptionPrefix}{slot}"] = settings.RedirectWrites.ToString()
        };
        return Replace(source, source.Media, options);
    }

    private static bool IsAvailable(IReadOnlyList<AtariMediaCompatibilityRule> rules,
        AtariMediaConfiguration media) => rules.Any(rule => rule.Kind == media.Kind
            && rule.Slots.Contains(media.Slot));

    private static (AtariMediaKind Kind, EmulationMediaSlot Slot) PrimaryDevice(AtariMachineModel model) => model switch
    {
        AtariMachineModel.JaguarCd => (AtariMediaKind.CompactDisc, EmulationMediaSlot.Cd0),
        AtariMachineModel.Atari2600 or AtariMachineModel.Atari5200 or AtariMachineModel.Atari7800
            or AtariMachineModel.Lynx or AtariMachineModel.Jaguar or AtariMachineModel.Xegs
            => (AtariMediaKind.Cartridge, EmulationMediaSlot.Cartridge0),
        _ => (AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0)
    };

    internal static AtariMachineConfiguration AddOrReplace(AtariMachineConfiguration source,
        AtariMediaConfiguration media, EmulationMediaSlot? replacedSlot)
    {
        Validate(source.Model, media);
        if (source.Media.Any(item => item.Slot == media.Slot && item.Slot != replacedSlot))
            throw new InvalidOperationException(AtariStorageSettingsConstants.IdentifierResource);
        var items = source.Media.Where(item => item.Slot != replacedSlot).Append(media).ToArray();
        return ReplaceMedia(source, items);
    }

    internal static AtariMachineConfiguration Remove(AtariMachineConfiguration source,
        EmulationMediaSlot slot)
    {
        var options = source.Options.Where(option => !option.Key.EndsWith($".{slot}", StringComparison.Ordinal))
            .ToDictionary(option => option.Key, option => option.Value);
        return Replace(source, source.Media.Where(item => item.Slot != slot).ToArray(), options);
    }

    internal static void Validate(AtariMachineModel model, AtariMediaConfiguration media)
    {
        var compatibility = AtariCompatibilityCatalog.Get(model);
        if (!compatibility.Media.Any(rule => rule.Kind == media.Kind
                && rule.Availability == AtariMediaAvailability.Available && rule.Slots.Contains(media.Slot)))
            throw new InvalidOperationException(AtariStorageSettingsConstants.NoDeviceResource);
        if (media.StorageBus is { } bus && !SupportsBus(model, bus))
            throw new InvalidOperationException(AtariStorageSettingsConstants.NoDeviceResource);
        if (string.IsNullOrWhiteSpace(media.Path))
            throw new ArgumentException(AtariStorageSettingsConstants.PathResource, nameof(media));
    }

    internal static bool IsRemovable(AtariMediaKind kind) => kind is AtariMediaKind.Floppy
        or AtariMediaKind.Cassette or AtariMediaKind.Cartridge or AtariMediaKind.CompactDisc;

    private static IReadOnlyList<AtariStorageBus> Buses(AtariMachineModel model, AtariMediaKind kind)
    {
        if (kind == AtariMediaKind.Directory) return [AtariStorageBus.Gemdos];
        if (kind != AtariMediaKind.HardDisk || AtariCompatibilityCatalog.Get(model).Core != AtariCoreKind.Hatari)
            return [];
        var storage = AtariStModelCatalog.Get(model).Storage;
        var buses = new List<AtariStorageBus>();
        if (storage.Contains(AtariStStorageCapability.Acsi)) buses.Add(AtariStorageBus.Acsi);
        if (storage.Contains(AtariStStorageCapability.Ide)) buses.Add(AtariStorageBus.Ide);
        return buses;
    }

    private static bool SupportsBus(AtariMachineModel model, AtariStorageBus bus)
    {
        if (AtariCompatibilityCatalog.Get(model).Core != AtariCoreKind.Hatari) return false;
        var storage = AtariStModelCatalog.Get(model).Storage;
        return bus switch
        {
            AtariStorageBus.Acsi => storage.Contains(AtariStStorageCapability.Acsi),
            AtariStorageBus.Ide => storage.Contains(AtariStStorageCapability.Ide),
            AtariStorageBus.Gemdos => storage.Contains(AtariStStorageCapability.GemdosDirectory),
            _ => false
        };
    }

    private static AtariMachineConfiguration ReplaceMedia(AtariMachineConfiguration source,
        IReadOnlyList<AtariMediaConfiguration> media) => Replace(source, media, source.Options);

    private static AtariMachineConfiguration Replace(AtariMachineConfiguration source,
        IReadOnlyList<AtariMediaConfiguration> media, IReadOnlyDictionary<string, string> options) =>
        new(source.Model, source.Firmwares, media,
        options, source.Input, source.Id, source.SchemaVersion, source.AudioEnabled,
        source.VideoRenderer, source.Folders);

    private static AtariStorageDeviceItem Device(AtariMachineConfiguration configuration,
        AtariMediaConfiguration media, bool canRemove) => new(media,
        DeviceIdentifier(configuration.Model, media.Kind, media.Slot),
        DeviceModel(configuration, media), canRemove);

    private static string DeviceModel(AtariMachineConfiguration configuration, AtariMediaConfiguration media)
    {
        if (media.Kind == AtariMediaKind.Floppy)
        {
            var choices = FloppyModels(configuration.Model);
            var selected = Option(configuration, ModelOptionPrefix, media.Slot);
            return choices.FirstOrDefault(choice => choice.Value == selected)?.DisplayName
                ?? choices[0].DisplayName;
        }
        return media.Kind switch
        {
            AtariMediaKind.HardDisk => media.StorageBus?.ToString().ToUpperInvariant() ?? "ACSI/IDE",
            AtariMediaKind.Directory => "GEMDOS",
            AtariMediaKind.CompactDisc => "CD-ROM",
            AtariMediaKind.Cassette => LocExtension.Get(AtariStorageSettingsConstants.CassetteResource),
            AtariMediaKind.Cartridge => LocExtension.Get(AtariStorageSettingsConstants.CartridgeResource),
            _ => KindName(media.Kind)
        };
    }

    private static string? Option(AtariMachineConfiguration configuration, string prefix,
        EmulationMediaSlot slot) => configuration.Options.TryGetValue($"{prefix}{slot}", out var value)
        ? value : null;

    private static bool IsPhysicalSlot(AtariMachineModel model, AtariMediaKind kind, EmulationMediaSlot slot) =>
        Family(model) != AtariMachineFamily.St
        || kind != AtariMediaKind.Floppy
        || slot is EmulationMediaSlot.Floppy0 or EmulationMediaSlot.Floppy1;

    private static string KindName(AtariMediaKind kind) => LocExtension.Get(kind switch
    {
        AtariMediaKind.Floppy => AtariStorageSettingsConstants.FloppyResource,
        AtariMediaKind.HardDisk => AtariStorageSettingsConstants.HardDiskResource,
        AtariMediaKind.Directory => AtariStorageSettingsConstants.DirectoryResource,
        AtariMediaKind.Cassette => AtariStorageSettingsConstants.CassetteResource,
        AtariMediaKind.Cartridge => AtariStorageSettingsConstants.CartridgeResource,
        AtariMediaKind.CompactDisc => AtariStorageSettingsConstants.CompactDiscResource,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    });
}
