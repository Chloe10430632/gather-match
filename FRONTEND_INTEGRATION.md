# Nuxt 與 ASP.NET Core 串接紀錄

驗證日期：2026-09-14。

## 已接通的範圍

主揪註冊後登入、登入失敗訊息、Cookie 身分恢復與登出；建立活動（日期、自訂地點、預算、類型、地區、固定截止時間）；依活動 ID 查詢本人活動；修改名稱與既有候選日期／時間。

瀏覽器 → 同站 Nuxt `/api` → ASP.NET Core Controller／Service／Repository → EF Core／PostgreSQL。Nuxt 不直接操作資料庫。

- `nuxt-gather-match/app/components/HostWorkspace.vue`：表單、載入／錯誤狀態、UTC 轉換、活動 URL、修改與到期控制。
- `nuxt-gather-match/app/composables/useHostApi.ts`、`app/types/api.ts`：型別化統一 API 契約，不自動重試寫入。
- `nuxt-gather-match/server/api/[...path].ts`：固定上游與路徑白名單、Cookie 轉送、Origin／自訂 header 檢查、no-store。
- `AuthController.Me`：從已驗證身分取得有效帳號，回傳公開 DTO。
- `ReferenceDataController`：唯讀取得有效參照資料，避免前端猜測資料庫 ID；不需 migration 或新增 DI 註冊，沿用既有 DbContext。

## 啟動與設定

後端：專案根目錄執行 `dotnet run --project asp-gather-match/asp-gather-match/asp-gather-match --launch-profile http`，使用 .NET SDK 10 與既有 User Secrets。

前端：`cd nuxt-gather-match`，首次執行 `npm ci`，再 `npm run dev`。開啟 `http://localhost:3000`。

前端 server 預設上游為 `http://localhost:5138`。需要修改時，在啟動 Nuxt 的環境設定 `NUXT_API_BASE`。此值不放在 public runtimeConfig，不是資料庫連線字串。

正式執行使用 `npm run build` 與 `node .output/server/index.mjs`，不能只有靜態檔案。對外必須使用 HTTPS；ASP.NET Core 若位於 HTTP 內網，部署時需正確處理 HTTPS 重新導向與受信任 forwarded headers，不能盲目信任任意 forwarded headers。後端不應繞過 Nuxt 邊界直接公開。本次尚未部署，因此未宣稱正式環境 Cookie／代理設定已驗證。

## 驗證結果與重跑

1. `dotnet test asp-gather-match/asp-gather-match/asp-gather-match.slnx --no-restore`：69 個通過，包含新增的 me 成功／失效身分與 SQLite 參照資料篩選排序測試。
2. 前端 `npm run typecheck`：通過。Nuxt 產生的 Vue 自動匯入型別指向 `vue/index`，造成部分回呼參數成為 any；相關檔案改用明確 Vue imports，沒有降低 strict 標準。
3. 前端 `npm run build`：通過。Windows 沙箱阻擋 Nitro 對使用者目錄的 readlink 時，經權限核准後建置成功。套件仍有 DEP0155 deprecation 提示。
4. build 後 `npm run test:proxy`：隔離 HTTP fixture 與真正 production Nitro server；驗證多個 Set-Cookie、HttpOnly、Cookie 回送、建立 201、PATCH body／method、404 外層、跨來源／缺少 header 的 403、非白名單 404，以及登出清除 Cookie。此測試不代表資料庫寫入驗證。
5. build 後 `node tests/ui-fixture.mjs`，開 `http://127.0.0.1:3001`：本機記憶體 fixture，不接外部資料庫。瀏覽器已驗證登入 → 建立活動 → 名稱／日期／時間修改 → 重整恢復 → 登出。fixture 檢查建立使用數字 ID、UTC deadline；修改保留選項 ID 且不夾帶固定欄位。使用 fixture 的臺北市／中山區，測試 Email 可用 `fixture@example.invalid` 與任意測試密碼；Ctrl+C 結束。
6. 真實後端只讀：由 `http://localhost:3000/api/reference-data` 成功取得 6 個類型、22 個縣市及 368 個行政區；`/api/auth/me` 未登入回傳 401。沒有新增或修改 Supabase 資料。
7. `git diff --check`：通過。
8. 品牌 favicon：新增 `app/public/favicon.svg` 與 Nuxt head 設定；production build 成功，首頁包含 icon link，圖示 HTTP 回應為 200／image/svg+xml。

## 目前限制

- 朋友投票、公開分享頁、截止結算尚未有 API；原 Demo 為獨立模式，不能宣稱投票已保存到資料庫。
- 建立成功的分享碼僅在本次畫面可查看，不寫入 localStorage；重整後無法重新取得。公開入口完成前不產生不可用的分享連結。
- 沒有活動列表 API；目前使用活動編號或收藏 `?activity=ID` URL 找回活動。
- 截止時間、地點、預算、類型與地區固定。UI 到期禁用只是輔助，後端仍是授權與商業驗證的最終依據。
- 本次沒有以 Nuxt 完成真實 Supabase 寫入驗收。既有後端 197 項真實 HTTP 檢查屬 2026-09-10，不能當成本次 Nuxt 的真實資料庫驗證。
