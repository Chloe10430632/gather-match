<script setup lang="ts">
import type { ActivityDraft } from "~/types/activity";

defineProps<{ activity: ActivityDraft }>();

const participantName = defineModel<string>({ required: true });
const emit = defineEmits<{
  back: [];
  continue: [];
}>();
const nameInput = ref(participantName.value);
const feedback = ref("");
const hasJoined = ref(Boolean(participantName.value.trim()));

function joinActivity() {
  const name = nameInput.value.trim();

  if (!name) {
    feedback.value = "請先輸入你的名字。";
    return;
  }

  participantName.value = name;
  feedback.value = "";
  hasJoined.value = true;
}

function fillTestData() {
  nameInput.value = "Chloe";
  feedback.value = "";
}
</script>

<template>
  <section class="rounded-[2rem] border border-stone-200/90 bg-white/90 p-6 shadow-card backdrop-blur sm:p-9" aria-labelledby="join-title">
    <div class="mb-6">
      <p class="mb-3 text-xs font-black tracking-[0.18em] text-teal">朋友投票 · 免註冊</p>
      <h2 id="join-title" class="text-3xl font-black tracking-tight">你收到一個活動邀請</h2>
      <p class="mt-2 text-sm leading-6 text-stone-500">輸入群組裡大家認得的名字，就可以開始填寫偏好。</p>
    </div>

    <div class="rounded-2xl border border-stone-200 bg-paper p-5">
      <div class="flex items-center gap-3">
        <span class="grid h-11 w-11 shrink-0 place-items-center rounded-xl bg-coral-soft text-xl" aria-hidden="true">👋</span>
        <div>
          <p class="text-xs font-black text-coral">邀請你一起決定</p>
          <h3 class="mt-1 text-xl font-black">{{ activity.activityName }}</h3>
        </div>
      </div>
      <div class="mt-4 flex flex-wrap gap-2 text-xs font-bold">
        <span class="rounded-full bg-white px-3 py-1.5 text-stone-600">{{ activity.selectedDates.length }} 個候選日期</span>
        <span class="rounded-full bg-white px-3 py-1.5 text-stone-600">{{ activity.city }} {{ activity.district }}</span>
        <span class="rounded-full bg-white px-3 py-1.5 text-stone-600">{{ activity.activityType }}</span>
      </div>
    </div>

    <form v-if="!hasJoined" class="mt-6" @submit.prevent="joinActivity">
      <div class="flex items-center justify-between gap-3">
        <label class="block text-sm font-black" for="participant-name">你的名字</label>
        <button class="rounded-full border border-dashed border-teal px-3 py-1.5 text-xs font-black text-teal transition hover:bg-teal-soft" type="button" @click="fillTestData">⚡ 帶入測試名字</button>
      </div>
      <input
        id="participant-name"
        v-model="nameInput"
        class="mt-2 w-full rounded-xl border border-stone-200 bg-paper px-4 py-3 outline-none placeholder:text-stone-400 focus:border-teal focus:ring-4 focus:ring-teal/10"
        maxlength="20"
        placeholder="例如：Chloe"
        type="text"
        autocomplete="name"
        :aria-describedby="feedback ? 'participant-name-feedback' : undefined"
        :aria-invalid="feedback ? 'true' : undefined"
        @input="feedback = ''"
      >
      <p v-if="feedback" id="participant-name-feedback" class="mt-2 text-sm font-bold text-coral-dark" role="alert">{{ feedback }}</p>
      <p v-else class="mt-2 text-xs text-stone-500">不需要建立帳號，名字只用來辨識這次回覆。</p>

      <button class="mt-5 w-full rounded-xl bg-coral px-5 py-4 font-black text-white shadow-lg shadow-coral/20 transition hover:-translate-y-0.5 hover:bg-coral-dark focus:outline-none focus:ring-4 focus:ring-coral/20" type="submit">
        開始填寫偏好 →
      </button>
    </form>

    <div v-else class="mt-6 rounded-2xl bg-teal-soft p-5 text-center" role="status">
      <p class="text-lg font-black text-teal">嗨，{{ participantName }}！</p>
      <p class="mt-1 text-sm text-stone-600">名字已記下，下一步會從候選日期開始投票。</p>
      <button class="mt-4 w-full rounded-xl bg-teal px-5 py-4 font-black text-white transition hover:-translate-y-0.5 hover:bg-teal-dark focus:outline-none focus:ring-4 focus:ring-teal/20" type="button" @click="emit('continue')">
        填寫日期偏好 →
      </button>
    </div>

    <button class="mt-5 px-2 py-3 text-sm font-black text-stone-500 hover:text-ink" type="button" @click="emit('back')">← 返回活動邀請</button>
  </section>
</template>
