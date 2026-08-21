namespace GWGUI.Emulation;

public sealed record EmulationMediaDevice(
    EmulationMediaSlot Slot,
    EmulationMediaType MediaType,
    IReadOnlyList<string> AcceptedExtensions,
    bool IsRemovable = true,
    bool RequiresMachineRecreation = false,
    string? DisplayLabel = null,
    FloppyDriveDialogOptions? FloppyOptions = null,
    IReadOnlyList<EmulationSettingsChoice>? InterfaceChoices = null,
    string? ImageDirectory = null);
