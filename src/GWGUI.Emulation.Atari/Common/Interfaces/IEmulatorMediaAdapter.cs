namespace GWGUI.Emulation.Atari.Common.Interfaces;

internal interface IEmulatorMediaAdapter
{
    MediaConfiguration? SelectPrimaryMedia(MachineConfiguration configuration);
    IReadOnlyDictionary<string, string> GetConfiguredOptions(MachineConfiguration configuration);
    EmulatorPreparedContent? PrepareContent(MachineConfiguration configuration,
        MediaConfiguration? media, string sessionDirectory, ExternalCoreInfo coreInfo);
    SessionMedia? PrepareInsertedMedia(MachineConfiguration configuration,
        MediaConfiguration media, string sessionDirectory, ExternalCoreInfo coreInfo);
    void ValidatePreparedContent(EmulatorPreparedContent? preparedContent, bool diskControlAvailable);
    void ValidateInsertion(MachineConfiguration configuration, MediaConfiguration media);
    bool SupportsDiskControlOperations { get; }
    bool SupportsDiskControl(MediaConfiguration media);
    bool SupportsEjection(EmulationMediaSlot slot);
    Exception ContentLoadException(string message);
    void CleanupPreparedContent(EmulatorPreparedContent? preparedContent);
}
