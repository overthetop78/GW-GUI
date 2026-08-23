param(
    [string]$ApplicationPath = (Join-Path $PSScriptRoot '..\build\Debug\GW GUI\gwgui.exe'),
    [string]$TargetDeviceSearchText = 'Manette Super Nintendo',
    [string]$ScreenshotPath = (Join-Path $PSScriptRoot '..\tmp\captures\gwgui-debug-controllers-layout-20260823.png')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
Add-Type @'
using System.Runtime.InteropServices;
public static class ControllerAuditMouse {
    [DllImport("user32.dll")] public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, System.UIntPtr extra);
}
'@

function Find-ProcessElement([int]$ProcessId, [string]$AutomationId, [System.Windows.Automation.ControlType]$ControlType) {
    $processCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $ProcessId)
    $idCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::AutomationIdProperty, $AutomationId)
    $typeCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, $ControlType)
    $condition = New-Object System.Windows.Automation.AndCondition($processCondition, $idCondition, $typeCondition)
    [System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

function Find-NamedDescendant([System.Windows.Automation.AutomationElement]$Root, [string]$Name, [System.Windows.Automation.ControlType]$ControlType) {
    $nameCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty, $Name)
    $typeCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, $ControlType)
    $condition = New-Object System.Windows.Automation.AndCondition($nameCondition, $typeCondition)
    $Root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

function Invoke-Element([System.Windows.Automation.AutomationElement]$Element) {
    $pattern = $null
    if ($Element.TryGetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern, [ref]$pattern)) {
        $pattern.Expand()
        return
    }
    if ($Element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$pattern)) {
        $pattern.Invoke()
        return
    }
    if ($Element.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$pattern)) {
        $pattern.Select()
        return
    }
    throw "Aucun mécanisme d'action pris en charge pour $($Element.Current.AutomationId)."
}

function Select-Element([System.Windows.Automation.AutomationElement]$Element) {
    $Element.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
}

