<script setup lang="ts">
defineProps<{ currentStep: number }>();

const steps = ["建立活動", "確認地點", "朋友投票", "真的約出去"];
</script>

<template>
  <section class="mb-4 rounded-2xl border border-stone-200/90 bg-white/80 px-4 py-4 shadow-sm backdrop-blur sm:px-6" aria-label="活動建立進度">
    <div class="mb-4 flex items-center justify-between gap-4">
      <p class="text-xs font-black tracking-[0.18em] text-teal">建立活動進度</p>
      <span class="rounded-full bg-teal/10 px-3 py-1.5 text-xs font-black text-teal">STEP {{ String(currentStep).padStart(2, "0") }} / 04</span>
    </div>

    <ol class="grid grid-cols-4 gap-1 sm:gap-3">
      <li v-for="(step, index) in steps" :key="step" class="relative min-w-0 text-center">
        <div class="mb-2 flex items-center" aria-hidden="true">
          <span v-if="index > 0" class="h-0.5 flex-1" :class="index + 1 <= currentStep ? 'bg-coral' : 'bg-stone-200'" />
          <span
            class="grid h-7 w-7 shrink-0 place-items-center rounded-full text-[11px] font-black transition"
            :class="index + 1 <= currentStep ? 'bg-coral text-white shadow-sm shadow-coral/20' : 'bg-stone-100 text-stone-400'"
          >
            {{ index + 1 }}
          </span>
          <span v-if="index < steps.length - 1" class="h-0.5 flex-1" :class="index + 1 < currentStep ? 'bg-coral' : 'bg-stone-200'" />
        </div>
        <span class="block truncate text-[10px] font-bold sm:text-xs" :class="index + 1 <= currentStep ? 'text-ink' : 'text-stone-400'">{{ step }}</span>
      </li>
    </ol>
  </section>
</template>
