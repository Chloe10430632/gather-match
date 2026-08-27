# 揪哪天？（Gathering Match）— 專案進度交接

最後更新：2026-08-27

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

- 建立 ASP.NET Core 10 Web API 專案，Target Framework 為 `net10.0`。
- 後端目前可成功建置及啟動，最近一次結果為 0 個警告、0 個錯誤。
- 已建立 Supabase project，但尚未建立 Gathering Match 正式資料表。
- 已透過 .NET User Secrets 保存 Supabase PostgreSQL Session pooler Connection String；未將密碼寫入專案檔案。
- 已安裝並固定以下套件版本：
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore` `10.0.11`
  - `Microsoft.EntityFrameworkCore.Design` `10.0.11`
  - `Npgsql.EntityFrameworkCore.PostgreSQL` `10.0.3`
- `dotnet-ef` CLI 已更新為 `10.0.11`。
- 已決定主揪登入採 ASP.NET Core Identity，不使用 Supabase Auth，也不自行實作密碼雜湊。
- 已建立 `ApplicationUser : IdentityUser<long>`，目前新增 `DisplayName` 欄位。
- 已建立 `ApplicationDbContext : IdentityUserContext<ApplicationUser, long>`；第一階段不加入 Roles／UserRoles。
- 已使用 Fluent API 將 `DisplayName` 設為必填且最多 50 字。
- `Program.cs` 已註冊 Npgsql、`ApplicationDbContext`、Identity、Authentication 與 Authorization。
- 尚未加入 `MapIdentityApi<ApplicationUser>()`，因此還沒有開放註冊／登入 endpoints。
- 已決定 C# 與 PostgreSQL 資料表／欄位統一使用 PascalCase，不加入 `EFCore.NamingConventions`。

## 重要檔案

- `DATABASE_DESIGN_DRAFT.md`：最小 ERD、資料表欄位、關聯、Identity 與外部資料保存策略草案。
- `asp-gather-match/asp-gather-match/asp-gather-match/Models/ApplicationUser.cs`：ASP.NET Core Identity 使用者 Entity。
- `asp-gather-match/asp-gather-match/asp-gather-match/Data/ApplicationDbContext.cs`：Identity／EF Core DbContext 與 Fluent API。
- `asp-gather-match/asp-gather-match/asp-gather-match/Program.cs`：Npgsql、Identity、Authentication 與 Authorization 服務註冊。
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
- 已建立 ASP.NET Core 10 Web API 與 Supabase project；尚未建立第一個 EF Core Migration、Gathering Match 業務 Entity 或 Deadline 背景排程。
- 尚未建立真正的 Nuxt 公開分享路由；目前朋友入口仍是同一頁面的元件切換。
- 示意分享連結尚未建立或載入真實活動資料。
- 活動、朋友與投票內容都尚未寫入資料庫。
- `localStorage` 目前同時保存示意 Token 與投票內容，只供前端流程驗證；正式版瀏覽器只應保存參加者識別碼，投票內容由後端保存與驗證。
- 正式版需要兩種識別：網址中的活動 `shareToken`，以及朋友瀏覽器保存的 `participantToken`。資料庫只保存 `participantToken` 的安全雜湊值。
- 清除瀏覽器資料、使用無痕模式或更換裝置時，免登入參加者可能無法自動找回原身分；這是 V0.1 可接受但需清楚說明的限制。
- 公開連結目前沒有預設受邀名單或預計人數，因此回覆進度只顯示「幾人已完成」，不顯示不可靠的分母。
- GitHub 已建立 Private repository：`Chloe10430632/gather-match`，本機 `main` 已追蹤 `origin/main`。

## 建議下一步

ASP.NET Core Identity、EF Core、Npgsql、User Secrets 與 `ApplicationDbContext` 基礎已完成。下次延續引導式開發，由使用者親手輸入程式碼：

1. 在 `ApplicationUser` 補上尚未實作的 `IsActive`、`CreatedAt`、`UpdatedAt`。
2. 建置並檢查實際程式碼。
3. 建立第一個 `InitialIdentity` Migration，但先不要執行 `database update`。
4. 閱讀 Migration，確認只包含預期的 Identity User、Claim、Login、Token 資料表，沒有 Roles／UserRoles。
5. 確認表名與欄位維持 PascalCase，以及 `DisplayName` 的必要性與長度限制。
6. 檢查 Migration 後，再由使用者決定是否套用至 Supabase PostgreSQL。
7. Identity Schema 確認後，再逐一建立 `Activity` 等 Gathering Match 業務 Entity。

建議下一個小步驟：補完 `ApplicationUser` 的三個專案欄位；不要直接執行 `dotnet ef database update`、手動建立 Supabase 正式資料表或串 Google Places API。

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
請先閱讀專案根目錄的 PROJECT_STATUS.md、README.md，以及 nuxt-gather-match 目前的程式碼。
我們要延續「揪哪天？（Gathering Match）」專案。請保留現在的 Nuxt 3 + TypeScript + Tailwind 架構，Vue 狀態盡量使用 ref，不要使用 reactive；也不要先串後端或 Google Places API。

今天請從 PROJECT_STATUS.md 的「建議下一步」開始，用引導式、小步驟的方式繼續，由我親手輸入後端程式碼。前端 Step 1～3 與主揪回覆管理 Demo 已完成；後端採 ASP.NET Core 10 Web API、ASP.NET Core Identity、EF Core Code First + Migration 與 Npgsql，資料庫採 Supabase PostgreSQL。

後端已完成 `ApplicationUser`、`ApplicationDbContext`、User Secrets 與 Identity／Npgsql 服務註冊，但尚未建立 Migration 或正式資料表。請先檢查實際程式碼，再從補上 `ApplicationUser` 的 `IsActive`、`CreatedAt`、`UpdatedAt` 繼續；不要直接執行 `database update`、先寫 Controller、手動建立正式資料表或串 Google Places API。
```
