# 揪哪天？（Gathering Match）— 專案進度交接

最後更新：2026-08-26

## 專案位置

- 工作區根目錄：`Side Project 揪團工具`
- Nuxt 前端：`nuxt-gather-match/`
- 前端技術：Nuxt 3、Vue 3、TypeScript、Tailwind CSS
- 後端規劃：ASP.NET Core 8 Web API、EF Core Code First、Npgsql
- 資料庫規劃：Supabase Free 代管的 PostgreSQL

## 目前做到哪裡

目前正在製作 MVP 的前端操作流程，尚未串接後端或真實地點 API。

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

- 使用 ASP.NET Core 8 Web API 作為唯一的應用程式後端，Nuxt 不直接操作 Supabase。
- Supabase 第一階段只作為代管 PostgreSQL，不先加入 Supabase Auth、Realtime、Storage、Edge Functions 或 Data API。
- 使用 Entity Framework Core 作為 ORM，透過 `Npgsql.EntityFrameworkCore.PostgreSQL` 連接 PostgreSQL。
- 採用「先設計資料模型，再用 Code First 實作」：
  1. 先確認 ERD、欄位、關聯、限制與索引。
  2. 建立 C# Entity、`DbContext` 與 Fluent API 設定。
  3. 以 EF Core Migration 建立及更新 Supabase PostgreSQL Schema。
- 正式 Schema 不以 Supabase Table Editor 手動修改為主要流程，避免資料庫與 Migration 紀錄不同步。
- Connection string 應保存於 .NET User Secrets 或部署環境變數，不可提交至 GitHub。
- Supabase Free 目前足以支援開發與小規模 Demo；需要不中斷服務、自動備份或超過免費額度時才評估升級。

## 重要檔案

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
- 尚未建立 ASP.NET Core 8 Web API、Supabase project、EF Core Entity／Migration 或 Deadline 背景排程。
- 尚未建立真正的 Nuxt 公開分享路由；目前朋友入口仍是同一頁面的元件切換。
- 示意分享連結尚未建立或載入真實活動資料。
- 活動、朋友與投票內容都尚未寫入資料庫。
- `localStorage` 目前同時保存示意 Token 與投票內容，只供前端流程驗證；正式版瀏覽器只應保存參加者識別碼，投票內容由後端保存與驗證。
- 正式版需要兩種識別：網址中的活動 `shareToken`，以及朋友瀏覽器保存的 `participantToken`。資料庫只保存 `participantToken` 的安全雜湊值。
- 清除瀏覽器資料、使用無痕模式或更換裝置時，免登入參加者可能無法自動找回原身分；這是 V0.1 可接受但需清楚說明的限制。
- 公開連結目前沒有預設受邀名單或預計人數，因此回覆進度只顯示「幾人已完成」，不顯示不可靠的分母。
- GitHub 已建立 Private repository：`Chloe10430632/gather-match`，本機 `main` 已追蹤 `origin/main`。

## 建議下一步

Step 3 與主揪回覆管理的純前端 UI／UX 已完成；後端與資料庫技術方向也已確認。下一次先建立 ASP.NET Core 8 Web API 空專案，再整理最小資料模型，不急著串 Google Places API：

1. 使用者在 Visual Studio 2026 建立 ASP.NET Core 8 Web API 空專案後，確認專案位置、範本選項與目前檔案。
2. 確認 `User → Activity → DateOption／PlaceOption／Participant → DateVote／PlaceVote` 的關聯與必要欄位。
3. 定義 `shareToken`、`participantToken` 的產生、保存、雜湊與驗證方式。
4. 建立 C# Entity、`DbContext` 與 Fluent API 關聯設定。
5. 加入 Npgsql，建立並檢查第一個 EF Core Migration，再套用至 Supabase PostgreSQL。
6. 依資料模型撰寫最小 RESTful API 契約。
7. 後續再將主揪管理頁與朋友公開投票頁拆成 `/activities/:id/manage` 與 `/join/:shareToken` 等正式 Nuxt 路由。

建議下一個小步驟：使用者建立 ASP.NET Core 8 Web API 空專案後，先檢查專案設定，再一起設計最小 ERD；不要先寫 Controller 或直接在 Supabase 手動建立正式資料表。

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

今天請從 PROJECT_STATUS.md 的「建議下一步」開始，用引導式、小步驟的方式繼續。前端 Step 1～3 與主揪回覆管理 Demo 已完成；後端採 ASP.NET Core 8 Web API，資料庫採 Supabase PostgreSQL，ORM 採 EF Core Code First + Migration。

請先檢查新建立的後端專案設定，再一起設計最小 ERD、欄位與關聯。不要先寫 Controller、直接手動建立正式資料表，或串 Google Places API。
```
