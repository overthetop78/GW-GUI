using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GWGUI.App.Contracts.Emulation.Network;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation;
using C = GWGUI.App.Constants.Emulation.EmulationNetworkLayoutConstants;

namespace GWGUI.App.Views.Controls.Emulation.Network;

/// <summary>Tableau de suivi uniquement ; les connexions et leur compatibilité appartiennent à Emulation et aux modules.</summary>
internal sealed class EmulationNetworkSection : UserControl
{
    private readonly DataGrid _table = new()
    {
        IsReadOnly = true,
        AutoGenerateColumns = false,
        CanUserAddRows = false,
        CanUserDeleteRows = false,
        HeadersVisibility = DataGridHeadersVisibility.Column,
        RowHeaderWidth = C.NoMargin,
        FontSize = C.FontSize,
        MinRowHeight = C.RowHeight,
        Margin = new Thickness(C.OuterMargin),
        EnableRowVirtualization = true,
        EnableColumnVirtualization = true
    };
    private readonly TextBlock _description = new()
    {
        TextWrapping = TextWrapping.Wrap,
        FontWeight = FontWeights.Normal,
        Margin = new Thickness(C.OuterMargin, C.OuterMargin, C.OuterMargin, C.NoMargin)
    };
    private bool _subscribed;
    private int _refreshPending;

    internal EmulationNetworkSection()
    {
        Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(C.StylesUri, UriKind.Relative) });
        _table.SetResourceReference(StyleProperty, C.TableStyleResource);
        var root = new DockPanel();
        DockPanel.SetDock(_description, Dock.Top);
        root.Children.Add(_description);
        root.Children.Add(_table);
        Content = root;
        Loaded += (_, _) =>
        {
            if (!_subscribed)
            {
                EmulationSdk.Networks.Changed += RegistryChanged;
                _subscribed = true;
            }
            RefreshRows();
        };
        Unloaded += (_, _) =>
        {
            if (_subscribed) EmulationSdk.Networks.Changed -= RegistryChanged;
            _subscribed = false;
            _table.ItemsSource = null;
        };
        RefreshLocalizedContent();
    }

    internal void RefreshLocalizedContent()
    {
        _description.Text = LocExtension.Get(C.DescriptionResource);
        _table.Columns.Clear();
        _table.Columns.Add(new DataGridCheckBoxColumn
        {
            Header = Header(C.RunningResource, C.RunningWidth),
            Binding = new Binding(nameof(EmulationNetworkRow.IsRunning)) { Mode = BindingMode.OneWay },
            ElementStyle = (Style)Resources[C.StateStyleResource],
            Width = C.RunningWidth
        });
        Column(C.MachineResource, nameof(EmulationNetworkRow.MachineId), C.MachineWidth);
        Column(C.InterfaceResource, nameof(EmulationNetworkRow.Interface), C.InterfaceWidth);
        Column(C.NetworkResource, nameof(EmulationNetworkRow.Network), C.NetworkWidth);
        Column(C.ProtocolResource, nameof(EmulationNetworkRow.Protocol), C.ProtocolWidth);
        Column(C.FormatResource, nameof(EmulationNetworkRow.TransferFormat), C.FormatWidth);
        Column(C.TransportResource, nameof(EmulationNetworkRow.Transport), C.TransportWidth);
        Column(C.IpResource, nameof(EmulationNetworkRow.Ip), C.IpWidth);
        Column(C.PortResource, nameof(EmulationNetworkRow.Port), C.PortWidth);
        Column(C.AddressResource, nameof(EmulationNetworkRow.Address), C.NativeAddressWidth);
        Column(C.RelayResource, nameof(EmulationNetworkRow.Relay), C.RelayWidth);
        Column(C.EndpointResource, nameof(EmulationNetworkRow.HostEndpoint), C.EndpointWidth);
        RefreshRows();
    }

    private void Column(string resource, string property, double width) => _table.Columns.Add(new DataGridTextColumn
    {
        Header = Header(resource, width),
        Binding = new Binding(property) { Mode = BindingMode.OneWay },
        ElementStyle = (Style)Resources[C.TextStyleResource],
        Width = width
    });

    private static TextBlock Header(string resource, double width) => new()
    {
        Text = LocExtension.Get(resource),
        ToolTip = LocExtension.Get(resource),
        TextWrapping = TextWrapping.Wrap,
        MaxWidth = width - C.HeaderAvailableSpace,
        VerticalAlignment = VerticalAlignment.Center
    };

    private void RefreshRows() => _table.ItemsSource = EmulationSdk.Networks.Endpoints.Select(endpoint => new EmulationNetworkRow(endpoint)).ToArray();

    private void RegistryChanged(object? sender, EventArgs args)
    {
        if (Dispatcher.HasShutdownStarted || Interlocked.Exchange(ref _refreshPending, C.RefreshPending) != C.NoRefreshPending) return;
        _ = Dispatcher.InvokeAsync(() =>
        {
            try { if (_subscribed) RefreshRows(); }
            finally { Interlocked.Exchange(ref _refreshPending, C.NoRefreshPending); }
        });
    }
}
