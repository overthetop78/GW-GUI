namespace GWGUI.Emulation.Services;

/// <summary>Relie le contrat public de traitement des entrées à la copie stable existante.</summary>
/// <remarks>Le service ne conserve aucune référence aux instantanés reçus.</remarks>
internal sealed class EmulationInputProcessingService : IEmulationInputProcessing
{
    /// <summary>Index de codes natifs construit à partir de la table clavier existante, sans conserver de touches appuyées.</summary>
    private readonly IReadOnlyDictionary<uint, EmulationKey[]> _nativeKeys = ExternalCoreKeyboardFunctions.CreateKeyMap().GroupBy(item => item.Value).ToDictionary(group => group.Key, group => group.Select(item => item.Key).ToArray());

    /// <inheritdoc/>
    public IReadOnlyDictionary<EmulationKey, uint> CreateKeyMap() => ExternalCoreKeyboardFunctions.CreateKeyMap();

    /// <inheritdoc/>
    public short ReadNativeInput(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id)
    {
        // L’octet bas identifie la classe du périphérique ; les bits hauts portent ses variantes natives.
        switch (device & 0xffu)
        {
            case 2: return EmulationInputMappingFunctions.NativeMouseState(snapshot.Pointer, port, index, id);
            case 3: return _nativeKeys.TryGetValue(id, out var keys) && keys.Any(snapshot.Keys.Contains) ? (short)1 : (short)0;
        }
        if (port >= snapshot.Controllers.Count) return 0;
        var controller = snapshot.Controllers[(int)port];
        return (device & 0xffu) switch
        {
            1 => EmulationInputMappingFunctions.NativeJoypadState(controller, id, sizeof(uint) * 8),
            5 => EmulationInputMappingFunctions.NativeAnalogState(controller, index, id),
            _ => 0
        };
    }

    /// <inheritdoc/>
    public EmulationInputSnapshot Freeze(EmulationInputSnapshot? snapshot) => EmulationInputMappingFunctions.Freeze(snapshot);

    /// <inheritdoc/>
    public IReadOnlySet<EmulationKey> MapKeyboard(EmulationInputSnapshot snapshot, IReadOnlyDictionary<string, string>? mappings) => EmulationInputMappingFunctions.MapKeyboard(snapshot, mappings);

    /// <inheritdoc/>
    public EmulationControllerState ResolveController(string? deviceId, IReadOnlyList<EmulationControllerState> controllers, int fallbackIndex) => EmulationInputMappingFunctions.ResolveController(deviceId, controllers, fallbackIndex);

    /// <inheritdoc/>
    public float InputSourcePressure(string source, EmulationControllerState controller, EmulationInputSnapshot snapshot) => EmulationInputMappingFunctions.InputSourcePressure(source, controller, snapshot);

    /// <inheritdoc/>
    public EmulationControllerState ApplyDeadZones(EmulationControllerState controller, int percent) => EmulationInputMappingFunctions.ApplyDeadZones(controller, percent);

    /// <inheritdoc/>
    public bool IsValidBinding(string binding) => EmulationKeyboardBindingFunctions.IsValid(binding);

    /// <inheritdoc/>
    public EmulationControllerState ResolveSourceController(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback) => EmulationInputMappingFunctions.ResolveSourceController(source, controllers, fallback);

    /// <inheritdoc/>
    public float MappedInputMagnitude(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot, int deadZone) => EmulationInputMappingFunctions.MappedInputMagnitude(mapping, controller, snapshot, deadZone);
}
