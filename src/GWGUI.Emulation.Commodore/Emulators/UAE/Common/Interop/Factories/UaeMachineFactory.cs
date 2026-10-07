
namespace GWGUI.Emulation.Commodore.Emulators.UAE.Common.Interop.Factories;

internal abstract class UaeMachineFactory : AmigaMachineFactory
{
    internal override MachineConfiguration PrepareConfiguration(MachineConfiguration configuration) =>
        UaeOptionFunctions.ToNative(configuration);

    internal override IReadOnlyDictionary<string, string> NativeOptions(MachineConfiguration configuration,
        IReadOnlyList<MediaConfiguration> media)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(),
            StringComparer.Ordinal)
        {
            [UaeOptionConstants.Model] = ((AmigaModel)ModelCatalog.Get(configuration.Model)).BackendModel,
            [UaeOptionConstants.Kickstart] = UaeFirmwareConstants.Auto,
            [UaeOptionConstants.MouseToggle] = UaeFirmwareConstants.RightControl
        };
        var floppies = media.Where(item => item.Category == MediaCategory.Floppy).ToArray();
        if (floppies.Length > UaeContentConstants.SingleContentCount)
            options[UaeOptionConstants.FloppyMultidrive] = configuration.MountFloppiesInSeparateDrives
                ? SettingsValueConstants.Enabled : SettingsValueConstants.Disabled;
        if (floppies.Length > BufferConstants.EmptyCollectionCount && floppies.All(item => item.IsReadOnly))
            options[UaeOptionConstants.FloppyWriteProtection] = SettingsValueConstants.Enabled;
        return options;
    }
}
