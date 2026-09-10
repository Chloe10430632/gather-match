# 揪哪天？（Gathering Match）— 專案進度交接

最後更新：2026-09-10

## 專案位置

- 工作區根目錄：`Side Project 揪團工具`
- Nuxt 前端：`nuxt-gather-match/`
- 前端技術：Nuxt 3、Vue 3、TypeScript、Tailwind CSS
- 後端：ASP.NET Core 10 Web API（`net10.0`）、EF Core Code First、Npgsql
- 資料庫規劃：Supabase Free 代管的 PostgreSQL

## 目前做到哪裡

前端 Step 1～3 與主揪回覆管理 Demo 已完成，目前開始建立 ASP.NET Core 後端基礎；尚未串接前後端或真實地點 API。

### 已完成

- 建立 Nuxt 3 前端專案，資料夾名稱為 `nuxt-gather-match`。
- 使用 Tailwind CSS 完成首頁與建立活動表單的視覺設計。
- Step 1「建立活動」：
  - 活動名稱。
  - 可複選、不必連續的月曆日期選擇器。
  - 每人預算。
  - 通用活動類型，不限定聚餐。
  - 台灣縣市與行政區連動選擇。
  - 必填欄位驗證與提示文字。
- Step 2「確認地點」：
  - 暫時使用 3 筆寫死的示意推薦資料。
  - 可複選候選地點，上限 5 個。
  - 主揪可自行輸入地點名稱或 Google Maps 連結。
  - 可返回 Step 1 修改活動條件。
- Step 3「朋友投票」純前端流程：
  - 主揪確認候選地點後，顯示活動摘要與示意分享連結。
  - 模擬朋友開啟公開連結，不需登入主揪帳號。
  - 朋友輸入顯示名稱後免註冊加入。
  - 對每個候選日期選擇「可以／看情況／不行」。
  - 對每個候選地點選擇「很想去／可以／不想去」。
  - 所有候選項目都回答後才能送出。
  - 回覆完成頁顯示目前已完成的人數，並可展開檢查或返回修改自己的回覆。
- 主揪回覆管理純前端畫面：
  - 從活動分享完成頁進入。
  - 使用 4 位 Demo 參加者呈現已完成人數。
  - 顯示各候選日期與地點的三段式偏好統計。
  - 可展開查看每位朋友的完整回覆與完成時間。
  - 公開連結沒有受邀名單，因此不虛構未回覆人數或完成率。
- 前端示意參加者識別：
  - 完整送出時使用 `crypto.randomUUID()` 產生示意 `participantToken`。
  - 以固定 Demo 活動分享碼區分活動，將名字、Token 與投票內容暫存於 `localStorage`。
  - 同一瀏覽器再次開啟同一 Demo 活動時，會直接還原資料並進入回覆進度頁。
  - 修改後重新送出會沿用原 Token，不會在示意流程中建立另一位參加者。
  - 回覆進度頁可清除這台裝置的示意身分，方便重測第一次加入流程。
- 測試資料輔助按鈕：
  - Step 1 自動填入活動資料與相對於當天的三個未來日期。
  - Step 2 自動選擇三個候選地點。
  - 朋友入口自動填入測試名字。
  - 日期與地點投票可自動填入混合偏好。
  - 測試按鈕只填資料，不會自動送出或切換下一頁。
- 將原本分開的階段進度條與 `STEP` 顯示整合到表單上方：
  1. 建立活動
  2. 確認地點
  3. 朋友投票
  4. 真的約出去
- Vue 狀態目前以 `ref` 為主，沒有使用 `reactive`。
- 最近一次 production build 已成功完成。

### 已確認的後端與資料庫方向

- 使用 ASP.NET Core 10 Web API 作為唯一的應用程式後端，Nuxt 不直接操作 Supabase。
- Supabase 第一階段只作為代管 PostgreSQL，不先加入 Supabase Auth、Realtime、Storage、Edge Functions 或 Data API。
- 使用 Entity Framework Core 作為 ORM，透過 `Npgsql.EntityFrameworkCore.PostgreSQL` 連接 PostgreSQL。
- 採用「先設計資料模型，再用 Code First 實作」：
  1. 先確認 ERD、欄位、關聯、限制與索引。
  2. 建立 C# Entity、`DbContext` 與 Fluent API 設定。
  3. 以 EF Core Migration 建立及更新 Supabase PostgreSQL Schema。
