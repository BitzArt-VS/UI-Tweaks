@echo off
setlocal

if "%VINTAGE_STORY%"=="" (
    echo ERROR: VINTAGE_STORY environment variable is not set.
    exit /b 1
)

set TARGET=%~dp0lib

call :copy_dll "VintagestoryAPI.dll"
call :copy_dll "VintagestoryLib.dll"
call :copy_dll "Mods\VSSurvivalMod.dll"
call :copy_dll "Mods\VSEssentials.dll"
call :copy_dll "Mods\VSCreativeMod.dll"
call :copy_dll "Lib\Newtonsoft.Json.dll"
call :copy_dll "Lib\0Harmony.dll"
call :copy_dll "Lib\protobuf-net.dll"
call :copy_dll "Lib\cairo-sharp.dll"
call :copy_dll "Lib\Microsoft.Data.Sqlite.dll"

echo Done.
exit /b 0

:copy_dll
set SRC=%VINTAGE_STORY%\%~1
if not exist "%SRC%" (
    echo WARNING: Not found, skipping: %~1
    exit /b 0
)
copy /y "%SRC%" "%TARGET%\" >nul
echo Copied: %~1
exit /b 0
