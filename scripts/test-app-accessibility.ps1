param(
    [string]$ApplicationPath,
    [int]$ExpectedTabCount = 5
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ([string]::IsNullOrWhiteSpace($ApplicationPath)) { $ApplicationPath = Join-Path $repository 'artifacts\portable\GW GUI\GW GUI.exe' }
$application = [IO.Path]::GetFullPath($ApplicationPath)
if (-not (Test-Path -LiteralPath $application -PathType Leaf)) { throw "Application not found: $application" }

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

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

    $tabCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::TabItem)
    $tabs = $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCondition)
    if ($tabs.Count -ne $ExpectedTabCount) { throw "Expected $ExpectedTabCount main tabs, found $($tabs.Count)." }

    $audited = @{}
    foreach ($tab in $tabs) {
        if ([string]::IsNullOrWhiteSpace($tab.Current.Name)) { throw 'A main tab has no accessible name.' }
        $selection = $tab.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
        $selection.Select()
        Start-Sleep -Milliseconds 100
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
    }
}
finally {
    if (-not $process.HasExited) {
        $null = $process.CloseMainWindow()
        if (-not $process.WaitForExit(5000)) { Stop-Process -Id $process.Id -Force }
    }
}
