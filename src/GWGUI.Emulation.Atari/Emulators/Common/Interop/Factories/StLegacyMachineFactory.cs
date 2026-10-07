namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Factories;

internal abstract class StLegacyMachineFactory(EmulatorCatalogEntry entry) : MachineFactory(entry)
{
    public override IReadOnlyDictionary<string, string> GetConfiguredOptions(MachineConfiguration configuration) =>
        StLegacyOptionFunctions.Apply(configuration);

    public override IReadOnlyDictionary<string, string> PrepareOptions(
        IReadOnlyDictionary<string, string> options) => StLegacyOptionFunctions.ToNativeOptions(options);

    public override EmulatorPreparedContent? PrepareContent(MachineConfiguration configuration,
        MediaConfiguration? media, string sessionDirectory, ExternalCoreInfo coreInfo)
    {
        var content = StContentFunctions.Prepare(configuration, sessionDirectory, coreInfo.Extensions);
        return content is null ? null : new EmulatorPreparedContent(content.Configuration,
            content.RuntimePath, coreInfo.NeedsFullPath,
            StLegacyOptionFunctions.ApplyWriteProtection(configuration.Options, content.Storage),
            content.SessionMedia, content.BootFloppy, State: content);
    }

    public override SessionMedia? PrepareInsertedMedia(MachineConfiguration configuration,
        MediaConfiguration media, string sessionDirectory, ExternalCoreInfo coreInfo) =>
        ScpMediaFunctions.Prepare(configuration, media, sessionDirectory, coreInfo.Extensions);

    public override bool SupportsDiskControl(MediaConfiguration media) =>
        media.Category is MediaCategory.Floppy or MediaCategory.Cassette;

    public override bool SupportsDiskControlOperations => true;

    public override void CleanupPreparedContent(EmulatorPreparedContent? preparedContent) =>
        StContentFunctions.Cleanup(preparedContent?.State as StContent);
}
