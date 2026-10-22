using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Exceptions;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Services;

internal sealed class EmulationLegacyOptionHost<TOption> : IDisposable
{
    private const int MaximumVariableCount = 4096;
    private const char DescriptionSeparator = ';';
    private const char ValueSeparator = '|';
    private readonly Dictionary<string, string> _values;
    private readonly Dictionary<string, TOption> _options = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IEmulationNativeString> _strings = new(StringComparer.Ordinal);
    private bool _updated;
    private readonly Func<string, string, string, string, IReadOnlyList<string>, TOption> _createOption;
    private readonly Func<TOption, string, bool> _accepts;
    private readonly Func<TOption, string, TOption> _withCurrent;
    internal EmulationLegacyOptionHost(IReadOnlyDictionary<string, string> values,
        Func<string, string, string, string, IReadOnlyList<string>, TOption> createOption,
        Func<TOption, string, bool> accepts, Func<TOption, string, TOption> withCurrent)
    {
        _values = new(values, StringComparer.Ordinal);
        _createOption = createOption;
        _accepts = accepts;
        _withCurrent = withCurrent;
    }
    internal IReadOnlyList<TOption> Options => _options.Values.ToArray();

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
            if (!values.Contains(current, StringComparer.Ordinal)) throw GWGUI.Emulation.Services.EmulationErrorService.CreateError(EmulationMessageCategory.Emulator, EmulationMessageCode.OptionInvalid, null);
            _values[key] = current;
            _options[key] = _createOption(key, description[..separator].Trim(), values[0], current, values);
        }
        throw new InvalidOperationException("Native option table has no terminating entry.");
    }

    internal bool ReturnValue(nint data)
    {
        if (data == nint.Zero) return false;
        var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data);
        var key = Marshal.PtrToStringUTF8(variable.Key);
        if (key is null || !_values.TryGetValue(key, out var value)) return false;
        if (!_strings.TryGetValue(key, out var text)) text = ExternalCoreUtf8String.AddTo(_strings, key, value);
        variable.Value = text.Pointer;
        Marshal.StructureToPtr(variable, data, false);
        return true;
    }

    internal void SetValue(string key, string value)
    {
        if (!_options.TryGetValue(key, out var option) || !_accepts(option, value)) throw new ArgumentException(nameof(value));
        if (_values[key] == value) return;
        _values[key] = value;
        _options[key] = _withCurrent(option, value);
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
        try { foreach (var value in _strings.Values) value.Dispose(); }
        finally
        {
            _strings.Clear();
            _values.Clear();
            _options.Clear();
            _updated = false;
        }
    }
}
