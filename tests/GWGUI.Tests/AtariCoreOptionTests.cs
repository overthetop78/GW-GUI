using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;
using GWGUI.Emulation.Common;

namespace GWGUI.Tests;

public sealed class AtariCoreOptionTests
{
    public static TheoryData<string, AtariCoreKind, int> OfficialCatalogs => new()
    {
        { "hatari.dll", AtariCoreKind.Hatari, AtariCoreOptionConstants.HatariDefinitionCount },
        { "atari800.dll", AtariCoreKind.Atari800, AtariCoreOptionConstants.Atari800DefinitionCount },
        { "stella.dll", AtariCoreKind.Stella, AtariCoreOptionConstants.StellaDefinitionCount },
        { "prosystem.dll", AtariCoreKind.ProSystem, AtariCoreOptionConstants.ProSystemDefinitionCount },
        { "beetle-lynx.dll", AtariCoreKind.BeetleLynx, AtariCoreOptionConstants.BeetleLynxDefinitionCount },
        { "virtual-jaguar.dll", AtariCoreKind.VirtualJaguar, AtariCoreOptionConstants.VirtualJaguarDefinitionCount }
    };

    [Theory]
    [MemberData(nameof(OfficialCatalogs))]
    [Trait("Category", "LocalAssets")]
    public void OfficialCore_CopiesEveryAnnouncedOption(string fileName, AtariCoreKind kind, int expectedCount)
    {
        Assert.True(Enum.IsDefined(kind));
        var executable = Path.Combine(AppContext.BaseDirectory, "gwgui.app.exe");
        var corePath = Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", fileName);
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = executable,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            ArgumentList =
            {
                AtariCoreOptionProbeConstants.CommandLineArgument,
                corePath,
                kind.ToString()
            }
        }) ?? throw new InvalidOperationException();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        Assert.True(process.WaitForExit(AtariCoreOptionProbeConstants.ProcessTimeoutMilliseconds),
            $"Option probe timed out for {kind}.");
        Assert.Equal(AtariCoreOptionProbeConstants.SuccessExitCode, process.ExitCode);
        Assert.True(int.TryParse(output.Trim(), out var actualCount), error);
        Assert.Equal(expectedCount, actualCount);
    }

    [Fact]
    public void ValueUpdate_UsesStablePointerAndClearsUpdateFlag()
    {
        using var native = new NativeLegacyVariables("video_mode", "Video mode; fast|accurate");
        using var callbacks = CreateCallbacks(new Dictionary<string, string> { ["unknown_document_key"] = "kept" }, out var root);
        try
        {
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetVariables, native.Pointer));

        using var key = new ExternalCoreUtf8String("video_mode");
        using var accurate = new ExternalCoreUtf8String("accurate");
        var variablePointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.Variable>());
        var updatePointer = Marshal.AllocHGlobal(sizeof(byte));
            try
            {
            Marshal.StructureToPtr(new ExternalCoreApi.Variable { Key = key.Pointer, Value = accurate.Pointer },
                variablePointer, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetVariable, variablePointer));
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetVariableUpdate, updatePointer));
            Assert.Equal(AtariConstants.NativeBooleanTrue, Marshal.ReadByte(updatePointer));
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetVariableUpdate, updatePointer));
            Assert.Equal(AtariConstants.NativeBooleanFalse, Marshal.ReadByte(updatePointer));

            Marshal.StructureToPtr(new ExternalCoreApi.Variable { Key = key.Pointer }, variablePointer, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetVariable, variablePointer));
            var first = Marshal.PtrToStructure<ExternalCoreApi.Variable>(variablePointer).Value;
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.GetVariable, variablePointer));
            var second = Marshal.PtrToStructure<ExternalCoreApi.Variable>(variablePointer).Value;
            Assert.Equal(first, second);
            Assert.Equal("accurate", Marshal.PtrToStringUTF8(first));
            Assert.Equal("kept", callbacks.OptionDocumentValues["unknown_document_key"]);
            }
            finally
            {
                Marshal.FreeHGlobal(variablePointer);
                Marshal.FreeHGlobal(updatePointer);
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void VersionOneAndInternational_CopyLabelsDescriptionsAndEnglishDefaults()
    {
        using var english = NativeOptionDefinitions.VersionOne("renderer", "Renderer", "Rendering help",
            "accurate", ("fast", "Fast"), ("accurate", "Accurate"));
        using var localized = NativeOptionDefinitions.VersionOne("renderer", "Moteur", "Aide locale",
            "fast", ("fast", "Rapide"), ("accurate", "Précis"));
        using var international = NativeOptionDefinitions.International(english.Pointer, localized.Pointer);
        using var callbacks = CreateCallbacks(new Dictionary<string, string>(), out var root);
        try
        {
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetCoreOptionsInternational,
                international.Pointer));
            var option = Assert.Single(callbacks.Options);
            Assert.Equal("Moteur", option.Name);
            Assert.Equal("Aide locale", option.Description);
            Assert.Equal("accurate", option.DefaultValue);
            Assert.Equal("Rapide", option.Values[AtariCoreOptionConstants.FirstEntryIndex].Label);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void VersionTwo_CopiesCategoriesCategorizedTextVisibilityAndDisplayCallback()
    {
        using var options = NativeOptionDefinitions.VersionTwo("video", "Video", "Video options", "renderer",
            "Video > Renderer", "Renderer", "Rendering help", "Short help", "accurate",
            ("fast", "Fast"), ("accurate", "Accurate"));
        using var callbacks = CreateCallbacks(new Dictionary<string, string>(), out var root);
        var updateInvoked = false;
        ExternalCoreApi.UpdateCoreOptionsDisplay update = () => updateInvoked = true;
        var updateData = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.CoreOptionsUpdateDisplayCallback>());
        var visibilityData = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.CoreOptionDisplay>());
        using var key = new ExternalCoreUtf8String("renderer");
        try
        {
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetCoreOptionsV2, options.Pointer));
            var category = Assert.Single(callbacks.OptionCategories);
            var option = Assert.Single(callbacks.Options);
            Assert.Equal("video", category.Key);
            Assert.Equal("Video options", category.Description);
            Assert.Equal("Renderer", option.CategorizedName);
            Assert.Equal("Short help", option.CategorizedDescription);

            Marshal.StructureToPtr(new ExternalCoreApi.CoreOptionsUpdateDisplayCallback
            {
                Callback = Marshal.GetFunctionPointerForDelegate(update)
            }, updateData, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback, updateData));
            Marshal.StructureToPtr(new ExternalCoreApi.CoreOptionDisplay { Key = key.Pointer, Visible = false },
                visibilityData, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetCoreOptionsDisplay, visibilityData));
            Assert.False(Assert.Single(callbacks.Options).IsVisible);
            callbacks.SetOption("renderer", "fast");
            Assert.True(updateInvoked);
            GC.KeepAlive(update);
        }
        finally
        {
            Marshal.FreeHGlobal(updateData);
            Marshal.FreeHGlobal(visibilityData);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void VersionTwoInternational_UsesLocalizedTextAndEnglishDefault()
    {
        using var english = NativeOptionDefinitions.VersionTwo("video", "Video", "Video options", "renderer",
            "Video > Renderer", "Renderer", "Rendering help", "Short help", "accurate",
            ("fast", "Fast"), ("accurate", "Accurate"));
        using var localized = NativeOptionDefinitions.VersionTwo("video", "Vidéo", "Options vidéo", "renderer",
            "Vidéo > Moteur", "Moteur", "Aide rendu", "Aide courte", "fast",
            ("fast", "Rapide"), ("accurate", "Précis"));
        using var international = NativeOptionDefinitions.International(english.Pointer, localized.Pointer);
        using var callbacks = CreateCallbacks(new Dictionary<string, string>(), out var root);
        try
        {
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetCoreOptionsV2International,
                international.Pointer));
            Assert.Equal("Vidéo", Assert.Single(callbacks.OptionCategories).Name);
            var option = Assert.Single(callbacks.Options);
            Assert.Equal("Vidéo > Moteur", option.Name);
            Assert.Equal("Moteur", option.CategorizedName);
            Assert.Equal("accurate", option.DefaultValue);
            Assert.Equal("Rapide", option.Values[AtariCoreOptionConstants.FirstEntryIndex].Label);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static AtariExternalHostCallbacks CreateCallbacks(IReadOnlyDictionary<string, string> configured,
        out string root)
    {
        root = Path.Combine(Path.GetTempPath(), "GWGUI-Atari-OptionCallback", Guid.NewGuid().ToString("N"));
        return new(root, root, root, root, configured);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "GWGUI.sln"))) current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException();
    }

    private sealed class NativeLegacyVariables : IDisposable
    {
        private readonly ExternalCoreUtf8String _key;
        private readonly ExternalCoreUtf8String _definition;
        internal NativeLegacyVariables(string key, string definition)
        {
            _key = new(key);
            _definition = new(definition);
            var size = Marshal.SizeOf<ExternalCoreApi.Variable>();
            Pointer = Marshal.AllocHGlobal(size * AtariCoreOptionConstants.TerminatedArrayEntryCount);
            Marshal.StructureToPtr(new ExternalCoreApi.Variable { Key = _key.Pointer, Value = _definition.Pointer },
                Pointer, false);
            Marshal.StructureToPtr(new ExternalCoreApi.Variable(), Pointer + size, false);
        }
        internal nint Pointer { get; }
        public void Dispose()
        {
            Marshal.FreeHGlobal(Pointer);
            _key.Dispose();
            _definition.Dispose();
        }
    }

    private sealed class NativeOptionDefinitions : IDisposable
    {
        private readonly List<ExternalCoreUtf8String> _strings = [];
        private readonly List<nint> _allocations = [];
        private NativeOptionDefinitions() { }
        internal nint Pointer { get; private set; }

        internal static NativeOptionDefinitions VersionOne(string key, string name, string description,
            string defaultValue, params (string Value, string Label)[] values)
        {
            var owner = new NativeOptionDefinitions();
            var prefix = AtariCoreOptionConstants.LegacyDefinitionPointerCount;
            owner.Pointer = owner.AllocateDefinition(prefix, key, name, description, null, null, null,
                defaultValue, values);
            return owner;
        }

        internal static NativeOptionDefinitions VersionTwo(string categoryKey, string categoryName,
            string categoryDescription, string key, string name, string categorizedName, string description,
            string categorizedDescription, string defaultValue, params (string Value, string Label)[] values)
        {
            var owner = new NativeOptionDefinitions();
            var categories = owner.AllocatePointers(AtariCoreOptionConstants.CategoryPointerCount
                * AtariCoreOptionConstants.TerminatedArrayEntryCount);
            Write(categories, AtariCoreOptionConstants.KeyPointerIndex, owner.String(categoryKey));
            Write(categories, AtariCoreOptionConstants.NamePointerIndex, owner.String(categoryName));
            Write(categories, AtariCoreOptionConstants.DescriptionPointerIndex, owner.String(categoryDescription));
            var definitions = owner.AllocateDefinition(
                AtariCoreOptionConstants.VersionTwoDefinitionPointerCountBeforeValues, key, name, description,
                categorizedName, categorizedDescription, categoryKey, defaultValue, values);
            var options = owner.AllocatePointers(AtariCoreOptionConstants.InternationalPointerCount);
            Write(options, AtariCoreOptionConstants.CategoriesPointerIndex, categories);
            Write(options, AtariCoreOptionConstants.DefinitionsPointerIndex, definitions);
            owner.Pointer = options;
            return owner;
        }

        internal static NativeOptionDefinitions International(nint english, nint localized)
        {
            var owner = new NativeOptionDefinitions();
            owner.Pointer = owner.AllocatePointers(AtariCoreOptionConstants.InternationalPointerCount);
            Write(owner.Pointer, AtariCoreOptionConstants.EnglishPointerIndex, english);
            Write(owner.Pointer, AtariCoreOptionConstants.LocalPointerIndex, localized);
            return owner;
        }

        private nint AllocateDefinition(int prefix, string key, string name, string description,
            string? categorizedName, string? categorizedDescription, string? categoryKey, string defaultValue,
            IReadOnlyList<(string Value, string Label)> values)
        {
            var pointerCount = prefix + AtariCoreOptionConstants.MaximumValues
                * AtariCoreOptionConstants.ValuePointerCount + AtariCoreOptionConstants.DefaultValuePointerCount;
            var definitions = AllocatePointers(pointerCount * AtariCoreOptionConstants.TerminatedArrayEntryCount);
            Write(definitions, AtariCoreOptionConstants.KeyPointerIndex, String(key));
            Write(definitions, AtariCoreOptionConstants.NamePointerIndex, String(name));
            if (prefix == AtariCoreOptionConstants.LegacyDefinitionPointerCount)
                Write(definitions, AtariCoreOptionConstants.DescriptionPointerIndex, String(description));
            else
            {
                Write(definitions, AtariCoreOptionConstants.CategorizedNamePointerIndex, String(categorizedName!));
                Write(definitions, AtariCoreOptionConstants.VersionTwoDescriptionPointerIndex, String(description));
                Write(definitions, AtariCoreOptionConstants.CategorizedDescriptionPointerIndex,
                    String(categorizedDescription!));
                Write(definitions, AtariCoreOptionConstants.CategoryKeyPointerIndex, String(categoryKey!));
            }
            for (var index = AtariCoreOptionConstants.FirstEntryIndex; index < values.Count; index++)
            {
                var valueIndex = prefix + index * AtariCoreOptionConstants.ValuePointerCount;
                Write(definitions, valueIndex + AtariCoreOptionConstants.ValuePointerIndex, String(values[index].Value));
                Write(definitions, valueIndex + AtariCoreOptionConstants.LabelPointerIndex, String(values[index].Label));
            }
            var defaultIndex = prefix + AtariCoreOptionConstants.MaximumValues
                * AtariCoreOptionConstants.ValuePointerCount;
            Write(definitions, defaultIndex, String(defaultValue));
            return definitions;
        }

        private nint AllocatePointers(int count)
        {
            var pointer = Marshal.AllocHGlobal(count * IntPtr.Size);
            _allocations.Add(pointer);
            for (var index = AtariCoreOptionConstants.FirstEntryIndex; index < count; index++)
                Marshal.WriteIntPtr(pointer, index * IntPtr.Size, nint.Zero);
            return pointer;
        }

        private nint String(string value)
        {
            var text = new ExternalCoreUtf8String(value);
            _strings.Add(text);
            return text.Pointer;
        }

        private static void Write(nint pointer, int index, nint value) =>
            Marshal.WriteIntPtr(pointer, index * IntPtr.Size, value);

        public void Dispose()
        {
            foreach (var allocation in _allocations) Marshal.FreeHGlobal(allocation);
            foreach (var text in _strings) text.Dispose();
        }
    }
}
