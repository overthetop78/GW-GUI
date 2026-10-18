using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Functions;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Bandai.Emulators.Common.Interop.Services;

internal sealed class CoreOptionHost : IDisposable, IEmulationCoreOptionCommands
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

    public void RegisterLegacyVariables(nint data) => ReplaceCatalog(CoreOptionFunctions.CopyLegacyVariables(data), []);

    public void RegisterVersionOne(nint data) => ReplaceCatalog(CoreOptionFunctions.CopyLegacyDefinitions(data), []);

    public void RegisterVersionOneInternational(nint data)
    => ReplaceCatalog(EmulationCoreOptionFunctions.CopyInternationalDefinitions<CoreOption>(data,
        CoreOptionConstants.EnglishPointerIndex, CoreOptionConstants.LocalPointerIndex,
        CoreOptionFunctions.CopyLegacyDefinitions, CoreOptionFunctions.MergeLocalizedDefinitions), []);

    public void RegisterVersionTwo(nint data)
    {
        var options = CoreOptionFunctions.CopyVersionTwoDefinitions(data, out var categories);
        ReplaceCatalog(options, categories);
    }

    public void RegisterVersionTwoInternational(nint data)
    {
        var options = EmulationCoreOptionFunctions.CopyInternationalDefinitions<CoreOption, CoreOptionCategory>(data,
            CoreOptionConstants.EnglishPointerIndex, CoreOptionConstants.LocalPointerIndex,
            CoreOptionFunctions.CopyVersionTwoDefinitions, CoreOptionFunctions.MergeLocalizedDefinitions,
            CoreOptionFunctions.MergeLocalizedCategories, out var categories);
        ReplaceCatalog(options, categories);
    }

    public bool ReturnValue(nint data) => _host.ReturnValue(data);

    public bool SetNativeValue(nint data) => _host.SetNativeValue(data);

    public bool GetAndClearUpdated(nint data) => _host.GetAndClearUpdated(data);

    internal void SetValue(string key, string value)
    {
        if (!_host.TrySetValue(key, value))
            throw new EmulationException(ErrorCategory.Option, ErrorCode.OptionInvalid, CoreFunctions.CreateInvalidOptionValueMessage(key, value));
    }

    public bool ApplyVisibility(nint data)
        => _host.ApplyVisibility(data);

    public bool CaptureDisplayUpdate(nint data)
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
