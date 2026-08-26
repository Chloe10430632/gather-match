<script setup lang="ts">
import type { PlaceOption, PlacePreference } from "~/types/activity";

const props = defineProps<{
  participantName: string;
  places: PlaceOption[];
}>();

const placePreferences = defineModel<Record<string, PlacePreference>>({ required: true });
const emit = defineEmits<{
  back: [];
  continue: [];
  submitted: [];
}>();
const feedback = ref("");
const submitted = ref(false);

const preferenceOptions: Array<{
  value: PlacePreference;
  label: string;
  icon: string;
  selectedClass: string;
}> = [
  { value: "preferred", label: "很想去", icon: "♥", selectedClass: "border-coral bg-coral-soft text-coral-dark" },
  { value: "acceptable", label: "可以", icon: "✓", selectedClass: "border-teal bg-teal-soft text-teal" },
  { value: "avoid", label: "不想去", icon: "×", selectedClass: "border-stone-500 bg-stone-100 text-stone-700" },
];

const completedCount = computed(() => props.places.filter((place) => placePreferences.value[place.id]).length);
const isComplete = computed(() => completedCount.value === props.places.length);

function choosePreference(placeId: string, preference: PlacePreference) {
  placePreferences.value = {
    ...placePreferences.value,
    [placeId]: preference,
  };
  feedback.value = "";
  submitted.value = false;
}

function submitPlacePreferences() {
  if (!isComplete.value) {
    feedback.value = `還有 ${props.places.length - completedCount.value} 個地點尚未選擇。`;
    return;
  }

  submitted.value = true;
  feedback.value = "";
  emit("submitted");
}

function fillTestData() {
  const testPreferences: PlacePreference[] = ["preferred", "acceptable", "avoid"];
  placePreferences.value = Object.fromEntries(
    props.places.map((place, index) => [place.id, testPreferences[index % testPreferences.length]]),
  );
  feedback.value = "";
  submitted.value = false;
}
</script>

<template>
  <section class="rounded-[2rem] border border-stone-200/90 bg-white/90 p-6 shadow-card backdrop-blur sm:p-9" aria-labelledby="place-vote-title">
    <div class="mb-6 flex items-start justify-between gap-4">
      <div>
        <p class="mb-3 text-xs font-black tracking-[0.18em] text-teal">朋友投票 · 地點</p>
        <h2 id="place-vote-title" class="text-3xl font-black tracking-tight">{{ participantName }}，比較想去哪裡？</h2>
        <p class="mt-2 text-sm leading-6 text-stone-500">每個地點都選一個偏好，不必只選出唯一首選。</p>
      </div>
      <div class="flex shrink-0 flex-col items-end gap-2">
        <span class="rounded-full bg-teal-soft px-3 py-1.5 text-xs font-black text-teal">{{ completedCount }}／{{ places.length }}</span>
        <button class="rounded-full border border-dashed border-teal px-3 py-1.5 text-xs font-black text-teal transition hover:bg-teal-soft" type="button" @click="fillTestData">⚡ 自動填寫</button>
      </div>
    </div>

    <div class="space-y-4">
      <fieldset v-for="place in places" :key="place.id" class="rounded-2xl border border-stone-200 bg-paper p-4">
        <legend class="max-w-full px-1">
          <span class="block truncate text-base font-black">{{ place.name }}</span>
          <span class="mt-1 block text-xs font-bold text-teal">{{ place.category }} · {{ place.price }}</span>
        </legend>
        <p class="mt-2 text-xs leading-5 text-stone-500">{{ place.description }}</p>
        <div class="mt-3 grid grid-cols-3 gap-2">
          <label v-for="option in preferenceOptions" :key="option.value" class="cursor-pointer">
            <input
              class="sr-only"
              type="radio"
              :name="`place-${place.id}`"
              :value="option.value"
              :checked="placePreferences[place.id] === option.value"
              @change="choosePreference(place.id, option.value)"
            >
            <span
              class="flex min-h-20 flex-col items-center justify-center rounded-xl border bg-white px-2 py-3 text-center text-sm font-black transition hover:-translate-y-0.5 hover:shadow-sm"
              :class="placePreferences[place.id] === option.value ? option.selectedClass : 'border-stone-200 text-stone-500'"
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
      <span class="mx-auto grid h-12 w-12 place-items-center rounded-full bg-teal text-xl text-white" aria-hidden="true">✓</span>
      <p class="mt-3 text-lg font-black text-teal">回覆完成！</p>
      <p class="mt-1 text-sm text-stone-600">日期和地點偏好都已記下，謝謝你幫忙讓這次聚會成團。</p>
      <button class="mt-4 w-full rounded-xl bg-teal px-5 py-4 font-black text-white transition hover:-translate-y-0.5 focus:outline-none focus:ring-4 focus:ring-teal/20" type="button" @click="emit('continue')">查看回覆進度 →</button>
    </div>
    <button v-else class="mt-5 w-full rounded-xl bg-coral px-5 py-4 font-black text-white shadow-lg shadow-coral/20 transition hover:-translate-y-0.5 hover:bg-coral-dark focus:outline-none focus:ring-4 focus:ring-coral/20" type="button" @click="submitPlacePreferences">
      送出這次回覆
    </button>

    <button class="mt-3 px-2 py-3 text-sm font-black text-stone-500 hover:text-ink" type="button" @click="emit('back')">← 返回修改日期偏好</button>
  </section>
</template>
