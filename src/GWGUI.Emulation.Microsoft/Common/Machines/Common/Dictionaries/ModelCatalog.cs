namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<Model> All { get; } =
    [
        new(ModelConstants.Xbox, "Xbox", ModelConstants.BackendXbox,
            ModelConstants.RamXboxKib, false, 0, 0, false, false, false, false,
            ControllerPortCount: 4, HasBuiltInCompactDiscDrive: true,
            SupportsCompactDiscDrive: true, CpuModels: [ModelConstants.CpuPentiumIii],
            VideoChip: ModelConstants.VideoNv2A, AudioChip: ModelConstants.AudioMcpx,
            CpuFrequency: ModelConstants.FrequencyXbox),
        new(ModelConstants.Xbox360, "Xbox 360", ModelConstants.BackendXbox360,
            ModelConstants.RamXbox360Kib, false, 0, 0, false, false, false, false,
            ControllerPortCount: 4, HasBuiltInCompactDiscDrive: true,
            SupportsCompactDiscDrive: false, CpuModels: [ModelConstants.CpuXenon],
            VideoChip: ModelConstants.VideoXenos, AudioChip: ModelConstants.AudioXma,
            CpuFrequency: ModelConstants.FrequencyXbox360)
    ];

    public static Model Get(string id) => All.FirstOrDefault(model =>
            model.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);

    public static string BackendModelFor(string id) => Get(id).BackendModel;
}
