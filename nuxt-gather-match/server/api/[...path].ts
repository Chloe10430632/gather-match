import { createError, defineEventHandler, getHeader, getRequestURL, proxyRequest, setResponseHeader } from "h3";

export default defineEventHandler((event) => {
  const url = getRequestURL(event);
  // Only the application's known endpoints can reach the fixed upstream.
  if (!/^\/api\/(auth\/(me|login|register|logout)|reference-data|activities(?:\/[1-9]\d*)?)$/.test(url.pathname)) {
    throw createError({ statusCode: 404 });
  }
  if (!["GET", "HEAD"].includes(event.method)) {
    // Browser clients must use our JSON API client. Cross-origin HTML forms cannot
    // supply this custom header; this server does not enable CORS.
    if (getHeader(event, "x-gather-match") !== "1" ||
        (getHeader(event, "origin") && getHeader(event, "origin") !== url.origin) ||
        getHeader(event, "sec-fetch-site") === "cross-site") {
      throw createError({ statusCode: 403, statusMessage: "Invalid request origin" });
    }
  }
  const base = useRuntimeConfig(event).apiBase.replace(/\/$/, "");
  return proxyRequest(event, `${base}${url.pathname}${url.search}`, {
    onResponse: () => { setResponseHeader(event, "Cache-Control", "no-store"); },
  });
});
