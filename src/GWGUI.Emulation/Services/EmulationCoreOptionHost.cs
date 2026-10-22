using System.Runtime.ExceptionServices;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Services;

internal sealed class EmulationCoreOptionHost<TOption, TValue> : IEmulationCoreOptionHost<TOption> where TOption : class
{
    private readonly Dictionary<string, string> _values;
    private readonly HashSet<string> _configuredKeys;
    private readonly Dictionary<string, IEmulationNativeString> _nativeValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _visibility = new(StringComparer.Ordinal);
    private readonly Func<TOption, string> _keyFor;
    private readonly Func<TOption, string> _defaultFor;
    private readonly Func<TOption, IReadOnlyList<TValue>> _choicesFor;
    private readonly Func<TValue, string> _valueFor;
    private readonly Func<TOption, string, TOption> _withCurrent;
    private readonly Func<TOption, bool, TOption> _withVisibility;
    private readonly Func<TOption, string, bool, TOption> _initialize;
    private readonly Func<TOption, TOption>? _present;
    private ExternalCoreApi.UpdateCoreOptionsDisplay? _displayUpdate;
    private int _updated;

    internal EmulationCoreOptionHost(IReadOnlyDictionary<string, string> configured,
        Func<TOption, string> keyFor, Func<TOption, string> defaultFor,
        Func<TOption, IReadOnlyList<TValue>> choicesFor, Func<TValue, string> valueFor,
        Func<TOption, string, TOption> withCurrent, Func<TOption, bool, TOption> withVisibility,
        Func<TOption, string, bool, TOption> initialize, Func<TOption, TOption>? present = null)
    {
        _values = new(configured, StringComparer.Ordinal);
        _configuredKeys = configured.Keys.ToHashSet(StringComparer.Ordinal);
        _keyFor = keyFor;
        _defaultFor = defaultFor;
        _choicesFor = choicesFor;
        _valueFor = valueFor;
        _withCurrent = withCurrent;
        _withVisibility = withVisibility;
        _initialize = initialize;
        _present = present;
    }

    public IReadOnlyList<TOption> Catalog { get; private set; } = [];
    public IReadOnlyDictionary<string, string> DocumentValues => _values;

    public bool ReturnValue(nint data) => EmulationCoreOptionFunctions.ReturnNativeValue(data, _values, NativeValue);

    public bool SetNativeValue(nint data) => EmulationCoreOptionFunctions.SetNativeValue(data, TrySetValue);

    public bool GetAndClearUpdated(nint data) => EmulationCoreOptionFunctions.GetAndClearUpdated(data, ref _updated);

    public bool ApplyVisibility(nint data)
    {
        if (!EmulationCoreOptionFunctions.ApplyNativeVisibility(data, _visibility, Catalog, _keyFor,
                _withVisibility, out var catalog)) return false;
        Catalog = catalog;
        return true;
    }

    public bool CaptureDisplayUpdate(nint data)
    {
        if (!EmulationCoreOptionFunctions.TryGetDisplayCallback(data, out var callback)) return false;
        _displayUpdate = callback;
        return true;
    }

    public void ValidateConfiguredValues(Action<string, string> invalid) =>
        EmulationCoreOptionFunctions.ValidateConfiguredValues(_configuredKeys, _values, Catalog,
            _keyFor, _choicesFor, _valueFor, invalid);

    public bool TrySetValue(string key, string value)
    {
        var option = Catalog.FirstOrDefault(item => _keyFor(item).Equals(key, StringComparison.Ordinal));
        if (option is null) return false;
        if (_choicesFor(option).Count != 0 &&
            !_choicesFor(option).Any(item => _valueFor(item).Equals(value, StringComparison.Ordinal))) return false;
        _values[key] = value;
        ReplaceNativeValue(key, value);
        Catalog = Catalog.Select(item => _keyFor(item).Equals(key, StringComparison.Ordinal)
            ? _withCurrent(item, value) : item).ToArray();
        Interlocked.Exchange(ref _updated, 1);
        _displayUpdate?.Invoke();
        return true;
    }

    public void ReplaceCatalog(IReadOnlyList<TOption> options)
    {
        var catalog = options.Select(option =>
        {
            var key = _keyFor(option);
            var selected = _values.TryGetValue(key, out var configured) ? configured : _defaultFor(option);
            if (!_values.ContainsKey(key)) _values[key] = selected;
            ReplaceNativeValue(key, selected);
            return _initialize(option, selected, !_visibility.TryGetValue(key, out var visible) || visible);
        });
        Catalog = (_present is null ? catalog : catalog.Select(_present)).ToArray();
    }

    private nint NativeValue(string key, string value)
    {
        if (_nativeValues.TryGetValue(key, out var native)) return native.Pointer;
        ReplaceNativeValue(key, value);
        return _nativeValues[key].Pointer;
    }

    private void ReplaceNativeValue(string key, string value)
    {
        if (_nativeValues.Remove(key, out var previous)) previous.Dispose();
        ExternalCoreUtf8String.AddTo(_nativeValues, key, value);
    }

    public void Dispose()
    {
        Exception? firstError = null;
        try
        {
            foreach (var value in _nativeValues.Values)
            {
                try { value.Dispose(); }
                catch (Exception error) { firstError ??= error; }
            }
        }
        finally { _nativeValues.Clear(); }
        if (firstError is not null) ExceptionDispatchInfo.Capture(firstError).Throw();
    }
}
