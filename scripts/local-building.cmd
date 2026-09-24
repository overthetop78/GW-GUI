@echo off
setlocal EnableExtensions EnableDelayedExpansion

pushd "%~dp0.." || exit /b 1

set "BUILD_CONFIGURATION="
set "MODULES_INPUT="
set "MODULES_GIVEN="

:parse_arguments
if "%~1"=="" goto arguments_parsed
set "ARGUMENT=%~1"
if /i "!ARGUMENT!"=="--building" (
    if defined BUILD_CONFIGURATION goto duplicate_building
    goto read_building_value
)
if /i "!ARGUMENT!"=="--modules" (
    if defined MODULES_GIVEN goto duplicate_modules
    set "MODULES_GIVEN=1"
    set "READING_MODULES=1"
    shift
    goto parse_arguments
)
if /i "!ARGUMENT:~0,11!"=="--building=" (
    if defined BUILD_CONFIGURATION goto duplicate_building
    set "BUILD_CONFIGURATION=!ARGUMENT:~11!"
    set "READING_MODULES="
) else if /i "!ARGUMENT:~0,10!"=="--modules=" (
    if defined MODULES_GIVEN goto duplicate_modules
    set "MODULES_GIVEN=1"
    set "MODULES_INPUT=!ARGUMENT:~10!"
    set "READING_MODULES="
) else if defined READING_MODULES (
    if defined MODULES_INPUT (
        set "MODULES_INPUT=!MODULES_INPUT!,!ARGUMENT!"
    ) else (
        set "MODULES_INPUT=!ARGUMENT!"
    )
) else (
    echo Erreur : parametre inconnu : %~1
    echo Utilisation : scripts\local-building.cmd [--building=debug^|release] [--modules=A^|0^|id1,id2^|numero1,numero2]
    popd
    exit /b 1
)
shift
goto parse_arguments

:read_building_value
shift
if "%~1"=="" goto missing_building_value
set "BUILD_CONFIGURATION=%~1"
set "READING_MODULES="
shift
goto parse_arguments

:arguments_parsed
call :load_modules
if errorlevel 1 (
    popd
    exit /b 1
)

if not defined BUILD_CONFIGURATION call :ask_building
call :normalize_building
if errorlevel 1 (
    popd
    exit /b 1
)

if not defined MODULES_GIVEN call :ask_modules
call :resolve_modules
if errorlevel 1 (
    popd
    exit /b 1
)

echo.
echo Construction !BUILD_CONFIGURATION!...
set "POWERSHELL7=C:\Program Files\WindowsApps\Microsoft.PowerShell_7.6.6.0_x64__8wekyb3d8bbwe\pwsh.exe"
if "!MODULE_MODE!"=="all" (
    "!POWERSHELL7!" -NoProfile -File "scripts\local-building\build.ps1" -Configuration "!BUILD_CONFIGURATION!" -AllModules
) else if "!MODULE_MODE!"=="selected" (
    "!POWERSHELL7!" -NoProfile -File "scripts\local-building\build.ps1" -Configuration "!BUILD_CONFIGURATION!" -Module "!SELECTED_MODULES!"
) else (
    "!POWERSHELL7!" -NoProfile -File "scripts\local-building\build.ps1" -Configuration "!BUILD_CONFIGURATION!"
)
set "RESULT=!ERRORLEVEL!"
popd
exit /b !RESULT!

:load_modules
set /a MODULE_COUNT=0
for /d %%D in ("src\GWGUI.Emulation.*") do (
    if exist "%%~fD\module.json" (
        set "CURRENT_ID="
        set "CURRENT_VERSION="
        for /f "usebackq tokens=2 delims=:" %%V in (`findstr /r /c:"\"id\"[ ]*:" "%%~fD\module.json"`) do set "CURRENT_ID=%%V"
        for /f "usebackq tokens=2 delims=:" %%V in (`findstr /r /c:"\"moduleVersion\"[ ]*:" "%%~fD\module.json"`) do set "CURRENT_VERSION=%%V"
        set "CURRENT_ID=!CURRENT_ID: =!"
        set "CURRENT_ID=!CURRENT_ID:,=!"
        set "CURRENT_ID=!CURRENT_ID:"=!"
        set "CURRENT_VERSION=!CURRENT_VERSION: =!"
        set "CURRENT_VERSION=!CURRENT_VERSION:,=!"
        set "CURRENT_VERSION=!CURRENT_VERSION:"=!"
        if not defined CURRENT_ID (
            echo Erreur : identifiant absent de %%~fD\module.json.
            exit /b 1
        )
        if not defined CURRENT_VERSION (
            echo Erreur : version absente de %%~fD\module.json.
            exit /b 1
        )
        set /a MODULE_COUNT+=1
        set "MODULE_ID_!MODULE_COUNT!=!CURRENT_ID!"
        set "MODULE_VERSION_!MODULE_COUNT!=!CURRENT_VERSION!"
    )
)
exit /b 0

