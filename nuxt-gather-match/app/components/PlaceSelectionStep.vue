<script setup lang="ts">
import { computed, ref } from "vue";
import type { ActivityDraft, PlaceOption } from "~/types/activity";

interface DisplayPlace extends PlaceOption {
  icon: string;
}

const props = defineProps<{ activity: ActivityDraft }>();
const emit = defineEmits<{
  back: [];
  confirm: [places: PlaceOption[]];
}>();

const recommendedPlaces: DisplayPlace[] = [
  {
    id: "recommended-1",
    name: "日光共享空間",
    category: "室內多功能空間",
    price: "約 NT$ 600／人",
    description: "座位彈性、交通方便，適合聊天、聚會與小型活動。",
    source: "recommended",
    accent: "bg-amber-100 text-amber-700",
    icon: "☀️",
  },
  {
    id: "recommended-2",
    name: "河岸活動廣場",
    category: "戶外開放空間",
    price: "免費",
    description: "空間寬敞、適合散步和戶外活動，周邊也有休息區。",
    source: "recommended",
    accent: "bg-teal-soft text-teal",
    icon: "🌿",
  },
  {
    id: "recommended-3",
    name: "聚點複合空間",
    category: "複合式活動場地",
    price: "約 NT$ 800／人",
    description: "室內空間完整，可因應聚餐、桌遊或主題活動。",
    source: "recommended",
    accent: "bg-violet-100 text-violet-700",
    icon: "✨",
  },
];

const customPlaces = ref<DisplayPlace[]>([]);
const selectedPlaceIds = ref<string[]>([]);
const customPlaceName = ref("");
const feedback = ref("");
const allPlaces = computed(() => [...recommendedPlaces, ...customPlaces.value]);

const budgetLabel = computed(() => ({
  "under-500": "NT$ 500 以下",
  "500-800": "NT$ 500–800",
  "800-1200": "NT$ 800–1,200",
  "over-1200": "NT$ 1,200 以上",
}[props.activity.budget] ?? props.activity.budget));

function togglePlace(placeId: string) {
  if (selectedPlaceIds.value.includes(placeId)) {
    selectedPlaceIds.value = selectedPlaceIds.value.filter((id) => id !== placeId);
    feedback.value = "";
    return;
  }

  if (selectedPlaceIds.value.length >= 5) {
    feedback.value = "候選地點最多 5 個。";
    return;
  }

  selectedPlaceIds.value = [...selectedPlaceIds.value, placeId];
  feedback.value = "";
}

function addCustomPlace() {
  const placeName = customPlaceName.value.trim();
  if (!placeName) {
    feedback.value = "請先輸入地點名稱。";
    return;
  }

  if (allPlaces.value.some((place) => place.name.toLowerCase() === placeName.toLowerCase())) {
    feedback.value = "這個地點已經在候選清單中。";
    return;
  }

  if (selectedPlaceIds.value.length >= 5) {
    feedback.value = "候選地點最多 5 個。";
    return;
  }

  const customPlace: DisplayPlace = {
    id: `custom-${Date.now()}`,
    name: placeName,
    category: "主揪自訂地點",
    price: "待確認",
    description: `由主揪加入，地點位於 ${props.activity.city}${props.activity.district} 或附近。`,
    source: "custom",
    accent: "bg-coral-soft text-coral-dark",
    icon: "📍",
  };

  customPlaces.value.push(customPlace);
  selectedPlaceIds.value.push(customPlace.id);
  customPlaceName.value = "";
  feedback.value = "已加入自訂地點。";
}

function fillTestData() {
  selectedPlaceIds.value = recommendedPlaces.slice(0, 3).map((place) => place.id);
  customPlaceName.value = "";
  feedback.value = "已帶入 3 個測試候選地點。";
}

function confirmPlaces() {
  if (selectedPlaceIds.value.length === 0) {
    feedback.value = "請至少加入 1 個候選地點。";
    return;
  }

  const selectedPlaces = allPlaces.value
    .filter((place) => selectedPlaceIds.value.includes(place.id))
    .map(({ icon: _icon, ...place }) => place);

  emit("confirm", selectedPlaces);
}
</script>

