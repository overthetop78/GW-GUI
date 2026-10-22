namespace GWGUI.Emulation.Services;

/// <summary>Relie le contrat public de traitement des entrées à la copie stable existante.</summary>
/// <remarks>Le service ne conserve aucune référence aux instantanés reçus.</remarks>
internal sealed class EmulationInputProcessingService : IEmulationInputProcessing
{
    public IReadOnlyDictionary<string, EmulationKey> ToKeys(IReadOnlyDictionary<string, string> values) => EmulationKeyboardBindingFunctions.ToKeys(values);
    public IReadOnlyDictionary<string, string> ToStrings(IReadOnlyDictionary<string, EmulationKey>? values, IEqualityComparer<string>? emptyComparer = null) => EmulationKeyboardBindingFunctions.ToStrings(values, emptyComparer);
    public bool IsValidSourcePrefix(string? source) => EmulationKeyboardBindingFunctions.IsValidSourcePrefix(source);

    /// <inheritdoc/>
    public ushort Modifiers(IReadOnlySet<EmulationKey> keys) => ExternalCoreKeyboardFunctions.Modifiers(keys);

    /// <inheritdoc/>
    public uint Character(uint code, IReadOnlySet<EmulationKey> keys) => ExternalCoreKeyboardFunctions.Character(code, keys);

    /// <inheritdoc/>
    public void AddRange(IDictionary<EmulationKey, uint> map, EmulationKey firstKey, uint firstCode, int count) => ExternalCoreKeyboardFunctions.AddRange(map, firstKey, firstCode, count);

    /// <inheritdoc/>
    public void PublishTransitions(IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<EmulationKey, uint> keyMap, ushort modifiers,
        ref IReadOnlySet<EmulationKey> previous, ref ExternalCoreApi.KeyboardEvent? keyboardEvent,
        Func<EmulationKey, bool> isModifier, Func<uint, IReadOnlySet<EmulationKey>, uint> characterFor)
        => ExternalCoreKeyboardFunctions.PublishTransitions(keys, keyMap, modifiers, ref previous, ref keyboardEvent, isModifier, characterFor);

    /// <inheritdoc/>
    public void PublishCodeTransitions<TSet>(TSet current, ref TSet previous, ExternalCoreApi.KeyboardEvent keyboardEvent, ushort modifiers,
        Func<IEnumerable<uint>, IEnumerable<uint>> orderReleased, Func<IEnumerable<uint>, IEnumerable<uint>> orderPressed, Func<uint, uint> characterFor) where TSet : IReadOnlySet<uint>
        => ExternalCoreKeyboardFunctions.PublishCodeTransitions(current, ref previous, keyboardEvent, modifiers, orderReleased, orderPressed, characterFor);

    /// <inheritdoc/>
    public int ControllerButtonIndex(string commandId) => EmulationInputMappingFunctions.ControllerButtonIndex(commandId);

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
    /// <inheritdoc/>
    public short NativeMouseState(EmulationPointerState pointer, uint port, uint index, uint id) => EmulationInputMappingFunctions.NativeMouseState(pointer, port, index, id);

    /// <inheritdoc/>
    public short NativeJoypadState(uint buttons, uint id, uint maximumButtonCount) => EmulationInputMappingFunctions.NativeJoypadState(buttons, id, maximumButtonCount);

    /// <inheritdoc/>
    public short NativeJoypadState(EmulationControllerState controller, uint id, uint maximumButtonCount) => EmulationInputMappingFunctions.NativeJoypadState(controller, id, maximumButtonCount);

    /// <inheritdoc/>
    public short NativeAnalogState(EmulationControllerState controller, uint index, uint id) => EmulationInputMappingFunctions.NativeAnalogState(controller, index, id);

    /// <inheritdoc/>
    public string? BindingDisplayValue(string resourceKey, string? invariant = null) => EmulationInputMappingFunctions.BindingDisplayValue(resourceKey, invariant);

    /// <inheritdoc/>
    public int SaturatingAdd(int left, int right) => EmulationInputMappingFunctions.SaturatingAdd(left, right);

    /// <inheritdoc/>
    public Dictionary<string, string> MouseBindings<TAction>(IReadOnlyDictionary<string, TAction>? mappings, TAction none) where TAction : struct, Enum => EmulationInputMappingFunctions.MouseBindings<TAction>(mappings, none);

    /// <inheritdoc/>
    public Dictionary<string, TAction>? CreateMouseMappings<TAction>(EmulationInputBindingSet? bindings, TAction none) where TAction : struct, Enum => EmulationInputMappingFunctions.CreateMouseMappings<TAction>(bindings, none);

