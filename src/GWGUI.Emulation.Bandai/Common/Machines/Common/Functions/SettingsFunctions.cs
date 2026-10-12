using System.Globalization;
using WS = GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Constants.OptionConstants;
using PD = GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Constants.OptionConstants;
using GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Dictionaries;
using GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Dictionaries;

namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;
internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock>
        {
            Audio(configuration)
        };
        AddHardware(blocks, configuration);
        if (configuration.Family == MachineFamily.WonderSwan)
        {
            AddNative(
                blocks,
                configuration,
                BeetleWonderSwanOptionCatalog.All.Where(option => option.Key is not (WS.OptionLanguage or WS.OptionNameSource or WS.OptionBirthYear or WS.OptionBirthMonth or WS.OptionBirthDay or WS.OptionSex or WS.OptionBlood))
            );
        }
        else if (configuration.Family == MachineFamily.Playdia)
        {
            AddNative(blocks, configuration, PlaydiaEmuOptionCatalog.All.Where(option => option.Key == PD.OptionSwapAb));
        }
        else
            blocks.Add(Block(
                    EmulationMachineTab.Rom,
                    SettingsConstants.ResourceFirmware,
                    new EmulationSettingsField(
                        SettingsConstants.Firmware,
                        EmulationMachineTab.Rom,
                        SettingsConstants.ResourceFirmware,
                        SettingsConstants.ResourceFirmware,
                        EmulationSettingsEditor.Path,
                        configuration.Firmwares.SingleOrDefault()?.Path,
                        RequiresRestart: true
                    )
                ));
        return blocks;
    }

    private static void AddHardware(
        List<EmulationSettingsBlock> blocks,
        MachineConfiguration configuration
    )
    {
        var hardware = HardwareModelCatalog.Get(configuration.Model);
        if (hardware is null)
        {
            blocks.Add(Block(
                    EmulationMachineTab.General,
                    SettingsConstants.ResourceEmulator,
                    Information(SettingsConstants.PlaydiaHle, EmulationMachineTab.General, SettingsConstants.ResourceEmulator, SettingsConstants.ResourcePlaydiaHle, string.Empty)
                ));
            return;
        }

        blocks.Add(Block(
                EmulationMachineTab.Cpu,
                SettingsConstants.ResourceCpuProcessor,
                new(
                    SettingsConstants.CpuModel,
                    EmulationMachineTab.Cpu,
                    SettingsConstants.ResourceCpuProcessor,
                    SettingsConstants.ResourceCpuModel,
                    EmulationSettingsEditor.Information,
                    hardware.CpuResourceKey,
                    [new(hardware.CpuResourceKey, hardware.CpuResourceKey)]
                ),
                Information(
                    SettingsConstants.CpuFrequency,
                    EmulationMachineTab.Cpu,
                    SettingsConstants.ResourceCpuProcessor,
                    SettingsConstants.ResourceCpuSpeedOriginal,
                    hardware.CpuFrequencyHz >= SettingsConstants.HertzPerMegahertz
                        ? $"{(hardware.CpuFrequencyHz / SettingsConstants.HertzPerMegahertz).ToString("0.######", CultureInfo.CurrentCulture)} {SettingsConstants.MegahertzUnit}"
                        : $"{(hardware.CpuFrequencyHz / SettingsConstants.HertzPerKilohertz).ToString("0.###", CultureInfo.CurrentCulture)} {SettingsConstants.KilohertzUnit}"
                )
            ));
        var memoryFields = new List<EmulationSettingsField>
        {
            Information(
                SettingsConstants.MainMemory,
                EmulationMachineTab.Ram,
                SettingsConstants.ResourceMemoryMain,
                SettingsConstants.ResourceMemoryMain,
                Memory(hardware.MainMemoryBytes)
            )
        };
        if (hardware.DisplayMemoryBytes > 0)
            memoryFields.Add(Information(SettingsConstants.DisplayMemory, EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, SettingsConstants.ResourceDisplayMemory, Memory(hardware.DisplayMemoryBytes)));
        blocks.Add(Block(EmulationMachineTab.Ram, SettingsConstants.ResourceMemoryMain, memoryFields.ToArray()));
        blocks.Add(Block(
                EmulationMachineTab.Video,
                SettingsConstants.ResourceVideoResolution,
                Information(SettingsConstants.DisplayResolution, EmulationMachineTab.Video, SettingsConstants.ResourceVideoResolution, SettingsConstants.ResourceVideoResolution, $"{hardware.DisplayWidth} × {hardware.DisplayHeight}")
            ));
    }

    private static string Memory(long bytes) => bytes >= SettingsConstants.BytesPerKibibyte 
        ? $"{(bytes / SettingsConstants.BytesPerKibibyte).ToString(CultureInfo.InvariantCulture)} {SettingsConstants.KibibyteUnit}" 
        : $"{bytes.ToString(CultureInfo.InvariantCulture)} {SettingsConstants.ByteUnit}";
    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value);
    private static void AddNative(
        List<EmulationSettingsBlock> blocks,
        MachineConfiguration configuration,
        IEnumerable<CoreOption> options
    )
    {
        foreach (var group in options.GroupBy(option => option.Key switch
        {
            WS.OptionRotateDisplay or WS.OptionMonoPalette or WS.OptionGfxColors or WS.OptionFrameskip or WS.OptionFrameskipThreshold or WS.Option60hzMode => EmulationMachineTab.Video,
            WS.OptionSoundSampleRate or WS.OptionSoundLowPass => EmulationMachineTab.Audio,
            WS.OptionRotateKeymap or PD.OptionSwapAb => EmulationMachineTab.Controllers,
            _ => EmulationMachineTab.General
        }))
        {
            var resource = group.Key switch
            {
                EmulationMachineTab.Video => SettingsConstants.ResourceNativeVideo,
                EmulationMachineTab.Audio => SettingsConstants.ResourceNativeAudio,
                EmulationMachineTab.Controllers => SettingsConstants.ResourceNativeControls,
                _ => SettingsConstants.ResourceOwner
            };
            blocks.Add(Block(group.Key, resource, group.Select(option => Native(configuration, option, group.Key, resource)).ToArray())with { IsCompact = group.Key == EmulationMachineTab.Audio });
        }
    }

    private static EmulationSettingsField Native(
        MachineConfiguration configuration,
        CoreOption option,
        EmulationMachineTab tab,
        string blockId
    )
    {
        var resource = SettingsConstants.NativeOptionResourcePrefix + option.Key;
        var choices = option.Values.Select((choice, index) => new EmulationSettingsChoice(
                choice.Value,
                string.Format(CultureInfo.InvariantCulture, SettingsConstants.NativeChoiceResourceFormat, resource, index),
                option.Key == WS.OptionGfxColors
                    ? choice.Value switch
                    {
                        "16bit" => "16 bits",
                        "24bit" => "24 bits",
                        _ => choice.Value
                    }
                    : option.Key == WS.OptionSoundSampleRate && double.TryParse(choice.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var hertz)
                    ? $"{(hertz / SettingsConstants.HertzPerKilohertz).ToString("0.###", CultureInfo.CurrentCulture)} {SettingsConstants.KilohertzUnit}"
                    : option.Key == WS.OptionMonoPalette && index > 0 ? choice.Label : null
            )).ToArray();
        return new(
            option.Key,
            tab,
            blockId,
            resource,
            EmulationSettingsEditor.Selection,
            configuration.Options.GetValueOrDefault(option.Key, option.DefaultValue),
            choices,
            ExplanationResourceKey: resource + SettingsConstants.HelpResourceSuffix,
            RequiresRestart: RuntimeOptionFunctions.RequiresRestart(configuration.Core, option.Key)
        );
    }

    private static EmulationSettingsBlock Audio(MachineConfiguration configuration) => Block(
        EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
        new(
            SettingsConstants.AudioEnabled,
            EmulationMachineTab.Audio,
            SettingsConstants.ResourceAudio,
            SettingsConstants.ResourceAudioEnabled,
            EmulationSettingsEditor.Toggle,
            configuration.AudioEnabled ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled
        ),
        new(
            SettingsConstants.AudioOutput,
            EmulationMachineTab.Audio,
            SettingsConstants.ResourceAudio,
            SettingsConstants.ResourceAudioOutput,
            EmulationSettingsEditor.Selection,
            configuration.Options.GetValueOrDefault(SettingsConstants.AudioOutput, SettingsConstants.DefaultOutput),
            [new(SettingsConstants.DefaultOutput, SettingsConstants.ResourceDefaultOutput)],
            ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices
        ),
        AudioSelection(configuration, SettingsConstants.AudioLatency, SettingsConstants.ResourceAudioLatency, SettingsConstants.DefaultLatency, SettingsConstants.Latencies, SettingsConstants.MillisecondsUnit),
        AudioSelection(configuration, VideoAudioSettingsConstants.AudioVolumeOption, SettingsConstants.ResourceAudioVolume, SettingsConstants.DefaultVolume, SettingsConstants.Volumes, SettingsConstants.PercentUnit)
    );
    private static EmulationSettingsField AudioSelection(MachineConfiguration configuration, string id, string resource, int fallback, IReadOnlyList<int> values, string unit) => new(
        id,
        EmulationMachineTab.Audio,
        SettingsConstants.ResourceAudio,
        resource,
        EmulationSettingsEditor.Selection,
        configuration.Options.GetValueOrDefault(id, fallback.ToString(CultureInfo.InvariantCulture)),
        values.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, $"{value.ToString(CultureInfo.InvariantCulture)} {unit}")).ToArray()
    );
    private static EmulationSettingsBlock Block(EmulationMachineTab tab, string resource, params EmulationSettingsField[] fields) => new(resource, tab, resource, fields);
}