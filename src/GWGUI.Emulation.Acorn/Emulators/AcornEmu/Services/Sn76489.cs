using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Générateur TI SN76489AN : trois tons carrés et un bruit périodique ou pseudo-aléatoire.</summary>
/// <remarks>Ce composant ignore le modèle BBC, le VIA et la sortie physique. Tick reçoit une horloge d'entrée ;
/// Output représente la somme unipolaire des quatre voies avant le couplage analogique.
/// Les registres sont sélectionnés par les octets portant le bit 7 ; les octets suivants complètent le registre sélectionné.</remarks>
internal sealed class Sn76489
{
    private readonly int[] _periods = new int[3], _counts = new int[4], _attenuations = new int[4];
    private readonly bool[] _levels = new bool[3];
    private int _divider, _selected, _noise, _shift;

    internal Sn76489() => Reset();

    /// <summary>Somme instantanée, sans duplication stéréo ni application du volume de l'hôte.</summary>
    internal int Output
    {
        get
        {
            var level = (_shift & 1) != 0 ? SoundConstants.Amplitudes[_attenuations[3]] : 0;
            for (var channel = 0; channel < 3; channel++) if (_levels[channel]) level += SoundConstants.Amplitudes[_attenuations[channel]];
            return level;
        }
    }

    /// <summary>Réinitialise les registres et oscillateurs ; le MOS écrit ensuite ses volumes de démarrage.</summary>
    internal void Reset()
    {
        Array.Clear(_periods); Array.Clear(_counts); Array.Clear(_attenuations); Array.Clear(_levels);
        _divider = _selected = _noise = 0; _shift = SoundConstants.NoiseSeed;
    }

    /// <summary>Accepte un octet du bus lorsque le câblage propriétaire valide une écriture.</summary>
    internal void Write(byte value)
    {
        var latch = (value & 0x80) != 0;
        if (latch) _selected = (value >> 4) & 7;
        var channel = _selected >> 1;
        if ((_selected & 1) != 0) _attenuations[channel] = value & 15;
        else if (channel == 3) { _noise = value & 7; _shift = SoundConstants.NoiseSeed; }
        else _periods[channel] = latch ? (_periods[channel] & 0x3f0) | (value & 15) : (_periods[channel] & 15) | ((value & 63) << 4);
    }

    /// <summary>Avance d'une période d'entrée ; les compteurs internes travaillent après division par seize.</summary>
    internal void Tick()
    {
        if (++_divider < SoundConstants.ClockDivider) return;
        _divider = 0;
        for (var channel = 0; channel < 3; channel++)
            if (--_counts[channel] <= 0) { _levels[channel] = !_levels[channel]; _counts[channel] = Period(channel); }
        if (--_counts[3] > 0) return;
        var feedback = ((_shift & 4) != 0) ^ ((_noise & 4) != 0 && (_shift & 8) != 0);
        _shift = (_shift >> 1) | (feedback ? SoundConstants.NoiseSeed : 0);
        _counts[3] = (_noise & 3) == 3 ? 2 * Period(2) : 1 << (5 + (_noise & 3));
    }

    /// <summary>Résout le diviseur TI ; fréquence du ton = horloge / (32 × période).</summary>
    private int Period(int channel) => _periods[channel] == 0 ? SoundConstants.ZeroPeriod : _periods[channel];
}
