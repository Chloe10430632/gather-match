import type { ApiResponse } from "~/types/api";

export function useHostApi() {
  return async <T>(path: string, method: "GET" | "POST" | "PATCH" = "GET", body?: unknown) => {
    const result = await $fetch<ApiResponse<T>>(`/api/${path}`, {
      method, body: body as Record<string, unknown> | undefined,
      credentials: "same-origin", headers: { "X-Gather-Match": "1" }, retry: 0,
    });
    return result.data;
  };
}
