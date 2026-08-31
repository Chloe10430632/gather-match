# 台灣縣市與行政區參照資料方案

最後確認：2026-08-31

## 目的

Gather Match 需要自己的 `Cities`、`Districts` 參照資料，讓建立活動時可以使用 Foreign Key 驗證縣市與行政區。應用程式平常讀取自己的 PostgreSQL，不在每次使用者操作時呼叫政府 API。

## 官方來源

資料提供機關：內政部國土測繪中心（NLSC）。

- 縣市清單：`https://api.nlsc.gov.tw/other/ListCounty`
- 鄉鎮市區清單（戶政）：`https://api.nlsc.gov.tw/other/ListTown1/{countycode}`
- 回傳格式：XML
- 授權：政府資料開放授權條款第 1 版
- 更新頻率：不定期

## 欄位對應

### City

| 官方欄位 | 範例 | 專案欄位 | 用途 |
| --- | --- | --- | --- |
| `countycode` | `A` | 不保存 | 只用來呼叫 `ListTown1/A` |
| `countycode01` | `63000` | `City.GovernmentCode` | 穩定的五碼行政代碼 |
| `countyname` | `臺北市` | `City.Name` | 官方名稱 |

### District

| 官方欄位 | 範例 | 專案欄位 | 用途 |
| --- | --- | --- | --- |
| `towncode` | `63000010` | `District.GovernmentCode` | 穩定的八碼行政代碼 |
| `towncode01` | `A01` | 不保存 | 官方 API 之間傳遞用的短代碼 |
| `townname` | `松山區` | `District.Name` | 官方名稱 |

資料庫仍使用數字 `Id` 作為 Primary Key；`GovernmentCode` 是唯一且穩定的外部識別，不使用名稱當作關聯鍵。

## 匯入方式決策

不將全臺縣市／行政區全部直接寫在 `ApplicationDbContext.HasData`，原因是資料筆數多、官方資料可能調整，會讓 `DbContext` 與 Model Snapshot 過度膨脹。

採用以下流程：

1. 維護程式從官方 API 下載資料。
2. 轉換成專案內版本化的 UTF-8 JSON 快照。
3. 人工檢查新增、改名與停用差異。
4. 以 Transaction 執行可重複匯入：依 `GovernmentCode` 新增或更新資料。
5. 官方資料中消失的項目改為 `IsActive = false`，不直接刪除，保留歷史 Activity 關聯。
6. 寫入完成後驗證縣市數量、行政區數量、重複代碼與 City／District 關係。

正式部署不在應用程式每次啟動時自動下載政府資料，也不讓 Nuxt 直接呼叫官方 API。

## 名稱注意事項

官方名稱使用「臺」，例如 `臺北市`、`臺中市`、`臺南市`、`臺東縣`。目前前端 Demo 使用部分「台」字名稱；前後端串接時應改以資料庫 ID／API Response 為準，不使用名稱作為識別鍵。

## 已完成的匯入工具

已建立：

- `scripts/Update-LocationReferenceSnapshot.ps1`：下載 NLSC XML、檢查代碼與重複值，產生 UTF-8 JSON 快照。
- `Data/SeedData/taiwan-location-reference.json`：2026-08-31 產生的完整快照，包含 22 個縣市與 368 個行政區。
- `Data/LocationReferenceImporter.cs`：驗證完整快照，使用 Transaction 依 `GovernmentCode` 新增／更新，並將官方來源中消失的資料停用。

驗證快照：

```powershell
dotnet run -- validate-locations .\Data\SeedData\taiwan-location-reference.json
```

匯入 Supabase：

```powershell
dotnet run -- import-locations .\Data\SeedData\taiwan-location-reference.json
```

匯入命令會輸出新增、更新、停用與總筆數。2026-08-31 已完成第一次正式匯入：新增 22 個縣市與 368 個行政區；第二次執行新增、更新、停用皆為 0，確認資料不會重複插入。

第一版不加入自動排程，也不在正式 API Request 中即時同步官方資料。
