<script setup lang="ts">
import type { ActivityDraft, DatePreference, ParticipantResponse, PlaceOption, PlacePreference } from "~/types/activity";

const props = defineProps<{ activity: ActivityDraft; participants: ParticipantResponse[]; places: PlaceOption[] }>();
const emit = defineEmits<{ back: [] }>();

const dateLabels: Record<DatePreference, string> = { available: "可以", maybe: "看情況", unavailable: "不行" };
const placeLabels: Record<PlacePreference, string> = { preferred: "很想去", acceptable: "可以", avoid: "不想去" };

const dateSummaries = computed(() => props.activity.selectedDates.map((date) => ({
  id: date,
  label: formatDate(date),
  positive: props.participants.filter((participant) => participant.datePreferences[date] === "available").length,
  neutral: props.participants.filter((participant) => participant.datePreferences[date] === "maybe").length,
  negative: props.participants.filter((participant) => participant.datePreferences[date] === "unavailable").length,
})));

const placeSummaries = computed(() => props.places.map((place) => ({
  id: place.id,
  label: place.name,
  positive: props.participants.filter((participant) => participant.placePreferences[place.id] === "preferred").length,
  neutral: props.participants.filter((participant) => participant.placePreferences[place.id] === "acceptable").length,
  negative: props.participants.filter((participant) => participant.placePreferences[place.id] === "avoid").length,
})));

function formatDate(date: string) {
  return new Intl.DateTimeFormat("zh-TW", { month: "numeric", day: "numeric", weekday: "short" }).format(new Date(`${date}T00:00:00`));
}
</script>

<template>
  <section class="rounded-[2rem] border border-stone-200/90 bg-white/90 p-6 shadow-card backdrop-blur sm:p-9" aria-labelledby="manage-title">
    <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
      <div>
        <p class="mb-2 text-xs font-black tracking-[0.18em] text-teal">主揪回覆管理</p>
        <h2 id="manage-title" class="text-3xl font-black tracking-tight">{{ activity.activityName }}</h2>
        <p class="mt-2 text-sm leading-6 text-stone-500">以下為多位 Demo 參加者資料，用來確認主揪需要的資訊。</p>
      </div>
      <div class="shrink-0 rounded-2xl bg-coral-soft px-5 py-4 text-center">
        <p class="text-3xl font-black text-coral">{{ participants.length }}</p>
        <p class="text-xs font-black text-coral-dark">人已完成</p>
      </div>
    </div>

    <div class="mt-6 grid gap-4 lg:grid-cols-2">
      <div class="rounded-2xl border border-stone-200 bg-paper p-5">
        <h3 class="font-black">日期回覆概況</h3>
        <div class="mt-4 space-y-3">
          <div v-for="summary in dateSummaries" :key="summary.id" class="rounded-xl bg-white p-3 shadow-sm">
            <p class="font-black">{{ summary.label }}</p>
            <p class="mt-2 text-xs font-bold text-stone-500"><span class="text-teal">{{ summary.positive }} 可以</span> · {{ summary.neutral }} 看情況 · <span class="text-coral-dark">{{ summary.negative }} 不行</span></p>
          </div>
        </div>
      </div>

      <div class="rounded-2xl border border-stone-200 bg-paper p-5">
        <h3 class="font-black">地點回覆概況</h3>
        <div class="mt-4 space-y-3">
          <div v-for="summary in placeSummaries" :key="summary.id" class="rounded-xl bg-white p-3 shadow-sm">
            <p class="truncate font-black">{{ summary.label }}</p>
            <p class="mt-2 text-xs font-bold text-stone-500"><span class="text-teal">{{ summary.positive }} 很想去</span> · {{ summary.neutral }} 可以 · <span class="text-coral-dark">{{ summary.negative }} 不想去</span></p>
          </div>
        </div>
      </div>
    </div>

    <div class="mt-6">
      <div class="flex items-end justify-between gap-3">
        <h3 class="text-lg font-black">朋友回覆明細</h3>
        <span class="text-xs font-bold text-stone-500">依完成時間排列</span>
      </div>
      <div class="mt-3 space-y-3">
        <details v-for="participant in participants" :key="participant.id" class="group rounded-2xl border border-stone-200 bg-white p-4">
          <summary class="flex cursor-pointer list-none items-center justify-between gap-3">
            <div><p class="font-black">{{ participant.displayName }}</p><p class="mt-1 text-xs font-bold text-stone-400">{{ participant.submittedAt }} 完成</p></div>
            <span class="text-teal transition group-open:rotate-180" aria-hidden="true">⌄</span>
          </summary>
          <div class="mt-4 grid gap-4 border-t border-stone-100 pt-4 text-sm sm:grid-cols-2">
            <div>
              <p class="mb-2 text-xs font-black tracking-wide text-stone-400">日期</p>
              <p v-for="date in activity.selectedDates" :key="date" class="mb-1 flex justify-between gap-2"><span>{{ formatDate(date) }}</span><strong class="text-teal">{{ dateLabels[participant.datePreferences[date]!] }}</strong></p>
            </div>
            <div>
              <p class="mb-2 text-xs font-black tracking-wide text-stone-400">地點</p>
              <p v-for="place in places" :key="place.id" class="mb-1 flex justify-between gap-2"><span class="truncate">{{ place.name }}</span><strong class="shrink-0 text-teal">{{ placeLabels[participant.placePreferences[place.id]!] }}</strong></p>
            </div>
          </div>
        </details>
      </div>
    </div>

    <div class="mt-6 rounded-2xl bg-teal-soft p-4 text-xs font-bold leading-5 text-stone-600">此 Demo 沒有受邀名單，所以不顯示未回覆者或完成率；正式版需要後端提供參加者、送出時間與每個候選項目的偏好。</div>
    <button class="mt-5 w-full rounded-xl border border-teal px-5 py-4 font-black text-teal transition hover:bg-teal hover:text-white" type="button" @click="emit('back')">← 返回分享頁</button>
  </section>
</template>
