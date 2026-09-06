using GWGUI.Domain.Formats.Parsing;
namespace GWGUI.Tests.Hardware.CommandsAndParsing;
internal static class ExternalOutputParsingScenarios
{
    public static void Information(bool malformed)
    {
        var text=malformed?"Model without colon\nPortability: unknown\nCommand failed\nFirmwareVersion: wrong":
            " host tools: 1.2 \r\nCOM42\r\n Model: GW V4 \nMCU: STM32\nFirmware: 0.31\nSerial: serial\nUSB: Full Speed\nGitHub request failed";
        var result=GWGUI.Domain.Hardware.Parsing.GwInfoParser.Parse(text); Assert.Equal(text,result.RawOutput);
        if(malformed) { Assert.Null(result.Model); Assert.Null(result.Port); Assert.Null(result.FirmwareVersion); Assert.False(result.HasNetworkWarning); }
        else { Assert.Equal("1.2",result.HostToolsVersion); Assert.Equal("COM42",result.Port); Assert.Equal("GW V4",result.Model); Assert.Equal("STM32",result.Mcu); Assert.Equal("0.31",result.FirmwareVersion); Assert.Equal("serial",result.SerialNumber); Assert.Equal("Full Speed",result.UsbSpeed); Assert.True(result.HasNetworkWarning); }
        var empty=GWGUI.Domain.Hardware.Parsing.GwInfoParser.Parse(""); Assert.Null(empty.Model); Assert.Null(empty.Port); Assert.False(empty.HasNetworkWarning);
    }
    public static void Parse()
    {
        var result=GwFormatCapabilitiesParser.ParseReadHelp("ignored.fake .bad\nFORMAT options: amiga.amigados ibm.1440 AMIGA.AMIGADOS\nSupported file suffixes: .scp .hfe .SCP");
        Assert.Equal(new[]{"amiga.amigados","ibm.1440"},result.FormatIds.Order());
        Assert.Equal(new[]{".hfe",".scp"},result.ImageExtensions.Order());
        Assert.True(result.IsKnown);
        Assert.False(GwFormatCapabilitiesParser.ParseReadHelp("partial output without sections").IsKnown);
        Assert.False(GwFormatCapabilitiesParser.ParseReadHelp(null).IsKnown);
    }
}
