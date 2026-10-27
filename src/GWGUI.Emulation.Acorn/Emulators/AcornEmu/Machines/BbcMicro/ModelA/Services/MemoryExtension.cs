using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.MemoryExtensionConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services;

/// <summary>RAM parallèle Aries B20 ou Watford 32K ; la vidéo reste raccordée à la RAM BBC.</summary>
internal sealed class MemoryExtension
{
    private readonly MemoryExtensionSettings _settings;
    private readonly byte[] _ram;
    private int _selection = C.MainSelection;

    /// <summary>Alloue uniquement la capacité de la carte choisie.</summary>
    internal MemoryExtension(MemoryExtensionSettings settings)
    {
        _settings = settings;
        _ram = new byte[settings.Bytes];
    }

    /// <summary>Lit la RAM sélectionnée par les écritures de commande ; aucun accès ROM n'est redirigé.</summary>
    internal byte Read(ushort address, byte[] main)
    {
        if (_settings.Enabled && _selection == C.UnmappedSelection) return C.UnmappedValue;
        return UsesBoard(address) ? _ram[Offset(address)] : main[address & (main.Length - 1)];
    }

    /// <summary>Écrit dans la RAM sélectionnée, sans modifier la RAM vidéo lorsque la carte est sélectionnée.</summary>
    internal void Write(ushort address, byte value, byte[] main)
    {
        if (_settings.Enabled && _selection == C.UnmappedSelection) return;
        if (UsesBoard(address)) _ram[Offset(address)] = value;
        else main[address & (main.Length - 1)] = value;
    }

    /// <summary>Décode les écritures FFFE/FFFF, et FFFC/FFFD pour Watford ; la donnée écrite n'intervient pas.</summary>
    internal bool Control(ushort address)
    {
        if (!_settings.Enabled || address < (_settings.Card == C.Watford32 ? C.FullRamSelect : C.MainRamSelect)) return false;
        _selection = address - C.FullRamSelect;
        return true;
    }

    /// <summary>Une mise sous tension efface la carte et sélectionne la RAM BBC ; BREAK conserve son état.</summary>
    internal void Reset(bool cold)
    {
        if (!cold) return;
        Array.Clear(_ram);
        _selection = C.MainSelection;
    }

    private bool UsesBoard(ushort address) => _settings.Enabled
        && (_selection == C.FullSelection || _selection == C.ShadowSelection && address >= C.ShadowStart);

    private int Offset(ushort address) => _settings.Card == C.AriesB20 ? address - C.ShadowStart : address;
}
