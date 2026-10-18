using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Functions;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Bandai.Emulators.Common.Interop.Services;

internal sealed class CoreOptionHost : IDisposable
{
    private readonly EmulationCoreOptionHost<CoreOption, CoreOptionValue> _host;
    private readonly IReadOnlyDictionary<string, CoreOption> _presentation;

    internal CoreOptionHost(IReadOnlyDictionary<string, string> configured, IReadOnlyList<CoreOption>? presentation = null)
    {
        _host = new EmulationCoreOptionHost<CoreOption, CoreOptionValue>(
            configured,
            keyFor: static option => option.Key,
            defaultFor: static option => option.DefaultValue,
            choicesFor: static option => option.Values,
            valueFor: static value => value.Value,
            withCurrent: static (option, value) => option with { CurrentValue = value },
            withVisibility: (option, visible) => option with { IsVisible = visible && IsExposed(option.Key) },
            initialize: (option, selected, nativeVisible) => option with
            {
                CurrentValue = selected,
                IsVisible = option.IsVisible && IsExposed(option.Key) && (nativeVisible)
            },
            present: ApplyPresentation);
        _presentation = (presentation ?? []).ToDictionary(option => option.Key, StringComparer.Ordinal);
    }

    internal IReadOnlyList<CoreOption> Catalog => _host.Catalog;
    internal IReadOnlyList<CoreOptionCategory> Categories { get; private set; } = [];
    internal IReadOnlyDictionary<string, string> DocumentValues => _host.DocumentValues;

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

    internal bool ReturnValue(nint data) => _host.ReturnValue(data);

    internal bool SetNativeValue(nint data) => _host.SetNativeValue(data);

    internal bool GetAndClearUpdated(nint data) => _host.GetAndClearUpdated(data);

    internal void SetValue(string key, string value)
    {
        if (!_host.TrySetValue(key, value))
            throw new EmulationException(ErrorCategory.Option, ErrorCode.OptionInvalid, CoreFunctions.CreateInvalidOptionValueMessage(key, value));
    }

    internal bool ApplyVisibility(nint data)
        => _host.ApplyVisibility(data);

    internal bool CaptureDisplayUpdate(nint data)
        => _host.CaptureDisplayUpdate(data);

    internal void ValidateConfiguredValues() => _host.ValidateConfiguredValues(static (key, value) => throw new EmulationException(ErrorCategory.Option, ErrorCode.OptionInvalid,
                CoreFunctions.CreateInvalidOptionValueMessage(key, value)));

    private void ReplaceCatalog(IReadOnlyList<CoreOption> options, IReadOnlyList<CoreOptionCategory> categories)
    {
        Categories = categories;
        _host.ReplaceCatalog(options);
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
                    InvariantDisplayValue = choice.InvariantDisplayValue
                }
                : value).ToArray()
        };
    }

    public void Dispose()
        => _host.Dispose();
}
