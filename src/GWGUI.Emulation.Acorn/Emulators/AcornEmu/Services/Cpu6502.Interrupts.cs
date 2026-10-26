namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Broches d'interruption du NMOS 6502, indépendantes des périphériques raccordés.</summary>
internal sealed partial class Cpu6502
{
    private bool _nmiLevel, _nmiPending;

    /// <summary>La NMI est mémorisée sur front actif ; un niveau maintenu ne déclenche pas plusieurs entrées.</summary>
    private void SampleNmi()
    {
        var active = bus.Nmi;
        if (active && !_nmiLevel) _nmiPending = true;
        _nmiLevel = active;
    }

    /// <summary>Empile le retour et l'état sans indicateur BRK, puis charge le vecteur choisi en sept cycles.</summary>
    private void EnterInterrupt(long start, ushort vector)
    {
        Push((byte)(PC >> 8)); Push((byte)PC); Push((byte)((P | Unused) & ~Break));
        P |= InterruptDisable;
        PC = ReadWord(vector);
        Pad(start, 7);
    }
}
