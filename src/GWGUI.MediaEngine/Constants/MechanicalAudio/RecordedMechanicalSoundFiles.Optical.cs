using System.Collections.Generic;
using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    /// <summary>Profils enregistrés des lecteurs optiques.</summary>
    internal static class Optical
    {
        /// <summary>Enregistrements Unidentified CD-ROM-tray-drive.</summary>
        internal static RecordedMechanicalSoundProfile UnidentifiedCdRomTrayDrive { get; } = new(
            Id: M.OpticalTray,
            Device: M.Optical,
            DisplayName: "Unidentified CD-ROM-tray-drive",
            Samples:
            [
                new("Optical/Unidentified/CD-ROM-tray-drive/Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)])
                {
                    SourceFileName = "spin.wav",
                    SourcePath = "Other sources - IBMulator/cdrom/spin.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "1896a6b3dda0d9736db83fddb98ed50e4d688f046bcab76a71ebc5888de59750"
                },
                new("Optical/Unidentified/CD-ROM-tray-drive/Motor-start.flac", [new(R.Start)])
                {
                    SourceFileName = "spin_start.wav",
                    SourcePath = "Other sources - IBMulator/cdrom/spin_start.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "0195f7d0044a1e1c9cf03f142efcb7b35f58d215087e578142677d6cc9875513"
                },
                new("Optical/Unidentified/CD-ROM-tray-drive/Motor-stop.flac", [new(M.Stop)])
                {
                    SourceFileName = "spin_stop.wav",
                    SourcePath = "Other sources - IBMulator/cdrom/spin_stop.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "8fe39ee8f6b754e3cde16dc05206b5d3085770c2838bdf19fd15167a79813748"
                },
                new("Optical/Unidentified/CD-ROM-tray-drive/Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)])
                {
                    SourceFileName = "seek_step.wav",
                    SourcePath = "Other sources - IBMulator/cdrom/seek_step.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "7e61c6deaba10e6d1c864a1308cfcd27dc4cb77021b2ac8b420caece28241277"
                },
                new("Optical/Unidentified/CD-ROM-tray-drive/Seek/Toward-center/Movement.flac", [new(M.Seek, Direction: 1)])
                {
                    SourceFileName = "seek_in.wav",
                    SourcePath = "Other sources - IBMulator/cdrom/seek_in.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "3e6eb1826d224833a61efd599d044458bce22e1d1079b620453c09b57746f37c"
                },
                new("Optical/Unidentified/CD-ROM-tray-drive/Seek/Toward-edge/Movement.flac", [new(M.Seek, Direction: -1)])
                {
                    SourceFileName = "seek_out.wav",
                    SourcePath = "Other sources - IBMulator/cdrom/seek_out.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "38dbea6a09c39fb5e398b4540150e8ad9392d7ec2448728e89d4e1e0b684e27b"
                },
                new("Optical/Unidentified/CD-ROM-tray-drive/Tray-close.flac", [new(M.Insert)])
                {
                    SourceFileName = "close_tray.wav",
                    SourcePath = "Other sources - IBMulator/cdrom/close_tray.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "3d7b20877fa058a79377a54e08be7b67b41d7cb7a5d1c11cca622cd7305e8030"
                },
                new("Optical/Unidentified/CD-ROM-tray-drive/Tray-open.flac", [new(M.Eject)])
                {
                    SourceFileName = "open_tray.wav",
                    SourcePath = "Other sources - IBMulator/cdrom/open_tray.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "abd4d09d9a4ad8581c70c6afa64a1ee47e6a2ce654233eb7adae03def78423f9"
                },
            ],
            MissingOperations: []);

        /// <summary>Profils regroupés par matériel pour cette catégorie de lecteurs.</summary>
        internal static IReadOnlyList<RecordedMechanicalSoundProfile> Catalogs { get; } =
        [
            UnidentifiedCdRomTrayDrive,
        ];
    }
}
