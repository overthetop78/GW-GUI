namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorMediaAdapter
{
    IReadOnlyList<string> SupportedContentExtensions { get; }
    int GetMemoryCardCount(MachineConfiguration configuration) => configuration.Model switch
    {
        ModelConstants.Psp => 1,
        ModelConstants.PlayStation or ModelConstants.PlayStation2 => StorageSettingsFunctionsConstants.MemoryCardCount,
        _ => 0
    };
    bool HasFixedMemoryCardDirectories => false;
    string? GetMemoryCardDirectory(int index) => null;
}
