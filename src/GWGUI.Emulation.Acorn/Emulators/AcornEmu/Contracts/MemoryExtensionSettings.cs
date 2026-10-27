using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.MemoryExtensionConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;

/// <summary>Carte Shadow RAM choisie ; sa capacité reste distincte de la RAM principale.</summary>
internal sealed record MemoryExtensionSettings(string Card)
{
    /// <summary>Présence physique de la carte.</summary>
    internal bool Enabled => Card != C.None;
    /// <summary>Capacité physique de la carte sélectionnée, sans addition à la RAM principale.</summary>
    internal int Bytes => Card switch
    {
        C.None => 0,
        C.AriesB20 => C.AriesBytes,
        C.Watford32 => C.WatfordBytes,
        _ => throw new ArgumentOutOfRangeException(nameof(Card))
    };
    /// <summary>Nom invariant de la ROM de gestion attendue.</summary>
    internal string FirmwareName => Card switch
    {
        C.AriesB20 => C.AriesName,
        C.Watford32 => C.WatfordName,
        _ => throw new ArgumentOutOfRangeException(nameof(Card))
    };
}
