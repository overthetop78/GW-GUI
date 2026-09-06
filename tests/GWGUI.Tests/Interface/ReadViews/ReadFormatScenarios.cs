using GWGUI.App.ViewModels.Operations;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Read;
using GWGUI.App.Controllers.MainWindow;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Read;
using GWGUI.Domain.Commands.Building;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Windows;
using System.Windows.Controls;
namespace GWGUI.Tests.Interface.ReadViews;
internal static class ReadFormatScenarios
{
    internal sealed record SelectionContext(ReadTabSection View,MainWindowViewModel Model,ReadTabController Controller,List<ReadRequest> Requests);

    internal static SelectionContext CreateSelectionContext(string? folderResponse=null)
    {
        var view=new ReadTabSection();
        var model=new MainWindowViewModel("synthetic","synthetic");
        view.DataContext=model;
        var settings=new AppSettings();settings.Engines.PhysicalRead=OperationEngine.Internal;
        var formats=new[]{new DiskFormat("test.one","family-one","one",[new(".one","one"),new(".two","two",true)]),new DiskFormat("test.other","family-other","other",[new(".other","other",true)])};
        var catalog=ControlledDependencies.Simulate<IImageFormatCatalog>((method,_)=>method.Name=="get_Formats"?formats:throw new InvalidOperationException(method.Name));
        var requests=new List<ReadRequest>();
        var builder=ControlledDependencies.Simulate<IGwCommandBuilder>((method,args)=>{
            Assert.Equal("BuildRead",method.Name);var request=(ReadRequest)args[0]!;requests.Add(request);return ReadCommandBuilder.Build(request);
        });
        var dialogs=ControlledDependencies.Simulate<IFileDialogService>((method,_)=>{
            Assert.Equal("SelectFolder",method.Name);return folderResponse;
        });
        var controller=new ReadTabController(view,model,null!,()=>catalog,()=>settings,builder,dialogs,
            ControlledDependencies.Reject<IBusinessDialogService>(),ControlledDependencies.Reject<IMessageDialogService>(),
            null!,null!,null!,null!,null!,null!,new TextBox(),new TextBox(),()=>"virtual-device",()=>"B",()=>true,()=>null,()=>{},()=>{},()=>{});
        return new(view,model,controller,requests);
    }

    public static void Selection()
    {
        var context=CreateSelectionContext();
        var image=context.View.ImageBlock;
        Assert.Equal(".scp",context.Controller.GetExtension());
        image.RawScpRadio.IsChecked=false;image.KnownFormatRadio.IsChecked=true;
        image.FamilyCombo.ItemsSource=new[]{"family-one","family-other"};image.FamilyCombo.SelectedIndex=0;
        context.Controller.FamilyChanged();context.Controller.FormatChanged();context.Controller.ModeChanged();
        Assert.Equal(Visibility.Visible,image.KnownFormatPanel.Visibility);
        Assert.Equal("test.one",Assert.IsType<DiskFormat>(image.FormatCombo.SelectedItem).Id);
        Assert.Equal(".two",context.Controller.GetExtension());
        context.Controller.BuildCommand("virtual-output.two");
        var request=Assert.Single(context.Requests);
        Assert.Equal(ReadResultKind.KnownFormat,request.ResultKind);Assert.Equal("test.one",request.FormatId);
        Assert.Equal("virtual-output.two",request.DestinationPath);Assert.Equal("virtual-device",request.Device);Assert.Equal("B",request.Drive);
        image.FamilyCombo.SelectedIndex=1;context.Controller.FamilyChanged();context.Controller.FormatChanged();
        Assert.Equal(".other",context.Controller.GetExtension());
        image.RawScpRadio.IsChecked=true;image.KnownFormatRadio.IsChecked=false;context.Controller.ModeChanged();
        Assert.Equal(Visibility.Collapsed,image.KnownFormatPanel.Visibility);Assert.Equal(".scp",context.Controller.GetExtension());
    }
    public static void Options()
    {
        var model=new ReadOperationViewModel();
        model.EnableFakeIndex();
        Assert.True(model.FakeIndex.Enabled);Assert.False(model.HardSectors.Enabled);
        model.EnableHardSectors();
        Assert.False(model.FakeIndex.Enabled);Assert.True(model.HardSectors.Enabled);
        model.EnableDensel();
        model.EnableTg43();
        Assert.False(model.Densel.Enabled);Assert.True(model.Tg43.Enabled);
        model.Revs.Enabled=true;model.Revs.Value="3";
        Assert.Equal("3",model.BuildOptions().Single(x=>x.Argument=="--revs").Value);
        model.ExpertArguments="synthetic";
        model.ResetOptionalSettings();
        Assert.Empty(model.BuildOptions());Assert.Empty(model.ExpertArguments);
    }
}
