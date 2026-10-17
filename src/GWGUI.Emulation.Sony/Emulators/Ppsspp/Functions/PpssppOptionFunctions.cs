using GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Dictionaries;
using GWGUI.Emulation.Sony.Emulators.Common.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions;

internal static class PpssppOptionFunctions
{
    internal static IReadOnlyList<CoreOption> Options(MachineConfiguration configuration)
    {
        var vulkan = ToNative(configuration).Options?.GetValueOrDefault(OptionConstants.BackendKey) == OptionConstants.VulkanBackend;
        return OptionCatalog.All.Select(option => option.Key switch
        {
            OptionConstants.InternalResolutionKey or OptionConstants.Cropto16x9Key => option with { RequiresRestart = vulkan },
            OptionConstants.MulitsampleLevelKey => option with { IsVisible = vulkan },
            _ => option
        }).ToArray();
    }

    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        return SystemLanguageFunctions.Configure(GraphicsSettingsFunctions.Configure(configuration with
        {
            Options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        }, OptionCatalog.All));
    }
}


