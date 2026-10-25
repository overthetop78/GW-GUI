using System.Collections.ObjectModel;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Enums;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Constants;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Dictionaries;

internal static class ModelDictionary
{
    internal static readonly IReadOnlyDictionary<MachineModel, string> NativeValues = new ReadOnlyDictionary<MachineModel, string>(new Dictionary<MachineModel, string>
    {
        [MachineModel.Mo5] = RuntimeOptionConstants.Mo5,
        [MachineModel.Mo6] = RuntimeOptionConstants.Mo6,
        [MachineModel.To7] = RuntimeOptionConstants.To7,
        [MachineModel.To770] = RuntimeOptionConstants.To770,
        [MachineModel.To8] = RuntimeOptionConstants.To8,
        [MachineModel.To8D] = RuntimeOptionConstants.To8D,
        [MachineModel.To9] = RuntimeOptionConstants.To9,
        [MachineModel.To9Plus] = RuntimeOptionConstants.To9Plus
    });
}
