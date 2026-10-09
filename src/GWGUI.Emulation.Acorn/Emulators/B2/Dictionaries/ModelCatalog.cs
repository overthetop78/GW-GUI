using GWGUI.Emulation.Acorn.Emulators.B2.Constants;
using GWGUI.Emulation.Acorn.Emulators.B2.Contracts;
using GWGUI.Emulation.Acorn.Emulators.B2.Enums;

namespace GWGUI.Emulation.Acorn.Emulators.B2.Dictionaries;

public static class B2ModelCatalog
{
    public static IReadOnlyList<B2ModelDefinition> All { get; } = HardwareModelFunctions.Values(
        new B2ModelDefinition(B2Model.ModelBAcorn1770, MachineModel.BbcMicroModelB, OptionConstants.ModelBAcorn1770),
        new B2ModelDefinition(B2Model.ModelBWatfordDdb2, MachineModel.BbcMicroModelB, OptionConstants.ModelBWatfordDdb2),
        new B2ModelDefinition(B2Model.ModelBWatfordDdb3, MachineModel.BbcMicroModelB, OptionConstants.ModelBWatfordDdb3),
        new B2ModelDefinition(B2Model.ModelBOpus1770, MachineModel.BbcMicroModelB, OptionConstants.ModelBOpus1770),
        new B2ModelDefinition(B2Model.ModelBOpusChallenger256K, MachineModel.BbcMicroModelB, OptionConstants.ModelBOpusChallenger256K),
        new B2ModelDefinition(B2Model.ModelBOpusChallenger512K, MachineModel.BbcMicroModelB, OptionConstants.ModelBOpusChallenger512K),
        new B2ModelDefinition(B2Model.ModelBPlus, MachineModel.BbcMicroModelB, OptionConstants.ModelBPlus),
        new B2ModelDefinition(B2Model.ModelBPlus128, MachineModel.BbcMicroModelB, OptionConstants.ModelBPlus128),
        new B2ModelDefinition(B2Model.Master128Mos320, MachineModel.BbcMaster128, OptionConstants.Master128));
}
