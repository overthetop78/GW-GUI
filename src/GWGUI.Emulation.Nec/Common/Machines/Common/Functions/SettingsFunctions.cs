using GWGUI.Emulation.Nec.Emulators.BeetlePce.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetleSgx.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetleSgx.Functions;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Functions;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Constants;
using GWGUI.Emulation.Nec.Common.Machines.PcEngineDuo.Functions;

namespace GWGUI.Emulation.Nec.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var model = ModelCatalog.Get(configuration.Model);
        var options = configuration.Options ?? new Dictionary<string, string>();
        var pcfx = configuration.EmulatorId == BeetlePcfxConstants.Id;
        var sgx = configuration.EmulatorId == BeetleSgxConstants.Id;
        var geargrafx = configuration.EmulatorId == GeargrafxConstants.Id;
        var cd = model.Id == PcEngineDuoMachineConstants.Id;
        var cdCapable = model.SupportsCdDrive && model.SupportsCartridgeSlot;
        var ram = model.RamKib.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var ramDisplay = model.RamKib >= SettingsDescriptionFunctionsConstants.KibPerMib
            ? $"{model.RamKib / SettingsDescriptionFunctionsConstants.KibPerMib}{SettingsDescriptionFunctionsConstants.MemoryUnitMib}"
            : $"{ram}{SettingsDescriptionFunctionsConstants.MemoryUnitKib}";
        var video = pcfx
            ? BeetlePcfxSettingsDescriptionFunctions.Video(options)
            : sgx ? BeetleSgxSettingsDescriptionFunctions.Video(options)
                : geargrafx ? GeargrafxSettingsDescriptionFunctions.Video(model.Id, options)
                : BeetlePceSettingsDescriptionFunctions.Video(options);
        var audio = new List<EmulationSettingsField>
        {
            Toggle(SettingsConstants.AudioEnabled, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.Audio,
                SettingsDescriptionFunctionsConstants.ResourceAudioEnabled, configuration.AudioEnabled),
            AudioOutput(configuration.Audio?.OutputDeviceId),
            Select(SettingsConstants.AudioLatency, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.Audio,
                SettingsDescriptionFunctionsConstants.ResourceAudioLatency,
                (configuration.Audio?.LatencyMilliseconds
                    ?? SettingsDescriptionFunctionsConstants.ThirdAudioLatency).ToString(),
                SettingsDescriptionFunctionsConstants.AudioLatencies
                    .Select(value => Invariant(value.ToString(),
                        $"{value}{SettingsDescriptionFunctionsConstants.MillisecondUnit}", value)))
        };
        if (pcfx) BeetlePcfxSettingsDescriptionFunctions.AddAudio(audio, options);
        else if (geargrafx) GeargrafxSettingsDescriptionFunctions.AddAudio(audio, options);
        else if (cdCapable && (model.HasBuiltInCdDrive
            || bool.TryParse(options.GetValueOrDefault(
                BeetlePceStorageConstants.CdDriveEnabledOption), out var cdEnabled)
                && cdEnabled))
        {
            if (sgx) BeetleSgxSettingsDescriptionFunctions.AddCdAudio(audio, options);
            else BeetlePceSettingsDescriptionFunctions.AddCdAudio(audio, options);
        }
        var blocks = new List<EmulationSettingsBlock>
        {
            Block(SettingsDescriptionFunctionsConstants.General, EmulationMachineTab.General,
                SettingsDescriptionFunctionsConstants.ResourceGeneral,
                SettingsDescriptionFunctionsConstants.IconGeneral,
                SettingsDescriptionFunctionsConstants.TwoColumns,
                Information(SettingsConstants.Model, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.General,
                    SettingsDescriptionFunctionsConstants.ResourceMachineModel, model.DisplayName),
                Information(SettingsConstants.Emulator, EmulationMachineTab.General,
                    SettingsDescriptionFunctionsConstants.General,
                    SettingsDescriptionFunctionsConstants.ResourceEmulator,
                    EmulatorCatalog.GetAll(model.Id).FirstOrDefault(item => item.Id == configuration.EmulatorId)
                        ?.DisplayName ?? configuration.EmulatorId)),
            Block(SettingsDescriptionFunctionsConstants.Cpu, EmulationMachineTab.Cpu,
                SettingsDescriptionFunctionsConstants.ResourceCpuProcessor,
                SettingsDescriptionFunctionsConstants.IconCpu,
                SettingsDescriptionFunctionsConstants.TwoColumns,
                Information(SettingsConstants.Model + SettingsDescriptionFunctionsConstants.ModelCpuSuffix,
                    EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu,
                    SettingsDescriptionFunctionsConstants.ResourceCpuModel, model.CpuName),
                Information(SettingsConstants.Model
                    + SettingsDescriptionFunctionsConstants.ModelFrequencySuffix,
                    EmulationMachineTab.Cpu, SettingsDescriptionFunctionsConstants.Cpu,
                    SettingsDescriptionFunctionsConstants.ResourceCpuSpeed, model.CpuClock)),
            Block(SettingsDescriptionFunctionsConstants.Memory, EmulationMachineTab.Ram,
                SettingsDescriptionFunctionsConstants.ResourceMemoryMain,
                SettingsDescriptionFunctionsConstants.IconMemory,
                SettingsDescriptionFunctionsConstants.OneColumn,
                Select(SettingsConstants.Ram, EmulationMachineTab.Ram,
                    SettingsDescriptionFunctionsConstants.Memory,
                    SettingsDescriptionFunctionsConstants.ResourceMemoryMain, ram,
                    [Invariant(ram, ramDisplay, model.RamKib)], isEnabled: false)),
            Block(SettingsDescriptionFunctionsConstants.Firmware, EmulationMachineTab.Rom,
                SettingsDescriptionFunctionsConstants.ResourceRom,
                SettingsDescriptionFunctionsConstants.IconFirmware,
                SettingsDescriptionFunctionsConstants.TwoColumns,
                model.SupportsCdDrive
                    ? FirmwarePath(configuration.FirmwarePath)
                    : Information(SettingsConstants.FirmwareIntegrated, EmulationMachineTab.Rom,
                        SettingsDescriptionFunctionsConstants.Firmware,
                        SettingsDescriptionFunctionsConstants.ResourceFirmwareNotRequired,
                        SettingsDescriptionFunctionsConstants.CpuFrequency)),
            Block(SettingsDescriptionFunctionsConstants.Video, EmulationMachineTab.Video,
                SettingsDescriptionFunctionsConstants.ResourceVideo,
                SettingsDescriptionFunctionsConstants.IconVideo,
                SettingsDescriptionFunctionsConstants.TwoColumns, video),
            Block(SettingsDescriptionFunctionsConstants.Audio, EmulationMachineTab.Audio,
                SettingsDescriptionFunctionsConstants.ResourceAudio,
                SettingsDescriptionFunctionsConstants.IconAudio,
                SettingsDescriptionFunctionsConstants.TwoColumns, audio.ToArray())
        };
        if (geargrafx)
        {
            var cpu = blocks.First(block => block.Tab == EmulationMachineTab.Cpu);
            var index = blocks.IndexOf(cpu);
            blocks[index] = cpu with { Fields = cpu.Fields.Concat(
                [GeargrafxSettingsDescriptionFunctions.CpuRegion(model.Id, options)]).ToArray() };
        }
        if (cd)
            blocks.Insert(PcEngineDuoMachineConstants.CdMemoryBlockPosition,
                PcEngineDuoSettingsFunctions.CdRamBlock());
        if (model.MouseButtonCount > SettingsDescriptionFunctionsConstants.FirstChoice)
            blocks.Add(Block(SettingsDescriptionFunctionsConstants.Mouse, EmulationMachineTab.Mouse,
                SettingsDescriptionFunctionsConstants.ResourceMouseTab,
                SettingsDescriptionFunctionsConstants.MouseIcon,
                SettingsDescriptionFunctionsConstants.TwoColumns,
                pcfx ? BeetlePcfxSettingsDescriptionFunctions.Mouse(options)
                    : sgx ? BeetleSgxSettingsDescriptionFunctions.Mouse(options)
                        : geargrafx ? GeargrafxSettingsDescriptionFunctions.Mouse(options)
                        : BeetlePceSettingsDescriptionFunctions.Mouse(options)));
        return blocks;
    }
}
