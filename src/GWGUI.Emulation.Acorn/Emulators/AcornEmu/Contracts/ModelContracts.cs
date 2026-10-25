using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;

/// <summary>Association d'un modèle réellement implémenté à son constructeur et aux caractéristiques du rendu de son adaptation.</summary>
/// <remarks>Les caractéristiques matérielles et firmwares viennent de Common/Machines.
/// Ce contrat décrit les sorties de l'implémentation AcornEmu ; il ne redéfinit aucune identité de BIOS.</remarks>
internal sealed record AcornEmuModelDefinition(MachineModel Model, double FramesPerSecond, int Width, int Height, int BytesPerPixel, EmulationPixelFormat PixelFormat, float AspectRatio, EmulationVideoStandard Region, Func<MachineConfiguration, IAcornMachine> Create);
