using System.Collections.Generic;
using GWGUI.MediaEngine.Contracts;
using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;
using R = GWGUI.MediaEngine.Constants.RecordedMechanicalSoundConstants;

namespace GWGUI.MediaEngine.Constants;

internal static partial class RecordedMechanicalSoundFiles
{
    /// <summary>Profils enregistrés des lecteurs de disques durs.</summary>
    internal static class HardDisk
    {
        /// <summary>Enregistrements IBM H3171.</summary>
        internal static RecordedMechanicalSoundProfile IbmH3171 { get; } = new(
            Id: "HardDisk/IBM/H3171",
            Device: M.HardDisk,
            DisplayName: "IBM H3171",
            Samples:
            [
                new("HardDisk/IBM/H3171/Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)])
                {
                    SourceFileName = "1993_IBM_H3171_3.5_SPINDLE_RUNNING.wav",
                    SourcePath = "sounds/hdd/1993 IBM H3171/1993_IBM_H3171_3.5_SPINDLE_RUNNING.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "a7b8603784d0b804d5192e78b546a3c972ff3842cdcdfc50a6703e97f98062b1"
                },
                new("HardDisk/IBM/H3171/Motor-start.flac", [new(R.Start)])
                {
                    SourceFileName = "1993_IBM_H3171_3.5_SPINDLE_SPINUP.wav",
                    SourcePath = "sounds/hdd/1993 IBM H3171/1993_IBM_H3171_3.5_SPINDLE_SPINUP.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "ca7e54b1e9b9b7f7b82b12d876d989aec6b34dfcdc0c57d3df81dc46efd46014"
                },
                new("HardDisk/IBM/H3171/Motor-stop.flac", [new(M.Stop)])
                {
                    SourceFileName = "1993_IBM_H3171_3.5_SPINDLE_SPINDOWN.wav",
                    SourcePath = "sounds/hdd/1993 IBM H3171/1993_IBM_H3171_3.5_SPINDLE_SPINDOWN.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "4674890ef24a89b4dae7830642a71ddedf040695042ba563b197f5a542628b8d"
                },
                new("HardDisk/IBM/H3171/Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)])
                {
                    SourceFileName = "1993_IBM_H3171_3.5_SEEK_1TRACK.wav",
                    SourcePath = "sounds/hdd/1993 IBM H3171/1993_IBM_H3171_3.5_SEEK_1TRACK.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "cd4ee91cd93821361e4982734dc229af3ca726bf3d4f5b4a18edd9c58e611f6b"
                },
            ],
            MissingOperations: []);

        /// <summary>Enregistrements Miniscribe 8425.</summary>
        internal static RecordedMechanicalSoundProfile Miniscribe8425 { get; } = new(
            Id: M.Winchester,
            Device: M.HardDisk,
            DisplayName: "Miniscribe 8425",
            Samples:
            [
                new("HardDisk/Miniscribe/8425/Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)])
                {
                    SourceFileName = "1987_Miniscribe 8425_3600RPM_RUNNING.wav",
                    SourcePath = "sounds/hdd/1987 Miniscribe 8425/1987_Miniscribe 8425_3600RPM_RUNNING.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "03a2b61444127f45ba76abeae872507516299e142c58474dabd443bc855533b9"
                },
                new("HardDisk/Miniscribe/8425/Motor-start.flac", [new(R.Start)])
                {
                    SourceFileName = "1987_Miniscribe 8425_3600RPM_SPINUP.wav",
                    SourcePath = "sounds/hdd/1987 Miniscribe 8425/1987_Miniscribe 8425_3600RPM_SPINUP.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "60d11855e885f37fcb8b94fe48e2363216949b896adba38324324fecefe9ac31"
                },
                new("HardDisk/Miniscribe/8425/Motor-stop.flac", [new(M.Stop)])
                {
                    SourceFileName = "1987_Miniscribe 8425_3600RPM_SPINDOWN.wav",
                    SourcePath = "sounds/hdd/1987 Miniscribe 8425/1987_Miniscribe 8425_3600RPM_SPINDOWN.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "82027be9017879a6ab7ea2201f4b2446db626f0a41e579d98f60b0f75b7b6d42"
                },
                new("HardDisk/Miniscribe/8425/Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)])
                {
                    SourceFileName = "1987_Miniscribe 8425_3600RPM_SEEK_1TRACK.wav",
                    SourcePath = "sounds/hdd/1987 Miniscribe 8425/1987_Miniscribe 8425_3600RPM_SEEK_1TRACK.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "16098713d00c03cedfb411d4bd991dc059387c8f803d7562ebf3582065c7d234"
                },
            ],
            MissingOperations: []);

        /// <summary>Enregistrements Quantum Bigfoot 2110AT.</summary>
        internal static RecordedMechanicalSoundProfile QuantumBigfoot2110at { get; } = new(
            Id: "HardDisk/Quantum/Bigfoot 2110AT",
            Device: M.HardDisk,
            DisplayName: "Quantum Bigfoot 2110AT",
            Samples:
            [
                new("HardDisk/Quantum/Bigfoot 2110AT/Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)])
                {
                    SourceFileName = "1997_Quantum Bigfoot 2110AT_3600RPM_SPINDLE_RUNNING.wav",
                    SourcePath = "sounds/hdd/1997 Quantum Bigfoot 2110AT/1997_Quantum Bigfoot 2110AT_3600RPM_SPINDLE_RUNNING.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "20e351c327539c39209ad3e481a312e6ac52c2112b7583ade15fcb035762424d"
                },
                new("HardDisk/Quantum/Bigfoot 2110AT/Motor-start.flac", [new(R.Start)])
                {
                    SourceFileName = "1997_Quantum Bigfoot 2110AT_3600RPM_SPINDLE_SPINUP.wav",
                    SourcePath = "sounds/hdd/1997 Quantum Bigfoot 2110AT/1997_Quantum Bigfoot 2110AT_3600RPM_SPINDLE_SPINUP.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "62c2ef11d17a6a8ca44ec4b7d1a407f988824436a45f99ce91b1cb47197441b6"
                },
                new("HardDisk/Quantum/Bigfoot 2110AT/Motor-stop.flac", [new(M.Stop)])
                {
                    SourceFileName = "1997_Quantum Bigfoot 2110AT_3600RPM_SPINDLE_SPINDOWN.wav",
                    SourcePath = "sounds/hdd/1997 Quantum Bigfoot 2110AT/1997_Quantum Bigfoot 2110AT_3600RPM_SPINDLE_SPINDOWN.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "be916100aef2631cdbaf21c793eb15f2cde6e14b2b7d24330c7699b8dfafb7de"
                },
                new("HardDisk/Quantum/Bigfoot 2110AT/Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)])
                {
                    SourceFileName = "1997_Quantum Bigfoot 2110AT_3600RPM_SEEK_1TRACK.wav",
                    SourcePath = "sounds/hdd/1997 Quantum Bigfoot 2110AT/1997_Quantum Bigfoot 2110AT_3600RPM_SEEK_1TRACK.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "c22940d7bee5e8ca2b7b603ecde49bc5ae14cac716c254d771a0ae2994ea2376"
                },
            ],
            MissingOperations: []);

        /// <summary>Enregistrements Seagate ST3120A.</summary>
        internal static RecordedMechanicalSoundProfile SeagateSt3120a { get; } = new(
            Id: M.HardDisk35,
            Device: M.HardDisk,
            DisplayName: "Seagate ST3120A",
            Samples:
            [
                new("HardDisk/Seagate/ST3120A/Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)])
                {
                    SourceFileName = "ST3120A Running.wav",
                    SourcePath = "sounds/hdd/1992 Seagate ST3120A/ST3120A Running.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "c427f39e48cd69b811809c762e9089982aa29d7ebcc039303a41c6bf812a33e6"
                },
                new("HardDisk/Seagate/ST3120A/Motor-start-with-initialization.flac", [new(R.Start)])
                {
                    SourceFileName = "ST3120A SpinUp with initializing.wav",
                    SourcePath = "sounds/hdd/1992 Seagate ST3120A/ST3120A SpinUp with initializing.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "a51fe903d429ce1794b82e2046fdf733da2386904a64e745cf2eab0035cbbd7b"
                },
                new("HardDisk/Seagate/ST3120A/Motor-start.flac", [new(R.Start)])
                {
                    SourceFileName = "ST3120A SpinUp.wav",
                    SourcePath = "sounds/hdd/1992 Seagate ST3120A/ST3120A SpinUp.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "a374fc4ab50571e54f145da10618d38f1fdbe2504a845e89531fccefea9f2170"
                },
                new("HardDisk/Seagate/ST3120A/Motor-stop.flac", [new(M.Stop)])
                {
                    SourceFileName = "ST3120A Spindown.wav",
                    SourcePath = "sounds/hdd/1992 Seagate ST3120A/ST3120A Spindown.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "f4f589b616ee0f2bbb2d47f27c16a263ffafa2f85d0b66f3bef8e22e06f6feae"
                },
                new("HardDisk/Seagate/ST3120A/Seek/Single-track-movement.flac", [new(M.Seek, Distance: 1)])
                {
                    SourceFileName = "ST3120A Track.wav",
                    SourcePath = "sounds/hdd/1992 Seagate ST3120A/ST3120A Track.wav",
                    SourceRepository = "86Box/assets",
                    SourceRevision = "82de0b6c1e070e606c5ca23ac3a33c2c486ece26",
                    SourceSha256 = "a3f5a6bd4604ab25237bf94c3877e9f168be9d41e8f2a19efcc1caedad64a897"
                },
            ],
            MissingOperations: []);

        /// <summary>Enregistrements Unidentified Hard-disk-drive.</summary>
        internal static RecordedMechanicalSoundProfile UnidentifiedHardDiskDrive { get; } = new(
            Id: "HardDisk/Unidentified/Hard-disk-drive",
            Device: M.HardDisk,
            DisplayName: "Unidentified Hard-disk-drive",
            Samples:
            [
                new("HardDisk/Unidentified/Hard-disk-drive/Motor-running.flac", [new(M.Motor, Loop: true), new(M.Read, Loop: true), new(M.Write, Loop: true)])
                {
                    SourceFileName = "drive_spin.wav",
                    SourcePath = "Other sources - IBMulator/hdd/drive_spin.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "b3e5fbd036c9931d56e16a392eb9f61435ba44d5a706dcf861d713fe7f9fbc48"
                },
                new("HardDisk/Unidentified/Hard-disk-drive/Motor-start.flac", [new(R.Start)])
                {
                    SourceFileName = "drive_spin_up.wav",
                    SourcePath = "Other sources - IBMulator/hdd/drive_spin_up.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "0505ee9b51d52e9e728d46a6ce77ce42e017f443fe22da1f573c27cccb809094"
                },
                new("HardDisk/Unidentified/Hard-disk-drive/Motor-stop.flac", [new(M.Stop)])
                {
                    SourceFileName = "drive_spin_down.wav",
                    SourcePath = "Other sources - IBMulator/hdd/drive_spin_down.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "a2ea89b4afec96f0e734e5c06217d45bfc127a355d532b1c0e4eeac22d51a1e0"
                },
                new("HardDisk/Unidentified/Hard-disk-drive/Seek/Head-movement.flac", [new(M.Seek)])
                {
                    SourceFileName = "drive_seek.wav",
                    SourcePath = "Other sources - IBMulator/hdd/drive_seek.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "5d02b0bddf013fd48b1f9932653ed3301cc9cb903f6c5b43f5c4f6e6e3a5f680"
                },
                new("HardDisk/Unidentified/Hard-disk-drive/Seek/Long-head-movement.flac", [new(M.Seek)])
                {
                    SourceFileName = "drive_seek_long.wav",
                    SourcePath = "Other sources - IBMulator/hdd/drive_seek_long.wav",
                    SourceRepository = "andresdelcampo/RetroDriveSounds",
                    SourceRevision = "88a3fd41050bc7c0f8f9c3240f3a97146d786e4a",
                    SourceSha256 = "0c831a4321790fc79044ce676b13611901331a9783b846140a2368ced4920dcd"
                },
            ],
            MissingOperations: []);

        /// <summary>Profils regroupés par matériel pour cette catégorie de lecteurs.</summary>
        internal static IReadOnlyList<RecordedMechanicalSoundProfile> Catalogs { get; } =
        [
            IbmH3171,
            Miniscribe8425,
            QuantumBigfoot2110at,
            SeagateSt3120a,
            UnidentifiedHardDiskDrive,
        ];
    }
}
