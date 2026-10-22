using CommonControllerFunctions = GWGUI.Emulation.Sony.Common.Functions.ControllerFunctions;
namespace GWGUI.Emulation.Sony.Common.Machines.Common.Dictionaries;

public static class ControllerCatalog
{
    internal static IReadOnlyList<ControllerType> Available(MachineConfiguration configuration, int port, bool usb = false)
    {
        if (configuration.Model is not (ModelConstants.PlayStation or ModelConstants.PlayStation2))
            return CommonControllerFunctions.Supported(configuration, port);
        if (usb) return UsbTypes(configuration, port);
        var native = CommonControllerFunctions.Supported(configuration, port)
            .Where(type => configuration.Model != ModelConstants.PlayStation2
                || type is not (ControllerType.Keyboard or ControllerType.Mouse or ControllerType.KeyboardAndMouse or ControllerType.GunCon2));
        return native.ToArray();
    }

    internal static bool IsUsbPort(MachineConfiguration configuration, int port) =>
        configuration.Model == ModelConstants.PlayStation2
        && (configuration.EmulatorId == "pcee2" ? port < 2 : port is 8 or 9);

    internal static bool IsSupported(MachineConfiguration configuration, ControllerType type, int port, bool usb)
    {
        if (!usb) return CommonControllerFunctions.Supported(configuration, port).Contains(type);
        return UsbTypes(configuration, port).Contains(type);
    }

    private static IReadOnlyList<ControllerType> UsbTypes(MachineConfiguration configuration, int port) =>
        (new Engine().Adapter(configuration) as IEmulatorControllerAdapter)?.GetUsbControllerTypes(configuration,
            configuration.EmulatorId == "pcee2" ? port : port is 8 or 9 ? port - 8 : port)
            ?? [ControllerType.None];

