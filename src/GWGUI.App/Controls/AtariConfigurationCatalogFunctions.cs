using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariConfigurationCatalogFunctions
{
    private static readonly AtariMachineModel[] DisplayOrder =
    [
        AtariMachineModel.St,
        AtariMachineModel.Stf,
        AtariMachineModel.Stfm,
        AtariMachineModel.MegaSt,
        AtariMachineModel.Ste,
        AtariMachineModel.MegaSte,
        AtariMachineModel.Tt,
        AtariMachineModel.Falcon,
        AtariMachineModel.Atari400,
        AtariMachineModel.Atari800,
        AtariMachineModel.Atari800Xl,
        AtariMachineModel.Atari130Xe,
        AtariMachineModel.XlXe,
        AtariMachineModel.Xegs,
        AtariMachineModel.Atari2600,
        AtariMachineModel.Atari5200,
        AtariMachineModel.Atari7800,
        AtariMachineModel.Lynx,
        AtariMachineModel.Jaguar,
        AtariMachineModel.JaguarCd
    ];

    internal static IReadOnlyList<AtariModelItem> Models() => DisplayOrder
        .Select(model => new AtariModelItem(model, ModelName(model)))
        .ToArray();

    internal static string ModelName(AtariMachineModel model) => model == AtariMachineModel.Lynx
        ? "Atari Lynx" : LocExtension.Get(ResourceKey(model));

    internal static AtariMachineConfiguration ChangeModel(AtariMachineConfiguration? current,
        AtariMachineModel model) => current is not null && current.Model == model
        ? current
        : new AtariMachineConfiguration(model);

    internal static AtariMachineConfiguration ConfigurationForModel(AtariMachineConfiguration? current,
        AtariMachineModel model, IEnumerable<AtariMachineConfiguration> savedConfigurations) =>
        savedConfigurations.FirstOrDefault(configuration => configuration.Model == model)
        ?? ChangeModel(current, model);

    internal static string DisplayName(AtariMachineConfiguration configuration, string modelName) =>
        EmulationConfigurationDisplayFunctions.Atari(configuration, modelName);

    private static string ResourceKey(AtariMachineModel model) => model switch
    {
        AtariMachineModel.St => AtariConfigurationCatalogConstants.StResource,
        AtariMachineModel.Stf => AtariConfigurationCatalogConstants.StfResource,
        AtariMachineModel.Stfm => AtariConfigurationCatalogConstants.StfmResource,
        AtariMachineModel.MegaSt => AtariConfigurationCatalogConstants.MegaStResource,
        AtariMachineModel.Ste => AtariConfigurationCatalogConstants.SteResource,
        AtariMachineModel.MegaSte => AtariConfigurationCatalogConstants.MegaSteResource,
        AtariMachineModel.Tt => AtariConfigurationCatalogConstants.TtResource,
        AtariMachineModel.Falcon => AtariConfigurationCatalogConstants.FalconResource,
        AtariMachineModel.Atari400 => AtariConfigurationCatalogConstants.Atari400Resource,
        AtariMachineModel.Atari800 => AtariConfigurationCatalogConstants.Atari800Resource,
        AtariMachineModel.Atari800Xl => AtariConfigurationCatalogConstants.Atari800XlResource,
        AtariMachineModel.Atari130Xe => AtariConfigurationCatalogConstants.Atari130XeResource,
        AtariMachineModel.XlXe => AtariConfigurationCatalogConstants.XlXeResource,
        AtariMachineModel.Xegs => AtariConfigurationCatalogConstants.XegsResource,
        AtariMachineModel.Atari5200 => AtariConfigurationCatalogConstants.Atari5200Resource,
        AtariMachineModel.Atari2600 => AtariConfigurationCatalogConstants.Atari2600Resource,
        AtariMachineModel.Atari7800 => AtariConfigurationCatalogConstants.Atari7800Resource,
        AtariMachineModel.Lynx => AtariConfigurationCatalogConstants.LynxResource,
        AtariMachineModel.Jaguar => AtariConfigurationCatalogConstants.JaguarResource,
        AtariMachineModel.JaguarCd => AtariConfigurationCatalogConstants.JaguarCdResource,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };
}

internal sealed record AtariModelItem(AtariMachineModel Model, string DisplayName)
{
    public override string ToString() => DisplayName;
}

internal sealed record AtariConfigurationItem(AtariMachineConfiguration Configuration, string DisplayName);
