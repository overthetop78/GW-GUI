using System.Reflection;
using GWGUI.MediaEngine.Recognition.Definitions;

namespace GWGUI.Tests;

/// <summary>Vérifie le contrat public des identifiants de formats d’images disque.</summary>
public sealed class DiskImageFormatIdsTests
{
    [Fact]
    public void FixedIdentifiersAndPrefixesKeepTheirPublicValues()
    {
        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            "unknown", "imd", "td0",
            "acorn.adfs.", "acorn.adfs.800", "acorn.dfs.", "acorn.dfs.ss", "acorn.dfs.ss80",
            "acorn.dfs.ds", "acorn.dfs.ds80",
            "amiga.", "amiga.amigados", "amiga.amigados_hd",
            "amstrad.", "amstrad.cpc", "amstrad.pcw",
            "apple2.", "apple2.appledos", "apple2.appledos.140", "apple2.dos", "apple2.dos32",
            "apple2.dos33", "apple2.gcr", "apple2.nofs", "apple2.prodos", "apple2.prodos.140",
            "apple2.prodos.800", "apple2.rwts18", "apple3.", "apple3.sos",
            "applelisa.", "applelisa.macworks", "applelisa.office", "applelisa.raw",
            "applemac.", "applemac.gcr", "applemac.hfs", "applemac.mfs",
            "atari.", "atari.90", "atari.130", "atari.180",
            "atarist.", "atarist.180", "atarist.360", "atarist.400", "atarist.440", "atarist.720",
            "atarist.800", "atarist.810", "atarist.880", "atarist.1440",
            "commodore.", "commodore.1541", "commodore.1571", "commodore.1581",
            "commodore900.", "commodore900.coherent", "dec.rx02",
            "epson.qx10.", "epson.qx10.320", "epson.qx10.396", "epson.qx10.399", "epson.qx10.400",
            "epson.qx10.booter", "epson.qx10.logo",
            "ibm.", "ibm.160", "ibm.180", "ibm.320", "ibm.360", "ibm.720", "ibm.800", "ibm.1200",
            "ibm.1440", "ibm.1680", "ibm.dmf", "ibm.2880", "ibm.scan",
            "mac.", "mac.400", "mac.800", "mac.1440",
            "msx.", "msx.1d", "msx.1dd", "msx.2d", "msx.2dd",
            "ucsd.", "ucsd.ibm.mfm"
        };
        var actual = typeof(DiskImageFormatIds)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(expected.SetEquals(actual),
            $"Attendu seulement: {string.Join(", ", expected.Except(actual))}; obtenu seulement: {string.Join(", ", actual.Except(expected))}");
    }

    [Fact]
    public void BuildsIdentifiersProducedFromReaderCapacitiesAndGeometries()
    {
        Assert.Equal(DiskImageFormatIds.AtariSt720,
            DiskImageFormatIds.AtariStFromCapacity(80L * 2 * 9 * 512));
        Assert.Equal(DiskImageFormatIds.Ibm1440,
            DiskImageFormatIds.IbmFromCapacity(80L * 2 * 18 * 512));
        Assert.Equal("atari.atr.512.1440", DiskImageFormatIds.AtariAtr(512, 1440));
        Assert.Equal("atari.scp.256.18", DiskImageFormatIds.AtariScp(256, 18));
    }
}
