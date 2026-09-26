using GWGUI.Emulation.Atari.Common.Constants;
using GWGUI.Emulation.Atari.Common.Machines.Common.Constants;
using GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Atari.Common.Machines.Common.Enums;
using GWGUI.Emulation.Atari.Common.Machines.Common.Functions;
using GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Constants;
using GWGUI.Emulation.Atari.Common.Services;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests.Emulation.MachineAdapters;

internal static class AtariCassetteScenarios
{
    internal static void PlaybackSendsOneReturnPulse()
    {
        Assert.Equal([EmulationCassetteCommand.Play],
            CassetteInputController.AvailableCommands);
        Assert.Equal(EmulationCassetteState.Empty,
            CassetteStateFunctions.From(mounted: false, motorActive: false));
        Assert.Equal(EmulationCassetteState.Stopped,
            CassetteStateFunctions.From(mounted: true, motorActive: false));
        Assert.Equal(EmulationCassetteState.Playing,
            CassetteStateFunctions.From(mounted: true, motorActive: true));

        var controller = new CassetteInputController(
            Configuration(MachineModel.Atari800, Cassette(autoBoot: false), cassetteBoot: false));
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
                     MachineModel.Atari400, MachineModel.Atari800,
                     MachineModel.Atari800Xl, MachineModel.Atari130Xe,
                     MachineModel.Xegs, MachineModel.XlXe
                 })
        {
            var media = Cassette(autoBoot: true);
            var configuration = Configuration(model, media, cassetteBoot: false);
            var prepared = new Atari800PreparedMedia(media, Atari800ContentType.Cassette,
                "virtual.cas", null);

            Assert.Equal(EightBitSettingsConstants.Enabled,
                Atari800MediaFunctions.ApplyOptions(configuration, prepared)
                    [EightBitSettingsConstants.CassetteBootOptionKey]);
            Assert.Equal(EightBitSettingsConstants.Enabled,
                Atari800MediaFunctions.ApplyOptions(configuration with
                    {
                        Options = new Dictionary<string, string>(configuration.Options)
                        {
                            [EightBitSettingsConstants.SioAccelerationOptionKey] =
                                EightBitSettingsConstants.Disabled
                        }
                    }, prepared)[EightBitSettingsConstants.SioAccelerationOptionKey]);
        }
    }

    internal static void AutomaticBootSendsOneDelayedReturnOnlyForOriginalModels()
    {
        foreach (var model in new[] { MachineModel.Atari400, MachineModel.Atari800 })
        {
            var controller = new CassetteInputController(
                Configuration(model, Cassette(autoBoot: true), cassetteBoot: false));
            controller.SetPhysicalInput(EmulationInputSnapshot.Empty with
            {
                Keys = new HashSet<EmulationKey> { EmulationKey.A }
            });

            for (var frame = 0; frame < CassetteInputController.AutomaticReturnDelayFrames; frame++)
                Assert.Equal([EmulationKey.A], controller.NextFrame().Keys);

            var pulse = controller.NextFrame();
            Assert.Equal(2, pulse.Keys.Count);
            Assert.Contains(EmulationKey.A, pulse.Keys);
            Assert.Contains(EmulationKey.Return, pulse.Keys);
            Assert.Equal([EmulationKey.A], controller.NextFrame().Keys);
        }

        foreach (var model in new[]
                 {
                     MachineModel.Atari800Xl, MachineModel.Atari130Xe,
                     MachineModel.Xegs, MachineModel.XlXe
                 })
        {
            var controller = new CassetteInputController(
                Configuration(model, Cassette(autoBoot: true), cassetteBoot: false));
            for (var frame = 0; frame <= CassetteInputController.AutomaticReturnDelayFrames; frame++)
                Assert.DoesNotContain(EmulationKey.Return, controller.NextFrame().Keys);
        }
    }

    private static MachineConfiguration Configuration(MachineModel model,
        MediaConfiguration media, bool cassetteBoot) => new(model, media: [media],
        options: new Dictionary<string, string>
        {
            [EightBitSettingsConstants.CassetteBootOptionKey] = cassetteBoot
                ? EightBitSettingsConstants.Enabled
                : EightBitSettingsConstants.Disabled
        });

    private static MediaConfiguration Cassette(bool autoBoot) => new(
        "virtual.cas", MediaCategory.Cassette, EmulationMediaSlot.Cassette0,
        CassetteAutoBoot: autoBoot);
}
