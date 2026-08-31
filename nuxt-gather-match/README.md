# nuxt-gather-match

「揪哪天?（Gathering Match）」的 Nuxt 3 前端專案。

## Current Status

- 已完成 Step 1～3 與主揪回覆管理的純前端 Demo。
- 活動與投票資料目前仍保存在前端狀態及 `localStorage`，尚未呼叫後端 API。
- ASP.NET Core 後端已完成 Identity、活動規劃資料模型、Supabase PostgreSQL Schema 與「新增活動」API 第一版。
- 下一個整合階段會先完成主揪註冊／登入，再由這個前端送出活動、候選日期與候選地點。

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

## Production

```bash
npm run build
npm run preview
```