- 正式 Schema 不以 Supabase Table Editor 手動修改為主要流程，避免資料庫與 Migration 紀錄不同步。
- Connection string 應保存於 .NET User Secrets 或部署環境變數，不可提交至 GitHub。
- Supabase Free 目前足以支援開發與小規模 Demo；需要不中斷服務、自動備份或超過免費額度時才評估升級。

### 已完成的後端基礎

- 已加入 `POST /api/auth/logout`，透過 `SignInManager.SignOutAsync` 清除 Cookie，可重複呼叫。
- 已加入 `GatherMatch.Tests`（xUnit／Moq），涵蓋登入、登出與既有活動建立規則；單元測試不存取外部資料庫。
- 已加入 `GET /api/activities/{id}`：Repository 同時篩選活動 ID 與登入主揪 ID，包含排序後的候選日期／地點；其他主揪或不存在均回傳 `404 not_found`，DTO 不包含分享碼或雜湊。
- 2026-09-10：26 個單元測試通過；本機 API 連接開發 Supabase 的註冊、登入、建立、單筆查詢、登出通過 65 項檢查，包含跨主揪隔離與登出後 `401`。詳細紀錄與測試資料見 `BACKEND_VERIFICATION.md`。
- 2026-09-10 完成本次最終目標：加入 `PATCH /api/activities/{id}`，只允許主揪修改尚未截止的 `open` 活動名稱與既有候選日期／時間；保留選項 ID、數量與排序，其他欄位固定。截止時間不可延長。
- 修改 API 對不可修改欄位及無效日期回傳 `400`；非本人／不存在回傳 `404`；已截止或非 `open` 回傳 `409`。日期交換以交易處理唯一索引，不刪除選項。
- 最新驗證：62 個單元測試＋3 個 SQLite 記憶體關聯式測試通過；完整真實 Cookie／Supabase HTTP 流程通過 197 項檢查，包含日期交換、固定欄位、到期拒絕修改與登出後三個活動 endpoints 的 `401`。