$application = [IO.Path]::GetFullPath($ApplicationPath)
$capture = [IO.Path]::GetFullPath($ScreenshotPath)
$process = Start-Process -FilePath $application -PassThru
try {
    $main = $null
    for ($attempt = 0; $attempt -lt 80 -and $null -eq $main; $attempt++) {
        Start-Sleep -Milliseconds 100
        $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $process.Id)
        $main = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Children, $condition)
    }
    if ($null -eq $main) { throw 'Fenêtre initiale introuvable.' }

    Start-Sleep -Seconds 3
    $continue = Find-ProcessElement $process.Id 'ContinueButton' ([System.Windows.Automation.ControlType]::Button)
    if ($null -ne $continue) { Invoke-Element $continue; Start-Sleep -Milliseconds 300 }

    $main = $null
    $mainTabsCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'MainTabs')
    for ($attempt = 0; $attempt -lt 80 -and $null -eq $main; $attempt++) {
        Start-Sleep -Milliseconds 100
        $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
        foreach ($window in $windows) {
            if ($null -ne $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $mainTabsCondition)) { $main = $window; break }
        }
    }
    if ($null -eq $main) { throw 'Fenêtre principale introuvable.' }

    $optionsMenu = Find-ProcessElement $process.Id 'Options' ([System.Windows.Automation.ControlType]::MenuItem)
    if ($null -eq $optionsMenu) { throw 'Menu Options introuvable.' }
    $menuBounds = $optionsMenu.Current.BoundingRectangle
    [System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point ([int]($menuBounds.Left + $menuBounds.Width / 2)), ([int]($menuBounds.Top + $menuBounds.Height / 2))
    [ControllerAuditMouse]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
    [ControllerAuditMouse]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 150
    $menuTypeCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::MenuItem)
    $allMenuItems = [System.Windows.Automation.AutomationElement]::RootElement.FindAll([System.Windows.Automation.TreeScope]::Descendants, $menuTypeCondition)
    foreach ($menuItem in $allMenuItems) { "MENU|$($menuItem.Current.ProcessId)|$($menuItem.Current.AutomationId)|$($menuItem.Current.Name)" }
    $preferences = Find-ProcessElement $process.Id 'PreferencesMenuItem' ([System.Windows.Automation.ControlType]::MenuItem)
    if ($null -eq $preferences) { throw 'Commande Préférences introuvable.' }

    $watch = [Diagnostics.Stopwatch]::StartNew()
    Invoke-Element $preferences
    $options = $null
    for ($attempt = 0; $attempt -lt 100 -and $null -eq $options; $attempt++) {
        Start-Sleep -Milliseconds 50
        $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $process.Id)
        $elements = [System.Windows.Automation.AutomationElement]::RootElement.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
        $largestArea = 0
        foreach ($window in $elements) {
            if ($window.Current.ControlType -ne [System.Windows.Automation.ControlType]::Window) { continue }
            $bounds = $window.Current.BoundingRectangle
            $area = $bounds.Width * $bounds.Height
            if ($window.Current.NativeWindowHandle -ne $main.Current.NativeWindowHandle -and $area -gt $largestArea) {
                $options = $window
                $largestArea = $area
            }
        }
    }
    if ($null -eq $options) { throw 'Fenêtre Options introuvable.' }
    $watch.Stop()
    "OptionsOpeningMilliseconds=$($watch.ElapsedMilliseconds)"
    "OptionsWindow=$($options.Current.Name)|$($options.Current.AutomationId)|$($options.Current.BoundingRectangle)"

    $tabCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::TabItem)
    $tabs = $options.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCondition)
    foreach ($tab in $tabs) { "TAB|$($tab.Current.Name)|$($tab.Current.AutomationId)|$($tab.Current.IsOffscreen)|$($tab.Current.BoundingRectangle)" }

    if ($tabs.Count -lt 1) { throw 'Aucun onglet Options trouvé.' }
    $controllers = $tabs[$tabs.Count - 1]
    Select-Element $controllers
    Start-Sleep -Seconds 5

    $windowBounds = $options.Current.BoundingRectangle
    [System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point ([int]($windowBounds.Left + 500)), ([int]($windowBounds.Top + 247))
    [ControllerAuditMouse]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
    [ControllerAuditMouse]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 150
    [System.Windows.Forms.SendKeys]::SendWait($TargetDeviceSearchText)
    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
    Start-Sleep -Milliseconds 700

    $comboCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::ComboBox)
    $combos = $options.FindAll([System.Windows.Automation.TreeScope]::Descendants, $comboCondition)
    $deviceSelector = $null
    $top = [double]::PositiveInfinity
    foreach ($combo in $combos) {
        if (-not $combo.Current.IsOffscreen -and $combo.Current.BoundingRectangle.Top -lt $top) {
            $deviceSelector = $combo
            $top = $combo.Current.BoundingRectangle.Top
        }
    }
    if ($null -ne $deviceSelector) {
        $deviceSelector.SetFocus()
        [System.Windows.Forms.SendKeys]::SendWait('%{DOWN}')
        Start-Sleep -Milliseconds 150
        [System.Windows.Forms.SendKeys]::SendWait($TargetDeviceSearchText)
        [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
        Start-Sleep -Milliseconds 700
        "SelectedDevice=$($deviceSelector.Current.Name)"
    }

    $sliderCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Slider)
    $sliders = $options.FindAll([System.Windows.Automation.TreeScope]::Descendants, $sliderCondition)
    "RumbleSlider=$($sliders.Count -gt 0)"
    $buttonCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Button)
    $buttons = $options.FindAll([System.Windows.Automation.TreeScope]::Descendants, $buttonCondition)
    foreach ($button in $buttons) {
        if ($button.Current.Name -like 'Moteur *' -or $button.Current.Name -like '*gâchette*') {
            "RumbleMotor=$($button.Current.Name)|Enabled=$($button.Current.IsEnabled)"
        }
    }
    $bounds = $options.Current.BoundingRectangle
    $bitmap = New-Object System.Drawing.Bitmap ([int]$bounds.Width), ([int]$bounds.Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.CopyFromScreen([int]$bounds.Left, [int]$bounds.Top, 0, 0, $bitmap.Size)
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($capture)) | Out-Null
    $bitmap.Save($capture, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bitmap.Dispose()
    "Screenshot=$capture"
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        $null = $process.CloseMainWindow()
        if (-not $process.WaitForExit(5000)) { Stop-Process -Id $process.Id -Force }
    }
}
