namespace GWGUI.MediaEngine.FileSystems.Apple.Lisa;

/// <summary>Contient la version et le nom décodés depuis la page MDDF.</summary>
internal sealed record LisaMddf
{
    /// <summary>Crée les informations décodées depuis le MDDF.</summary>
    public LisaMddf(ushort version, string volumeName)
    {
        Version = version;
        VolumeName = volumeName;
    }

    /// <summary>Version du catalogue annoncée par le volume.</summary>
    public ushort Version { get; }

    /// <summary>Nom du volume.</summary>
    public string VolumeName { get; }
}
