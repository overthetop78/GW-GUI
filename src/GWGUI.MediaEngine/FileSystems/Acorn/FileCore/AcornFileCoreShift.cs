namespace GWGUI.MediaEngine.FileSystems.Acorn.FileCore;

/// <summary>Applique un décalage FileCore vérifié : positif pour multiplier, négatif pour diviser.</summary>
public static class AcornFileCoreShift
{
    /// <summary>Applique le décalage demandé avec contrôle de plage et de débordement.</summary>
    public static int Apply(int value, int shift)
    {
        if (shift is <= -32 or >= 32) throw AcornFileCoreExceptions.InvalidShift(shift);
        if (shift < 0) return value >> -shift;
        return checked((int)((long)value << shift));
    }
}