- 建立 ASP.NET Core 10 Web API 專案，Target Framework 為 `net10.0`。
- 後端目前可成功建置及啟動，最近一次結果為 0 個警告、0 個錯誤。
- 已建立 Supabase project，並已將 `InitialIdentity` Migration 套用到 Supabase PostgreSQL。
- 已透過 .NET User Secrets 保存 Supabase PostgreSQL Session pooler Connection String；未將密碼寫入專案檔案。
- 已安裝並固定以下套件版本：
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore` `10.0.11`
  - `Microsoft.EntityFrameworkCore.Design` `10.0.11`
  - `Npgsql.EntityFrameworkCore.PostgreSQL` `10.0.3`
- `dotnet-ef` CLI 已更新為 `10.0.11`。
- 已決定主揪登入採 ASP.NET Core Identity，不使用 Supabase Auth，也不自行實作密碼雜湊。
- 已建立 `ApplicationUser : IdentityUser<long>`，包含 `DisplayName`、`IsActive`、`CreatedAt`、`UpdatedAt`。
- 已建立 `ApplicationDbContext : IdentityUserContext<ApplicationUser, long>`；第一階段不加入 Roles／UserRoles。
- 已使用 Fluent API 將 `DisplayName` 設為必填且最多 50 字。
- `Program.cs` 已註冊 Npgsql、`ApplicationDbContext`、Identity、Authentication 與 Authorization。
- 已建立自訂 `POST /api/auth/register` 與 `POST /api/auth/login`，註冊契約包含 `Email`、`Password`、`DisplayName`。
- 註冊由 `UserManager<ApplicationUser>` 套用 Identity 密碼規則與密碼雜湊；登入由 `SignInManager<ApplicationUser>` 建立同站 Identity Cookie，Swagger 登入後可沿用該身分。
- Auth API 沿用 `ApiResponse<T>` 統一成功與錯誤外層；登入失敗不揭露帳號是否存在。
- 已決定 C# 與 PostgreSQL 資料表／欄位統一使用 PascalCase，不加入 `EFCore.NamingConventions`。
- 已建立並套用 `InitialIdentity` Migration；資料庫包含 `AspNetUsers`、`AspNetUserClaims`、`AspNetUserLogins`、`AspNetUserTokens`。
- 已建立活動規劃資料模型：`ActivityType`、`City`、`District`、`Activity`、`DateOption`、`PlaceOption`。
- 已在 Fluent API 設定活動資料的欄位長度、預設值、唯一限制、外鍵、刪除行為與必要 Check Constraints。
- 已建立並套用 `AddActivityPlanning` Migration；Supabase 已有六張活動規劃資料表，建置結果為 0 個警告、0 個錯誤。
- 已建立並套用 `EnableRowLevelSecurity` Migration，為四張 Identity 表及六張業務表啟用 RLS；目前不建立 Data API 公開 Policy，資料統一由 ASP.NET Core 後端存取。
- 已建立 `POST /api/activities` 新增活動 API 的第一條垂直流程：
  - Request／Response DTO 定義建立活動、候選日期與 1～5 個自訂候選地點的 API 契約。
  - Repository 負責檢查有效參照資料，並以 EF Core 一次儲存 Activity aggregate。
  - Service 負責預算、Deadline、候選日期／地點及縣市／行政區關係等商業驗證。
  - Controller 從登入使用者的 Identity claim 取得 `HostUserId`，不接受客戶端指定主揪 ID。
  - 公開分享 Token 只在建立成功時回傳一次，資料庫僅保存 SHA-256 雜湊。
- OpenAPI 已確認列出此 endpoint 與 `201`、`400`、`401` 回應；未登入呼叫實測回傳 `401`。
- Development 環境已加入 Swagger UI，可從 `/swagger` 查看及操作 API；正式環境不開放。
- API 已建立泛型 `ApiResponse<T>` 統一外層，包含 `success`、`data`、`error` 與 `traceId`：
  - 建立活動成功以 `ApiResponse<CreateActivityResponse>` 回傳。
  - DTO／商業規則驗證、401、403、404 均使用相同錯誤結構與正確 HTTP Status Code。
  - 未預期 Exception 由 `IExceptionHandler` 集中記錄並回傳不含內部細節的 500 Response。
  - 預期中的商業驗證仍由 Service Result 表達，不使用 Exception 控制流程。
- 已建立 `SeedActivityTypes` Migration，透過 EF Core `HasData` 管理六筆固定活動類型：`meal`、`coffee`、`outdoor`、`culture`、`travel`、`other`。
- `SeedActivityTypes` 已套用到 Supabase PostgreSQL；執行日誌確認插入六筆資料、調整 identity sequence，並寫入 `__EFMigrationsHistory`。
- 已建立台灣位置參照資料工具與完整 JSON 快照：22 個縣市、368 個行政區；應用程式 validator 已確認代碼格式、唯一性與隸屬關係。
- `LocationReferenceImporter` 已將 22 個縣市與 368 個行政區匯入 Supabase；第二次執行新增、更新、停用皆為 0，已確認可重複執行且不會重複插入。
- Auth endpoints 已出現在 OpenAPI；無效登入已實測回傳統一的 `401 invalid_credentials`，且後端建置為 0 個警告、0 個錯誤。
- 已使用 Supabase 真實資料庫完成註冊、Cookie 登入與新增活動成功路徑驗證：Activity ID `1` 與其 2 筆候選日期、2 筆候選地點由 EF Core aggregate 一次寫入，建立成功時有回傳分享 Token。
- 真實寫入測試發現 Npgsql 的 `timestamptz` 只接受 UTC `DateTimeOffset`；`ActivityService` 現在會在保存前以 `ToUniversalTime()` 正規化 Deadline，保留原本時間點。

## 重要檔案

- `DATABASE_DESIGN_DRAFT.md`：最小 ERD、資料表欄位、關聯、Identity 與外部資料保存策略草案。
- `asp-gather-match/asp-gather-match/asp-gather-match/Models/ApplicationUser.cs`：ASP.NET Core Identity 使用者 Entity。
- `asp-gather-match/asp-gather-match/asp-gather-match/Data/ApplicationDbContext.cs`：Identity／EF Core DbContext 與 Fluent API。
- `asp-gather-match/asp-gather-match/asp-gather-match/Program.cs`：Npgsql、Identity、Authentication 與 Authorization 服務註冊。
- `asp-gather-match/asp-gather-match/asp-gather-match/Contracts/Auth/`：主揪註冊、登入與登入使用者 Response DTO。
- `asp-gather-match/asp-gather-match/asp-gather-match/Controllers/AuthController.cs`：主揪註冊與 Cookie 登入 endpoints。
- `asp-gather-match/asp-gather-match/asp-gather-match/Contracts/Activities/`：新增活動 Request／Response DTO。
- `asp-gather-match/asp-gather-match/asp-gather-match/Contracts/Common/`：泛型 API Response 與統一錯誤格式。
- `asp-gather-match/asp-gather-match/asp-gather-match/ErrorHandling/GlobalExceptionHandler.cs`：未預期 Exception 的全域處理。
- `LOCATION_REFERENCE_DATA.md`：台灣縣市／行政區官方來源、代碼欄位與匯入策略。
- `scripts/Update-LocationReferenceSnapshot.ps1`：從 NLSC API 產生並檢查位置資料 JSON 快照。
- `asp-gather-match/asp-gather-match/asp-gather-match/Data/LocationReferenceImporter.cs`：位置快照驗證及 Transaction upsert。
- `asp-gather-match/asp-gather-match/asp-gather-match/Data/SeedData/taiwan-location-reference.json`：版本化的完整位置資料快照。
- `asp-gather-match/asp-gather-match/asp-gather-match/Repositories/`：活動資料存取介面與 EF Core 實作。
- `asp-gather-match/asp-gather-match/asp-gather-match/Services/`：新增活動商業規則與建立流程。
- `asp-gather-match/asp-gather-match/asp-gather-match/Controllers/ActivitiesController.cs`：受 Identity 保護的新增活動 HTTP endpoint。
- `nuxt-gather-match/app/app.vue`：頁面入口、Step 1～3 流程切換、跨步驟草稿與投票狀態、示意參加者 Session。
- `nuxt-gather-match/app/components/ActivityProgress.vue`：四階段進度條。
- `nuxt-gather-match/app/components/ActivitySetupForm.vue`：Step 1 表單與驗證。
- `nuxt-gather-match/app/components/DateMultiPicker.vue`：日期複選月曆。
- `nuxt-gather-match/app/components/LocationSelector.vue`：縣市／行政區連動選單。
- `nuxt-gather-match/app/components/PlaceSelectionStep.vue`：Step 2 推薦與自訂候選地點。
- `nuxt-gather-match/app/components/ActivityShareStep.vue`：主揪分享完成頁與示意分享連結。
- `nuxt-gather-match/app/components/ParticipantEntryStep.vue`：朋友免註冊輸入名字入口。
- `nuxt-gather-match/app/components/DateVotingStep.vue`：候選日期三段式偏好投票。
- `nuxt-gather-match/app/components/PlaceVotingStep.vue`：候選地點三段式偏好投票。
- `nuxt-gather-match/app/components/ResponseProgressStep.vue`：回覆完成、目前人數與個人回覆摘要。
- `nuxt-gather-match/app/components/HostResponseManagementStep.vue`：主揪查看日期／地點統計及朋友回覆明細。
- `nuxt-gather-match/app/types/activity.ts`：活動、地點、日期／地點偏好與示意 Session 型別。
- `nuxt-gather-match/app/data/taiwanLocations.ts`：縣市與行政區資料。

## 尚未完成／目前限制

- Step 2 的推薦地點只是 Demo Data，尚未依活動類型、預算與地區真正篩選。
- Google Maps 連結目前只會當成一般文字加入，尚未解析地點資訊。
- 尚未製作 Step 4「截止結算／最佳方案」。
- 已完成活動建立、主揪單筆查詢與名稱／既有候選日期修改；不含新增／刪除日期選項、投票 Entity 或 Deadline 背景排程。
- `ActivityType` Seed 與 City、District 參照資料皆已套用；真實註冊、Cookie 登入與新增活動 aggregate 已使用一次性測試資料驗證成功。
- 目前 Auth API 使用同站 Identity Cookie，適合 Swagger 與同站開發驗證；Nuxt 分站部署前仍需確認 CORS、Cookie `SameSite`／`Secure` 與 CSRF 防護策略。
- 尚未建立真正的 Nuxt 公開分享路由；目前朋友入口仍是同一頁面的元件切換。
- 示意分享連結尚未建立或載入真實活動資料。
- Nuxt Demo 的活動、朋友與投票內容尚未串接後端；後端活動建立已實際寫入資料庫。
- `localStorage` 目前同時保存示意 Token 與投票內容，只供前端流程驗證；正式版瀏覽器只應保存參加者識別碼，投票內容由後端保存與驗證。
- 正式版需要兩種識別：網址中的活動 `shareToken`，以及朋友瀏覽器保存的 `participantToken`。資料庫只保存 `participantToken` 的安全雜湊值。
- 清除瀏覽器資料、使用無痕模式或更換裝置時，免登入參加者可能無法自動找回原身分；這是 V0.1 可接受但需清楚說明的限制。
- 公開連結目前沒有預設受邀名單或預計人數，因此回覆進度只顯示「幾人已完成」，不顯示不可靠的分母。
- GitHub 已建立 Private repository：`Chloe10430632/gather-match`，本機 `main` 已追蹤 `origin/main`。

## 建議下一步

主揪登入／登出、活動建立、本人單筆查詢與名稱／候選日期修改已全部完成，並通過自動化測試及真實 Cookie HTTP 驗證。完整驗收與重跑方式見 `BACKEND_VERIFICATION.md`。

使用者已確認：建立後截止時間固定，不讓主揪反覆替未投票者延長；只可修改尚未截止的 `open` 活動名稱與日期。投票、結算與排程均不屬於本次工作範圍。
後續加入投票模型前，必須先處理「日期變更不得沿用原投票」的規則，目前尚無投票資料可受影響。

### 部署前可觀測性待辦

目前先確定 Logging 原則，不立即導入 ELK 套件或基礎設施：

1. 應用程式持續使用 `ILogger`，避免商業程式碼綁定特定雲端或 Log 平台。
2. Docker 化時將 Log 改為包含 `trace.id`、`service.name` 等欄位的 ECS 結構化 JSON，輸出至 `stdout`／`stderr`，不只寫在 Container 內部檔案。
3. 本機 Docker 環境完成後，再以 Elastic Agent（或學習階段的 Filebeat）收集 Log，接到 Elasticsearch 與 Kibana 驗證集中查詢。
4. 選定 AWS／Azure／GCP 與部署方式後，再決定使用 Elastic Cloud 或自行管理 Elastic Stack，並設定保留期限、Lifecycle Policy、敏感資料遮罩與告警。
5. 此項排在「Docker 化」之後、「部署真人測試 MVP」之前；目前不阻塞活動、登入與投票功能開發。

## 啟動方式

在終端機進入前端資料夾：

```powershell
cd "C:\Users\Chloe SHI\Documents\ChatGPT\Side Project 揪團工具\nuxt-gather-match"
```

啟動開發環境：

```powershell
npm run dev
```

瀏覽器開啟：`http://localhost:3000/`

其他常用指令：

```powershell
npm run build
npm run preview
```

## 開新對話時可直接貼的文字

```text
請先閱讀專案根目錄的 PROJECT_STATUS.md、README.md、DATABASE_DESIGN_DRAFT.md，以及前後端目前的程式碼。
我們要延續「揪哪天？（Gathering Match）」專案。請保留 Nuxt 3 + TypeScript + Tailwind 與 ASP.NET Core 10 Web API + EF Core + Npgsql 架構；Nuxt 不直接操作 Supabase。

前端 Step 1～3 與主揪回覆管理 Demo 已完成。後端已建立 ASP.NET Core Identity，以及 ActivityType、City、District、Activity、DateOption、PlaceOption；InitialIdentity、AddActivityPlanning、EnableRowLevelSecurity 三個 Migration 都已套用至 Supabase PostgreSQL。

主揪註冊／登入／登出、活動建立、本人單筆查詢與名稱／既有候選日期修改已完成，65 個自動化測試與 197 項真實 API 檢查通過。請檢查實際程式碼、Git 狀態與 BACKEND_VERIFICATION.md，再依使用者的新任務決定範圍。暫時不要自行建立 Participant／Vote／FormationResult、Deadline 背景排程或 Google Places API。
```
