using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using System.IO;

namespace GWGUI.App.Controls;

internal static class AtariStorageSettingsFunctions
{
    internal static AtariStorageView Create(AtariMachineConfiguration configuration)
    {
        var allAvailable = AtariCompatibilityCatalog.Get(configuration.Model).Media
            .Where(rule => rule.Availability == AtariMediaAvailability.Available)
            .ToArray();
        var primary = PrimaryDevice(configuration.Model);
        var devicesAvailable = allAvailable.Where(rule => rule.Kind == primary.Kind && rule.Slots.Contains(primary.Slot))
            .Select(rule => rule with { Slots = [primary.Slot] }).ToArray();
        var types = allAvailable.Select(rule => rule.Kind).Distinct()
            .Select(kind => new AtariStorageTypeChoice(kind, KindName(kind))).ToArray();
        var slots = allAvailable.GroupBy(rule => rule.Kind).ToDictionary(group => group.Key,
            group => (IReadOnlyList<AtariStorageSlotChoice>)group.SelectMany(rule => rule.Slots).Distinct()
                .Select(slot => new AtariStorageSlotChoice(slot, slot.ToString())).ToArray());
        var buses = types.ToDictionary(type => type.Kind,
            type => (IReadOnlyList<AtariStorageBusChoice>)Buses(configuration.Model, type.Kind)
                .Select(bus => new AtariStorageBusChoice(bus, bus.ToString())).ToArray());
        var devices = devicesAvailable.SelectMany(rule => rule.Slots.Select(slot => (rule.Kind, Slot: slot)))
            .Distinct()
            .Select(device =>
            {
                var media = configuration.Media.FirstOrDefault(item => item.Slot == device.Slot)
                    ?? new AtariMediaConfiguration(string.Empty, device.Kind, device.Slot);
                return new AtariStorageDeviceItem(media, KindName(device.Kind));
            }).ToArray();
        return new AtariStorageView(types, slots, buses, devices);
    }

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
        EmulationMediaSlot slot) => ReplaceMedia(source, source.Media.Where(item => item.Slot != slot).ToArray());

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
        IReadOnlyList<AtariMediaConfiguration> media) => new(source.Model, source.Firmwares, media,
        source.Options, source.Input, source.Id, source.SchemaVersion, source.AudioEnabled,
        source.VideoRenderer, source.Folders);

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
