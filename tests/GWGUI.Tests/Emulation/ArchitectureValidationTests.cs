using GWGUI.Emulation;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class ArchitectureValidationTests
{
    [Theory]
    [InlineData("IEmulatorConfigurationValidation", "ValidateConfiguration", typeof(void))]
    [InlineData("IEmulatorMediaCapabilities", "DescribeMediaDevices", typeof(IReadOnlyList<GWGUI.Emulation.Contracts.EmulationMediaDevice>))]
    public void EveryCommonUsesTheSameCapabilityContractsIncludingExample(string contractName, string methodName, Type returnType)
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly)
            .Distinct().OrderBy(assembly => assembly.GetName().Name, StringComparer.Ordinal).ToArray();
        Assert.Equal(24, modules.Length);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces." + contractName, throwOnError: true)!;
            Assert.True(contract.IsInterface);
            Assert.False(contract.IsVisible);
            var method = Assert.Single(contract.GetMethods());
            Assert.Equal(methodName, method.Name);
            Assert.True(method.IsAbstract);
            Assert.Equal(returnType, method.ReturnType);
            var parameter = Assert.Single(method.GetParameters());
            Assert.Equal("configuration", parameter.Name);
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(parameter.ParameterType));
            if (contractName == "IEmulatorConfigurationValidation")
                Assert.Null(assembly.GetType(root + ".Common.Interfaces.IEmulatorConfigurationValidator"));
            foreach (var implementation in assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)))
                Assert.False(Assert.Single(implementation.GetInterfaceMap(contract).TargetMethods).IsAbstract);
            if (root == "GWGUI.Emulation.Apple" && contractName == "IEmulatorConfigurationValidation")
                Assert.Contains(contract, assembly.GetType(root + ".Common.Interfaces.IEmulatorConfiguration")!.GetInterfaces());
            if (root == "GWGUI.Emulation.Example")
                Assert.Contains(contract, assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter")!.GetInterfaces());
            if (contractName == "IEmulatorMediaCapabilities")
            {
                if (root is "GWGUI.Emulation.Acorn" or "GWGUI.Emulation.Example")
                    Assert.Contains(contract, assembly.GetType(root + ".Common.Interfaces.IEmulatorStorageSettings")!.GetInterfaces());
                if (root == "GWGUI.Emulation.Snk")
                    Assert.Contains(contract, assembly.GetType(root + ".Common.Interfaces.IEmulatorMediaAdapter")!.GetInterfaces());
                if (root is "GWGUI.Emulation.Sharp" or "GWGUI.Emulation.Sinclair")
                    Assert.Null(assembly.GetType(root + ".Common.Interfaces.IEmulatorStorageSettings"));
            }
        }
    }

    [Fact]
    public void EveryCommonUsesTheSameAudioBufferFeedbackContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly)
            .Distinct().OrderBy(assembly => assembly.GetName().Name, StringComparer.Ordinal).ToArray();
        Assert.Equal(24, modules.Length);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAudioBufferFeedback", throwOnError: true)!;
            Assert.True(contract.IsInterface);
            Assert.False(contract.IsVisible);
            Assert.Equal(2, contract.GetMethods().Length);
            var latency = Assert.Single(contract.GetProperties());
            Assert.Equal("MinimumAudioLatencyMilliseconds", latency.Name);
            Assert.Equal(typeof(int), latency.PropertyType);
            Assert.Null(latency.SetMethod);
            Assert.True(latency.GetMethod!.IsAbstract);
            var publish = contract.GetMethod("SetAudioBufferStatus")!;
            Assert.NotNull(publish);
            Assert.True(publish.IsAbstract);
            Assert.Equal(typeof(void), publish.ReturnType);
            Assert.Equal(typeof(AudioOutputBufferStatus?), Assert.Single(publish.GetParameters()).ParameterType);
            var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
            foreach (var implementation in implementations)
            {
                Assert.All(implementation.GetInterfaceMap(contract).TargetMethods, method => Assert.False(method.IsAbstract));
                Assert.Null(implementation.GetProperty("OutputAudioStatus", BindingFlags.Public | BindingFlags.Instance));
            }
            if (root is "GWGUI.Emulation.Example" or "GWGUI.Emulation.Snk")
            {
                var core = assembly.GetType(root + ".Common.Interfaces.IEmulatorCore")!;
                Assert.Contains(contract, core.GetInterfaces());
                Assert.Null(core.GetProperty("OutputAudioStatus"));
                Assert.Equal(2, implementations.Length);
            }
            if (root == "GWGUI.Emulation.Watara")
                Assert.Equal(2, implementations.Length);
        }
    }

    [Fact]
    public void SdkGrantsInternalAccessOnlyToTheExistingTestAssemblies()
    {
        var grants = typeof(EmulationSdk).Assembly.GetCustomAttributes<InternalsVisibleToAttribute>()
            .Select(attribute => new AssemblyName(attribute.AssemblyName).Name).ToArray();
        var tests = new[] { "GWGUI.Tests", "GWGUI.LocalDiskImageTests", "GWGUI.TemporaryLibretroMediaReader" };
        Assert.All(grants, name => Assert.Contains(name, tests));
        Assert.DoesNotContain(grants, name => name!.StartsWith("GWGUI.Emulation.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(typeof(ExternalCoreApiConstants))]
    [InlineData(typeof(ExternalCoreKeyboardConstants))]
    [InlineData(typeof(ExternalCoreExportNames))]
    public void NativeDataContractsExposeConstantsWithoutImplementationMethods(Type contract)
    {
        Assert.True(contract.IsPublic);
        Assert.Empty(contract.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        var fields = contract.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.NotEmpty(fields);
        Assert.All(fields, field => Assert.True(field.IsLiteral && !field.IsInitOnly));
    }

    [Fact]
    public void KeyboardCalculationDetailsRemainInternal()
    {
        foreach (var name in new[] { "UppercaseOffset", "FirstRangeOffset", "ModifierFirstOrder", "ModifierLastOrder" })
        {
            var field = typeof(ExternalCoreKeyboardConstants).GetField(name, BindingFlags.Static | BindingFlags.NonPublic)!;
            Assert.NotNull(field);
            Assert.True(field.IsAssembly);
        }
    }

    [Theory]
    [InlineData(nameof(IEmulationNativeResources.CreateString), typeof(IEmulationNativeString), "ExternalCoreUtf8String")]
    [InlineData(nameof(IEmulationNativeResources.AddString), typeof(IEmulationNativeString), "ExternalCoreUtf8String")]
    [InlineData(nameof(IEmulationNativeResources.CreateLoadedContent), typeof(IEmulationLoadedContent), "ExternalCoreLoadedContent")]
    [InlineData(nameof(IEmulationNativeResources.CreateVirtualFileSystem), typeof(IEmulationVirtualFileSystem), "ExternalCoreVirtualFileSystem")]
    [InlineData(nameof(IEmulationNativeResources.CreateLibrary), typeof(IEmulationCoreLibrary), "ExternalCoreLibrary")]
    [InlineData(nameof(IEmulationNativeResources.CreateDiskControl), typeof(IEmulationDiskControl), "ExternalCoreDiskControl")]
    [InlineData(nameof(IEmulationNativeResources.CreateAudioCallbackHost), typeof(IEmulationAudioCallbackHost), "ExternalCoreAudioCallbackHost")]
    [InlineData(nameof(IEmulationNativeResources.CreateVideoCallbackHost), typeof(IEmulationVideoCallbackHost), "ExternalCoreVideoCallbackHost")]
    [InlineData(nameof(IEmulationNativeResources.CreateEnvironmentCallbackHost), typeof(IEmulationEnvironmentCallbackHost), "ExternalCoreEnvironmentCallbackHost")]
    [InlineData(nameof(IEmulationNativeResources.CreateHardwareRenderContext), typeof(IEmulationHardwareRenderContext), "EmulationOpenGlHardwareRenderContext")]
    public void NativeResourceFactoriesExposeInterfacesAndKeepOwnersInternal(string methodName, Type contract, string implementationName)
    {
        var methods = typeof(IEmulationNativeResources).GetMethods().Where(method => method.Name == methodName).ToArray();
        Assert.NotEmpty(methods);
        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
        Assert.All(methods, method => Assert.Equal(contract, method.ReturnType));
        var implementation = typeof(EmulationSdk).Assembly.GetType("GWGUI.Emulation.Services." + implementationName, throwOnError: true)!;
        Assert.False(implementation.IsVisible);
        Assert.True(contract.IsAssignableFrom(implementation));
    }

    [Theory]
    [InlineData(nameof(IEmulationNativeResources.CreateCoreOptionHost), typeof(IEmulationCoreOptionHost<>), "EmulationCoreOptionHost`2")]
    [InlineData(nameof(IEmulationNativeResources.CreateVariableOptionHost), typeof(IEmulationVariableOptionHost<>), "EmulationLegacyOptionHost`1")]
    public void GenericOptionFactoriesExposeInterfacesAndKeepOwnersInternal(string methodName, Type contract, string implementationName)
    {
        var factory = typeof(IEmulationNativeResources).GetMethod(methodName)!;
        Assert.True(factory.IsGenericMethodDefinition);
        Assert.Equal(contract, factory.ReturnType.GetGenericTypeDefinition());
        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
        var implementation = typeof(EmulationSdk).Assembly.GetType("GWGUI.Emulation.Services." + implementationName, throwOnError: true)!;
        Assert.False(implementation.IsVisible);
        Assert.Contains(implementation.GetInterfaces(), item => item.IsGenericType && item.GetGenericTypeDefinition() == contract);
    }

    [Fact]
    public void EnvironmentFactoryDoesNotDisposeBorrowedAudioAndVideoAfterOptionCreationFails()
    {
        using var audio = EmulationSdk.NativeResources.CreateAudioCallbackHost();
        using var video = EmulationSdk.NativeResources.CreateVideoCallbackHost();
        var expected = new InvalidOperationException("option creation rejected");
        var error = Assert.Throws<InvalidOperationException>(() =>
        {
            using var unexpected = EmulationSdk.NativeResources.CreateEnvironmentCallbackHost(audio: audio, video: video, options: new Dictionary<string, string>(),
                systemDirectory: string.Empty, contentDirectory: string.Empty, saveDirectory: string.Empty, assetsDirectory: string.Empty, createOptions: _ => throw expected);
        });
        Assert.Same(expected, error);
        AssertBorrowedCallbackOwnersStillWork(audio, video);
    }

    [Fact]
    public void EnvironmentFactoryDisposesOwnedOptionsAfterDirectoryValidationFails()
    {
        using var audio = EmulationSdk.NativeResources.CreateAudioCallbackHost();
        using var video = EmulationSdk.NativeResources.CreateVideoCallbackHost();
        var options = new TrackedVariableOptions();
        try
        {
            Assert.Throws<ArgumentException>(() =>
            {
                using var unexpected = EmulationSdk.NativeResources.CreateEnvironmentCallbackHost(audio: audio, video: video, options: new Dictionary<string, string>(),
                    systemDirectory: string.Empty, contentDirectory: string.Empty, saveDirectory: string.Empty, assetsDirectory: string.Empty, createOptions: _ => options);
            });
            Assert.Equal(1, options.Disposals);
            AssertBorrowedCallbackOwnersStillWork(audio, video);
        }
        finally { if (options.Disposals == 0) options.Dispose(); }
    }

    private static void AssertBorrowedCallbackOwnersStillWork(IEmulationAudioCallbackHost audio, IEmulationVideoCallbackHost video)
    {
        audio.SampleRate = 20;
        audio.SampleCallback(1, 2);
        Assert.True(audio.TryDequeue(out _));
        video.Callback((nint)1, 1, 1, 1);
        Assert.NotNull(video.Failure);
    }

    private sealed class TrackedVariableOptions : IEmulationVariableOptionHost<EmulationEnabledOption>
    {
        internal int Disposals { get; private set; }
        public IReadOnlyList<EmulationEnabledOption> Options => [];
        public bool Register(nint data) => throw new NotSupportedException();
        public bool ReturnValue(nint data) => throw new NotSupportedException();
        public void SetValue(string key, string value) => throw new NotSupportedException();
        public bool ReturnUpdated(nint data) => throw new NotSupportedException();
        public void Dispose() => Disposals++;
    }

    [Theory]
    [InlineData(0u, true)]
    [InlineData(1u, true)]
    [InlineData(2u, true)]
    [InlineData(3u, true)]
    [InlineData(4u, false)]
    public void VirtualFileSystemNegotiatesTheExistingNativeContract(uint requestedVersion, bool accepted)
    {
        var files = EmulationSdk.NativeResources.CreateVirtualFileSystem();
        nint request = 0;
        try
        {
            Assert.False(files.GetType().IsVisible);
            Assert.False(files.Provide(nint.Zero));
            request = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.VfsInterfaceInfo>());
            var initial = new ExternalCoreApi.VfsInterfaceInfo { RequiredInterfaceVersion = requestedVersion, Interface = (nint)123 };
            Marshal.StructureToPtr(initial, request, false);
            Assert.Equal(accepted, files.Provide(request));
            var negotiated = Marshal.PtrToStructure<ExternalCoreApi.VfsInterfaceInfo>(request);
            if (accepted)
            {
                Assert.Equal(3u, negotiated.RequiredInterfaceVersion);
                Assert.NotEqual(nint.Zero, negotiated.Interface);
                var callbacks = Marshal.PtrToStructure<ExternalCoreApi.VfsInterface>(negotiated.Interface);
                Assert.NotEqual(nint.Zero, callbacks.Open);
                Assert.NotEqual(nint.Zero, callbacks.Read);
                Assert.NotEqual(nint.Zero, callbacks.Write);
                Assert.NotEqual(nint.Zero, callbacks.Close);
                Assert.NotEqual(nint.Zero, callbacks.OpenDirectory);
                Assert.NotEqual(nint.Zero, callbacks.CloseDirectory);
            }
            else
            {
                Assert.Equal(initial.RequiredInterfaceVersion, negotiated.RequiredInterfaceVersion);
                Assert.Equal(initial.Interface, negotiated.Interface);
            }
            files.Dispose();
            Assert.False(files.Provide(request));
            var afterDisposal = Marshal.PtrToStructure<ExternalCoreApi.VfsInterfaceInfo>(request);
            Assert.Equal(negotiated.RequiredInterfaceVersion, afterDisposal.RequiredInterfaceVersion);
            Assert.Equal(negotiated.Interface, afterDisposal.Interface);
        }
        finally
        {
            try { files.Dispose(); }
            finally { if (request != nint.Zero) Marshal.FreeHGlobal(request); }
        }
    }

    [Theory]
    [InlineData(nameof(EmulationSdk.CreateAudioBuffer), typeof(IEmulationAudioBuffer))]
    [InlineData(nameof(EmulationSdk.CreateVideoBufferSet), typeof(IEmulationVideoBufferSet))]
    [InlineData(nameof(EmulationSdk.CreateSharedVideoWriter), typeof(IEmulationSharedVideoWriter))]
    public void BufferFactoriesExposePublicInterfaces(string factoryName, Type contract)
    {
        var factory = typeof(EmulationSdk).GetMethod(factoryName)!;
        Assert.True(factory.IsPublic);
        Assert.Equal(contract, factory.ReturnType);
        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
    }

    [Theory]
    [InlineData(nameof(EmulationSdk.AudioProcessing), typeof(IEmulationAudioProcessing))]
    [InlineData(nameof(EmulationSdk.SessionDirectories), typeof(IEmulationSessionDirectories))]
    [InlineData(nameof(EmulationSdk.StateFiles), typeof(IEmulationStateFiles))]
    [InlineData(nameof(EmulationSdk.InputProcessing), typeof(IEmulationInputProcessing))]
    [InlineData(nameof(EmulationSdk.HostProtocol), typeof(IEmulationHostProtocol))]
    [InlineData(nameof(EmulationSdk.CoreReleases), typeof(IEmulationCoreReleases))]
    [InlineData(nameof(EmulationSdk.CoreDiagnostics), typeof(IEmulationCoreDiagnostics))]
    [InlineData(nameof(EmulationSdk.OptionProcessing), typeof(IEmulationCoreOptionProcessing))]
    [InlineData(nameof(EmulationSdk.EnvironmentProcessing), typeof(IEmulationCoreEnvironmentProcessing))]
    [InlineData(nameof(EmulationSdk.HardwareSettings), typeof(IEmulationHardwareSettings))]
    [InlineData(nameof(EmulationSdk.FirmwareProcessing), typeof(IEmulationFirmwareProcessing))]
    [InlineData(nameof(EmulationSdk.MediaRules), typeof(IEmulationMediaRules))]
    [InlineData(nameof(EmulationSdk.VideoProcessing), typeof(IEmulationVideoProcessing))]
    [InlineData(nameof(EmulationSdk.MachineExecution), typeof(IEmulationMachineExecution))]
    [InlineData(nameof(EmulationSdk.Messages), typeof(IEmulationMessages))]
    [InlineData(nameof(EmulationSdk.ConfigurationFiles), typeof(IEmulationConfigurationFiles))]
    [InlineData(nameof(EmulationSdk.Adapters), typeof(IEmulationAdapters))]
    [InlineData(nameof(EmulationSdk.NativeResources), typeof(IEmulationNativeResources))]
    public void SharedProcessingApisExposePublicInterfacesAndInternalImplementations(string serviceName, Type contract)
    {
        var entry = typeof(EmulationSdk).GetProperty(serviceName)!;
        Assert.Equal(contract, entry.PropertyType);
        Assert.True(contract.IsInterface);
        Assert.True(contract.IsPublic);
        Assert.False(entry.GetValue(null)!.GetType().IsVisible);
    }

    [Fact]
    public void NativeStringPreservesUtf8CapacityAndReleasesItsPointer()
    {
        var text = EmulationSdk.NativeResources.CreateString("été", minimumByteCapacity: 32);
        try
        {
            Assert.False(text.GetType().IsVisible);
            Assert.Equal("été", Marshal.PtrToStringUTF8(text.Pointer));
            var bytes = new byte[32];
            Marshal.Copy(text.Pointer, bytes, 0, bytes.Length);
            Assert.Equal(new byte[] { 0xc3, 0xa9, 0x74, 0xc3, 0xa9 }, bytes[..5]);
            Assert.All(bytes[5..], value => Assert.Equal((byte)0, value));
        }
        finally { text.Dispose(); }
        Assert.Equal(nint.Zero, text.Pointer);
        text.Dispose();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NativeStringRetainsTheRequestedPathEncoding(bool useWindowsAnsi)
    {
        var text = EmulationSdk.NativeResources.CreateString("été", useWindowsAnsi);
        try
        {
            var decoded = useWindowsAnsi && OperatingSystem.IsWindows() ? Marshal.PtrToStringAnsi(text.Pointer) : Marshal.PtrToStringUTF8(text.Pointer);
            Assert.Equal("été", decoded);
        }
        finally { text.Dispose(); }
    }

    [Fact]
    public void NativeStringRegistrationTransfersOwnershipOnlyAfterSuccessfulAdd()
    {
        var owner = new Dictionary<string, IEmulationNativeString>(StringComparer.Ordinal);
        try
        {
            var text = EmulationSdk.NativeResources.AddString(owner, "mode", "normal");
            Assert.Same(text, owner["mode"]);
            Assert.Throws<ArgumentException>(() => EmulationSdk.NativeResources.AddString(owner, "mode", "replacement"));
            Assert.Same(text, owner["mode"]);
            Assert.Equal("normal", Marshal.PtrToStringUTF8(text.Pointer));
        }
        finally
        {
            foreach (var text in owner.Values) text.Dispose();
        }
        Assert.All(owner.Values, text => Assert.Equal(nint.Zero, text.Pointer));
    }

    [Fact]
    public void NativeStringRejectsInvalidArgumentsBeforeAllocating()
    {
        Assert.Throws<ArgumentNullException>(() => EmulationSdk.NativeResources.CreateString(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => EmulationSdk.NativeResources.CreateString("text", -1));
    }

    [Fact]
    public void LoadedContentPathOnlyKeepsTheDescriptorAliveUntilDisposal()
    {
        var path = Path.Combine(Path.GetTempPath(), "exemple-é.bin");
        var content = EmulationSdk.NativeResources.CreateLoadedContent(path, needsFullPath: true, useWindowsAnsiPath: false);
        try
        {
            Assert.False(content.GetType().IsVisible);
            var descriptor = Marshal.PtrToStructure<ExternalCoreApi.GameInfo>(content.GameInfo);
            Assert.Equal(path, Marshal.PtrToStringUTF8(descriptor.Path));
            Assert.Equal(nint.Zero, descriptor.Data);
            Assert.Equal((nuint)0, descriptor.Size);
            Assert.Equal(nint.Zero, descriptor.Metadata);
        }
        finally { content.Dispose(); }
        Assert.Equal(nint.Zero, content.GameInfo);
        content.Dispose();
    }

    [Theory]
    [InlineData("EmulationInputMappingFunctions")]
    [InlineData("ExternalCoreKeyboardFunctions")]
    [InlineData("EmulationHostProtocolFunctions")]
    [InlineData("EmulationChildProcessLifetime")]
    [InlineData("EmulationCoreOptionFunctions")]
    [InlineData("ExternalCoreEnvironmentFunctions")]
    [InlineData("EmulationHardwareSettingsFunctions")]
    public void SharedAlgorithmsRemainInternalBehindThePublicProcessingContract(string implementation)
    {
        var owner = typeof(EmulationSdk).Assembly.GetType("GWGUI.Emulation.Functions." + implementation, throwOnError: true)!;
        Assert.False(owner.IsVisible);
        Assert.Empty(owner.GetMethods(BindingFlags.Public | BindingFlags.Static));
    }

    [Theory]
    [InlineData(typeof(IEmulationCoreOptionProcessing), "EmulationCoreOptionFunctions", 24)]
    [InlineData(typeof(IEmulationCoreEnvironmentProcessing), "ExternalCoreEnvironmentFunctions", 34)]
    [InlineData(typeof(IEmulationHardwareSettings), "EmulationHardwareSettingsFunctions", 12)]
    public void ProcessingContractsRetainEveryInternalOperationWithItsSignatureAndDefaults(Type contract, string implementation, int operationCount)
    {
        var owner = typeof(EmulationSdk).Assembly.GetType("GWGUI.Emulation.Functions." + implementation, throwOnError: true)!;
        var algorithms = owner.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(method => method.IsAssembly).ToArray();
        var api = contract.GetMethods();
        Assert.Equal(operationCount, algorithms.Length);
        Assert.Equal(algorithms.Length, api.Length);
        static string Signature(MethodInfo method) => method.Name + "`" + method.GetGenericArguments().Length + "(" +
            string.Join(",", method.GetParameters().Select(parameter => (parameter.IsOut ? "out " : string.Empty) + parameter.ParameterType + " " + parameter.Name +
                (parameter.HasDefaultValue ? "=" + (parameter.DefaultValue?.ToString() ?? "null") : string.Empty))) + "):" + method.ReturnType;
        Assert.Equal(algorithms.Select(Signature).Order(StringComparer.Ordinal), api.Select(Signature).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void SavedStateBinaryAlgorithmsRemainInternalBehindThePublicInterface()
    {
        var implementation = typeof(EmulationSdk).Assembly.GetType("GWGUI.Emulation.Functions.EmulationStateFileFunctions", throwOnError: true)!;
        Assert.False(implementation.IsVisible);
        foreach (var name in new[] { "Read", "ReadFile", "Write", "WriteFile" })
        {
            Assert.Null(implementation.GetMethod(name, BindingFlags.Public | BindingFlags.Static));
            Assert.True(implementation.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.IsAssembly);
        }
    }

    [Fact]
    public void ReleaseApiExposesAnInterfaceAndKeepsItsImplementationInternal()
    {
        var entry = typeof(EmulationSdk)
            .GetProperty(nameof(EmulationSdk.CoreReleases))!;
        Assert.Equal(typeof(IEmulationCoreReleases), entry.PropertyType);
        Assert.True(entry.PropertyType.IsPublic);
        var implementation = EmulationSdk.CoreReleases.GetType();
        Assert.False(implementation.IsVisible);
        Assert.Null(implementation.GetMethod("ReadJsonAsync", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
    }

    [Fact]
    public void DiagnosticApiExposesAnInterfaceAndKeepsItsImplementationInternal()
    {
        var entry = typeof(EmulationSdk)
            .GetProperty(nameof(EmulationSdk.CoreDiagnostics))!;
        Assert.Equal(typeof(IEmulationCoreDiagnostics), entry.PropertyType);
        Assert.True(entry.PropertyType.IsPublic);
        var implementation = EmulationSdk.CoreDiagnostics.GetType();
        Assert.False(implementation.IsVisible);
        foreach (var name in new[] { "ReadArchitecture", "ReadDeclaredVersion", "ReadExports" })
            Assert.Null(implementation.GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
    }

    [Theory]
    [InlineData("x64", true)]
    [InlineData("X64", false)]
    [InlineData("x64 ", false)]
    [InlineData("arm64", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ReadsTheErrorOnlyOnceAndOnlyAfterOrdinalRejection(string? actual, bool accepted)
    {
        var reads = 0;
        string Message()
        {
            reads++;
            return "architecture refusée";
        }

        if (accepted)
        {
            EmulationSdk.CoreDiagnostics.RequireArchitecture(actual, "x64", Message);
            Assert.Equal(0, reads);
        }
        else
        {
            var error = Assert.Throws<InvalidDataException>(() =>
                EmulationSdk.CoreDiagnostics.RequireArchitecture(actual, "x64", Message));
            Assert.Equal("architecture refusée", error.Message);
            Assert.Equal(1, reads);
        }
    }

    [Fact]
    public void ConfigurationFilesExposeExistingPersistenceOperationsThroughThePublicContract()
    {
        var contract = typeof(IEmulationConfigurationFiles);
        Assert.False(EmulationSdk.ConfigurationFiles.GetType().IsVisible);
        foreach (var name in new[] { "StorePath", "ResolvePath", "DeleteIfExists", "DeleteDirectoryIfExists", "EnumerateDocumentPaths", "WithSaveGateAsync" })
            Assert.Single(contract.GetMethods(), method => method.Name == name);
        var writes = contract.GetMethods().Where(method => method.Name == "WriteDocumentAsync").ToArray();
        Assert.Equal(new[] { 4, 8 }, writes.Select(method => method.GetParameters().Length).Order().ToArray());
        Assert.Equal(3, contract.GetMethods().Count(method => method.Name == "ReadDocumentsAsync"));
    }

    [Fact]
    public void PreservesTheMessageProviderFailureAfterRejection()
    {
        var expected = new InvalidOperationException("message unavailable");
        string Message() => throw expected;

        EmulationSdk.CoreDiagnostics.RequireArchitecture("x64", "x64", Message);
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() =>
            EmulationSdk.CoreDiagnostics.RequireArchitecture("unknown", "x64", Message)));
    }
}
