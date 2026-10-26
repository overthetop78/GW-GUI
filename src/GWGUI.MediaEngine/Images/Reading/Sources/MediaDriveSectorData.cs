using System.Collections;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Images.Reading.Sources;

/// <summary>Signale les lectures des octets d’un secteur sans recopier leurs données ni posséder le lecteur.</summary>
internal sealed class MediaDriveSectorData(IReadOnlyList<byte> data, IMediaDriveMechanics drive, MediaDrivePosition position, bool writing) : IReadOnlyList<byte>
{
    internal IMediaDriveMechanics Drive => drive;
    internal bool Writing => writing;
    public int Count => data.Count;

    public byte this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            if (index >= data.Count) throw new ArgumentOutOfRangeException(nameof(index));
            Begin();
            try { return data[index]; }
            finally { drive.EndAccess(); }
        }
    }

    public IEnumerator<byte> GetEnumerator()
    {
        Begin();
        try
        {
            foreach (var value in data) yield return value;
        }
        finally { drive.EndAccess(); }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void Begin()
    {
        if (writing) drive.BeginWrite(position);
        else drive.BeginRead(position);
    }
}