    /// <inheritdoc/>
    public Dictionary<string, string> CopyNonEmptyBindings(IReadOnlyDictionary<string, string> values) => EmulationInputMappingFunctions.CopyNonEmptyBindings(values);

    /// <inheritdoc/>
    public bool HasInvalidBindings(IReadOnlyDictionary<string, string>? bindings, IReadOnlySet<string> commandIds, Func<string, bool> isValidSource) => EmulationInputMappingFunctions.HasInvalidBindings(bindings, commandIds, isValidSource);

    /// <inheritdoc/>
    public bool HasInvalidControllerPortNumbers(EmulationInputSettings settings, int count) => EmulationInputMappingFunctions.HasInvalidControllerPortNumbers(settings, count);

    /// <inheritdoc/>
    public Dictionary<string, string> ResolveKeyboardBindings(IReadOnlyList<InputBindingDefinition> definitions, IReadOnlyDictionary<string, string>? input) => EmulationInputMappingFunctions.ResolveKeyboardBindings(definitions, input);

    /// <inheritdoc/>
    public Dictionary<string, string> ResolveBindings(IReadOnlyList<InputBindingDefinition> definitions, IReadOnlyDictionary<string, string>? values) => EmulationInputMappingFunctions.ResolveBindings(definitions, values);

    /// <inheritdoc/>
    public TBinding[] CreateControllerBindings<TPeripheral, TBinding>(EmulationInputSettings settings, Func<int, TPeripheral, bool> supportsPeripheral, Func<EmulationControllerPort, TPeripheral, IReadOnlyDictionary<string, string>, TBinding> createBinding) where TPeripheral : struct, Enum => EmulationInputMappingFunctions.CreateControllerBindings<TPeripheral, TBinding>(settings, supportsPeripheral, createBinding);

    /// <inheritdoc/>
    public IReadOnlySet<EmulationKey> MapKeyboard(IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, EmulationKey>? mappings) => EmulationInputMappingFunctions.MapKeyboard(keys, mappings);

    /// <inheritdoc/>
    public string? ParseControllerDeviceId(string? source) => EmulationInputMappingFunctions.ParseControllerDeviceId(source);

    /// <inheritdoc/>
    public EmulationControllerState ResolveSourceController(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback, bool fallbackWhenMissing) => EmulationInputMappingFunctions.ResolveSourceController(source, controllers, fallback, fallbackWhenMissing);

    /// <inheritdoc/>
    public bool TryRemovePrefix(string? value, string prefix, out string source) => EmulationInputMappingFunctions.TryRemovePrefix(value, prefix, out source);

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, bool> PhysicalMouse(EmulationPointerState pointer) => EmulationInputMappingFunctions.PhysicalMouse(pointer);

    /// <inheritdoc/>
    public float ControllerSourceValue(string source, EmulationControllerState controller) => EmulationInputMappingFunctions.ControllerSourceValue(source, controller);

    /// <inheritdoc/>
    public bool IsControllerSourcePressed(string source, EmulationControllerState controller) => EmulationInputMappingFunctions.IsControllerSourcePressed(source, controller);

    /// <inheritdoc/>
    public bool IsControllerSourcePressed(string source, EmulationControllerState controller, float value) => EmulationInputMappingFunctions.IsControllerSourcePressed(source, controller, value);

    /// <inheritdoc/>
    public short MapControllerAxis(IReadOnlyDictionary<string, string> mappings, string positiveCommand, string negativeCommand, EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse) => EmulationInputMappingFunctions.MapControllerAxis(mappings, positiveCommand, negativeCommand, controller, physical, keys, mouse);

    /// <inheritdoc/>
    public short MapControllerPressure(IReadOnlyDictionary<string, string> mappings, string command, EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse) => EmulationInputMappingFunctions.MapControllerPressure(mappings, command, controller, physical, keys, mouse);

    /// <inheritdoc/>
    public float InputSourcePressure(string source, EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse) => EmulationInputMappingFunctions.InputSourcePressure(source, controller, physical, keys, mouse);

    /// <inheritdoc/>
    public float ControllerSourcePressure(string source, EmulationControllerState controller) => EmulationInputMappingFunctions.ControllerSourcePressure(source, controller);

    /// <inheritdoc/>
    public string? OppositeControllerAxisSource(string source) => EmulationInputMappingFunctions.OppositeControllerAxisSource(source);

    /// <inheritdoc/>
    public bool IsMappedInputPressed(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot) => EmulationInputMappingFunctions.IsMappedInputPressed(mapping, controller, snapshot);

    /// <inheritdoc/>
    public short NormalizedAxis(float value) => EmulationInputMappingFunctions.NormalizedAxis(value);
}
