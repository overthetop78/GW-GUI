param(
    [string]$SetupPath,
    [string]$ExpectedVersion = '0.1.0',
    [ValidateSet('english', 'french')][string]$InstallerLanguage = 'english',
    [string]$InstallDirectory
)

$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$artifacts = [IO.Path]::GetFullPath((Join-Path $repository 'artifacts'))
$artifactsPrefix = $artifacts + [IO.Path]::DirectorySeparatorChar
if ([string]::IsNullOrWhiteSpace($SetupPath)) { $SetupPath = Join-Path $artifacts "GW-GUI-$ExpectedVersion-win-x64-setup.exe" }
if ([string]::IsNullOrWhiteSpace($InstallDirectory)) { $InstallDirectory = Join-Path $artifacts "installer-interactive-$InstallerLanguage" }
$setup = [IO.Path]::GetFullPath($SetupPath)
$destination = [IO.Path]::GetFullPath($InstallDirectory)
$uninstallRegistryPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{7B909A70-92B3-48E5-82CB-51A584ECE231}_is1'
if (-not $destination.StartsWith($artifactsPrefix, [StringComparison]::OrdinalIgnoreCase)) { throw 'InstallDirectory must be inside artifacts.' }
if (-not (Test-Path -LiteralPath $setup -PathType Leaf)) { throw "Installer not found: $setup" }
if (Test-Path -LiteralPath $destination) { throw "Interactive-test destination already exists: $destination" }
if (Test-Path -LiteralPath $uninstallRegistryPath) { throw 'An installed GW GUI registration already exists.' }
if (Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -like 'GW-GUI-*-setup*' }) { throw 'Another GW GUI installer process is already running.' }

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
$shell = New-Object -ComObject WScript.Shell
$labels = if ($InstallerLanguage -eq 'french') {
    @{
        License='Accord de licence'
        Destination='Dossier de destination'
        Tasks=('T' + [char]0x00e2 + 'ches suppl' + [char]0x00e9 + 'mentaires')
        Ready=('Pr' + [char]0x00ea + 't ' + [char]0x00e0 + ' installer')
        Finish='Terminer'
        NextKeys='%s'
        InstallKeys='%i'
    }
} else {
    @{ License='License Agreement'; Destination='Select Destination Location'; Tasks='Select Additional Tasks'; Ready='Ready to Install'; Finish='Finish'; NextKeys='%n'; InstallKeys='%i' }
}

function Get-InstallerProcess {
    Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -like 'GW-GUI-*-setup.tmp' -and $_.MainWindowTitle } | Select-Object -First 1
}
function Get-WindowNames([Diagnostics.Process]$Process) {
    $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $Process.Id)
    $root = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Children, $condition)
    if (-not $root) { return @() }
    @($root.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name } | Where-Object { $_ })
}
function Wait-ForPage([Diagnostics.Process]$Process, [string]$ExpectedName, [int]$Seconds = 15) {
    $deadline = (Get-Date).AddSeconds($Seconds)
    do {
        Start-Sleep -Milliseconds 200
        if ($Process.HasExited) { throw "Installer exited before page '$ExpectedName'." }
        $names = Get-WindowNames $Process
        if ($names -contains $ExpectedName) { return $names }
    } while ((Get-Date) -lt $deadline)
    throw "Installer page '$ExpectedName' did not appear."
}
function Send-InstallerKeys([Diagnostics.Process]$Process, [string]$Keys) {
    if (-not $shell.AppActivate($Process.Id)) { throw 'Could not activate the installer window.' }
    $shell.SendKeys($Keys)
    Start-Sleep -Milliseconds 250
}

