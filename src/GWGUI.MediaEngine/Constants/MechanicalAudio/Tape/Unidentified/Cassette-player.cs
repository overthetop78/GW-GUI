using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    internal static partial class Tape
    {
        /// <summary>Dossier externe du magnétophone sans référence commerciale identifiée.</summary>
        private const string CassettePlayerDirectory = "Tape/Unidentified/Cassette-player/";
        /// <summary>Commande enregistrée de mise en lecture.</summary>
        private const string PlayStart = "TapePlayStart";
        /// <summary>Commande enregistrée de mise en avance rapide.</summary>
        private const string ForwardStart = "TapeForwardStart";
        /// <summary>Commande enregistrée de mise en rembobinage.</summary>
        private const string RewindStart = "TapeRewindStart";

        /// <summary>Actions et rotations du même magnétophone ; lecture et enregistrement partagent le mécanisme de défilement.</summary>
        internal static RecordedMechanicalSoundProfile UnidentifiedCassettePlayer { get; } = new(
            M.Standard, M.Cassette, "Cassette player",
            [
                new(CassettePlayerDirectory + "Play-start.flac", [new(PlayStart)]),
                new(CassettePlayerDirectory + "Play-running.flac", [new(M.Play, Loop: true, StartOperation: PlayStart), new(M.Record, Loop: true, StartOperation: PlayStart), new(M.Motor, Loop: true, StartOperation: PlayStart)]),
                new(CassettePlayerDirectory + "Forward-start.flac", [new(ForwardStart)]),
                new(CassettePlayerDirectory + "Forward-running.flac", [new(M.FastForward, Loop: true, StartOperation: ForwardStart)]),
                new(CassettePlayerDirectory + "Rewind-start.flac", [new(RewindStart)]),
                new(CassettePlayerDirectory + "Rewind-running.flac", [new(M.Rewind, Loop: true, StartOperation: RewindStart)]),
                new(CassettePlayerDirectory + "Stop.flac", [new(M.Stop)]),
                new(CassettePlayerDirectory + "Pause.flac", [new(M.Pause)]),
                new(CassettePlayerDirectory + "Cassette-insert.flac", [new(M.Insert)]),
                new(CassettePlayerDirectory + "Cassette-eject.flac", [new(M.Eject)]),
            ]);
    }
}
