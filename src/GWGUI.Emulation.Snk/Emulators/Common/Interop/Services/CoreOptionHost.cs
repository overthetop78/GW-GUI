using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;

internal sealed class CoreOptionHost : IDisposable
{
    private readonly EmulationCoreOptionHost<CoreOption, CoreOptionValue> _host;

    internal CoreOptionHost(IReadOnlyDictionary<string, string> configured)
    {
        _host = new EmulationCoreOptionHost<CoreOption, CoreOptionValue>(
            configured,
            keyFor: static option => option.Key,
            defaultFor: static option => option.DefaultValue,
            choicesFor: static option => option.Values,
            valueFor: static value => value.Value,
            withCurrent: static (option, value) => option with { CurrentValue = value },
            withVisibility: (option, visible) => option with { IsVisible = visible },
            initialize: (option, selected, nativeVisible) => option with
            {
                CurrentValue = selected,
                IsVisible = nativeVisible
            });
    }

    internal IReadOnlyList<CoreOption> Catalog => _host.Catalog;
    internal IReadOnlyList<CoreOptionCategory> Categories { get; private set; } = [];
    internal IReadOnlyDictionary<string, string> DocumentValues => _host.DocumentValues;

    internal void RegisterLegacyVariables(nint data) => ReplaceCatalog(CoreOptionFunctions.CopyLegacyVariables(data), []);

    internal void RegisterVersionOne(nint data) => ReplaceCatalog(CoreOptionFunctions.CopyLegacyDefinitions(data), []);

    internal void RegisterVersionOneInternational(nint data)
    => ReplaceCatalog(EmulationCoreOptionFunctions.CopyInternationalDefinitions<CoreOption>(data,
        CoreOptionConstants.EnglishPointerIndex, CoreOptionConstants.LocalPointerIndex,
        CoreOptionFunctions.CopyLegacyDefinitions, CoreOptionFunctions.MergeLocalizedDefinitions), []);

    internal void RegisterVersionTwo(nint data)
    {
        var options = CoreOptionFunctions.CopyVersionTwoDefinitions(data, out var categories);
        ReplaceCatalog(options, categories);
    }

    internal void RegisterVersionTwoInternational(nint data)
    {
        var options = EmulationCoreOptionFunctions.CopyInternationalDefinitions<CoreOption, CoreOptionCategory>(data,
            CoreOptionConstants.EnglishPointerIndex, CoreOptionConstants.LocalPointerIndex,
            CoreOptionFunctions.CopyVersionTwoDefinitions, CoreOptionFunctions.MergeLocalizedDefinitions,
            CoreOptionFunctions.MergeLocalizedCategories, out var categories);
        ReplaceCatalog(options, categories);
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

    public void Dispose()
        => _host.Dispose();
}
