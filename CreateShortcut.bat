@echo off
chcp 65001 > nul
echo ==============================================
echo  NoteX デスクトップ ショートカット作成
echo ==============================================
echo.

set "EXE_PATH=%~dp0publish_standalone\NoteX.exe"
set "DESKTOP=%USERPROFILE%\Desktop"
set "SHORTCUT_PATH=%DESKTOP%\NoteX.lnk"

powershell -NoProfile -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('%SHORTCUT_PATH%'); $s.TargetPath = '%EXE_PATH%'; $s.WorkingDirectory = '%~dp0publish_standalone'; $s.IconLocation = '%EXE_PATH%,0'; $s.Description = 'NoteX - Multi-page Notepad'; $s.Save()"

if exist "%SHORTCUT_PATH%" (
    echo [成功] デスクトップにショートカットを作成しました:
    echo        %SHORTCUT_PATH%
) else (
    echo [エラー] ショートカットの作成に失敗しました。
)
echo.
pause