:ask_building
echo Configuration :
echo   1 = Debug
echo   2 = Release
set "BUILD_CONFIGURATION="
set /p "BUILD_CONFIGURATION=Choix : "
if not defined BUILD_CONFIGURATION goto ask_building
exit /b 0

:normalize_building
if /i "!BUILD_CONFIGURATION!"=="1" set "BUILD_CONFIGURATION=Debug"
if /i "!BUILD_CONFIGURATION!"=="D" set "BUILD_CONFIGURATION=Debug"
if /i "!BUILD_CONFIGURATION!"=="Debug" set "BUILD_CONFIGURATION=Debug"
if /i "!BUILD_CONFIGURATION!"=="2" set "BUILD_CONFIGURATION=Release"
if /i "!BUILD_CONFIGURATION!"=="R" set "BUILD_CONFIGURATION=Release"
if /i "!BUILD_CONFIGURATION!"=="Release" set "BUILD_CONFIGURATION=Release"
if "!BUILD_CONFIGURATION!"=="Debug" exit /b 0
if "!BUILD_CONFIGURATION!"=="Release" exit /b 0
echo Erreur : --building doit etre debug ou release.
exit /b 1

:ask_modules
echo.
echo Modules :
echo   A = Tous les modules
echo   0 = Aucun module
for /l %%N in (1,1,!MODULE_COUNT!) do echo   %%N = !MODULE_ID_%%N! - v!MODULE_VERSION_%%N!
set "MODULES_INPUT="
set /p "MODULES_INPUT=Choix (plusieurs numeros separes par des virgules) : "
if not defined MODULES_INPUT goto ask_modules
set "MODULES_GIVEN=1"
exit /b 0

:resolve_modules
if not defined MODULES_INPUT (
    echo Erreur : --modules ne peut pas etre vide.
    exit /b 1
)
if /i "!MODULES_INPUT!"=="A" (
    set "MODULE_MODE=all"
    exit /b 0
)
if /i "!MODULES_INPUT!"=="All" (
    set "MODULE_MODE=all"
    exit /b 0
)
if /i "!MODULES_INPUT!"=="0" (
    set "MODULE_MODE=none"
    exit /b 0
)
if /i "!MODULES_INPUT!"=="None" (
    set "MODULE_MODE=none"
    exit /b 0
)

set "SELECTED_MODULES="
set "MODULES_LIST=!MODULES_INPUT:,= !"
for %%S in (!MODULES_LIST!) do (
    call :resolve_module "%%~S"
    if errorlevel 1 exit /b 1
)
if not defined SELECTED_MODULES (
    echo Erreur : aucun module selectionne.
    exit /b 1
)
set "MODULE_MODE=selected"
exit /b 0

:resolve_module
set "REQUESTED_MODULE=%~1"
set "RESOLVED_MODULE="
for /l %%N in (1,1,!MODULE_COUNT!) do (
    if "!REQUESTED_MODULE!"=="%%N" set "RESOLVED_MODULE=!MODULE_ID_%%N!"
    if /i "!REQUESTED_MODULE!"=="!MODULE_ID_%%N!" set "RESOLVED_MODULE=!MODULE_ID_%%N!"
)
if not defined RESOLVED_MODULE (
    echo Erreur : module inconnu : !REQUESTED_MODULE!
    exit /b 1
)
if defined SELECTED_MODULES (
    set "SELECTED_MODULES=!SELECTED_MODULES!,!RESOLVED_MODULE!"
) else (
    set "SELECTED_MODULES=!RESOLVED_MODULE!"
)
exit /b 0

:duplicate_building
echo Erreur : --building est indique plusieurs fois.
popd
exit /b 1

:duplicate_modules
echo Erreur : --modules est indique plusieurs fois.
popd
exit /b 1

:missing_building_value
echo Erreur : --building doit etre suivi de debug ou release.
popd
exit /b 1
