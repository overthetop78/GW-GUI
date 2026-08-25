@echo off
setlocal EnableExtensions

pushd "%~dp0.." || exit /b 1

where gh >nul 2>&1 || (
    echo Erreur : GitHub CLI ^(gh^) est introuvable.
    echo Installez-le avec : winget install --id GitHub.cli
    popd
    exit /b 1
)

gh auth status >nul 2>&1 || (
    echo Erreur : GitHub CLI n'est pas connecte. Lancez : gh auth login
    popd
    exit /b 1
)

:ask_major
set "VERSION_MAJOR="
set /p "VERSION_MAJOR=Major : "
call :validate_number VERSION_MAJOR || (
    echo Erreur : Major doit contenir uniquement des chiffres.
    goto ask_major
)

:ask_minor
set "VERSION_MINOR="
set /p "VERSION_MINOR=Minor : "
call :validate_number VERSION_MINOR || (
    echo Erreur : Minor doit contenir uniquement des chiffres.
    goto ask_minor
)

:ask_revision
set "VERSION_REVISION="
set /p "VERSION_REVISION=Revision : "
call :validate_number VERSION_REVISION || (
    echo Erreur : Revision doit contenir uniquement des chiffres.
    goto ask_revision
)
set "VERSION=%VERSION_MAJOR%.%VERSION_MINOR%.%VERSION_REVISION%"

set "NOTES_FILE=.github\release-notes\v%VERSION%.md"
if not exist "%NOTES_FILE%" (
    echo Fichier Markdown introuvable : %NOTES_FILE%
    call :confirm_without_notes
    if errorlevel 1 (
        echo Publication annulee.
        echo Pensez a creer, commit et push le fichier Markdown avant de relancer ce script.
        popd
        exit /b 0
    )
    set "NOTES_FILE="
)

echo 1. Latest
echo 2. Pre-release
echo 3. Aucun label
set /p "PUBLICATION_CHOICE=Type de publication : "

if "%PUBLICATION_CHOICE%"=="1" set "PUBLICATION_TYPE=latest"
if "%PUBLICATION_CHOICE%"=="2" set "PUBLICATION_TYPE=prerelease"
if "%PUBLICATION_CHOICE%"=="3" set "PUBLICATION_TYPE=none"
if not defined PUBLICATION_TYPE (
    echo Erreur : choix invalide.
    popd
    exit /b 1
)

if defined NOTES_FILE (
    gh workflow run release.yml --ref main -f "version=%VERSION%" -f "notes_file=%NOTES_FILE%" -f "publication_type=%PUBLICATION_TYPE%"
) else (
    gh workflow run release.yml --ref main -f "version=%VERSION%" -f "publication_type=%PUBLICATION_TYPE%"
)
set "RESULT=%ERRORLEVEL%"
popd
exit /b %RESULT%

:validate_number
setlocal EnableDelayedExpansion
set "VALUE=!%~1!"
if not defined VALUE exit /b 1
for /f "delims=0123456789" %%A in ("!VALUE!") do exit /b 1
exit /b 0

:confirm_without_notes
set "WITHOUT_NOTES="
set /p "WITHOUT_NOTES=Continuer sans fichier Markdown et laisser GitHub generer les notes ? (O/N) : "
if /i "%WITHOUT_NOTES%"=="O" exit /b 0
if /i "%WITHOUT_NOTES%"=="N" exit /b 1
echo Erreur : repondez O ou N.
goto confirm_without_notes
