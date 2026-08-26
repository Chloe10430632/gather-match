<script setup lang="ts">
import type { ActivityDraft, DatePreference } from "~/types/activity";

const props = defineProps<{
  activity: ActivityDraft;
  participantName: string;
}>();

const datePreferences = defineModel<Record<string, DatePreference>>({ required: true });
const emit = defineEmits<{
  back: [];
  continue: [];
}>();
const feedback = ref("");
const submitted = ref(false);

const preferenceOptions: Array<{
  value: DatePreference;
  label: string;
  icon: string;
  selectedClass: string;
}> = [
  { value: "available", label: "可以", icon: "✓", selectedClass: "border-teal bg-teal-soft text-teal" },
  { value: "maybe", label: "看情況", icon: "?", selectedClass: "border-amber-400 bg-amber-50 text-amber-700" },
  { value: "unavailable", label: "不行", icon: "×", selectedClass: "border-coral bg-coral-soft text-coral-dark" },
];

const completedCount = computed(() => props.activity.selectedDates.filter((date) => datePreferences.value[date]).length);
const isComplete = computed(() => completedCount.value === props.activity.selectedDates.length);

function formatDate(date: string) {
  return new Intl.DateTimeFormat("zh-TW", {
    month: "numeric",
    day: "numeric",
    weekday: "long",
  }).format(new Date(`${date}T00:00:00`));
}

function choosePreference(date: string, preference: DatePreference) {
  datePreferences.value = {
    ...datePreferences.value,
    [date]: preference,
  };
  feedback.value = "";
  submitted.value = false;
}

function submitDatePreferences() {
  if (!isComplete.value) {
    feedback.value = `還有 ${props.activity.selectedDates.length - completedCount.value} 個日期尚未選擇。`;
    return;
  }

  submitted.value = true;
  feedback.value = "";
}

function fillTestData() {
  const testPreferences: DatePreference[] = ["available", "maybe", "unavailable"];
  datePreferences.value = Object.fromEntries(
    props.activity.selectedDates.map((date, index) => [date, testPreferences[index % testPreferences.length]]),
  );
  feedback.value = "";
  submitted.value = false;
}
</script>

<template>
  <section class="rounded-[2rem] border border-stone-200/90 bg-white/90 p-6 shadow-card backdrop-blur sm:p-9" aria-labelledby="date-vote-title">
    <div class="mb-6 flex items-start justify-between gap-4">
      <div>
        <p class="mb-3 text-xs font-black tracking-[0.18em] text-teal">朋友投票 · 日期</p>
        <h2 id="date-vote-title" class="text-3xl font-black tracking-tight">{{ participantName }}，哪些日期可以？</h2>
        <p class="mt-2 text-sm leading-6 text-stone-500">每個日期都選一個答案，讓主揪不用再逐一確認。</p>
      </div>
      <div class="flex shrink-0 flex-col items-end gap-2">
        <span class="rounded-full bg-teal-soft px-3 py-1.5 text-xs font-black text-teal">{{ completedCount }}／{{ activity.selectedDates.length }}</span>
        <button class="rounded-full border border-dashed border-teal px-3 py-1.5 text-xs font-black text-teal transition hover:bg-teal-soft" type="button" @click="fillTestData">⚡ 自動填寫</button>
      </div>
    </div>

    <div class="space-y-4">
      <fieldset v-for="date in activity.selectedDates" :key="date" class="rounded-2xl border border-stone-200 bg-paper p-4">
        <legend class="px-1 text-base font-black">{{ formatDate(date) }}</legend>
        <div class="mt-3 grid grid-cols-3 gap-2">
          <label v-for="option in preferenceOptions" :key="option.value" class="cursor-pointer">
            <input
              class="sr-only"
              type="radio"
              :name="`date-${date}`"
              :value="option.value"
              :checked="datePreferences[date] === option.value"
              @change="choosePreference(date, option.value)"
            >
            <span
              class="flex min-h-20 flex-col items-center justify-center rounded-xl border bg-white px-2 py-3 text-center text-sm font-black transition hover:-translate-y-0.5 hover:shadow-sm"
              :class="datePreferences[date] === option.value ? option.selectedClass : 'border-stone-200 text-stone-500'"
            >
              <span class="mb-1 text-lg" aria-hidden="true">{{ option.icon }}</span>
              {{ option.label }}
            </span>
          </label>
        </div>
      </fieldset>
    </div>

    <p v-if="feedback" class="mt-4 text-center text-sm font-bold text-coral-dark" role="alert">{{ feedback }}</p>

    <div v-if="submitted" class="mt-5 rounded-2xl bg-teal-soft p-5 text-center" role="status">
      <p class="font-black text-teal">日期偏好已記下</p>
      <p class="mt-1 text-sm text-stone-600">下一步會接著選擇候選地點偏好。</p>
      <button class="mt-4 w-full rounded-xl bg-teal px-5 py-4 font-black text-white transition hover:-translate-y-0.5 focus:outline-none focus:ring-4 focus:ring-teal/20" type="button" @click="emit('continue')">填寫地點偏好 →</button>
    </div>
    <button v-else class="mt-5 w-full rounded-xl bg-coral px-5 py-4 font-black text-white shadow-lg shadow-coral/20 transition hover:-translate-y-0.5 hover:bg-coral-dark focus:outline-none focus:ring-4 focus:ring-coral/20" type="button" @click="submitDatePreferences">
      確認日期偏好 →
    </button>

    <button class="mt-3 px-2 py-3 text-sm font-black text-stone-500 hover:text-ink" type="button" @click="emit('back')">← 返回修改名字</button>
  </section>
</template>
