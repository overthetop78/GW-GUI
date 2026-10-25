namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;

/// <summary>Échanges entre le cœur AcornEmu et l'assemblage matériel choisi dans son catalogue de modèles.</summary>
/// <remarks>Le cœur ne connaît ni le bus, ni la RAM, ni les périphériques d'un modèle particulier.
/// Chaque assemblage possède ses composants gérés ; le thread et la présentation restent possédés par la session Common.</remarks>
internal interface IAcornMachine
{
    byte[] RunFrame();
    /// <summary>Transfère les échantillons mono produits depuis le dernier prélèvement.</summary>
    short[] DrainAudio();
    void SetInput(EmulationInputSnapshot snapshot);
    void Reset(bool cold);
    void SetOption(string key, string value);
}
