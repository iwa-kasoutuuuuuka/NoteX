<p align="center">
  <img src="https://raw.githubusercontent.com/iwa-kasoutuuuuuka/NoteX/main/Resources/app.png" width="160" height="160" alt="NoteX App Icon" />
</p>

<h1 align="center">NoteX</h1>

<p align="center">
  <strong>1つのファイル（.txtx）で複数ページのテキストを扱える、Windows向け次世代タブ型メモ帳</strong><br>
  Windows 11 メモ帳と同等の軽快な操作感を維持しながら、ファイル散乱問題をスマートに解決します。
</p>

<p align="center">
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 10" /></a>
  <a href="https://microsoft.com"><img src="https://img.shields.io/badge/Platform-Windows%2011%20%2F%2010-0078D6?logo=windows&logoColor=white" alt="Platform" /></a>
  <a href="https://github.com/iwa-kasoutuuuuuka/NoteX/releases/latest"><img src="https://img.shields.io/badge/Release-v1.0.0-blue?logo=github" alt="Release v1.0.0" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-green.svg" alt="License: MIT" /></a>
</p>

<p align="center">
  <a href="https://github.com/iwa-kasoutuuuuuka/NoteX/releases/download/v1.0.0/NoteX_v1.0_Portable_win-x64.zip">
    <img src="https://img.shields.io/badge/📦_Download-NoteX_v1.0_Portable_Zip_(win--x64)-2ea44f?style=for-the-badge&logo=windows&logoColor=white" alt="Download NoteX Portable Zip" />
  </a>
</p>

<p align="center">
  👉 <strong><a href="https://github.com/iwa-kasoutuuuuuka/NoteX/releases/download/v1.0.0/NoteX_v1.0_Portable_win-x64.zip">NoteX_v1.0_Portable_win-x64.zip をダウンロード</a></strong><br>
  <em>※ インストール不要・.NETランタイム不要。解凍して NoteX.exe をダブルクリックするだけで直ちに動作します。</em>
</p>

---

## 📑 目次
- [💡 開発の背景](#-開発の背景)
- [✨ 主な機能](#-主な機能)
- [🚀 クイックスタート (使い方)](#-クイックスタートポータブル版の使い方)
- [⌨️ ショートカットキー一覧](#️-ショートカットキー一覧)
- [📁 .txtx ファイル仕様](#-txtx-ファイルフォーマット)
- [🔒 セキュリティとプライバシー](#-セキュリティとプライバシー)
- [🛠️ ビルドと実行](#️-ビルドと実行)
- [便利なユーティリティスクリプト](#便利なユーティリティスクリプト)
- [❓ よくある質問 (FAQ)](#-よくある質問-faq)
- [📄 ライセンス](#-ライセンス)

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

# 単体テストの実行 (31件全合格: セキュリティ・ストレステスト含む)
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

## 🚀 クイックスタート（ポータブル版の使い方）

1. **ダウンロード**:  
   [NoteX_v1.0_Portable_win-x64.zip](https://github.com/iwa-kasoutuuuuuka/NoteX/releases/download/v1.0.0/NoteX_v1.0_Portable_win-x64.zip) をダウンロードします。
2. **解凍**:  
   ダウンロードした Zip ファイルを右クリックし、「すべて展開」を選択して任意のフォルダ（デスクトップ、USBメモリなど）に展開します。
3. **起動**:  
   展開したフォルダ内の **`NoteX.exe`** をダブルクリックするとすぐに起動します。  
   *(管理者権限や .NET ランタイムの事前インストールは不要です)*

> [!TIP]
> **初回起動時の「WindowsによってPCが保護されました」表示について**  
> 未署名のオープンソースアプリケーションのため、Windows Defender SmartScreen の警告画面が表示される場合があります。  
> **「詳細情報」をクリック ➜ 「実行」ボタン** をクリックすると起動できます。本ソフトは完全ローカル完結で外部通信を一切行わない安全なソフトウェアです。

---

## 🔒 セキュリティとプライバシー

- **完全オフライン・外部通信ゼロ**:
  - テレメトリ、アクセス解析、クラッシュレポート等の外部通信は一切実装されていません。すべてのテキスト・設定データはあなたのPCローカル内のみで完結します。
- **パストラバーサル防御**:
  - テキストファイル一括エクスポート時、ページタイトルに悪意あるパス（`../../` など）が含まれていても、出力先ディレクトリ外へのファイル生成を物理的に遮断します。
- **Windows 予約デバイス名の自動無害化**:
  - Windows でエラーや不具合の原因となる予約デバイス名（`CON`, `PRN`, `AUX`, `NUL`, `COM1-9`, `LPT1-9` など）を自動検知して安全な名前にサニタイズします。
- **メモリ保護 & 破損ファイル自動復旧**:
  - 異常に巨大なファイル（50MB以上）によるメモリ枯渇（DoS）を防止し、破損したJSONデータも安全にフォールバックして読み込みます。

---

## 便利なユーティリティスクリプト

同梱のバッチファイルを実行することで、より便利に利用できます：

- **`CreateShortcut.bat`**:  
  デスクトップに NoteX の起動ショートカットアイコンを自動作成します。
- **`Associate_txtx.bat`**:  
  `.txtx` ファイルをダブルクリックした際に直接 NoteX で開けるように Windows へ関連付けを登録します（管理者権限不要、現在のユーザーのみに安全に登録）。

---

## ❓ よくある質問 (FAQ)

### Q. アイコンが歯車マークのまま変わらない時はどうすればいいですか？
Windows のエクスプローラーが古いアイコンキャッシュを保持している場合があります。以下のいずれかで解消します：
1. `CreateShortcut.bat` を実行してデスクトップに新しいショートカットを作成する。
2. タスクマネージャーから「エクスプローラー」を右クリックして「再起動」する。

### Q. `.txtx` ファイルは他のエディタでも開けますか？
はい。`.txtx` は標準的な **UTF-8 JSON 形式** のプレーンテキストファイルです。NoteX がない環境でも、VS Code やメモ帳等のテキストエディタで直接開いて文章を閲覧・復旧できます。

### Q. 複数のメモ帳（.txt）を1つのNoteXファイルにまとめるには？
メニューの **「変換・エクスポート」➜「テキストファイル (.txt) をページとして取り込み」** を選択し、まとめたい複数の `.txt` を選択してください。ファイル名がそのまま各タブ（ページ名）になって一括で取り込まれます。

---

## 📄 ライセンス

本プロジェクトは [MIT License](LICENSE) の下で公開されています。

