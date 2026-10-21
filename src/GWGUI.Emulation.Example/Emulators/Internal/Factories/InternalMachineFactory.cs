using GWGUI.Emulation.Example.Emulators.Internal.Constants;
using GWGUI.Emulation.Example.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Example.Emulators.Internal.Factories;

/// <summary>Adaptateur du cœur ExampleInternal sous les mêmes interfaces que les deux autres émulateurs fictifs.</summary>
/// <remarks>Le catalogue découvre cette fabrique par son interface. Les deux profils sont déclarés par
/// MachineFactory ; leur validation, leur stockage et leur boucle de session restent dans Common.
/// Le cœur est appelé directement dans le processus de la machine, par le même contrat IEmulatorCore.</remarks>
internal sealed class InternalMachineFactory() : MachineFactory(Emulator.Internal, EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorConstants.DescriptionResourceKey) { }
