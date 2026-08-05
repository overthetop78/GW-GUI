param(
    [string]$ApplicationPath,
    [int]$ExpectedTabCount = 5,
    [int]$MinimumLogicalWidth = 1280,
    [int]$MinimumLogicalHeight = 720
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ([string]::IsNullOrWhiteSpace($ApplicationPath)) { $ApplicationPath = Join-Path $repository 'artifacts\portable\GW GUI\GW GUI.exe' }
$application = [IO.Path]::GetFullPath($ApplicationPath)
if (-not (Test-Path -LiteralPath $application -PathType Leaf)) { throw "Application not found: $application" }

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class GwGuiWindowAudit {
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll", SetLastError=true)] public static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
}
'@

$interactiveTypes = @(
    'ControlType.Button', 'ControlType.Edit', 'ControlType.ComboBox', 'ControlType.TabItem',
    'ControlType.CheckBox', 'ControlType.RadioButton', 'ControlType.MenuItem', 'ControlType.TreeItem',
    'ControlType.Slider', 'ControlType.Hyperlink'
)
$process = Start-Process -FilePath $application -PassThru
try {
    $root = $null
    for ($attempt = 0; $attempt -lt 40 -and $null -eq $root; $attempt++) {
        Start-Sleep -Milliseconds 250
        $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $process.Id)
        $root = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Children, $condition)
    }
    if ($null -eq $root) { throw 'The GW GUI main window was not exposed through UI Automation.' }
    if ([string]::IsNullOrWhiteSpace($root.Current.Name)) { throw 'The main window has no accessible name.' }

    $process.Refresh()
    $dpi = [GwGuiWindowAudit]::GetDpiForWindow($process.MainWindowHandle)
    if ($dpi -eq 0) { $dpi = 96 }
    $expectedWidth = [int][Math]::Round($MinimumLogicalWidth * $dpi / 96)
    $expectedHeight = [int][Math]::Round($MinimumLogicalHeight * $dpi / 96)
    if (-not [GwGuiWindowAudit]::SetWindowPos($process.MainWindowHandle, [IntPtr]::Zero, 20, 20, $expectedWidth, $expectedHeight, 0x0040)) {
        throw 'The main window could not be resized for the minimum-size audit.'
    }
    Start-Sleep -Milliseconds 300
    $bounds = $root.Current.BoundingRectangle
    if ([Math]::Abs($bounds.Width - $expectedWidth) -gt 8 -or [Math]::Abs($bounds.Height - $expectedHeight) -gt 8) {
        throw "The minimum window is $([Math]::Round($bounds.Width))x$([Math]::Round($bounds.Height)) physical pixels at $dpi DPI; expected ${expectedWidth}x${expectedHeight}."
    }

    $tabCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::TabItem)
    $tabs = $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCondition)
    if ($tabs.Count -ne $ExpectedTabCount) { throw "Expected $ExpectedTabCount main tabs, found $($tabs.Count)." }

    $audited = @{}
    $primaryActions = @('ReadExecuteButton', 'WriteExecuteButton', 'ConvertExecuteButton', $null, 'EraseExecuteButton')
    $visiblePrimaryActions = 0
    for ($tabIndex = 0; $tabIndex -lt $tabs.Count; $tabIndex++) {
        $tab = $tabs[$tabIndex]
        if ([string]::IsNullOrWhiteSpace($tab.Current.Name)) { throw 'A main tab has no accessible name.' }
        $selection = $tab.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
        $selection.Select()
        Start-Sleep -Milliseconds 100
        $actionId = $primaryActions[$tabIndex]
        if ($null -ne $actionId) {
            $actionCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $actionId)
            $action = $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $actionCondition)
            if ($null -eq $action -or $action.Current.IsOffscreen) { throw "$actionId is not visible at the minimum window size." }
            if ($null -eq $action.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)) { throw "$actionId is not invocable." }
            $visiblePrimaryActions++
            if ($tabIndex -eq 4) {
                $toolsCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'ToolsList')
                $tools = $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $toolsCondition)
                $itemCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::ListItem)
                $toolItems = $tools.FindAll([System.Windows.Automation.TreeScope]::Children, $itemCondition)
                if ($toolItems.Count -ne 2) { throw "Expected two maintenance tools, found $($toolItems.Count)." }
                $toolItems[1].GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
                Start-Sleep -Milliseconds 100
                $cleanCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'CleanExecuteButton')
                $clean = $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $cleanCondition)
                if ($null -eq $clean -or $clean.Current.IsOffscreen) { throw 'CleanExecuteButton is not visible at the minimum window size.' }
                if ($null -eq $clean.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)) { throw 'CleanExecuteButton is not invocable.' }
                $visiblePrimaryActions++
            }
        }
        $elements = $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
        foreach ($element in $elements) {
            $type = $element.Current.ControlType.ProgrammaticName
            if ($type -notin $interactiveTypes -or $element.Current.IsOffscreen) { continue }
            if ($element.Current.AutomationId -in @('PageUp', 'PageDown', 'PART_LineUpButton', 'PART_LineDownButton')) { continue }
            $identity = "$type|$($element.Current.AutomationId)|$($element.Current.BoundingRectangle)"
            $audited[$identity] = [pscustomobject]@{ Type=$type; Name=$element.Current.Name; AutomationId=$element.Current.AutomationId; Tab=$tab.Current.Name }
        }
    }

    $missing = @($audited.Values | Where-Object { [string]::IsNullOrWhiteSpace($_.Name) })
    if ($missing.Count -gt 0) {
        $missing | Sort-Object Tab,Type,AutomationId | Format-Table -AutoSize
        throw "$($missing.Count) visible interactive control(s) have no accessible name."
    }

    [pscustomobject]@{
        Window = $root.Current.Name
        Tabs = $tabs.Count
        InteractiveControls = $audited.Count
        MissingNames = $missing.Count
        MinimumLogicalSize = "${MinimumLogicalWidth}x${MinimumLogicalHeight}"
        Dpi = $dpi
        VisiblePrimaryActions = $visiblePrimaryActions
    }
}
finally {
    if (-not $process.HasExited) {
        $null = $process.CloseMainWindow()
        if (-not $process.WaitForExit(5000)) { Stop-Process -Id $process.Id -Force }
    }
}
