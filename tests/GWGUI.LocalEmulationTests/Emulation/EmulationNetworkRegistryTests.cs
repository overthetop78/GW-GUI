using System.Net;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Services;

namespace GWGUI.Tests.Emulation;

public sealed class EmulationNetworkRegistryTests
{
    private static EmulationNetworkCompatibility NativeCompatible(EmulationNetworkEndpoint _) => EmulationNetworkCompatibility.Compatible;

    [Fact]
    public void ConfigurationsRemainOfflineUntilOwnedByARunningMachine()
    {
        var registry = new EmulationNetworkService();
        var configuration = new Configuration(Guid.NewGuid(), "machine-a");
        var module = new Module();
        registry.SetConfigurations(module, [configuration]);
        var configured = Assert.Single(registry.Endpoints);
        Assert.False(configured.IsRunning);
        IEmulationNetworkRegistration? registration = null;
        try
        {
            registration = registry.Register(configured, NativeCompatible);
            Assert.True(Assert.Single(registry.Endpoints).IsRunning);
            registry.SetConfigurations(module, [configuration]);
            Assert.True(Assert.Single(registry.Endpoints).IsRunning);
        }
        finally { registration?.Dispose(); }
        Assert.False(Assert.Single(registry.Endpoints).IsRunning);
        registry.SetConfigurations(module, []);
        Assert.Empty(registry.Endpoints);
    }

    [Theory]
    [InlineData(EmulationNetworkType.Econet, EmulationNetworkInterfaceType.Econet, EmulationNetworkTransferFormat.AunPacket, EmulationNetworkTransport.Udp, EmulationNetworkCompatibility.NetworkMismatch)]
    [InlineData(EmulationNetworkType.Ethernet, EmulationNetworkInterfaceType.Ethernet, EmulationNetworkTransferFormat.EthernetIIFrame, EmulationNetworkTransport.Tcp, EmulationNetworkCompatibility.TransportMismatch)]
    [InlineData(EmulationNetworkType.Ethernet, EmulationNetworkInterfaceType.Ethernet, EmulationNetworkTransferFormat.PppFrame, EmulationNetworkTransport.Udp, EmulationNetworkCompatibility.FormatMismatch)]
    [InlineData(EmulationNetworkType.Ethernet, EmulationNetworkInterfaceType.Wireless, EmulationNetworkTransferFormat.EthernetIIFrame, EmulationNetworkTransport.Udp, EmulationNetworkCompatibility.InterfaceMismatch)]
    public void ASharedHostTransportDoesNotConvertIncompatibleNetworks(EmulationNetworkType network,
        EmulationNetworkInterfaceType type, EmulationNetworkTransferFormat format, EmulationNetworkTransport transport,
        EmulationNetworkCompatibility expected)
    {
        var registry = new EmulationNetworkService();
        var first = Endpoint(Guid.NewGuid(), "machine-a");
        var second = Endpoint(Guid.NewGuid(), "machine-b") with { Network = network, InterfaceType = type, TransferFormat = format, Transport = transport };
        IEmulationNetworkRegistration? a = null;
        IEmulationNetworkRegistration? b = null;
        try
        {
            a = registry.Register(first, NativeCompatible);
            b = registry.Register(second, NativeCompatible);
            Assert.Equal(expected, registry.Compatibility(first, second));
            Assert.Empty(registry.Peers(first));
        }
        finally { try { a?.Dispose(); } finally { b?.Dispose(); } }
    }

    [Fact]
    public void NativeValidationMustSucceedInBothDirections()
    {
        var registry = new EmulationNetworkService();
        var first = Endpoint(Guid.NewGuid(), "machine-a");
        var second = Endpoint(Guid.NewGuid(), "machine-b");
        IEmulationNetworkRegistration? a = null;
        IEmulationNetworkRegistration? b = null;
        try
        {
            a = registry.Register(first, NativeCompatible);
            b = registry.Register(second, _ => EmulationNetworkCompatibility.NativeParametersMismatch);
            Assert.Equal(EmulationNetworkCompatibility.NativeParametersMismatch, registry.Compatibility(first, second));
            Assert.Empty(registry.Peers(first));
            Assert.Empty(registry.Peers(second));
        }
        finally { try { a?.Dispose(); } finally { b?.Dispose(); } }
    }

    [Fact]
    public void MachineIdentityIsNotANetworkCompatibilityRuleAndAddressesStayStructured()
    {
        var registry = new EmulationNetworkService();
        var first = Endpoint(Guid.NewGuid(), "PC-Ryzen-5700X") with { IpAddress = IPAddress.IPv6Loopback, Port = 32768 };
        var second = Endpoint(Guid.NewGuid(), "PS5") with { IpAddress = IPAddress.IPv6Loopback, Port = 32769 };
        IEmulationNetworkRegistration? a = null;
        IEmulationNetworkRegistration? b = null;
        try
        {
            a = registry.Register(first, NativeCompatible);
            b = registry.Register(second, NativeCompatible);
            Assert.Equal(EmulationNetworkCompatibility.Compatible, registry.Compatibility(first, second));
            var peer = Assert.Single(registry.Peers(first));
            Assert.Equal(IPAddress.IPv6Loopback, peer.IpAddress);
            Assert.Equal(32769, peer.Port);
            b.Update(second with { Port = 40000 });
            Assert.Equal(40000, Assert.Single(registry.Peers(first)).Port);
        }
        finally { try { a?.Dispose(); } finally { b?.Dispose(); } }
        Assert.Empty(registry.Endpoints);
    }

