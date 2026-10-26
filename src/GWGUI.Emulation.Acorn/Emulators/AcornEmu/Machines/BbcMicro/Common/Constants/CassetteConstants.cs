namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants;

/// <summary>Horloges et seuils de la liaison cassette BBC, distincts du PSG et du PCM sonore.</summary>
internal static class CassetteConstants
{
    internal const int CpuClock = (int)BbcMicroModelConstants.CpuFrequencyHz;
    internal const int SerialClock = 19200;
    /// <summary>Présence enregistrée du magnétophone externe ; la prise de la carte reste présente indépendamment.</summary>
    internal const string ConnectedOption = "acornemu.cassette-connected";
    internal const int RecordingSampleRate = 48000;
    /// <summary>Seuil entre demi-ondes 2 400 Hz et 1 200 Hz : 300 microsecondes.</summary>
    internal const int HalfWaveThreshold = CpuClock * 3 / 10000;
    /// <summary>Détection de porteuse vers 174 ms, relevée sur BBC et utilisée par jsbeeb.</summary>
    internal const int CarrierHalfWaves = 836;
    internal const int CarrierPulseTicks = CpuClock / 4800;
    /// <summary>Formats de signal réellement pris en charge, partagés par la fabrique et le cœur.</summary>
    internal static readonly IReadOnlyList<string> Extensions = Array.AsReadOnly<string>([".uef", ".csw", ".wav", ".raw"]);
    internal static readonly IReadOnlySet<EmulationCassetteCommand> Commands = new HashSet<EmulationCassetteCommand> { EmulationCassetteCommand.Play, EmulationCassetteCommand.Stop, EmulationCassetteCommand.Pause, EmulationCassetteCommand.Rewind, EmulationCassetteCommand.FastForward };
    internal static readonly IReadOnlySet<EmulationCassetteCommand> RecordingCommands = new HashSet<EmulationCassetteCommand>(Commands) { EmulationCassetteCommand.Record };
}
