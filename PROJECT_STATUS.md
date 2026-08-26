# 揪哪天？（Gathering Match）— 專案進度交接

最後更新：2026-08-26

## 專案位置

- 工作區根目錄：`Side Project 揪團工具`
- Nuxt 前端：`nuxt-gather-match/`
- 技術：Nuxt 3、Vue 3、TypeScript、Tailwind CSS

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
- 尚未建立 ASP.NET Core Web API、資料庫或 Deadline 背景排程。
- 尚未建立真正的 Nuxt 公開分享路由；目前朋友入口仍是同一頁面的元件切換。
- 示意分享連結尚未建立或載入真實活動資料。
- 活動、朋友與投票內容都尚未寫入資料庫。
- `localStorage` 目前同時保存示意 Token 與投票內容，只供前端流程驗證；正式版瀏覽器只應保存參加者識別碼，投票內容由後端保存與驗證。
- 正式版需要兩種識別：網址中的活動 `shareToken`，以及朋友瀏覽器保存的 `participantToken`。資料庫只保存 `participantToken` 的安全雜湊值。
- 清除瀏覽器資料、使用無痕模式或更換裝置時，免登入參加者可能無法自動找回原身分；這是 V0.1 可接受但需清楚說明的限制。
- 公開連結目前沒有預設受邀名單或預計人數，因此回覆進度只顯示「幾人已完成」，不顯示不可靠的分母。
- 目前 Git 工作目錄內的專案檔案尚未建立第一次 commit。

## 建議下一步

Step 3 與主揪回覆管理的純前端 UI／UX 已完成。下一次建議整理正式前後端資料流與最小資料模型，不急著串 Google Places API：

1. 將主揪管理頁與朋友公開投票頁拆成清楚的 Nuxt 路由草案，例如 `/activities/:id/manage` 與 `/join/:shareToken`。
2. 確認 `User → Activity → Participant → DateVote／PlaceVote` 的關聯與必要欄位。
3. 定義 `shareToken`、`participantToken` 的產生、保存、雜湊與驗證方式。
4. 先寫最小 RESTful API 契約，再決定是否開始建立 ASP.NET Core Web API。
5. 依已完成的主揪回覆管理畫面，確認 API 需提供回覆人數、完成時間與逐候選項目偏好。

建議下一個小步驟優先處理第 2 項：確認 `User → Activity → Participant → DateVote／PlaceVote` 的最小關聯與必要欄位，再據此撰寫 API 契約。

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

今天請從 PROJECT_STATUS.md 的「建議下一步」開始，用引導式、小步驟的方式繼續。Step 3 的朋友投票純前端流程已完成，下一步優先討論並製作「主揪查看朋友回覆」的純前端管理畫面；不要先串後端或 Google Places API。
```
