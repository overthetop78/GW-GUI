using GWGUI.Emulation;
using GWGUI.Emulation.Enums;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class ArchitectureValidationTests
{
    [Fact]
    public async Task EveryCommonUsesItsControllerContractForPortCounts()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var checkedConfigurations = 0;
        var describedAdapters = new HashSet<string>(StringComparer.Ordinal);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var engine = Activator.CreateInstance(engineType)!;
            var controllerContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorControllerAdapter", throwOnError: true)!;
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.True(controllerContract.IsAssignableFrom(adapterContract), root);
            Assert.Equal(new[] { "DescribeInput", "GetControllerBindingDefinitions", "GetControllerPortCount", "GetControllerPortDisplayName", "GetControllerPortDisplayNumber", "GetPeripherals", "GetPeripherals", "GetUsbPeripherals" }, controllerContract.GetMethods().Select(method => method.Name).Order(StringComparer.Ordinal));
            Assert.Contains(controllerContract.GetInterfaces(), type => type.Name == "IEmulatorInputMapper");
            Assert.All(controllerContract.GetMethods(), method => Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(method.GetParameters()[0].ParameterType)));
            var countMethod = controllerContract.GetMethod("GetControllerPortCount")!;
            Assert.Equal(typeof(int), countMethod.ReturnType);
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(Assert.Single(countMethod.GetParameters()).ParameterType));
            var functions = assembly.GetType(root + ".Common.Functions.ControllerFunctions", throwOnError: true)!;
            var count = functions.GetMethod("PortCount", BindingFlags.Static | BindingFlags.NonPublic)!;
            var modelCount = functions.GetMethod("ModelPortCount", BindingFlags.Static | BindingFlags.NonPublic)!;
            Assert.Contains(GetCompiledCalls(count), call => call.Method.DeclaringType == controllerContract && call.Method.Name == countMethod.Name);
            var settingsFunctions = assembly.GetType(root + ".Common.Machines.Common.Functions.InputSettingsFunctions", throwOnError: true)!;
            Assert.Contains(settingsFunctions.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).SelectMany(GetCompiledCalls), call => call.Method.DeclaringType == functions && call.Method.Name == count.Name);
            var adaptersById = engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine)!;
            var adapters = ((System.Collections.IEnumerable)adaptersById.GetType().GetProperty("Values")!.GetValue(adaptersById)!).Cast<object>().ToArray();
            var machines = (IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!;
            foreach (var machine in machines)
            {
                var original = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                var candidates = adapters.Select(adapter => (Adapter: adapter, Definition: (EmulationEmulatorDefinition)adapter.GetType().GetInterfaces()
                    .SelectMany(type => type.GetProperties()).First(property => property.Name == "Definition").GetValue(adapter)!))
                    .Where(item => item.Definition.MachineIds.Contains(machine.Id)).ToArray();
                if (candidates.Length == 0)
                {
                    Assert.Equal(modelCount.Invoke(null, [original]), count.Invoke(null, [original]));
                    checkedConfigurations++;
                }
                foreach (var candidate in candidates)
                {
                    var configuration = await (ValueTask<IEmulationConfiguration>)engineType.GetMethod("UseEmulatorAsync")!.Invoke(engine, [original, candidate.Definition.Id, CancellationToken.None])!;
                    var expected = (int)countMethod.Invoke(candidate.Adapter, [configuration])!;
                    Assert.InRange(expected, 0, EmulationHostProtocolConstants.MaximumInputControllerCount);
                    Assert.Equal(expected, count.Invoke(null, [configuration]));
                    var settings = (EmulationInputSettings)engineType.GetMethod("DescribeInputSettings")!.Invoke(engine, [configuration])!;
                    // Les deux ports USB existants de PS2 sont décrits en plus du bus de manettes.
                    var extraUsbPorts = root.EndsWith(".Sony", StringComparison.Ordinal) && machine.Id == "PlayStation2" && candidate.Definition.Id != "pcee2" ? 2 : 0;
                    if (describedAdapters.Add(root + "/" + candidate.Definition.Id))
                    {
                        var map = candidate.Adapter.GetType().GetInterfaceMap(controllerContract);
                        var categoryType = controllerContract.GetMethod("DescribeInput")!.GetParameters()[2].ParameterType;
                        foreach (var method in controllerContract.GetMethods())
                        {
                            var parameters = method.GetParameters();
                            if (settings.ControllerPorts.Count == 0 && method.Name is not ("GetControllerPortCount" or "GetUsbPeripherals") && !(method.Name == "GetPeripherals" && parameters.Length == 1)) continue;
                            var firstPort = settings.ControllerPorts.FirstOrDefault(port => !port.IsUsb) ?? settings.ControllerPorts.FirstOrDefault();
                            var firstCategory = firstPort is not null && Enum.TryParse(categoryType, firstPort.SelectedControllerId, true, out var parsedCategory) ? parsedCategory : Enum.GetValues(categoryType).GetValue(0);
                            object?[] arguments = parameters.Select((parameter, index) => index == 0 ? configuration : parameter.ParameterType == typeof(int) ? method.Name == "GetUsbPeripherals" ? (object)0 : firstPort?.Number - 1 ?? 0 : firstCategory).ToArray();
                            var result = method.Invoke(candidate.Adapter, arguments);
                            Assert.False(map.TargetMethods[Array.IndexOf(map.InterfaceMethods, method)].IsAbstract);
                            if (method.Name is "GetPeripherals" or "GetUsbPeripherals")
                                Assert.All(((System.Collections.IEnumerable)result!).Cast<object>(), category => Assert.True(Enum.IsDefined(categoryType, category)));
                        }
                        foreach (var port in settings.ControllerPorts)
                        {
                            if (!Enum.TryParse(categoryType, port.SelectedControllerId, true, out var peripheral)) continue;
                            var index = port.Number - 1;
                            foreach (var method in controllerContract.GetMethods().Where(method => method.Name is "DescribeInput" or "GetControllerBindingDefinitions" or "GetControllerPortDisplayName" or "GetControllerPortDisplayNumber"))
                            {
                                var target = map.TargetMethods[Array.IndexOf(map.InterfaceMethods, method)];
                                var arguments = method.Name switch
                                {
                                    "DescribeInput" => new[] { configuration, (object)index, peripheral },
                                    "GetControllerBindingDefinitions" => new[] { configuration, peripheral, (object)index },
                                    _ => new[] { configuration, (object)index }
                                };
                                var result = method.Invoke(candidate.Adapter, arguments);
                                if (target.DeclaringType != adapterContract) continue;
                                if (method.Name == "DescribeInput") Assert.Equal(EmulationSdk.InputProcessing.DescribeInput(settings, index, port.SelectedControllerId), Assert.IsAssignableFrom<IReadOnlyList<InputActionDefinition>>(result));
                                else if (method.Name == "GetControllerBindingDefinitions") Assert.Equal(EmulationSdk.InputProcessing.GetControllerBindingDefinitions(settings, port.SelectedControllerId, index), result as IReadOnlyList<InputBindingDefinition>);
                                else if (method.Name == "GetControllerPortDisplayName") Assert.Equal(port.DisplayName, result);
                                else Assert.Equal(port.DisplayNumber, result);
                            }
                        }
                    }
                    Assert.True(settings.ControllerPorts.Count == expected + extraUsbPorts, $"{root}/{machine.Id}/{candidate.Definition.Id}: {settings.ControllerPorts.Count} ports décrits, {expected} annoncés, {extraUsbPorts} USB supplémentaires");
                    checkedConfigurations++;
                }
            }
        }
        Assert.True(checkedConfigurations > 24);
    }
    public sealed class ControllerDescriptionProbe
    {
        public List<IEmulationConfiguration> Configurations { get; } = [];

        public IReadOnlyList<InputActionDefinition> Describe<TConfiguration, TPeripheral>(TConfiguration configuration, int port, TPeripheral peripheral) where TConfiguration : IEmulationConfiguration
        {
            Configurations.Add(configuration);
            return [];
        }
    }

    [Fact]
    public void NativeControllerMappingPassesTheOriginalSessionToItsDescriptionCallback()
    {
        var assembly = Assembly.Load("gwgui.emulation.coleco");
        const string root = "GWGUI.Emulation.Coleco";
        var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
        var engine = Activator.CreateInstance(engineType)!;
        var machine = ((IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!).First();
        var configuration = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
        var functions = assembly.GetType(root + ".Emulators.Common.Interop.Functions.NativeInputSnapshotFunctions", throwOnError: true)!;
        var apply = functions.GetMethod("Apply", BindingFlags.Static | BindingFlags.NonPublic)!;
        var callbackType = apply.GetParameters()[3].ParameterType;
        var callbackArguments = callbackType.GetGenericArguments();
        var probe = new ControllerDescriptionProbe();
        var callbackMethod = typeof(ControllerDescriptionProbe).GetMethod(nameof(ControllerDescriptionProbe.Describe))!
            .MakeGenericMethod(callbackArguments[0], callbackArguments[2]);
        var callback = callbackMethod.CreateDelegate(callbackType, probe);
        var core = configuration.GetType().GetProperty("Core")!.GetValue(configuration);
        var snapshot = Assert.IsType<EmulationInputSnapshot>(apply.Invoke(null, [EmulationInputSnapshot.Empty, configuration, core, callback]));
        Assert.NotEmpty(probe.Configurations);
        Assert.All(probe.Configurations, captured => Assert.Same(configuration, captured));
        Assert.Equal(probe.Configurations.Count, snapshot.Controllers.Count);
    }
    public class FeedbackCoreProxy : DispatchProxy
    {
        public IReadOnlyList<ControllerFeedbackState> States { get; set; } = [];
        public int Reads { get; private set; }
        public Exception? ControllerChangeError { get; set; }
        public object? MappedConfiguration { get; private set; }
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod!.Name == "get_ControllerFeedback") { Reads++; return States; }
            if (targetMethod.Name == "SetControllerPortDevice") { if (ControllerChangeError is { } error) throw error; return null; }
            if (targetMethod.Name == "MapInput") { MappedConfiguration = args![1]; return args[0]; }
            if (targetMethod.Name == "Dispose") return null;
            throw new InvalidOperationException(targetMethod.Name);
        }
    }

    [Theory]
    [InlineData("Acorn")]
    [InlineData("Apple")]
    [InlineData("Atari")]
    public void PeripheralChangesUpdateSessionInputOnlyAfterCoreAcceptance(string family)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var root = "GWGUI.Emulation." + family;
        var engineType = assembly.GetType(root + ".Common.Services.Engine")!;
        var engine = Activator.CreateInstance(engineType)!;
        var models = (IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!;
        var configuration = models.Select(model => engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [model.Id])!)
            .First(value => ((EmulationInputSettings)engineType.GetMethod("DescribeInputSettings")!.Invoke(engine, [value])!).ControllerPorts.Count > 0);
        var settings = (EmulationInputSettings)engineType.GetMethod("DescribeInputSettings")!.Invoke(engine, [configuration])!;
        var first = settings.ControllerPorts[0] with
        {
            PhysicalDeviceId = "physical-device", DeadZonePercent = 17,
            OutputBindings = new Dictionary<string, EmulationControllerOutputBinding> { [EmulationControllerFeedbackCommands.Strong] = new("motor-device", EmulationControllerMotor.LowFrequency) }
        };
        var ports = settings.ControllerPorts.Select(port => port.Number == first.Number ? first : port).ToArray();
        var applied = engineType.GetMethod("ApplyInputSettings")!.Invoke(engine, [configuration, settings with { ControllerPorts = ports }])!;
        var machineType = assembly.GetType(root + ".Common.Services.Machine")!;
        var coreType = assembly.GetType(root + ".Common.Interfaces.IEmulatorCore")!;
        var core = DispatchProxy.Create(coreType, typeof(FeedbackCoreProxy));
        try
        {
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            var owner = RuntimeHelpers.GetUninitializedObject(machineType);
            machineType.GetField("<Configuration>k__BackingField", fields)!.SetValue(owner, applied);
            var sessionField = machineType.GetField("_inputConfiguration", fields)!;
            sessionField.SetValue(owner, applied);
            machineType.GetField("_core", fields)!.SetValue(owner, core);
            var media = machineType.GetField("_mountedMedia", fields)!;
            media.SetValue(owner, Activator.CreateInstance(media.FieldType));
            var setter = machineType.GetMethod("SetControllerPortDevice")!;
            var callback = Assert.Single(GetCompiledCalls(setter), call => call.Method.Name.Contains("<SetControllerPortDevice>", StringComparison.Ordinal)).Method as MethodInfo;
            var closure = RuntimeHelpers.GetUninitializedObject(callback!.DeclaringType!);
            callback.DeclaringType!.GetField("<>4__this")!.SetValue(closure, owner);
            callback.DeclaringType.GetField("port")!.SetValue(closure, first.Number - 1);
            var category = callback.DeclaringType.GetField("peripheral")!;
            category.SetValue(closure, Enum.Parse(category.FieldType, "None"));
            // Exécute le corps réel de la commande sans créer de thread ni de cœur natif.
            callback.Invoke(closure, []);
            var session = sessionField.GetValue(owner)!;
            Assert.NotSame(applied, session);
            var updated = (EmulationInputSettings)engineType.GetMethod("DescribeInputSettings")!.Invoke(engine, [session])!;
            var changed = Assert.Single(updated.ControllerPorts, port => port.Number == first.Number);
            Assert.Equal("None", changed.SelectedControllerId);
            Assert.Equal(first.PhysicalDeviceId, changed.PhysicalDeviceId);
            Assert.Equal(first.DeadZonePercent, changed.DeadZonePercent);
            Assert.Equal(first.Bindings.Values, changed.Bindings.Values);
            Assert.Equal(first.OutputBindings, changed.OutputBindings);
            var current = machineType.GetMethod("CurrentConfiguration", fields)!.Invoke(owner, [])!;
            Assert.Same(session.GetType().GetProperty("Input")!.GetValue(session), current.GetType().GetProperty("Input")!.GetValue(current));
            var mapper = DispatchProxy.Create(assembly.GetType(root + ".Common.Interfaces.IEmulatorInputMapper")!, typeof(FeedbackCoreProxy));
            machineType.GetField("_inputMapper", fields)!.SetValue(owner, mapper);
            machineType.GetMethod("MapInput", fields)!.Invoke(owner, [EmulationInputSnapshot.Empty]);
            Assert.Same(session, ((FeedbackCoreProxy)mapper).MappedConfiguration);
            var refused = new InvalidOperationException("refused");
            ((FeedbackCoreProxy)core).ControllerChangeError = refused;
            Assert.Same(refused, Assert.Throws<TargetInvocationException>(() => callback.Invoke(closure, [])).InnerException);
            Assert.Same(session, sessionField.GetValue(owner));
            Assert.NotEqual("None", ((EmulationInputSettings)engineType.GetMethod("DescribeInputSettings")!.Invoke(engine, [applied])!).ControllerPorts[0].SelectedControllerId);
        }
        finally { ((IDisposable)core).Dispose(); }
    }

    [Fact]
    public async Task CommonMachinesProjectConfiguredEffectsFromTheirCoreInMemory()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        var activeFamilies = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var engineType = assembly.GetType(root + ".Common.Services.Engine")!;
            var engine = Activator.CreateInstance(engineType)!;
            var descriptions = (IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!;
            var outputContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorControllerFeedbackAdapter")!;
            var outputMethod = outputContract.GetMethod("GetControllerOutputs")!;
            var peripheralType = outputMethod.GetParameters()[1].ParameterType;
            var exercised = false;
            foreach (var description in descriptions)
            {
                var configuration = engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [description.Id])!;
                var adapter = engineType.GetMethod("SelectedAdapter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(engine, [configuration]);
                if (adapter is null)
                {
                    var adapters = engineType.GetMethod("Adapters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var catalog = assembly.GetType(root + ".Common.Dictionaries.EmulatorCatalog")!;
                    var values = adapters?.Invoke(engine, [description.Id]) ?? catalog.GetMethod("CreateAdapters", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, []);
                    var available = (values as System.Collections.IEnumerable)?.Cast<object>().FirstOrDefault();
                    if (available is null) continue;
                    var id = (string)available.GetType().GetInterfaces().SelectMany(type => type.GetProperties()).First(property => property.Name == "EmulatorId").GetValue(available)!;
                    configuration = await (ValueTask<IEmulationConfiguration>)engineType.GetMethod("UseEmulatorAsync")!.Invoke(engine, [configuration, id, CancellationToken.None])!;
                    adapter = engineType.GetMethod("SelectedAdapter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(engine, [configuration]);
                }
                if (adapter is null) continue;
                var settings = (EmulationInputSettings)engineType.GetMethod("DescribeInputSettings")!.Invoke(engine, [configuration])!;
                foreach (var port in settings.ControllerPorts)
                {
                    var choice = port.ControllerChoices.FirstOrDefault(item => Enum.TryParse(peripheralType, item.Id, true, out var category)
                        && ((IReadOnlyList<EmulationControllerOutputDefinition>)outputMethod.Invoke(adapter, [configuration, category])!).Count != 0);
                    if (choice is null) continue;
                    var outputs = new Dictionary<string, EmulationControllerOutputBinding>
                    {
                        [EmulationControllerFeedbackCommands.Strong] = new("strong-device", EmulationControllerMotor.LowFrequency),
                        [EmulationControllerFeedbackCommands.Weak] = new("weak-device", EmulationControllerMotor.HighFrequency)
                    };
                    var ports = settings.ControllerPorts.Select(item => item.Number == port.Number ? item with { SelectedControllerId = choice.Id, VisualId = choice.DefaultVisualId ?? item.VisualId, OutputBindings = outputs } : item).ToArray();
                    var applied = engineType.GetMethod("ApplyInputSettings")!.Invoke(engine, [configuration, settings with { ControllerPorts = ports }])!;
                    var machineType = assembly.GetType(root + ".Common.Services.Machine")!;
                    var coreType = assembly.GetType(root + ".Common.Interfaces.IEmulatorCore")!;
                    var core = DispatchProxy.Create(coreType, typeof(FeedbackCoreProxy));
                    try
                    {
                        var proxy = (FeedbackCoreProxy)core;
                        proxy.States = [new(port.Number - 1, EmulationControllerFeedbackCommands.Strong, .25f), new(port.Number - 1, EmulationControllerFeedbackCommands.Weak, 1f)];
                        // Seules les données du getter sont fournies : aucun constructeur de runtime, fichier ou thread n’est utilisé.
                        var owner = RuntimeHelpers.GetUninitializedObject(machineType);
                        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                        (machineType.GetField("_configuration", flags) ?? machineType.GetField("<Configuration>k__BackingField", flags))!.SetValue(owner, applied);
                        machineType.GetField("_gate", flags)!.SetValue(owner, new object());
                        machineType.GetField("_core", flags)!.SetValue(owner, core);
                        var input = applied.GetType().GetProperty("Input")!.GetValue(applied);
                        var inputField = machineType.GetField("_inputConfiguration", flags);
                        inputField?.SetValue(owner, inputField.FieldType.IsInstanceOfType(applied) ? applied : input);
                        var media = machineType.GetField("_mountedMedia", flags);
                        if (media is not null) media.SetValue(owner, Activator.CreateInstance(media.FieldType));
                        machineType.GetProperty("State")!.SetValue(owner, EmulationMachineState.Running);
                        Assert.Equal(new[] { new EmulationControllerOutputState("strong-device", EmulationControllerMotor.LowFrequency, .25f), new EmulationControllerOutputState("weak-device", EmulationControllerMotor.HighFrequency, 1f) }, ((IEmulationControllerFeedbackSource)owner).ControllerOutputs);
                        Assert.Equal(1, proxy.Reads);
                        machineType.GetProperty("State")!.SetValue(owner, EmulationMachineState.Paused);
                        Assert.Empty(((IEmulationControllerFeedbackSource)owner).ControllerOutputs);
                        Assert.Equal(1, proxy.Reads);
                        machineType.GetProperty("State")!.SetValue(owner, EmulationMachineState.Running);
                        Assert.Equal(2, ((IEmulationControllerFeedbackSource)owner).ControllerOutputs.Count);
                        Assert.Equal(2, proxy.Reads);
                        var currentSettings = (EmulationInputSettings)engineType.GetMethod("DescribeInputSettings")!.Invoke(engine, [applied])!;
                        var clearedPorts = currentSettings.ControllerPorts.Select(item => item with { OutputBindings = new Dictionary<string, EmulationControllerOutputBinding>() }).ToArray();
                        var cleared = engineType.GetMethod("ApplyInputSettings")!.Invoke(engine, [applied, currentSettings with { ControllerPorts = clearedPorts }])!;
                        if (inputField is not null)
                            inputField.SetValue(owner, inputField.FieldType.IsInstanceOfType(cleared) ? cleared : cleared.GetType().GetProperty("Input")!.GetValue(cleared));
                        else
                            (machineType.GetField("_configuration", flags) ?? machineType.GetField("<Configuration>k__BackingField", flags))!.SetValue(owner, cleared);
                        Assert.Empty(((IEmulationControllerFeedbackSource)owner).ControllerOutputs);
                        Assert.Equal(2, proxy.Reads);
                    }
                    finally { ((IDisposable)core).Dispose(); }
                    exercised = true;
                    break;
                }
                if (exercised) break;
            }
            if (exercised) activeFamilies++;
            else Assert.Contains(assembly.GetName().Name, new[] { "gwgui.emulation.microsoft", "gwgui.emulation.wenquxing" });
        }
        Assert.Equal(22, activeFamilies);
    }

    private static List<(int Offset, MethodBase Method)> GetCompiledCalls(MethodInfo source)
    {
        var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Where(field => field.FieldType == typeof(OpCode))
            .Select(field => (OpCode)field.GetValue(null)!).GroupBy(opcode => unchecked((ushort)opcode.Value)).ToDictionary(group => group.Key, group => group.First());
        var code = source.GetMethodBody()!.GetILAsByteArray()!;
        var result = new List<(int, MethodBase)>();
        for (var offset = 0; offset < code.Length;)
        {
            var start = offset;
            ushort value = code[offset++];
            if (value == 0xfe) value = (ushort)(0xfe00 | code[offset++]);
            var opcode = opcodes[value];
            if (opcode.OperandType == OperandType.InlineMethod)
                result.Add((start, source.Module.ResolveMethod(BitConverter.ToInt32(code, offset), source.DeclaringType?.GetGenericArguments(), source.GetGenericArguments())!));
            offset += opcode.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineBrTarget or OperandType.InlineField or OperandType.InlineI or OperandType.InlineMethod or OperandType.InlineSig or OperandType.InlineString or OperandType.InlineType or OperandType.ShortInlineR => 4,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(code, offset),
                _ => throw new InvalidOperationException(opcode.Name)
            };
        }
        return result;
    }

    [Fact]
    public void EveryCommonPublishesFeedbackThroughTheSameServiceAndStopsInactiveOutputs()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var gateway = assembly.GetType(root + ".Common.Functions.ControllerFunctions", throwOnError: true)!.GetProperty("Feedback", BindingFlags.Static | BindingFlags.NonPublic)!;
            Assert.Equal(typeof(IEmulationControllerFeedbackProcessing), gateway.PropertyType);
            Assert.Same(EmulationSdk.ControllerFeedback, gateway.GetValue(null));
            var machine = assembly.GetType(root + ".Common.Services.Machine", throwOnError: true)!;
            Assert.Contains(typeof(IEmulationControllerFeedbackSource), machine.GetInterfaces());
            var output = machine.GetProperty("ControllerOutputs")!.GetMethod!;
            var calls = GetCompiledCalls(output);
            Assert.Contains(calls, call => call.Method.DeclaringType == typeof(IEmulationControllerFeedbackProcessing) && call.Method.Name == nameof(IEmulationControllerFeedbackProcessing.CreateBindings));
            Assert.Contains(calls, call => call.Method.DeclaringType == typeof(IEmulationControllerFeedbackProcessing) && call.Method.Name == nameof(IEmulationControllerFeedbackProcessing.ResolveStates));
            var state = machine.GetProperty("State")!;
            var disposed = machine.GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic)!;
            // Les objets non initialisés servent seulement aux gardes : aucun cœur, thread, flux ou périphérique n’est créé.
            var owner = RuntimeHelpers.GetUninitializedObject(machine);
            foreach (var inactive in Enum.GetValues<EmulationMachineState>().Where(value => value != EmulationMachineState.Running))
            {
                state.SetValue(owner, inactive);
                Assert.Empty(((IEmulationControllerFeedbackSource)owner).ControllerOutputs);
            }
            state.SetValue(owner, EmulationMachineState.Running);
            disposed.SetValue(owner, true);
            Assert.Empty(((IEmulationControllerFeedbackSource)owner).ControllerOutputs);
            var suspended = machine.GetField("_mediaSuspended", BindingFlags.Instance | BindingFlags.NonPublic);
            if (suspended is not null)
            {
                disposed.SetValue(owner, false);
                suspended.SetValue(owner, true);
                Assert.Empty(((IEmulationControllerFeedbackSource)owner).ControllerOutputs);
            }
        }
    }

    [Fact]
    public void EveryProcessHostAndReaderUseTheSharedFeedbackCodec()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var checkedPairs = 0;
        foreach (var assembly in modules)
        {
            var clients = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.Name == "ProcessCore").ToArray();
            if (assembly.GetName().Name == "gwgui.emulation.microsoft") { Assert.Empty(clients); continue; }
            var client = Assert.Single(clients);
            var host = assembly.GetType(client.Namespace + ".CoreHost", throwOnError: true)!;
            var property = client.GetProperty("ControllerFeedback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)!;
            Assert.Equal(typeof(IReadOnlyList<ControllerFeedbackState>), property.PropertyType);
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodInfo[] Methods(Type owner) => assembly.GetTypes().Where(type => type == owner || type.FullName!.StartsWith(owner.FullName + "+", StringComparison.Ordinal))
                .SelectMany(type => type.GetMethods(flags)).Where(method => method.GetMethodBody() is not null).ToArray();
            var writes = Methods(host).SelectMany(method => GetCompiledCalls(method)).Where(call => call.Method.DeclaringType == typeof(IEmulationHostProtocol) && call.Method.Name == nameof(IEmulationHostProtocol.WriteControllerFeedback)).ToArray();
            Assert.Equal(2, writes.Length);
            var reads = Methods(client).Where(method => GetCompiledCalls(method).Any(call => call.Method.DeclaringType == typeof(IEmulationHostProtocol) && call.Method.Name == nameof(IEmulationHostProtocol.ReadControllerFeedback))).ToArray();
            Assert.Equal(2, reads.Length);
            foreach (var reader in reads)
                Assert.Contains(GetCompiledCalls(reader), call => call.Method.DeclaringType == client && call.Method.Name == property.SetMethod!.Name);
            Assert.Null(assembly.GetType("GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.ControllerFeedbackProtocol"));
            checkedPairs++;
        }
        Assert.Equal(23, checkedPairs);
    }

    [Fact]
    public void SharedFeedbackCodecPreservesBothMotorsAtItsCapacityAndTheFollowingField()
    {
        var states = Enumerable.Range(0, 10).SelectMany(port => new[]
        {
            new ControllerFeedbackState(port, EmulationControllerFeedbackCommands.Strong, 1f),
            new ControllerFeedbackState(port, EmulationControllerFeedbackCommands.Weak, 0f)
        }).ToArray();
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        EmulationSdk.HostProtocol.WriteControllerFeedback(writer, states);
        EmulationSdk.HostProtocol.WriteControllerFeedback(writer, []);
        writer.Write(123456);
        writer.Flush();
        stream.Position = 0;
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        Assert.Equal(states, EmulationSdk.HostProtocol.ReadControllerFeedback(reader));
        Assert.Empty(EmulationSdk.HostProtocol.ReadControllerFeedback(reader));
        Assert.Equal(123456, reader.ReadInt32());
        Assert.Equal(stream.Length, stream.Position);
    }

    [Theory]
    [InlineData(-1, "RumbleStrong", .5f)]
    [InlineData(10, "RumbleWeak", .5f)]
    [InlineData(0, "Unknown", .5f)]
    [InlineData(0, "RumbleStrong", float.NaN)]
    [InlineData(0, "RumbleWeak", float.PositiveInfinity)]
    [InlineData(0, "RumbleWeak", -1f)]
    [InlineData(0, "RumbleWeak", 2f)]
    public void SharedFeedbackCodecRejectsInvalidStatesBeforeWriting(int port, string command, float strength)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        Assert.Throws<InvalidDataException>(() => EmulationSdk.HostProtocol.WriteControllerFeedback(writer, [new(port, command, strength)]));
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public void SharedFeedbackCodecRejectsDuplicatesOversizedSnapshotsAndTruncatedPackets()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        var state = new ControllerFeedbackState(0, EmulationControllerFeedbackCommands.Strong, .5f);
        Assert.Throws<InvalidDataException>(() => EmulationSdk.HostProtocol.WriteControllerFeedback(writer, [state, state]));
        Assert.Throws<InvalidDataException>(() => EmulationSdk.HostProtocol.WriteControllerFeedback(writer, Enumerable.Repeat(state, 21).ToArray()));
        Assert.Equal(0, stream.Length);
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        foreach (var count in new[] { -1, 21, int.MaxValue })
        {
            stream.SetLength(0); stream.Position = 0;
            writer.Write(count); writer.Flush(); stream.Position = 0;
            Assert.Throws<InvalidDataException>(() => EmulationSdk.HostProtocol.ReadControllerFeedback(reader));
        }
        stream.SetLength(0); stream.Position = 0;
        writer.Write(1); writer.Write(0); writer.Write(true); writer.Flush(); stream.Position = 0;
        Assert.Throws<EndOfStreamException>(() => EmulationSdk.HostProtocol.ReadControllerFeedback(reader));
    }

    [Fact]
    public void EveryCoreUsesTheSharedFeedbackContractAndNativeReceiversKeepEffects()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        string[] receivers = ["Acorn", "Amstrad", "Apple", "Atari", "Bandai", "Coleco", "Commodore", "Epoch", "Fairchild", "Mattel", "Nec", "Nintendo", "Philips", "Sega", "Sharp", "Sinclair", "Snk"];
        var checkedReceivers = 0;
        var sharedHosts = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            Assert.Null(assembly.GetType(root + ".Common.Machines.Common.Contracts.ControllerFeedbackState"));
            var core = assembly.GetType(root + ".Common.Interfaces.IEmulatorCore", throwOnError: true)!;
            var runtime = Assert.Single(core.GetInterfaces(), type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEmulationCoreRuntime<,>));
            var property = runtime.GetProperty("ControllerFeedback")!;
            Assert.Equal(typeof(IReadOnlyList<ControllerFeedbackState>), property.PropertyType);
            Assert.Null(core.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));
            foreach (var implementation in assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && core.IsAssignableFrom(type)))
            {
                var map = implementation.GetInterfaceMap(runtime);
                var target = map.TargetMethods[Array.IndexOf(map.InterfaceMethods, property.GetMethod!)];
                Assert.False(target.IsAbstract);
            }
            var nativeName = root switch
            {
                "GWGUI.Emulation.Panasonic" => "Opera",
                "GWGUI.Emulation.Thomson" => "Theodore",
                "GWGUI.Emulation.Watara" => "Potator",
                "GWGUI.Emulation.Wenquxing" => "Wqxemu",
                _ => null
            };
            if (nativeName is not null)
            {
                var native = assembly.GetType(root + ".Emulators." + nativeName + ".Interop.Services.ExternalCore", throwOnError: true)!;
                Assert.Equal(typeof(IReadOnlyList<ControllerFeedbackState>), native.GetProperty("ControllerFeedback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)!.PropertyType);
                Assert.Equal(typeof(IEmulationEnvironmentCallbackHost), native.GetField("_environment", BindingFlags.Instance | BindingFlags.NonPublic)!.FieldType);
                sharedHosts++;
            }
            if (!receivers.Any(family => root == "GWGUI.Emulation." + family)) continue;
            var callbacks = assembly.GetType(root + ".Emulators.Common.Interop.Services.ExternalHostCallbacks", throwOnError: true)!;
            var feedback = callbacks.GetProperty("Feedback", BindingFlags.Instance | BindingFlags.NonPublic)!;
            Assert.Equal(typeof(IEmulationControllerFeedbackAccumulator), feedback.PropertyType);
            // Ce récepteur non initialisé ne possède ni cœur, ni flux, ni ressource native : seul son stockage est fourni.
            var receiver = RuntimeHelpers.GetUninitializedObject(callbacks);
            using var states = EmulationSdk.EnvironmentProcessing.CreateControllerFeedbackAccumulator(2);
            callbacks.GetField("<Feedback>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(receiver, states);
            var receive = callbacks.GetMethod("OnSetRumbleState", BindingFlags.Instance | BindingFlags.NonPublic)!;
            Assert.True(Assert.IsType<bool>(receive.Invoke(receiver, [0u, 0u, ushort.MaxValue])));
            Assert.Equal(new ControllerFeedbackState(0, EmulationControllerFeedbackCommands.Strong, 1f), Assert.Single(states.Snapshot()));
            Assert.False(Assert.IsType<bool>(receive.Invoke(receiver, [2u, 0u, ushort.MaxValue])));
            checkedReceivers++;
        }
        Assert.Equal(17, checkedReceivers);
        Assert.Equal(4, sharedHosts);
    }

    [Fact]
    public void EveryCommonPreservesControllerOutputBindingsInSettingsAndConfiguration()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var checkedFamilies = 0;
        var roundTrippedFamilies = new HashSet<string>(StringComparer.Ordinal);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Machines.Common.Contracts.ControllerBinding");
            if (root == "GWGUI.Emulation.Wenquxing") Assert.Null(contract);
            else
            {
                Assert.NotNull(contract);
                Assert.Equal(typeof(IReadOnlyDictionary<string, EmulationControllerOutputBinding>), contract.GetProperty("OutputBindings")!.PropertyType);
                checkedFamilies++;
            }
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var engine = Activator.CreateInstance(engineType)!;
            var machines = Assert.IsAssignableFrom<IReadOnlyList<EmulationMachineDefinition>>(engineType.GetProperty("Machines")!.GetValue(engine));
            foreach (var machine in machines)
            {
                try
                {
                    var configuration = engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                    var describe = engineType.GetMethod("DescribeInputSettings")!;
                    var settings = Assert.IsType<EmulationInputSettings>(describe.Invoke(engine, [configuration]));
                    if (root == "GWGUI.Emulation.Wenquxing") Assert.Empty(settings.ControllerPorts);
                    // Sony valide déjà les effets selon le visuel et le cœur ; ses cas de filtrage sont couverts séparément.
                    if (root == "GWGUI.Emulation.Sony" || settings.ControllerPorts.Count == 0) continue;
                    var outputs = new Dictionary<string, EmulationControllerOutputBinding>
                    {
                        [EmulationControllerFeedbackCommands.Strong] = new("physical-device", EmulationControllerMotor.LowFrequency),
                        [EmulationControllerFeedbackCommands.Weak] = new("other-device", EmulationControllerMotor.HighFrequency)
                    };
                    var ports = settings.ControllerPorts.Select(port => port with { OutputBindings = outputs }).ToArray();
                    var applied = engineType.GetMethod("ApplyInputSettings")!.Invoke(engine, [configuration, settings with { ControllerPorts = ports }])!;
                    var serialized = System.Text.Json.JsonSerializer.Serialize(applied, applied.GetType());
                    var restored = System.Text.Json.JsonSerializer.Deserialize(serialized, applied.GetType())!;
                    var roundTrip = Assert.IsType<EmulationInputSettings>(describe.Invoke(engine, [restored]));
                    roundTrippedFamilies.Add(root);
                    Assert.Equal(ports.Length, roundTrip.ControllerPorts.Count);
                    foreach (var port in roundTrip.ControllerPorts)
                        Assert.Equal(outputs.OrderBy(item => item.Key), port.OutputBindings.OrderBy(item => item.Key));
                    Assert.All(Assert.IsType<EmulationInputSettings>(describe.Invoke(engine, [configuration])).ControllerPorts, port => Assert.Empty(port.OutputBindings));
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(root + ": " + machine.Id, exception);
                }
            }
        }
        Assert.Equal(23, checkedFamilies);
        Assert.Equal(22, roundTrippedFamilies.Count);
    }

    [Fact]
    public void EveryCommonDescribesControllerOutputsThroughTheSameContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorControllerFeedbackAdapter", throwOnError: true)!;
            Assert.True(contract.IsInterface);
            Assert.False(contract.IsVisible);
            var operation = Assert.Single(contract.GetMethods());
            Assert.Equal("GetControllerOutputs", operation.Name);
            Assert.True(operation.IsAbstract);
            Assert.Equal(typeof(IReadOnlyList<EmulationControllerOutputDefinition>), operation.ReturnType);
            var parameters = operation.GetParameters();
            Assert.Equal(new[] { "configuration", "peripheral" }, parameters.Select(parameter => parameter.Name));
            Assert.Equal(root + ".Common.Machines.Common.Contracts.MachineConfiguration", parameters[0].ParameterType.FullName);
            Assert.Equal(root == "GWGUI.Emulation.Wenquxing" ? typeof(EmulationPeripheralCategory).FullName : root + ".Common.Machines.Common.Enums.PeripheralCategory", parameters[1].ParameterType.FullName);
            var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Contains(contract, adapter.GetInterfaces());
            foreach (var implementation in assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)))
            {
                var target = Assert.Single(implementation.GetInterfaceMap(contract).TargetMethods);
                Assert.False(target.IsAbstract);
                var concrete = implementation.GetMethod(operation.Name, BindingFlags.Public | BindingFlags.Instance, parameters.Select(parameter => parameter.ParameterType).ToArray());
                if (concrete is not null)
                {
                    Assert.Equal(concrete, target);
                    if (root == "GWGUI.Emulation.Example")
                    {
                        var configuration = RuntimeHelpers.GetUninitializedObject(parameters[0].ParameterType);
                        var backend = RuntimeHelpers.GetUninitializedObject(implementation);
                        foreach (var peripheral in Enum.GetValues(parameters[1].ParameterType))
                        {
                            var outputs = Assert.IsAssignableFrom<IReadOnlyList<EmulationControllerOutputDefinition>>(operation.Invoke(backend, [configuration, peripheral]));
                            if (peripheral.ToString() is "None" or "Keyboard") Assert.Empty(outputs);
                            else Assert.Equal(new[] { EmulationControllerFeedbackCommands.Strong, EmulationControllerFeedbackCommands.Weak }, outputs.Select(output => output.Id));
                        }
                    }
                }
                else
                {
                    Assert.Equal(adapter, target.DeclaringType);
                    var configuration = RuntimeHelpers.GetUninitializedObject(parameters[0].ParameterType);
                    var backend = RuntimeHelpers.GetUninitializedObject(implementation);
                    foreach (var peripheral in Enum.GetValues(parameters[1].ParameterType))
                    {
                        var outputs = Assert.IsAssignableFrom<IReadOnlyList<EmulationControllerOutputDefinition>>(operation.Invoke(backend, [configuration, peripheral]));
                        if (peripheral.ToString() is "None" or "Keyboard") Assert.Empty(outputs);
                        else Assert.Equal(new[] { EmulationControllerFeedbackCommands.Strong, EmulationControllerFeedbackCommands.Weak }, outputs.Select(output => output.Id));
                    }
                }
            }
            MethodInfo? consumer = root switch
            {
                "GWGUI.Emulation.Example" => assembly.GetType(root + ".Common.Services.Machine")!.GetProperty("ControllerOutputs")!.GetMethod,
                "GWGUI.Emulation.Sony" => assembly.GetType(root + ".Common.Machines.Common.Functions.InputSettingsFunctions")!.GetMethod("VisualOutputs", BindingFlags.Static | BindingFlags.NonPublic),
                _ => null
            };
            if (consumer is not null)
            {
                var owner = consumer.DeclaringType!;
                var methods = assembly.GetTypes().Where(type => type == owner || type.FullName!.StartsWith(owner.FullName + "+", StringComparison.Ordinal))
                    .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)).Where(method => method.GetMethodBody() is not null);
                Assert.Contains(methods.SelectMany(GetCompiledCalls), call => call.Method.DeclaringType == contract && call.Method.Name == operation.Name);
            }
        }
    }

    [Fact]
    public void EveryCommonUsesTheSameVideoAndNoContentCapabilities()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var noContentFactories = new HashSet<MethodInfo>();
        static bool Calls(MethodInfo caller, MethodInfo target)
        {
            var code = caller.GetMethodBody()!.GetILAsByteArray()!;
            return new byte[] { 0x28, 0x6f }.Any(opcode => code.AsSpan().IndexOf(new[] { opcode }.Concat(BitConverter.GetBytes(target.MetadataToken)).ToArray()) >= 0);
        }
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorCapabilities", throwOnError: true)!;
            Assert.True(contract.IsInterface);
            Assert.False(contract.IsVisible);
            Assert.Equal(new[] { "SupportsNoContent", "SupportsVideoSettings" }, contract.GetProperties().Select(property => property.Name).Order(StringComparer.Ordinal));
            Assert.Contains(assembly.GetType(root + ".Common.Interfaces.IEmulatorPointerCapabilities"), contract.GetInterfaces());
            foreach (var property in contract.GetProperties())
            {
                Assert.Equal(typeof(bool), property.PropertyType);
                Assert.True(property.GetMethod!.IsAbstract);
                Assert.Empty(property.GetMethod.GetParameters());
                Assert.Null(property.SetMethod);
            }
            var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Contains(contract, adapter.GetInterfaces());
            var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)).ToArray();
            if (root == "GWGUI.Emulation.Microsoft") Assert.Empty(implementations);
            else Assert.NotEmpty(implementations);
            foreach (var implementation in implementations)
            {
                var map = implementation.GetInterfaceMap(contract);
                foreach (var (operation, target) in map.InterfaceMethods.Zip(map.TargetMethods))
                {
                    var concrete = implementation.GetMethod(operation.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (concrete is not null) Assert.Equal(concrete, target);
                    else Assert.Equal(adapter, target.DeclaringType);
                    Assert.False(target.IsAbstract);
                }
                var backend = RuntimeHelpers.GetUninitializedObject(implementation);
                var video = Assert.IsType<bool>(contract.GetProperty("SupportsVideoSettings")!.GetValue(backend));
                Assert.Equal(implementation.Name != "CrocoDSMachineFactory", video);
                var noContent = Assert.IsType<bool>(contract.GetProperty("SupportsNoContent")!.GetValue(backend));
                var expected = root == "GWGUI.Emulation.Example" || implementation.Name is "OperaMachineFactory" or "TheodoreMachineFactory" or "WqxemuMachineFactory" or "Px68kMachineFactory" or "FuseMachineFactory";
                Assert.Equal(expected, noContent);
                var create = implementation.GetMethod("Create", BindingFlags.Public | BindingFlags.Instance)!;
                var hasPolicy = create.ReturnType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Any(constructor => constructor.GetParameters().Any(parameter => parameter.Name == "supportsNoContent"));
                if (hasPolicy)
                {
                    create = create.DeclaringType!.GetMethod(create.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, create.GetParameters().Select(parameter => parameter.ParameterType).ToArray())!;
                    Assert.True(Calls(create, contract.GetProperty("SupportsNoContent")!.GetMethod!), root);
                    noContentFactories.Add(create);
                }
            }
            var engine = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            Assert.True(Calls(engine.GetMethod("Describe")!, contract.GetProperty("SupportsVideoSettings")!.GetMethod!), root);
        }
        Assert.Equal(8, noContentFactories.Count);
    }

    [Fact]
    public void EveryCommonResolvesNativeContentFormatsThroughTheSameContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var nativeBridges = 0;
        var specializedResolvers = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorContentFormats", throwOnError: true)!;
            Assert.True(contract.IsInterface);
            Assert.False(contract.IsVisible);
            Assert.Equal(new[] { "GetMediaDependencies", "ResolveContentExtensions", "get_SupportedContentExtensions" }, contract.GetMethods().Select(method => method.Name).Order(StringComparer.Ordinal));
            var resolve = contract.GetMethod("ResolveContentExtensions")!;
            Assert.Equal(typeof(IReadOnlySet<string>), resolve.ReturnType);
            Assert.Equal(new[] { typeof(IReadOnlySet<string>), typeof(bool) }, resolve.GetParameters().Select(parameter => parameter.ParameterType));
            Assert.Equal(new[] { "declaredExtensions", "requiresFilePath" }, resolve.GetParameters().Select(parameter => parameter.Name));
            var dependencies = contract.GetMethod("GetMediaDependencies")!;
            Assert.Equal(typeof(IReadOnlyList<string>), dependencies.ReturnType);
            Assert.Equal(typeof(string), Assert.Single(dependencies.GetParameters()).ParameterType);
            Assert.Equal("path", Assert.Single(dependencies.GetParameters()).Name);
            var supported = contract.GetProperty("SupportedContentExtensions")!.GetMethod!;
            Assert.Equal(typeof(IReadOnlyList<string>), supported.ReturnType);
            Assert.All(contract.GetMethods(), method => Assert.True(method.IsAbstract));
            var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Contains(contract, adapter.GetInterfaces());
            var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)).ToArray();
            if (root == "GWGUI.Emulation.Microsoft") Assert.Empty(implementations);
            else Assert.NotEmpty(implementations);
            foreach (var implementation in implementations)
            {
                var map = implementation.GetInterfaceMap(contract);
                foreach (var (operation, target) in map.InterfaceMethods.Zip(map.TargetMethods))
                {
                    var concrete = implementation.GetMethod(operation.Name, BindingFlags.Public | BindingFlags.Instance, operation.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
                    if (concrete is not null) Assert.Equal(concrete, target);
                    else
                    {
                        Assert.Equal(adapter, target.DeclaringType);
                        var backend = RuntimeHelpers.GetUninitializedObject(implementation);
                        if (operation == resolve)
                        {
                            IReadOnlySet<string> declared = new HashSet<string>(["rom", "BIN"], StringComparer.Ordinal);
                            Assert.Same(declared, operation.Invoke(backend, [declared, false]));
                            Assert.Same(declared, operation.Invoke(backend, [declared, true]));
                            Assert.Equal(new[] { "rom", "BIN" }, declared);
                        }
                        else Assert.Empty(Assert.IsAssignableFrom<IEnumerable<string>>(operation.Invoke(backend, operation == dependencies ? ["unused"] : [])));
                    }
                }
                if (implementation.Name is "BeetleWonderSwanMachineFactory" or "BlueMSXMachineFactory")
                {
                    specializedResolvers++;
                    var constants = assembly.GetType(implementation.Namespace!.Replace(".Factories", ".Constants", StringComparison.Ordinal) + ".EmulatorConstants")!;
                    var extension = Assert.IsType<string>(constants.GetField("RawCartridgeExtension", BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue());
                    IReadOnlySet<string> declared = new HashSet<string>(["native", "zip"], StringComparer.Ordinal);
                    foreach (var requiresPath in new[] { false, true })
                    {
                        var extensions = Assert.IsAssignableFrom<IReadOnlySet<string>>(resolve.Invoke(RuntimeHelpers.GetUninitializedObject(implementation), [declared, requiresPath]));
                        Assert.True(declared.IsSubsetOf(extensions));
                        Assert.Equal(implementation.Name == "BlueMSXMachineFactory" || !requiresPath, extensions.Contains(extension));
                        Assert.Equal(new[] { "native", "zip" }, declared);
                    }
                }
            }
            if (root == "GWGUI.Emulation.Microsoft") continue;
            var bridge = Assert.Single(assembly.GetTypes(), type => type.IsClass && type.Name == "ExternalCore" && type.Namespace!.StartsWith(root + ".Emulators.", StringComparison.Ordinal));
            var code = bridge.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Cast<MethodBase>()
                .Concat(bridge.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)).Select(method => method.GetMethodBody()?.GetILAsByteArray()).Where(body => body is not null).Cast<byte[]>();
            Assert.Contains(code, body => new byte[] { 0x28, 0x6f }.Any(opcode => body.AsSpan().IndexOf(new[] { opcode }.Concat(BitConverter.GetBytes(resolve.MetadataToken)).ToArray()) >= 0));
            nativeBridges++;
        }
        Assert.Equal(23, nativeBridges);
        Assert.Equal(2, specializedResolvers);
    }

    [Fact]
    public void EveryCommonPreparesPersistentStorageBeforeCreatingOwnedCores()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var creationMethods = new HashSet<MethodInfo>();
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorPersistentStorage", throwOnError: true)!;
            Assert.True(contract.IsInterface);
            Assert.False(contract.IsVisible);
            var prepare = Assert.Single(contract.GetMethods());
            Assert.Equal("EnsurePersistentStorage", prepare.Name);
            Assert.Equal(typeof(void), prepare.ReturnType);
            Assert.True(prepare.IsAbstract);
            var parameters = prepare.GetParameters();
            Assert.Equal(new[] { "configuration", "directory" }, parameters.Select(parameter => parameter.Name));
            var configuration = assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration")!;
            Assert.Equal(new[] { configuration, typeof(string) }, parameters.Select(parameter => parameter.ParameterType));
            var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Contains(contract, adapter.GetInterfaces());
            var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)).ToArray();
            if (root == "GWGUI.Emulation.Microsoft") Assert.Empty(implementations);
            else Assert.NotEmpty(implementations);
            foreach (var implementation in implementations)
            {
                var target = Assert.Single(implementation.GetInterfaceMap(contract).TargetMethods);
                var concrete = implementation.GetMethod(prepare.Name, BindingFlags.Public | BindingFlags.Instance, [configuration, typeof(string)]);
                if (concrete is not null) Assert.Equal(concrete, target);
                else
                {
                    Assert.Equal(adapter, target.DeclaringType);
                    Assert.Empty(GetCompiledCalls(target)); // Vérification préalable : ce défaut n'appelle aucune API de fichiers.
                    Assert.Null(prepare.Invoke(RuntimeHelpers.GetUninitializedObject(implementation), [RuntimeHelpers.GetUninitializedObject(configuration), "unused"]));
                }
                var create = implementation.GetMethod("Create", BindingFlags.Public | BindingFlags.Instance)!;
                // Une même méthode héritée peut avoir plusieurs ReflectedType : contrôler sa déclaration une seule fois.
                create = create.DeclaringType!.GetMethod(create.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                    create.GetParameters().Select(parameter => parameter.ParameterType).ToArray())!;
                if (!creationMethods.Add(create)) continue;
                var calls = GetCompiledCalls(create);
                var preparation = Assert.Single(calls, call => call.Method == prepare);
                var resolver = Assert.Single(calls, call => call.Method.Name == "Invoke" && call.Method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual([configuration]) && call.Method is MethodInfo { ReturnType: var result } && result == typeof(string));
                Assert.True(resolver.Offset < preparation.Offset, create.DeclaringType!.FullName);
                var core = assembly.GetType(root + ".Common.Interfaces.IEmulatorCore")!;
                var allocation = calls.First(call => call.Method is ConstructorInfo constructor && core.IsAssignableFrom(constructor.DeclaringType)
                    || call.Method is MethodInfo method && core.IsAssignableFrom(method.ReturnType));
                Assert.True(preparation.Offset < allocation.Offset, create.DeclaringType!.FullName);
                var machine = Assert.Single(calls, call => call.Method is ConstructorInfo constructor && constructor.DeclaringType == create.ReturnType);
                var cleanup = Assert.Single(create.GetMethodBody()!.ExceptionHandlingClauses, clause => clause.Flags == ExceptionHandlingClauseOptions.Finally && clause.TryOffset <= machine.Offset && machine.Offset < clause.TryOffset + clause.TryLength);
                Assert.Contains(calls, call => call.Method.Name == "Dispose" && cleanup.HandlerOffset <= call.Offset && call.Offset < cleanup.HandlerOffset + cleanup.HandlerLength);
            }
        }
        Assert.Equal(24, creationMethods.Count);
    }

    [Fact]
    public void EveryCommonSelectsConfigurationThroughTheSameAdapterContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorConfigurationSelection", throwOnError: true)!;
            var operation = Assert.Single(contract.GetMethods());
            Assert.True(contract.IsInterface);
            Assert.False(contract.IsVisible);
            Assert.Equal("SelectConfiguration", operation.Name);
            Assert.True(operation.IsAbstract);
            var parameter = Assert.Single(operation.GetParameters());
            Assert.Equal("configuration", parameter.Name);
            Assert.Equal(assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration"), parameter.ParameterType);
            Assert.Equal(parameter.ParameterType, operation.ReturnType);
            var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Contains(contract, adapter.GetInterfaces());
            var identity = Assert.Single(adapter.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance), method => method.Name.EndsWith("IEmulatorConfigurationSelection.SelectConfiguration", StringComparison.Ordinal));
            Assert.False(identity.IsAbstract);
            var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)).ToArray();
            if (root == "GWGUI.Emulation.Microsoft") Assert.Empty(implementations);
            else Assert.NotEmpty(implementations);
            foreach (var implementation in implementations)
            {
                var concrete = implementation.GetMethod("SelectConfiguration", BindingFlags.Public | BindingFlags.Instance, [parameter.ParameterType]);
                Assert.Equal(concrete ?? identity, Assert.Single(implementation.GetInterfaceMap(contract).TargetMethods));
                if (concrete is null)
                {
                    // Le comportement par défaut doit conserver exactement l'objet reçu et ses données opaques.
                    var configuration = RuntimeHelpers.GetUninitializedObject(parameter.ParameterType);
                    var backend = RuntimeHelpers.GetUninitializedObject(implementation);
                    Assert.Same(configuration, operation.Invoke(backend, [configuration]));
                }
            }
            var engine = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var selection = engine.GetMethod("UseEmulatorAsync")!;
            var code = selection.GetMethodBody()!.GetILAsByteArray()!;
            Assert.True(new byte[] { 0x28, 0x6f }.Any(opcode => code.AsSpan().IndexOf(new[] { opcode }.Concat(BitConverter.GetBytes(operation.MetadataToken)).ToArray()) >= 0), root);
        }
    }

    [Fact]
    public void FirmwareSelectionKeepsEveryRoleAndRequiresAnUnambiguousChoice()
    {
        var first = new EmulationFirmwareCandidate("digest:system", "firmware.bin", "system", null, EmulationFirmwareCompatibility.Compatible, "system");
        var second = first with { Id = "digest:drive", DestinationFieldId = "drive" };
        var alternate = second with { Id = "alternate:drive" };
        IReadOnlyList<EmulationFirmwareCandidate> candidates = [first, second, alternate];
        var processing = EmulationSdk.FirmwareProcessing;
        Assert.Same(first, processing.SelectCandidate(candidates, first.Id, first.DestinationFieldId));
        Assert.Same(alternate, processing.SelectCandidate(candidates, alternate.Id, alternate.DestinationFieldId));
        Assert.Same(first, processing.SelectCandidate(candidates, null, "system"));
        Assert.Throws<InvalidOperationException>(() => processing.SelectCandidate(candidates, null, "drive"));
        Assert.Throws<InvalidOperationException>(() => processing.SelectCandidate(candidates, null, null));
        Assert.Throws<InvalidOperationException>(() => processing.SelectCandidate([], null, null));
        var incompatible = first with { Compatibility = EmulationFirmwareCompatibility.Incompatible, DestinationFieldId = null };
        Assert.Same(incompatible, processing.SelectCandidate([incompatible], "unrecognized", "system"));
        Assert.Equal(new[] { first, second, alternate }, candidates);
    }

    [Fact]
    public void EveryCommonUsesTheCompleteStorageContractAndItsActualBackendImplementations()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var reference = typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly.GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorStorageSettings", throwOnError: true)!;
        static string TypeName(Type type) => typeof(IEmulationConfiguration).IsAssignableFrom(type) ? "configuration" : type.FullName!;
        static string Signature(MethodInfo method) => method.Name + ":" + TypeName(method.ReturnType) + "(" + string.Join(",", method.GetParameters().Select(parameter => TypeName(parameter.ParameterType) + " " + parameter.Name)) + "):" + method.IsAbstract;
        var expected = reference.GetMethods().Select(Signature).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(11, expected.Length);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorStorageSettings", throwOnError: true)!;
            Assert.True(contract.IsInterface);
            Assert.False(contract.IsVisible);
            Assert.Equal(expected, contract.GetMethods().Select(Signature).Order(StringComparer.Ordinal));
            Assert.Contains(assembly.GetType(root + ".Common.Interfaces.IEmulatorMediaCapabilities"), contract.GetInterfaces());
            var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Contains(contract, adapter.GetInterfaces());
            var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)).ToArray();
            if (root == "GWGUI.Emulation.Microsoft") Assert.Empty(implementations);
            else Assert.NotEmpty(implementations);
            foreach (var implementation in implementations)
            {
                var mapping = implementation.GetInterfaceMap(contract);
                for (var index = 0; index < mapping.InterfaceMethods.Length; index++)
                {
                    var method = mapping.InterfaceMethods[index];
                    var concrete = implementation.GetMethod(method.Name, BindingFlags.Instance | BindingFlags.Public, method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
                    Assert.Equal(concrete ?? method, mapping.TargetMethods[index]);
                    Assert.False(mapping.TargetMethods[index].IsAbstract);
                }
            }
            var engine = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            foreach (var (operation, target) in new[] { ("DescribeStorageSettings", "DescribeStorage"), ("ApplyStorageSettings", "ApplyStorage") })
            {
                var body = engine.GetMethod(operation)!.GetMethodBody()!.GetILAsByteArray()!;
                var token = contract.GetMethod(target)!.MetadataToken;
                Assert.True(new byte[] { 0x28, 0x6f }.Any(opcode => body.AsSpan().IndexOf(new[] { opcode }.Concat(BitConverter.GetBytes(token)).ToArray()) >= 0), root + "." + operation);
            }
            var module = Assert.Single(assembly.GetTypes(), type => type.IsClass && !type.IsAbstract && typeof(IEmulationModule).IsAssignableFrom(type));
            Assert.True(typeof(IEmulationStorageSettingsManager).IsAssignableFrom(module), root);
            if (root == "GWGUI.Emulation.Wenquxing")
            {
                var configuration = RuntimeHelpers.GetUninitializedObject(contract.GetMethod("DescribeStorage")!.GetParameters()[0].ParameterType);
                var backend = Activator.CreateInstance(Assert.Single(implementations), nonPublic: true)!;
                var storage = Assert.IsType<EmulationStorageSettings>(contract.GetMethod("DescribeStorage")!.Invoke(backend, [configuration]));
                Assert.Empty(storage.AvailableDevices);
                Assert.Empty(storage.ConfiguredSlots);
                Assert.Empty(storage.MountedMedia);
                Assert.Same(configuration, contract.GetMethod("ApplyStorage")!.Invoke(backend, [configuration, storage]));
                Assert.Equal(0, contract.GetMethod("GetMemoryCardCount")!.Invoke(backend, [configuration]));
                Assert.Empty(Assert.IsAssignableFrom<IEnumerable<int>>(contract.GetMethod("GetMemoryCardIndexes")!.Invoke(backend, [configuration])));
                var invalid = storage with { ConfiguredSlots = [EmulationMediaSlot.Cartridge0] };
                Assert.IsType<ArgumentException>(Assert.Throws<TargetInvocationException>(() => contract.GetMethod("ApplyStorage")!.Invoke(backend, [configuration, invalid])).InnerException);
                var visibility = Assert.IsType<EmulationSettingsVisibility>(engine.GetProperty("DefaultVisibility")!.GetValue(Activator.CreateInstance(engine, nonPublic: true)));
                Assert.False(visibility.Tabs[EmulationMachineTab.Storage]);
            }
        }
    }

    [Fact]
    public void EveryCommonSelectsTheSameCatalogInstanceAndAllowsAnIncompleteSelection()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var selector = Assert.Single(engineType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic), method => method.Name == "SelectedAdapter");
            var parameter = Assert.Single(selector.GetParameters());
            Assert.Equal("configuration", parameter.Name);
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(parameter.ParameterType));
            Assert.Equal(assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true), selector.ReturnType);
            var engine = Activator.CreateInstance(engineType, nonPublic: true)!;
            var adapters = Assert.IsAssignableFrom<System.Collections.IDictionary>(engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine));
            var core = parameter.ParameterType.GetProperty("Core");
            var identifier = parameter.ParameterType.GetProperty("EmulatorId");
            foreach (System.Collections.DictionaryEntry entry in adapters)
            {
                var configuration = RuntimeHelpers.GetUninitializedObject(parameter.ParameterType);
                if (core?.PropertyType.IsEnum == true)
                {
                    var key = Assert.IsType<string>(entry.Value!.GetType().GetProperty("EmulatorKey")!.GetValue(entry.Value));
                    core.SetValue(configuration, Enum.Parse(core.PropertyType, key));
                }
                else
                {
                    Assert.NotNull(identifier);
                    identifier.SetValue(configuration, entry.Key);
                }
                Assert.Same(entry.Value, selector.Invoke(engine, [configuration]));
                Assert.Same(entry.Value, selector.Invoke(engine, [configuration]));
            }
            var incomplete = RuntimeHelpers.GetUninitializedObject(parameter.ParameterType);
            if (core?.PropertyType.IsEnum == true)
            {
                var hasNone = Enum.GetNames(core.PropertyType).Contains("None", StringComparer.Ordinal);
                var absent = hasNone ? Enum.Parse(core.PropertyType, "None") : Enum.ToObject(core.PropertyType, -1);
                if (!hasNone) Assert.False(Enum.IsDefined(core.PropertyType, absent));
                core.SetValue(incomplete, absent);
                Assert.Null(selector.Invoke(engine, [incomplete]));
            }
            else
            {
                Assert.NotNull(identifier);
                foreach (var key in new[] { null, string.Empty, "__missing_emulator__" })
                {
                    identifier.SetValue(incomplete, key);
                    Assert.Null(selector.Invoke(engine, [incomplete]));
                }
            }
        }
    }

    [Fact]
    public void EveryEngineAppliesSettingsThroughTheSameAdapterContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettingsApplication", throwOnError: true)!;
            Assert.True(contract.IsInterface);
            Assert.False(contract.IsVisible);
            var method = Assert.Single(contract.GetMethods());
            Assert.Equal("ApplySettings", method.Name);
            Assert.True(method.IsAbstract);
            var parameters = method.GetParameters();
            Assert.Equal(new[] { "configuration", "values" }, parameters.Select(parameter => parameter.Name));
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(method.ReturnType));
            Assert.Equal(method.ReturnType, parameters[0].ParameterType);
            Assert.Equal(typeof(IReadOnlyDictionary<string, string?>), parameters[1].ParameterType);
            var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Contains(contract, adapter.GetInterfaces());
            var defaultImplementation = Assert.Single(adapter.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic), candidate => candidate.Name.EndsWith(".ApplySettings", StringComparison.Ordinal));
            Assert.False(defaultImplementation.IsAbstract);
            var adapters = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)).ToArray();
            if (root == "GWGUI.Emulation.Microsoft") Assert.Empty(adapters);
            else Assert.NotEmpty(adapters);
            foreach (var implementation in adapters)
            {
                var concrete = implementation.GetMethod("ApplySettings", BindingFlags.Instance | BindingFlags.Public);
                Assert.Equal(concrete ?? defaultImplementation, Assert.Single(implementation.GetInterfaceMap(contract).TargetMethods));
                if (concrete is null)
                {
                    var configuration = RuntimeHelpers.GetUninitializedObject(method.ReturnType);
                    var values = new Dictionary<string, string?> { ["unchanged"] = "value" };
                    Assert.Same(configuration, method.Invoke(RuntimeHelpers.GetUninitializedObject(implementation), [configuration, values]));
                    Assert.Equal("value", values["unchanged"]);
                }
            }
            var engine = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var body = engine.GetMethod("ApplySettings")!.GetMethodBody()!.GetILAsByteArray()!;
            Assert.True(new byte[] { 0x28, 0x6f }.Any(opcode => body.AsSpan().IndexOf(new[] { opcode }.Concat(BitConverter.GetBytes(method.MetadataToken)).ToArray()) >= 0), root);
        }
    }

    [Fact]
    public void ConvertersUseTheSameModuleAndCategoryNamesAsExample()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var converters = 0;
        var peripheralConverters = 0;
        foreach (var assembly in modules)
        {
            var module = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModule).IsAssignableFrom(type));
            var moduleNamespace = module.Namespace!;
            var root = moduleNamespace[..moduleNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var conversion = assembly.GetType(root + ".Common.Machines.Common.Functions.EmulationMediaConversionFunctions", throwOnError: true)!;
            var methods = conversion.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods.Where(method => method.ReturnType.Name == "MediaConfiguration" && method.GetParameters().FirstOrDefault()?.ParameterType == typeof(EmulationMedia)))
            {
                Assert.Equal("ToModule", method.Name);
                converters++;
            }
            foreach (var method in methods.Where(method => method.ReturnType.Name == "MediaCategory" && method.GetParameters().FirstOrDefault()?.ParameterType == typeof(EmulationMediaType)))
                Assert.Equal("ToCategory", method.Name);
            var peripheralConversion = assembly.GetType(root + ".Common.Machines.Common.Functions.EmulationPeripheralConversionFunctions");
            if (peripheralConversion is null) continue;
            foreach (var method in peripheralConversion.GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(method => method.ReturnType.Name == "PeripheralCategory" && method.GetParameters().FirstOrDefault()?.ParameterType == typeof(EmulationPeripheralCategory)))
            {
                Assert.Equal("ToModule", method.Name);
                peripheralConverters++;
            }
        }
        Assert.Equal(17, converters);
        Assert.Equal(16, peripheralConverters);
    }

    [Fact]
    public void EveryModuleFacadeDelegatesItsPublicOperationsToItsCommonEngine()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        foreach (var assembly in modules)
        {
            var module = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModule).IsAssignableFrom(type));
            var field = Assert.Single(module.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly));
            Assert.Equal("_engine", field.Name);
            Assert.True(field.IsPrivate);
            Assert.True(field.IsInitOnly);
            var moduleNamespace = module.Namespace!;
            var root = moduleNamespace[..moduleNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var engine = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            Assert.Equal(engine, field.FieldType);
            foreach (var property in module.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                Assert.Equal(property.PropertyType, engine.GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!.PropertyType);
            foreach (var method in module.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(method => !method.IsSpecialName))
            {
                var common = engine.GetMethod(method.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
                Assert.NotNull(common);
                Assert.Equal(method.ReturnType, common.ReturnType);
            }
        }
    }

    [Fact]
    public void EveryCommonCreatesIndependentFrameStoresWithTheSameAccumulationAsExample()
    {
        var implementation = typeof(IEmulationInputFrames).Assembly.GetType("GWGUI.Emulation.Services.EmulationInputFrameStore", throwOnError: true)!;
        Assert.False(implementation.IsVisible);
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var common = assembly.GetType(root + ".Common.Functions.ControllerFunctions", throwOnError: true)!;
            var create = Assert.Single(common.GetMethods(BindingFlags.NonPublic | BindingFlags.Static), method => method.Name == "CreateInputFrames");
            Assert.Equal(typeof(IEmulationInputFrames), create.ReturnType);
            Assert.Empty(create.GetParameters());
            var first = Assert.IsAssignableFrom<IEmulationInputFrames>(create.Invoke(null, null));
            var second = Assert.IsAssignableFrom<IEmulationInputFrames>(create.Invoke(null, null));
            Assert.NotSame(first, second);
            var keys = new HashSet<EmulationKey> { EmulationKey.A };
            var input = EmulationInputSnapshot.Empty with { Keys = keys, Pointer = new(3, -4, 2, true, false, false) { HorizontalWheel = -2 } };
            first.Update(input);
            first.Update(input);
            keys.Clear();
            first.Poll();
            Assert.Contains(EmulationKey.A, first.Polled.Keys);
            Assert.Equal(6, first.Polled.Pointer.DeltaX);
            Assert.Equal(-8, first.Polled.Pointer.DeltaY);
            Assert.Equal(4, first.Polled.Pointer.Wheel);
            Assert.Equal(-4, first.Polled.Pointer.HorizontalWheel);
            first.Poll();
            Assert.Equal(0, first.Polled.Pointer.DeltaX);
            Assert.Equal(0, first.Polled.Pointer.DeltaY);
            Assert.Equal(0, first.Polled.Pointer.Wheel);
            Assert.Equal(0, first.Polled.Pointer.HorizontalWheel);
            Assert.True(first.Polled.Pointer.Left);
            second.Poll();
            Assert.Empty(second.Polled.Keys);
            Assert.Equal(0, second.Polled.Pointer.DeltaX);
            first.Update(null);
            first.Poll();
            Assert.Empty(first.Polled.Keys);
            Assert.False(first.Polled.Pointer.Left);
        }
    }

    [Fact]
    public void EveryCommonExposesExecutionMessagesAndBindingValidationThroughPublicContracts()
    {
        var implementation = typeof(IEmulationFrameTimer).Assembly.GetType("GWGUI.Emulation.Services.EmulationFrameTimer", throwOnError: true)!;
        Assert.False(implementation.IsVisible);
        foreach (var name in new[] { "GWGUI.Emulation.Functions.EmulationFrameOutputFunctions", "GWGUI.Emulation.Services.EmulationErrorService", "GWGUI.Emulation.Services.EmulationModuleLocalization", "GWGUI.Emulation.Functions.ConfigurationFileAccessFunctions", "GWGUI.Emulation.Functions.EmulationAdapterFunctions", "GWGUI.Emulation.Functions.EmulationKeyboardBindingFunctions", "GWGUI.Emulation.Functions.EmulationVideoFunctions", "GWGUI.Emulation.Functions.ExternalCoreLifecycleFunctions", "GWGUI.Emulation.Functions.JsonConfigurationRecoveryFunctions" })
            Assert.False(typeof(IEmulationMachineExecution).Assembly.GetType(name, throwOnError: true)!.IsVisible);
        Assert.True(typeof(EmulationMachineCommand).IsVisible);
        foreach (var name in new[] { "Execute", "Enqueue", "FailPending" })
            Assert.Null(typeof(EmulationMachineCommand).GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
        var create = typeof(IEmulationMachineExecution).GetMethod("CreateFrameTimer", [typeof(CancellationToken)])!;
        Assert.Equal(typeof(IEmulationFrameTimer), create.ReturnType);
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var common = assembly.GetType(root + ".Common.Functions.CoreHostFunctions", throwOnError: true)!;
            var execution = common.GetProperty("MachineExecution", BindingFlags.NonPublic | BindingFlags.Static)!;
            Assert.Equal(typeof(IEmulationMachineExecution), execution.PropertyType);
            Assert.Same(EmulationSdk.MachineExecution, execution.GetValue(null));
            var messages = common.GetProperty("Messages", BindingFlags.NonPublic | BindingFlags.Static)!;
            Assert.Equal(typeof(IEmulationMessages), messages.PropertyType);
            Assert.Same(EmulationSdk.Messages, messages.GetValue(null));
            var createLocalization = common.GetMethod("CreateLocalization", BindingFlags.NonPublic | BindingFlags.Static, [typeof(Assembly), typeof(string)])!;
            Assert.Equal(typeof(IEmulationModuleLocalization), createLocalization.ReturnType);
            var localization = Assert.IsAssignableFrom<IEmulationModuleLocalization>(createLocalization.Invoke(null, [assembly, root + ".Resources."]));
            Assert.NotSame(localization, createLocalization.Invoke(null, [assembly, root + ".Resources."]));
            foreach (var culture in new[] { System.Globalization.CultureInfo.InvariantCulture, System.Globalization.CultureInfo.GetCultureInfo("fr-FR"), System.Globalization.CultureInfo.GetCultureInfo("ja-JP") })
            {
                Assert.False(localization.TryGetString("__unknown-key-common-contract__", culture, out var value));
                Assert.Empty(value);
            }
            var controllers = assembly.GetType(root + ".Common.Functions.ControllerFunctions", throwOnError: true)!;
            var validateBinding = controllers.GetMethod("IsValidBinding", BindingFlags.NonPublic | BindingFlags.Static, [typeof(string)])!;
            Assert.Equal(typeof(bool), validateBinding.ReturnType);
            Assert.True((bool)validateBinding.Invoke(null, [null])!);
            Assert.True((bool)validateBinding.Invoke(null, ["Keyboard:A"])!);
            Assert.False((bool)validateBinding.Invoke(null, ["__invalid-input-binding__"])!);
        }
    }

    [Fact]
    public void EveryCommonCreatesIndependentInputAccumulatorsThroughThePublicContract()
    {
        var implementation = typeof(IEmulationInputAccumulator).Assembly.GetType("GWGUI.Emulation.Services.EmulationInputAccumulator", throwOnError: true)!;
        Assert.False(implementation.IsVisible);
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var common = assembly.GetType(root + ".Common.Functions.ControllerFunctions", throwOnError: true)!;
            var create = common.GetMethod("CreateInputAccumulator", BindingFlags.NonPublic | BindingFlags.Static)!;
            Assert.Equal(typeof(IEmulationInputAccumulator), create.ReturnType);
            var first = Assert.IsAssignableFrom<IEmulationInputAccumulator>(create.Invoke(null, null));
            var second = Assert.IsAssignableFrom<IEmulationInputAccumulator>(create.Invoke(null, null));
            Assert.NotSame(first, second);
            var input = EmulationInputSnapshot.Empty with { Pointer = new(4, int.MaxValue, 2, true, false, false) { HorizontalWheel = int.MinValue } };
            first.Update(input);
            first.Update(input);
            Assert.Equal(EmulationInputSnapshot.Empty, second.Consume());
            var accumulated = first.Consume();
            Assert.Equal(8, accumulated.Pointer.DeltaX);
            Assert.Equal(int.MaxValue, accumulated.Pointer.DeltaY);
            Assert.Equal(4, accumulated.Pointer.Wheel);
            Assert.Equal(int.MinValue, accumulated.Pointer.HorizontalWheel);
            var consumed = first.Consume();
            Assert.Equal(0, consumed.Pointer.DeltaX);
            Assert.Equal(0, consumed.Pointer.DeltaY);
            Assert.Equal(0, consumed.Pointer.Wheel);
            Assert.Equal(0, consumed.Pointer.HorizontalWheel);
            Assert.True(consumed.Pointer.Left);
            first.Update(input);
            first.Update(null);
            Assert.Equal(4, first.Consume().Pointer.DeltaX);
            Assert.False(first.Consume().Pointer.Left);
            first.Update(input);
            first.Reset();
            Assert.Equal(EmulationInputSnapshot.Empty, first.Consume());
        }
    }

    [Theory]
    [InlineData("Sharp")]
    [InlineData("Sinclair")]
    public void RuntimeCapabilitiesUseExamplesRoleAndKeepTheirDeclaredValues(string family)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var root = $"GWGUI.Emulation.{family}.Common.Interfaces.";
        var contract = assembly.GetType(root + "IEmulatorCapabilities", throwOnError: true)!;
        var reference = Assembly.Load("gwgui.emulation.example").GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorCapabilities", throwOnError: true)!;
        Assert.Null(assembly.GetType(root + "IEmulatorRuntimeCapabilities"));
        Assert.Contains(contract.GetInterfaces(), item => item.Name == "IEmulatorPointerCapabilities");
        var capability = contract.GetProperty("SupportsNoContent")!;
        Assert.Equal(reference.GetProperty("SupportsNoContent")!.PropertyType, capability.PropertyType);
        var expected = family == "Sharp" ? new Dictionary<string, bool> { ["Px68kMachineFactory"] = true, ["X1MachineFactory"] = false }
            : new Dictionary<string, bool> { ["FuseMachineFactory"] = true, ["EightyOneMachineFactory"] = false };
        var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
        Assert.Equal(expected.Keys.Order(), implementations.Select(type => type.Name).Order());
        foreach (var implementation in implementations)
        {
            var adapter = Activator.CreateInstance(implementation, nonPublic: true)!;
            Assert.Equal(expected[implementation.Name], (bool)capability.GetValue(adapter)!);
        }
        if (family == "Sinclair")
        {
            Assert.Null(assembly.GetType(root + "IEmulatorInputSettings"));
            var controllers = assembly.GetType(root + "IEmulatorControllerAdapter", throwOnError: true)!;
            var method = Assert.Single(controllers.GetMethods(), candidate => candidate.Name == "GetPeripherals" && candidate.GetParameters().Length == 1);
            Assert.Equal("GetPeripherals", method.Name);
            var configurationType = Assert.Single(method.GetParameters()).ParameterType;
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(configurationType));
            var engineType = assembly.GetType($"GWGUI.Emulation.{family}.Common.Services.Engine", throwOnError: true)!;
            var engine = Activator.CreateInstance(engineType)!;
            var machineDefinitions = (IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!;
            foreach (var implementation in implementations)
            {
                Assert.True(controllers.IsAssignableFrom(implementation));
                var adapter = Activator.CreateInstance(implementation, nonPublic: true)!;
                var peripheral = method.ReturnType.GetGenericArguments().Single();
                var choices = implementation.Name == "FuseMachineFactory" ? Enum.GetNames(peripheral) : new[] { "None", "CursorJoystick" };
                foreach (var machine in machineDefinitions)
                    Assert.Equal(choices, ((System.Collections.IEnumerable)method.Invoke(adapter, [engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])])!).Cast<object>().Select(item => item.ToString()));
            }
        }
    }

    [Fact]
    public void SettingsApplicationUsesTheSameContractInAppleCommodoreSonyAndExample()
    {
        foreach (var family in new[] { "Apple", "Commodore", "Sony", "Example" })
        {
            var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
            var contract = assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.IEmulatorSettingsApplication", throwOnError: true)!;
            var method = Assert.Single(contract.GetMethods());
            Assert.Equal("ApplySettings", method.Name);
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(method.ReturnType));
            var parameters = method.GetParameters();
            Assert.Equal(new[] { "configuration", "values" }, parameters.Select(parameter => parameter.Name));
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(parameters[0].ParameterType));
            Assert.Equal(typeof(IReadOnlyDictionary<string, string>), parameters[1].ParameterType);
            var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
            Assert.NotEmpty(implementations);
            foreach (var implementation in implementations)
                Assert.All(implementation.GetInterfaceMap(contract).TargetMethods, target => Assert.False(target.IsAbstract));
            Assert.DoesNotContain(assembly.GetTypes().SelectMany(type => type.GetMethods()), candidate => candidate.Name is "ApplyBackendSettings" or "ApplySettingsOptions");
            if (family == "Example")
            {
                Assert.Equal(3, implementations.Length);
                Assert.Contains(contract, assembly.GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorAdapter", throwOnError: true)!.GetInterfaces());
            }
        }
    }

    [Theory]
    [InlineData("Apple", "PrepareFirmware")]
    [InlineData("Commodore", "GetFirmwareSlots")]
    [InlineData("Commodore", "ScanFirmware")]
    [InlineData("Nec", "GetFirmwareSlots")]
    [InlineData("Nintendo", "GetFirmwareSlots")]
    [InlineData("Sega", "GetFirmwareSlots")]
    [InlineData("Sony", "GetFirmwareSlots")]
    [InlineData("Sony", "get_RequiresExternalFirmware")]
    [InlineData("Snk", "FirmwareRoles")]
    [InlineData("Snk", "IsFirmwareRequired")]
    public void FirmwareSettingsUseTheSameOperationsAsExample(string family, string operation)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var contract = assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.IEmulatorFirmwareSettings", throwOnError: true)!;
        var exampleAssembly = typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly;
        var example = exampleAssembly.GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorFirmwareSettings", throwOnError: true)!;
        var exampleAdapter = exampleAssembly.GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
        Assert.Contains(example, exampleAdapter.GetInterfaces());
        var method = contract.GetMethod(operation)!;
        var reference = example.GetMethod(operation)!;
        Assert.NotNull(method);
        Assert.NotNull(reference);
        // Chaque module possède ses modèles de configuration et de firmware : leurs identités restent opaques au SDK.
        static string Shape(Type type) => type.IsGenericType ? type.Name + "<" + string.Join(",", type.GetGenericArguments().Select(Shape)) + ">" : type.Name;
        Assert.Equal(Shape(reference.ReturnType), Shape(method.ReturnType));
        Assert.Equal(reference.GetParameters().Select(parameter => (parameter.Name, Shape(parameter.ParameterType))), method.GetParameters().Select(parameter => (parameter.Name, Shape(parameter.ParameterType))));
        var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
        Assert.NotEmpty(implementations);
        foreach (var implementation in implementations)
            Assert.All(implementation.GetInterfaceMap(contract).TargetMethods, target => Assert.False(target.IsAbstract));
        if (family is "Commodore" or "Nintendo" or "Sega")
        {
            var adapter = assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Contains(contract, adapter.GetInterfaces());
            Assert.DoesNotContain(adapter.GetMethods(), declaration => declaration.Name is "GetFirmwareSlots" or "ScanFirmware");
            foreach (var implementation in implementations)
            {
                var target = implementation.GetMethod(operation, method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
                Assert.Contains(target, implementation.GetInterfaceMap(contract).TargetMethods);
            }
        }
        if (family == "Nec") Assert.Null(contract.GetProperty("FirmwareSlots"));
    }

    [Theory]
    [InlineData("Apple", "IEmulatorConfigurationOptions", "IEmulatorConfiguration", "InitializeOptions")]
    [InlineData("Apple", "IEmulatorContentFormats", "IEmulatorMediaAdapter", "GetMediaDependencies")]
    [InlineData("Apple", "IEmulatorStorageSettings", "IEmulatorStorageConfiguration", "ApplyStorageConfiguration")]
    [InlineData("Snk", "IEmulatorInitialContentSettings", "IEmulatorMediaAdapter", "ResolveInitialContentPaths")]
    [InlineData("Sony", "IEmulatorStorageSettings", "IEmulatorMediaAdapter", "GetMemoryCardCount")]
    [InlineData("Sony", "IEmulatorContentFormats", "IEmulatorMediaAdapter", "get_SupportedContentExtensions")]
    [InlineData("Nintendo", "IEmulatorContentFormats", "IEmulatorMediaAdapter", "get_SupportedContentExtensions")]
    [InlineData("Sega", "IEmulatorContentFormats", "IEmulatorMediaAdapter", "get_SupportedContentExtensions")]
    public void CommonRolesUseTheSameContractAndOperationNamesAsExample(string family, string contractName, string previousName, string operation)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var contract = assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.{contractName}", throwOnError: true)!;
        var example = typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly
            .GetType("GWGUI.Emulation.Example.Common.Interfaces." + contractName, throwOnError: true)!;
        Assert.Null(assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.{previousName}"));
        var method = contract.GetMethod(operation)!;
        var reference = example.GetMethod(operation)!;
        Assert.NotNull(method);
        Assert.NotNull(reference);
        if (typeof(IEmulationConfiguration).IsAssignableFrom(reference.ReturnType))
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(method.ReturnType));
        else Assert.Equal(reference.ReturnType, method.ReturnType);
        Assert.Equal(reference.GetParameters().Select(parameter => parameter.Name), method.GetParameters().Select(parameter => parameter.Name));
        var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
        Assert.NotEmpty(implementations);
        foreach (var implementation in implementations)
            Assert.All(implementation.GetInterfaceMap(contract).TargetMethods, target => Assert.False(target.IsAbstract));
        if (family is "Nintendo" or "Sega")
        {
            var adapter = assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Contains(contract, adapter.GetInterfaces());
            Assert.Null(adapter.GetProperty("SupportedContentExtensions"));
            foreach (var implementation in implementations)
                Assert.Contains(implementation.GetProperty("SupportedContentExtensions")!.GetMethod, implementation.GetInterfaceMap(contract).TargetMethods);
        }
    }

    [Fact]
    public void SettingsOptionProvidersUseTheSameOperationAndParametersAsExample()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var providers = 0;
        foreach (var module in modules)
        {
            var contract = module.GetTypes().SingleOrDefault(type => type.IsInterface && type.Name == "IEmulatorSettingsAdapter");
            if (contract?.GetMethod("GetSettingsOptions") is not { } readOptions) continue;
            providers++;
            Assert.DoesNotContain(contract.GetMethods(), method => method.Name == "DescribeOptions");
            Assert.Single(contract.GetMethods(), method => method.Name == "GetSettingsOptions");
            var parameters = readOptions.GetParameters();
            Assert.Equal(2, parameters.Length);
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(parameters[0].ParameterType));
            Assert.Equal("configuration", parameters[0].Name);
            Assert.Equal(typeof(EmulatorManagementContext), parameters[1].ParameterType);
            Assert.Equal("context", parameters[1].Name);
            Assert.All(parameters, parameter => { Assert.True(parameter.IsOptional); Assert.Null(parameter.DefaultValue); });
            var implementations = module.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
            Assert.NotEmpty(implementations);
            foreach (var implementation in implementations)
            {
                Assert.DoesNotContain(implementation.GetMethods(), method => method.Name == "DescribeOptions");
                Assert.All(implementation.GetInterfaceMap(contract).TargetMethods, method => Assert.False(method.IsAbstract));
                var adapter = Activator.CreateInstance(implementation, nonPublic: true)!;
                Assert.IsAssignableFrom<System.Collections.IEnumerable>(readOptions.Invoke(adapter, [null, null]));
            }
        }
        Assert.Equal(12, providers);
    }

    [Theory]
    [InlineData("Example")]
    [InlineData("Snk")]
    [InlineData("Sony")]
    public void ControllerAdaptersUseTheSamePortAndPeripheralOperationNames(string family)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var contract = assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.IEmulatorControllerAdapter", throwOnError: true)!;
        var count = contract.GetMethod("GetControllerPortCount")!;
        Assert.NotNull(count);
        Assert.Equal(typeof(int), count.ReturnType);
        Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(Assert.Single(count.GetParameters()).ParameterType));
        Assert.DoesNotContain(contract.GetMethods(), method => method.Name is "ControllerPortCount" or "GetControllerTypes" or "GetUsbControllerTypes");
        if (family != "Snk")
        {
            Assert.Equal(2, contract.GetMethods().Count(method => method.Name == "GetPeripherals" && typeof(IEmulationConfiguration).IsAssignableFrom(method.GetParameters()[0].ParameterType)));
            var usb = contract.GetMethod("GetUsbPeripherals")!;
            Assert.NotNull(usb);
            Assert.Equal(typeof(int), usb.GetParameters()[1].ParameterType);
        }
        var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
        Assert.NotEmpty(implementations);
        foreach (var implementation in implementations)
        {
            Assert.DoesNotContain(implementation.GetMethods(), method => method.Name is "ControllerPortCount" or "GetControllerTypes" or "GetUsbControllerTypes");
            Assert.All(implementation.GetInterfaceMap(contract).TargetMethods, method => Assert.False(method.IsAbstract));
        }
    }

    [Fact]
    public void EveryModulePlacesItsCoreAndPeripheralContractsInCommon()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var peripheralCategories = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var controllerContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorControllerAdapter", throwOnError: true)!;
            foreach (var peripheralMethod in controllerContract.GetMethods().Where(method => method.Name is "GetPeripherals" or "DescribeInput"))
            {
                var first = peripheralMethod.GetParameters()[0];
                Assert.Equal("configuration", first.Name);
                Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(first.ParameterType));
            }
            Assert.Null(assembly.GetType(root + ".Common.Contracts.InputActionDefinition"));
            Assert.Null(assembly.GetType(root + ".Common.Machines.Common.Enums.ControllerType"));
            var constructionEngine = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            Assert.False(constructionEngine.IsVisible);
            Assert.True(factory.IsVisible);
            var commonServices = assembly.GetTypes().Where(type => type.IsClass && (type.Namespace == root + ".Common.Services" || type.Namespace?.StartsWith(root + ".Emulators.", StringComparison.Ordinal) == true && type.Namespace.EndsWith(".Services", StringComparison.Ordinal))).ToArray();
            Assert.NotEmpty(commonServices);
            Assert.All(commonServices, service => Assert.False(service.IsVisible, service.FullName));
            var construct = constructionEngine.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, [typeof(EmulationModuleContext)])!;
            Assert.NotNull(construct);
            Assert.Equal(typeof(IEmulationModule), construct.ReturnType);
            Assert.Equal("context", Assert.Single(construct.GetParameters()).Name);
            var factoryCode = factory.GetMethod("Create")!.GetMethodBody()!.GetILAsByteArray()!;
            byte[] commonCall = [(byte)System.Reflection.Emit.OpCodes.Call.Value, .. BitConverter.GetBytes(construct.MetadataToken)];
            Assert.True(factoryCode.AsSpan().IndexOf(commonCall) >= 0, factory.FullName);
            var moduleFactory = Assert.IsAssignableFrom<IEmulationModuleFactory>(Activator.CreateInstance(factory));
            Assert.Equal("context", Assert.Throws<ArgumentNullException>(() => moduleFactory.Create(null!)).ParamName);
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var runtimeOptions = adapterContract.GetMethod("GetRuntimeOptions")!;
            Assert.NotNull(runtimeOptions);
            Assert.Equal(typeof(IReadOnlyDictionary<string, string>), runtimeOptions.ReturnType);
            var runtimeConfiguration = Assert.Single(runtimeOptions.GetParameters());
            Assert.Equal("configuration", runtimeConfiguration.Name);
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(runtimeConfiguration.ParameterType));
            foreach (var implementation in assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapterContract.IsAssignableFrom(type)))
                Assert.Contains(implementation.GetInterfaceMap(adapterContract).TargetMethods, method => method.Name == "GetRuntimeOptions" && !method.IsAbstract);
            if (root is "GWGUI.Emulation.Amstrad" or "GWGUI.Emulation.Commodore" or "GWGUI.Emulation.Microsoft" or "GWGUI.Emulation.Nec" or "GWGUI.Emulation.Nintendo" or "GWGUI.Emulation.Sega")
            {
                var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
                var engine = Activator.CreateInstance(engineType)!;
                var engineOptions = engineType.GetMethod("RuntimeOptions")!;
                var adapters = ((System.Collections.IEnumerable)engineType.GetField("_adapters", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(engine)!).Cast<object>()
                    .Select(pair => pair.GetType().GetProperty("Value")!.GetValue(pair)!).ToArray();
                var constructor = Assert.Single(runtimeConfiguration.ParameterType.GetConstructors());
                var parameters = constructor.GetParameters();
                var optionsIndex = Array.FindIndex(parameters, parameter => parameter.Name == "Options");
                Assert.True(optionsIndex >= 0);
                foreach (var adapter in adapters.Cast<object?>().Append(null))
                {
                    var definition = adapter is null ? null : (EmulationEmulatorDefinition)adapter.GetType().GetProperty("Definition")!.GetValue(adapter)!;
                    var model = definition?.MachineIds.First() ?? "runtime-options";
                    object selection = parameters[1].ParameterType.IsEnum
                        ? adapter is null ? Enum.ToObject(parameters[1].ParameterType, int.MaxValue) : Enum.Parse(parameters[1].ParameterType, (string)adapter.GetType().GetProperty("EmulatorKey")!.GetValue(adapter)!)
                        : definition?.Id ?? "runtime-options";
                    foreach (var options in new IReadOnlyDictionary<string, string>?[] { null, new Dictionary<string, string>(StringComparer.Ordinal) { ["MixedCase"] = "1", ["mixedcase"] = "2" } })
                    {
                        var arguments = parameters.Select(parameter => parameter.HasDefaultValue ? parameter.DefaultValue : null).ToArray();
                        arguments[0] = model;
                        arguments[1] = selection;
                        arguments[optionsIndex] = options;
                        var configuration = constructor.Invoke(arguments);
                        var actual = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(engineOptions.Invoke(engine, [configuration]));
                        Assert.Equal(options?.Count ?? 0, actual.Count);
                        foreach (var pair in options ?? new Dictionary<string, string>()) Assert.Equal(pair.Value, actual[pair.Key]);
                        if (options is not null)
                        {
                            if (root is "GWGUI.Emulation.Commodore" or "GWGUI.Emulation.Microsoft")
                            {
                                Assert.NotSame(options, actual);
                                var copy = Assert.IsType<Dictionary<string, string>>(actual);
                                copy["MixedCase"] = "changed";
                                Assert.Equal("1", options["MixedCase"]);
                            }
                            else Assert.Same(options, actual);
                        }
                    }
                }
            }
            var peripheral = assembly.GetType(root + ".Common.Machines.Common.Enums.PeripheralCategory");
            if (peripheral is not null)
            {
                Assert.True(peripheral.IsEnum);
                Assert.True(peripheral.IsPublic);
                Assert.NotEmpty(Enum.GetNames(peripheral));
                peripheralCategories++;
            }
            var core = assembly.GetType(root + ".Common.Interfaces.IEmulatorCore", throwOnError: true)!;
            Assert.True(core.IsInterface);
            Assert.False(core.IsVisible);
            Assert.Null(assembly.GetType(root + ".Emulators.Common.Interfaces.IEmulatorCore"));
            Assert.Contains(core.GetInterfaces(), contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IEmulationCoreRuntime<,>));
            var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && core.IsAssignableFrom(type)).ToArray();
            Assert.True(implementations.Length > 0 || root == "GWGUI.Emulation.Microsoft", root);
            foreach (var implementation in implementations)
                Assert.All(implementation.GetInterfaceMap(core).TargetMethods, method => Assert.False(method.IsAbstract));
        }
        Assert.Equal(23, peripheralCategories);
    }

    [Theory]
    [InlineData("Acorn")]
    [InlineData("Amstrad")]
    [InlineData("Apple")]
    [InlineData("Atari")]
    [InlineData("Bandai")]
    [InlineData("Coleco")]
    [InlineData("Commodore")]
    [InlineData("Epoch")]
    [InlineData("Example")]
    [InlineData("Fairchild")]
    [InlineData("Mattel")]
    [InlineData("Microsoft")]
    [InlineData("Nec")]
    [InlineData("Nintendo")]
    [InlineData("Panasonic")]
    [InlineData("Philips")]
    [InlineData("Sega")]
    [InlineData("Sharp")]
    [InlineData("Sinclair")]
    [InlineData("Snk")]
    [InlineData("Sony")]
    [InlineData("Thomson")]
    [InlineData("Watara")]
    [InlineData("Wenquxing")]
    public void InputMappersReceiveTheSameCompleteConfigurationContract(string family)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var contract = assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.IEmulatorInputMapper", throwOnError: true)!;
        var method = Assert.Single(contract.GetMethods());
        Assert.Equal("MapInput", method.Name);
        Assert.Equal(typeof(EmulationInputSnapshot), method.ReturnType);
        var parameters = method.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(EmulationInputSnapshot), parameters[0].ParameterType);
        Assert.Equal("snapshot", parameters[0].Name);
        Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(parameters[1].ParameterType));
        Assert.Equal("configuration", parameters[1].Name);
        var adapter = assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
        Assert.Contains(contract, adapter.GetInterfaces());
        var adapters = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)).ToArray();
        if (family == "Microsoft") Assert.Empty(adapters);
        else Assert.NotEmpty(adapters);
        foreach (var implementation in adapters)
        {
            Assert.True(contract.IsAssignableFrom(implementation), implementation.FullName);
            var selected = Assert.Single(implementation.GetInterfaceMap(contract).TargetMethods);
            var concrete = implementation.GetMethod("MapInput", BindingFlags.Instance | BindingFlags.Public);
            if (concrete is not null) Assert.Equal(concrete, selected);
            else Assert.Contains(selected, adapter.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic));
        }
        var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
        // Le catalogue Microsoft existant ne déclare encore aucune fabrique concrète.
        if (family != "Microsoft") Assert.NotEmpty(implementations);
        if (family is "Coleco" or "Epoch" or "Example" or "Fairchild" or "Mattel" or "Philips" or "Sharp" or "Sinclair")
        {
            var controllers = assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.IEmulatorControllerAdapter", throwOnError: true)!;
            Assert.Contains(contract, controllers.GetInterfaces());
            Assert.Contains(controllers.GetMethods(), method => method.Name is "GetPeripherals" or "DescribeInput");
            Assert.All(implementations, implementation => Assert.True(controllers.IsAssignableFrom(implementation)));
            if (family is "Coleco" or "Epoch" or "Example" or "Fairchild" or "Mattel" or "Philips")
            {
                var describe = Assert.Single(controllers.GetMethods(), method => method.Name == "DescribeInput");
                var descriptionParameters = describe.GetParameters();
                Assert.Equal(new[] { "configuration", "port", "peripheral" }, descriptionParameters.Select(parameter => parameter.Name));
                Assert.Equal("MachineConfiguration", descriptionParameters[0].ParameterType.Name);
                Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(descriptionParameters[0].ParameterType));
                Assert.Equal(typeof(int), descriptionParameters[1].ParameterType);
                Assert.Equal("PeripheralCategory", descriptionParameters[2].ParameterType.Name);
                Assert.True(descriptionParameters[2].ParameterType.IsEnum);
                var action = Assert.Single(describe.ReturnType.GetGenericArguments());
                Assert.Equal(typeof(InputActionDefinition), action);
                Assert.Equal(typeof(InputBindingDefinition), action.GetProperty("Definition")!.PropertyType);
                Assert.Equal(typeof(EmulationKey?), action.GetProperty("Key")!.PropertyType);
                foreach (var implementation in implementations)
                    Assert.All(implementation.GetInterfaceMap(controllers).TargetMethods, method => Assert.False(method.IsAbstract));
                if (family == "Coleco")
                    Assert.Single(controllers.GetMethods(), method => method.Name == "GetPeripherals" && method.GetParameters() is [{ Name: "configuration" }]);
            }
        }
        if (family is "Sharp" or "Sinclair" or "Snk")
        {
            var hardwareRoot = $"GWGUI.Emulation.{family}.Common.Machines.Common";
            var action = assembly.GetType(hardwareRoot + ".Contracts.ControllerActionDefinition", throwOnError: true)!;
            var command = action.GetProperty("Command")!.PropertyType;
            Assert.True(command.IsEnum);
            Assert.Equal(hardwareRoot + ".Enums.ControllerCommand", command.FullName);
            Assert.Equal(typeof(InputBindingDefinition), action.GetProperty("Definition", BindingFlags.Instance | BindingFlags.NonPublic)!.PropertyType);
            var catalog = assembly.GetType(hardwareRoot + ".Dictionaries.ControllerCatalog", throwOnError: true)!;
            var actions = catalog.GetMethod("Actions", BindingFlags.Static | BindingFlags.NonPublic)!;
            Assert.Equal(action, Assert.Single(actions.ReturnType.GetGenericArguments()));
            Assert.Null(assembly.GetType($"GWGUI.Emulation.{family}.Common.Contracts.InputActionDefinition"));
            if (family == "Sharp") Assert.Equal(typeof(bool), action.GetProperty("Analog")!.PropertyType);
        }
        if (family is "Coleco" or "Mattel" or "Philips" or "Sharp" or "Sinclair")
        {
            var settings = assembly.GetType($"GWGUI.Emulation.{family}.Common.Machines.Common.Functions.InputSettingsFunctions", throwOnError: true)!;
            var peripherals = settings.GetMethod("Peripherals", BindingFlags.Static | BindingFlags.NonPublic)!;
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(Assert.Single(peripherals.GetParameters()).ParameterType));
            Assert.Contains(GetCompiledCalls(peripherals), call => call.Method.Name == "GetPeripherals" && typeof(IEmulationConfiguration).IsAssignableFrom(call.Method.GetParameters()[0].ParameterType));
        }
        if (family is "Epoch" or "Fairchild" or "Mattel" or "Philips")
        {
            var snapshots = assembly.GetType($"GWGUI.Emulation.{family}.Common.Machines.Common.Dictionaries.InputSnapshotDictionary", throwOnError: true)!;
            var actions = snapshots.GetMethod("Actions", BindingFlags.Static | BindingFlags.NonPublic)!;
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(actions.GetParameters()[0].ParameterType));
            Assert.Contains(GetCompiledCalls(actions), call => call.Method.Name == "DescribeInput" && typeof(IEmulationConfiguration).IsAssignableFrom(call.Method.GetParameters()[0].ParameterType));
            Assert.DoesNotContain(GetCompiledCalls(actions), call => call.Method is ConstructorInfo constructor && typeof(IEmulationConfiguration).IsAssignableFrom(constructor.DeclaringType));
        }
        foreach (var implementation in implementations)
        {
            Assert.DoesNotContain(implementation.GetMethods(), method => method.Name == "MapInput" && method.GetParameters().Length != 2);
            Assert.All(implementation.GetInterfaceMap(contract).TargetMethods, method => Assert.False(method.IsAbstract));
        }
    }

    [Theory]
    [InlineData("Bandai")]
    [InlineData("Coleco")]
    [InlineData("Example")]
    public void ContentFormatsUseTheSameResolutionContract(string family)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var contract = assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.IEmulatorContentFormats", throwOnError: true)!;
        var method = Assert.Single(contract.GetMethods(), method => method.Name == "ResolveContentExtensions");
        Assert.Equal("ResolveContentExtensions", method.Name);
        Assert.Equal(typeof(IReadOnlySet<string>), method.ReturnType);
        Assert.Equal(new[] { typeof(IReadOnlySet<string>), typeof(bool) }, method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Contains(assembly.GetTypes(), type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type));
    }

    [Fact]
    public void ExampleBackendsDescribeConfiguredHardwareAndNativeOptions()
    {
        var assembly = typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly;
        var models = GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries.HardwareModelCatalog.All;
        var contract = assembly.GetType("GWGUI.Emulation.Example.Common.Interfaces.IEmulatorSettings", throwOnError: true)!;
        var describe = contract.GetMethod("DescribeSettings")!;
        var backends = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
        Assert.Equal(3, backends.Length);
        foreach (var backend in backends)
        {
            var adapter = Activator.CreateInstance(backend, nonPublic: true)!;
            foreach (var model in models)
            {
                var configuration = new GWGUI.Emulation.Example.Common.Machines.Common.Contracts.MachineConfiguration(model.Model);
                var blocks = Assert.IsAssignableFrom<IReadOnlyList<EmulationSettingsBlock>>(describe.Invoke(adapter, [configuration, null]));
                var memory = Assert.Single(blocks.SelectMany(block => block.Fields), field => field.Id == "example_main_memory");
                Assert.Equal(model.MainMemoryBytes.ToString(System.Globalization.CultureInfo.InvariantCulture) + " B", memory.Value);
                var options = Assert.IsAssignableFrom<IReadOnlyList<GWGUI.Emulation.Example.Common.Contracts.CoreOption>>(backend.GetMethod("GetSettingsOptions")!.Invoke(adapter, [configuration, null]));
                foreach (var option in options.Where(option => option.IsVisible))
                    Assert.Contains(blocks.SelectMany(block => block.Fields), field => field.Id == option.Key && field.Value == configuration.Options.GetValueOrDefault(option.Key, option.CurrentValue));
            }
        }
    }
    [Theory]
    [InlineData("IEmulatorConfigurationValidation", "ValidateConfiguration", typeof(void))]
    [InlineData("IEmulatorSettings", "DescribeSettings", typeof(IReadOnlyList<EmulationSettingsBlock>))]
    [InlineData("IEmulatorPointerCapabilities", "SupportsPointer", typeof(bool))]
    [InlineData("IEmulatorMediaCapabilities", "DescribeMediaDevices", typeof(IReadOnlyList<GWGUI.Emulation.Contracts.EmulationMediaDevice>))]
    [InlineData("IEmulatorVideoSettings", "GetOutputResolution", typeof(ValueTuple<int, int>?))]
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
            var methods = contract.GetMethods();
            Assert.Equal(contractName == "IEmulatorPointerCapabilities" ? 2 : 1, methods.Length);
            var method = Assert.Single(methods, candidate => candidate.Name == methodName);
            Assert.Equal(methodName, method.Name);
            Assert.True(method.IsAbstract);
            Assert.Equal(returnType, method.ReturnType);
            var parameters = method.GetParameters();
            Assert.Equal(contractName == "IEmulatorSettings" ? 2 : 1, parameters.Length);
            var parameter = parameters[0];
            if (contractName == "IEmulatorSettings")
            {
                Assert.Equal(typeof(EmulatorManagementContext), parameters[1].ParameterType);
                Assert.Equal("context", parameters[1].Name);
                Assert.True(parameters[1].HasDefaultValue);
                Assert.Null(parameters[1].DefaultValue);
            }
            Assert.Equal("configuration", parameter.Name);
            Assert.True(typeof(IEmulationConfiguration).IsAssignableFrom(parameter.ParameterType));
            if (contractName is "IEmulatorConfigurationValidation" or "IEmulatorMediaCapabilities" or "IEmulatorVideoSettings")
            {
                if (contractName == "IEmulatorConfigurationValidation")
                    Assert.Null(assembly.GetType(root + ".Common.Interfaces.IEmulatorConfigurationValidator"));
                var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
                Assert.Contains(contract, adapter.GetInterfaces());
                var defaultImplementation = Assert.Single(adapter.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic), candidate => candidate.Name.EndsWith("." + methodName, StringComparison.Ordinal));
                Assert.False(defaultImplementation.IsAbstract);
                var adapters = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)).ToArray();
                if (root == "GWGUI.Emulation.Microsoft") Assert.Empty(adapters);
                else Assert.NotEmpty(adapters);
                foreach (var implementation in adapters)
                {
                    Assert.True(contract.IsAssignableFrom(implementation), implementation.FullName);
                    var selected = Assert.Single(implementation.GetInterfaceMap(contract).TargetMethods);
                    var concrete = implementation.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
                    Assert.Equal(concrete ?? defaultImplementation, selected);
                    if (contractName == "IEmulatorVideoSettings" && concrete is null)
                        Assert.Null(method.Invoke(RuntimeHelpers.GetUninitializedObject(implementation), [null]));
                }
            }
            if (contractName == "IEmulatorSettings")
            {
                var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
                Assert.Contains(contract, adapter.GetInterfaces());
                var adapters = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)).ToArray();
                Assert.All(adapters, implementation => Assert.True(contract.IsAssignableFrom(implementation), implementation.FullName));
                if (root != "GWGUI.Emulation.Microsoft")
                {
                    Assert.NotEmpty(adapters);
                    Assert.Contains(assembly.GetTypes(), type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type));
                }
                else
                {
                    Assert.Empty(adapters);
                    Assert.Contains(adapter.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic), method => method.Name.EndsWith(".DescribeSettings", StringComparison.Ordinal) && !method.IsAbstract);
                }
            }
            foreach (var implementation in assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)))
            {
                Assert.All(implementation.GetInterfaceMap(contract).TargetMethods, target => Assert.False(target.IsAbstract));
                if (contractName == "IEmulatorPointerCapabilities")
                {
                    Assert.Null(implementation.GetProperty("SupportsPointerCapture", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                    Assert.Null(implementation.GetProperty("SupportsPointerSettings", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                }
                if (contractName == "IEmulatorSettings")
                {
                    Assert.Null(implementation.GetMethod("GetSettingsBlocks"));
                    Assert.True(implementation.GetMethods(BindingFlags.Public | BindingFlags.Instance).Count(candidate => candidate.Name == "DescribeSettings") <= 1, implementation.FullName);
                    var description = Assert.Single(implementation.GetInterfaceMap(contract).TargetMethods);
                    Assert.Equal(parameters.Select(value => value.ParameterType), description.GetParameters().Select(value => value.ParameterType));
                }
            }
            if (contractName == "IEmulatorSettings")
            {
                var extension = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettingsAdapter");
                if (extension is not null)
                    Assert.Contains(contract, extension.GetInterfaces());
            }
            if (contractName == "IEmulatorPointerCapabilities")
            {
                var capture = contract.GetMethod("SupportsPointerCapture")!;
                Assert.True(capture.IsAbstract);
                Assert.Equal(typeof(bool), capture.ReturnType);
                var captureParameter = Assert.Single(capture.GetParameters());
                Assert.Equal("configuration", captureParameter.Name);
                Assert.Equal(parameter.ParameterType, captureParameter.ParameterType);
                var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
                Assert.Contains(contract, adapterContract.GetInterfaces());
                var adapters = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapterContract.IsAssignableFrom(type)).ToArray();
                if (root == "GWGUI.Emulation.Microsoft")
                    Assert.Empty(adapters);
                else
                {
                    Assert.NotEmpty(adapters);
                    var engine = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
                    var runtime = engine.GetMethod("CreateRuntimeAsync")!;
                    var stateMachine = runtime.GetCustomAttribute<AsyncStateMachineAttribute>()!.StateMachineType;
                    var code = stateMachine.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)!.GetMethodBody()!.GetILAsByteArray()!;
                    byte[] call = [0x6f, .. BitConverter.GetBytes(capture.MetadataToken)];
                    Assert.True(code.AsSpan().IndexOf(call) >= 0, root);
                }
                foreach (var implementation in adapters)
                {
                    var mapping = implementation.GetInterfaceMap(contract);
                    var target = mapping.TargetMethods[Array.FindIndex(mapping.InterfaceMethods, candidate => candidate.Name == "SupportsPointerCapture")];
                    Assert.True(target.DeclaringType!.IsClass);
                    Assert.Equal(implementation.GetMethod("SupportsPointerCapture"), target);
                }

                if (root == "GWGUI.Emulation.Amstrad")
                {
                    var capabilities = assembly.GetType(root + ".Common.Interfaces.IEmulatorCapabilities")!;
                    Assert.Contains(contract, capabilities.GetInterfaces());
                    Assert.DoesNotContain(assembly.GetTypes().SelectMany(type => type.GetProperties()), property => property.Name == "SupportsMouse");
                    Assert.Equal(2, assembly.GetTypes().Count(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)));
                }
                if (root is "GWGUI.Emulation.Sharp" or "GWGUI.Emulation.Sinclair")
                {
                    var runtime = assembly.GetType(root + ".Common.Interfaces.IEmulatorCapabilities")!;
                    Assert.Contains(contract, runtime.GetInterfaces());
                    Assert.Null(runtime.GetProperty("SupportsPointerSettings"));
                }
                if (root == "GWGUI.Emulation.Example")
                {
                    var capabilities = assembly.GetType(root + ".Common.Interfaces.IEmulatorCapabilities")!;
                    Assert.Contains(contract, capabilities.GetInterfaces());
                    Assert.DoesNotContain(capabilities.GetMethods(), method => method.Name == "SupportsPointer");
                }
            }
            if (root == "GWGUI.Emulation.Apple" && contractName == "IEmulatorConfigurationValidation")
                Assert.DoesNotContain(contract, assembly.GetType(root + ".Common.Interfaces.IEmulatorConfigurationOptions")!.GetInterfaces());
            if (contractName == "IEmulatorConfigurationValidation" && root == "GWGUI.Emulation.Sony")
            {
                var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
                var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)).ToArray();
                var settings = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettingsAdapter", throwOnError: true)!;
                Assert.Null(settings.GetMethod("ValidateConfiguration"));
                Assert.Null(settings.GetMethod("GetRuntimeOptions"));
                var runtime = adapter.GetMethod("GetRuntimeOptions")!;
                Assert.Equal(typeof(IReadOnlyDictionary<string, string>), runtime.ReturnType);
                foreach (var implementation in implementations)
                    Assert.Equal(implementation.GetMethod("GetRuntimeOptions"), implementation.GetInterfaceMap(adapter).TargetMethods.Single(target => target.Name == "GetRuntimeOptions"));
            }
            if (root == "GWGUI.Emulation.Example")
                Assert.Contains(contract, assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter")!.GetInterfaces());
        }
    }

    [Fact]
    public void EveryCommonUsesTheSameRuntimePolicyContract()
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
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorRuntimePolicy", throwOnError: true)!;
            Assert.True(contract.IsInterface);
            Assert.False(contract.IsVisible);
            var methods = contract.GetMethods();
            Assert.Equal(2, methods.Length);
            var media = Assert.Single(methods, method => method.Name == "RequiresMediaRestart");
            var option = Assert.Single(methods, method => method.Name == "RequiresOptionRestart");
            var configuration = assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration", throwOnError: true)!;
            Assert.Equal(new[] { configuration, typeof(EmulationMediaSlot), typeof(string) }, media.GetParameters().Select(parameter => parameter.ParameterType));
            Assert.Equal(new[] { "configuration", "slot", "path" }, media.GetParameters().Select(parameter => parameter.Name));
            Assert.Equal(new[] { configuration, typeof(string) }, option.GetParameters().Select(parameter => parameter.ParameterType));
            Assert.Equal(new[] { "configuration", "key" }, option.GetParameters().Select(parameter => parameter.Name));
            Assert.All(methods, method => { Assert.True(method.IsAbstract); Assert.Equal(typeof(bool), method.ReturnType); });
            var machine = assembly.GetType(root + ".Common.Services.Machine", throwOnError: true)!;
            var constructor = Assert.Single(machine.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic));
            var policyParameter = Assert.Single(constructor.GetParameters(), parameter => parameter.Name == "runtimePolicy");
            Assert.Equal(contract, policyParameter.ParameterType);
            Assert.True(policyParameter.HasDefaultValue);
            Assert.Null(policyParameter.DefaultValue);
            var policyField = machine.GetField("_runtimePolicy", BindingFlags.Instance | BindingFlags.NonPublic)!;
            Assert.Equal(contract, policyField.FieldType);
            Assert.True(policyField.IsInitOnly);
            var owners = new[] { machine }.Concat(machine.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic));
            var projections = owners.SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => method.Name.Contains("AvailableOptions", StringComparison.Ordinal))
                .Select(method => method.GetMethodBody()?.GetILAsByteArray()).Where(body => body is not null).Cast<byte[]>().ToArray();
            Assert.Contains(projections, body => new byte[] { 0x28, 0x6f }.Any(opcode => body.AsSpan().IndexOf(new[] { opcode }.Concat(BitConverter.GetBytes(option.MetadataToken)).ToArray()) >= 0));
            if (root is "GWGUI.Emulation.Apple" or "GWGUI.Emulation.Bandai" or "GWGUI.Emulation.Coleco" or "GWGUI.Emulation.Epoch" or "GWGUI.Emulation.Example" or "GWGUI.Emulation.Fairchild" or "GWGUI.Emulation.Mattel" or "GWGUI.Emulation.Panasonic" or "GWGUI.Emulation.Philips" or "GWGUI.Emulation.Sharp" or "GWGUI.Emulation.Sinclair" or "GWGUI.Emulation.Snk" or "GWGUI.Emulation.Thomson" or "GWGUI.Emulation.Watara" or "GWGUI.Emulation.Wenquxing")
            {
                var decision = machine.GetMethod("NeedsMediaRestart", BindingFlags.Instance | BindingFlags.NonPublic)!;
                var body = decision.GetMethodBody()!.GetILAsByteArray()!;
                Assert.True(new byte[] { 0x28, 0x6f }.Any(opcode => body.AsSpan().IndexOf(new[] { opcode }.Concat(BitConverter.GetBytes(media.MetadataToken)).ToArray()) >= 0), root);
            }
            if (root is "GWGUI.Emulation.Bandai" or "GWGUI.Emulation.Coleco" or "GWGUI.Emulation.Epoch" or "GWGUI.Emulation.Example" or "GWGUI.Emulation.Fairchild")
            {
                var rules = assembly.GetType(root + ".Common.Machines.Common.Functions.MediaRuntimeFunctions", throwOnError: true)!;
                var decision = rules.GetMethod("RequiresMediaRestart", BindingFlags.Static | BindingFlags.NonPublic)!;
                foreach (var category in Enum.GetValues<EmulationMediaCategory>())
                    foreach (var index in new[] { -1, 0, 1, 4, 128 })
                    {
                        var slot = new EmulationMediaSlot(category, index);
                        var expected = root != "GWGUI.Emulation.Example" && (slot == EmulationMediaSlot.Cartridge0 || root == "GWGUI.Emulation.Bandai" && slot == EmulationMediaSlot.Cd0);
                        Assert.Equal(expected, decision.Invoke(null, [slot]));
                    }
            }
            var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Contains(contract, adapter.GetInterfaces());
            var defaults = adapter.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var method in methods)
                Assert.False(Assert.Single(defaults, candidate => candidate.Name.EndsWith("." + method.Name, StringComparison.Ordinal)).IsAbstract);
            var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)).ToArray();
            if (root == "GWGUI.Emulation.Microsoft") Assert.Empty(implementations);
            else Assert.NotEmpty(implementations);
            foreach (var implementation in implementations)
            {
                var mapping = implementation.GetInterfaceMap(contract);
                for (var index = 0; index < mapping.InterfaceMethods.Length; index++)
                {
                    var method = mapping.InterfaceMethods[index];
                    var concrete = implementation.GetMethod(method.Name, BindingFlags.Public | BindingFlags.Instance);
                    var fallback = Assert.Single(defaults, candidate => candidate.Name.EndsWith("." + method.Name, StringComparison.Ordinal));
                    Assert.Equal(concrete ?? fallback, mapping.TargetMethods[index]);
                }
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
            var machine = assembly.GetType(root + ".Common.Services.Machine", throwOnError: true)!;
            var owners = new[] { machine }.Concat(machine.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic));
            var bodies = owners.SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Select(method => method.GetMethodBody()?.GetILAsByteArray()).Where(body => body is not null).Cast<byte[]>().ToArray();
            bool Calls(int token) => bodies.Any(body => new byte[] { 0x28, 0x6f }.Any(opcode => body.AsSpan().IndexOf(new[] { opcode }.Concat(BitConverter.GetBytes(token)).ToArray()) >= 0));
            Assert.True(Calls(publish.MetadataToken), root + ".SetAudioBufferStatus");
            Assert.True(Calls(latency.GetMethod.MetadataToken), root + ".MinimumAudioLatencyMilliseconds");
            var controller = assembly.GetType(root + ".Common.Services.AudioOutputController", throwOnError: true)!;
            var applyLatency = controller.GetMethod("TrySetMinimumLatency", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
            Assert.True(Calls(applyLatency.MetadataToken), root + ".TrySetMinimumLatency");
            var startAudio = machine.GetMethod("StartAudio", BindingFlags.NonPublic | BindingFlags.Instance)!;
            Assert.NotNull(startAudio);
            var startup = startAudio.GetMethodBody()!.GetILAsByteArray()!;
            int StartupCall(int token) => new byte[] { 0x28, 0x6f }
                .Select(opcode => startup.AsSpan().IndexOf(new[] { opcode }.Concat(BitConverter.GetBytes(token)).ToArray()))
                .Where(index => index >= 0).DefaultIfEmpty(-1).Min();
            var readMinimum = StartupCall(latency.GetMethod.MetadataToken);
            var applyMinimum = StartupCall(applyLatency.MetadataToken);
            var startOutput = StartupCall(controller.GetMethod("Start", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, [typeof(int)])!.MetadataToken);
            Assert.True(readMinimum >= 0, root + ".StartAudio.MinimumAudioLatencyMilliseconds");
            Assert.True(applyMinimum > readMinimum, root + ".StartAudio.TrySetMinimumLatency");
            Assert.True(startOutput > applyMinimum, root + ".StartAudio.Start");
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
    public void EveryCommonUsesTheSameFirmwareFileInspectionContract()
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
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareAdapter", throwOnError: true)!;
            Assert.True(contract.IsInterface);
            Assert.False(contract.IsVisible);
            var scan = Assert.Single(contract.GetMethods());
            Assert.Equal("ScanFirmwareFileAsync", scan.Name);
            Assert.True(scan.IsAbstract);
            Assert.Equal(typeof(ValueTask<IReadOnlyList<EmulationFirmwareCandidate>>), scan.ReturnType);
            var parameters = scan.GetParameters();
            Assert.Equal(3, parameters.Length);
            Assert.Equal(assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration"), parameters[0].ParameterType);
            Assert.Equal(typeof(string), parameters[1].ParameterType);
            Assert.Equal(typeof(CancellationToken), parameters[2].ParameterType);
            var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Contains(contract, adapter.GetInterfaces());
            var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
            foreach (var implementation in implementations)
            {
                var target = Assert.Single(implementation.GetInterfaceMap(contract).TargetMethods);
                Assert.False(target.IsAbstract);
                Assert.True(target.IsPublic);
                Assert.False(target.DeclaringType!.IsInterface);
                Assert.Null(implementation.GetMethod("InspectFirmwareAsync"));
                Assert.Null(implementation.GetMethod("ScanFirmwareAsync"));
            }
            if (root != "GWGUI.Emulation.Microsoft")
                Assert.NotEmpty(implementations);

            // La façade publique transmet chaque opération au même Engine Common, y compris sans backend installé.
            var facade = Assert.Single(assembly.GetTypes(), type => type.IsClass && !type.IsAbstract && typeof(IEmulationModule).IsAssignableFrom(type));
            Assert.True(typeof(IEmulationFirmwareManager).IsAssignableFrom(facade), root);
            var engine = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            static bool Calls(MethodInfo caller, MethodInfo target)
            {
                var code = caller.GetMethodBody()!.GetILAsByteArray()!;
                return new byte[] { 0x28, 0x6f }.Any(opcode => code.AsSpan().IndexOf(new[] { opcode }.Concat(BitConverter.GetBytes(target.MetadataToken)).ToArray()) >= 0);
            }
            var publicMap = facade.GetInterfaceMap(typeof(IEmulationFirmwareManager));
            for (var index = 0; index < publicMap.InterfaceMethods.Length; index++)
            {
                var operation = publicMap.InterfaceMethods[index];
                var target = engine.GetMethod(operation.Name, operation.GetParameters().Select(parameter => parameter.ParameterType).ToArray())!;
                Assert.NotNull(target);
                Assert.Equal(operation.ReturnType, target.ReturnType);
                Assert.True(Calls(publicMap.TargetMethods[index], target), root + "." + operation.Name);
            }

            // Les callbacks et leurs machines d'état restent dans Common. Les deux parcours de dossier existants
            // passent par le contrat de catalogue du backend, qui conserve ses filtres avant l'inspection par fichier.
            var inspection = root is "GWGUI.Emulation.Commodore" or "GWGUI.Emulation.Example"
                ? assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareSettings")!.GetMethod("ScanFirmware")!
                : scan;
            var commonMethods = assembly.GetTypes().Where(type => type.FullName!.StartsWith(root + ".Common.", StringComparison.Ordinal))
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => method.GetMethodBody() is not null);
            Assert.Contains(commonMethods, method => Calls(method, inspection));
        }
    }

    [Theory]
    [InlineData("Example")]
    [InlineData("Panasonic")]
    [InlineData("Wenquxing")]
    public void FirmwareCapabilitiesUseTheSameDescriptionAndCompatibilityContract(string family)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var root = "GWGUI.Emulation." + family;
        var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareCapabilities", throwOnError: true)!;
        var configuration = assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration", throwOnError: true)!;
        var category = assembly.GetType(root + ".Common.Machines.Common.Enums.FirmwareCategory", throwOnError: true)!;
        var requirement = assembly.GetType(root + ".Common.Machines.Common.Contracts.FirmwareRequirement", throwOnError: true)!;
        Assert.True(contract.IsInterface);
        Assert.False(contract.IsVisible);
        Assert.Equal(2, contract.GetMethods().Length);
        var describe = contract.GetMethod("DescribeFirmware")!;
        Assert.True(describe.IsAbstract);
        Assert.Equal(typeof(IReadOnlyList<>).MakeGenericType(requirement), describe.ReturnType);
        Assert.Equal(configuration, Assert.Single(describe.GetParameters()).ParameterType);
        var compatible = contract.GetMethod("IsFirmwareCompatible")!;
        Assert.True(compatible.IsAbstract);
        Assert.Equal(typeof(bool), compatible.ReturnType);
        Assert.Equal(new[] { configuration, category, typeof(string) }, compatible.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(category, requirement.GetProperty("Role")!.PropertyType);
        Assert.Equal(typeof(string), requirement.GetProperty("FieldId")!.PropertyType);
        Assert.Equal(typeof(string), requirement.GetProperty("DisplayResourceKey")!.PropertyType);
        Assert.Equal(typeof(IReadOnlyList<string>), requirement.GetProperty("FileNames")!.PropertyType);
        Assert.Equal(typeof(bool), requirement.GetProperty("IsRequired")!.PropertyType);
        var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
        Assert.NotEmpty(implementations);
        foreach (var implementation in implementations)
            Assert.All(implementation.GetInterfaceMap(contract).TargetMethods, method => Assert.False(method.IsAbstract));
        if (family == "Example")
            Assert.Contains(contract, assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter")!.GetInterfaces());
    }

    [Theory]
    [InlineData("Panasonic", new[] { "SystemRom", "FontRom" })]
    [InlineData("Wenquxing", new[] { "SystemRom", "NorFlash", "NandFlash", "NandFirstPlane" })]
    public void RenamedFirmwareCategoriesPreserveTheirValuesAndStoredImageKeys(string family, string[] names)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var root = "GWGUI.Emulation." + family;
        var category = assembly.GetType(root + ".Common.Machines.Common.Enums.FirmwareCategory", throwOnError: true)!;
        Assert.Null(assembly.GetType(root + ".Common.Machines.Common.Enums.FirmwareRole"));
        Assert.Equal(names, Enum.GetNames(category));
        Assert.Equal(Enumerable.Range(0, names.Length), Enum.GetValues(category).Cast<object>().Select(Convert.ToInt32));
        var configuration = assembly.GetType(root + ".Common.Machines.Common.Contracts.FirmwareConfiguration", throwOnError: true)!;
        var images = names.ToDictionary(name => name, name => name + ".rom", StringComparer.Ordinal);
        var json = System.Text.Json.JsonSerializer.Serialize(new { Images = images });
        var restored = System.Text.Json.JsonSerializer.Deserialize(json, configuration)!;
        using var stored = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(restored, configuration));
        var actual = stored.RootElement.GetProperty("Images").EnumerateObject().ToDictionary(property => property.Name, property => property.Value.GetString());
        Assert.Equal(images.Count, actual.Count);
        foreach (var image in images)
            Assert.Equal(image.Value, actual[image.Key]);
    }

    [Theory]
    [InlineData("Example")]
    [InlineData("Nec")]
    public void FirmwareIdentificationUsesObservedCandidatesWithoutOwningOtherCapabilities(string family)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var root = "GWGUI.Emulation." + family;
        var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareInspection", throwOnError: true)!;
        var observed = assembly.GetType(root + ".Common.Machines.Common.Contracts.ScannedFirmware", throwOnError: true)!;
        Assert.True(contract.IsInterface);
        Assert.False(contract.IsVisible);
        Assert.Empty(contract.GetInterfaces());
        var identify = Assert.Single(contract.GetMethods());
        Assert.Equal("IdentifyFirmware", identify.Name);
        Assert.True(identify.IsAbstract);
        Assert.Equal(observed, identify.ReturnType);
        Assert.Equal(observed, Assert.Single(identify.GetParameters()).ParameterType);
        var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
        Assert.NotEmpty(implementations);
        foreach (var implementation in implementations)
            Assert.False(Assert.Single(implementation.GetInterfaceMap(contract).TargetMethods).IsAbstract);
        if (family == "Nec")
        {
            Assert.Null(assembly.GetType(root + ".Common.Machines.Common.Contracts.Firmware"));
            Assert.Equal(new[] { "Path", "Size", "Md5", "Sha256", "LastWriteTimeUtc", "Type", "IsKnown", "IsOfficial", "Name", "Version", "CompatibleModels", "IsVerified" },
                observed.GetProperties().Select(property => property.Name));
        }
    }

    [Theory]
    [InlineData("pac-n1.bin", true)]
    [InlineData("pce-lp1.bin", true)]
    [InlineData("PAC-N10.BIN", true)]
    [InlineData("unknown.bin", false)]
    public void NecFirmwareIdentificationPreservesObservedMetadata(string fileName, bool known)
    {
        var candidate = new GWGUI.Emulation.Nec.Common.Machines.Common.Contracts.ScannedFirmware(
            Path.Combine("firmware", fileName), 4096, "observed-md5", "observed-sha256", DateTime.UnixEpoch,
            GWGUI.Emulation.Nec.Common.Machines.Common.Enums.FirmwareType.Unknown, false, false, null, null, [], false);
        var owner = candidate.GetType().Assembly.GetType("GWGUI.Emulation.Nec.Emulators.Geargrafx.Functions.GeargrafxFirmwareFunctions", throwOnError: true)!;
        var identify = owner.GetMethod("Identify", BindingFlags.Static | BindingFlags.NonPublic)!;
        var result = identify.Invoke(null, [candidate]);
        if (!known)
        {
            Assert.Null(result);
            return;
        }
        var recognized = Assert.IsType<GWGUI.Emulation.Nec.Common.Machines.Common.Contracts.ScannedFirmware>(result);
        Assert.True(recognized.IsKnown);
        Assert.False(recognized.IsOfficial);
        Assert.False(recognized.IsVerified);
        Assert.Equal(candidate.Path, recognized.Path);
        Assert.Equal(candidate.Size, recognized.Size);
        Assert.Equal(candidate.Md5, recognized.Md5);
        Assert.Equal(candidate.Sha256, recognized.Sha256);
        Assert.Equal(candidate.LastWriteTimeUtc, recognized.LastWriteTimeUtc);
        Assert.Equal(GWGUI.Emulation.Nec.Common.Machines.Common.Enums.FirmwareType.SystemRom, recognized.Type);
        Assert.Equal(Path.GetFileNameWithoutExtension(fileName), recognized.Name);
        Assert.Single(recognized.CompatibleModels);
    }

    [Fact]
    public void SdkGrantsInternalAccessOnlyToTheExistingTestAssemblies()
    {
        var grants = typeof(EmulationSdk).Assembly.GetCustomAttributes<InternalsVisibleToAttribute>()
            .Select(attribute => new AssemblyName(attribute.AssemblyName).Name).ToArray();
        var tests = new[] { "GWGUI.Tests", "GWGUI.LocalEmulationTests", "GWGUI.LocalDiskImageTests", "GWGUI.TemporaryLibretroMediaReader" };
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
    public void SimpleEnvironmentCallbacksUseTheSharedOwnerAndKeepTheirOptionFactories()
    {
        foreach (var (family, emulator) in new[] { ("Panasonic", "Opera"), ("Thomson", "Theodore"), ("Watara", "Potator"), ("Wenquxing", "Wqxemu") })
        {
            var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
            var root = $"GWGUI.Emulation.{family}.Emulators.{emulator}.Interop.Services.";
            Assert.Null(assembly.GetType(root + "EnvironmentCallbackHost"));
            var core = assembly.GetType(root + "ExternalCore", throwOnError: true)!;
            Assert.Equal(typeof(IEmulationEnvironmentCallbackHost), core.GetField("_environment", BindingFlags.NonPublic | BindingFlags.Instance)!.FieldType);
            var feedback = assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.IEmulatorAudioBufferFeedback", throwOnError: true)!;
            Assert.True(feedback.IsAssignableFrom(core));
            var process = assembly.GetType($"GWGUI.Emulation.{family}.Common.Services.ProcessCore", throwOnError: true)!;
            Assert.True(feedback.IsAssignableFrom(process));
            var optionsFactory = assembly.GetType(root + "OptionCallbackHost") ?? assembly.GetType($"GWGUI.Emulation.{family}.Emulators.{emulator}.Functions.RuntimeOptionFunctions", throwOnError: true)!;
            Assert.True(optionsFactory.IsAbstract && optionsFactory.IsSealed);
            var create = optionsFactory.GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static) ?? optionsFactory.GetMethod("CreateCallbackHost", BindingFlags.NonPublic | BindingFlags.Static)!;
            Assert.Equal(typeof(IEmulationVariableOptionHost<EmulationEnabledOption>), create.ReturnType);
            using var options = Assert.IsAssignableFrom<IEmulationVariableOptionHost<EmulationEnabledOption>>(create.Invoke(null, [new Dictionary<string, string>()]));
            Assert.Empty(options.Options);
        }
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
        if (contract == typeof(IEmulationCoreEnvironmentProcessing))
        {
            var factory = Assert.Single(api, method => method.Name == nameof(IEmulationCoreEnvironmentProcessing.CreateControllerFeedbackAccumulator));
            Assert.Equal(typeof(IEmulationControllerFeedbackAccumulator), factory.ReturnType);
            var capacity = Assert.Single(factory.GetParameters());
            Assert.Equal(typeof(int?), capacity.ParameterType);
            Assert.Equal("maximumPortCount", capacity.Name);
            Assert.True(capacity.HasDefaultValue);
            Assert.Null(capacity.DefaultValue);
            api = api.Where(method => method != factory).ToArray();
        }
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
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        foreach (var module in modules)
        {
            var factory = Assert.Single(module.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var factoryNamespace = factory.Namespace!;
            var root = factoryNamespace[..factoryNamespace.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var common = module.GetType(root + ".Common.Functions.CoreReleaseFunctions", throwOnError: true)!;
            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            var gateway = Assert.Single(common.GetProperties(flags));
            Assert.Equal("Releases", gateway.Name);
            Assert.Equal(typeof(IEmulationCoreReleases), gateway.PropertyType);
            Assert.Same(EmulationSdk.CoreReleases, gateway.GetValue(null));
            Assert.Equal("get_Releases", Assert.Single(common.GetMethods(flags)).Name);
        }
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
