using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using System.Text;
namespace GWGUI.Tests.Media.ImageContainers;
internal static class AmstradContainerScenarios
{
    public static async Task Invalid(int damage)
    {
        var bytes=new byte[768]; "EXTENDED CPC DSK File\r\nDisk-Info\r\n"u8.CopyTo(bytes); bytes[48]=bytes[49]=1; bytes[52]=2;
        "Track-Info\r\n"u8.CopyTo(bytes.AsSpan(256)); bytes[277]=1; bytes[282]=1; bytes[286]=128;
        if(damage==0) bytes[0]=0;
        if(damage==1) bytes[48]=0;
        if(damage==2) bytes[49]=3;
        if(damage==3) bytes[256]=0;
        if(damage==4) bytes[52]=10;
        if(damage==5) { bytes[286]=255; bytes[287]=255; }
        await Assert.ThrowsAsync<InvalidDataException>(()=>new CpcDskReader().ReadDetailedAsync(bytes.AsMemory()));
    }

    public static async Task Read(bool extended)
    {
        var bytes = new byte[768];
        Encoding.ASCII.GetBytes(extended ? "EXTENDED CPC DSK File\r\nDisk-Info\r\n" : "MV - CPCEMU Disk-File\r\nDisk-Info\r\n").CopyTo(bytes, 0);
        bytes[48] = bytes[49] = 1; bytes[51] = 2; bytes[52] = 2;
        "Track-Info\r\n"u8.CopyTo(bytes.AsSpan(256)); bytes[277] = 1; bytes[282] = 7; bytes[286] = 128;
        bytes[512] = 42; bytes[639] = 93;
        var reader = new CpcDskReader(); var result = await reader.ReadDetailedAsync(bytes.AsMemory());
        Assert.Equal(extended ? CpcDskContainerKind.Extended : CpcDskContainerKind.Standard, result.Kind);
        var block = Assert.Single(result.SectorImage.AvailableBlocks);
        Assert.Equal(128, block.Data.Count); Assert.Equal(42, block.Data[0]); Assert.Equal(93, block.Data[^1]);
        Assert.Equal(7, block.Address.Number);
        var files = new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles();
        var writer = new CpcDskWriter(files); await writer.WriteAsync(result,"output");
        var written = files.Files["output"];
        Assert.Equal(extended?768:640,written.Length); Assert.Equal(1,written[48]); Assert.Equal(1,written[49]);
        Assert.Equal(7,written[282]); Assert.Equal(42,written[512]); Assert.Equal(93,written[639]);
        Assert.Equal(extended?2:128,written[extended?52:50]);
        var decoded = await reader.ReadDetailedAsync(written.AsMemory());
        Assert.Equal(block.Data,Assert.Single(decoded.SectorImage.AvailableBlocks).Data);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>writer.WriteAsync(result,"cancelled",new CancellationToken(true)));
        using var source = new CancellationTokenSource(); source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reader.ReadDetailedAsync(bytes.AsMemory(), source.Token));
        bytes[277] = 40;
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadDetailedAsync(bytes.AsMemory()));
    }
}
