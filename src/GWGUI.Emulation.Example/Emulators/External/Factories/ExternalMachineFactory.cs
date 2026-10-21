using GWGUI.Emulation.Example.Emulators.External.Constants;
using GWGUI.Emulation.Example.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Example.Emulators.External.Factories;

/// <summary>Adaptateur du cœur ExampleExternal sous les mêmes interfaces que les deux autres émulateurs fictifs.</summary>
/// <remarks>Le catalogue découvre cette fabrique par son interface. Les deux profils sont déclarés par
/// MachineFactory ; leur validation, leur stockage et leur boucle de session restent dans Common.
/// Les appels d’exports de DLL sont simulés en mémoire ; aucune DLL externe n’est créée ou chargée.</remarks>
internal sealed class ExternalMachineFactory() : MachineFactory(Emulator.External, EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorConstants.DescriptionResourceKey) { }
