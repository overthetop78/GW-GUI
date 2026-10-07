using GWGUI.Emulation.Atari.Emulators.HatariB.Constants;
using GWGUI.Emulation.Atari.Emulators.HatariB.Functions;

namespace GWGUI.Emulation.Atari.Emulators.HatariB.Factories;

internal sealed class HatariBMachineFactory() : MachineFactory(EmulatorConstants.Entry)
{
    public override IReadOnlyDictionary<string, string> GetConfiguredOptions(MachineConfiguration configuration) =>
        OptionFunctions.Apply(configuration);

    public override IReadOnlyDictionary<string, string> PrepareOptions(IReadOnlyDictionary<string, string> options) =>
        OptionFunctions.ToNative(options);

    public override void PrepareSystemDirectory(MachineConfiguration configuration, string systemDirectory) =>
        FirmwareRuntimeFunctions.PrepareSystemDirectory(configuration, systemDirectory, validateRequired:
            configuration.Firmwares.Any(item => item.Category == FirmwareCategory.Tos));

    public override EmulatorPreparedContent? PrepareContent(MachineConfiguration configuration,
        MediaConfiguration? media, string sessionDirectory, ExternalCoreInfo coreInfo)
    {
        var content = StContentFunctions.Prepare(configuration, sessionDirectory, coreInfo.Extensions);
        return content is null ? null : new EmulatorPreparedContent(content.Configuration,
            content.RuntimePath, coreInfo.NeedsFullPath, GetConfiguredOptions(configuration),
            content.SessionMedia, content.BootFloppy, State: content);
    }

    public override SessionMedia? PrepareInsertedMedia(MachineConfiguration configuration,
        MediaConfiguration media, string sessionDirectory, ExternalCoreInfo coreInfo) =>
        ScpMediaFunctions.Prepare(configuration, media, sessionDirectory, coreInfo.Extensions);

    public override bool SupportsDiskControl(MediaConfiguration media) => media.Category == MediaCategory.Floppy;
    public override bool SupportsDiskControlOperations => true;

    public override void CleanupPreparedContent(EmulatorPreparedContent? preparedContent) =>
        StContentFunctions.Cleanup(preparedContent?.State as StContent);
}
