<script setup lang="ts">

interface CalendarDay {
  day: number;
  dateKey: string;
  isPast: boolean;
}

defineProps<{ error?: string }>();
const selectedDates = defineModel<string[]>({ required: true });
const now = new Date();
const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
const calendarCursor = ref(new Date(now.getFullYear(), now.getMonth(), 1));
const showCalendar = ref(false);
const weekDays = ["日", "一", "二", "三", "四", "五", "六"];

const calendarTitle = computed(() =>
  new Intl.DateTimeFormat("zh-TW", { year: "numeric", month: "long" }).format(calendarCursor.value),
);
const canGoToPreviousMonth = computed(() =>
  calendarCursor.value > new Date(today.getFullYear(), today.getMonth(), 1),
);
const calendarDays = computed<(CalendarDay | null)[]>(() => {
  const year = calendarCursor.value.getFullYear();
  const month = calendarCursor.value.getMonth();
  const firstWeekday = new Date(year, month, 1).getDay();
  const totalDays = new Date(year, month + 1, 0).getDate();
  const days: (CalendarDay | null)[] = Array.from({ length: firstWeekday }, () => null);

  for (let day = 1; day <= totalDays; day += 1) {
    const date = new Date(year, month, day);
    days.push({ day, dateKey: toDateKey(date), isPast: date < today });
  }

  return days;
});

function toDateKey(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function formatSelectedDate(dateKey: string) {
  const [year, month, day] = dateKey.split("-").map(Number);
  const weekday = new Intl.DateTimeFormat("zh-TW", { weekday: "short" }).format(new Date(year, month - 1, day));
  return `${month}/${day} ${weekday}`;
}

function toggleDate(dateKey: string) {
  selectedDates.value = selectedDates.value.includes(dateKey)
    ? selectedDates.value.filter((id) => id !== dateKey)
    : [...selectedDates.value, dateKey].sort();
}

function changeMonth(offset: number) {
  calendarCursor.value = new Date(
    calendarCursor.value.getFullYear(),
    calendarCursor.value.getMonth() + offset,
    1,
  );
}
</script>

<template>
  <fieldset>
    <div class="mb-2 flex items-end justify-between gap-3">
      <div>
        <legend class="text-sm font-bold text-slate-700">我有空的日期 <small class="text-coral-dark">*</small></legend>
        <p class="mt-1 text-xs text-stone-500">可複選，日期不必連續</p>
      </div>
      <button v-if="selectedDates.length" class="text-xs font-black text-coral-dark hover:underline" type="button" @click="showCalendar = !showCalendar">
        {{ showCalendar ? "收起月曆" : "調整日期" }}
      </button>
    </div>

    <button
      v-if="selectedDates.length === 0 && !showCalendar"
      class="flex w-full items-center justify-center gap-2 rounded-2xl border-2 border-dashed bg-paper px-4 py-5 text-sm font-black text-stone-500 transition hover:border-coral hover:bg-coral-soft hover:text-coral-dark"
      :class="error ? 'border-coral-dark' : 'border-stone-300'"
      :aria-invalid="Boolean(error)"
      aria-describedby="dates-error"
      type="button"
      @click="showCalendar = true"
    >
      <span class="grid h-7 w-7 place-items-center rounded-full bg-white text-xl shadow-sm">＋</span>
      選擇可行日期
    </button>

    <p v-if="error" id="dates-error" class="mt-1.5 text-xs font-bold text-coral-dark">{{ error }}</p>

    <div v-if="selectedDates.length" class="mb-3 flex flex-wrap gap-2">
      <button
        v-for="dateKey in selectedDates"
        :key="dateKey"
        class="group flex items-center gap-1.5 rounded-full bg-coral-soft px-3 py-1.5 text-xs font-bold text-coral-dark"
        :aria-label="`移除 ${formatSelectedDate(dateKey)}`"
        type="button"
        @click="toggleDate(dateKey)"
      >
        {{ formatSelectedDate(dateKey) }}
        <span class="text-coral/70 group-hover:text-coral-dark" aria-hidden="true">×</span>
      </button>
    </div>

    <div v-if="showCalendar" class="rounded-2xl border border-stone-200 bg-paper p-3 sm:p-4">
      <div class="mb-3 flex items-center justify-between">
        <button class="grid h-9 w-9 place-items-center rounded-full text-lg text-slate-600 transition hover:bg-white disabled:cursor-not-allowed disabled:opacity-25" :disabled="!canGoToPreviousMonth" aria-label="上一個月" type="button" @click="changeMonth(-1)">‹</button>
        <strong class="text-sm">{{ calendarTitle }}</strong>
        <button class="grid h-9 w-9 place-items-center rounded-full text-lg text-slate-600 transition hover:bg-white" aria-label="下一個月" type="button" @click="changeMonth(1)">›</button>
      </div>

      <div class="grid grid-cols-7 text-center">
        <span v-for="weekday in weekDays" :key="weekday" class="py-1 text-[11px] font-bold text-stone-400">{{ weekday }}</span>
        <template v-for="(calendarDay, index) in calendarDays" :key="calendarDay?.dateKey ?? `empty-${index}`">
          <span v-if="!calendarDay" />
          <button
            v-else
            class="mx-auto grid h-9 w-9 place-items-center rounded-full text-sm font-bold transition"
            :class="[
              selectedDates.includes(calendarDay.dateKey) ? 'bg-coral text-white shadow-md shadow-coral/20' : 'text-slate-700 hover:bg-coral-soft hover:text-coral-dark',
              calendarDay.isPast ? 'cursor-not-allowed opacity-25' : '',
            ]"
            :disabled="calendarDay.isPast"
            :aria-pressed="selectedDates.includes(calendarDay.dateKey)"
            type="button"
            @click="toggleDate(calendarDay.dateKey)"
          >
            {{ calendarDay.day }}
          </button>
        </template>
      </div>
    </div>
  </fieldset>
</template>
