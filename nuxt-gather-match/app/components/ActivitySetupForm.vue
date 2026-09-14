<script setup lang="ts">
import { watch } from "vue";
import type { ActivityDraft } from "~/types/activity";

const draft = defineModel<ActivityDraft>({ required: true });
const emit = defineEmits<{ continue: [] }>();
const activityNameError = ref("");
const datesError = ref("");
const locationError = ref("");

watch(() => draft.value.activityName, (activityName) => {
  if (activityName.trim()) activityNameError.value = "";
});

watch(() => draft.value.selectedDates, (selectedDates) => {
  if (selectedDates.length > 0) datesError.value = "";
});

watch([() => draft.value.city, () => draft.value.district], ([city, district]) => {
  if (city && district) locationError.value = "";
});

function continueToPlaces() {
  activityNameError.value = draft.value.activityName.trim() ? "" : "請輸入活動名稱";
  datesError.value = draft.value.selectedDates.length > 0 ? "" : "請至少選擇一個日期";
  locationError.value = draft.value.city && draft.value.district ? "" : "請選擇縣市與行政區";

  if (activityNameError.value || datesError.value || locationError.value) return;

  emit("continue");
}

function fillTestData() {
  const futureDates = [7, 10, 14].map((daysToAdd) => {
    const date = new Date();
    date.setDate(date.getDate() + daysToAdd);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  });

  draft.value = {
    activityName: "週末朋友聚會",
    selectedDates: futureDates,
    budget: "500-800",
    activityType: "聚餐",
    city: "台北市",
    district: "中山區",
  };
  activityNameError.value = "";
  datesError.value = "";
  locationError.value = "";
}
</script>

<template>
  <section class="rounded-[2rem] border border-stone-200/90 bg-white/90 p-6 shadow-card backdrop-blur sm:p-10" aria-labelledby="create-title">
    <div class="mb-7 flex items-start justify-between gap-4">
      <div>
        <p class="mb-3 text-xs font-black tracking-[0.18em] text-teal">STEP 01</p>
        <h2 id="create-title" class="text-3xl font-black tracking-tight">先開一場活動</h2>
      </div>
      <div class="flex items-center gap-3">
        <button class="rounded-full border border-dashed border-teal px-3 py-1.5 text-xs font-black text-teal transition hover:bg-teal-soft" type="button" @click="fillTestData">⚡ 帶入測試資料</button>
        <span class="hidden rotate-6 text-4xl sm:block" aria-hidden="true">🥂</span>
      </div>
    </div>

    <form class="space-y-5" @submit.prevent="continueToPlaces">
      <label class="block">
        <span class="mb-2 block text-sm font-bold text-slate-700">活動名稱 <small class="text-coral-dark">*</small></span>
        <input
          v-model="draft.activityName"
          class="w-full rounded-xl border bg-paper px-4 py-3.5 outline-none transition placeholder:text-stone-400 focus:ring-4"
          :class="activityNameError ? 'border-coral-dark focus:border-coral-dark focus:ring-coral/10' : 'border-stone-200 focus:border-teal focus:ring-teal/10'"
          :aria-invalid="Boolean(activityNameError)"
          aria-describedby="activity-name-error"
          placeholder="例如：8 月姐妹聚會"
          type="text"
        >
        <p v-if="activityNameError" id="activity-name-error" class="mt-1.5 text-xs font-bold text-coral-dark">{{ activityNameError }}</p>
      </label>

      <DateMultiPicker v-model="draft.selectedDates" :error="datesError" />

      <div class="grid gap-3 sm:grid-cols-2">
        <label>
          <span class="mb-2 block text-sm font-bold text-slate-700">每人預算</span>
          <select v-model="draft.budget" class="w-full rounded-xl border border-stone-200 bg-paper px-4 py-3.5 outline-none focus:border-teal focus:ring-4 focus:ring-teal/10">
            <option value="under-500">NT$ 500 以下</option>
            <option value="500-800">NT$ 500–800</option>
            <option value="800-1200">NT$ 800–1,200</option>
            <option value="over-1200">NT$ 1,200 以上</option>
          </select>
        </label>
        <label>
          <span class="mb-2 block text-sm font-bold text-slate-700">活動類型</span>
          <select v-model="draft.activityType" class="w-full rounded-xl border border-stone-200 bg-paper px-4 py-3.5 outline-none focus:border-teal focus:ring-4 focus:ring-teal/10">
            <option>聚餐</option>
            <option>咖啡聊天</option>
            <option>戶外活動</option>
            <option>看展／電影</option>
            <option>旅行</option>
            <option>其他</option>
          </select>
        </label>
      </div>

      <LocationSelector v-model:city="draft.city" v-model:district="draft.district" :error="locationError" />

      <button class="flex w-full items-center justify-center gap-2 rounded-xl bg-coral px-5 py-4 font-black text-white shadow-lg shadow-coral/20 transition hover:-translate-y-0.5 hover:bg-coral-dark focus:outline-none focus:ring-4 focus:ring-coral/20" type="submit">
        下一步：推薦適合地點 <span aria-hidden="true">→</span>
      </button>
      <p class="text-center text-xs text-stone-500">還不用註冊，1 分鐘就能建立活動</p>
    </form>
  </section>
</template>
