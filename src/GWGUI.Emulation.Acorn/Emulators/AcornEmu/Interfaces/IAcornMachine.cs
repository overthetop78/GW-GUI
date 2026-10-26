namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;

/// <summary>Échanges entre le cœur AcornEmu et l'assemblage matériel choisi dans son catalogue de modèles.</summary>
/// <remarks>Le cœur ne connaît ni le bus, ni la RAM, ni les périphériques d'un modèle particulier.
/// Chaque assemblage possède ses composants gérés ; le thread et la présentation restent possédés par la session Common.</remarks>
internal interface IAcornMachine
{
    byte[] RunFrame();
    /// <summary>Transfère les échantillons mono produits depuis le dernier prélèvement.</summary>
    short[] DrainAudio();
    IEmulatorCassetteTransport CassetteTransport { get; }
    void InsertCassette(EmulationCassetteSignal signal, bool writable = false);
    bool CassetteHasChanges { get; }
    EmulationCassetteSignal RecordedCassette { get; }
    void MarkCassetteSaved();
    void RefreshCassetteIndex(EmulationCassetteSignal signal);
    void ConfigureCassette(EmulationCassetteSettings settings);
    void EjectCassette();
    void AttachFloppy(int index, IEmulationDiskImage? image);
    bool FloppyActive(int index);
    bool FloppyMotor(int index);
    bool FloppyReading(int index);
    bool FloppyWriting(int index);
    long FloppySeekSteps(int index);
    long FloppyReadBytes(int index);
    long FloppyWrittenBytes(int index);
    void ConfigureFloppySpeed(int index, int percent);
    void AttachHardDisk(int unit, IEmulationDiskImage? image);
    bool HardDiskActive(int unit);
    bool HardDiskMotor(int unit);
    long HardDiskSeekCount(int unit);
    long HardDiskReadBlocks(int unit);
    long HardDiskWrittenBlocks(int unit);
    void ConfigureHardDisk(int unit, int latencyMilliseconds, int speedPercent, int? writeLatencyMilliseconds = null);
    /// <summary>Raccorde les trames et l'état de ligne du transport possédé par le cœur.</summary>
    Action<ReadOnlyMemory<byte>>? NetworkFrameTransmitted { get; set; }
    Action<int>? NetworkPoll { get; set; }
    bool ReceiveNetworkFrame(ReadOnlySpan<byte> frame, bool validFcs = true);
    void SetNetworkLine(bool clockPresent, bool clearToSend = true, bool idle = true);
    void SetInput(EmulationInputSnapshot snapshot);
    void Reset(bool cold);
    void SetOption(string key, string value);
}
