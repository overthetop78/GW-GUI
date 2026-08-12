namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Contient une paire positionnelle de blocs de segment RT-11.</summary>
/// <param name="Bytes">Octets positionnés des deux blocs.</param>
/// <param name="FirstPresent">Présence du premier bloc.</param>
/// <param name="SecondPresent">Présence du second bloc.</param>
/// <param name="FirstValid">Validité du premier bloc.</param>
/// <param name="SecondValid">Validité du second bloc.</param>
public sealed record Rt11BlockPairResult(IReadOnlyList<byte> Bytes, bool FirstPresent, bool SecondPresent, bool FirstValid, bool SecondValid)
{
    /// <summary>Indique si les deux blocs sont présents et valides.</summary>
    public bool IsValid => FirstPresent && SecondPresent && FirstValid && SecondValid;
}
