# 主揪後端流程驗證

驗證日期：2026-09-10。後端使用既有開發用 Supabase PostgreSQL 與 ASP.NET Core Identity。

## 已通過

- `POST /api/auth/register` 建立明確標示 TEST 的帳號。
- `POST /api/auth/login`：錯誤密碼回傳 `401 invalid_credentials`；正確密碼取得 Cookie 並回傳相同使用者 ID。
- `POST /api/activities`：匿名回傳 `401`；登入後回傳 `201`、Location 與一次性分享碼。
- `GET /api/activities/{id}`：主揪取得已保存活動與兩個日期、兩個地點；其他帳號與不存在 ID 均回傳 `404 not_found`。
- `PATCH /api/activities/{id}`：主揪可修改尚未截止的 `open` 活動名稱與既有候選日期／時間，重新 GET 確認資料已保存；未提及的日期保持原值。
- 修改保留候選 ID、數量、排序與所有固定欄位；真正交換兩個相同開始時間的日期也通過 PostgreSQL 唯一索引。
- 夾帶截止時間、類型、地點、預算、地區、主揪 ID 或狀態一律 `400`；空白／過長名稱、無效時間、重複 ID、未知 ID、缺少日期、重複日期與空修改皆 `400`，失敗後重查確認沒有部分寫入。
- 他人／不存在活動修改 `404`；新建短期限 TEST 活動並等待自然到期，修改回傳 `409 activity_not_editable`，沒有改動既有 Deadline 來製造測試條件。
- Deadline 接受 `+08:00`，回傳 UTC，保留相同時間點（PostgreSQL 微秒精度）；查詢 DTO 不回傳分享碼或雜湊。
- `POST /api/auth/logout`：清除 Cookie；同一 Session 隨後查詢、建立與修改均回傳 `401`；重複登出回傳 `200`。
- 以上成功／失敗回應均驗證 `success`、`traceId` 與適用的錯誤代碼。
- `dotnet build asp-gather-match/asp-gather-match/asp-gather-match.slnx --no-restore`：0 警告、0 錯誤。
- `dotnet test asp-gather-match/asp-gather-match/asp-gather-match.slnx --no-restore`：65 通過、0 失敗、0 略過。包含 62 個單元測試、3 個 SQLite 記憶體關聯式測試；後者涵蓋 Repository 主揪隔離、日期交換、第二次寫入失敗時整筆交易回復。無外部測試依賴。
- `pwsh -NoProfile -File scripts/Test-HostFlow.ps1`：最後一輪 197 項檢查通過。這是實際連接開發 PostgreSQL 的 HTTP 驗證，不列入上述單元／SQLite 測試數量。

## 實作責任

- `UpdateActivityRequest` 限定輸入名稱與日期，拒絕額外欄位；API 範例見 README。
- `ActivitiesController` 從 Identity claim 取得主揪 ID，將 Service Result 轉成統一 HTTP 回應。
- `ActivityService` 檢查歸屬、狀態、截止時間與完整日期集合的唯一性；所有驗證通過才修改 entity。
- `ActivityRepository` 使用 EF Core 保存；多筆日期鍵更新分兩階段置於同一交易，避免交換日期時觸發唯一索引衝突，失敗整筆回復，不刪除選項。
- 本次沿用既有 DI 註冊與 Authentication → Authorization 管線，未修改 Schema 或 `Program.cs`。

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
每次執行會新增兩個 TEST 帳號、兩個 TEST 活動（一般活動與短期限活動），每場各兩個候選日期／地點，不清除資料。
密碼隨機產生，不輸出、不保存；Cookie 與分享碼也只保留在記憶體。測試帳號供自動驗證使用，並非附帶可公開密碼的共用帳號。
手動 Swagger 驗收可使用 `/api/auth/register` 自行設定私人測試帳號，再依上述順序操作。

## 本次保留的開發資料

| Run ID | 使用者 ID | 活動 ID | 結果 |
| --- | --- | --- | --- |
| `host-flow-20260910-084220-a0526b` | 3、4 | 2 | 腳本時間精度比對需修正；資料已建立 |
| `host-flow-20260910-084310-72a4b6` | 5、6 | 3 | 腳本 JSON 日期自動轉換需修正；資料已建立 |
| `host-flow-20260910-084346-3962d8` | 7、8 | 4 | 65 項檢查全部通過 |
| `host-flow-20260910-085910-c38447` | 9、10 | 5、6 | 完整含修改流程 197 項檢查通過；活動 5 更新名稱與日期，活動 6 自然到期後驗證拒絕修改 |

本次工作從登入／登出區塊到修改完成，共新增 8 個帳號、5 個活動、10 個候選日期、10 個候選地點。最後一輪新增 2 個帳號、2 個活動、4 個日期與 4 個地點；先前三輪測試資料仍保留。帳號 Email 為 `<Run ID>-owner@example.invalid` 或 `<Run ID>-other@example.invalid`，DisplayName 與活動名稱均包含 TEST 與 Run ID。
所有測試 Session 已登出。本次未執行 Migration、刪除資料、正式部署或權限修改；原有活動 ID 1 未修改。

## 已知限制與未包含範圍

- 本次後端垂直流程已完成；日期修改僅更新既有項目，不提供新增／刪除日期或變更排序。
- 截止時間建立後固定；到期即拒絕修改，但尚無背景排程自動改變 Status 或結算。到期活動資料的 Status 仍可能是 `open`，API 會另外檢查 Deadline。
- 目前無投票 Entity；未來加入投票前須先保護既有回覆，不能直接沿用舊票到修改後日期。
- 尚未實作編輯版本衝突提示；多個主揪視窗同時修改同一欄位時，採最後成功寫入的值。
- 不含 Nuxt 串接、活動列表、公開分享查詢、投票、結算、背景排程或 Google Places。
- Cookie 登出清除目前瀏覽器憑證，不代表伺服器立即撤銷所有已複製 Cookie；目前未加入全裝置 Session 撤銷。
- 跨站 Nuxt 的 CORS／SameSite／Secure／CSRF 策略仍是正式整合前的工作。
