<script setup lang="ts">
import type { ActivityDraft, DatePreference, PlaceOption, PlacePreference } from "~/types/activity";

const props = defineProps<{
  activity: ActivityDraft;
  participantName: string;
  places: PlaceOption[];
  datePreferences: Record<string, DatePreference>;
  placePreferences: Record<string, PlacePreference>;
}>();

const emit = defineEmits<{
  back: [];
  resetDemoSession: [];
}>();

const dateLabels: Record<DatePreference, string> = {
  available: "可以",
  maybe: "看情況",
  unavailable: "不行",
};

const placeLabels: Record<PlacePreference, string> = {
  preferred: "很想去",
  acceptable: "可以",
  avoid: "不想去",
};

const preferredDateCount = computed(() => Object.values(props.datePreferences)
  .filter((preference) => preference === "available").length);

const preferredPlaceCount = computed(() => Object.values(props.placePreferences)
  .filter((preference) => preference === "preferred").length);

function formatDate(date: string) {
  return new Intl.DateTimeFormat("zh-TW", {
    month: "numeric",
    day: "numeric",
    weekday: "short",
  }).format(new Date(`${date}T00:00:00`));
}
</script>

<template>
  <section class="rounded-[2rem] border border-stone-200/90 bg-white/90 p-6 shadow-card backdrop-blur sm:p-9" aria-labelledby="progress-title">
    <div class="text-center">
      <span class="mx-auto grid h-14 w-14 place-items-center rounded-full bg-teal text-2xl text-white shadow-lg shadow-teal/20" aria-hidden="true">✓</span>
      <p class="mb-3 mt-5 text-xs font-black tracking-[0.18em] text-teal">回覆已送出</p>
      <h2 id="progress-title" class="text-3xl font-black tracking-tight">謝謝你，{{ participantName }}！</h2>
      <p class="mt-2 text-sm leading-6 text-stone-500">主揪已經能看到你的回覆，截止後系統會根據所有已回覆資料整理方案。</p>
    </div>

    <div class="my-6 rounded-2xl bg-coral-soft p-5 text-center">
      <p class="text-xs font-black tracking-[0.14em] text-coral-dark">目前回覆進度</p>
      <p class="mt-2 text-4xl font-black text-coral">1 <span class="text-lg text-coral-dark">人已完成</span></p>
      <p class="mt-2 text-xs leading-5 text-stone-600">公開連結沒有預設受邀名單，因此目前只顯示已完成的人數。</p>
    </div>

    <details class="group rounded-2xl border border-stone-200 bg-paper p-5">
      <summary class="flex cursor-pointer list-none items-center justify-between font-black">
        檢查我的回覆
        <span class="text-teal transition group-open:rotate-180" aria-hidden="true">⌄</span>
      </summary>

      <div class="mt-5 space-y-5 border-t border-stone-200 pt-5 text-sm">
        <div>
          <div class="flex items-center justify-between gap-3">
            <h3 class="font-black">日期偏好</h3>
            <span class="text-xs font-bold text-stone-500">{{ preferredDateCount }} 天可以</span>
          </div>
          <ul class="mt-2 space-y-2">
            <li v-for="date in activity.selectedDates" :key="date" class="flex items-center justify-between gap-3 rounded-lg bg-white px-3 py-2">
              <span class="font-bold">{{ formatDate(date) }}</span>
              <span class="text-xs font-black text-teal">{{ dateLabels[datePreferences[date]!] }}</span>
            </li>
          </ul>
        </div>

        <div>
          <div class="flex items-center justify-between gap-3">
            <h3 class="font-black">地點偏好</h3>
            <span class="text-xs font-bold text-stone-500">{{ preferredPlaceCount }} 個很想去</span>
          </div>
          <ul class="mt-2 space-y-2">
            <li v-for="place in places" :key="place.id" class="flex items-center justify-between gap-3 rounded-lg bg-white px-3 py-2">
              <span class="min-w-0 truncate font-bold">{{ place.name }}</span>
              <span class="shrink-0 text-xs font-black text-teal">{{ placeLabels[placePreferences[place.id]!] }}</span>
            </li>
          </ul>
        </div>
      </div>
    </details>

    <button class="mt-5 w-full rounded-xl border border-teal px-5 py-4 font-black text-teal transition hover:bg-teal hover:text-white" type="button" @click="emit('back')">修改我的回覆</button>
    <button class="mt-2 w-full px-4 py-3 text-xs font-black text-stone-400 transition hover:text-coral-dark" type="button" @click="emit('resetDemoSession')">清除這台裝置的示意身分，重新測試</button>
  </section>
</template>
