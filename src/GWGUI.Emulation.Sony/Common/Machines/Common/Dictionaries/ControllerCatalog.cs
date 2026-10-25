using CommonControllerFunctions = GWGUI.Emulation.Sony.Common.Functions.ControllerFunctions;
namespace GWGUI.Emulation.Sony.Common.Machines.Common.Dictionaries;

public static class ControllerCatalog
{
    internal static IReadOnlyList<PeripheralCategory> Available(MachineConfiguration configuration, int port, bool usb = false)
    {
        if (configuration.Model is not (ModelConstants.PlayStation or ModelConstants.PlayStation2))
            return CommonControllerFunctions.Supported(configuration, port);
        if (usb) return UsbTypes(configuration, port);
        var native = CommonControllerFunctions.Supported(configuration, port)
            .Where(type => configuration.Model != ModelConstants.PlayStation2
                || type is not (PeripheralCategory.Keyboard or PeripheralCategory.Mouse or PeripheralCategory.KeyboardAndMouse or PeripheralCategory.GunCon2));
        return native.ToArray();
    }

    /// <summary>Consulte la disposition USB annoncée par le contrat de l'adaptateur.</summary>
    internal static IReadOnlyList<int> UsbPorts(MachineConfiguration configuration) => new Engine().SelectedAdapter(configuration)?.GetUsbControllerPorts(configuration) ?? [];

    /// <summary>Résout l'indice relatif au bus ; -1 indique un port extérieur à ce bus.</summary>
    internal static int UsbPortIndex(MachineConfiguration configuration, int port) => UsbPorts(configuration).ToList().IndexOf(port);

    internal static bool IsUsbPort(MachineConfiguration configuration, int port) => UsbPortIndex(configuration, port) >= 0;

    /// <summary>Distingue les ports USB ajoutés après les ports ordinaires de ceux déjà inclus dans leur numérotation.</summary>
    internal static bool IsSyntheticUsbPort(MachineConfiguration configuration, int port) => port >= CommonControllerFunctions.PortCount(configuration) && IsUsbPort(configuration, port);

    internal static bool IsSupported(MachineConfiguration configuration, PeripheralCategory type, int port, bool usb)
    {
        if (!usb) return CommonControllerFunctions.Supported(configuration, port).Contains(type);
        return UsbTypes(configuration, port).Contains(type);
    }

    private static IReadOnlyList<PeripheralCategory> UsbTypes(MachineConfiguration configuration, int port) =>
        (new Engine().Adapter(configuration) as IEmulatorControllerAdapter)?.GetUsbPeripherals(configuration,
            UsbPortIndex(configuration, port) is var index && index >= 0 ? index : port)
            ?? [PeripheralCategory.None];

