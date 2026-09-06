using GWGUI.Infrastructure.Hardware.Greaseweazle;
namespace GWGUI.Tests.Hardware.GreaseweazleProtocol;
internal static class ProtocolFramesScenarios
{
    public static async Task Firmware(int failure)
    {
        var transport=new Transport { FirmwareMinor=failure==1?(byte)30:(byte)31,MainFirmware=failure==2?(byte)0:(byte)1 };
        var client=new GreaseweazleProtocolClient(transport);
        if(failure!=0) { await Assert.ThrowsAsync<InvalidOperationException>(()=>client.OpenAsync("virtual-port").AsTask()); Assert.False(transport.IsOpen); Assert.Equal(1,transport.Closes); }
        else
        {
            var info=await client.OpenAsync("virtual-port"); Assert.Equal(new Version(0,31),info.Version); Assert.Equal(24000000u,info.SampleFrequency); Assert.True(info.IsMainFirmware);
            await client.CloseAsync(); Assert.Null(client.Firmware);
        }
        Assert.Equal(new[]{"virtual-port:9600","10000","9600","discard"},transport.Setup); Assert.Equal(new byte[]{0,3,0},Assert.Single(transport.Commands));
    }
    public static async Task Flux(bool write,int failure)
    {
        var transport=new Transport(); await using var client=new GreaseweazleProtocolClient(transport,maximumFluxUnderflowRetries:1);
        await client.OpenAsync("virtual"); await client.SelectDriveAsync(0); await client.SetMotorAsync(true); transport.Commands.Clear();
        transport.FluxError=write?(byte)5:(byte)4; transport.FluxFailures=failure==1?1:failure==2?2:0;
        for(var attempt=0;attempt<2;attempt++) { transport.Chunks.Enqueue([1,249]); transport.Chunks.Enqueue(failure==3?[]:[250,1,0]); }
        async Task Run()
        {
            if(write) await client.WriteFluxAsync(new uint[]{1,249,250},true,false,0x12345678);
            else
            {
                var capture=await client.ReadFluxAsync(2,0x12345678,retries:1);
                Assert.Equal(new uint[]{1,249,250},capture.FluxIntervals); Assert.Equal(24000000u,capture.SampleFrequency);
            }
        }
        if(failure==2) { var error=await Assert.ThrowsAsync<GreaseweazleProtocolException>(Run); Assert.Equal(transport.FluxError,(byte)error.Acknowledgement); }
        else if(failure==3) await Assert.ThrowsAsync<EndOfStreamException>(Run);
        else await Run();
        var count=failure is 1 or 2?2:1; Assert.Equal(count,transport.Commands.Count(command=>command[0]==(write?8:7)));
        var expected=write?new byte[]{8,8,1,0,120,86,52,18}:new byte[]{7,8,120,86,52,18,3,0};
        Assert.All(transport.Commands.Where(command=>command[0]==(write?8:7)),command=>Assert.Equal(expected,command));
        if(write) Assert.All(transport.Commands.Where(command=>command[0]==1),stream=>Assert.Equal(new byte[]{1,249,250,1,255,2,207,33,1,1,249,0},stream));
        var previous=transport.Commands.Count; using var cancel=new CancellationTokenSource(); cancel.Cancel();
        if(write) await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>client.WriteFluxAsync(new uint[]{1},false,false,cancellationToken:cancel.Token).AsTask());
        else await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>client.ReadFluxAsync(1,cancellationToken:cancel.Token).AsTask());
        Assert.Equal(previous,transport.Commands.Count);
        await client.ResetAsync(); await Assert.ThrowsAsync<InvalidOperationException>(()=>client.SetMotorAsync(true).AsTask());
    }
    private sealed class Transport:IGreaseweazleSerialTransport
    {
        public bool IsOpen{get;private set;}=true;
        public List<byte[]> Commands{get;}=[];
        public byte Acknowledgement,WrongCommand;
        public int Closes;
        public List<string> Setup=[];
        public byte FirmwareMinor=31, MainFirmware=1;
        public int FluxFailures;
        public byte FluxError;
        public Queue<byte[]> Chunks=[];
        public ValueTask OpenAsync(string portName,int baudRate,CancellationToken cancellationToken=default){cancellationToken.ThrowIfCancellationRequested(); Setup.Add(portName+":"+baudRate);IsOpen=true;return ValueTask.CompletedTask;}
        public ValueTask SetBaudRateAsync(int baudRate,CancellationToken cancellationToken=default){Setup.Add(baudRate.ToString()); return ValueTask.CompletedTask;}
        public ValueTask DiscardBuffersAsync(CancellationToken cancellationToken=default){Setup.Add("discard"); return ValueTask.CompletedTask;}
        public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,CancellationToken cancellationToken=default)
        {cancellationToken.ThrowIfCancellationRequested();Commands.Add(buffer.ToArray());return ValueTask.CompletedTask;}
        public ValueTask<int> ReadAsync(Memory<byte> buffer,CancellationToken cancellationToken=default)
        { cancellationToken.ThrowIfCancellationRequested(); var bytes=Chunks.Dequeue(); bytes.CopyTo(buffer); return ValueTask.FromResult(bytes.Length); }
        public ValueTask ReadExactlyAsync(Memory<byte> buffer,CancellationToken cancellationToken=default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(buffer.Length==32) { buffer.Span.Clear(); buffer.Span[1]=FirmwareMinor; buffer.Span[2]=MainFirmware; new byte[]{0,54,110,1}.CopyTo(buffer[4..]); return ValueTask.CompletedTask; }
            if(buffer.Length==1) { buffer.Span[0]=0; return ValueTask.CompletedTask; }
            Assert.Equal(2,buffer.Length);
            buffer.Span[0]=WrongCommand==0?Commands[^1][0]:WrongCommand;
            buffer.Span[1]=Commands[^1][0]==9 && FluxFailures-->0?FluxError:Acknowledgement;return ValueTask.CompletedTask;
        }
        public ValueTask CloseAsync(CancellationToken cancellationToken=default){IsOpen=false;Closes++;return ValueTask.CompletedTask;}
        public ValueTask DisposeAsync()=>ValueTask.CompletedTask;
    }
    public static async Task Commands()
    {
        var transport=new Transport();
        var client=new GreaseweazleProtocolClient(transport);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.SetMotorAsync(true).AsTask());
        Assert.Empty(transport.Commands);
        await client.SetBusTypeAsync(GreaseweazleBusType.IbmPc);
        await client.SelectDriveAsync(1);
        await client.SetMotorAsync(true);
        await client.SeekAsync(300,1);
        await client.CloseAsync();
        byte[][] expected=[[14,3,1],[12,3,1],[6,4,1,1],[2,4,44,1],[3,3,1],[6,4,1,0],[13,2]];
        Assert.Equal(expected.Length,transport.Commands.Count);
        for(var i=0;i<expected.Length;i++)Assert.Equal(expected[i],transport.Commands[i]);
        Assert.Equal(1,transport.Closes);
    }
    public static async Task Errors()
    {
        var transport=new Transport{Acknowledgement=1};
        var client=new GreaseweazleProtocolClient(transport);
        await Assert.ThrowsAsync<GreaseweazleProtocolException>(()=>client.SelectDriveAsync(0).AsTask());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.SetMotorAsync(true).AsTask());
        transport.Acknowledgement=0;transport.WrongCommand=99;
        await Assert.ThrowsAsync<IOException>(()=>client.SelectDriveAsync(0).AsTask());
        Assert.Equal(2,transport.Commands.Count);
    }
}
