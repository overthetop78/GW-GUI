namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Compare une zone utilisateur et un nom CP/M sans tenir compte de la casse du nom.</summary>
internal sealed class CpmExtentKeyComparer : IEqualityComparer<(byte User, string Name)>
{
    /// <summary>Compare deux clés d'extent.</summary>
    public bool Equals((byte User, string Name) x, (byte User, string Name) y) => x.User == y.User && StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name);
    /// <summary>Calcule le code de hachage d'une clé d'extent.</summary>
    public int GetHashCode((byte User, string Name) obj) => HashCode.Combine(obj.User, StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name));
}
