<script setup lang="ts">
import { taiwanLocations } from "~/data/taiwanLocations";

defineProps<{ error?: string }>();
const selectedCity = defineModel<string>("city", { required: true });
const selectedDistrict = defineModel<string>("district", { required: true });
const cityOptions = Object.keys(taiwanLocations);
const districtOptions = computed(() =>
  selectedCity.value ? taiwanLocations[selectedCity.value] ?? [] : [],
);

watch(selectedCity, () => {
  selectedDistrict.value = "";
});
</script>

<template>
  <fieldset>
    <legend class="text-sm font-bold text-slate-700">活動地區 <small class="text-coral-dark">*</small></legend>
    <p class="mb-2 mt-1 text-xs text-stone-500">作為下一步推薦候選地點的搜尋範圍</p>
    <div class="grid gap-3 sm:grid-cols-2">
      <label>
        <span class="sr-only">縣市</span>
        <select v-model="selectedCity" class="w-full rounded-xl border bg-paper px-4 py-3.5 outline-none focus:ring-4" :class="error ? 'border-coral-dark focus:border-coral-dark focus:ring-coral/10' : 'border-stone-200 focus:border-teal focus:ring-teal/10'" :aria-invalid="Boolean(error)" aria-describedby="location-error">
          <option disabled value="">選擇縣市</option>
          <option v-for="city in cityOptions" :key="city" :value="city">{{ city }}</option>
        </select>
      </label>
      <label>
        <span class="sr-only">行政區</span>
        <select v-model="selectedDistrict" class="w-full rounded-xl border bg-paper px-4 py-3.5 outline-none focus:ring-4 disabled:cursor-not-allowed disabled:bg-stone-100 disabled:text-stone-400" :class="error ? 'border-coral-dark focus:border-coral-dark focus:ring-coral/10' : 'border-stone-200 focus:border-teal focus:ring-teal/10'" :disabled="!selectedCity" :aria-invalid="Boolean(error)" aria-describedby="location-error">
          <option disabled value="">{{ selectedCity ? "選擇行政區" : "請先選擇縣市" }}</option>
          <option v-for="district in districtOptions" :key="district" :value="district">{{ district }}</option>
        </select>
      </label>
    </div>
    <p v-if="error" id="location-error" class="mt-1.5 text-xs font-bold text-coral-dark">{{ error }}</p>
  </fieldset>
</template>