    [Fact]
    public void NonIpInterfacesDoNotNeedFakeIpAddressesOrPorts()
    {
        var registry = new EmulationNetworkService();
        var first = Endpoint(Guid.NewGuid(), "CPC") with
        {
            InterfaceType = EmulationNetworkInterfaceType.SerialPort,
            Network = EmulationNetworkType.SerialNetwork,
            Protocols = [EmulationNetworkProtocol.Ppp],
            TransferFormat = EmulationNetworkTransferFormat.SerialBitStream,
            Transport = EmulationNetworkTransport.InProcess,
            IpAddress = null,
            Port = null,
            HostEndpoint = "serial-a"
        };
        var second = first with { ConfigurationId = Guid.NewGuid(), MachineId = "ST", HostEndpoint = "serial-b" };
        IEmulationNetworkRegistration? a = null;
        IEmulationNetworkRegistration? b = null;
        try
        {
            a = registry.Register(first, NativeCompatible);
            b = registry.Register(second, NativeCompatible);
            var peer = Assert.Single(registry.Peers(first));
            Assert.Null(peer.IpAddress);
            Assert.Null(peer.Port);
            Assert.Equal("serial-b", peer.HostEndpoint);
        }
        finally { try { a?.Dispose(); } finally { b?.Dispose(); } }
    }

    [Fact]
    public void SeveralInterfacesOfTheSameRouterAreNotAutomaticallyConnectedToEachOther()
    {
        var registry = new EmulationNetworkService();
        var first = Endpoint(Guid.NewGuid(), "Cisco");
        var second = first with { InterfaceId = "ethernet-b" };
        IEmulationNetworkRegistration? a = null;
        IEmulationNetworkRegistration? b = null;
        try
        {
            a = registry.Register(first, NativeCompatible);
            b = registry.Register(second, NativeCompatible);
            Assert.Equal(2, registry.Endpoints.Count);
            Assert.Equal(EmulationNetworkCompatibility.SameMachine, registry.Compatibility(first, second));
            Assert.Empty(registry.Peers(first));
            a.Dispose();
            Assert.True(Assert.Single(registry.Endpoints).IsRunning);
        }
        finally { try { a?.Dispose(); } finally { b?.Dispose(); } }
    }

    [Fact]
    public void RuntimeOwnershipIsReleasedAfterObserverErrorsAndCannotBeRevivedByOldOwners()
    {
        var registry = new EmulationNetworkService();
        var endpoint = Endpoint(Guid.NewGuid(), "machine-a");
        EventHandler failing = (_, _) => throw new InvalidOperationException();
        registry.Changed += failing;
        Assert.Throws<InvalidOperationException>(() => registry.Register(endpoint, NativeCompatible));
        Assert.Empty(registry.Endpoints);
        registry.Changed -= failing;
        var old = registry.Register(endpoint, NativeCompatible);
        try { Assert.Throws<InvalidOperationException>(() => registry.Register(endpoint, NativeCompatible)); }
        finally { old.Dispose(); }
        var current = registry.Register(endpoint, NativeCompatible);
        try
        {
            old.Dispose();
            Assert.True(Assert.Single(registry.Endpoints).IsRunning);
            Assert.Throws<ObjectDisposedException>(() => old.Update(endpoint));
        }
        finally { current.Dispose(); }
        Assert.Empty(registry.Endpoints);
    }

    private static EmulationNetworkEndpoint Endpoint(Guid id, string machine) => new("test", id, machine, "ethernet-a",
        EmulationNetworkInterfaceType.Ethernet, EmulationNetworkType.Ethernet, [EmulationNetworkProtocol.Ipv4, EmulationNetworkProtocol.Ipv6],
        EmulationNetworkTransferFormat.EthernetIIFrame, EmulationNetworkTransport.Udp, IPAddress.Loopback, 32768);

    private sealed record Configuration(Guid Id, string MachineId) : IEmulationConfiguration
    {
        public string ModuleId => "test";
    }

    private sealed class Module : IEmulationModule, IEmulationNetworkManager
    {
        public string Id => "test";
        public string DisplayResourceKey => string.Empty;
        public string BrandImagePath => string.Empty;
        public IReadOnlyList<EmulationMachineDefinition> Machines => [];
        public EmulationSettingsVisibility DefaultVisibility => throw new NotSupportedException();
        public IReadOnlyList<EmulationNetworkEndpoint> DescribeNetworks(IEmulationConfiguration configuration) => [Endpoint(configuration.Id, configuration.MachineId)];
        public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode) { exitCode = default; return false; }
        public EmulationMachineSettings Describe(string machineId, IEmulationConfiguration? configuration = null) => throw new NotSupportedException();
        public IEmulationConfiguration CreateConfiguration(string machineId) => throw new NotSupportedException();
        public IEmulationConfiguration ChangeMachine(IEmulationConfiguration configuration, string machineId) => throw new NotSupportedException();
        public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration, IReadOnlyDictionary<string, string?> values) => throw new NotSupportedException();
        public IReadOnlyDictionary<string, string> RuntimeOptions(IEmulationConfiguration configuration) => throw new NotSupportedException();
        public EmulationConfigurationSummary SummarizeConfiguration(IEmulationConfiguration configuration) => throw new NotSupportedException();
        public ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(IEmulationConfiguration configuration, EmulationRuntimeServices services, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask DeleteConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
