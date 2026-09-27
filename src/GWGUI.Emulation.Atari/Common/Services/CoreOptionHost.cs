using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed class CoreOptionHost : IDisposable
{
    private readonly Dictionary<string, string> _values;
    private readonly HashSet<string> _configuredKeys;
    private readonly Dictionary<string, ExternalCoreUtf8String> _nativeValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _visibility = new(StringComparer.Ordinal);
    private ExternalCoreApi.UpdateCoreOptionsDisplay? _displayUpdate;
    private int _updated;

    internal CoreOptionHost(IReadOnlyDictionary<string, string> configured)
    {
        _values = new(configured, StringComparer.Ordinal);
        _configuredKeys = configured.Keys.ToHashSet(StringComparer.Ordinal);
    }

    internal IReadOnlyList<CoreOption> Catalog { get; private set; } = [];
    internal IReadOnlyList<CoreOptionCategory> Categories { get; private set; } = [];
    internal IReadOnlyDictionary<string, string> DocumentValues => _values;

    internal void RegisterLegacyVariables(nint data) =>
        ReplaceCatalog(CoreOptionFunctions.CopyLegacyVariables(data), []);

    internal void RegisterVersionOne(nint data) =>
        ReplaceCatalog(CoreOptionFunctions.CopyLegacyDefinitions(data), []);

    internal void RegisterVersionOneInternational(nint data)
    {
        if (data == nint.Zero) { ReplaceCatalog([], []); return; }
        var englishPointer = Marshal.ReadIntPtr(data,
            CoreOptionConstants.EnglishPointerIndex * IntPtr.Size);
        var localPointer = Marshal.ReadIntPtr(data,
            CoreOptionConstants.LocalPointerIndex * IntPtr.Size);
        var english = CoreOptionFunctions.CopyLegacyDefinitions(englishPointer);
        var local = CoreOptionFunctions.CopyLegacyDefinitions(localPointer);
        ReplaceCatalog(CoreOptionFunctions.MergeLocalizedDefinitions(english, local), []);
    }

    internal void RegisterVersionTwo(nint data)
    {
        var options = CoreOptionFunctions.CopyVersionTwoDefinitions(data, out var categories);
        ReplaceCatalog(options, categories);
    }

    internal void RegisterVersionTwoInternational(nint data)
    {
        if (data == nint.Zero) { ReplaceCatalog([], []); return; }
        var englishPointer = Marshal.ReadIntPtr(data,
            CoreOptionConstants.EnglishPointerIndex * IntPtr.Size);
        var localPointer = Marshal.ReadIntPtr(data,
            CoreOptionConstants.LocalPointerIndex * IntPtr.Size);
        var english = CoreOptionFunctions.CopyVersionTwoDefinitions(englishPointer, out var englishCategories);
        var local = CoreOptionFunctions.CopyVersionTwoDefinitions(localPointer, out var localCategories);
        ReplaceCatalog(CoreOptionFunctions.MergeLocalizedDefinitions(english, local),
            CoreOptionFunctions.MergeLocalizedCategories(englishCategories, localCategories));
    }

    internal bool ReturnValue(nint data)
    {
        if (data == nint.Zero) return true;
        var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data);
        var key = variable.Key == nint.Zero ? null : Marshal.PtrToStringUTF8(variable.Key);
        variable.Value = key is not null && _values.TryGetValue(key, out var value)
            ? NativeValue(key, value) : nint.Zero;
        Marshal.StructureToPtr(variable, data, false);
        return true;
    }

    internal bool SetNativeValue(nint data)
    {
        if (data == nint.Zero) return false;
        var variable = Marshal.PtrToStructure<ExternalCoreApi.Variable>(data);
        var key = variable.Key == nint.Zero ? null : Marshal.PtrToStringUTF8(variable.Key);
        var value = variable.Value == nint.Zero ? null : Marshal.PtrToStringUTF8(variable.Value);
        if (string.IsNullOrWhiteSpace(key) || value is null) return false;
        return SetValue(key, value, requireAnnouncedKey: true);
    }

    internal bool GetAndClearUpdated(nint data)
    {
        if (data == nint.Zero) return false;
        return CoreFunctions.WriteBoolean(data,
            Interlocked.Exchange(ref _updated, CoreOptionConstants.NoEntries) != CoreOptionConstants.NoEntries);
    }

    internal void SetValue(string key, string value)
    {
        if (!SetValue(key, value, requireAnnouncedKey: true))
            throw new EmulationException(ErrorCategory.Option, ErrorCode.OptionInvalid,
                CoreFunctions.CreateInvalidOptionValueMessage(key, value));
    }

    internal bool ApplyVisibility(nint data)
    {
        if (data == nint.Zero) return false;
        var display = Marshal.PtrToStructure<ExternalCoreApi.CoreOptionDisplay>(data);
        var key = display.Key == nint.Zero ? null : Marshal.PtrToStringUTF8(display.Key);
        if (string.IsNullOrWhiteSpace(key)) return false;
        _visibility[key] = display.Visible;
        Catalog = Catalog.Select(option => option.Key.Equals(key, StringComparison.Ordinal)
            ? option with { IsVisible = display.Visible } : option).ToArray();
        return true;
    }

    internal bool CaptureDisplayUpdate(nint data)
    {
        if (data == nint.Zero) return false;
        var pointer = Marshal.PtrToStructure<ExternalCoreApi.CoreOptionsUpdateDisplayCallback>(data).Callback;
        _displayUpdate = pointer == nint.Zero ? null
            : Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.UpdateCoreOptionsDisplay>(pointer);
        return true;
    }

    internal void ValidateConfiguredValues()
    {
        foreach (var key in _configuredKeys)
        {
            var option = Catalog.FirstOrDefault(item => item.Key.Equals(key, StringComparison.Ordinal));
            if (option is null || option.Values.Count == CoreOptionConstants.NoEntries) continue;
            if (!option.Values.Any(item => item.Value.Equals(_values[key], StringComparison.Ordinal)))
                throw new EmulationException(ErrorCategory.Option, ErrorCode.OptionInvalid,
                    CoreFunctions.CreateInvalidOptionValueMessage(key, _values[key]));
        }
    }

    private bool SetValue(string key, string value, bool requireAnnouncedKey)
    {
        var option = Catalog.FirstOrDefault(item => item.Key.Equals(key, StringComparison.Ordinal));
        if (requireAnnouncedKey && option is null) return false;
        if (option is not null && option.Values.Count != CoreOptionConstants.NoEntries &&
            !option.Values.Any(item => item.Value.Equals(value, StringComparison.Ordinal))) return false;
        _values[key] = value;
        ReplaceNativeValue(key, value);
        Catalog = Catalog.Select(item => item.Key.Equals(key, StringComparison.Ordinal)
            ? item with { CurrentValue = value } : item).ToArray();
        Interlocked.Exchange(ref _updated, CoreOptionConstants.DefaultValuePointerCount);
        _displayUpdate?.Invoke();
        return true;
    }

    private void ReplaceCatalog(IReadOnlyList<CoreOption> options,
        IReadOnlyList<CoreOptionCategory> categories)
    {
        Categories = categories;
        Catalog = options.Select(option =>
        {
            var selected = _values.TryGetValue(option.Key, out var configured) ? configured : option.DefaultValue;
            if (!_values.ContainsKey(option.Key)) _values[option.Key] = selected;
            ReplaceNativeValue(option.Key, selected);
            return option with
            {
                CurrentValue = selected,
                IsVisible = !_visibility.TryGetValue(option.Key, out var visible) || visible
            };
        }).ToArray();
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
        _nativeValues.Add(key, new ExternalCoreUtf8String(value));
    }

    public void Dispose()
    {
        foreach (var value in _nativeValues.Values) value.Dispose();
        _nativeValues.Clear();
    }
}
