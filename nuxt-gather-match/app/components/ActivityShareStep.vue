<script setup lang="ts">
import type { ActivityDraft, PlaceOption } from "~/types/activity";

const props = defineProps<{
  activity: ActivityDraft;
  places: PlaceOption[];
}>();

const emit = defineEmits<{
  back: [];
  openJoin: [];
  openManage: [];
}>();
const copyFeedback = ref("");
const demoShareUrl = "gathermatch.tw/join/demo-0823";

const formattedDates = computed(() => props.activity.selectedDates.map((date) => {
  const value = new Date(`${date}T00:00:00`);
  return new Intl.DateTimeFormat("zh-TW", {
    month: "numeric",
    day: "numeric",
    weekday: "short",
  }).format(value);
}));

async function copyShareUrl() {
  try {
    await navigator.clipboard.writeText(`https://${demoShareUrl}`);
    copyFeedback.value = "已複製示意連結。";
  } catch {
    copyFeedback.value = "目前無法自動複製，請手動選取連結。";
  }
}
</script>

<template>
  <section class="rounded-[2rem] border border-stone-200/90 bg-white/90 p-6 shadow-card backdrop-blur sm:p-9" aria-labelledby="share-title">
    <div class="mb-6 text-center">
      <span class="mx-auto mb-4 grid h-14 w-14 place-items-center rounded-2xl bg-teal-soft text-2xl" aria-hidden="true">🎉</span>
      <p class="mb-3 text-xs font-black tracking-[0.18em] text-teal">STEP 03</p>
      <h2 id="share-title" class="text-3xl font-black tracking-tight">活動已準備好分享</h2>
      <p class="mt-2 text-sm leading-6 text-stone-500">這是純前端示意頁，連結尚未建立真實活動資料。</p>
    </div>

    <div class="rounded-2xl border border-stone-200 bg-paper p-5">
      <div class="flex items-start justify-between gap-3">
        <div>
          <p class="text-xs font-black tracking-[0.14em] text-coral">活動邀請</p>
          <h3 class="mt-1 text-xl font-black">{{ activity.activityName }}</h3>
        </div>
        <span class="rounded-full bg-cream px-3 py-1.5 text-xs font-bold text-stone-600">{{ activity.activityType }}</span>
      </div>

      <dl class="mt-5 space-y-4 text-sm">
        <div>
          <dt class="font-black text-stone-500">候選日期</dt>
          <dd class="mt-2 flex flex-wrap gap-2">
            <span v-for="date in formattedDates" :key="date" class="rounded-lg bg-white px-3 py-2 font-bold text-ink shadow-sm">{{ date }}</span>
          </dd>
        </div>
        <div>
          <dt class="font-black text-stone-500">候選地點</dt>
          <dd class="mt-2 space-y-2">
            <div v-for="place in places" :key="place.id" class="flex items-center gap-2 rounded-lg bg-white px-3 py-2 font-bold shadow-sm">
              <span class="text-coral" aria-hidden="true">●</span>
              {{ place.name }}
            </div>
          </dd>
        </div>
      </dl>
    </div>

    <div class="mt-5 rounded-2xl bg-teal-soft p-4">
      <label class="text-xs font-black tracking-[0.12em] text-teal" for="share-url">示意分享連結</label>
      <div class="mt-2 flex flex-col gap-2 sm:flex-row">
        <input id="share-url" class="min-w-0 flex-1 rounded-xl border border-teal/20 bg-white px-4 py-3 text-sm font-bold text-stone-600 outline-none" :value="demoShareUrl" readonly>
        <button class="rounded-xl bg-teal px-4 py-3 text-sm font-black text-white transition hover:-translate-y-0.5" type="button" @click="copyShareUrl">複製連結</button>
      </div>
      <p v-if="copyFeedback" class="mt-2 text-xs font-bold text-teal" role="status">{{ copyFeedback }}</p>
    </div>

    <div class="mt-6 grid gap-3 sm:grid-cols-2">
      <button class="rounded-xl border border-teal px-5 py-4 font-black text-teal transition hover:bg-teal hover:text-white" type="button" @click="emit('openManage')">查看主揪回覆管理</button>
      <button class="flex-1 rounded-xl bg-coral px-5 py-4 font-black text-white shadow-lg shadow-coral/20 transition hover:-translate-y-0.5 hover:bg-coral-dark focus:outline-none focus:ring-4 focus:ring-coral/20" type="button" @click="emit('openJoin')">
        模擬朋友開啟連結 →
      </button>
    </div>
    <button class="mt-3 w-full px-4 py-3 text-sm font-black text-stone-500 hover:text-ink" type="button" @click="emit('back')">← 返回修改地點</button>
  </section>
</template>
