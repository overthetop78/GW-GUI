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

gh workflow run module-directory.yml --ref main
set "RESULT=%ERRORLEVEL%"
popd
exit /b %RESULT%
