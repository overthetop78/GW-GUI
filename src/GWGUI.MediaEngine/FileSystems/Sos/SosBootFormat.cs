using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.FileSystems.Sos;

/// <summary>Définit le marqueur et la fenêtre d'amorçage utilisés pour sonder Apple SOS.</summary>
internal static class SosBootFormat
{
    /// <summary>Capacité de l'image Apple III examinée.</summary>
    public const int ImageCapacity = AppleIIGeometry.Capacity;
    /// <summary>Longueur de la fenêtre d'amorçage examinée.</summary>
    public const int SearchLength = 128;
    /// <summary>Marqueur ASCII SOS recherché sans tenir compte de la casse.</summary>
    public static ReadOnlySpan<byte> Marker => "SOS"u8;

    /// <summary>Recherche le marqueur dans la fenêtre utile sans construire de chaîne.</summary>
    public static bool ContainsMarker(ReadOnlySpan<byte> data)
    {
        var window = data[..Math.Min(SearchLength, data.Length)];
        for (var offset = 0; offset <= window.Length - Marker.Length; offset++)
        {
            var matches = true;
            for (var index = 0; index < Marker.Length; index++)
            {
                var value = window[offset + index];
                if (value is >= (byte)'a' and <= (byte)'z') value -= (byte)('a' - 'A');
                if (value != Marker[index]) { matches = false; break; }
            }
            if (matches) return true;
        }
        return false;
    }
}