    internal static IReadOnlyList<string>? Visuals(PeripheralCategory type, bool ps2) => type switch
    {
        PeripheralCategory.ArcadeStick => [EmulationControllerVisualIds.SonyHoriFightingStickPs, EmulationControllerVisualIds.SonyHoriFightingStick2, EmulationControllerVisualIds.SonyHoriRealArcadePro, EmulationControllerVisualIds.SonyNamcoArcadeStick, EmulationControllerVisualIds.SonyAsciiArcadeStick, EmulationControllerVisualIds.SonyAsciiFighterStickV],
        PeripheralCategory.SerialWheel => [EmulationControllerVisualIds.SonyMadCatzWheel, EmulationControllerVisualIds.SonyInteractV3, EmulationControllerVisualIds.SonyFanatecSpeedster2, EmulationControllerVisualIds.SonyFanatecSpeedster3],
        PeripheralCategory.JogCon => [EmulationControllerVisualIds.SonyJogCon],
        PeripheralCategory.Grip => [EmulationControllerVisualIds.SonyAsciiGrip],
        PeripheralCategory.BiohazardPad => [EmulationControllerVisualIds.SonyBiohazardPad],
        PeripheralCategory.SphereController => [EmulationControllerVisualIds.SonyAsciiSphere360],
        PeripheralCategory.Glove => [EmulationControllerVisualIds.SonyRealityQuestGlove],
        PeripheralCategory.VolumeController => [EmulationControllerVisualIds.SonyVolumeController],
        PeripheralCategory.Guitar => ps2 ? [EmulationControllerVisualIds.SonyGuitarHeroSg, EmulationControllerVisualIds.SonyKramerStriker, EmulationControllerVisualIds.SonyGuitarFreaks] : [EmulationControllerVisualIds.SonyGuitarFreaks],
        PeripheralCategory.Beatmania => [EmulationControllerVisualIds.SonyBeatmania],
        PeripheralCategory.BeatmaniaIidx => [EmulationControllerVisualIds.SonyBeatmaniaIidx, EmulationControllerVisualIds.SonyBeatmaniaIidxArcade],
        PeripheralCategory.PopnMusic => ps2 ? [EmulationControllerVisualIds.SonyPopnMusic, EmulationControllerVisualIds.SonyPopnMusicArcade] : [EmulationControllerVisualIds.SonyPopnMusic],
        PeripheralCategory.TaikoDrum => [EmulationControllerVisualIds.SonyTaikoDrum],
        PeripheralCategory.TrainController => [EmulationControllerVisualIds.SonyDenshaController, EmulationControllerVisualIds.SonyDenshaOneHandle, EmulationControllerVisualIds.SonyDenshaMamecon],
        PeripheralCategory.JetController => [EmulationControllerVisualIds.SonyJetController],
        PeripheralCategory.FishingController => [EmulationControllerVisualIds.SonyFishingController],
        PeripheralCategory.KidsStation => [EmulationControllerVisualIds.SonyKidsStation],
        PeripheralCategory.MaestroBaton => [EmulationControllerVisualIds.SonyMaestroBaton],
        PeripheralCategory.Tambourine => [EmulationControllerVisualIds.SonyTambourine],
        PeripheralCategory.DvdRemote => [EmulationControllerVisualIds.SonyDvdRemote],
        PeripheralCategory.Katana => [EmulationControllerVisualIds.SonyOnimushaKatana],
        PeripheralCategory.Chainsaw => [EmulationControllerVisualIds.SonyRe4Chainsaw],
        PeripheralCategory.SerialMouse => [EmulationControllerVisualIds.SonyPlayStationMouse],
        PeripheralCategory.NetPlayController => [EmulationControllerVisualIds.SonyNetPlay],
        PeripheralCategory.EyeToy => [EmulationControllerVisualIds.SonyEyeToy],
        PeripheralCategory.SingStarMicrophones => [EmulationControllerVisualIds.SonySingStar, EmulationControllerVisualIds.SonySingStarWireless],
        PeripheralCategory.BuzzBuzzers => [EmulationControllerVisualIds.SonyBuzz, EmulationControllerVisualIds.SonyBuzzWireless],
        PeripheralCategory.UsbHeadset => [EmulationControllerVisualIds.SonySocomHeadset],
        PeripheralCategory.UsbMicrophone => [EmulationControllerVisualIds.SonyLogitechVantage, EmulationControllerVisualIds.SonyRockBandMicrophone, EmulationControllerVisualIds.SonyGuitarHeroMicrophone],
        PeripheralCategory.Seamic => [EmulationControllerVisualIds.SonySeamic],
        PeripheralCategory.UsbWheel => [EmulationControllerVisualIds.SonyGtForce, EmulationControllerVisualIds.SonyDrivingForce, EmulationControllerVisualIds.SonyDrivingForcePro, EmulationControllerVisualIds.SonyDrivingForceGt],
        PeripheralCategory.UsbFlightStick => [EmulationControllerVisualIds.SonyHoriFlightStick2],
        PeripheralCategory.UsbTrainController => [EmulationControllerVisualIds.SonyDenshaUsb, EmulationControllerVisualIds.SonyDenshaShinkansen, EmulationControllerVisualIds.SonyDenshaRyojohen, EmulationControllerVisualIds.SonyTrainMascon, EmulationControllerVisualIds.SonyMultiTrain],
        PeripheralCategory.KeyboardMania => [EmulationControllerVisualIds.SonyKeyboardMania],
        PeripheralCategory.DrumMania => [EmulationControllerVisualIds.SonyDrumMania],
        PeripheralCategory.ParaParaController => [EmulationControllerVisualIds.SonyParaPara],
        PeripheralCategory.RockBandGuitar => [EmulationControllerVisualIds.SonyRockBandGuitar],
        PeripheralCategory.RockBandDrums => [EmulationControllerVisualIds.SonyRockBandDrums],
        PeripheralCategory.GuitarHeroDrums => [EmulationControllerVisualIds.SonyGuitarHeroDrums],
        PeripheralCategory.DjHero => [EmulationControllerVisualIds.SonyDjHero],
        PeripheralCategory.GameTrak => [EmulationControllerVisualIds.SonyGameTrak],
        _ => null
    };

    public static IReadOnlyList<PeripheralCategory> Types(MachineConfiguration configuration) => CommonControllerFunctions.Supported(configuration);

    public static PeripheralCategory Default(MachineConfiguration configuration) => Types(configuration).FirstOrDefault(type => type != PeripheralCategory.None, PeripheralCategory.None);

}
