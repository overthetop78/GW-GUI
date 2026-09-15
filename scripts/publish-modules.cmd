@echo off
setlocal EnableExtensions EnableDelayedExpansion

pushd "%~dp0.." || exit /b 1

where gh >nul 2>&1 || (
    echo Erreur : GitHub CLI ^(gh^) est introuvable.
    popd
    exit /b 1
)

gh auth status >nul 2>&1 || (
    echo Erreur : GitHub CLI n'est pas connecte. Lancez : gh auth login
    popd
    exit /b 1
)

if not "%~1"=="" (
    set "MODULE_DIRECTORY=src\GWGUI.Emulation.%~1"
    if not exist "!MODULE_DIRECTORY!\module.json" (
        echo Erreur : module introuvable : %~1
        popd
        exit /b 1
    )
    call :publish_manifest "!MODULE_DIRECTORY!\module.json"
    set "RESULT=!ERRORLEVEL!"
    popd
    exit /b !RESULT!
)

set "FOUND_MODULE="
for /d %%D in ("src\GWGUI.Emulation.*") do (
    if exist "%%~fD\module.json" (
        set "FOUND_MODULE=1"
        call :publish_manifest "%%~fD\module.json"
        if errorlevel 1 (
            popd
            exit /b 1
        )
    )
)

if not defined FOUND_MODULE (
    echo Erreur : aucun module d'emulation n'a ete trouve.
    popd
    exit /b 1
)

popd
exit /b 0

:publish_manifest
set "MANIFEST=%~1"
set "MODULE_ID="
set "MODULE_VERSION="
for /f "usebackq tokens=2 delims=:" %%V in (`findstr /r /c:"\"id\"[ ]*:" "%MANIFEST%"`) do set "MODULE_ID=%%V"
for /f "usebackq tokens=2 delims=:" %%V in (`findstr /r /c:"\"moduleVersion\"[ ]*:" "%MANIFEST%"`) do set "MODULE_VERSION=%%V"
set "MODULE_ID=!MODULE_ID: =!"
set "MODULE_ID=!MODULE_ID:,=!"
set "MODULE_ID=!MODULE_ID:"=!"
set "MODULE_VERSION=!MODULE_VERSION: =!"
set "MODULE_VERSION=!MODULE_VERSION:,=!"
set "MODULE_VERSION=!MODULE_VERSION:"=!"

if not defined MODULE_ID (
    echo Erreur : identifiant absent de !MANIFEST!.
    exit /b 1
)
if not defined MODULE_VERSION (
    echo Erreur : version absente de !MANIFEST!.
    exit /b 1
)

set "RELEASE_TAG=module-!MODULE_ID!-v!MODULE_VERSION!"
set "NOTES_FILE=.github\release-notes\modules\!MODULE_ID!\v!MODULE_VERSION!.md"
set "GENERATE_NOTES=false"

gh release view "!RELEASE_TAG!" >nul 2>&1
if not errorlevel 1 (
    echo Deja publie : !RELEASE_TAG!
    exit /b 0
)

if not exist "!NOTES_FILE!" (
    echo Fichier Markdown introuvable : !NOTES_FILE!
    call :confirm_without_notes
    if errorlevel 1 exit /b 1
    set "GENERATE_NOTES=true"
)

set "PREVIOUS_RUN="
for /f %%R in ('gh run list --workflow module-release.yml --event workflow_dispatch --limit 1 --json databaseId --jq ".[0].databaseId"') do set "PREVIOUS_RUN=%%R"

echo Publication de !MODULE_ID! !MODULE_VERSION!...
gh workflow run module-release.yml --ref main -f "module=!MODULE_ID!" -f "generate_notes=!GENERATE_NOTES!"
if errorlevel 1 exit /b 1

set /a RUN_LOOKUP_ATTEMPTS=0
:wait_for_run
timeout /t 2 /nobreak >nul
set /a RUN_LOOKUP_ATTEMPTS+=1
if !RUN_LOOKUP_ATTEMPTS! gtr 60 (
    echo Erreur : execution GitHub Actions introuvable pour !MODULE_ID!.
    exit /b 1
)
set "RUN_ID="
for /f %%R in ('gh run list --workflow module-release.yml --event workflow_dispatch --limit 1 --json databaseId --jq ".[0].databaseId"') do set "RUN_ID=%%R"
if not defined RUN_ID goto wait_for_run
if "!RUN_ID!"=="!PREVIOUS_RUN!" goto wait_for_run

gh run watch "!RUN_ID!" --exit-status
if errorlevel 1 (
    echo Erreur : la publication de !MODULE_ID! !MODULE_VERSION! a echoue.
    exit /b 1
)

echo Publication terminee : !RELEASE_TAG!
exit /b 0

:confirm_without_notes
set "WITHOUT_NOTES="
set /p "WITHOUT_NOTES=Continuer sans fichier Markdown et laisser GitHub generer les notes ? (O/N) : "
if /i "!WITHOUT_NOTES!"=="O" exit /b 0
if /i "!WITHOUT_NOTES!"=="N" exit /b 1
echo Erreur : repondez O ou N.
goto confirm_without_notes
