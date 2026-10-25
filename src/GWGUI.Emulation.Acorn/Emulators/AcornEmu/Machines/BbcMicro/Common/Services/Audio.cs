using AudioConstants = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.AudioConstants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;

/// <summary>Raccorde le PSG BBC à l'horloge CPU et transforme son signal mono en PCM signé sur seize bits.</summary>
/// <remarks>Les échantillons intègrent les niveaux traversés pendant leur durée, au lieu de prendre seulement
/// la dernière valeur. La phase rationnelle reste conservée entre les images : aucun arrondi par frame ne modifie
/// la fréquence ou la durée. Le couplage continu retire le piédestal du PSG, sans filtre de présentation ajouté.
/// Cette classe ne crée aucune sortie, aucun thread et aucune ressource externe.</remarks>
internal sealed class Audio
{
    private readonly Sn76489 _chip = new();
    private readonly List<short> _samples = [];
    private readonly double _coupling = Math.Exp(-1 / (AudioConstants.SampleRate * AudioConstants.CouplingTimeSeconds));
    private int _phase;
    private long _area;
    private double _previousInput, _previousOutput;

    /// <summary>Écrit le bus lent lorsque la carte valide le PSG.</summary>
    internal void Write(byte value) => _chip.Write(value);

    /// <summary>Réinitialise le circuit, la phase et les données non encore transmises.</summary>
    internal void Reset()
    {
        _chip.Reset(); _samples.Clear(); _phase = 0; _area = 0; _previousInput = _previousOutput = 0;
    }

    /// <summary>Un cycle CPU apporte deux périodes de l'horloge sonore BBC de 4 MHz.</summary>
    internal void Tick()
    {
        for (var clock = 0; clock < AudioConstants.SoundClock / AudioConstants.CpuClock; clock++) _chip.Tick();
        var remaining = AudioConstants.SampleRate;
        var level = _chip.Output;
        while (remaining > 0)
        {
            var duration = Math.Min(remaining, AudioConstants.CpuClock - _phase);
            _area += (long)level * duration; _phase += duration; remaining -= duration;
            if (_phase != AudioConstants.CpuClock) continue;
            var input = (double)_area / AudioConstants.CpuClock;
            _previousOutput = input - _previousInput + _coupling * _previousOutput;
            _previousInput = input;
            _samples.Add((short)Math.Clamp(Math.Round(_previousOutput), short.MinValue, short.MaxValue));
            _phase = 0; _area = 0;
        }
    }

    /// <summary>Transfère une copie possédée des échantillons terminés, en conservant l'intégrale partielle.</summary>
    internal short[] DrainSamples()
    {
        var samples = _samples.ToArray(); _samples.Clear(); return samples;
    }
}
