@echo off
chcp 65001 > nul
echo ==============================================
echo  NoteX: .txtx ファイル関連付け登録
echo ==============================================
echo.
echo このPCの .txtx ファイルをダブルクリックした際に
echo NoteX で直接開けるように関連付けを行います。
echo.

set "EXE_PATH=%~dp0publish_standalone\NoteX.exe"

REM HKCU\Software\Classes に登録（管理者権限不要）
reg add "HKCU\Software\Classes\.txtx" /ve /d "NoteX.Document" /f > nul
reg add "HKCU\Software\Classes\NoteX.Document" /ve /d "NoteX ドキュメント" /f > nul
reg add "HKCU\Software\Classes\NoteX.Document\DefaultIcon" /ve /d "\"%EXE_PATH%\",0" /f > nul
reg add "HKCU\Software\Classes\NoteX.Document\shell\open\command" /ve /d "\"%EXE_PATH%\" \"%%1\"" /f > nul

if %ERRORLEVEL% equ 0 (
    echo [成功] .txtx ファイルの関連付けが完了しました！
    echo        今後 .txtx ファイルをダブルクリックすると NoteX で起動します。
) else (
    echo [エラー] 関連付けの登録に失敗しました。
)
echo.
pause
