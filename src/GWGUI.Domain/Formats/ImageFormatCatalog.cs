namespace GWGUI.Domain.Formats;

public sealed class BuiltInImageFormatCatalog : IImageFormatCatalog
{
    public IReadOnlyList<DiskFormat> Formats { get; }

    public BuiltInImageFormatCatalog(Func<string, string>? localize = null)
    {
        string T(string key, string fallback) => localize?.Invoke(key) ?? fallback;
        ImageExtension E(string extension, string key, string fallback, bool isDefault = false) => new(extension, T(key, fallback), isDefault);
        DiskFormat F(string id, string family, string fallback, IReadOnlyList<ImageExtension> extensions, bool common, string tag, params string[] sources) => new(id, family, T("Format." + id, fallback), extensions, common, Set(sources), tag);
        var ima = new Func<bool, ImageExtension>(isDefault => E(".ima", "Extension.ima", "IMA disk image", isDefault));
        var img = new Func<ImageExtension>(() => E(".img", "Extension.img", "IMG disk image"));
        IReadOnlyList<ImageExtension> Ibm() => [ima(true), img()];
        Formats =
        [
            new("raw.scp", "Raw", T("Format.raw.scp", "Raw flux image"), [E(".scp", "Extension.scp", "SuperCard Pro", true)], CompatibleSourceExtensions: Set(".scp", ".hfe"), Tag: "RAW-SCP"),
            F("amiga.amigados", "Amiga", "AmigaDOS — 880 KiB", [E(".adf", "Extension.adf.amiga", "Amiga Disk File", true)], true, "AMIGA-DD", ".scp", ".adf", ".hfe"),
            F("amiga.amigados_hd", "Amiga", "AmigaDOS HD — 1.76 MiB", [E(".adf", "Extension.adf.amiga", "Amiga Disk File", true)], true, "AMIGA-HD", ".scp", ".adf", ".hfe"),
            F("atarist.180", "Atari ST", "Atari ST — 180 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-180", ".scp", ".st", ".hfe"),
            F("atarist.360", "Atari ST", "Atari ST — 360 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-360", ".scp", ".st", ".msa", ".hfe"),
            F("atarist.400", "Atari ST", "Atari ST — 400 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-400", ".scp", ".st", ".msa", ".hfe"),
            F("atarist.440", "Atari ST", "Atari ST — 440 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-440", ".scp", ".st", ".msa", ".hfe"),
            F("atarist.720", "Atari ST", "Atari ST — 720 KiB", [E(".st", "Extension.st", "Atari ST image", true), E(".msa", "Extension.msa", "Magic Shadow Archiver")], true, "ST-720", ".scp", ".st", ".msa", ".hfe"),
            F("atarist.800", "Atari ST", "Atari ST — 800 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-800", ".scp", ".st", ".msa", ".hfe"),
            F("atarist.810", "Atari ST", "Atari ST — 810 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-810", ".scp", ".st", ".msa", ".hfe"),
            F("atarist.880", "Atari ST", "Atari ST — 880 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-880", ".scp", ".st", ".msa", ".hfe"),
            F("atarist.1440", "Atari ST", "Atari ST — 1.44 MiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-1440", ".scp", ".st", ".hfe"),
            F("atari.90", "Atari 8-bit", "Atari 8-bit — 90 KiB", [E(".atr", "Extension.atr", "Atari ATR image", true)], true, "ATARI8-90", ".scp", ".atr", ".hfe"),
            F("atari.130", "Atari 8-bit", "Atari 8-bit — 130 KiB", [E(".atr", "Extension.atr", "Atari ATR image", true)], true, "ATARI8-130", ".scp", ".atr", ".hfe"),
            F("atari.180", "Atari 8-bit", "Atari 8-bit — 180 KiB", [E(".atr", "Extension.atr", "Atari ATR image", true)], false, "ATARI8-180", ".scp", ".atr", ".hfe"),
            F("ibm.160", "IBM PC", "IBM PC — 160 KiB", Ibm(), false, "PC-160", ".scp", ".ima", ".img", ".hfe"), F("ibm.180", "IBM PC", "IBM PC — 180 KiB", Ibm(), false, "PC-180", ".scp", ".ima", ".img", ".hfe"),
            F("ibm.320", "IBM PC", "IBM PC — 320 KiB", Ibm(), false, "PC-320", ".scp", ".ima", ".img", ".hfe"), F("ibm.360", "IBM PC", "IBM PC — 360 KiB", Ibm(), true, "PC-360", ".scp", ".ima", ".img", ".hfe"),
            F("ibm.720", "IBM PC", "IBM PC — 720 KiB", Ibm(), true, "PC-720", ".scp", ".ima", ".img", ".hfe"), F("ibm.800", "IBM PC", "IBM PC — 800 KiB", Ibm(), false, "PC-800", ".scp", ".ima", ".img", ".hfe"),
            F("ibm.1200", "IBM PC", "IBM PC — 1.2 MiB", Ibm(), true, "PC-1200", ".scp", ".ima", ".img", ".hfe"), F("ibm.1440", "IBM PC", "IBM PC — 1.44 MiB", Ibm(), true, "PC-1440", ".scp", ".ima", ".img", ".hfe"),
            F("ibm.1680", "IBM PC", "IBM PC — 1.68 MiB", Ibm(), false, "PC-1680", ".scp", ".ima", ".img", ".hfe"), F("ibm.dmf", "IBM PC", "IBM PC — DMF 1.68 MiB", Ibm(), false, "PC-DMF", ".scp", ".ima", ".img", ".hfe"),
            F("ibm.2880", "IBM PC", "IBM PC — 2.88 MiB", Ibm(), false, "PC-2880", ".scp", ".ima", ".img", ".hfe"), F("ibm.scan", "IBM PC", "IBM PC — FM/MFM scan", Ibm(), false, "PC-SCAN", ".scp", ".hfe"),
            F("commodore.1541", "Commodore", "Commodore 64 — 1541", [E(".d64", "Extension.d64", "Commodore D64 image", true)], true, "C64-1541", ".scp", ".d64", ".hfe"),
            F("commodore.1571", "Commodore", "Commodore 128 — 1571", [E(".d71", "Extension.d71", "Commodore D71 image", true)], true, "C128-1571", ".scp", ".d71", ".hfe"),
            F("commodore.1581", "Commodore", "Commodore 128 — 1581", [E(".d81", "Extension.d81", "Commodore D81 image", true)], true, "C128-1581", ".scp", ".d81", ".hfe"),
            F("apple2.appledos.113", "Apple II", "Apple II DOS 3.2 — 113 KiB", [E(".d13", "Extension.d13", "Apple DOS 3.2 image", true)], false, "APPLE2-DOS32", ".scp", ".d13", ".nib", ".woz", ".2mg", ".hfe"),
            F("apple2.appledos.140", "Apple II", "Apple II DOS 3.3 — 140 KiB", [E(".do", "Extension.do", "Apple DOS-order image", true), E(".dsk", "Extension.dsk.apple", "Apple disk image"), E(".nib", "Extension.nib", "Apple nibble image"), E(".woz", "Extension.woz", "Apple WOZ image")], true, "APPLE2-DOS33", ".scp", ".do", ".dsk", ".nib", ".woz", ".2mg", ".hfe"),
            F("apple2.prodos.140", "Apple II", "Apple II ProDOS — 140 KiB", [E(".po", "Extension.po", "Apple ProDOS-order image", true), E(".2mg", "Extension.2mg", "Apple 2IMG image")], true, "APPLE2-PRODOS", ".scp", ".po", ".do", ".dsk", ".nib", ".woz", ".2mg", ".hfe"),
            F("apple2.prodos.800", "Apple II", "Apple II ProDOS — 800 KiB", [E(".po", "Extension.po", "Apple ProDOS-order image", true), E(".2mg", "Extension.2mg", "Apple 2IMG image")], true, "APPLE2-PRODOS-800", ".scp", ".po", ".2mg", ".image", ".hfe"),
            F("apple3.sos", "Apple III", "Apple III SOS — 140 KiB", [E(".po", "Extension.po", "Apple ProDOS-order image", true), E(".dsk", "Extension.dsk.apple", "Apple disk image")], false, "APPLE3-SOS", ".scp", ".po", ".do", ".dsk", ".2mg", ".hfe"),
            F("mac.400", "Apple Macintosh", "Apple Macintosh/Lisa GCR — 400 KiB", [E(".image", "Extension.image.apple", "Apple DiskCopy image", true), E(".img", "Extension.img.apple", "Apple raw disk image")], true, "MAC-400", ".scp", ".image", ".dc42", ".img", ".hfe"),
            F("mac.800", "Apple Macintosh", "Apple Macintosh GCR — 800 KiB", [E(".image", "Extension.image.apple", "Apple DiskCopy image", true), E(".img", "Extension.img.apple", "Apple raw disk image")], true, "MAC-800", ".scp", ".image", ".dc42", ".img", ".hfe"),
            F("mac.1440", "Apple Macintosh", "Apple Macintosh MFM — 1.44 MiB", [E(".img", "Extension.img.apple", "Apple raw disk image", true), E(".image", "Extension.image.apple", "Apple DiskCopy image")], true, "MAC-1440", ".scp", ".image", ".dc42", ".img", ".hfe"),
            F("acorn.adfs.800", "Acorn", "Acorn ADFS — 800 KiB", [E(".adf", "Extension.adf.acorn", "Acorn Disk File", true)], true, "ACORN-800", ".scp", ".adf", ".hfe"),
            new("acorn.dfs.ss", "Acorn / BBC Micro", "BBC DFS — 100 KiB", [new(".ssd", "SSD", true)], true, Set(".scp", ".ssd", ".img", ".hfe"), "BBC-DFS-SS"),
            new("acorn.dfs.ss80", "Acorn / BBC Micro", "BBC DFS — 200 KiB", [new(".ssd", "SSD", true)], true, Set(".scp", ".ssd", ".img", ".hfe"), "BBC-DFS-SS80"),
            new("acorn.dfs.ds", "Acorn / BBC Micro", "BBC DFS — 200 KiB (DS)", [new(".dsd", "DSD", true)], true, Set(".scp", ".dsd", ".img", ".hfe"), "BBC-DFS-DS"),
            new("acorn.dfs.ds80", "Acorn / BBC Micro", "BBC DFS — 400 KiB (DS)", [new(".dsd", "DSD", true)], true, Set(".scp", ".dsd", ".img", ".hfe"), "BBC-DFS-DS80"),
            new("amstrad.cpc", "Amstrad", "Amstrad CPC — 3″", [new(".dsk", "DSK", true), new(".edsk", "Extended DSK")], true, Set(".scp", ".dsk", ".edsk", ".hfe"), "AMSTRAD-CPC"),
            new("amstrad.pcw", "Amstrad", "Amstrad PCW — 3″", [new(".dsk", "DSK", true), new(".edsk", "Extended DSK")], true, Set(".scp", ".dsk", ".edsk", ".hfe"), "AMSTRAD-PCW"),
            new("epson.qx10.320", "Epson QX-10", "Epson QX-10 — 320 KiB", [new(".img", "IMG", true), new(".imd", "IMD")], false, Set(".scp", ".img", ".imd", ".hfe"), "EPSON-QX10-320"),
            new("epson.qx10.396", "Epson QX-10", "Epson QX-10 — 396 KiB", [new(".img", "IMG", true), new(".imd", "IMD")], false, Set(".scp", ".img", ".imd", ".hfe"), "EPSON-QX10-396"),
            new("epson.qx10.399", "Epson QX-10", "Epson QX-10 — 399 KiB", [new(".img", "IMG", true), new(".imd", "IMD")], false, Set(".scp", ".img", ".imd", ".hfe"), "EPSON-QX10-399"),
            new("epson.qx10.400", "Epson QX-10", "Epson QX-10 — 400 KiB", [new(".img", "IMG", true), new(".imd", "IMD")], false, Set(".scp", ".img", ".imd", ".hfe"), "EPSON-QX10-400"),
            new("epson.qx10.logo", "Epson QX-10", "Epson QX-10 — Logo", [new(".img", "IMG", true), new(".imd", "IMD")], false, Set(".scp", ".img", ".imd", ".hfe"), "EPSON-QX10-LOGO"),
            new("msx.1d", "MSX", "MSX — 180 KiB", [new(".dsk", "DSK", true)], true, Set(".scp", ".dsk", ".img", ".hfe"), "MSX-1D"),
            new("msx.1dd", "MSX", "MSX — 360 KiB (SS)", [new(".dsk", "DSK", true)], true, Set(".scp", ".dsk", ".img", ".hfe"), "MSX-1DD"),
            new("msx.2d", "MSX", "MSX — 360 KiB", [new(".dsk", "DSK", true)], true, Set(".scp", ".dsk", ".img", ".hfe"), "MSX-2D"),
            new("msx.2dd", "MSX", "MSX — 720 KiB", [new(".dsk", "DSK", true)], true, Set(".scp", ".dsk", ".img", ".hfe"), "MSX-2DD"),
            new("dec.rx02", "DEC", "DEC RX02 — 512 KiB", [new(".img", "IMG", true)], false, Set(".scp", ".img", ".imd", ".td0", ".hfe"), "DEC-RX02"),
            F("ucsd.ibm.mfm", "UCSD p-System", "UCSD p-System — IBM MFM", [new(".img", "IMG", true), new(".td0", "TD0")], false, "UCSD-IBM-MFM", ".scp", ".img", ".td0", ".hfe"),
            new("commodore900.coherent", "Commodore 900", "Commodore 900 — COHERENT", [new(".bin", "BIN", true), new(".img", "IMG")], false, Set(".scp", ".bin", ".img", ".hfe"), "C900-COHERENT"),
            new("applelisa.office", "Apple Lisa", "Apple Lisa Office System", [new(".image", "DiskCopy", true), new(".dc42", "DiskCopy 4.2")], false, Set(".scp", ".image", ".dc42", ".hfe"), "LISA-OFFICE"),
            new("applelisa.macworks", "Apple Lisa", "Apple Lisa — MacWorks", [new(".image", "DiskCopy", true), new(".dc42", "DiskCopy 4.2")], false, Set(".scp", ".image", ".dc42", ".hfe"), "LISA-MACWORKS"),
            F("raw.hfe", "Raw", "HxC flux image", [E(".hfe", "Extension.hfe", "HxC Floppy Emulator", true)], false, "RAW-HFE", ".scp", ".hfe")
        ];
    }

    public IReadOnlyList<DiskFormat> GetCompatibleOutputs(string sourceExtension)
    {
        var normalized = Normalize(sourceExtension);
        return Formats.Where(format => format.CompatibleSourceExtensions?.Contains(normalized) == true).ToArray();
    }

    private static HashSet<string> Set(params string[] values) => new(values.Select(Normalize), StringComparer.OrdinalIgnoreCase);
    private static string Normalize(string value) => value.StartsWith('.') ? value.ToLowerInvariant() : "." + value.ToLowerInvariant();
}
