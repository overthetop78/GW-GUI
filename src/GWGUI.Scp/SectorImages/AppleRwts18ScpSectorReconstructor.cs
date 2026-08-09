namespace GWGUI.Scp.SectorImages;

internal sealed class AppleRwts18ScpSectorReconstructor(AppleScpSectorDecoder decoder)
{
    public SectorImage Decode(ScpImage scp, CancellationToken cancellationToken)
    {
        var candidates = decoder.DecodeCandidates(scp, "apple2.rwts18", 768, cancellationToken);
        if (candidates.Count == 0)
            throw new InvalidDataException("No Apple II RWTS18 sectors could be decoded from the SCP image.");
        var blocks = candidates.Where(pair => pair.Key.Cylinder is >= 0 and < 50 &&
                                               pair.Key.Number is >= 0 and < 6)
            .Select(pair => AppleScpSectorDecoder.Select(
                pair.Key.Cylinder * 6 + pair.Key.Number, pair.Key, pair.Value)).ToArray();
        if (blocks.Length == 0)
            throw new InvalidDataException("No usable Apple II RWTS18 sectors could be reconstructed.");
        var tracks = Math.Max(35, blocks.Max(block => block.Address.Cylinder) + 1);
        return new("apple2.rwts18", 768, tracks, 1, 6, blocks);
    }
}
