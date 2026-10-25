using GWGUI.Emulation.Example.Emulators.Libretro.Constants;
using GWGUI.Emulation.Example.Emulators.Common.Interop.Factories;

namespace GWGUI.Emulation.Example.Emulators.Libretro.Factories;

/// <summary>Adaptateur du cœur ExampleLibretro sous les mêmes interfaces que les deux autres émulateurs fictifs.</summary>
/// <remarks>Le catalogue découvre cette fabrique par son interface. Les deux profils sont déclarés par
/// MachineFactory ; leur validation, leur stockage et leur boucle de session restent dans Common.
/// Les exports et callbacks libretro sont fournis par des délégués gérés conservant les signatures du SDK.</remarks>
internal sealed class LibretroMachineFactory() : MachineFactory(Emulator.Libretro, EmulatorConstants.Id, EmulatorConstants.DisplayName, EmulatorConstants.DescriptionResourceKey) { }
