using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
namespace GWGUI.Tests.Emulation.EmulationContracts;
internal static class MediaAndSavedStateScenarios
{
    public static async Task States(bool supported, bool fail)
    {
        var files=new Dictionary<string,byte[]>(); var folders=new List<string>(); var calls=new List<string>();
        byte[] live=[1,2,3]; var error=fail; var selected=0;
        var services=Enumerable.Range(0,2).Select(index=>GWGUI.Tests.Application.TestInfrastructure.ControlledDependencies.Simulate<GWGUI.Emulation.Interfaces.IEmulationSavedStates>((method,args)=>
        {
            if(method.Name=="get_IsSupported") return supported;
            var path=(string)args[0]!; ((CancellationToken)args[1]!).ThrowIfCancellationRequested(); calls.Add(index+":"+method.Name);
            if(error) return ValueTask.FromException(new IOException("synthetic state failure"));
            if(method.Name=="SaveAsync") files[path]=live.ToArray();
            else if(method.Name=="LoadAsync") live=files[path].ToArray();
            else throw new InvalidOperationException(method.Name);
            return ValueTask.CompletedTask;
        })).ToArray();
        var states=new GWGUI.App.Services.Emulation.MachineQuickStates(()=>services[selected],"virtual/state",files.ContainsKey,folders.Add);
        Assert.False(states.IsAvailable); Assert.False(await states.LoadAsync()); Assert.Empty(calls);
        if(!supported) { Assert.False(await states.SaveAsync()); Assert.Empty(folders); Assert.Empty(files); return; }
        if(fail) { await Assert.ThrowsAsync<IOException>(()=>states.SaveAsync()); Assert.False(states.IsAvailable); error=false; }
        Assert.True(await states.SaveAsync()); Assert.Equal(new byte[]{1,2,3},files["virtual/state"]); Assert.True(states.IsAvailable);
        live=[9]; selected=1;
        if(fail) { error=true; await Assert.ThrowsAsync<IOException>(()=>states.LoadAsync()); Assert.Equal(new byte[]{9},live); error=false; }
        Assert.True(await states.LoadAsync()); Assert.Equal(new byte[]{1,2,3},live); Assert.Equal("1:LoadAsync",calls[^1]);
        using var cancel=new CancellationTokenSource(); cancel.Cancel(); var count=calls.Count;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>states.SaveAsync(cancel.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>states.LoadAsync(cancel.Token)); Assert.Equal(count,calls.Count);
        Assert.All(folders,folder=>Assert.Equal("virtual",folder));
    }
    public static void Media()
    {
        var first=new EmulationMedia("virtual-a",EmulationMediaSlot.Floppy0,EmulationMediaType.Floppy,false,true);
        var second=new EmulationMedia("virtual-b",EmulationMediaSlot.Floppy1,EmulationMediaType.Floppy,false,true);
        var replacement=first with{Path="virtual-c"};
        var result=EmulationMediaRules.Replace([first,second],replacement);
        Assert.Contains(second,result);Assert.Contains(replacement,result);Assert.DoesNotContain(first,result);
        var ejected=EmulationMediaRules.Eject(result,EmulationMediaSlot.Floppy0);
        Assert.False(ejected.Single(x=>x.Slot==EmulationMediaSlot.Floppy0).IsInserted);
        Assert.True(ejected.Single(x=>x.Slot==EmulationMediaSlot.Floppy1).IsInserted);
        var bytes=EmulationMediaProtocol.Serialize(ejected);
        var restored=EmulationMediaProtocol.Deserialize(bytes);
        Assert.Equal(ejected,restored);
        Assert.Equal("virtual-c",restored.Single(x=>x.Slot==EmulationMediaSlot.Floppy0).Path);
    }
    public static void Invalid()
    {
        var floppy=new EmulationMedia("virtual",EmulationMediaSlot.Floppy0,EmulationMediaType.Floppy,false,true);
        Assert.Throws<ArgumentException>(()=>EmulationMediaRules.Validate([floppy,floppy]));
        Assert.Throws<ArgumentException>(()=>EmulationMediaRules.Validate([floppy with{Type=EmulationMediaType.CompactDisc}]));
        Assert.Throws<ArgumentException>(()=>EmulationMediaRules.Validate([floppy with{Path=""}]));
        var cd=floppy with {Slot=EmulationMediaSlot.Cd0,Type=EmulationMediaType.CompactDisc};
        Assert.Throws<ArgumentException>(()=>EmulationMediaRules.Validate([cd]));
        Assert.Single(EmulationMediaRules.Validate([cd with{IsReadOnly=true}]));
        var disk=floppy with {Slot=EmulationMediaSlot.HardDisk0,Type=EmulationMediaType.HardDisk};
        Assert.Throws<InvalidOperationException>(()=>EmulationMediaRules.Eject([disk],disk.Slot));
    }
}
