using System.Net;

namespace GWGUI.Emulation.Services;

/// <summary>État en mémoire partagé entre modules. Ne possède ni socket, ni processus, ni thread, ni fenêtre.</summary>
internal sealed class EmulationNetworkService : IEmulationNetworks
{
    private readonly object _gate = new();
    private readonly Dictionary<Identity, EmulationNetworkEndpoint> _configured = [];
    private readonly Dictionary<Identity, ActiveInterface> _active = [];
    private IReadOnlyList<EmulationNetworkEndpoint> _endpoints = Array.Empty<EmulationNetworkEndpoint>();
    private long _version;

    public long Version => Interlocked.Read(ref _version);
    public IReadOnlyList<EmulationNetworkEndpoint> Endpoints => Volatile.Read(ref _endpoints);
    public event EventHandler? Changed;

    public void SetConfigurations(IEmulationModule module, IReadOnlyList<IEmulationConfiguration> configurations)
    {
        var descriptions = module is IEmulationNetworkManager manager
            ? configurations.SelectMany(manager.DescribeNetworks).Select(Freeze).ToArray() : [];
        if (descriptions.Any(endpoint => endpoint.ModuleId != module.Id)) throw new ArgumentException(nameof(module));
        var replacement = descriptions.ToDictionary(Identity.Of, endpoint => endpoint with { IsRunning = false });
        lock (_gate)
        {
            foreach (var key in _configured.Keys.Where(key => key.Module == module.Id).ToArray()) _configured.Remove(key);
            foreach (var item in replacement) _configured.Add(item.Key, item.Value);
            Publish();
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public IEmulationNetworkRegistration Register(EmulationNetworkEndpoint endpoint,
        Func<EmulationNetworkEndpoint, EmulationNetworkCompatibility> nativeRule)
    {
        ArgumentNullException.ThrowIfNull(nativeRule);
        var frozen = Freeze(endpoint) with { IsRunning = true };
        var identity = Identity.Of(frozen);
        var token = Guid.NewGuid();
        lock (_gate)
        {
            if (_active.ContainsKey(identity)) throw new InvalidOperationException(EmulationNetworkConstants.DuplicateInterfaceMessage);
            _active.Add(identity, new(token, frozen, nativeRule));
            Publish();
        }
        try
        {
            Changed?.Invoke(this, EventArgs.Empty);
            return new Registration(this, identity, token);
        }
        catch
        {
            Remove(identity, token, notify: false);
            throw;
        }
    }

    public EmulationNetworkCompatibility Compatibility(EmulationNetworkEndpoint first, EmulationNetworkEndpoint second)
    {
        if (first.ModuleId == second.ModuleId && first.ConfigurationId == second.ConfigurationId)
            return EmulationNetworkCompatibility.SameMachine;
        ActiveInterface? local;
        ActiveInterface? remote;
        lock (_gate)
        {
            _active.TryGetValue(Identity.Of(first), out local);
            _active.TryGetValue(Identity.Of(second), out remote);
        }
        if (local is null || remote is null) return EmulationNetworkCompatibility.MachineOffline;
        first = local.Endpoint;
        second = remote.Endpoint;
        if (first.InterfaceType == EmulationNetworkInterfaceType.None || second.InterfaceType == EmulationNetworkInterfaceType.None
            || first.Network == EmulationNetworkType.None || second.Network == EmulationNetworkType.None
            || first.TransferFormat == EmulationNetworkTransferFormat.None || second.TransferFormat == EmulationNetworkTransferFormat.None
            || first.Transport == EmulationNetworkTransport.None || second.Transport == EmulationNetworkTransport.None)
            return EmulationNetworkCompatibility.InterfaceDisconnected;
        if (first.Network != second.Network) return EmulationNetworkCompatibility.NetworkMismatch;
        if (first.InterfaceType != second.InterfaceType) return EmulationNetworkCompatibility.InterfaceMismatch;
        if (first.TransferFormat != second.TransferFormat) return EmulationNetworkCompatibility.FormatMismatch;
        if (first.Transport != second.Transport) return EmulationNetworkCompatibility.TransportMismatch;
        var result = local.Rule(second);
        return result == EmulationNetworkCompatibility.Compatible
            ? remote.Rule(first) : result;
    }

    public IReadOnlyList<EmulationNetworkEndpoint> Peers(EmulationNetworkEndpoint endpoint) => Endpoints
        .Where(peer => peer.IsRunning && Compatibility(endpoint, peer) == EmulationNetworkCompatibility.Compatible).ToArray();

    private static EmulationNetworkEndpoint Freeze(EmulationNetworkEndpoint endpoint)
    {
        if (endpoint.Port is < EmulationNetworkConstants.MinimumPort or > EmulationNetworkConstants.MaximumPort)
            throw new ArgumentOutOfRangeException(nameof(endpoint.Port));
        return endpoint with
        {
            IpAddress = endpoint.IpAddress is null ? null : IPAddress.Parse(endpoint.IpAddress.ToString()),
            Protocols = Array.AsReadOnly(endpoint.Protocols.ToArray()),
            Addresses = Array.AsReadOnly(endpoint.Addresses?.ToArray() ?? []),
            Relays = Array.AsReadOnly(endpoint.Relays?.ToArray() ?? [])
        };
    }

    private void Publish()
    {
        var rows = new Dictionary<Identity, EmulationNetworkEndpoint>(_configured);
        foreach (var active in _active) rows[active.Key] = active.Value.Endpoint;
        Volatile.Write(ref _endpoints, Array.AsReadOnly(rows.Values.OrderBy(item => item.ModuleId, StringComparer.Ordinal)
            .ThenBy(item => item.MachineId, StringComparer.Ordinal).ThenBy(item => item.InterfaceId, StringComparer.Ordinal).ToArray()));
        Interlocked.Increment(ref _version);
    }

    private void Update(Identity identity, Guid token, EmulationNetworkEndpoint endpoint)
    {
        if (Identity.Of(endpoint) != identity) throw new ArgumentException(nameof(endpoint));
        var frozen = Freeze(endpoint) with { IsRunning = true };
        lock (_gate)
        {
            if (!_active.TryGetValue(identity, out var active) || active.Token != token) throw new ObjectDisposedException(nameof(Registration));
            _active[identity] = active with { Endpoint = frozen };
            Publish();
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void Remove(Identity identity, Guid token, bool notify = true)
    {
        lock (_gate)
        {
            if (!_active.TryGetValue(identity, out var active) || active.Token != token) return;
            _active.Remove(identity);
            Publish();
        }
        if (notify) Changed?.Invoke(this, EventArgs.Empty);
    }

    private readonly record struct Identity(string Module, Guid Configuration, string Interface)
    {
        internal static Identity Of(EmulationNetworkEndpoint endpoint) => new(endpoint.ModuleId, endpoint.ConfigurationId, endpoint.InterfaceId);
    }

    private sealed record ActiveInterface(Guid Token, EmulationNetworkEndpoint Endpoint,
        Func<EmulationNetworkEndpoint, EmulationNetworkCompatibility> Rule);

    private sealed class Registration(EmulationNetworkService owner, Identity identity, Guid token) : IEmulationNetworkRegistration
    {
        private EmulationNetworkService? _owner = owner;

        public void Update(EmulationNetworkEndpoint endpoint) =>
            (Volatile.Read(ref _owner) ?? throw new ObjectDisposedException(nameof(Registration))).Update(identity, token, endpoint);

        public void Dispose() => Interlocked.Exchange(ref _owner, null)?.Remove(identity, token);
    }
}
