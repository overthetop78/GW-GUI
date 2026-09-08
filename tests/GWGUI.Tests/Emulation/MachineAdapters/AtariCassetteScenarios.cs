using GWGUI.Emulation.Atari.Constants;
using GWGUI.Emulation.Atari.Contracts;
using GWGUI.Emulation.Atari.Enums;
using GWGUI.Emulation.Atari.Functions;
using GWGUI.Emulation.Atari.Services;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests.Emulation.MachineAdapters;

internal static class AtariCassetteScenarios
{
    internal static void PlaybackSendsOneReturnPulse()
    {
        Assert.Equal([EmulationCassetteCommand.Play],
            AtariCassetteInputController.AvailableCommands);
        Assert.Equal(EmulationCassetteState.Empty,
            AtariCassetteStateFunctions.From(mounted: false, motorActive: false));
        Assert.Equal(EmulationCassetteState.Stopped,
            AtariCassetteStateFunctions.From(mounted: true, motorActive: false));
        Assert.Equal(EmulationCassetteState.Playing,
            AtariCassetteStateFunctions.From(mounted: true, motorActive: true));

        var controller = new AtariCassetteInputController(
            Configuration(AtariMachineModel.Atari800, Cassette(autoBoot: false), cassetteBoot: false));
        controller.SetPhysicalInput(EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.A }
        });

        controller.Play();
        var pulse = controller.NextFrame();
        Assert.Equal(2, pulse.Keys.Count);
        Assert.Contains(EmulationKey.A, pulse.Keys);
        Assert.Contains(EmulationKey.Return, pulse.Keys);
        Assert.Equal([EmulationKey.A], controller.NextFrame().Keys);
    }

    internal static void AutomaticBootUsesTheCoreOptionForEveryComputerModel()
    {
        foreach (var model in new[]
                 {
                     AtariMachineModel.Atari400, AtariMachineModel.Atari800,
                     AtariMachineModel.Atari800Xl, AtariMachineModel.Atari130Xe,
                     AtariMachineModel.Xegs, AtariMachineModel.XlXe
                 })
        {
            var media = Cassette(autoBoot: true);
            var configuration = Configuration(model, media, cassetteBoot: false);
            var prepared = new Atari800PreparedMedia(media, Atari800ContentType.Cassette,
                "virtual.cas", null);

            Assert.Equal(AtariEightBitSettingsConstants.Enabled,
                Atari800MediaFunctions.ApplyOptions(configuration, prepared)
                    [AtariEightBitSettingsConstants.CassetteBootOptionKey]);
            Assert.Equal(AtariEightBitSettingsConstants.Enabled,
                Atari800MediaFunctions.ApplyOptions(configuration with
                    {
                        Options = new Dictionary<string, string>(configuration.Options)
                        {
                            [AtariEightBitSettingsConstants.SioAccelerationOptionKey] =
                                AtariEightBitSettingsConstants.Disabled
                        }
                    }, prepared)[AtariEightBitSettingsConstants.SioAccelerationOptionKey]);
        }
    }

    internal static void AutomaticBootSendsOneDelayedReturnOnlyForOriginalModels()
    {
        foreach (var model in new[] { AtariMachineModel.Atari400, AtariMachineModel.Atari800 })
        {
            var controller = new AtariCassetteInputController(
                Configuration(model, Cassette(autoBoot: true), cassetteBoot: false));
            controller.SetPhysicalInput(EmulationInputSnapshot.Empty with
            {
                Keys = new HashSet<EmulationKey> { EmulationKey.A }
            });

            for (var frame = 0; frame < AtariCassetteInputController.AutomaticReturnDelayFrames; frame++)
                Assert.Equal([EmulationKey.A], controller.NextFrame().Keys);

            var pulse = controller.NextFrame();
            Assert.Equal(2, pulse.Keys.Count);
            Assert.Contains(EmulationKey.A, pulse.Keys);
            Assert.Contains(EmulationKey.Return, pulse.Keys);
            Assert.Equal([EmulationKey.A], controller.NextFrame().Keys);
        }

        foreach (var model in new[]
                 {
                     AtariMachineModel.Atari800Xl, AtariMachineModel.Atari130Xe,
                     AtariMachineModel.Xegs, AtariMachineModel.XlXe
                 })
        {
            var controller = new AtariCassetteInputController(
                Configuration(model, Cassette(autoBoot: true), cassetteBoot: false));
            for (var frame = 0; frame <= AtariCassetteInputController.AutomaticReturnDelayFrames; frame++)
                Assert.DoesNotContain(EmulationKey.Return, controller.NextFrame().Keys);
        }
    }

    private static AtariMachineConfiguration Configuration(AtariMachineModel model,
        AtariMediaConfiguration media, bool cassetteBoot) => new(model, media: [media],
        options: new Dictionary<string, string>
        {
            [AtariEightBitSettingsConstants.CassetteBootOptionKey] = cassetteBoot
                ? AtariEightBitSettingsConstants.Enabled
                : AtariEightBitSettingsConstants.Disabled
        });

    private static AtariMediaConfiguration Cassette(bool autoBoot) => new(
        "virtual.cas", AtariMediaCategory.Cassette, EmulationMediaSlot.Cassette0,
        CassetteAutoBoot: autoBoot);
}
