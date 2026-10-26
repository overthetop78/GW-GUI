using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;
using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

internal sealed partial class EmulatorCore
{
    /// <summary>Recommence une action explicite, y compris deux commandes identiques successives.</summary>
    private void SetCassetteSoundEvent(string operation)
    {
        _mechanicalEvent = operation;
        _mechanicalOperation = operation;
        _mechanicalOffset = 0;
    }

    /// <summary>Mélange les enregistrements du mécanisme ; le signal de données de la bande reste indépendant.</summary>
    private void MixCassetteSound(double[] samples)
    {
        if (!_cassetteSettings.MechanicalSound || _cassetteSettings.Mode != EmulationCassettePlaybackMode.RealTime)
        {
            _mechanicalEvent = null;
            _mechanicalOperation = S.None;
            _mechanicalOffset = 0;
            return;
        }
        var operation = _mechanicalEvent ?? (CassetteActive ? CassetteOperation?.ToString() : null) ?? S.None;
        if (operation == S.None && _mechanicalOperation is S.Play or S.Record or S.FastForward or S.Rewind)
            SetCassetteSoundEvent(operation = S.Stop);
        if (operation != _mechanicalOperation)
        {
            _mechanicalOperation = operation;
            _mechanicalOffset = 0;
        }
        var noise = BbcMicroCassetteFunctions.Sound(operation, SampleRate, samples.Length, _mechanicalOffset, _cassetteSettings.MechanicalVolumePercent);
        for (var index = 0; index < samples.Length; index++) samples[index] += noise[index];
        _mechanicalOffset += samples.Length;
        if (_mechanicalEvent is not null && _mechanicalOffset >= Math.Ceiling(SampleRate * BbcMicroCassetteFunctions.SoundDuration(_mechanicalEvent)))
            _mechanicalEvent = null;
    }
}
