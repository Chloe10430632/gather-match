# 主揪後端流程驗證

驗證日期：2026-09-10。後端使用既有開發用 Supabase PostgreSQL 與 ASP.NET Core Identity。

## 已通過

- `POST /api/auth/register` 建立明確標示 TEST 的帳號。
- `POST /api/auth/login`：錯誤密碼回傳 `401 invalid_credentials`；正確密碼取得 Cookie 並回傳相同使用者 ID。
- `POST /api/activities`：匿名回傳 `401`；登入後回傳 `201`、Location 與一次性分享碼。
- `GET /api/activities/{id}`：主揪取得已保存活動與兩個日期、兩個地點；其他帳號與不存在 ID 均回傳 `404 not_found`。
- Deadline 接受 `+08:00`，回傳 UTC，保留相同時間點（PostgreSQL 微秒精度）；查詢 DTO 不回傳分享碼或雜湊。
- `POST /api/auth/logout`：清除 Cookie；同一 Session 隨後查詢與建立均回傳 `401`；重複登出回傳 `200`。
- 以上成功／失敗回應均驗證 `success`、`traceId` 與適用的錯誤代碼。
- `dotnet build asp-gather-match/asp-gather-match/asp-gather-match.slnx --no-restore`：0 警告、0 錯誤。
- `dotnet test asp-gather-match/asp-gather-match/asp-gather-match.slnx --no-build --no-restore`：26 通過、0 失敗、0 略過。
- `pwsh -NoProfile -File scripts/Test-HostFlow.ps1`：最後一輪 65 項檢查通過。

## 重跑方式

先確保後端 User Secrets 已有開發資料庫設定，並執行：

```powershell
dotnet run --project asp-gather-match/asp-gather-match/asp-gather-match --launch-profile http
```

在另一個 PowerShell 7.5 以上的終端機執行：

```powershell
pwsh -NoProfile -File scripts/Test-HostFlow.ps1
```

腳本預設使用本機 5138、活動類型 ID 1、縣市 ID 1；可透過參數指定其他有效參照 ID。
每次執行會新增兩個 TEST 帳號、一個 TEST 活動與兩個日期／地點，不清除資料。
密碼隨機產生，不輸出、不保存；Cookie 與分享碼也只保留在記憶體。測試帳號供自動驗證使用，並非附帶可公開密碼的共用帳號。
手動 Swagger 驗收可使用 `/api/auth/register` 自行設定私人測試帳號，再依上述順序操作。

## 本次保留的開發資料

| Run ID | 使用者 ID | 活動 ID | 結果 |
| --- | --- | --- | --- |
| `host-flow-20260910-084220-a0526b` | 3、4 | 2 | 腳本時間精度比對需修正；資料已建立 |
| `host-flow-20260910-084310-72a4b6` | 5、6 | 3 | 腳本 JSON 日期自動轉換需修正；資料已建立 |
| `host-flow-20260910-084346-3962d8` | 7、8 | 4 | 65 項檢查全部通過 |

共新增 6 個帳號、3 個活動、6 個候選日期、6 個候選地點。帳號 Email 為 `<Run ID>-owner@example.invalid` 或 `<Run ID>-other@example.invalid`，DisplayName 與活動名稱均包含 TEST 與 Run ID。
所有測試 Session 已登出。本次未執行 Migration、刪除資料、正式部署或權限修改；原有活動 ID 1 未修改。

## 尚待完成

- 活動修改 API、對應單元測試與真實 HTTP 驗證：等待確認可修改欄位範圍。`DATABASE_DESIGN_DRAFT.md` 第 363 行原本即列為待決問題。
- 不含 Nuxt 串接、活動列表、公開分享查詢、投票、結算、背景排程或 Google Places。
- Cookie 登出清除目前瀏覽器憑證，不代表伺服器立即撤銷所有已複製 Cookie；目前未加入全裝置 Session 撤銷。
- 跨站 Nuxt 的 CORS／SameSite／Secure／CSRF 策略仍是正式整合前的工作。
