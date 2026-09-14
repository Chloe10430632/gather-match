export default defineNuxtConfig({
  compatibilityDate: "2026-08-24",
  srcDir: "app/",
  serverDir: "server/",
  runtimeConfig: { apiBase: "http://localhost:5138" },
  devtools: { enabled: true },
  modules: ["@nuxtjs/tailwindcss"],
  css: ["~/assets/css/tailwind.css"],
  app: {
    head: {
      htmlAttrs: { lang: "zh-Hant" },
      title: "揪哪天?｜讓聚會真的約得成",
      link: [{ rel: "icon", type: "image/svg+xml", href: "/favicon.svg" }],
      meta: [
        {
          name: "description",
          content: "快速決定哪天、去哪裡，讓聚會不再停在群組訊息裡。",
        },
      ],
    },
  },
});
