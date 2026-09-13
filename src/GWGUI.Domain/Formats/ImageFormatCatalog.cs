namespace GWGUI.Domain.Formats;

public sealed class BuiltInImageFormatCatalog : IImageFormatCatalog
{
    public IReadOnlyList<DiskFormat> Formats { get; }

    public BuiltInImageFormatCatalog(Func<string, string>? localize = null)
    {
        string T(string key, string fallback) => localize?.Invoke(key) ?? fallback;
        ImageExtension E(string extension, string key, string fallback, bool isDefault = false) => new(extension, T(key, fallback), isDefault);
        DiskFormat F(string id, string family, string fallback, IReadOnlyList<ImageExtension> extensions, bool common, string tag, FloppyFormFactor formFactor, bool physicalRead, bool physicalWrite, params string[] sources) => new(id, family, T("Format." + id, fallback), extensions, common, Set(sources), tag, formFactor, physicalRead, physicalWrite);
        DiskFormat D(DiskFormat format, FloppyDensity density) => format with { Density = density };
        var ima = new Func<bool, ImageExtension>(isDefault => E(".ima", "Extension.ima", "IMA disk image", isDefault));
        var img = new Func<ImageExtension>(() => E(".img", "Extension.img", "IMG disk image"));
        IReadOnlyList<ImageExtension> Ibm() => [ima(true), img()];
        Formats =
        [
            new("raw.scp", "Raw", T("Format.raw.scp", "Raw flux image"), [E(".scp", "Extension.scp", "SuperCard Pro", true)], CompatibleSourceExtensions: Set(".scp", ".hfe"), Tag: "RAW-SCP", FormFactor: FloppyFormFactor.Unknown, SupportsPhysicalRead: true, SupportsPhysicalWrite: true),
            D(F("amiga.amigados", "Amiga", "AmigaDOS — 880 KiB", [E(".adf", "Extension.adf.amiga", "Amiga Disk File", true)], true, "AMIGA-DD", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".adf", ".hfe"), FloppyDensity.DoubleDensity),
            D(F("amiga.amigados_hd", "Amiga", "AmigaDOS HD — 1.76 MiB", [E(".adf", "Extension.adf.amiga", "Amiga Disk File", true)], true, "AMIGA-HD", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".adf", ".hfe"), FloppyDensity.HighDensity),
            D(F("atarist.180", "Atari ST", "Atari ST — 180 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-180", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".st", ".hfe"), FloppyDensity.DoubleDensity),
            D(F("atarist.360", "Atari ST", "Atari ST — 360 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-360", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".st", ".msa", ".hfe"), FloppyDensity.DoubleDensity),
            D(F("atarist.400", "Atari ST", "Atari ST — 400 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-400", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".st", ".msa", ".hfe"), FloppyDensity.DoubleDensity),
            D(F("atarist.440", "Atari ST", "Atari ST — 440 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-440", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".st", ".msa", ".hfe"), FloppyDensity.DoubleDensity),
            D(F("atarist.720", "Atari ST", "Atari ST — 720 KiB", [E(".st", "Extension.st", "Atari ST image", true), E(".msa", "Extension.msa", "Magic Shadow Archiver")], true, "ST-720", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".st", ".msa", ".hfe"), FloppyDensity.DoubleDensity),
            D(F("atarist.800", "Atari ST", "Atari ST — 800 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-800", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".st", ".msa", ".hfe"), FloppyDensity.DoubleDensity),
            D(F("atarist.810", "Atari ST", "Atari ST — 810 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-810", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".st", ".msa", ".hfe"), FloppyDensity.DoubleDensity),
            D(F("atarist.880", "Atari ST", "Atari ST — 880 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-880", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".st", ".msa", ".hfe"), FloppyDensity.DoubleDensity),
            D(F("atarist.1440", "Atari ST", "Atari ST — 1.44 MiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-1440", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".st", ".hfe"), FloppyDensity.HighDensity),
            F("atari.90", "Atari 8-bit", "Atari 8-bit — 90 KiB", [E(".atr", "Extension.atr", "Atari ATR image", true)], true, "ATARI8-90", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".atr", ".hfe"),
            F("atari.130", "Atari 8-bit", "Atari 8-bit — 130 KiB", [E(".atr", "Extension.atr", "Atari ATR image", true)], true, "ATARI8-130", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".atr", ".hfe"),
            F("atari.180", "Atari 8-bit", "Atari 8-bit — 180 KiB", [E(".atr", "Extension.atr", "Atari ATR image", true)], false, "ATARI8-180", FloppyFormFactor.FiveAndQuarterInch, false, false, ".scp", ".atr", ".hfe"),
            F("ibm.160", "IBM PC", "IBM PC — 160 KiB", Ibm(), false, "PC-160", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".ima", ".img", ".hfe"), F("ibm.180", "IBM PC", "IBM PC — 180 KiB", Ibm(), false, "PC-180", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".ima", ".img", ".hfe"),
            F("ibm.320", "IBM PC", "IBM PC — 320 KiB", Ibm(), false, "PC-320", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".ima", ".img", ".hfe"), F("ibm.360", "IBM PC", "IBM PC — 360 KiB", Ibm(), true, "PC-360", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".ima", ".img", ".hfe"),
            D(F("ibm.720", "IBM PC", "IBM PC — 720 KiB", Ibm(), true, "PC-720", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".ima", ".img", ".hfe"), FloppyDensity.DoubleDensity), D(F("ibm.800", "IBM PC", "IBM PC — 800 KiB", Ibm(), false, "PC-800", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".ima", ".img", ".hfe"), FloppyDensity.DoubleDensity),
            F("ibm.1200", "IBM PC", "IBM PC — 1.2 MiB", Ibm(), true, "PC-1200", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".ima", ".img", ".hfe"), D(F("ibm.1440", "IBM PC", "IBM PC — 1.44 MiB", Ibm(), true, "PC-1440", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".ima", ".img", ".hfe"), FloppyDensity.HighDensity),
            D(F("ibm.1680", "IBM PC", "IBM PC — 1.68 MiB", Ibm(), false, "PC-1680", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".ima", ".img", ".hfe"), FloppyDensity.HighDensity), D(F("ibm.dmf", "IBM PC", "IBM PC — DMF 1.68 MiB", Ibm(), false, "PC-DMF", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".ima", ".img", ".hfe"), FloppyDensity.HighDensity),
            D(F("ibm.2880", "IBM PC", "IBM PC — 2.88 MiB", Ibm(), false, "PC-2880", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".ima", ".img", ".hfe"), FloppyDensity.ExtendedDensity), F("ibm.scan", "IBM PC", "IBM PC — FM/MFM scan", Ibm(), false, "PC-SCAN", FloppyFormFactor.Unknown, true, true, ".scp", ".hfe"),
            F("commodore.1541", "Commodore", "Commodore 64 — 1541", [E(".d64", "Extension.d64", "Commodore D64 image", true)], true, "C64-1541", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".d64", ".hfe"),
            F("commodore.1571", "Commodore", "Commodore 128 — 1571", [E(".d71", "Extension.d71", "Commodore D71 image", true)], true, "C128-1571", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".d71", ".hfe"),
            D(F("commodore.1581", "Commodore", "Commodore 128 — 1581", [E(".d81", "Extension.d81", "Commodore D81 image", true)], true, "C128-1581", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".d81", ".hfe"), FloppyDensity.DoubleDensity),
            F("apple2.appledos.113", "Apple II", "Apple II DOS 3.2 — 113 KiB", [E(".d13", "Extension.d13", "Apple DOS 3.2 image", true)], false, "APPLE2-DOS32", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".d13", ".nib", ".woz", ".2mg", ".hfe"),
            F("apple2.appledos.140", "Apple II", "Apple II DOS 3.3 — 140 KiB", [E(".do", "Extension.do", "Apple DOS-order image", true), E(".dsk", "Extension.dsk.apple", "Apple disk image"), E(".nib", "Extension.nib", "Apple nibble image"), E(".woz", "Extension.woz", "Apple WOZ image")], true, "APPLE2-DOS33", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".do", ".dsk", ".nib", ".woz", ".2mg", ".hfe"),
            F("apple2.prodos.140", "Apple II", "Apple II ProDOS — 140 KiB", [E(".po", "Extension.po", "Apple ProDOS-order image", true), E(".2mg", "Extension.2mg", "Apple 2IMG image")], true, "APPLE2-PRODOS", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".po", ".do", ".dsk", ".nib", ".woz", ".2mg", ".hfe"),
            D(F("apple2.prodos.800", "Apple II", "Apple II ProDOS — 800 KiB", [E(".po", "Extension.po", "Apple ProDOS-order image", true), E(".2mg", "Extension.2mg", "Apple 2IMG image")], true, "APPLE2-PRODOS-800", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".po", ".2mg", ".image", ".hfe"), FloppyDensity.DoubleDensity),
            F("apple3.sos", "Apple III", "Apple III SOS — 140 KiB", [E(".po", "Extension.po", "Apple ProDOS-order image", true), E(".dsk", "Extension.dsk.apple", "Apple disk image")], false, "APPLE3-SOS", FloppyFormFactor.FiveAndQuarterInch, true, true, ".scp", ".po", ".do", ".dsk", ".2mg", ".hfe"),
            D(F("mac.400", "Apple Macintosh", "Apple Macintosh/Lisa GCR — 400 KiB", [E(".image", "Extension.image.apple", "Apple DiskCopy image", true), E(".img", "Extension.img.apple", "Apple raw disk image")], true, "MAC-400", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".image", ".dc42", ".img", ".hfe"), FloppyDensity.DoubleDensity),
            D(F("mac.800", "Apple Macintosh", "Apple Macintosh GCR — 800 KiB", [E(".image", "Extension.image.apple", "Apple DiskCopy image", true), E(".img", "Extension.img.apple", "Apple raw disk image")], true, "MAC-800", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".image", ".dc42", ".img", ".hfe"), FloppyDensity.DoubleDensity),
            D(F("mac.1440", "Apple Macintosh", "Apple Macintosh MFM — 1.44 MiB", [E(".img", "Extension.img.apple", "Apple raw disk image", true), E(".image", "Extension.image.apple", "Apple DiskCopy image")], true, "MAC-1440", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".image", ".dc42", ".img", ".hfe"), FloppyDensity.HighDensity),
            D(F("acorn.adfs.800", "Acorn", "Acorn — ADFS · 800 KiB", [E(".adf", "Extension.adf.acorn", "Acorn Disk File", true)], true, "ACORN-800", FloppyFormFactor.ThreeAndHalfInch, true, true, ".scp", ".adf", ".hfe"), FloppyDensity.DoubleDensity),
            new("acorn.dfs.atom", "Acorn Atom", "Acorn Atom DOS — 100 KiB", [new(".dsk", "Atom DSK", true)], false, Set(".dsk", ".scp", ".hfe"), "ACORN-ATOM-DOS", FloppyFormFactor.FiveAndQuarterInch, true, true),
            new("apricot.pcxi.315", "ACT Apricot", "ACT Apricot PC/Xi — ApriDisk 315 KiB", [new(".dsk", "ApriDisk", true)], false, Set(".dsk"), "APRICOT-PCXI-315", FloppyFormFactor.ThreeAndHalfInch, true, true, FloppyDensity.DoubleDensity),
            new("acorn.dfs.ss", "Acorn / BBC Micro", "BBC DFS — 100 KiB", [new(".ssd", "SSD", true)], true, Set(".scp", ".ssd", ".img", ".hfe"), "BBC-DFS-SS", FloppyFormFactor.FiveAndQuarterInch, true, true),
            new("acorn.dfs.ss80", "Acorn / BBC Micro", "BBC DFS — 200 KiB", [new(".ssd", "SSD", true)], true, Set(".scp", ".ssd", ".img", ".hfe"), "BBC-DFS-SS80", FloppyFormFactor.FiveAndQuarterInch, true, true),
            new("acorn.dfs.ds", "Acorn / BBC Micro", "BBC DFS — 200 KiB (DS)", [new(".dsd", "DSD", true)], true, Set(".scp", ".dsd", ".img", ".hfe"), "BBC-DFS-DS", FloppyFormFactor.FiveAndQuarterInch, true, true),
            new("acorn.dfs.ds80", "Acorn / BBC Micro", "BBC DFS — 400 KiB (DS)", [new(".dsd", "DSD", true)], true, Set(".scp", ".dsd", ".img", ".hfe"), "BBC-DFS-DS80", FloppyFormFactor.FiveAndQuarterInch, true, true),
            new("amstrad.cpc", "Amstrad", "Amstrad CPC — 3″", [new(".dsk", "DSK", true), new(".edsk", "Extended DSK")], true, Set(".scp", ".dsk", ".edsk", ".hfe"), "AMSTRAD-CPC", FloppyFormFactor.ThreeInch, true, true),
            new("amstrad.pcw", "Amstrad", "Amstrad PCW — 3″", [new(".dsk", "DSK", true), new(".edsk", "Extended DSK")], true, Set(".scp", ".dsk", ".edsk", ".hfe"), "AMSTRAD-PCW", FloppyFormFactor.ThreeInch, true, true),
            new("epson.qx10.320", "Epson QX-10", "Epson QX-10 — 320 KiB", [new(".img", "IMG", true), new(".imd", "IMD")], false, Set(".scp", ".img", ".imd", ".hfe"), "EPSON-QX10-320", FloppyFormFactor.FiveAndQuarterInch, true, true),
            new("epson.qx10.396", "Epson QX-10", "Epson QX-10 — 396 KiB", [new(".img", "IMG", true), new(".imd", "IMD")], false, Set(".scp", ".img", ".imd", ".hfe"), "EPSON-QX10-396", FloppyFormFactor.FiveAndQuarterInch, true, true),
            new("epson.qx10.399", "Epson QX-10", "Epson QX-10 — 399 KiB", [new(".img", "IMG", true), new(".imd", "IMD")], false, Set(".scp", ".img", ".imd", ".hfe"), "EPSON-QX10-399", FloppyFormFactor.FiveAndQuarterInch, true, true),
            new("epson.qx10.400", "Epson QX-10", "Epson QX-10 — 400 KiB", [new(".img", "IMG", true), new(".imd", "IMD")], false, Set(".scp", ".img", ".imd", ".hfe"), "EPSON-QX10-400", FloppyFormFactor.FiveAndQuarterInch, true, true),
            new("epson.qx10.logo", "Epson QX-10", "Epson QX-10 — Logo", [new(".img", "IMG", true), new(".imd", "IMD")], false, Set(".scp", ".img", ".imd", ".hfe"), "EPSON-QX10-LOGO", FloppyFormFactor.FiveAndQuarterInch, true, true),
            new("msx.1d", "MSX", "MSX — 180 KiB", [new(".dsk", "DSK", true)], true, Set(".scp", ".dsk", ".img", ".hfe"), "MSX-1D", FloppyFormFactor.ThreeAndHalfInch, true, true, FloppyDensity.DoubleDensity),
            new("msx.1dd", "MSX", "MSX — 360 KiB (SS)", [new(".dsk", "DSK", true)], true, Set(".scp", ".dsk", ".img", ".hfe"), "MSX-1DD", FloppyFormFactor.ThreeAndHalfInch, true, true, FloppyDensity.DoubleDensity),
            new("msx.2d", "MSX", "MSX — 360 KiB", [new(".dsk", "DSK", true)], true, Set(".scp", ".dsk", ".img", ".hfe"), "MSX-2D", FloppyFormFactor.ThreeAndHalfInch, true, true, FloppyDensity.DoubleDensity),
            new("msx.2dd", "MSX", "MSX — 720 KiB", [new(".dsk", "DSK", true)], true, Set(".scp", ".dsk", ".img", ".hfe"), "MSX-2DD", FloppyFormFactor.ThreeAndHalfInch, true, true, FloppyDensity.DoubleDensity),
            new("dec.rx02", "DEC", "DEC RX02 — 512 KiB", [new(".img", "IMG", true)], false, Set(".scp", ".img", ".imd", ".td0", ".hfe"), "DEC-RX02", FloppyFormFactor.EightInch, true, true),
            F("ucsd.ibm.mfm", "UCSD p-System", "UCSD p-System — IBM MFM", [new(".img", "IMG", true), new(".td0", "TD0")], false, "UCSD-IBM-MFM", FloppyFormFactor.Unknown, true, true, ".scp", ".img", ".td0", ".hfe"),
            new("commodore900.coherent", "Commodore 900", "Commodore 900 — COHERENT", [new(".bin", "BIN", true), new(".img", "IMG")], false, Set(".scp", ".bin", ".img", ".hfe"), "C900-COHERENT", FloppyFormFactor.Unknown, true, true),
            new("applelisa.office", "Apple Lisa", "Apple Lisa Office System", [new(".image", "DiskCopy", true), new(".dc42", "DiskCopy 4.2")], false, Set(".scp", ".image", ".dc42", ".hfe"), "LISA-OFFICE", FloppyFormFactor.ThreeAndHalfInch, true, true, FloppyDensity.DoubleDensity),
            new("applelisa.macworks", "Apple Lisa", "Apple Lisa — MacWorks", [new(".image", "DiskCopy", true), new(".dc42", "DiskCopy 4.2")], false, Set(".scp", ".image", ".dc42", ".hfe"), "LISA-MACWORKS", FloppyFormFactor.ThreeAndHalfInch, true, true, FloppyDensity.DoubleDensity),
            F("raw.hfe", "Raw", "HxC flux image", [E(".hfe", "Extension.hfe", "HxC Floppy Emulator", true)], false, "RAW-HFE", FloppyFormFactor.Unknown, true, true, ".scp", ".hfe")
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
