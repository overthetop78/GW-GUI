using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Watara.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Watara.Emulators.Potator.Interop.Services;

internal sealed class OptionCallbackHost : IDisposable
{
    private readonly IEmulationVariableOptionHost<CoreOption> _host;

    internal OptionCallbackHost(IReadOnlyDictionary<string, string> values) => _host = CommonCoreHostFunctions.NativeResources.CreateVariableOptionHost<CoreOption>(values,
        static (key, name, defaultValue, current, choices) => new CoreOption(key, name, null, null, defaultValue, current, choices.Select(value => new CoreOptionValue(value, value)).ToArray()),
        static (option, value) => option.Values.Any(choice => choice.Value == value),
        static (option, value) => option with { CurrentValue = value });

    internal IReadOnlyList<CoreOption> Options => _host.Options;
    internal bool Register(nint data) => _host.Register(data);
    internal bool ReturnValue(nint data) => _host.ReturnValue(data);
    internal void SetValue(string key, string value) => _host.SetValue(key, value);
    internal bool ReturnUpdated(nint data) => _host.ReturnUpdated(data);
    public void Dispose() => _host.Dispose();
}
