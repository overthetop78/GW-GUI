namespace GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<Model> All { get; } =
    [
        new("Sg1000", "SG-1000", ModelConstants.BackendSg1000, 1, false, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuZ80A], VideoChip: ModelConstants.VideoTms9918A,
            AudioChip: ModelConstants.AudioSn76489),
        new("Sc3000", "SC-3000", "sc3000", 2, true, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuZ80A], VideoChip: ModelConstants.VideoTms9918A,
            AudioChip: ModelConstants.AudioSn76489),
        new("MarkIII", "Mark III", "mark3", 8, false, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuZ80A], VideoChip: ModelConstants.VideoSega3155124,
            AudioChip: ModelConstants.AudioSn76489, HasBuiltInSegaCardSlot: true,
            SupportsSegaCardSlot: true),
        new("MasterSystem", "Master System", ModelConstants.BackendMasterSystem, 8, false, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuZ80A], VideoChip: ModelConstants.VideoSega3155246,
            AudioChip: ModelConstants.AudioSn76489, HasBuiltInSegaCardSlot: true,
            SupportsSegaCardSlot: true, SupportsThreeDGlasses: true),
        new("MegaDrive", "Mega Drive / Genesis", ModelConstants.BackendMegaDrive, 64, false, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuMotorola68000, ModelConstants.CpuZ80A],
            VideoChip: ModelConstants.VideoSega3155313, AudioChip: ModelConstants.AudioYm2612),
        new("Pico", "Pico", ModelConstants.BackendPico, 64, false, 0, 0, false, false, true, true,
            ControllerPortCount: 0, CpuModels: [ModelConstants.CpuMotorola68000],
            VideoChip: ModelConstants.VideoSega3155313, AudioChip: ModelConstants.AudioYm2612),
        new("GameGear", "Game Gear", "gamegear", 8, false, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuZ80A], VideoChip: ModelConstants.VideoSega3155246,
            AudioChip: ModelConstants.AudioSn76489),
        new("Saturn", "Saturn", "saturn", 2048, false, 0, 0, false, false, true, true,
            HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,
            CpuModels: [ModelConstants.CpuHitachiSh2], VideoChip: ModelConstants.VideoSaturnVdp,
            AudioChip: ModelConstants.AudioSaturnScsp),
        new("Dreamcast", "Dreamcast", "dreamcast", 16384, false, 0, 0, false, false, true, true,
            HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,
            CpuModels: [ModelConstants.CpuHitachiSh4], VideoChip: ModelConstants.VideoPowerVr2,
            AudioChip: ModelConstants.AudioDreamcastAica),
        new(ModelConstants.Naomi, "NAOMI", ModelConstants.BackendNaomi, ModelConstants.Ram32768Kib,
            false, 0, 0, false, false, true, true, SupportsCompactDiscDrive: true,
            CpuModels: [ModelConstants.CpuHitachiSh4], VideoChip: ModelConstants.VideoPowerVr2,
            AudioChip: ModelConstants.AudioDreamcastAica),
        new(ModelConstants.Naomi2, "NAOMI 2", ModelConstants.BackendNaomi2, ModelConstants.Ram32768Kib,
            false, 0, 0, false, false, true, true, SupportsCompactDiscDrive: true,
            CpuModels: [ModelConstants.CpuHitachiSh4], VideoChip: ModelConstants.VideoPowerVr2DualWithElan,
            AudioChip: ModelConstants.AudioDreamcastAica)
    ];

    public static Model Get(string id) => All.FirstOrDefault(model =>
            model.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);

    public static string BackendModelFor(string id) => Get(id).BackendModel;
}
