namespace GWGUI.Scp.Images;

internal static class DiskSystemCatalog
{
    public static string NameFor(string formatId)
    {
        var id = formatId.ToLowerInvariant();
        return id switch
        {
            var value when value.StartsWith("apple2.") => "Apple II",
            var value when value.StartsWith("apple3.") => "Apple III",
            var value when value.StartsWith("mac.") || value.StartsWith("applemac.") => "Apple Macintosh",
            var value when value.StartsWith("lisa.") || value.StartsWith("applelisa.") => "Apple Lisa",
            var value when value.StartsWith("amiga.") => "Amiga",
            var value when value.StartsWith("atarist.") => "Atari ST",
            var value when value.StartsWith("atari.") => "Atari 8-bit",
            var value when value.StartsWith("ibm.") => "IBM PC",
            var value when value.StartsWith("commodore.") => "Commodore",
            var value when value.StartsWith("amstrad.") => "Amstrad",
            var value when value.StartsWith("acorn.") || value.StartsWith("bbc.") => "Acorn / BBC Micro",
            var value when value.StartsWith("epson.qx10.") => "Epson QX-10",
            var value when value.StartsWith("msx.") => "MSX",
            var value when value.StartsWith("dec.") => "DEC",
            var value when value.StartsWith("coherent.") => "COHERENT",
            var value when value.StartsWith("ucsd.") => "UCSD p-System",
            _ => "—"
        };
    }
}
