namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<Model> All { get; } =
    [
        new(ModelConstants.GameWatch, "Game & Watch", ModelConstants.BackendGameWatch, 1, false, 0, 0, false, false, false, false, 0, 0,
            CpuModels: [ModelConstants.CpuSharpSm5], VideoChip: ModelConstants.VideoSharpSm5, AudioChip: ModelConstants.AudioSharpSm5),
        new(ModelConstants.Nes, "Nintendo Entertainment System / Famicom", ModelConstants.BackendNes, 2, false, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuRicoh2A03], VideoChip: ModelConstants.VideoRicoh2C02, AudioChip: ModelConstants.AudioRicoh2A03),
        new(ModelConstants.FamicomDisk, "Famicom Disk System", ModelConstants.BackendFamicomDisk, 32, false, 1, 1, false, false, false, false,
            CpuModels: [ModelConstants.CpuRicoh2A03], VideoChip: ModelConstants.VideoRicoh2C02, AudioChip: ModelConstants.AudioNintendo2C33),
        new(ModelConstants.Snes, "Super Nintendo / Super Famicom", ModelConstants.BackendSnes, 128, false, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuRicoh5A22], VideoChip: ModelConstants.VideoNintendoSppu, AudioChip: ModelConstants.AudioSonySpc700),
        new(ModelConstants.VirtualBoy, "Virtual Boy", ModelConstants.BackendVirtualBoy, 1024, false, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuNecV810], VideoChip: ModelConstants.VideoNecV810, AudioChip: ModelConstants.AudioNecV810),
        new(ModelConstants.Nintendo64, "Nintendo 64", ModelConstants.BackendNintendo64, 4096, false, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuNecVr4300], VideoChip: ModelConstants.VideoRealityCoprocessor, AudioChip: ModelConstants.AudioDsp),
        new(ModelConstants.GameBoy, "Game Boy", ModelConstants.BackendGameBoy, 8, false, 0, 0, false, false, true, true,
            ControllerPortCount: ModelConstants.IntegratedControllerPortCount,
            MouseButtonCount: ModelConstants.NoMouseButtonCount,
            CpuModels: [ModelConstants.CpuSharpLr35902], VideoChip: ModelConstants.VideoSharpLr35902, AudioChip: ModelConstants.AudioSharpLr35902),
        new(ModelConstants.GameBoyColor, "Game Boy Color", ModelConstants.BackendGameBoyColor, 32, false, 0, 0, false, false, true, true,
            ControllerPortCount: ModelConstants.IntegratedControllerPortCount,
            MouseButtonCount: ModelConstants.NoMouseButtonCount,
            CpuModels: [ModelConstants.CpuSharpLr35902], VideoChip: ModelConstants.VideoSharpLr35902, AudioChip: ModelConstants.AudioSharpLr35902),
        new(ModelConstants.GameBoyAdvance, "Game Boy Advance", ModelConstants.BackendGameBoyAdvance, 256, false, 0, 0, false, false, true, true,
            ControllerPortCount: ModelConstants.IntegratedControllerPortCount,
            MouseButtonCount: ModelConstants.NoMouseButtonCount,
            CpuModels: [ModelConstants.CpuArm7Tdmi], VideoChip: ModelConstants.VideoArm7Tdmi, AudioChip: ModelConstants.AudioArm7Tdmi),
        new(ModelConstants.NintendoDs, "Nintendo DS / DSi", ModelConstants.BackendNintendoDs, 4096, false, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuArm946Es], VideoChip: ModelConstants.VideoArm946Es, AudioChip: ModelConstants.AudioArm946Es),
        new(ModelConstants.Nintendo3Ds, "Nintendo 3DS", ModelConstants.BackendNintendo3Ds, 128 * 1024, false, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuArm11], VideoChip: ModelConstants.VideoArm11, AudioChip: ModelConstants.AudioArm11),
        new(ModelConstants.GameCube, "Nintendo GameCube", ModelConstants.BackendGameCube, 24 * 1024, false, 0, 0, false, false, false, false,
            HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,
            CpuModels: [ModelConstants.CpuIbmGekko], VideoChip: ModelConstants.VideoFlipper, AudioChip: ModelConstants.AudioDsp),
        new(ModelConstants.Wii, "Nintendo Wii", ModelConstants.BackendWii, 88 * 1024, false, 0, 0, false, false, false, false,
            HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,
            CpuModels: [ModelConstants.CpuIbmBroadway], VideoChip: ModelConstants.VideoHollywood, AudioChip: ModelConstants.AudioHollywood),
        new(ModelConstants.WiiU, "Nintendo Wii U", ModelConstants.BackendWiiU, 2048 * 1024, false, 0, 0, false, false, false, false,
            HasBuiltInCompactDiscDrive: true, SupportsCompactDiscDrive: true,
            CpuModels: [ModelConstants.CpuIbmEspresso], VideoChip: ModelConstants.VideoLatte, AudioChip: ModelConstants.AudioLatte),
        new(ModelConstants.Switch, "Nintendo Switch", ModelConstants.BackendSwitch, 4096 * 1024, false, 0, 0, false, false, true, true,
            CpuModels: [ModelConstants.CpuArmCortexA57], VideoChip: ModelConstants.VideoNvidiaMaxwell, AudioChip: ModelConstants.AudioNvidia)
    ];

    public static Model Get(string id) => All.FirstOrDefault(model =>
            model.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);

    public static string BackendModelFor(string id) => Get(id).BackendModel;
}