<template>
  <section class="rounded-[2rem] border border-stone-200/90 bg-white/90 p-6 shadow-card backdrop-blur sm:p-9" aria-labelledby="places-title">
    <div class="mb-6 flex items-start justify-between gap-4">
      <div>
        <p class="mb-3 text-xs font-black tracking-[0.18em] text-teal">STEP 02</p>
        <h2 id="places-title" class="text-3xl font-black tracking-tight">挑選候選地點</h2>
        <p class="mt-2 text-sm text-stone-500">先用示意推薦確認流程，之後再串接真實地點資料。</p>
      </div>
      <div class="flex shrink-0 flex-col items-end gap-2">
        <span class="rounded-full bg-amber-100 px-3 py-1.5 text-[11px] font-black text-amber-700">DEMO DATA</span>
        <button class="rounded-full border border-dashed border-teal px-3 py-1.5 text-xs font-black text-teal transition hover:bg-teal-soft" type="button" @click="fillTestData">⚡ 自動選擇</button>
      </div>
    </div>

    <div class="mb-5 flex flex-wrap gap-2 text-xs font-bold">
      <span class="rounded-full bg-teal-soft px-3 py-1.5 text-teal">{{ activity.city }} {{ activity.district }}</span>
      <span class="rounded-full bg-cream px-3 py-1.5 text-stone-600">{{ activity.activityType }}</span>
      <span class="rounded-full bg-cream px-3 py-1.5 text-stone-600">{{ budgetLabel }}</span>
    </div>

    <div class="space-y-3">
      <button
        v-for="place in allPlaces"
        :key="place.id"
        class="flex w-full items-start gap-3 rounded-2xl border p-4 text-left transition hover:-translate-y-0.5 hover:shadow-md"
        :class="selectedPlaceIds.includes(place.id) ? 'border-coral bg-coral-soft/60' : 'border-stone-200 bg-paper'"
        :aria-pressed="selectedPlaceIds.includes(place.id)"
        type="button"
        @click="togglePlace(place.id)"
      >
        <span class="grid h-11 w-11 shrink-0 place-items-center rounded-xl text-xl" :class="place.accent">{{ place.icon }}</span>
        <span class="min-w-0 flex-1">
          <span class="flex flex-wrap items-center gap-2">
            <strong class="text-base">{{ place.name }}</strong>
            <small v-if="place.source === 'custom'" class="rounded-full bg-white px-2 py-0.5 font-bold text-coral-dark">自訂</small>
          </span>
          <span class="mt-1 block text-xs font-bold text-teal">{{ place.category }} · {{ place.price }}</span>
          <span class="mt-1.5 block text-xs leading-5 text-stone-500">{{ place.description }}</span>
        </span>
        <span class="grid h-6 w-6 shrink-0 place-items-center rounded-full border text-xs" :class="selectedPlaceIds.includes(place.id) ? 'border-coral bg-coral text-white' : 'border-stone-300 text-transparent'">✓</span>
      </button>
    </div>

    <div class="my-6 flex items-center gap-3 text-xs font-bold text-stone-400">
      <span class="h-px flex-1 bg-stone-200" />
      已經有想去的地方？
      <span class="h-px flex-1 bg-stone-200" />
    </div>

    <form class="flex flex-col gap-2 sm:flex-row" @submit.prevent="addCustomPlace">
      <label class="flex-1">
        <span class="sr-only">自訂地點名稱</span>
        <input v-model="customPlaceName" class="w-full rounded-xl border border-stone-200 bg-paper px-4 py-3 outline-none placeholder:text-stone-400 focus:border-teal focus:ring-4 focus:ring-teal/10" placeholder="輸入地點名稱或貼上 Google Maps 連結" type="text">
      </label>
      <button class="rounded-xl border border-teal px-4 py-3 text-sm font-black text-teal transition hover:bg-teal hover:text-white" type="submit">＋ 加入候選</button>
    </form>

    <div class="mt-6 flex items-center justify-between gap-3">
      <button class="px-2 py-3 text-sm font-black text-stone-500 hover:text-ink" type="button" @click="emit('back')">← 返回修改</button>
      <span class="text-xs font-bold text-stone-500">已選 {{ selectedPlaceIds.length }}／5</span>
    </div>
    <button class="mt-2 flex w-full items-center justify-center gap-2 rounded-xl bg-coral px-5 py-4 font-black text-white shadow-lg shadow-coral/20 transition hover:-translate-y-0.5 hover:bg-coral-dark focus:outline-none focus:ring-4 focus:ring-coral/20" type="button" @click="confirmPlaces">
      確認候選地點 <span aria-hidden="true">→</span>
    </button>
    <p v-if="feedback" class="mt-3 text-center text-sm font-bold" :class="feedback.startsWith('已') ? 'text-teal' : 'text-coral-dark'" role="status">{{ feedback }}</p>
    <p v-else class="mt-3 text-center text-xs text-stone-500">推薦與自訂合計最多 5 個候選地點</p>
  </section>
</template>
