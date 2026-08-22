using GWGUI.Emulation.Atari.Cores;
using GWGUI.Emulation.Common;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GWGUI.Tests;

public sealed class AtariDiskControlTests
{
    [Fact]
    public void ExtendedControl_ReportsOrderedPathsLabelsAndCurrentIndex()
    {
        using var harness = new DiskControlHarness(["first.st", "second.st"], ["Disk one", "Disk two"]);
        var control = harness.CaptureExtended();
        harness.CurrentIndex = 1;

        var status = control.GetStatus();

        Assert.Equal(2, status.ImageCount);
        Assert.Equal(1, status.CurrentIndex);
        Assert.False(status.IsEjected);
        Assert.Equal(["first.st", "second.st"], status.Images.Select(image => image.Path));
        Assert.Equal(["Disk one", "Disk two"], status.Images.Select(image => image.Label));
    }

    [Fact]
    public void Select_EjectsChangesIndexAndReinserts()
    {
        using var harness = new DiskControlHarness(["first.st", "second.st"], ["First", "Second"]);
        var control = harness.CaptureBasic();

        control.Select(1);

        Assert.Equal(1u, harness.CurrentIndex);
        Assert.False(harness.Ejected);
        Assert.Equal([true, false], harness.EjectTransitions);
    }

    [Fact]
    public void RepeatedRotation_PreservesImageOrder()
    {
        using var harness = new DiskControlHarness(["first.st", "second.st", "third.st"],
            ["First", "Second", "Third"]);
        var control = harness.CaptureExtended();

        control.Select(1);
        control.Select(2);
        control.Select(0);
        var status = control.GetStatus();

        Assert.Equal(0, status.CurrentIndex);
        Assert.Equal(["first.st", "second.st", "third.st"], status.Images.Select(image => image.Path));
    }

    [Fact]
    public void Select_RestoresPreviousStateWhenSelectionFails()
    {
        using var harness = new DiskControlHarness(["first.st", "second.st"], ["First", "Second"])
        {
            RejectIndex = 1
        };
        var control = harness.CaptureBasic();

        Assert.Throws<InvalidOperationException>(() => control.Select(1));

        Assert.Equal(0u, harness.CurrentIndex);
        Assert.False(harness.Ejected);
    }

    [Fact]
    public void Insert_RejectsMissingMediaWithoutChangingControl()
    {
        using var harness = new DiskControlHarness(["first.st"], ["First"]);
        var control = harness.CaptureBasic();

        Assert.Throws<FileNotFoundException>(() => control.Insert(Path.Combine(harness.Root, "missing.st")));

        Assert.False(harness.Ejected);
        Assert.Equal(0, harness.ReplaceCalls);
    }

    [Fact]
    public void Insert_ReplacesCurrentImageAndKeepsNoFileHandle()
    {
        using var harness = new DiskControlHarness(["first.st"], ["First"]);
        var control = harness.CaptureBasic();
        var path = Path.Combine(harness.Root, "replacement.st");
        File.WriteAllBytes(path, [1, 2, 3]);

        control.Insert(path);

        Assert.Equal(1, harness.ReplaceCalls);
        Assert.False(harness.Ejected);
        using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.Equal(3, stream.Length);
    }

    [Fact]
    public void Eject_ChangesNativeState()
    {
        using var harness = new DiskControlHarness(["first.st"], ["First"]);
        var control = harness.CaptureBasic();

        control.Eject();

        Assert.True(harness.Ejected);
    }

    private sealed class DiskControlHarness : IDisposable
    {
        private readonly string[] _paths;
        private readonly string[] _labels;
        private readonly List<Delegate> _delegates = [];
        private nint _nativePointer;

        internal DiskControlHarness(string[] paths, string[] labels)
        {
            _paths = paths;
            _labels = labels;
            Root = Path.Combine(Path.GetTempPath(), "GWGUI-Atari-DiskControl", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        internal string Root { get; }
        internal bool Ejected { get; private set; }
        internal uint CurrentIndex { get; set; }
        internal uint? RejectIndex { get; init; }
        internal int ReplaceCalls { get; private set; }
        internal List<bool> EjectTransitions { get; } = [];

        internal AtariDiskControl CaptureBasic()
        {
            var native = CreateBasic();
            _nativePointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.DiskControl>());
            Marshal.StructureToPtr(native, _nativePointer, false);
            var control = new AtariDiskControl();
            control.Capture(_nativePointer);
            return control;
        }

        internal AtariDiskControl CaptureExtended()
        {
            ExternalCoreApi.GetImagePath getPath = (index, buffer, length) =>
                WriteText(_paths[checked((int)index)], buffer, length);
            ExternalCoreApi.GetImageLabel getLabel = (index, buffer, length) =>
                WriteText(_labels[checked((int)index)], buffer, length);
            Keep(getPath, getLabel);
            var native = new ExternalCoreApi.DiskControlExtended
            {
                Basic = CreateBasic(),
                GetImagePath = Marshal.GetFunctionPointerForDelegate(getPath),
                GetImageLabel = Marshal.GetFunctionPointerForDelegate(getLabel)
            };
            _nativePointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.DiskControlExtended>());
            Marshal.StructureToPtr(native, _nativePointer, false);
            var control = new AtariDiskControl();
            control.CaptureExtended(_nativePointer);
            return control;
        }

        private ExternalCoreApi.DiskControl CreateBasic()
        {
            ExternalCoreApi.SetEjectState setEject = value =>
            {
                Ejected = value;
                EjectTransitions.Add(value);
                return true;
            };
            ExternalCoreApi.GetEjectState getEject = () => Ejected;
            ExternalCoreApi.GetImageIndex getIndex = () => CurrentIndex;
            ExternalCoreApi.SetImageIndex setIndex = value =>
            {
                if (RejectIndex == value) return false;
                CurrentIndex = value;
                return true;
            };
            ExternalCoreApi.GetImageCount getCount = () => checked((uint)_paths.Length);
            ExternalCoreApi.ReplaceImage replace = (_, _) =>
            {
                ReplaceCalls++;
                return true;
            };
            ExternalCoreApi.AddImage add = () => true;
            Keep(setEject, getEject, getIndex, setIndex, getCount, replace, add);
            return new ExternalCoreApi.DiskControl
            {
                SetEjectState = Marshal.GetFunctionPointerForDelegate(setEject),
                GetEjectState = Marshal.GetFunctionPointerForDelegate(getEject),
                GetImageIndex = Marshal.GetFunctionPointerForDelegate(getIndex),
                SetImageIndex = Marshal.GetFunctionPointerForDelegate(setIndex),
                GetImageCount = Marshal.GetFunctionPointerForDelegate(getCount),
                ReplaceImage = Marshal.GetFunctionPointerForDelegate(replace),
                AddImage = Marshal.GetFunctionPointerForDelegate(add)
            };
        }

        private void Keep(params Delegate[] delegates) => _delegates.AddRange(delegates);

        private static bool WriteText(string value, nint buffer, nuint length)
        {
            var bytes = Encoding.UTF8.GetBytes(value + '\0');
            if ((nuint)bytes.Length > length) return false;
            Marshal.Copy(bytes, 0, buffer, bytes.Length);
            return true;
        }

        public void Dispose()
        {
            if (_nativePointer != nint.Zero) Marshal.FreeHGlobal(_nativePointer);
            Directory.Delete(Root, true);
            GC.KeepAlive(_delegates);
        }
    }
}
