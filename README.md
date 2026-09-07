# NoteX 📝

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%2011%20%2F%2010-0078D6?logo=windows&logoColor=white)](https://microsoft.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

> **1つのファイル（.txtx）で複数ページのテキストを扱える、Windows向け次世代タブ型メモ帳。**  
> Windows 11 のメモ帳と同等の基本操作感・軽快さを維持しながら、タブごとにファイルが増えてしまう問題を解決します。

---

## 💡 開発の背景

Windows 11 の標準メモ帳にタブ機能が実装されましたが、タブごとに個別のテキストファイル（`.txt`）を作成・保存する必要があり、関連するメモや章立て・タスクをまとめる際にファイルが散乱してしまいます。

**NoteX** は、**「1つのファイル（.txtx）＝ 複数ページ（タブ）」** というドキュメント構造を提供し、関連するテキストをスマートに1つに集約できるように開発されました。

```
【従来のメモ帳】
メモ1.txt, メモ2.txt, メモ3.txt, アイデア.txt ... (ファイルが散乱)

【NoteX (.txtx)】
プロジェクトA.txtx
├── [タブ1] 要件定義
├── [タブ2] TODOリスト
└── [タブ3] 議事録
```

---

## ✨ 主な機能

### 1. 複数ページ管理（タブUI）
- **タブ操作**:
  - `＋` ボタンまたは `Ctrl+T` で即座に新規ページを追加。
  - `✕` ボタンまたは `Ctrl+W` でページを閉じる。
  - タブ名をダブルクリック、または `F2` キーで直接名前変更。
  - 右クリックコンテキストメニュー（名前の変更、ページの複製、左右並べ替え、テキスト出力）。
  - 未保存の変更があるタブには変更インジケータ（●）を表示。

### 2. 個別 `.txt` との多彩な相互変換
- **全ページ一括エクスポート**:
  - 全タブのテキストを指定フォルダに個別の `.txt` ファイルとして一括書き出し。
  - ページ順序を維持する連番プレフィックス（例: `01_タイトル.txt`）の付与に対応。
  - 文字コード（UTF-8 / Shift_JIS）および改行コード（CRLF / LF）の選択が可能。
- **現在のページをエクスポート**:
  - 表示中のアクティブなページのみを単一の `.txt` ファイルとして書き出し。
- **全ページ結合エクスポート**:
  - 全ページを区切り線ヘッダー付きで1つの大きな `.txt` ファイルにまとめて出力。
- **テキストファイルのインポート**:
  - 複数の `.txt` ファイルを選択して、NoteX の新しいページ群として一括取り込み（ファイル名が自動でページ名になります）。
  - フォルダ内の全 `.txt` ファイルの一括取り込みにも対応。

### 3. メモ帳と同等の基本編集 & Windows 11 Fluent スタイル
- **編集機能**: 高速タイピング、完全な Undo / Redo、日付と時刻の挿入 (`F5`)。
- **検索・置換**: `Ctrl+F`（検索）、`Ctrl+H`（置換）、`F3` / `Shift+F3`（次/前を検索）。
- **表示設定**:
  - 行番号表示（エディタと完全スクロール同期、トグル切替可能）。
  - 右端での折り返し（ワードラップ）切替。
  - フォント名・フォントサイズの変更。
  - 拡大 / 縮小（`Ctrl + マウスホイール`、`Ctrl + +/-/0`）。
- **テーマ切替**:
  - システム設定に従う / ライトモード / ダークモード。
- **ステータスバー**:
  - キャレット位置（行・列）、文字数（テキスト選択時は選択文字数を表示）、ズーム倍率、改行コード、エンコーディング、ページ数インジケータ。

---

## ⌨️ ショートカットキー一覧

| キー操作 | 機能 |
| :--- | :--- |
| **Ctrl + N** | 新規ドキュメントを作成 |
| **Ctrl + O** | 既存の NoteX ドキュメント (.txtx) を開く |
| **Ctrl + S** | 上書き保存 |
| **Ctrl + Shift + S** | 名前を付けて保存 |
| **Ctrl + T** | 新規ページ（タブ）を追加 |
| **Ctrl + W** | 現在のページを閉じる |
| **F2 / ダブルクリック** | ページ名の変更 |
| **Ctrl + F** | インライン検索バーを表示 |
| **Ctrl + H** | インライン置換バーを表示 |
| **F3 / Shift + F3** | 次を検索 / 前を検索 |
| **F5** | 現在の日付と時刻を挿入 |
| **Ctrl + ホイール** | 画面の拡大 / 縮小 |
| **Ctrl + Plus / Minus** | 画面の拡大 / 縮小 |
| **Ctrl + 0** | 既定の拡大率 (100%) にリセット |
| **Esc** | 検索・置換バーを閉じる |

---

## 📁 .txtx ファイルフォーマット

`.txtx` ファイルは、UTF-8 エンコーディングのオープンな JSON 形式です。  
万が一 NoteX 以外の環境でも標準のテキストエディタで閲覧・復元が可能で、Git等のバージョン管理ツールでも差分を簡単に追跡できます。

```json
{
  "format": "NoteX",
  "version": "1.0",
  "title": "ノート名",
  "createdAt": "2026-09-07T10:00:00.0000000+09:00",
  "updatedAt": "2026-09-07T10:00:00.0000000+09:00",
  "activePageIndex": 0,
  "pages": [
    {
      "id": "c1f2e8b0-8c29-4d33-91ab-6b1d2e9a5c88",
      "title": "ページ 1",
      "content": "テキスト内容...",
      "encoding": "utf-8",
      "createdAt": "2026-09-07T10:00:00.0000000+09:00",
      "modifiedAt": "2026-09-07T10:00:00.0000000+09:00"
    }
  ]
}
```

---

## 🛠️ ビルドと実行

### 動作環境
- **OS**: Windows 10 / 11 (x64)
- **ランタイム**: .NET 10.0 (または .NET Desktop Runtime 10)

### 開発・テスト
```powershell
# リポジトリのクローン
git clone https://github.com/iwa-kasoutuuuuuka/NoteX.git
cd NoteX

# 単体テストの実行 (12件全合格)
dotnet test

# アプリケーションの起動
dotnet run --project NoteX/NoteX.csproj
```

### 配布用実行ファイルのビルド

#### 1. 完全ローカル・単一自己完結型EXE（推奨・ランタイム不要）
.NET ランタイムが未インストールのPCや、オフライン環境、USBメモリでも単体で動作するポータブル版です。
```powershell
dotnet publish NoteX/NoteX.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./publish_standalone
```
- `publish_standalone/NoteX.exe` を任意の場所にコピーするだけで即座に使用可能です。
- 同フォルダに `portable.txt` を配置することで、設定ファイル (`settings.json`) もローカルに保存される「完全ポータブルモード」で動作します。

#### 2. フレームワーク依存版（軽量・要.NET 10ランタイム）
```powershell
dotnet publish NoteX/NoteX.csproj -c Release -r win-x64 --self-contained false -o ./publish
```

---

## 便利なユーティリティスクリプト

- **`CreateShortcut.bat`**: デスクトップに NoteX の起動ショートカットを自動作成します。
- **`Associate_txtx.bat`**: `.txtx` ファイルをダブルクリックした際に NoteX で開けるように Windows への関連付けを登録します（管理者権限不要）。

---

## 📄 ライセンス

本プロジェクトは [MIT License](LICENSE) の下で公開されています。
