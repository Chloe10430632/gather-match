# nuxt-gather-match

「揪哪天?（Gathering Match）」的 Nuxt 3 前端專案。

## Current Status

- 首頁主揪管理已串接 ASP.NET Core：註冊、登入、登出、恢復身分、活動建立、查詢、名稱與候選日期／時間修改。
- 類型與縣市／行政區選項使用後端參照資料 ID，截止時間由本地時間轉為 UTC。
- 同站 `/api` 經 Nitro server 代理至 `NUXT_API_BASE`（預設 `http://localhost:5138`），登入 Cookie 不存入 localStorage。
- 原有 Step 1～3 投票 Demo 保留在獨立入口，僅 Demo 使用 localStorage；公開分享／投票 API 尚未實作。

完整專案進度與下一步請見根目錄的 [`PROJECT_STATUS.md`](../PROJECT_STATUS.md)。

## Setup

```bash
npm install
```

## Development

```bash
npm run dev
```

開啟 `http://localhost:3000`。

先從專案根目錄啟動後端：`dotnet run --project asp-gather-match/asp-gather-match/asp-gather-match --launch-profile http`。
完整驗證及限制見 [FRONTEND_INTEGRATION.md](../FRONTEND_INTEGRATION.md)。

```bash
npm run typecheck
npm run build
npm run test:proxy
```

`test:proxy` 啟動隔離上游與 production Nitro server，不連線雲端。手動 UI 測試可在 build 後執行 `node tests/ui-fixture.mjs`，開啟 `http://127.0.0.1:3001`；僅使用記憶體測試資料，結束按 Ctrl+C。

## Production

```bash
npm run build
npm run preview
```
