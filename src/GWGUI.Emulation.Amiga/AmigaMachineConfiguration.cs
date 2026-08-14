namespace GWGUI.Emulation.Amiga;

public enum AmigaCoreKind { External }
public enum AmigaControllerType { Automatic, RetroPad, Cd32Pad, AnalogJoystick, Joystick, Keyboard, None }
public sealed record AmigaControllerBinding(int Port, AmigaControllerType Type, string? DeviceId = null,
    IReadOnlyDictionary<string, string>? ButtonMappings = null);
public sealed record AmigaInputConfiguration(IReadOnlyDictionary<string, GWGUI.Emulation.EmulationKey>? KeyboardMappings = null,
    string? MouseDeviceId = null, bool CaptureMouse = true, IReadOnlyList<AmigaControllerBinding>? ControllerBindings = null);

public sealed record AmigaMachineConfiguration(
    string Model,
    string KickstartPath,
    string? InitialDiskPath = null,
    string? ExtendedRomPath = null,
    string? RomKeyPath = null,
    AmigaCoreKind Core = AmigaCoreKind.External,
    IReadOnlyDictionary<string, string>? Options = null,
    Guid Id = default,
    bool AudioEnabled = true,
    IReadOnlyList<AmigaControllerType>? Controllers = null,
    AmigaInputConfiguration? Input = null)
{
    public static AmigaMachineConfiguration A500(string kickstartPath, string? diskPath = null) =>
        new("A500", kickstartPath, diskPath, Options: new Dictionary<string, string>
        {
            ["puae_model"] = "A500",
            ["puae_video_standard"] = "PAL",
            ["puae_floppy_multidrive"] = "disabled",
            ["puae_floppy_write_protection"] = "disabled"
        }, Id: Guid.NewGuid());

    public AmigaMachineConfiguration EnsureId() => Id == Guid.Empty ? this with { Id = Guid.NewGuid() } : this;
}
