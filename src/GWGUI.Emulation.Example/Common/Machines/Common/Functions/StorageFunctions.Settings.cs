using System.IO;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Décrit et applique les réglages de stockage d’une configuration.</summary>
/// <remarks>Ces opérations travaillent sur des descriptions ; elles n’ouvrent ni ne modifient les fichiers des médias.</remarks>
internal static class StorageSettingsFunctions
{
    /// <summary>Expose les périphériques, médias et réglages déclarés par l’adaptateur sélectionné.</summary>
    /// <param name="configuration">Configuration dont les médias sont convertis vers le contrat public.</param>
    /// <returns>Description considérant les emplacements disponibles comme configurés ; sans adaptateur compatible, les listes de périphériques et de réglages sont vides.</returns>
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        IReadOnlyList<EmulationMediaDevice> devices = configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorStorageSettings storage ? storage.DescribeMediaDevices(configuration) : [];
        var deviceSettings = configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorStorageSettings adapter ? adapter.DescribeDeviceSettings(configuration) : [];
        return new EmulationStorageSettings(devices, devices.Select(device => device.Slot).ToArray(), configuration.Media.Select(EmulationMediaConversionFunctions.ToCommon).ToArray(), deviceSettings);
    }

    /// <summary>Valide les choix de stockage puis produit la configuration mise à jour.</summary>
    /// <param name="configuration">Configuration d’origine, utilisée aussi pour conserver les libellés des médias existants.</param>
    /// <param name="settings">Emplacements configurés, médias montés et réglages de périphériques à appliquer.</param>
    /// <returns>Configuration validée après application des réglages par l’adaptateur et remplacement de la liste des médias.</returns>
    /// <exception cref="ArgumentException">Un emplacement est indisponible, un média incompatible, ou un réglage duplique un emplacement ou vise un emplacement non configuré.</exception>
    /// <remarks>Les extensions sont comparées sans distinction de casse. La validation finale applique également les contraintes générales de la machine.</remarks>
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        var devices = Describe(configuration).AvailableDevices;
        var available = devices.Select(device => device.Slot).ToHashSet();
        if (settings.ConfiguredSlots.Any(slot => !available.Contains(slot))
           || settings.MountedMedia.Any(media => !settings.ConfiguredSlots.Contains(media.Slot)
               || !devices.Any(device => device.Slot == media.Slot && device.MediaType == media.Type
                    && device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase))))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        // Un périphérique ne reçoit qu’un réglage, et uniquement si son emplacement est configuré.
        var configured = settings.DeviceSettings ?? [];
        if (configured.Select(item => item.Slot).Distinct().Count() != configured.Count || configured.Any(item => !settings.ConfiguredSlots.Contains(item.Slot)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(settings));
        var result = configuration.Core != Emulator.None && new Engine().Adapter(configuration) is IEmulatorStorageSettings adapter
            ? adapter.ApplyDeviceSettings(configuration, configured) : configuration;
        result = result with
        {
            Media = settings.MountedMedia.Select(media => EmulationMediaConversionFunctions.ToAcorn(media, configuration.Media)).ToArray()
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
