using GWGUI.App.Localization.Sources;
using GWGUI.App.ViewModels.Conversion;
using GWGUI.App.ViewModels.Input;
using GWGUI.App.ViewModels.Operations;
using GWGUI.App.ViewModels.Operations.Options;

namespace GWGUI.Tests.Interface;

public sealed class NotificationTests
{
    [Fact]
    public void OperationPropertiesNotifyTheirOwnerOnlyWhenTheirValueChanges()
    {
        var write = new WriteOperationViewModel();
        var conversion = new ConversionOperationViewModel();
        var changes = new List<(object? Sender, string? Name)>();
        write.PropertyChanged += (sender, args) => changes.Add((sender, args.PropertyName));
        conversion.PropertyChanged += (sender, args) => changes.Add((sender, args.PropertyName));

        write.SourcePath = "source";
        write.SourcePath = new string("source".ToCharArray());
        conversion.AddTags = true;
        conversion.AddTags = true;

        Assert.Equal(2, changes.Count);
        Assert.Same(write, changes[0].Sender);
        Assert.Equal(nameof(WriteOperationViewModel.SourcePath), changes[0].Name);
        Assert.Same(conversion, changes[1].Sender);
        Assert.Equal(nameof(ConversionOperationViewModel.AddTags), changes[1].Name);
    }

    [Fact]
    public void ReadNotifiesSequenceKindAfterItsIndexOnlyWhenTheIndexChanges()
    {
        var read = new ReadOperationViewModel();
        var changes = new List<string?>();
        read.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

        read.SequenceKindIndex = 1;
        read.SequenceKindIndex = 1;

        Assert.Equal(new[] { nameof(ReadOperationViewModel.SequenceKindIndex), nameof(ReadOperationViewModel.SequenceKind) }, changes);
    }

    [Fact]
    public void OptionFlagsAndValuesKeepTheirOwnPropertyNames()
    {
        var option = new ValueOptionViewModel("--test", "initial");
        var changes = new List<string?>();
        option.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

        option.Enabled = true;
        option.Enabled = true;
        option.Value = "changed";
        option.Value = new string("changed".ToCharArray());

        Assert.Equal(new[] { nameof(option.Enabled), nameof(option.Value) }, changes);
    }

    [Fact]
    public void BindingKeepsItsDependentNotificationsEvenForTheSameValue()
    {
        var row = new InputBindingRow("test", "Test", "A", "A");
        var changes = new List<string?>();
        row.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

        row.Binding = "B";
        row.Binding = "B";

        string[] expected = [nameof(row.Binding), nameof(row.BindingParts), nameof(row.ControllerDeviceName), nameof(row.ControllerDeviceNameVisibility)];
        Assert.Equal(expected.Concat(expected), changes);
    }

    [Fact]
    public void LocalizationRefreshKeepsVersionThenIndexerNotifications()
    {
        var source = new LocalizationSource();
        var changes = new List<string?>();
        var version = source.Version;
        source.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

        source.Refresh();

        Assert.Equal(version + 1, source.Version);
        Assert.Equal(new[] { nameof(source.Version), "Item[]" }, changes);
    }
}