    internal static IReadOnlyList<string>? Visuals(ControllerType type, bool ps2) => type switch
    {
        ControllerType.ArcadeStick => [EmulationControllerVisualIds.SonyHoriFightingStickPs, EmulationControllerVisualIds.SonyHoriFightingStick2, EmulationControllerVisualIds.SonyHoriRealArcadePro, EmulationControllerVisualIds.SonyNamcoArcadeStick, EmulationControllerVisualIds.SonyAsciiArcadeStick, EmulationControllerVisualIds.SonyAsciiFighterStickV],
        ControllerType.SerialWheel => [EmulationControllerVisualIds.SonyMadCatzWheel, EmulationControllerVisualIds.SonyInteractV3, EmulationControllerVisualIds.SonyFanatecSpeedster2, EmulationControllerVisualIds.SonyFanatecSpeedster3],
        ControllerType.JogCon => [EmulationControllerVisualIds.SonyJogCon],
        ControllerType.Grip => [EmulationControllerVisualIds.SonyAsciiGrip],
        ControllerType.BiohazardPad => [EmulationControllerVisualIds.SonyBiohazardPad],
        ControllerType.SphereController => [EmulationControllerVisualIds.SonyAsciiSphere360],
        ControllerType.Glove => [EmulationControllerVisualIds.SonyRealityQuestGlove],
        ControllerType.VolumeController => [EmulationControllerVisualIds.SonyVolumeController],
        ControllerType.Guitar => ps2 ? [EmulationControllerVisualIds.SonyGuitarHeroSg, EmulationControllerVisualIds.SonyKramerStriker, EmulationControllerVisualIds.SonyGuitarFreaks] : [EmulationControllerVisualIds.SonyGuitarFreaks],
        ControllerType.Beatmania => [EmulationControllerVisualIds.SonyBeatmania],
        ControllerType.BeatmaniaIidx => [EmulationControllerVisualIds.SonyBeatmaniaIidx, EmulationControllerVisualIds.SonyBeatmaniaIidxArcade],
        ControllerType.PopnMusic => ps2 ? [EmulationControllerVisualIds.SonyPopnMusic, EmulationControllerVisualIds.SonyPopnMusicArcade] : [EmulationControllerVisualIds.SonyPopnMusic],
        ControllerType.TaikoDrum => [EmulationControllerVisualIds.SonyTaikoDrum],
        ControllerType.TrainController => [EmulationControllerVisualIds.SonyDenshaController, EmulationControllerVisualIds.SonyDenshaOneHandle, EmulationControllerVisualIds.SonyDenshaMamecon],
        ControllerType.JetController => [EmulationControllerVisualIds.SonyJetController],
        ControllerType.FishingController => [EmulationControllerVisualIds.SonyFishingController],
        ControllerType.KidsStation => [EmulationControllerVisualIds.SonyKidsStation],
        ControllerType.MaestroBaton => [EmulationControllerVisualIds.SonyMaestroBaton],
        ControllerType.Tambourine => [EmulationControllerVisualIds.SonyTambourine],
        ControllerType.DvdRemote => [EmulationControllerVisualIds.SonyDvdRemote],
        ControllerType.Katana => [EmulationControllerVisualIds.SonyOnimushaKatana],
        ControllerType.Chainsaw => [EmulationControllerVisualIds.SonyRe4Chainsaw],
        ControllerType.SerialMouse => [EmulationControllerVisualIds.SonyPlayStationMouse],
        ControllerType.NetPlayController => [EmulationControllerVisualIds.SonyNetPlay],
        ControllerType.EyeToy => [EmulationControllerVisualIds.SonyEyeToy],
        ControllerType.SingStarMicrophones => [EmulationControllerVisualIds.SonySingStar, EmulationControllerVisualIds.SonySingStarWireless],
        ControllerType.BuzzBuzzers => [EmulationControllerVisualIds.SonyBuzz, EmulationControllerVisualIds.SonyBuzzWireless],
        ControllerType.UsbHeadset => [EmulationControllerVisualIds.SonySocomHeadset],
        ControllerType.UsbMicrophone => [EmulationControllerVisualIds.SonyLogitechVantage, EmulationControllerVisualIds.SonyRockBandMicrophone, EmulationControllerVisualIds.SonyGuitarHeroMicrophone],
        ControllerType.Seamic => [EmulationControllerVisualIds.SonySeamic],
        ControllerType.UsbWheel => [EmulationControllerVisualIds.SonyGtForce, EmulationControllerVisualIds.SonyDrivingForce, EmulationControllerVisualIds.SonyDrivingForcePro, EmulationControllerVisualIds.SonyDrivingForceGt],
        ControllerType.UsbFlightStick => [EmulationControllerVisualIds.SonyHoriFlightStick2],
        ControllerType.UsbTrainController => [EmulationControllerVisualIds.SonyDenshaUsb, EmulationControllerVisualIds.SonyDenshaShinkansen, EmulationControllerVisualIds.SonyDenshaRyojohen, EmulationControllerVisualIds.SonyTrainMascon, EmulationControllerVisualIds.SonyMultiTrain],
        ControllerType.KeyboardMania => [EmulationControllerVisualIds.SonyKeyboardMania],
        ControllerType.DrumMania => [EmulationControllerVisualIds.SonyDrumMania],
        ControllerType.ParaParaController => [EmulationControllerVisualIds.SonyParaPara],
        ControllerType.RockBandGuitar => [EmulationControllerVisualIds.SonyRockBandGuitar],
        ControllerType.RockBandDrums => [EmulationControllerVisualIds.SonyRockBandDrums],
        ControllerType.GuitarHeroDrums => [EmulationControllerVisualIds.SonyGuitarHeroDrums],
        ControllerType.DjHero => [EmulationControllerVisualIds.SonyDjHero],
        ControllerType.GameTrak => [EmulationControllerVisualIds.SonyGameTrak],
        _ => null
    };

    public static IReadOnlyList<ControllerType> Types(MachineConfiguration configuration) => CommonControllerFunctions.Supported(configuration);

    public static ControllerType Default(MachineConfiguration configuration) => Types(configuration).FirstOrDefault(type => type != ControllerType.None, ControllerType.None);

}
