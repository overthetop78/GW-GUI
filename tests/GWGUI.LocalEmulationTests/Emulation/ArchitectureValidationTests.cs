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
                OperandType.InlineBrTarget or OperandType.InlineField or OperandType.InlineI or OperandType.InlineMethod or OperandType.InlineSig or OperandType.InlineString or OperandType.InlineTok or OperandType.InlineType or OperandType.ShortInlineR => 4,
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
        Assert.Equal(12, expected.Length);
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
                    var fallback = adapter.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).SingleOrDefault(candidate => candidate.Name.EndsWith("." + method.Name, StringComparison.Ordinal));
                    Assert.Equal(concrete ?? fallback ?? method, mapping.TargetMethods[index]);
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
    [InlineData("Commodore", "ScanFirmwareAsync")]
    [InlineData("Nec", "GetFirmwareSlots")]
    [InlineData("Nintendo", "GetFirmwareSlots")]
    [InlineData("Sega", "GetFirmwareSlots")]
    [InlineData("Sony", "GetFirmwareSlots")]
    [InlineData("Sony", "RequiresExternalFirmware")]
    [InlineData("Snk", "GetFirmwareSlots")]
    public void FirmwareSettingsUseTheSameOperationsAsExample(string family, string operation)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var contractName = "IEmulatorFirmwareAdapter";
        var contract = assembly.GetType($"GWGUI.Emulation.{family}.Common.Interfaces.{contractName}", throwOnError: true)!;
        var exampleAssembly = typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly;
        var example = exampleAssembly.GetType("GWGUI.Emulation.Example.Common.Interfaces." + contractName, throwOnError: true)!;
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

    public class DefaultAdapterProbe : DispatchProxy
    {
        public object Adapter { get; set; } = null!;
        public string MachineId { get; set; } = string.Empty;
        public bool Preferred { get; set; }
        public List<IEmulationConfiguration> Selections { get; } = [];

        protected override object? Invoke(MethodInfo? method, object?[]? arguments)
        {
            ArgumentNullException.ThrowIfNull(method);
            if (method.Name.EndsWith("IsDefaultFor", StringComparison.Ordinal)) return Preferred && arguments is [string machineId] && machineId == MachineId;
            if (method.Name.EndsWith("SelectConfiguration", StringComparison.Ordinal)) Selections.Add((IEmulationConfiguration)arguments![0]!);
            return method.Invoke(Adapter, arguments);
        }
    }

    [Fact]
    public async Task EveryCommonExposesNativeSettingsRulesThroughTheSettingsContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var deferredRules = 0;
        var clockRules = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettings", throwOnError: true)!;
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var configurationType = assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration", throwOnError: true)!;
            Assert.Contains(contract, adapterContract.GetInterfaces());
            var deferred = contract.GetMethod("IsDeferredOption")!;
            var clock = contract.GetMethod("IsCpuClock")!;
            Assert.Equal(new[] { configurationType, typeof(string) }, deferred.GetParameters().Select(parameter => parameter.ParameterType));
            Assert.Equal(typeof(string), Assert.Single(clock.GetParameters()).ParameterType);
            Assert.Equal(typeof(bool), deferred.ReturnType);
            Assert.Equal(typeof(bool), clock.ReturnType);
            Assert.True(deferred.IsAbstract && clock.IsAbstract);
            var extension = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettingsAdapter");
            if (extension is not null) Assert.DoesNotContain(extension.GetMethods(), method => method.Name is "IsDeferredOption" or "IsCpuClock");
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var engine = Activator.CreateInstance(engineType)!;
            var byId = engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine)!;
            var adapters = ((System.Collections.IEnumerable)byId.GetType().GetProperty("Values")!.GetValue(byId)!).Cast<object>().ToArray();
            var machines = (IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!;
            foreach (var adapter in adapters)
            {
                var map = adapter.GetType().GetInterfaceMap(contract);
                foreach (var operation in new[] { deferred, clock })
                {
                    var target = map.TargetMethods[Array.IndexOf(map.InterfaceMethods, operation)];
                    var concrete = adapter.GetType().GetMethod(operation.Name);
                    Assert.Equal(concrete ?? adapterContract.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(method => method.Name.EndsWith("." + operation.Name, StringComparison.Ordinal)), target);
                }
                var definition = (EmulationEmulatorDefinition)adapter.GetType().GetInterfaces().SelectMany(type => type.GetProperties()).First(property => property.Name == "Definition").GetValue(adapter)!;
                var machine = machines.FirstOrDefault(item => item.Id == "NeoGeoMvs" && definition.MachineIds.Contains(item.Id)) ?? machines.First(item => definition.MachineIds.Contains(item.Id));
                var original = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                var configuration = await (ValueTask<IEmulationConfiguration>)engineType.GetMethod("UseEmulatorAsync")!.Invoke(engine, [original, definition.Id, CancellationToken.None])!;
                Assert.False((bool)deferred.Invoke(adapter, [configuration, "unknown-setting"])!);
                Assert.False((bool)clock.Invoke(adapter, ["unknown-setting"])!);
                if (root == "GWGUI.Emulation.Example")
                {
                    Assert.Null(adapter.GetType().GetMethod(deferred.Name));
                    var field = assembly.GetType(root + ".Common.Machines.Common.Constants.SettingsConstants", throwOnError: true)!.GetField("CpuFrequency", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.GetRawConstantValue();
                    Assert.True((bool)clock.Invoke(adapter, [field])!);
                    clockRules++;
                }
                if (root == "GWGUI.Emulation.Sony")
                {
                    var field = assembly.GetType(root + ".Emulators.BeetlePsx.Constants.OptionConstants", throwOnError: true)!.GetField("CpuFreqScaleKey", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.GetRawConstantValue();
                    var expected = (bool)adapter.GetType().GetMethod(clock.Name)!.Invoke(adapter, [field])!;
                    Assert.Equal(expected, (bool)clock.Invoke(adapter, [field])!);
                    if (expected) clockRules++;
                }
                if (root == "GWGUI.Emulation.Snk" && adapter.GetType().GetMethod(deferred.Name)!.DeclaringType == adapter.GetType())
                {
                    var prefixType = assembly.GetType(adapter.GetType().Namespace![..adapter.GetType().Namespace!.LastIndexOf(".Factories", StringComparison.Ordinal)] + ".Constants.RuntimeOptionConstants", throwOnError: true)!;
                    var prefix = (string)prefixType.GetField("DipSwitchPrefix", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.GetRawConstantValue()!;
                    var prepared = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(configuration, null)!;
                    configurationType.GetProperty("InitialContent")!.SetValue(prepared, "probe.zip");
                    Assert.Contains(new[] { "-", "_" }, separator => (bool)deferred.Invoke(adapter, [prepared, prefix + "probe" + separator + "switch"])!);
                    deferredRules++;
                }
            }
            if (root == "GWGUI.Emulation.Snk")
            {
                foreach (var (name, method) in new[] { (root + ".Common.Machines.Common.Functions.CoreOptionValidationFunctions", "Validate"), (root + ".Emulators.Common.Interop.Services.ExternalHostCallbacks", "ValidateDeferredOptions") })
                {
                    var owner = assembly.GetType(name, throwOnError: true)!;
                    var entry = owner.GetMethod(method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
                    Assert.Contains(GetCompiledCalls(entry), call => call.Method.DeclaringType == contract && call.Method.Name == deferred.Name);
                }
            }
            if (root == "GWGUI.Emulation.Sony")
            {
                var owner = assembly.GetType(root + ".Common.Machines.Common.Functions.SettingsDescriptionFunctions", throwOnError: true)!;
                var methods = assembly.GetTypes().Where(type => type == owner || type.FullName!.StartsWith(owner.FullName + "+", StringComparison.Ordinal))
                    .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)).Where(method => method.GetMethodBody() is not null);
                Assert.Contains(methods.SelectMany(GetCompiledCalls), call => call.Method.DeclaringType == contract && call.Method.Name == clock.Name);
            }
        }
        Assert.Equal(4, deferredRules);
        Assert.True(clockRules >= 4);
    }

    [Fact]
    public void EveryCommonUsesTheSameDefaultRecommendationContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var simulatedChoices = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorMachineDefaults", throwOnError: true)!;
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var configurationType = assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration", throwOnError: true)!;
            Assert.Contains(contract, adapterContract.GetInterfaces());
            var recommend = Assert.Single(contract.GetMethods());
            Assert.Equal("IsDefaultFor", recommend.Name);
            Assert.True(recommend.IsAbstract);
            Assert.Equal(typeof(bool), recommend.ReturnType);
            Assert.Equal(typeof(string), Assert.Single(recommend.GetParameters()).ParameterType);
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var engine = Activator.CreateInstance(engineType)!;
            var select = engineType.GetMethod("SelectConfiguration", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var selectedAdapter = engineType.GetMethod("SelectedAdapter", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
            Assert.Contains(GetCompiledCalls(select), call => call.Method.DeclaringType == contract && call.Method.Name == recommend.Name);
            Assert.Contains(GetCompiledCalls(select), call => call.Method.DeclaringType!.Name == "IEmulatorConfigurationSelection" && call.Method.Name == "SelectConfiguration");
            var callers = engineType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(method => method.GetMethodBody() is not null && GetCompiledCalls(method).Any(call => call.Method == select)).ToArray();
            Assert.Equal("CreateConfiguration", Assert.Single(callers).Name);
            var dictionary = (System.Collections.IDictionary)engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine)!;
            var entries = dictionary.Keys.Cast<object>().Select(key => (Key: key, Adapter: dictionary[key]!)).ToArray();
            var definitionProperty = adapterContract.GetInterfaces().SelectMany(type => type.GetProperties()).First(property => property.Name == "Definition");
            foreach (var entry in entries)
            {
                var target = Assert.Single(entry.Adapter.GetType().GetInterfaceMap(contract).TargetMethods);
                Assert.False(target.IsAbstract);
                var concrete = entry.Adapter.GetType().GetMethod(recommend.Name);
                Assert.Equal(concrete ?? adapterContract.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Single(method => method.Name.EndsWith(".IsDefaultFor", StringComparison.Ordinal)), target);
                if (concrete is null) Assert.False((bool)recommend.Invoke(entry.Adapter, ["unknown-machine"])!);
                if (root == "GWGUI.Emulation.Example")
                {
                    var exampleDefinition = (EmulationEmulatorDefinition)definitionProperty.GetValue(entry.Adapter)!;
                    foreach (var machineId in exampleDefinition.MachineIds)
                        Assert.Equal(entry.Adapter.GetType().Name == "InternalMachineFactory", (bool)recommend.Invoke(entry.Adapter, [machineId])!);
                    if (entry.Adapter.GetType().Name == "InternalMachineFactory") Assert.NotNull(concrete);
                }
            }
            var machines = (IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!;
            var configurations = machines.Select(machine => (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!).ToArray();
            foreach (var configuration in configurations)
            {
                var current = selectedAdapter.Invoke(engine, [configuration]);
                var recommended = entries.Where(entry => ((EmulationEmulatorDefinition)definitionProperty.GetValue(entry.Adapter)!).MachineIds.Contains(configuration.MachineId))
                    .Where(entry => (bool)recommend.Invoke(entry.Adapter, [configuration.MachineId])!).ToArray();
                if (current is not null && (bool)recommend.Invoke(current, [configuration.MachineId])!) Assert.Same(configuration, select.Invoke(engine, [configuration]));
                if (recommended.Length == 0) Assert.Same(configuration, select.Invoke(engine, [configuration]));
            }
            if (entries.Length == 0) { Assert.Equal("GWGUI.Emulation.Microsoft", root); continue; }
            var chosen = entries.Last();
            var definition = (EmulationEmulatorDefinition)definitionProperty.GetValue(chosen.Adapter)!;
            var source = configurations.First(configuration => definition.MachineIds.Contains(configuration.MachineId));
            var withoutCore = (IEmulationConfiguration)typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(source, null)!;
            var needsSelection = !ReferenceEquals(selectedAdapter.Invoke(engine, [source]), chosen.Adapter);
            if (!needsSelection)
            {
                var emulatorId = configurationType.GetProperty("EmulatorId");
                if (emulatorId is not null) { emulatorId.SetValue(withoutCore, string.Empty); needsSelection = true; }
                else
                {
                    var core = configurationType.GetProperty("Core")!;
                    if (Enum.TryParse(core.PropertyType, "None", out var none) && Enum.IsDefined(core.PropertyType, none!))
                    {
                        core.SetValue(withoutCore, none);
                        needsSelection = true;
                    }
                    else
                    {
                        var other = entries.FirstOrDefault(entry => !ReferenceEquals(entry.Adapter, chosen.Adapter));
                        if (other.Adapter is not null)
                        {
                            var key = adapterContract.GetInterfaces().Append(adapterContract).SelectMany(type => type.GetProperties()).First(property => property.Name == "EmulatorKey").GetValue(other.Adapter)!;
                            core.SetValue(withoutCore, Enum.Parse(core.PropertyType, (string)key));
                            needsSelection = true;
                        }
                    }
                }
            }
            DefaultAdapterProbe? preferred = null;
            try
            {
                foreach (var entry in entries)
                {
                    var proxy = DispatchProxy.Create(adapterContract, typeof(DefaultAdapterProbe));
                    var state = (DefaultAdapterProbe)proxy;
                    state.Adapter = entry.Adapter;
                    state.MachineId = source.MachineId;
                    state.Preferred = ReferenceEquals(entry.Adapter, chosen.Adapter);
                    if (state.Preferred) preferred = state;
                    dictionary[entry.Key] = proxy;
                }
                var result = Assert.IsAssignableFrom<IEmulationConfiguration>(select.Invoke(engine, [withoutCore]));
                var selected = Assert.IsAssignableFrom<DefaultAdapterProbe>(selectedAdapter.Invoke(engine, [result]));
                Assert.Same(preferred, selected);
                Assert.Equal(needsSelection ? 1 : 0, preferred!.Selections.Count);
                if (needsSelection) Assert.NotSame(withoutCore, result); else Assert.Same(withoutCore, result);
                Assert.Equal(source.MachineId, result.MachineId);
                Assert.Equal(source.Id, result.Id);
                simulatedChoices++;
            }
            finally
            {
                foreach (var entry in entries) dictionary[entry.Key] = entry.Adapter;
            }
        }
        Assert.Equal(23, simulatedChoices);
    }

    [Fact]
    public async Task EveryCommonProvidesTheSameInitialContentContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var describedAdapters = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorInitialContentSettings", throwOnError: true)!;
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var configurationType = assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration", throwOnError: true)!;
            Assert.Contains(contract, adapterContract.GetInterfaces());
            Assert.Equal(new[] { "ResolveInitialContentPaths", "get_SupportsInitialContent" }, contract.GetMethods().Select(method => method.Name).Order(StringComparer.Ordinal));
            var capability = contract.GetProperty("SupportsInitialContent")!;
            Assert.Equal(typeof(bool), capability.PropertyType);
            var resolve = contract.GetMethod("ResolveInitialContentPaths")!;
            Assert.True(resolve.IsAbstract);
            Assert.Equal(configurationType, Assert.Single(resolve.GetParameters()).ParameterType);
            Assert.Equal(typeof(IReadOnlyList<string>), resolve.ReturnType);
            var defaultResolve = Assert.Single(adapterContract.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic), method => method.Name.EndsWith(".ResolveInitialContentPaths", StringComparison.Ordinal));
            Assert.Contains(GetCompiledCalls(defaultResolve), call => call.Method.Name == "ResolveConfiguredMedia");
            if (root is "GWGUI.Emulation.Sinclair" or "GWGUI.Emulation.Snk" or "GWGUI.Emulation.Thomson")
            {
                var entries = assembly.GetType(root + ".Common.Machines.Common.Functions.SavedStateFunctions", throwOnError: true)!.GetMethod("ContentEntries", BindingFlags.Static | BindingFlags.NonPublic)!;
                Assert.Contains(GetCompiledCalls(entries), call => call.Method.DeclaringType == contract && call.Method.Name == resolve.Name);
            }
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var engine = Activator.CreateInstance(engineType)!;
            var byId = engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine)!;
            var adapters = ((System.Collections.IEnumerable)byId.GetType().GetProperty("Values")!.GetValue(byId)!).Cast<object>().ToArray();
            var machines = (IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!;
            foreach (var adapter in adapters)
            {
                var map = adapter.GetType().GetInterfaceMap(contract);
                Assert.All(map.TargetMethods, method => Assert.False(method.IsAbstract));
                var target = map.TargetMethods[Array.IndexOf(map.InterfaceMethods, resolve)];
                Assert.Equal(adapter.GetType().GetMethod(resolve.Name) ?? defaultResolve, target);
                var definition = (EmulationEmulatorDefinition)adapter.GetType().GetInterfaces().SelectMany(type => type.GetProperties()).First(property => property.Name == "Definition").GetValue(adapter)!;
                var machine = machines.First(item => definition.MachineIds.Contains(item.Id));
                var original = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                var configuration = await (ValueTask<IEmulationConfiguration>)engineType.GetMethod("UseEmulatorAsync")!.Invoke(engine, [original, definition.Id, CancellationToken.None])!;
                var paths = Assert.IsAssignableFrom<IReadOnlyList<string>>(resolve.Invoke(adapter, [configuration]));
                if (target.DeclaringType == adapterContract)
                {
                    var mediaResolver = adapterContract.GetInterfaces().SelectMany(type => type.GetMethods()).First(method => method.Name == "ResolveConfiguredMedia");
                    var media = (IReadOnlyList<EmulationMedia>)mediaResolver.Invoke(adapter, [configuration])!;
                    Assert.Equal(media.Where(item => item.IsInserted).OrderBy(item => item.Slot).Select(item => item.Path), paths);
                }
                var supported = (bool)capability.GetValue(adapter)!;
                if (root == "GWGUI.Emulation.Example" || root == "GWGUI.Emulation.Snk" || adapter.GetType().Name is "FuseMachineFactory" or "TheodoreMachineFactory") Assert.True(supported);
                else Assert.False(supported);
                if (root == "GWGUI.Emulation.Example")
                {
                    Assert.Null(adapter.GetType().GetMethod(resolve.Name));
                    var example = new GWGUI.Emulation.Example.Common.Machines.Common.Contracts.MachineConfiguration(GWGUI.Emulation.Example.Common.Machines.Common.Dictionaries.HardwareModelCatalog.All[0].Model, media:
                    [
                        new("second.dsk", GWGUI.Emulation.Example.Common.Machines.Common.Enums.MediaCategory.Floppy, EmulationMediaSlot.Floppy1),
                        new("first.dsk", GWGUI.Emulation.Example.Common.Machines.Common.Enums.MediaCategory.Floppy, EmulationMediaSlot.Floppy0),
                        new("ejected.cas", GWGUI.Emulation.Example.Common.Machines.Common.Enums.MediaCategory.Cassette, EmulationMediaSlot.Cassette0, IsInserted: false)
                    ]);
                    Assert.Equal(new[] { "first.dsk", "second.dsk" }, Assert.IsAssignableFrom<IReadOnlyList<string>>(resolve.Invoke(adapter, [example])));
                }
                if (adapter.GetType().Name is "TheodoreMachineFactory" or "PotatorMachineFactory")
                    Assert.Contains(GetCompiledCalls(target), call => call.Method.Name == "InitialContentPaths" && call.Method.DeclaringType!.Name == "MediaRuntimeFunctions");
                describedAdapters++;
            }
            if (root == "GWGUI.Emulation.Microsoft") Assert.Empty(adapters);
            else Assert.NotEmpty(adapters);
        }
        Assert.True(describedAdapters >= 24);
    }

    [Fact]
    public async Task EveryCommonProvidesTheCompleteConfigurationOptionContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal))
            .Select(Assembly.Load).Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var initializedAdapters = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorConfigurationOptions", throwOnError: true)!;
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var configurationType = assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration", throwOnError: true)!;
            Assert.Contains(contract, adapterContract.GetInterfaces());
            Assert.Equal(new[] { "GetDefaultOptions", "InitializeOptions", "NormalizeOptions", "ResolveModel", "ValidateOptions" }, contract.GetMethods().Select(method => method.Name).Order(StringComparer.Ordinal));
            foreach (var operation in contract.GetMethods())
            {
                Assert.True(operation.IsAbstract);
                var parameters = operation.GetParameters();
                Assert.Equal(configurationType, parameters[0].ParameterType);
                Assert.Equal(operation.Name == "InitializeOptions" ? 2 : 1, parameters.Length);
                if (operation.Name == "InitializeOptions") Assert.Equal(typeof(IDictionary<string, string>), parameters[1].ParameterType);
                var expectedReturn = operation.Name switch
                {
                    "InitializeOptions" or "ValidateOptions" => typeof(void),
                    "ResolveModel" => configurationType.GetProperty("Model")!.PropertyType,
                    _ => typeof(IReadOnlyDictionary<string, string>)
                };
                Assert.Equal(expectedReturn, operation.ReturnType);
            }
            var validate = Assert.Single(adapterContract.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance), method => method.Name.EndsWith(".ValidateOptions", StringComparison.Ordinal));
            Assert.Contains(GetCompiledCalls(validate), call => call.Method.Name == "ValidateConfiguration" && call.Method.DeclaringType!.Name == "IEmulatorConfigurationValidation");
            var initialize = Assert.Single(adapterContract.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance), method => method.Name.EndsWith(".InitializeOptions", StringComparison.Ordinal));
            Assert.Contains(GetCompiledCalls(initialize), call => call.Method.Name == "GetDefaultOptions" && call.Method.DeclaringType == contract);
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var engine = Activator.CreateInstance(engineType)!;
            var prepare = engineType.GetMethod("InitializeOptions", BindingFlags.NonPublic | BindingFlags.Instance)!;
            Assert.Contains(GetCompiledCalls(engineType.GetMethod("CreateConfiguration")!), call => call.Method == prepare);
            Assert.Contains(GetCompiledCalls(prepare), call => call.Method.DeclaringType == contract && call.Method.Name == "InitializeOptions");
            var byId = engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine)!;
            var adapters = ((System.Collections.IEnumerable)byId.GetType().GetProperty("Values")!.GetValue(byId)!).Cast<object>().ToArray();
            var machines = (IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!;
            foreach (var adapter in adapters)
            {
                var mapping = adapter.GetType().GetInterfaceMap(contract);
                var preparation = mapping.TargetMethods[Array.FindIndex(mapping.InterfaceMethods, method => method.Name == "InitializeOptions")];
                if (root == "GWGUI.Emulation.Example" || adapter.GetType().Name == "MiniVmacMachineFactory")
                {
                    Assert.Null(adapter.GetType().GetMethod("InitializeOptions", BindingFlags.Public | BindingFlags.Instance));
                    Assert.Equal(adapterContract, preparation.DeclaringType);
                }
                if (adapter.GetType().Name == "AppleWinMachineFactory") Assert.Equal(adapter.GetType(), preparation.DeclaringType);
                foreach (var operation in contract.GetMethods())
                {
                    var target = mapping.TargetMethods[Array.IndexOf(mapping.InterfaceMethods, operation)];
                    Assert.False(target.IsAbstract);
                    Assert.Equal(adapter.GetType().GetMethod(operation.Name) ?? adapterContract.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Single(method => method.Name.EndsWith("." + operation.Name, StringComparison.Ordinal)), target);
                }
                var definition = (EmulationEmulatorDefinition)adapter.GetType().GetInterfaces().SelectMany(type => type.GetProperties()).First(property => property.Name == "Definition").GetValue(adapter)!;
                var machine = machines.First(item => definition.MachineIds.Contains(item.Id));
                var original = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                var configuration = await (ValueTask<IEmulationConfiguration>)engineType.GetMethod("UseEmulatorAsync")!.Invoke(engine, [original, definition.Id, CancellationToken.None])!;
                var source = (IReadOnlyDictionary<string, string>?)configurationType.GetProperty("Options")!.GetValue(configuration);
                var snapshot = source?.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
                var defaults = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(contract.GetMethod("GetDefaultOptions")!.Invoke(adapter, [configuration]));
                var options = source is null ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(source, StringComparer.Ordinal);
                const string marker = "configuration-contract-probe";
                options[marker] = "preserved";
                var before = new Dictionary<string, string>(options, StringComparer.Ordinal);
                contract.GetMethod("InitializeOptions")!.Invoke(adapter, [configuration, options]);
                Assert.Equal("preserved", options[marker]);
                foreach (var item in defaults) Assert.Equal(before.GetValueOrDefault(item.Key, item.Value), options[item.Key]);
                Assert.Equal(configurationType.GetProperty("Model")!.GetValue(configuration), contract.GetMethod("ResolveModel")!.Invoke(adapter, [configuration]));
                var normalized = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(contract.GetMethod("NormalizeOptions")!.Invoke(adapter, [configuration]));
                var normalizer = mapping.TargetMethods[Array.FindIndex(mapping.InterfaceMethods, method => method.Name == "NormalizeOptions")];
                if (normalizer.DeclaringType == adapterContract)
                {
                    if (source is null) Assert.Empty(normalized);
                    else Assert.Same(source, normalized);
                }
                if (snapshot is null) Assert.Null(configurationType.GetProperty("Options")!.GetValue(configuration));
                else Assert.Equal(snapshot.OrderBy(item => item.Key, StringComparer.Ordinal), source!.OrderBy(item => item.Key, StringComparer.Ordinal));
                options.Remove(marker);
                var prepared = prepare.Invoke(engine, [configuration])!;
                var preparedOptions = (IReadOnlyDictionary<string, string>?)configurationType.GetProperty("Options")!.GetValue(prepared);
                Assert.Equal(options.OrderBy(item => item.Key, StringComparer.Ordinal), (preparedOptions ?? new Dictionary<string, string>()).OrderBy(item => item.Key, StringComparer.Ordinal));
                if (source is null ? options.Count == 0 : source.Count == options.Count && source.All(item => options.TryGetValue(item.Key, out var value) && value == item.Value))
                    Assert.Same(configuration, prepared);
                initializedAdapters++;
            }
            if (root == "GWGUI.Emulation.Microsoft") Assert.Empty(adapters);
            else Assert.NotEmpty(adapters);
        }
        Assert.True(initializedAdapters >= 24);
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
            Assert.Equal(contractName == "IEmulatorSettings" ? 7 : contractName is "IEmulatorPointerCapabilities" or "IEmulatorVideoSettings" ? 2 : 1, methods.Length);
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
                    var map = implementation.GetInterfaceMap(contract);
                    var selected = map.TargetMethods[Array.FindIndex(map.InterfaceMethods, candidate => candidate.Name == methodName)];
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
                    var settingsMap = implementation.GetInterfaceMap(contract);
                    var description = settingsMap.TargetMethods[Array.FindIndex(settingsMap.InterfaceMethods, candidate => candidate.Name == methodName)];
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
                var settings = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettings", throwOnError: true)!;
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
    public async Task EveryCommonResolvesMainMemoryThroughTheSettingsContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal)).Select(Assembly.Load)
            .Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var fractions = 0;
        var unreported = 0;
        var pspVariants = 0;
        var configuredMemory = 0;
        var amstradMemory = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettings", throwOnError: true)!;
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var configurationType = assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration", throwOnError: true)!;
            var operation = contract.GetMethod("GetMemoryKib")!;
            Assert.NotNull(operation);
            Assert.True(operation.IsAbstract);
            Assert.Equal(typeof(decimal?), operation.ReturnType);
            Assert.Equal(configurationType, Assert.Single(operation.GetParameters()).ParameterType);
            var extension = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettingsAdapter");
            if (extension is not null) Assert.Null(extension.GetMethod(operation.Name));
            var defaults = Assert.Single(adapterContract.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic), method => method.Name.EndsWith(".GetMemoryKib", StringComparison.Ordinal));
            var callers = assembly.GetTypes().Where(type => type.FullName!.Contains(".Functions.", StringComparison.Ordinal))
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => method.GetMethodBody() is not null && GetCompiledCalls(method).Any(call => call.Method == operation)).ToArray();
            Assert.NotEmpty(callers);
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var engine = Activator.CreateInstance(engineType)!;
            var dictionary = engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine)!;
            var adapters = ((System.Collections.IEnumerable)dictionary.GetType().GetProperty("Values")!.GetValue(dictionary)!).Cast<object>();
            var machines = (IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!;
            foreach (var adapter in adapters)
            {
                var map = adapter.GetType().GetInterfaceMap(contract);
                Assert.Equal(adapter.GetType().GetMethod(operation.Name) ?? defaults, map.TargetMethods[Array.IndexOf(map.InterfaceMethods, operation)]);
                var definition = (EmulationEmulatorDefinition)adapter.GetType().GetInterfaces().SelectMany(type => type.GetProperties()).First(property => property.Name == "Definition").GetValue(adapter)!;
                foreach (var machine in machines.Where(machine => definition.MachineIds.Contains(machine.Id)))
                {
                    var initial = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                    var configuration = await (ValueTask<IEmulationConfiguration>)engineType.GetMethod("UseEmulatorAsync")!.Invoke(engine, [initial, definition.Id, CancellationToken.None])!;
                    var before = System.Text.Json.JsonSerializer.Serialize(configuration, configurationType);
                    var memory = (decimal?)operation.Invoke(adapter, [configuration]);
                    if (memory is null) unreported++;
                    else
                    {
                        Assert.True(memory >= 0);
                        Assert.Equal(decimal.Truncate(memory.Value * 1024m), memory.Value * 1024m);
                        if (memory > 0 && memory < 1) fractions++;
                    }
                    if (root == "GWGUI.Emulation.Fairchild") Assert.Equal(0.0625m, memory);
                    if (root == "GWGUI.Emulation.Atari" && machine.Id == "Atari2600") Assert.Equal(0.125m, memory);
                    object WithOptions(IReadOnlyDictionary<string, string> options)
                    {
                        var copy = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(configuration, null)!;
                        configurationType.GetProperty("Options")!.SetValue(copy, options);
                        return copy;
                    }
                    var values = new Dictionary<string, string>((IReadOnlyDictionary<string, string>?)configurationType.GetProperty("Options")!.GetValue(configuration) ?? new Dictionary<string, string>(), StringComparer.Ordinal);
                    if (root == "GWGUI.Emulation.Sony" && definition.Id == "ppsspp")
                    {
                        var constants = assembly.GetType(root + ".Emulators.Ppsspp.Constants.PpssppConstants", throwOnError: true)!;
                        var options = assembly.GetType(root + ".Emulators.Ppsspp.Constants.OptionConstants", throwOnError: true)!;
                        var key = (string)options.GetField("PspModelKey", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue()!;
                        values[key] = (string)constants.GetField("Psp1000Model", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue()!;
                        var basic = (decimal?)operation.Invoke(adapter, [WithOptions(values)]);
                        values.Remove(key);
                        var expanded = (decimal?)operation.Invoke(adapter, [WithOptions(values)]);
                        Assert.Equal((decimal)(int)constants.GetField("ExpandedMemoryKib", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue()!, expanded);
                        Assert.True(expanded > basic);
                        pspVariants++;
                    }
                    if (root == "GWGUI.Emulation.Atari" && machine.Id == "St")
                    {
                        var constants = assembly.GetType(root + ".Common.Machines.Common.Constants.SettingsConstants", throwOnError: true)!;
                        var key = (string)constants.GetField("MainMemory", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue()!;
                        values[key] = "4194304";
                        Assert.Equal(4096m, (decimal?)operation.Invoke(adapter, [WithOptions(values)]));
                        configuredMemory++;
                    }
                    if (root == "GWGUI.Emulation.Amstrad" && machine.Id.Contains("464", StringComparison.OrdinalIgnoreCase) && definition.Id != "crocods")
                    {
                        var constants = assembly.GetType(root + ".Common.Machines.Common.Constants.SettingsConstants", throwOnError: true)!;
                        var key = (string)constants.GetField("Ram", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue()!;
                        values[key] = "576";
                        Assert.Equal(576m, (decimal?)operation.Invoke(adapter, [WithOptions(values)]));
                        amstradMemory++;
                        configuredMemory++;
                    }
                    Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(configuration, configurationType));
                }
            }
        }
        Assert.True(fractions > 0);
        Assert.True(unreported > 0);
        Assert.True(pspVariants > 0);
        Assert.True(configuredMemory > 0);
        Assert.True(amstradMemory > 0);
    }
    [Fact]
    public async Task EveryCommonResolvesAudioThroughTheSettingsContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal)).Select(Assembly.Load)
            .Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var constrained = 0;
        var runtimeConsumers = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettings", throwOnError: true)!;
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var audioType = assembly.GetType(root + ".Common.Machines.Common.Contracts.AudioConfiguration", throwOnError: true)!;
            Assert.Equal(new[] { "LatencyMilliseconds", "OutputDeviceId", "StereoSeparation" }, audioType.GetProperties().Select(property => property.Name).Order(StringComparer.Ordinal));
            var resolve = contract.GetMethod("GetAudioConfiguration")!;
            var choices = contract.GetMethod("GetAudioLatencyChoices")!;
            Assert.Equal(audioType, resolve.ReturnType);
            Assert.Equal(typeof(IReadOnlyList<int>), choices.ReturnType);
            Assert.True(resolve.IsAbstract);
            Assert.True(choices.IsAbstract);
            var configurationType = assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration", throwOnError: true)!;
            Assert.Equal(configurationType, Assert.Single(resolve.GetParameters()).ParameterType);
            Assert.Equal(configurationType, Assert.Single(choices.GetParameters()).ParameterType);
            var extension = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettingsAdapter");
            if (extension is not null)
            {
                Assert.Null(extension.GetMethod(resolve.Name));
                Assert.Null(extension.GetMethod(choices.Name));
            }
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var runtime = engineType.GetMethod("CreateRuntimeAsync")!;
            var state = runtime.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType;
            var methods = state is null ? new[] { runtime } : state.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var calls = methods.Where(method => method.GetMethodBody() is not null).SelectMany(GetCompiledCalls).ToArray();
            if (root == "GWGUI.Emulation.Microsoft")
                Assert.DoesNotContain(calls, call => call.Method == resolve);
            else
            {
                Assert.Contains(calls, call => call.Method == resolve);
                runtimeConsumers++;
            }
            var engine = Activator.CreateInstance(engineType)!;
            var dictionary = engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine)!;
            var adapters = ((System.Collections.IEnumerable)dictionary.GetType().GetProperty("Values")!.GetValue(dictionary)!).Cast<object>();
            var machines = (IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!;
            foreach (var adapter in adapters)
            {
                var map = adapter.GetType().GetInterfaceMap(contract);
                foreach (var operation in new[] { resolve, choices })
                {
                    var target = map.TargetMethods[Array.IndexOf(map.InterfaceMethods, operation)];
                    var fallback = adapterContract.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(method => method.Name.EndsWith("." + operation.Name, StringComparison.Ordinal));
                    Assert.Equal(adapter.GetType().GetMethod(operation.Name) ?? fallback, target);
                }
                var definition = (EmulationEmulatorDefinition)adapter.GetType().GetInterfaces().SelectMany(type => type.GetProperties()).First(property => property.Name == "Definition").GetValue(adapter)!;
                foreach (var machine in machines.Where(machine => definition.MachineIds.Contains(machine.Id)))
                {
                    var initial = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                    var configuration = await (ValueTask<IEmulationConfiguration>)engineType.GetMethod("UseEmulatorAsync")!.Invoke(engine, [initial, definition.Id, CancellationToken.None])!;
                    var before = System.Text.Json.JsonSerializer.Serialize(configuration, configurationType);
                    var latencies = Assert.IsAssignableFrom<IReadOnlyList<int>>(choices.Invoke(adapter, [configuration]));
                    Assert.NotEmpty(latencies);
                    Assert.All(latencies, latency => Assert.True(latency > 0));
                    Assert.Equal(latencies.Count, latencies.Distinct().Count());
                    var modified = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(configuration, null)!;
                    var stored = configurationType.GetProperty("Audio");
                    object Probe()
                    {
                        var audio = resolve.Invoke(adapter, [modified])!;
                        Assert.Equal(stored is null ? 100 : 37, audioType.GetProperty("StereoSeparation")!.GetValue(audio));
                        return audio;
                    }
                    if (stored is not null)
                    {
                        var audio = Activator.CreateInstance(audioType, ["audio-contract-device", 87, 37])!;
                        stored.SetValue(modified, audio);
                        var resolved = Probe();
                        Assert.Equal("audio-contract-device", audioType.GetProperty("OutputDeviceId")!.GetValue(resolved));
                        var latency = (int)audioType.GetProperty("LatencyMilliseconds")!.GetValue(resolved)!;
                        if (root == "GWGUI.Emulation.Sony" && definition.Id == "pcee2")
                        {
                            Assert.Contains(latency, latencies);
                            Assert.NotEqual(87, latency);
                            constrained++;
                        }
                        else Assert.Equal(87, latency);
                    }
                    else
                    {
                        var constants = assembly.GetType(root + ".Common.Machines.Common.Constants." + (root == "GWGUI.Emulation.Atari" ? "VideoAudioSettingsConstants" : "SettingsConstants"), throwOnError: true)!;
                        string Key(string name) => (string)constants.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
                        var output = Key(root == "GWGUI.Emulation.Atari" ? "AudioOutputOption" : "AudioOutput");
                        var latency = Key(root == "GWGUI.Emulation.Atari" ? "AudioLatencyOption" : "AudioLatency");
                        var defaults = Key(root == "GWGUI.Emulation.Atari" ? "DefaultAudioOutput" : "DefaultOutput");
                        var options = new Dictionary<string, string>((IReadOnlyDictionary<string, string>)configurationType.GetProperty("Options")!.GetValue(configuration)!, StringComparer.Ordinal)
                        { [output] = "audio-contract-device", [latency] = "87" };
                        configurationType.GetProperty("Options")!.SetValue(modified, options);
                        var resolved = Probe();
                        Assert.Equal("audio-contract-device", audioType.GetProperty("OutputDeviceId")!.GetValue(resolved));
                        Assert.Equal(87, audioType.GetProperty("LatencyMilliseconds")!.GetValue(resolved));
                        options[output] = defaults;
                        options[latency] = "50";
                        Assert.Null(audioType.GetProperty("OutputDeviceId")!.GetValue(Probe()));
                        options[latency] = "invalid";
                        if (root == "GWGUI.Emulation.Example")
                            Assert.IsType<FormatException>(Assert.Throws<TargetInvocationException>(() => resolve.Invoke(adapter, [modified])).InnerException);
                        else Assert.Equal(50, audioType.GetProperty("LatencyMilliseconds")!.GetValue(Probe()));
                    }
                    Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(configuration, configurationType));
                }
            }
        }
        Assert.Equal(23, runtimeConsumers);
        Assert.True(constrained > 0);
    }
    [Fact]
    public async Task EveryCommonRoutesGraphicsSettingsThroughItsVideoContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal)).Select(Assembly.Load)
            .Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var nativeConversions = 0;
        var unchangedConfigurations = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorVideoSettings", throwOnError: true)!;
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var operation = contract.GetMethod("ApplyVideoSettings")!;
            var configurationType = assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration", throwOnError: true)!;
            Assert.NotNull(operation);
            Assert.True(operation.IsAbstract);
            Assert.Equal(configurationType, operation.ReturnType);
            Assert.Equal(new[] { configurationType, typeof(EmulationGraphicsApi) }, operation.GetParameters().Select(parameter => parameter.ParameterType));
            Assert.Contains(contract, adapterContract.GetInterfaces());
            var defaults = Assert.Single(adapterContract.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic), method => method.Name.EndsWith(".ApplyVideoSettings", StringComparison.Ordinal));
            Assert.Empty(GetCompiledCalls(defaults)); // Le défaut ne lance aucune API graphique et ne possède aucune ressource.
            var settings = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettingsAdapter");
            if (settings is not null) Assert.Null(settings.GetMethod(operation.Name));
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var engineOperation = engineType.GetMethod(operation.Name)!;
            Assert.NotNull(engineOperation);
            Assert.Contains(GetCompiledCalls(engineOperation), call => call.Method == operation);
            var facadeType = Assert.Single(assembly.GetTypes(), type => type.IsClass && !type.IsAbstract && typeof(IEmulationModule).IsAssignableFrom(type));
            Assert.True(typeof(IEmulationVideoSettingsManager).IsAssignableFrom(facadeType));
            var facadeOperation = Assert.Single(facadeType.GetInterfaceMap(typeof(IEmulationVideoSettingsManager)).TargetMethods);
            Assert.Contains(GetCompiledCalls(facadeOperation), call => call.Method == engineOperation);
            var engine = Activator.CreateInstance(engineType)!;
            var facade = RuntimeHelpers.GetUninitializedObject(facadeType);
            facadeType.GetField("_engine", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(facade, engine);
            var manager = (IEmulationVideoSettingsManager)facade;
            Assert.Throws<ArgumentException>(() => manager.ApplyVideoSettings(null!, EmulationGraphicsApi.OpenGL));
            var dictionary = engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine)!;
            var adapters = ((System.Collections.IEnumerable)dictionary.GetType().GetProperty("Values")!.GetValue(dictionary)!).Cast<object>().ToArray();
            var machines = (IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!;
            foreach (var adapter in adapters)
            {
                var map = adapter.GetType().GetInterfaceMap(contract);
                var target = map.TargetMethods[Array.IndexOf(map.InterfaceMethods, operation)];
                Assert.Equal(adapter.GetType().GetMethod(operation.Name) ?? defaults, target);
                var definition = (EmulationEmulatorDefinition)adapter.GetType().GetInterfaces().SelectMany(type => type.GetProperties()).First(property => property.Name == "Definition").GetValue(adapter)!;
                foreach (var machine in machines.Where(machine => definition.MachineIds.Contains(machine.Id)))
                {
                    var initial = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                    var configuration = await (ValueTask<IEmulationConfiguration>)engineType.GetMethod("UseEmulatorAsync")!.Invoke(engine, [initial, definition.Id, CancellationToken.None])!;
                    var before = System.Text.Json.JsonSerializer.Serialize(configuration, configurationType);
                    foreach (var api in Enum.GetValues<EmulationGraphicsApi>())
                    {
                        var expected = (IEmulationConfiguration)operation.Invoke(adapter, [configuration, api])!;
                        var actual = manager.ApplyVideoSettings(configuration, api);
                        Assert.Equal(configuration.Id, actual.Id);
                        Assert.Equal(configuration.MachineId, actual.MachineId);
                        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(expected, configurationType), System.Text.Json.JsonSerializer.Serialize(actual, configurationType));
                        Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(configuration, configurationType));
                        if (target == defaults)
                        {
                            Assert.Same(configuration, actual);
                            unchangedConfigurations++;
                        }
                        else
                        {
                            Assert.Equal("GWGUI.Emulation.Sony", root);
                            Assert.Equal(api, configurationType.GetProperty("GraphicsApi")!.GetValue(actual));
                            nativeConversions++;
                        }
                    }
                }
            }
            if (adapters.Length == 0)
            {
                Assert.Equal("GWGUI.Emulation.Microsoft", root);
                foreach (var machine in machines)
                {
                    var configuration = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                    Assert.Same(configuration, manager.ApplyVideoSettings(configuration, EmulationGraphicsApi.Vulkan));
                }
            }
        }
        Assert.True(nativeConversions > 0);
        Assert.True(unchangedConfigurations > 0);
    }
    [Fact]
    public async Task EveryCommonProvidesTheSameFirmwareSlotCatalogue()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal)).Select(Assembly.Load)
            .Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var nativeCatalogues = 0;
        var projectedCatalogues = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var slot = assembly.GetType(root + ".Common.Machines.Common.Contracts.FirmwareSlot", throwOnError: true)!;
            var fields = new[] { "FieldId", "FileName", "LabelResourceKey", "IsRequired", "MachineIds", "DisplayName", "ExplanationResourceKey", "DetailedExplanationResourceKey", "CandidateFileNames", "SystemSubdirectory", "KnownMd5" };
            Assert.Equal(fields.Order(StringComparer.Ordinal), slot.GetProperties().Select(property => property.Name).Order(StringComparer.Ordinal));
            Assert.Null(assembly.GetType(root + ".Common.Contracts.FirmwareSlot"));
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareAdapter", throwOnError: true)!;
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var catalogue = contract.GetMethod("GetFirmwareSlots")!;
            Assert.NotNull(catalogue);
            Assert.True(catalogue.IsAbstract);
            Assert.Equal(typeof(IReadOnlyList<>).MakeGenericType(slot), catalogue.ReturnType);
            var defaultCatalogue = Assert.Single(adapterContract.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic), method => method.Name.EndsWith(".GetFirmwareSlots", StringComparison.Ordinal));
            Assert.Contains(GetCompiledCalls(defaultCatalogue), call => call.Method.Name == "DescribeSettings");
            var settings = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettings", throwOnError: true)!.GetMethod("DescribeSettings")!;
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var engine = Activator.CreateInstance(engineType)!;
            var dictionary = engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine)!;
            var adapters = ((System.Collections.IEnumerable)dictionary.GetType().GetProperty("Values")!.GetValue(dictionary)!).Cast<object>();
            var machines = (IReadOnlyList<EmulationMachineDefinition>)engineType.GetProperty("Machines")!.GetValue(engine)!;
            foreach (var adapter in adapters)
            {
                var map = adapter.GetType().GetInterfaceMap(contract);
                var target = map.TargetMethods[Array.IndexOf(map.InterfaceMethods, catalogue)];
                Assert.Equal(adapter.GetType().GetMethod(catalogue.Name) ?? defaultCatalogue, target);
                var definition = (EmulationEmulatorDefinition)adapter.GetType().GetInterfaces().SelectMany(type => type.GetProperties()).First(property => property.Name == "Definition").GetValue(adapter)!;
                foreach (var machine in machines.Where(machine => definition.MachineIds.Contains(machine.Id)))
                {
                    var initial = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                    var configuration = await (ValueTask<IEmulationConfiguration>)engineType.GetMethod("UseEmulatorAsync")!.Invoke(engine, [initial, definition.Id, CancellationToken.None])!;
                    var entries = ((System.Collections.IEnumerable)catalogue.Invoke(adapter, [configuration])!).Cast<object>().ToArray();
                    object? Value(object entry, string property) => slot.GetProperty(property)!.GetValue(entry);
                    Assert.Equal(entries.Length, entries.Select(entry => Value(entry, "FieldId")).Distinct().Count());
                    foreach (var entry in entries)
                    {
                        Assert.False(string.IsNullOrWhiteSpace((string?)Value(entry, "FieldId")));
                        Assert.False(string.IsNullOrWhiteSpace((string?)Value(entry, "LabelResourceKey")));
                    }
                    if (target == defaultCatalogue)
                    {
                        var blocks = (IReadOnlyList<EmulationSettingsBlock>)settings.Invoke(adapter, [configuration, null])!;
                        var expected = blocks.SelectMany(block => block.Fields).Where(field => field.Editor == EmulationSettingsEditor.Path && (field.Tab == EmulationMachineTab.Rom || field.DefaultFolderCategory == EmulationDefaultFolderCategory.Firmware))
                            .GroupBy(field => field.Id, StringComparer.Ordinal).Select(group => group.First()).ToArray();
                        Assert.Equal(expected.Select(field => field.Id), entries.Select(entry => (string)Value(entry, "FieldId")!));
                        for (var index = 0; index < entries.Length; index++)
                        {
                            Assert.Equal(expected[index].LabelResourceKey, Value(entries[index], "LabelResourceKey"));
                            Assert.Equal(expected[index].ExplanationResourceKey, Value(entries[index], "ExplanationResourceKey"));
                            Assert.Equal(expected[index].DetailedExplanationResourceKey, Value(entries[index], "DetailedExplanationResourceKey"));
                            Assert.Equal(string.Empty, Value(entries[index], "FileName"));
                        }
                        projectedCatalogues++;
                    }
                    else nativeCatalogues++;
                    if (root == "GWGUI.Emulation.Nec" && definition.Id == "np2kai")
                    {
                        Assert.NotEmpty(entries);
                        Assert.All(entries, entry => Assert.Equal("np2kai", Value(entry, "SystemSubdirectory")));
                        var bios = Assert.Single(entries, entry => (string)Value(entry, "FileName")! == "bios.rom");
                        Assert.Contains("bios.rom", (IReadOnlyList<string>)Value(bios, "CandidateFileNames")!);
                    }
                    if (root == "GWGUI.Emulation.Sony" && definition.Id is "pcsx2" or "pcee2")
                    {
                        var rom1 = Assert.Single(entries, entry => (string)Value(entry, "FileName")! == "pcsx2/bios/scph.rom1");
                        Assert.Equal("pcsx2/bios/scph.rom1", Value(rom1, "DisplayName"));
                        Assert.NotNull(Value(rom1, "ExplanationResourceKey"));
                        Assert.NotNull(Value(rom1, "CandidateFileNames"));
                    }
                }
            }
        }
        Assert.True(nativeCatalogues > 0);
        Assert.True(projectedCatalogues > 0);
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
            Assert.Equal(7, contract.GetMethods().Length);
            var scan = Assert.Single(contract.GetMethods(), method => method.Name == "ScanFirmwareFileAsync");
            Assert.Equal("ScanFirmwareFileAsync", scan.Name);
            Assert.True(scan.IsAbstract);
            Assert.Equal(typeof(ValueTask<IReadOnlyList<EmulationFirmwareCandidate>>), scan.ReturnType);
            var parameters = scan.GetParameters();
            Assert.Equal(3, parameters.Length);
            Assert.Equal(assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration"), parameters[0].ParameterType);
            Assert.Equal(typeof(string), parameters[1].ParameterType);
            Assert.Equal(typeof(CancellationToken), parameters[2].ParameterType);
            // Vérifie la préparation sans l'exécuter : les dossiers de session appartiennent aux hôtes.
            var prepare = contract.GetMethod("PrepareFirmware")!;
            Assert.NotNull(prepare);
            Assert.True(prepare.IsAbstract);
            Assert.Equal(typeof(void), prepare.ReturnType);
            Assert.Equal(new[] { parameters[0].ParameterType, typeof(string) }, prepare.GetParameters().Select(parameter => parameter.ParameterType));
            var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Contains(contract, adapter.GetInterfaces());
            var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && contract.IsAssignableFrom(type)).ToArray();
            foreach (var implementation in implementations)
            {
                var map = implementation.GetInterfaceMap(contract);
                var target = map.TargetMethods[Array.FindIndex(map.InterfaceMethods, method => method.Name == "ScanFirmwareFileAsync")];
                Assert.False(target.IsAbstract);
                Assert.True(target.IsPublic);
                Assert.False(target.DeclaringType!.IsInterface);
                Assert.Null(implementation.GetMethod("InspectFirmwareAsync"));
                Assert.Null(implementation.GetMethod("ScanFirmware"));
            }
            foreach (var implementation in implementations)
            {
                var map = implementation.GetInterfaceMap(contract);
                var target = map.TargetMethods[Array.FindIndex(map.InterfaceMethods, method => method.Name == prepare.Name)];
                Assert.False(target.IsAbstract);
                var native = implementation.GetMethod(prepare.Name, prepare.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
                if (native is not null)
                    Assert.Equal(native, target); // Un défaut Common ne doit pas masquer la préparation native.
                else
                {
                    Assert.Equal(adapter, target.DeclaringType);
                    Assert.Contains(GetCompiledCalls(target), call => call.Method.DeclaringType == typeof(Directory) && call.Method.Name == nameof(Directory.CreateDirectory));
                }
            }
            var preparationCallers = assembly.GetTypes().SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => method.GetMethodBody() is not null && GetCompiledCalls(method).Any(call => call.Method.DeclaringType == contract && call.Method.Name == prepare.Name)).ToArray();
            if (root.Split('.').Last() is "Apple" or "Atari" or "Coleco" or "Commodore" or "Epoch" or "Example" or "Fairchild" or "Mattel" or "Nec" or "Nintendo" or "Panasonic" or "Philips" or "Sega" or "Sharp" or "Sinclair" or "Snk" or "Wenquxing")
                Assert.NotEmpty(preparationCallers);
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
            var inspection = contract.GetMethod("ScanFirmwareAsync")!;
            var commonMethods = assembly.GetTypes().Where(type => type.FullName!.StartsWith(root + ".Common.", StringComparison.Ordinal))
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => method.GetMethodBody() is not null);
            Assert.Contains(commonMethods, method => Calls(method, inspection));
        }
    }

    [Fact]
    public async Task EveryCommonDescribesAndChecksFirmwareThroughItsSingleContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal)).Select(Assembly.Load)
            .Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var nativeProfiles = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareAdapter", throwOnError: true)!;
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var configurationType = assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration", throwOnError: true)!;
            var slotType = assembly.GetType(root + ".Common.Machines.Common.Contracts.FirmwareSlot", throwOnError: true)!;
            Assert.Null(assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareCapabilities"));
            Assert.Null(assembly.GetType(root + ".Common.Machines.Common.Contracts.FirmwareRequirement"));
            var compatible = contract.GetMethod("IsFirmwareCompatible")!;
            var describe = contract.GetMethod("GetFirmwareSlots")!;
            Assert.True(compatible.IsAbstract);
            Assert.Equal(typeof(bool), compatible.ReturnType);
            Assert.Equal(new[] { configurationType, typeof(string), typeof(string) }, compatible.GetParameters().Select(parameter => parameter.ParameterType));
            Assert.Equal(new[] { "configuration", "fieldId", "path" }, compatible.GetParameters().Select(parameter => parameter.Name));
            Assert.Equal(typeof(IReadOnlyList<>).MakeGenericType(slotType), describe.ReturnType);
            Assert.Contains(contract, adapterContract.GetInterfaces());
            var fallback = adapterContract.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(method => method.Name.EndsWith(".IsFirmwareCompatible", StringComparison.Ordinal));
            Assert.Contains(GetCompiledCalls(fallback), call => call.Method == describe);
            Assert.Contains(GetCompiledCalls(fallback), call => call.Method == contract.GetMethod("ScanFirmwareFileAsync"));
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var engine = Activator.CreateInstance(engineType, nonPublic: true)!;
            var adapters = (System.Collections.IEnumerable)engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine)!;
            var machines = Assert.IsAssignableFrom<IReadOnlyList<EmulationMachineDefinition>>(engineType.GetProperty("Machines")!.GetValue(engine));
            foreach (var entry in adapters)
            {
                var adapter = entry.GetType().GetProperty("Value")!.GetValue(entry)!;
                Assert.Null(adapter.GetType().GetMethod("DescribeFirmware"));
                var map = adapter.GetType().GetInterfaceMap(contract);
                Assert.Equal(adapter.GetType().GetMethod("IsFirmwareCompatible") ?? fallback, map.TargetMethods[Array.IndexOf(map.InterfaceMethods, compatible)]);
                var definition = (EmulationEmulatorDefinition)adapter.GetType().GetInterfaces().SelectMany(type => type.GetProperties()).First(property => property.Name == "Definition").GetValue(adapter)!;
                foreach (var machine in machines.Where(machine => definition.MachineIds.Contains(machine.Id)))
                {
                    var initial = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                    var configuration = await (ValueTask<IEmulationConfiguration>)engineType.GetMethod("UseEmulatorAsync")!.Invoke(engine, [initial, definition.Id, CancellationToken.None])!;
                    var before = System.Text.Json.JsonSerializer.Serialize(configuration, configurationType);
                    // Un champ inconnu doit être refusé avant toute tentative d'accès au chemin, volontairement invalide.
                    Assert.False((bool)compatible.Invoke(adapter, [configuration, "unknown-firmware-field", "\0"])!);
                    if (root is "GWGUI.Emulation.Panasonic" or "GWGUI.Emulation.Wenquxing")
                    {
                        var slots = ((System.Collections.IEnumerable)describe.Invoke(adapter, [configuration])!).Cast<object>().ToArray();
                        Assert.NotEmpty(slots);
                        var functions = assembly.GetType(root + ".Common.Machines.Common.Functions.FirmwareFunctions", throwOnError: true)!;
                        var role = functions.GetMethod("Role", BindingFlags.Static | BindingFlags.NonPublic)!;
                        var field = functions.GetMethod("Field", BindingFlags.Static | BindingFlags.NonPublic)!;
                        foreach (var slot in slots)
                        {
                            var id = (string)slotType.GetProperty("FieldId")!.GetValue(slot)!;
                            Assert.Equal(id, field.Invoke(null, [role.Invoke(null, [id])]));
                            var names = Assert.IsAssignableFrom<IReadOnlyList<string>>(slotType.GetProperty("CandidateFileNames")!.GetValue(slot));
                            Assert.NotEmpty(names);
                            Assert.Equal(names[0], slotType.GetProperty("FileName")!.GetValue(slot));
                            if (root == "GWGUI.Emulation.Panasonic") Assert.False((bool)slotType.GetProperty("IsRequired")!.GetValue(slot)!);
                        }
                        if (root == "GWGUI.Emulation.Panasonic") Assert.Equal(2, slots.Length);
                        else
                        {
                            var model = configurationType.GetProperty("Model")!.GetValue(configuration)!.ToString()!;
                            var expected = model switch
                            {
                                "NC1020" => new[] { "obj_lu.bin", "nc1020.fls" },
                                "PC1000" => new[] { "pc1000.rom", "pc1000.fls" },
                                "CC800" => new[] { "obj.bin", "cc800.fls" },
                                "NC2000" => new[] { "nc2000.nor", "nc2000.nand", "nc2000.nand0" },
                                "NC3000" => new[] { "nc3000.nor", "nc3000.nand", "nc3000.nand0" },
                                _ => throw new InvalidOperationException(model)
                            };
                            Assert.Equal(expected, slots.Select(slot => (string)slotType.GetProperty("FileName")!.GetValue(slot)!));
                            Assert.Equal(model == "NC3000" ? new[] { true, true, false } : Enumerable.Repeat(true, expected.Length), slots.Select(slot => (bool)slotType.GetProperty("IsRequired")!.GetValue(slot)!));
                        }
                        nativeProfiles++;
                    }
                    Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(configuration, configurationType));
                }
            }
        }
        Assert.Equal(6, nativeProfiles);
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

    [Fact]
    public void EveryCommonIdentifiesFirmwareThroughTheSharedObservedDataContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal)).Select(Assembly.Load)
            .Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var recognizedCount = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareAdapter", throwOnError: true)!;
            var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            Assert.Null(assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareInspection"));
            var identify = contract.GetMethod("IdentifyFirmware")!;
            Assert.True(identify.IsAbstract);
            Assert.True(identify.IsGenericMethodDefinition);
            var definition = Assert.Single(identify.GetGenericArguments());
            Assert.True((definition.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0);
            var observed = typeof(EmulationFirmwareObservation<>).MakeGenericType(definition);
            Assert.Equal(observed, identify.ReturnType);
            Assert.Equal(observed, Assert.Single(identify.GetParameters()).ParameterType);
            Assert.Contains(contract, adapter.GetInterfaces());
            var defaults = adapter.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(method => method.Name.EndsWith(".IdentifyFirmware", StringComparison.Ordinal));
            Assert.Empty(GetCompiledCalls(defaults));
            var probe = new EmulationFirmwareObservation<string>(Path.Combine("firmware", "pac-n10.bin"), 4096, "md5", EmulationFirmwareDetectionStatus.Unknown, "typed-catalogue-data", EmulationFirmwareCompatibility.Unknown, true, null,
                Sha256: "sha256", LastWriteTimeUtc: DateTime.UnixEpoch, Type: "Unknown", IsVerified: false);
            var operation = identify.MakeGenericMethod(typeof(string));
            foreach (var implementation in assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)))
            {
                var map = implementation.GetInterfaceMap(contract);
                var native = implementation.GetMethod("IdentifyFirmware");
                Assert.Equal(native ?? defaults, map.TargetMethods[Array.IndexOf(map.InterfaceMethods, identify)]);
                var instance = RuntimeHelpers.GetUninitializedObject(implementation);
                var result = (EmulationFirmwareObservation<string>?)operation.Invoke(instance, [probe]);
                if (native is null) Assert.Null(result);
                else
                {
                    Assert.NotNull(result);
                    Assert.True(result.IsKnown);
                    Assert.False(result.IsOfficial);
                    Assert.Equal(probe.Path, result.Path);
                    Assert.Equal(probe.SizeBytes, result.SizeBytes);
                    Assert.Equal(probe.Md5, result.Md5);
                    Assert.Equal(probe.Sha256, result.Sha256);
                    Assert.Equal(probe.LastWriteTimeUtc, result.LastWriteTimeUtc);
                    Assert.Same(probe.Definition, result.Definition);
                    Assert.Equal(probe.Compatibility, result.Compatibility);
                    Assert.Equal(probe.IsDuplicate, result.IsDuplicate);
                    Assert.Equal(probe.IsVerified, result.IsVerified);
                    Assert.Equal("SystemRom", result.Type);
                    recognizedCount++;
                }
            }
            if (root is "GWGUI.Emulation.Example" or "GWGUI.Emulation.Nec")
            {
                var calls = assembly.GetTypes().Where(type => type.FullName!.StartsWith(root + ".Common.", StringComparison.Ordinal))
                    .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    .Where(method => method.GetMethodBody() is not null).SelectMany(GetCompiledCalls);
                Assert.Contains(calls, call => call.Method is MethodInfo { IsGenericMethod: true } info && info.GetGenericMethodDefinition() == identify);
            }
            Assert.False(probe.IsKnown);
        }
        Assert.Equal(1, recognizedCount);
    }
    [Theory]
    [InlineData("pac-n1.bin", true)]
    [InlineData("pce-lp1.bin", true)]
    [InlineData("PAC-N10.BIN", true)]
    [InlineData("unknown.bin", false)]
    public void NecFirmwareIdentificationPreservesObservedMetadata(string fileName, bool known)
    {
        var candidate = new EmulationFirmwareObservation<string>(
            Path.Combine("firmware", fileName), 4096, "observed-md5", EmulationFirmwareDetectionStatus.Unknown, "catalogue", EmulationFirmwareCompatibility.Unknown, false, null,
            Sha256: "observed-sha256", LastWriteTimeUtc: DateTime.UnixEpoch, Type: "Unknown", IsVerified: false);
        var owner = Assembly.Load("gwgui.emulation.nec").GetType("GWGUI.Emulation.Nec.Emulators.Geargrafx.Functions.GeargrafxFirmwareFunctions", throwOnError: true)!;
        var identify = owner.GetMethod("Identify", BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(typeof(string));
        var result = identify.Invoke(null, [candidate]);
        if (!known)
        {
            Assert.Null(result);
            return;
        }
        var recognized = Assert.IsType<EmulationFirmwareObservation<string>>(result);
        Assert.True(recognized.IsKnown);
        Assert.False(recognized.IsOfficial);
        Assert.False(recognized.IsVerified);
        Assert.Equal(candidate.Path, recognized.Path);
        Assert.Equal(candidate.SizeBytes, recognized.SizeBytes);
        Assert.Equal(candidate.Md5, recognized.Md5);
        Assert.Equal(candidate.Sha256, recognized.Sha256);
        Assert.Equal(candidate.LastWriteTimeUtc, recognized.LastWriteTimeUtc);
        Assert.Equal("SystemRom", recognized.Type);
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
    [Fact]
    public async Task EveryCommonRoutesSettingsScopesThroughItsContracts()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal)).Select(Assembly.Load)
            .Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var probed = 0;
        var cartridgeFields = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var settings = assembly.GetType(root + ".Common.Interfaces.IEmulatorSettings", throwOnError: true)!;
            var storage = assembly.GetType(root + ".Common.Interfaces.IEmulatorStorageSettings", throwOnError: true)!;
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var machineBlocks = settings.GetMethod("GetMachineSettingsBlocks")!;
            var deviceFields = storage.GetMethod("GetStorageDeviceConfigurationFields")!;
            Assert.True(machineBlocks.IsAbstract);
            Assert.True(deviceFields.IsAbstract);
            Assert.Equal(typeof(IReadOnlyList<EmulationSettingsBlock>), machineBlocks.ReturnType);
            Assert.Equal(typeof(IReadOnlyList<EmulationSettingsField>), deviceFields.ReturnType);
            Assert.Equal(typeof(EmulatorManagementContext), machineBlocks.GetParameters()[1].ParameterType);
            Assert.True(machineBlocks.GetParameters()[1].HasDefaultValue);
            Assert.Equal(typeof(EmulationMediaCategory), deviceFields.GetParameters()[1].ParameterType);
            Assert.Contains(settings, adapterContract.GetInterfaces());
            Assert.Contains(storage, adapterContract.GetInterfaces());
            var consumers = assembly.GetTypes().Where(type => type.Namespace?.StartsWith(root + ".Common.", StringComparison.Ordinal) == true && !type.IsInterface)
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => method.GetMethodBody() is not null).SelectMany(GetCompiledCalls).ToArray();
            Assert.Contains(consumers, call => call.Method == machineBlocks);
            if (root is "GWGUI.Emulation.Sony" or "GWGUI.Emulation.Coleco") Assert.Contains(consumers, call => call.Method == deviceFields);
            if (root == "GWGUI.Emulation.Sony") Assert.Null(assembly.GetType(root + ".Common.Interfaces.IEmulatorSettingsAdapter"));
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var engine = Activator.CreateInstance(engineType, nonPublic: true)!;
            var adapters = (System.Collections.IEnumerable)engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine)!;
            var machines = Assert.IsAssignableFrom<IReadOnlyList<EmulationMachineDefinition>>(engineType.GetProperty("Machines")!.GetValue(engine));
            foreach (var entry in adapters)
            {
                var adapter = entry.GetType().GetProperty("Value")!.GetValue(entry)!;
                foreach (var (contract, operation) in new[] { (settings, machineBlocks), (storage, deviceFields) })
                {
                    var map = adapter.GetType().GetInterfaceMap(contract);
                    var fallback = adapterContract.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(method => method.Name.EndsWith("." + operation.Name, StringComparison.Ordinal));
                    Assert.Equal(adapter.GetType().GetMethod(operation.Name) ?? fallback, map.TargetMethods[Array.IndexOf(map.InterfaceMethods, operation)]);
                }
                var definition = (EmulationEmulatorDefinition)adapter.GetType().GetInterfaces().SelectMany(type => type.GetProperties()).First(property => property.Name == "Definition").GetValue(adapter)!;
                foreach (var machine in machines.Where(machine => definition.MachineIds.Contains(machine.Id)))
                {
                    var initial = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                    var configuration = await (ValueTask<IEmulationConfiguration>)engineType.GetMethod("UseEmulatorAsync")!.Invoke(engine, [initial, definition.Id, CancellationToken.None])!;
                    var before = System.Text.Json.JsonSerializer.Serialize(configuration, configuration.GetType());
                    var original = (IReadOnlyList<EmulationSettingsBlock>)settings.GetMethod("DescribeSettings")!.Invoke(adapter, [configuration, null])!;
                    var actual = (IReadOnlyList<EmulationSettingsBlock>)machineBlocks.Invoke(adapter, [configuration, null])!;
                    if (root != "GWGUI.Emulation.Sony")
                    {
                        var expected = root == "GWGUI.Emulation.Coleco" ? original.Where(block => block.Tab != EmulationMachineTab.Storage).ToArray() : original;
                        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Json.JsonSerializer.Serialize(actual));
                    }
                    else
                    {
                        Assert.Equal(original.Where(block => block.Tab != EmulationMachineTab.Storage).SelectMany(block => block.Fields).Select(field => field.Id),
                            actual.Where(block => block.Tab != EmulationMachineTab.Storage).SelectMany(block => block.Fields).Select(field => field.Id));
                    }
                    foreach (var category in Enum.GetValues<EmulationMediaCategory>())
                    {
                        var fields = (IReadOnlyList<EmulationSettingsField>)deviceFields.Invoke(adapter, [configuration, category])!;
                        if (root == "GWGUI.Emulation.Coleco" && category == EmulationMediaCategory.CartridgeSlot)
                        {
                            Assert.Equal(original.Where(block => block.Tab == EmulationMachineTab.Storage).SelectMany(block => block.Fields).Select(field => field.Id), fields.Select(field => field.Id));
                            cartridgeFields += fields.Count;
                        }
                        else if (root != "GWGUI.Emulation.Sony") Assert.Empty(fields);
                        else if (category != EmulationMediaCategory.CompactDiscDrive) Assert.Empty(fields);
                    }
                    Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(configuration, configuration.GetType()));
                    probed++;
                }
            }
        }
        Assert.True(probed > 24);
        Assert.True(cartridgeFields > 0);
    }
    [Fact]
    public void EveryCommonRoutesDirectoryFirmwareSearchThroughItsContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal)).Select(Assembly.Load)
            .Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var nativeSearches = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareAdapter", throwOnError: true)!;
            var adapter = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var operation = contract.GetMethod("ScanFirmwareAsync")!;
            Assert.True(operation.IsAbstract);
            Assert.Equal(typeof(ValueTask<IReadOnlyList<EmulationFirmwareCandidate>>), operation.ReturnType);
            Assert.Equal(new[] { assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration")!, typeof(string), typeof(CancellationToken) }, operation.GetParameters().Select(parameter => parameter.ParameterType));
            Assert.Equal(new[] { "configuration", "directory", "cancellationToken" }, operation.GetParameters().Select(parameter => parameter.Name));
            Assert.Contains(contract, adapter.GetInterfaces());
            var defaults = adapter.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(method => method.Name.EndsWith(".ScanFirmwareAsync", StringComparison.Ordinal));
            var state = defaults.GetCustomAttribute<AsyncStateMachineAttribute>()!.StateMachineType;
            var body = state.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance)!;
            Assert.Contains(GetCompiledCalls(body), call => call.Method.Name == "ScanDirectoryAsync" && call.Method.DeclaringType == typeof(IEmulationFirmwareProcessing));
            var engine = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var entry = engine.GetMethod("ScanFirmwareAsync")!;
            var execution = entry.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance) ?? entry;
            Assert.Contains(GetCompiledCalls(execution), call => call.Method == operation);
            var implementations = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && adapter.IsAssignableFrom(type)).ToArray();
            if (root == "GWGUI.Emulation.Microsoft") Assert.Empty(implementations);
            else Assert.NotEmpty(implementations);
            foreach (var implementation in implementations)
            {
                Assert.Null(implementation.GetMethod("ScanFirmware"));
                var map = implementation.GetInterfaceMap(contract);
                var target = map.TargetMethods[Array.IndexOf(map.InterfaceMethods, operation)];
                var native = implementation.GetMethod("ScanFirmwareAsync");
                Assert.Equal(native ?? defaults, target);
                if (native is not null) nativeSearches++;
                if (root is "GWGUI.Emulation.Atari" or "GWGUI.Emulation.Commodore" or "GWGUI.Emulation.Example" or "GWGUI.Emulation.Nec" or "GWGUI.Emulation.Nintendo" or "GWGUI.Emulation.Panasonic" or "GWGUI.Emulation.Sega" or "GWGUI.Emulation.Sony" or "GWGUI.Emulation.Wenquxing") Assert.NotNull(native);
            }
            var settings = assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareSettings");
            if (settings is not null) Assert.Null(settings.GetMethod("ScanFirmware"));
            if (root == "GWGUI.Emulation.Commodore") Assert.Null(settings);
        }
        Assert.True(nativeSearches > 24);
    }
    [Fact]
    public async Task EveryCommonResolvesGlobalFirmwareRequirementsFromItsContract()
    {
        var modules = typeof(ArchitectureValidationTests).Assembly.GetReferencedAssemblies()
            .Where(name => name.Name!.StartsWith("gwgui.emulation.", StringComparison.Ordinal)).Select(Assembly.Load)
            .Append(typeof(GWGUI.Emulation.Example.Modules.ExampleModuleFactory).Assembly).Distinct().ToArray();
        Assert.Equal(24, modules.Length);
        var nativeAlternatives = 0;
        var nativeRoles = 0;
        foreach (var assembly in modules)
        {
            var factory = Assert.Single(assembly.GetTypes(), type => !type.IsAbstract && typeof(IEmulationModuleFactory).IsAssignableFrom(type));
            var root = factory.Namespace![..factory.Namespace!.LastIndexOf(".Modules", StringComparison.Ordinal)];
            var contract = assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareAdapter", throwOnError: true)!;
            var adapterContract = assembly.GetType(root + ".Common.Interfaces.IEmulatorAdapter", throwOnError: true)!;
            var configurationType = assembly.GetType(root + ".Common.Machines.Common.Contracts.MachineConfiguration", throwOnError: true)!;
            var slotType = assembly.GetType(root + ".Common.Machines.Common.Contracts.FirmwareSlot", throwOnError: true)!;
            Assert.Null(assembly.GetType(root + ".Common.Interfaces.IEmulatorFirmwareSettings"));
            var required = contract.GetMethod("RequiresExternalFirmware")!;
            Assert.True(required.IsAbstract);
            Assert.Equal(typeof(bool), required.ReturnType);
            Assert.Equal(configurationType, Assert.Single(required.GetParameters()).ParameterType);
            var defaults = adapterContract.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(method => method.Name.EndsWith(".RequiresExternalFirmware", StringComparison.Ordinal));
            Assert.Contains(GetCompiledCalls(defaults), call => call.Method == contract.GetMethod("GetFirmwareSlots"));
            var engineType = assembly.GetType(root + ".Common.Services.Engine", throwOnError: true)!;
            var engine = Activator.CreateInstance(engineType, nonPublic: true)!;
            var adapters = (System.Collections.IEnumerable)engineType.GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(engine)!;
            var machines = Assert.IsAssignableFrom<IReadOnlyList<EmulationMachineDefinition>>(engineType.GetProperty("Machines")!.GetValue(engine));
            foreach (var entry in adapters)
            {
                var adapter = entry.GetType().GetProperty("Value")!.GetValue(entry)!;
                var implementation = adapter.GetType();
                Assert.Null(implementation.GetProperty("RequiresExternalFirmware"));
                Assert.Null(implementation.GetMethod("FirmwareRoles"));
                Assert.Null(implementation.GetMethod("IsFirmwareRequired"));
                var map = implementation.GetInterfaceMap(contract);
                Assert.Equal(implementation.GetMethod(required.Name) ?? defaults, map.TargetMethods[Array.IndexOf(map.InterfaceMethods, required)]);
                var definition = (EmulationEmulatorDefinition)implementation.GetInterfaces().SelectMany(type => type.GetProperties()).First(property => property.Name == "Definition").GetValue(adapter)!;
                foreach (var machine in machines.Where(machine => definition.MachineIds.Contains(machine.Id)))
                {
                    var initial = (IEmulationConfiguration)engineType.GetMethod("CreateConfiguration")!.Invoke(engine, [machine.Id])!;
                    var configuration = await (ValueTask<IEmulationConfiguration>)engineType.GetMethod("UseEmulatorAsync")!.Invoke(engine, [initial, definition.Id, CancellationToken.None])!;
                    var before = System.Text.Json.JsonSerializer.Serialize(configuration, configurationType);
                    var slots = ((System.Collections.IEnumerable)contract.GetMethod("GetFirmwareSlots")!.Invoke(adapter, [configuration])!).Cast<object>().ToArray();
                    var actual = (bool)required.Invoke(adapter, [configuration])!;
                    if (root == "GWGUI.Emulation.Sony")
                    {
                        var core = implementation.GetProperty("CoreDefinition", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(adapter)!;
                        Assert.Equal(core.GetType().GetProperty("RequiresExternalFirmware")!.GetValue(core), actual);
                        nativeAlternatives++;
                    }
                    else Assert.Equal(slots.Any(slot => (bool)slotType.GetProperty("IsRequired")!.GetValue(slot)!), actual);
                    if (root == "GWGUI.Emulation.Snk")
                    {
                        var model = configurationType.GetProperty("Model")!.GetValue(configuration)!;
                        var roles = ((System.Collections.IEnumerable)implementation.GetMethod("FirmwareRoles", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(adapter, [model])!).Cast<object>().ToArray();
                        Assert.Equal(roles.Select(role => "snk_firmware_" + role), slots.Select(slot => (string)slotType.GetProperty("FieldId")!.GetValue(slot)!));
                        var nativeRequired = implementation.GetMethod("IsFirmwareRequired", BindingFlags.NonPublic | BindingFlags.Instance)!;
                        for (var index = 0; index < roles.Length; index++)
                        {
                            Assert.Equal(nativeRequired.Invoke(adapter, [model, roles[index]]), slotType.GetProperty("IsRequired")!.GetValue(slots[index]));
                            Assert.Equal("Emulation.Snk.Firmware." + roles[index], slotType.GetProperty("LabelResourceKey")!.GetValue(slots[index]));
                        }
                        nativeRoles++;
                    }
                    Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(configuration, configurationType));
                }
            }
        }
        Assert.True(nativeAlternatives > 0);
        Assert.True(nativeRoles >= 16);
    }
}
