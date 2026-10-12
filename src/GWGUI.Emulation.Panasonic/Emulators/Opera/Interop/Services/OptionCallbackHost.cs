using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Services;

internal sealed class OptionCallbackHost : IDisposable
{
    private const int MaximumVariableCount = 4096;
    private const char DescriptionSeparator = ';';
    private const char ValueSeparator = '|';
    private readonly Dictionary<string, string> _values;
    private readonly Dictionary<string, CoreOption> _options = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ExternalCoreUtf8String> _strings = new(StringComparer.Ordinal);
    private bool _updated;
    internal OptionCallbackHost(IReadOnlyDictionary<string, string> values) => _values = new(values, StringComparer.Ordinal);
    internal IReadOnlyList<CoreOption> Options => _options.Values.ToArray();

    internal bool Register(nint data)
    {
        if (data == nint.Zero) return false;
        var size = Marshal.SizeOf<ExternalCoreApi.Variable>();
        for (var index = 0; index < MaximumVariableCount; index++)
        {
            var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data + checked(index * size));
            if (variable.Key == nint.Zero) return true;
            var key = Marshal.PtrToStringUTF8(variable.Key) ?? throw new InvalidOperationException("Native option key is invalid.");
            var description = Marshal.PtrToStringUTF8(variable.Value) ?? throw new InvalidOperationException("Native option descriptor is invalid.");
            var separator = description.IndexOf(DescriptionSeparator);
            if (separator < 0) throw new InvalidOperationException("Native option descriptor has no choices.");
            var values = description[(separator + 1)..].Trim().Split(ValueSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length == 0) throw new InvalidOperationException("Native option descriptor has no default value.");
            var current = _values.GetValueOrDefault(key, values[0]);
            if (!values.Contains(current, StringComparer.Ordinal)) throw new EmulationMessageException(new EmulationMessage(EmulationMessageCategory.Emulator, EmulationMessageCode.OptionInvalid, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog));
            _values[key] = current;
            _options[key] = new CoreOption(key, description[..separator].Trim(), null, null, values[0], current, values.Select(value => new CoreOptionValue(value, value)).ToArray(), IsVisible: key is not (GWGUI.Emulation.Panasonic.Emulators.Opera.Constants.RuntimeOptionConstants.Bios or GWGUI.Emulation.Panasonic.Emulators.Opera.Constants.RuntimeOptionConstants.Font));
        }
        throw new InvalidOperationException("Native option table has no terminating entry.");
    }

    internal bool ReturnValue(nint data)
    {
        if (data == nint.Zero) return false;
        var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data);
        var key = Marshal.PtrToStringUTF8(variable.Key);
        if (key is null || !_values.TryGetValue(key, out var value)) return false;
        if (!_strings.TryGetValue(key, out var text)) { text = new ExternalCoreUtf8String(value); _strings.Add(key, text); }
        variable.Value = text.Pointer;
        Marshal.StructureToPtr(variable, data, false);
        return true;
    }

    internal void SetValue(string key, string value)
    {
        if (!_options.TryGetValue(key, out var option) || !option.Values.Any(choice => choice.Value == value)) throw new ArgumentException(nameof(value));
        if (_values[key] == value) return;
        _values[key] = value;
        _options[key] = option with { CurrentValue = value };
        if (_strings.Remove(key, out var previous)) previous.Dispose();
        _updated = true;
    }

    internal bool ReturnUpdated(nint data)
    {
        if (data == nint.Zero) return false;
        Marshal.WriteByte(data, _updated ? (byte)1 : (byte)0);
        _updated = false;
        return true;
    }

    public void Dispose()
    {
        foreach (var value in _strings.Values) value.Dispose();
        _strings.Clear();
        _values.Clear();
        _options.Clear();
        _updated = false;
    }
}