$bootstrap = $null
$installer = $null
try {
    $bootstrap = Start-Process -FilePath $setup -ArgumentList @("/LANG=$InstallerLanguage", '/NOICONS', '/NORESTART', "/DIR=`"$destination`"") -PassThru
    $deadline = (Get-Date).AddSeconds(15)
    do { Start-Sleep -Milliseconds 200; $installer = Get-InstallerProcess } while (-not $installer -and (Get-Date) -lt $deadline)
    if (-not $installer) { throw 'Interactive installer window did not appear.' }

    $null = Wait-ForPage $installer $labels.License
    Send-InstallerKeys $installer '{TAB}'
    Send-InstallerKeys $installer '{UP}'
    Send-InstallerKeys $installer '{TAB}'
    Send-InstallerKeys $installer '{ENTER}'
    $null = Wait-ForPage $installer $labels.Destination
    Send-InstallerKeys $installer $labels.NextKeys
    $null = Wait-ForPage $installer $labels.Tasks
    Send-InstallerKeys $installer $labels.NextKeys
    $null = Wait-ForPage $installer $labels.Ready
    Send-InstallerKeys $installer $labels.InstallKeys
    $null = Wait-ForPage $installer $labels.Finish 90
    Send-InstallerKeys $installer '{ENTER}'
    if (-not $installer.WaitForExit(15000)) { throw 'Installer did not close from its final page.' }

    $registration = Get-ItemProperty -LiteralPath $uninstallRegistryPath
    if ($registration.DisplayVersion -ne $ExpectedVersion) { throw "Registered version is $($registration.DisplayVersion)." }
    if ($registration.'Inno Setup: Language' -ne $InstallerLanguage) { throw "Registered language is $($registration.'Inno Setup: Language')." }
    $productVersion = (Get-Item -LiteralPath (Join-Path $destination 'GW GUI.exe')).VersionInfo.ProductVersion
    if (-not $productVersion.StartsWith($ExpectedVersion, [StringComparison]::Ordinal)) { throw "Product version is $productVersion." }
    Start-Sleep -Milliseconds 500
    $launched = @(Get-Process -ErrorAction SilentlyContinue | Where-Object { try { $_.Path -and $_.Path.StartsWith($destination, [StringComparison]::OrdinalIgnoreCase) } catch { $false } })
    if ($launched.Count) { throw 'The unchecked final-page option still launched GW GUI.' }

    [pscustomobject]@{ InstallerLanguage=$InstallerLanguage; RegisteredVersion=$registration.DisplayVersion; ProductVersion=$productVersion; PagesVerified=5 }
}
finally {
    foreach ($process in @($installer, $bootstrap)) {
        if (-not $process) { continue }
        try {
            if (-not $process.HasExited) { $null = $process.CloseMainWindow(); if (-not $process.WaitForExit(3000)) { $process.Kill() } }
        }
        catch [InvalidOperationException] { }
    }
    $appProcesses = @(Get-Process -ErrorAction SilentlyContinue | Where-Object { try { $_.Path -and $_.Path.StartsWith($destination, [StringComparison]::OrdinalIgnoreCase) } catch { $false } })
    foreach ($process in $appProcesses) {
        try { $null = $process.CloseMainWindow(); if (-not $process.WaitForExit(3000)) { $process.Kill() } }
        catch [InvalidOperationException] { }
    }
    $uninstaller = Join-Path $destination 'unins000.exe'
    if (Test-Path -LiteralPath $uninstaller -PathType Leaf) {
        $uninstall = Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART') -Wait -PassThru
        if ($uninstall.ExitCode -ne 0) { throw "Uninstaller exited with code $($uninstall.ExitCode)." }
    }
    if (Test-Path -LiteralPath $uninstallRegistryPath) { throw 'The uninstall registration remained after cleanup.' }
    if (Test-Path -LiteralPath $destination) {
        $remaining = @(Get-ChildItem -LiteralPath $destination -Force)
        if ($remaining.Count -ne 0) { throw "Uninstaller left $($remaining.Count) item(s) in $destination." }
        Remove-Item -LiteralPath $destination -Force
    }
}

Write-Output 'Interactive installer validation passed and all isolated state was removed.'
