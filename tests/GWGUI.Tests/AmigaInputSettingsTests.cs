using GWGUI.App.Controls;
using GWGUI.Emulation.Amiga;

namespace GWGUI.Tests;

public sealed class AmigaInputSettingsTests
{
    [Fact]
    public void EveryCatalogModelExposesItsTwoStandardControllerPorts()
    {
        Assert.Equal(10, AmigaModelCatalog.All.Count);
        Assert.All(AmigaModelCatalog.All, model => Assert.Equal(2, model.ControllerPortCount));
    }

    [Fact]
    public void EveryCatalogModelOffersThePuaeStandardControllerTypes()
    {
        foreach (var model in AmigaModelCatalog.All)
        {
            var types = AmigaControllerSettingsFunctions.Types(model);
            Assert.DoesNotContain(AmigaControllerType.Automatic, types);
            Assert.Contains(AmigaControllerType.Joystick, types);
            Assert.Contains(AmigaControllerType.AnalogJoystick, types);
            Assert.Contains(AmigaControllerType.None, types);
            Assert.Equal(model.Id == "CD32", types.Contains(AmigaControllerType.Cd32Pad));
        }
    }

    [Fact]
    public void Cd32OffersItsPadWhileA500DoesNot()
    {
        var cd32 = AmigaControllerSettingsFunctions.Types(AmigaModelCatalog.All.Single(model => model.Id == "CD32"));
        var a500 = AmigaControllerSettingsFunctions.Types(AmigaModelCatalog.All.Single(model => model.Id == "A500"));

        Assert.Contains(AmigaControllerType.Cd32Pad, cd32);
        Assert.DoesNotContain(AmigaControllerType.Cd32Pad, a500);
    }

    [Fact]
    public void LegacyAutomaticControllerIsConvertedToTheModelDefault()
    {
        var cd32 = AmigaModelCatalog.All.Single(model => model.Id == "CD32");
        var a500 = AmigaModelCatalog.All.Single(model => model.Id == "A500");

        Assert.Equal(AmigaControllerType.Cd32Pad,
            AmigaControllerSettingsFunctions.Normalize(cd32, AmigaControllerType.Automatic));
        Assert.Equal(AmigaControllerType.Joystick,
            AmigaControllerSettingsFunctions.Normalize(a500, AmigaControllerType.Automatic));
    }

    [Fact]
    public void ParallelAdapterPortsOnlyAcceptJoysticks()
    {
        Assert.Equal([AmigaControllerType.Joystick, AmigaControllerType.None],
            AmigaControllerSettingsFunctions.ParallelPortTypes());
    }

    [Fact]
    public void Cd32PadDefinesEveryEmulatedButton()
    {
        var actions = AmigaControllerSettingsFunctions.Definitions(AmigaControllerType.Cd32Pad)
            .Select(definition => definition.Id).ToArray();

        Assert.Equal(["Up", "Down", "Left", "Right", "B", "A", "Y", "X", "L", "R", "Start", "L2"], actions);
    }

    [Fact]
    public void MouseDefinitionsFollowTheSelectedModel()
    {
        foreach (var model in AmigaModelCatalog.All)
            Assert.Equal(model.MouseButtonCount, AmigaMouseSettingsFunctions.Definitions(model).Count);
    }
}
