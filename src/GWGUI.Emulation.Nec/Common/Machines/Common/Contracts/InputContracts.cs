using System.Text.Json.Serialization;

namespace GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;

/// <summary>Associations persistantes des entrées du module, décrites sous les mêmes noms que dans Example.</summary>
/// <remarks>Les données restent empruntées par les projections. Les noms JSON existants sont conservés par les attributs du contrat.</remarks>
public sealed record InputConfiguration(
    IReadOnlyDictionary<string, EmulationKey>? KeyboardMappings = null,
    string? MouseDeviceId = null,
    bool CaptureMouse = true,
    [property: JsonPropertyName("controllerBindings")] IReadOnlyList<ControllerBinding>? Controllers = null,
    IReadOnlyDictionary<string, MouseAction>? MouseButtonMappings = null,
    EmulationKey ReleaseMouseKey = EmulationKey.Escape,
    IReadOnlyDictionary<string, string>? KeyboardBindings = null,
    string? ReleaseMouseBinding = null,
    bool ParallelJoystickAdapterEnabled = false);

