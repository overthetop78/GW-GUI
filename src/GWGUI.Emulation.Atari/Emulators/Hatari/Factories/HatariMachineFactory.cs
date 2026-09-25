using GWGUI.Emulation.Atari.Emulators.Hatari.Constants;
using GWGUI.Emulation.Atari.Emulators.Hatari.Contracts;
using GWGUI.Emulation.Atari.Emulators.Hatari.Functions;
using GWGUI.Emulation.Atari.Emulators.Hatari.Services;

namespace GWGUI.Emulation.Atari.Emulators.Hatari.Factories;

internal sealed class HatariMachineFactory() : MachineFactory(Emulator.Hatari)
{
    public override IReadOnlyDictionary<string, string> GetConfiguredOptions(MachineConfiguration configuration) =>
        MachineOptionFunctions.Apply(configuration);

    public override EmulatorPreparedContent? PrepareContent(MachineConfiguration configuration,
        MediaConfiguration? media, string sessionDirectory, ExternalCoreInfo coreInfo)
    {
        var content = HatariContentFunctions.Prepare(configuration, sessionDirectory, coreInfo.Extensions);
        return content is null ? null : new EmulatorPreparedContent(content.Configuration,
            content.RuntimePath, coreInfo.NeedsFullPath,
            HatariStorageFunctions.ApplyWriteProtection(configuration.Options, content.Storage),
            content.SessionMedia, content.BootFloppy, State: content);
    }

    public override SessionMedia? PrepareInsertedMedia(MachineConfiguration configuration,
        MediaConfiguration media, string sessionDirectory, ExternalCoreInfo coreInfo) =>
        ScpMediaFunctions.Prepare(configuration, media, sessionDirectory, coreInfo.Extensions);

    public override bool SupportsDiskControl(MediaConfiguration media) =>
        media.Category is MediaCategory.Floppy or MediaCategory.Cassette;

    public override bool SupportsDiskControlOperations => true;

    public override void CleanupPreparedContent(EmulatorPreparedContent? preparedContent) =>
        HatariContentFunctions.Cleanup(preparedContent?.State as HatariContent);
}
