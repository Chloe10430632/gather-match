import type { Config } from "tailwindcss";

export default <Partial<Config>>{
  theme: {
    extend: {
      colors: {
        ink: "#263B3A",
        coral: {
          DEFAULT: "#F16F5C",
          dark: "#D85343",
          soft: "#FFF1ED",
        },
        teal: {
          DEFAULT: "#1F7770",
          soft: "#E9F5F2",
        },
        cream: "#F7F0E4",
        paper: "#FFFDF8",
      },
      boxShadow: {
        card: "0 28px 70px rgba(65, 55, 44, 0.13)",
      },
      fontFamily: {
        sans: ["Noto Sans TC", "PingFang TC", "Microsoft JhengHei", "sans-serif"],
      },
    },
  },
};
