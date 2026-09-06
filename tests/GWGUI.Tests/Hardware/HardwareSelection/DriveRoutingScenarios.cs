using GWGUI.Domain.Hardware;
using GWGUI.Domain.Settings.Hardware;
namespace GWGUI.Tests.Hardware.HardwareSelection;
internal static class DriveRoutingScenarios
{
    public static void Selection(int change)
    {
        var settings = new GWGUI.Domain.Settings.AppSettings();
        var first = new ControllerSettings { UsbId="ONE", LastPort="virtual1", IsAvailable=true };
        var second = new ControllerSettings { UsbId="two", LastPort="virtual2", IsAvailable=true };
        var a = new DriveSettings { Id=Guid.NewGuid().ToString(), ControllerUsbId="one", Selection="A" };
        var b = new DriveSettings { Id=Guid.NewGuid().ToString(), ControllerUsbId="two", Selection="B" };
        settings.Controllers.AddRange([first,second]); settings.Drives.AddRange([a,b]);
        var bar = new GWGUI.App.Views.Controls.Shell.ApplicationStatusBar();
        var messages = 0; var enabled = false; var changes = 0;
        var dialogs = GWGUI.Tests.Application.TestInfrastructure.ControlledDependencies.Simulate<GWGUI.App.Interfaces.Services.Dialogs.IMessageDialogService>((method,args)=>
        {
            Assert.Equal("Show", method.Name); Assert.Equal("Hardware.SelectedDisconnected", args![0]); messages++;
            return GWGUI.App.Enums.Services.Dialogs.UserDialogResult.Ok;
        });
        var controller = new GWGUI.App.Services.Hardware.HardwareSelectionController(bar,new("synthetic","synthetic"),()=>settings,dialogs,value=>enabled=value,()=>changes++,(key,args)=>key);
        controller.Refresh(); Assert.Equal(2,bar.HardwareChoices.Items.Count); Assert.True(enabled);
        bar.HardwareChoices.SelectedIndex=1; controller.OnSelectionChanged(); Assert.Equal(1,changes);
        Assert.Equal("virtual2",controller.DeviceArgument()); Assert.True(controller.EnsureAvailable());
        controller.Refresh(); Assert.Equal(b.Id,controller.Selected!.Drive.Id);
        if(change==0) second.IsAvailable=false;
        else if(change==1) settings.Drives.Remove(b);
        else settings.Controllers.Remove(second);
        Assert.False(controller.EnsureAvailable()); Assert.Equal(1,messages);
        controller.Refresh();
        if(change==0) { Assert.Equal(b.Id,controller.Selected!.Drive.Id); Assert.False(enabled); }
        else { Assert.Equal(a.Id,controller.Selected!.Drive.Id); Assert.True(enabled); Assert.Null(controller.DeviceArgument()); }
        settings.Controllers.Clear(); settings.Drives.Clear(); controller.Refresh();
        Assert.Null(controller.Selected); Assert.Null(controller.DeviceArgument()); Assert.Null(controller.DriveArgument());
        Assert.True(controller.EnsureAvailable());
    }
    public static void Route()
    {
        var first=new ControllerSettings{UsbId="one",LastPort="virtual1",IsAvailable=true};
        var second=new ControllerSettings{UsbId="two",LastPort="virtual2",IsAvailable=true};
        var a=new DriveSettings{ControllerUsbId="one",Selection="A"};
        var b=new DriveSettings{ControllerUsbId="one",Selection="B"};
        var c=new DriveSettings{ControllerUsbId="two",Selection="A"};
        Assert.Equal("virtual1",HardwareRoutingPolicy.DeviceArgument([first,second],[a,b,c],b));
        Assert.Equal("B",HardwareRoutingPolicy.DriveArgument([a,b,c],b));
        Assert.Null(HardwareRoutingPolicy.DriveArgument([a,b,c],c));
        second.IsAvailable=false;
        Assert.Null(HardwareRoutingPolicy.DeviceArgument([first,second],[a,b,c],a));
        Assert.Null(HardwareRoutingPolicy.DeviceArgument([first],[a],null));
    }
    public static void Assign()
    {
        var a=new DriveSettings{ControllerUsbId="ONE"};
        var b=new DriveSettings{ControllerUsbId="one"};
        var other=new DriveSettings{ControllerUsbId="other",Selection="kept"};
        HardwareRoutingPolicy.AssignAutomaticDriveSelections(new[]{a,b,other},"one");
        Assert.Equal("A",a.Selection);Assert.Equal("B",b.Selection);Assert.Equal("kept",other.Selection);
        Assert.Throws<ArgumentException>(()=>HardwareRoutingPolicy.AssignAutomaticDriveSelections(new[]{a,b,new DriveSettings{ControllerUsbId="one"}},"one"));
    }
}
