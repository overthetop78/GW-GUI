namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;

/// <summary>Broches vues par le 6502 ; le CPU ignore le modèle de machine et le SDK de l'application.</summary>
/// <remarks>Chaque lecture ou écriture du processeur avance d'abord l'horloge par Tick. Le bus appartient à la machine.</remarks>
internal interface ICpuBus
{
    byte Read(ushort address);
    void Write(ushort address, byte value);
    void Tick();
    bool Irq { get; }
}
