namespace GWGUI.Tests.Emulation.MachineAdapters;
public sealed class MachineAdaptersTests
{
    public static IEnumerable<object[]> AtariModels => GWGUI.Emulation.Atari.Dictionaries.AtariModelCatalog.All.Select(model=>new object[]{model.Id});
    [Theory] [MemberData(nameof(AtariModels))] public void EachAtariModuleMapsSettingsWithoutLoadingItsCore(string model)=>MachineConfigurationMappingScenarios.AtariModule(model);
    [Fact] public void EightBitSettingsNormalizeUnsupportedValuesAndDependencies()=>MachineConfigurationMappingScenarios.EightBitNormalization();
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] public Task AmigaCoreFailuresLeaveConsistentStateAndCleanSession(int failure)=>MachineAdapterFailureScenarios.AmigaBoundary(failure);
    [Fact] public void AmigaSettingsMapPathsAudioCropAndStorageWithoutHost()=>MachineConfigurationMappingScenarios.AmigaOptions();
    [Theory]
    [InlineData("gwgui_atari_main_memory","524288","hatari_ramsize","0")]
    [InlineData("gwgui_atari_main_memory","14680064","hatari_ramsize","14")]
    [InlineData("gwgui_atari_main_memory","42","hatari_ramsize",null)]
    [InlineData("gwgui_atari_main_memory","bad","hatari_ramsize",null)]
    [InlineData("gwgui_atari_cpu_frequency","8","hatari_cpu_freq","8")]
    [InlineData("gwgui_atari_cpu_frequency","99","hatari_cpu_freq",null)]
    [InlineData("gwgui_atari_video_crop","enabled","hatari_video_crop_overscan","true")]
    [InlineData("gwgui_atari_video_crop","disabled","hatari_video_crop_overscan","false")]
    [InlineData("gwgui_atari_video_standard","monochrome","hatari_video_hires","true")]
    [InlineData("gwgui_atari_video_standard","NTSC","hatari_forcerefresh","1")]
    [InlineData("gwgui_atari_video_standard","PAL","hatari_forcerefresh","2")]
    [InlineData("gwgui_atari_mouse_speed","50","hatari_emulated_mouse_speed","1")]
    [InlineData("gwgui_atari_mouse_speed","100","hatari_emulated_mouse_speed","2")]
    [InlineData("gwgui_atari_mouse_speed","125","hatari_emulated_mouse_speed","3")]
    [InlineData("gwgui_atari_mouse_speed","150","hatari_emulated_mouse_speed","4")]
    [InlineData("gwgui_atari_mouse_speed","175","hatari_emulated_mouse_speed","5")]
    [InlineData("gwgui_atari_mouse_speed","200","hatari_emulated_mouse_speed","6")]
    [InlineData("gwgui_atari_mouse_speed","bad","hatari_emulated_mouse_speed",null)]
    [InlineData("gwgui_atari_video_frameskip","2","hatari_frameskips","2")]
    public void AtariMappedOptionsRespectUnitsLimitsAndCrop(string source,string value,string target,string? expected)=>MachineConfigurationMappingScenarios.AtariOption(source,value,target,expected);
    [Theory]
    [InlineData("Core","CoreNotFound","Emulator","EmulatorNotInstalled")]
    [InlineData("Core","CoreRejected","Emulator","EmulatorRejected")]
    [InlineData("Firmware","FirmwareMissing","Firmware","FirmwareMissing")]
    [InlineData("Firmware","FirmwareInvalid","Firmware","FirmwareIncompatible")]
    [InlineData("Content","ContentNotFound","Media","MediaNotFound")]
    [InlineData("Content","ContentUnsupported","Media","MediaUnsupported")]
    [InlineData("Option","OptionInvalid","Machine","OptionInvalid")]
    [InlineData("Host","HostProtocolFailure","Emulator","HostCommunicationFailed")]
    [InlineData("State","StateInvalid","SavedState","SavedStateInvalid")]
    [InlineData("State","StateIncompatible","SavedState","SavedStateIncompatible")]
    public Task AdapterErrorsAreTranslatedAndFailedSessionDisposed(string category,string code,string expectedCategory,string expectedCode) => MachineAdapterFailureScenarios.Boundary(category,code,expectedCategory,expectedCode);
    [Fact] public Task FailedRecreationLeavesSessionStoppedAndAllowsRetry() => MachineAdapterFailureScenarios.RecreationFailure();
    [Theory]
    [InlineData("St", "St", "Hatari")] [InlineData("Stf", "St", "Hatari")] [InlineData("Stfm", "St", "Hatari")] [InlineData("MegaSt", "St", "Hatari")]
    [InlineData("Ste", "St", "Hatari")] [InlineData("MegaSte", "St", "Hatari")] [InlineData("Tt", "St", "Hatari")] [InlineData("Falcon", "St", "Hatari")]
    [InlineData("Atari400", "EightBit", "Atari800")] [InlineData("Atari800", "EightBit", "Atari800")] [InlineData("Atari800Xl", "EightBit", "Atari800")]
    [InlineData("Atari130Xe", "EightBit", "Atari800")] [InlineData("Xegs", "EightBit", "Atari800")] [InlineData("XlXe", "EightBit", "Atari800")]
    [InlineData("Atari5200", "Atari5200", "Atari800")] [InlineData("Atari2600", "Atari2600", "Stella")] [InlineData("Atari7800", "Atari7800", "ProSystem")]
    [InlineData("Lynx", "Lynx", "BeetleLynx")] [InlineData("Jaguar", "Jaguar", "VirtualJaguar")] [InlineData("JaguarCd", "Jaguar", "VirtualJaguar")]
    public void AtariModelsChooseExpectedAdapter(string model, string family, string adapter) => MachineCapabilitiesScenarios.Atari(model, family, adapter);
    [Theory]
    [InlineData("A500", "68000", "OCS", 512)] [InlineData("A500PLUS", "68000", "ECS", 1024)] [InlineData("A600", "68000", "ECS", 1024)]
    [InlineData("A1000", "68000", "OCS", 512)] [InlineData("A1200", "68020", "AGA", 2048)] [InlineData("A2000", "68000", "ECS", 1024)]
    [InlineData("A3000", "68030", "ECS", 2048)] [InlineData("A4000", "68040", "AGA", 2048)] [InlineData("CDTV", "68000", "OCS", 1024)] [InlineData("CD32", "68020", "AGA", 2048)]
    public void AmigaModelsExposeImplementedDefaults(string model, string cpu, string chipset, int chipMemory) => MachineCapabilitiesScenarios.Amiga(model, cpu, chipset, chipMemory);
    [Fact] public void IncompatibleConfigurationIsRejected() => MachineCapabilitiesScenarios.Invalid();
    [Fact] public void AdapterOptionsAreMappedWithoutCore() => MachineConfigurationMappingScenarios.Options();
    [Fact] public void MediaCategoriesAreMappedWithoutCore() => MachineConfigurationMappingScenarios.Media();
}
