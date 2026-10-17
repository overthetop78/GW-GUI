using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Services;

internal sealed class CoreOptionHost : IDisposable
{
    private readonly Dictionary<string, string> _values;
    private readonly HashSet<string> _configuredKeys;
    private readonly IReadOnlyDictionary<string, CoreOption> _presentation;
    private readonly Dictionary<string, ExternalCoreUtf8String> _nativeValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _visibility = new(StringComparer.Ordinal);
    private ExternalCoreApi.UpdateCoreOptionsDisplay? _displayUpdate;
    private int _updated;

    internal CoreOptionHost(IReadOnlyDictionary<string, string> configured, IReadOnlyList<CoreOption>? presentation = null)
    {
        _values = new(configured, StringComparer.Ordinal);
        _configuredKeys = configured.Keys.ToHashSet(StringComparer.Ordinal);
        _presentation = (presentation ?? []).ToDictionary(option => option.Key, StringComparer.Ordinal);
    }

    internal IReadOnlyList<CoreOption> Catalog { get; private set; } = [];
    internal IReadOnlyList<CoreOptionCategory> Categories { get; private set; } = [];
    internal IReadOnlyDictionary<string, string> DocumentValues => _values;

    internal void RegisterLegacyVariables(nint data) => ReplaceCatalog(CoreOptionFunctions.CopyLegacyVariables(data), []);

    internal void RegisterVersionOne(nint data) => ReplaceCatalog(CoreOptionFunctions.CopyLegacyDefinitions(data), []);

    internal void RegisterVersionOneInternational(nint data)
    {
        if (data == nint.Zero) { ReplaceCatalog([], []); return; }
        var englishPointer = Marshal.ReadIntPtr(data, CoreOptionConstants.EnglishPointerIndex * IntPtr.Size);
        var localPointer = Marshal.ReadIntPtr(data, CoreOptionConstants.LocalPointerIndex * IntPtr.Size);
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
        var englishPointer = Marshal.ReadIntPtr(data, CoreOptionConstants.EnglishPointerIndex * IntPtr.Size);
        var localPointer = Marshal.ReadIntPtr(data, CoreOptionConstants.LocalPointerIndex * IntPtr.Size);
        var english = CoreOptionFunctions.CopyVersionTwoDefinitions(englishPointer, out var englishCategories);
        var local = CoreOptionFunctions.CopyVersionTwoDefinitions(localPointer, out var localCategories);
        ReplaceCatalog(CoreOptionFunctions.MergeLocalizedDefinitions(english, local), CoreOptionFunctions.MergeLocalizedCategories(englishCategories, localCategories));
    }

    internal bool ReturnValue(nint data) => EmulationCoreOptionFunctions.ReturnNativeValue(data, _values, NativeValue);

    internal bool SetNativeValue(nint data) => EmulationCoreOptionFunctions.SetNativeValue(data, (key, value) => SetValue(key, value, requireAnnouncedKey: true));

    internal bool GetAndClearUpdated(nint data) => EmulationCoreOptionFunctions.GetAndClearUpdated(data, ref _updated);

    internal void SetValue(string key, string value)
    {
        if (!SetValue(key, value, requireAnnouncedKey: true))
            throw new EmulationException(ErrorCategory.Option, ErrorCode.OptionInvalid, CoreFunctions.CreateInvalidOptionValueMessage(key, value));
    }

    internal bool ApplyVisibility(nint data)
    {
        if (!EmulationCoreOptionFunctions.ApplyNativeVisibility(data, _visibility, Catalog, static option => option.Key,
            (option, visible) => option with { IsVisible = visible && IsExposed(option.Key) }, out var catalog)) return false;
        Catalog = catalog;
        return true;
    }

    internal bool CaptureDisplayUpdate(nint data)
    {
        if (!EmulationCoreOptionFunctions.TryGetDisplayCallback(data, out var callback)) return false;
        _displayUpdate = callback;
        return true;
    }

    internal void ValidateConfiguredValues() =>
        EmulationCoreOptionFunctions.ValidateConfiguredValues(_configuredKeys, _values, Catalog,
            static option => option.Key, static option => option.Values, static value => value.Value,
            static (key, value) => throw new EmulationException(ErrorCategory.Option, ErrorCode.OptionInvalid,
                CoreFunctions.CreateInvalidOptionValueMessage(key, value)));

    private bool SetValue(string key, string value, bool requireAnnouncedKey)
    {
        var option = Catalog.FirstOrDefault(item => item.Key.Equals(key, StringComparison.Ordinal));
        if (requireAnnouncedKey && option is null) return false;
        if (option is not null && option.Values.Count != CoreOptionConstants.NoEntries &&
            !option.Values.Any(item => item.Value.Equals(value, StringComparison.Ordinal))) return false;
        _values[key] = value;
        ReplaceNativeValue(key, value);
        Catalog = Catalog.Select(item => item.Key.Equals(key, StringComparison.Ordinal) ? item with { CurrentValue = value } : item).ToArray();
        Interlocked.Exchange(ref _updated, CoreOptionConstants.DefaultValuePointerCount);
        _displayUpdate?.Invoke();
        return true;
    }

    private void ReplaceCatalog(IReadOnlyList<CoreOption> options, IReadOnlyList<CoreOptionCategory> categories)
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
                IsVisible = option.IsVisible && IsExposed(option.Key) && (!_visibility.TryGetValue(option.Key, out var visible) || visible)
            };
        }).Select(ApplyPresentation).ToArray();
    }

    private bool IsExposed(string key) => !_presentation.TryGetValue(key, out var presentation) || presentation.IsVisible;

    private CoreOption ApplyPresentation(CoreOption option)
    {
        if (!_presentation.TryGetValue(option.Key, out var presentation)) return option;
        var choices = presentation.Values.ToDictionary(value => value.Value, StringComparer.Ordinal);
        return option with
        {
            Name = presentation.Name,
            ResourceKey = presentation.ResourceKey,
            ExplanationResourceKey = presentation.ExplanationResourceKey,
            RequiresRuntimeReload = presentation.RequiresRuntimeReload,
            Values = option.Values.Select(value => choices.TryGetValue(value.Value, out var choice)
                ? value with
                {
                    Label = choice.InvariantDisplayValue ?? value.Label,
                    ResourceKey = choice.ResourceKey,
                    InvariantDisplayValue = choice.InvariantDisplayValue,
                    VideoStandard = choice.VideoStandard,
                    FrequencyHz = choice.FrequencyHz
                }
                : value).ToArray()
        };
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
        foreach (var value in _nativeValues.Values) value.Dispose();
        _nativeValues.Clear();
    }
}
