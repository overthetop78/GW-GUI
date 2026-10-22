using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Atari.Common.Functions.CoreHostFunctions;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Services;

internal sealed class CoreOptionHost : IDisposable, IEmulationCoreOptionCommands
{
    private readonly IEmulationCoreOptionHost<CoreOption> _host;

    internal CoreOptionHost(IReadOnlyDictionary<string, string> configured)
    {
        _host = CommonCoreHostFunctions.NativeResources.CreateCoreOptionHost<CoreOption, CoreOptionValue>(
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

    public void RegisterLegacyVariables(nint data) => ReplaceCatalog(CoreOptionFunctions.CopyLegacyVariables(data), []);

    public void RegisterVersionOne(nint data) => ReplaceCatalog(CoreOptionFunctions.CopyLegacyDefinitions(data), []);

    public void RegisterVersionOneInternational(nint data)
    => ReplaceCatalog(CommonCoreHostFunctions.OptionProcessing.CopyInternationalDefinitions<CoreOption>(data,
        CoreOptionConstants.EnglishPointerIndex, CoreOptionConstants.LocalPointerIndex,
        CoreOptionFunctions.CopyLegacyDefinitions, CoreOptionFunctions.MergeLocalizedDefinitions), []);

    public void RegisterVersionTwo(nint data)
    {
        var options = CoreOptionFunctions.CopyVersionTwoDefinitions(data, out var categories);
        ReplaceCatalog(options, categories);
    }

    public void RegisterVersionTwoInternational(nint data)
    {
        var options = CommonCoreHostFunctions.OptionProcessing.CopyInternationalDefinitions<CoreOption, CoreOptionCategory>(data,
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

    public void Dispose()
        => _host.Dispose();
}
