using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Services;

namespace GWGUI.Tests.Emulation;

public sealed class CoreDiskControlTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmptyCaptureKeepsTheProfilePolicy(bool reset)
    {
        using var fixture = new Fixture();
        var owner = new RichOwner(fixture, reset);
        fixture.Capture(owner);
        fixture.Api = default;
        if (reset)
        {
            fixture.Capture(owner);
            Assert.False(owner.IsAvailable);
            Assert.Equal(0, owner.ImageCount);
            Assert.Equal(-1, owner.CurrentIndex);
            Assert.True(owner.IsEjected);
        }
        else
        {
            var error = Assert.Throws<InvalidOperationException>(() => fixture.Capture(owner));
            Assert.Equal("incomplete", error.Message);
            Assert.True(owner.IsAvailable);
            Assert.Equal(2, owner.ImageCount);
        }
    }

    [Fact]
    public void FailedCaptureKeepsTheAssignmentsAlreadyMadeAndTheOldTail()
    {
        using var fixture = new Fixture();
        var owner = new NativeOwner();
        fixture.Capture(owner);
        fixture.Api.SetEjectState = fixture.Pointer((ExternalCoreApi.SetEjectState)(_ => { fixture.Trace.Add("new-eject"); return true; }));
        fixture.Api.GetImageCount = 0;
        Assert.Equal("incomplete", Assert.Throws<InvalidOperationException>(() => fixture.Capture(owner)).Message);
        owner.Eject();
        Assert.Equal(2, owner.ImageCount);
        Assert.Equal(new[] { "new-eject", "count" }, fixture.Trace);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NegativeIndexRetainsTheNativeOrRichCountReadPolicy(bool rich)
    {
        using var fixture = new Fixture();
        ExternalCoreDiskControl owner = rich ? new RichOwner(fixture) : new NativeOwner();
        fixture.Capture(owner);
        Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => owner.Select(-1)).ParamName);
        Assert.Equal(rich ? Array.Empty<string>() : new[] { "count" }, fixture.Trace);
    }

    [Fact]
    public void SelectionUsesTheRequestedErrorAndRollsBackToTheOldIndex()
    {
        using var fixture = new Fixture { RefuseSelection = true };
        var owner = new NativeOwner();
        fixture.Capture(owner);
        Assert.Equal("requested-select", Assert.Throws<InvalidOperationException>(() => owner.Select(0)).Message);
        Assert.Equal(new[] { "count", "index", "ejected", "eject:True", "select:0", "select:1", "eject:False" }, fixture.Trace);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void RichContentIsDisposedAfterRollbackEvenIfRollbackThrows(bool gameInfoFails, bool rollbackFails)
    {
        using var fixture = new Fixture { RefuseReplacement = true, GameInfoFails = gameInfoFails, RollbackFails = rollbackFails };
        var owner = new RichOwner(fixture);
        fixture.Capture(owner);
        var error = Assert.Throws<InvalidOperationException>(() => owner.Insert("disc.bin"));
        Assert.Equal(rollbackFails ? "rollback" : gameInfoFails ? "game-info" : "replace", error.Message);
        Assert.Equal("dispose", fixture.Trace[^1]);
        Assert.Equal("eject:False", fixture.Trace[^2]);
        Assert.Equal(1, fixture.Disposals);
    }

    [Fact]
    public void ContentCreationFailureDoesNotEnterTheRollbackRegion()
    {
        using var fixture = new Fixture { CreationFails = true };
        var owner = new RichOwner(fixture);
        fixture.Capture(owner);
        Assert.Equal("create", Assert.Throws<InvalidOperationException>(() => owner.Insert("disc.bin")).Message);
        Assert.Equal(new[] { "ejected", "eject:True", "count", "index", "create" }, fixture.Trace);
        Assert.Equal(0, fixture.Disposals);
    }

    [Fact]
    public void AlreadyEjectedContentFailureDoesNotReinsertTheTray()
    {
        using var fixture = new Fixture { Ejected = true, RefuseReplacement = true };
        var owner = new RichOwner(fixture);
        fixture.Capture(owner);
        Assert.Throws<InvalidOperationException>(() => owner.Insert("disc.bin"));
        Assert.DoesNotContain(fixture.Trace, item => item.StartsWith("eject:", StringComparison.Ordinal));
        Assert.Equal("dispose", fixture.Trace[^1]);
    }

    [Fact]
    public void NativeContentKeepsTheFullUtf8PathAndAnEmptyDataPointer()
    {
        using var fixture = new Fixture { InspectNativeGame = true };
        var owner = new NativeOwner();
        fixture.Capture(owner);
        owner.Insert("jeu-é.bin");
        Assert.Equal(Path.GetFullPath("jeu-é.bin"), fixture.NativePath);
        Assert.Equal(new[] { "ejected", "eject:True", "count", "index", "replace:1", "select:1", "eject:False" }, fixture.Trace);
    }

    [Fact]
    public void ZeroImagesCreatesASlotWithoutReadingAnIndex()
    {
        using var fixture = new Fixture { Count = 0 };
        var owner = new RichOwner(fixture);
        fixture.Capture(owner);
        owner.Insert("disc.bin");
        Assert.Equal(new[] { "ejected", "eject:True", "count", "add", "create", "game-info", "replace:0", "select:0", "eject:False", "dispose" }, fixture.Trace);
    }

    [Fact]
    public void ExtendedEmptyCaptureStillInstallsItsOptionalTextCallbacks()
    {
        using var fixture = new Fixture();
        var owner = new RichOwner(fixture, true);
        fixture.Api = default;
        fixture.Capture(owner, extended: true);
        Assert.False(owner.IsAvailable);
        Assert.Equal("path", owner.GetPath(0));
        Assert.Equal("label", owner.GetLabel(0));
        owner.Clear();
        Assert.Null(owner.GetPath(0));
        Assert.Null(owner.GetLabel(0));
    }

    [Fact]
    public void NativeSentinelAndCheckedConversionsRemainDistinct()
    {
        using var fixture = new Fixture { Index = uint.MaxValue, Count = uint.MaxValue };
        var owner = new NativeOwner();
        fixture.Capture(owner);
        Assert.Equal(-1, owner.CurrentIndex);
        Assert.Throws<OverflowException>(() => owner.ImageCount);
        fixture.Index = int.MaxValue + 1u;
        Assert.Throws<OverflowException>(() => owner.CurrentIndex);
    }

    private sealed class Fixture : IDisposable
    {
        internal ExternalCoreApi.DiskControl Api;
        internal readonly List<string> Trace = [];
        private readonly List<Delegate> _callbacks = [];
        private nint _buffer;
        internal uint Count = 2, Index = 1;
        internal bool Ejected, RefuseSelection, RefuseReplacement, CreationFails, GameInfoFails, RollbackFails, InspectNativeGame;
        internal int Disposals;
        internal string? NativePath;

        internal Fixture()
        {
            Api = new ExternalCoreApi.DiskControl
            {
                SetEjectState = Pointer((ExternalCoreApi.SetEjectState)(value =>
                {
                    Trace.Add("eject:" + value);
                    if (!value && RollbackFails) throw new InvalidOperationException("rollback");
                    Ejected = value;
                    return true;
                })),
                GetEjectState = Pointer((ExternalCoreApi.GetEjectState)(() => { Trace.Add("ejected"); return Ejected; })),
                GetImageIndex = Pointer((ExternalCoreApi.GetImageIndex)(() => { Trace.Add("index"); return Index; })),
                SetImageIndex = Pointer((ExternalCoreApi.SetImageIndex)(index => { Trace.Add("select:" + index); return !RefuseSelection; })),
                GetImageCount = Pointer((ExternalCoreApi.GetImageCount)(() => { Trace.Add("count"); return Count; })),
                ReplaceImage = Pointer((ExternalCoreApi.ReplaceImage)((index, game) =>
                {
                    Trace.Add("replace:" + index);
                    if (InspectNativeGame)
                    {
                        var info = Marshal.PtrToStructure<ExternalCoreApi.GameInfo>(game);
                        NativePath = Marshal.PtrToStringUTF8(info.Path);
                        Assert.Equal(nint.Zero, info.Data);
                        Assert.Equal((nuint)0, info.Size);
                    }
                    return !RefuseReplacement;
                })),
                AddImage = Pointer((ExternalCoreApi.AddImage)(() => { Trace.Add("add"); return true; }))
            };
        }

        internal nint Pointer(Delegate callback)
        {
            _callbacks.Add(callback);
            return Marshal.GetFunctionPointerForDelegate(callback);
        }

        internal void Capture(ExternalCoreDiskControl owner, bool extended = false)
        {
            if (_buffer == 0) _buffer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.DiskControlExtended>());
            if (extended)
            {
                var api = new ExternalCoreApi.DiskControlExtended
                {
                    Basic = Api,
                    GetImagePath = Pointer((ExternalCoreApi.GetImagePath)((_, buffer, _) => Write(buffer, "path"))),
                    GetImageLabel = Pointer((ExternalCoreApi.GetImageLabel)((_, buffer, _) => Write(buffer, "label")))
                };
                Marshal.StructureToPtr(api, _buffer, false);
                owner.CaptureExtended(_buffer);
            }
            else
            {
                Marshal.StructureToPtr(Api, _buffer, false);
                owner.Capture(_buffer);
            }
        }

        private static bool Write(nint buffer, string text)
        {
            Marshal.Copy(System.Text.Encoding.UTF8.GetBytes(text + "\0"), 0, buffer, text.Length + 1);
            return true;
        }

        public void Dispose()
        {
            try { if (_buffer != 0) Marshal.FreeHGlobal(_buffer); }
            finally { _buffer = 0; GC.KeepAlive(_callbacks); }
        }
    }

    private sealed class Content(Fixture fixture) : IDisposable
    {
        internal nint GameInfo
        {
            get
            {
                fixture.Trace.Add("game-info");
                if (fixture.GameInfoFails) throw new InvalidOperationException("game-info");
                return 1;
            }
        }
        public void Dispose() { fixture.Disposals++; fixture.Trace.Add("dispose"); }
    }

    private sealed class NativeOwner : ExternalCoreDiskControl
    {
        protected override bool MediaExists(string path) => true;
        protected override string UnavailableMessage => "unavailable";
        protected override string IncompleteMessage => "incomplete";
        protected override string EjectFailedMessage => "eject";
        protected override string SelectFailedMessage => "select";
        protected override string InsertFailedMessage => "insert";
        protected override string CreateSlotFailedMessage => "add";
        protected override string ReplaceFailedMessage => "replace";
        protected override string MediaMissingMessage => "missing";
        protected override string RequestedSelectFailedMessage => "requested-select";
    }

    private sealed class RichOwner(Fixture fixture, bool reset = false) : ExternalRichDiskControl<Content>
    {
        protected override bool ResetOnEmptyCapture => reset;
        protected override bool MediaExists(string path) => true;
        protected override Content CreateContent(string path)
        {
            fixture.Trace.Add("create");
            if (fixture.CreationFails) throw new InvalidOperationException("create");
            return new Content(fixture);
        }
        protected override nint GetGameInfo(Content content) => content.GameInfo;
        protected override string UnavailableMessage => "unavailable";
        protected override string IncompleteMessage => "incomplete";
        protected override string EjectFailedMessage => "eject";
        protected override string SelectFailedMessage => "select";
        protected override string InsertFailedMessage => "insert";
        protected override string CreateSlotFailedMessage => "add";
        protected override string ReplaceFailedMessage => "replace";
        protected override string MediaMissingMessage => "missing";
    }
}
