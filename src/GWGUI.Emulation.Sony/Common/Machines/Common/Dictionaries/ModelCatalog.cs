namespace GWGUI.Emulation.Sony.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<Model> All { get; } =
    [
        Machines.PocketStation.Dictionaries.ModelCatalog.PocketStation,
        new(ModelConstants.PlayStation, "PlayStation / PS one", ModelConstants.BackendPlayStation,
            (int)RamCapacity._2MB, false, 0, 0, false, false, false, false, HasBuiltInCompactDiscDrive: true,
            SupportsCompactDiscDrive: true, CpuModels: [ModelConstants.CpuR3000A],
            VideoChip: ModelConstants.VideoGpuPs1, AudioChip: ModelConstants.AudioSpu,
            CpuFrequency: ModelConstants.FrequencyPs1),
        new(ModelConstants.PlayStation2, "PlayStation 2 / PStwo", ModelConstants.BackendPlayStation2,
            (int)RamCapacity._32MB, false, 0, 0, false, false, false, false, HasBuiltInCompactDiscDrive: true,
            SupportsCompactDiscDrive: true, CpuModels: [ModelConstants.CpuR5900],
            VideoChip: ModelConstants.VideoGraphicsSynthesizer, AudioChip: ModelConstants.AudioSpu2,
            CpuFrequency: ModelConstants.FrequencyPs2),
        new(ModelConstants.Psp, "PlayStation Portable", ModelConstants.BackendPsp,
            (int)RamCapacity._32MB, false, 0, 0, false, false, false, false, ControllerPortCount: 1,
            HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,
            CpuModels: [ModelConstants.CpuAllegrex], VideoChip: ModelConstants.VideoPspGpu,
            AudioChip: ModelConstants.AudioPsp, CpuFrequency: ModelConstants.FrequencyPsp),
        new(ModelConstants.PsVita, "PlayStation Vita", ModelConstants.BackendPsVita,
            (int)RamCapacity._512MB, false, 0, 0, false, false, true, true, ControllerPortCount: 1,
            CpuModels: [ModelConstants.CpuCortexA9], VideoChip: ModelConstants.VideoSgX543,
            AudioChip: ModelConstants.AudioVita, CpuFrequency: ModelConstants.FrequencyVita),
        new(ModelConstants.PlayStation3, "PlayStation 3", ModelConstants.BackendPlayStation3,
            (int)RamCapacity._256MB, false, 0, 0, false, false, false, false,
            HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,
            CpuModels: [ModelConstants.CpuCellBroadbandEngine], VideoChip: ModelConstants.VideoRsx,
            AudioChip: ModelConstants.AudioCell, CpuFrequency: ModelConstants.FrequencyPs3),
        new(ModelConstants.PlayStation4, "PlayStation 4", ModelConstants.BackendPlayStation4,
            (int)RamCapacity._8GB, false, 0, 0, false, false, false, false,
            HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,
            CpuModels: [ModelConstants.CpuJaguar], VideoChip: ModelConstants.VideoGcn,
            AudioChip: ModelConstants.AudioAmd, CpuFrequency: ModelConstants.FrequencyPs4),
        new(ModelConstants.PlayStation5, "PlayStation 5", ModelConstants.BackendPlayStation5,
            (int)RamCapacity._16GB, false, 0, 0, false, false, false, false,
            HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,
            CpuModels: [ModelConstants.CpuZen2], VideoChip: ModelConstants.VideoRdna2,
            AudioChip: ModelConstants.AudioTempest, CpuFrequency: ModelConstants.FrequencyPs5)
    ];

    public static Model Get(string id) => All.FirstOrDefault(model =>
            model.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);

    public static string BackendModelFor(string id) => Get(id).BackendModel;
}
