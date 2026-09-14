<script setup lang="ts">
import { computed, nextTick, ref } from "vue";
import type {
  ActivityDraft,
  DatePreference,
  DemoParticipantSession,
  PlaceOption,
  PlacePreference,
  ParticipantResponse,
} from "~/types/activity";

const currentStep = ref<1 | 2 | 3>(1);
const demoMode = ref(false);
const activityDraft = ref<ActivityDraft>({
  activityName: "",
  selectedDates: [],
  budget: "500-800",
  activityType: "聚餐",
  city: "",
  district: "",
});
const selectedPlaces = ref<PlaceOption[]>([]);
const stepThreeView = ref<"share" | "manage" | "join" | "date-vote" | "place-vote" | "response-progress">("share");
const participantName = ref("");
const datePreferences = ref<Record<string, DatePreference>>({});
const placePreferences = ref<Record<string, PlacePreference>>({});
const demoActivityShareToken = "demo-0823";
const demoSessionStorageKey = `gather-match:participant:${demoActivityShareToken}`;

const demoParticipantResponses = computed<ParticipantResponse[]>(() => {
  const dates = activityDraft.value.selectedDates;
  const places = selectedPlaces.value;
  const dateVote = (values: DatePreference[]) => Object.fromEntries(dates.map((date, index) => [date, values[index % values.length]]));
  const placeVote = (values: PlacePreference[]) => Object.fromEntries(places.map((place, index) => [place.id, values[index % values.length]]));

  return [
    { id: "demo-1", displayName: "小安", submittedAt: "今天 10:18", datePreferences: dateVote(["available", "available", "maybe"]), placePreferences: placeVote(["preferred", "acceptable", "acceptable"]) },
    { id: "demo-2", displayName: "Kevin", submittedAt: "今天 10:42", datePreferences: dateVote(["maybe", "available", "unavailable"]), placePreferences: placeVote(["acceptable", "preferred", "avoid"]) },
    { id: "demo-3", displayName: "Amy", submittedAt: "今天 11:05", datePreferences: dateVote(["available", "maybe", "available"]), placePreferences: placeVote(["preferred", "acceptable", "acceptable"]) },
    { id: "demo-4", displayName: "阿哲", submittedAt: "今天 11:26", datePreferences: dateVote(["unavailable", "available", "maybe"]), placePreferences: placeVote(["acceptable", "preferred", "acceptable"]) },
  ];
});

const heroContent = computed(() => currentStep.value === 1
  ? {
      eyebrow: "GATHERING MATCH",
      titleBefore: "別再問「大家",
      titleAccent: "哪天",
      titleAfter: "可以？」",
      description: "把日期、地點和大家的偏好集中起來。時間到，就替這群拖拖拉拉的人找出最容易成團的方案。",
    }
  : currentStep.value === 2 ? {
      eyebrow: "PLACE MATCHING",
      titleBefore: "把「去哪裡」也",
      titleAccent: "一起決定",
      titleAfter: "。",
      description: "系統先依活動條件提出候選，主揪可以採用推薦，也能直接加入心中已經想好的地點。",
    }
  : {
      eyebrow: "READY TO SHARE",
      titleBefore: "候選都好了，換朋友來",
      titleAccent: "投票",
      titleAfter: "。",
      description: "把活動連結丟進群組，朋友不用註冊就能填寫日期與地點偏好。先確認分享前看到的資訊是否夠清楚。",
    });

async function goToStep(step: 1 | 2 | 3) {
  currentStep.value = step;
  await nextTick();
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function confirmPlaces(places: PlaceOption[]) {
  selectedPlaces.value = places;
  stepThreeView.value = "share";
  goToStep(3);
}

function openJoinView() {
  const savedSession = readDemoParticipantSession();
  if (savedSession) {
    participantName.value = savedSession.participantName;
    datePreferences.value = savedSession.datePreferences;
    placePreferences.value = savedSession.placePreferences;
    stepThreeView.value = "response-progress";
    window.scrollTo({ top: 0, behavior: "smooth" });
    return;
  }

  stepThreeView.value = "join";
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function returnToShareView() {
  stepThreeView.value = "share";
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function openManageView() {
  stepThreeView.value = "manage";
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function openDateVote() {
  stepThreeView.value = "date-vote";
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function returnToJoinView() {
  stepThreeView.value = "join";
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function openPlaceVote() {
  stepThreeView.value = "place-vote";
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function returnToDateVote() {
  stepThreeView.value = "date-vote";
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function openResponseProgress() {
  stepThreeView.value = "response-progress";
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function returnToPlaceVote() {
  stepThreeView.value = "place-vote";
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function saveDemoParticipantSession() {
  const existingSession = readDemoParticipantSession();
  const session: DemoParticipantSession = {
    participantToken: existingSession?.participantToken ?? crypto.randomUUID(),
    participantName: participantName.value,
    datePreferences: datePreferences.value,
    placePreferences: placePreferences.value,
  };

  localStorage.setItem(demoSessionStorageKey, JSON.stringify(session));
}

function readDemoParticipantSession(): DemoParticipantSession | null {
  const savedValue = localStorage.getItem(demoSessionStorageKey);
  if (!savedValue) return null;

  try {
    const session = JSON.parse(savedValue) as Partial<DemoParticipantSession>;
    if (
      typeof session.participantToken !== "string"
      || typeof session.participantName !== "string"
      || !session.datePreferences
      || !session.placePreferences
    ) {
      localStorage.removeItem(demoSessionStorageKey);
      return null;
    }

    return session as DemoParticipantSession;
  } catch {
    localStorage.removeItem(demoSessionStorageKey);
    return null;
  }
}

function resetDemoParticipantSession() {
  localStorage.removeItem(demoSessionStorageKey);
  participantName.value = "";
  datePreferences.value = {};
  placePreferences.value = {};
  stepThreeView.value = "join";
  window.scrollTo({ top: 0, behavior: "smooth" });
}
</script>

<template>
  <div class="relative min-h-screen overflow-hidden bg-paper font-sans text-ink">
    <div class="pointer-events-none absolute -left-28 -top-28 h-96 w-96 rounded-full bg-coral/10 blur-3xl" />
    <div class="pointer-events-none absolute -right-24 top-24 h-80 w-80 rounded-full bg-teal/10 blur-3xl" />

    <header class="relative z-10 mx-auto flex w-[min(1180px,calc(100%-2.5rem))] items-center justify-between py-7">
      <a class="flex items-center gap-3 text-xl font-black tracking-tight" href="#top" aria-label="揪哪天首頁" @click.prevent="goToStep(1)">
        <span class="grid h-10 w-10 -rotate-3 place-items-center rounded-[14px_14px_14px_4px] bg-coral text-white shadow-lg shadow-coral/20">揪</span>
        <span>揪哪天?</span>
      </a>
    </header>
    <nav class="relative z-10 mx-auto flex w-[min(1180px,calc(100%-2.5rem))] gap-4" aria-label="使用模式">
      <button type="button" :aria-pressed="!demoMode" class="rounded-xl border border-teal px-4 py-2 font-bold text-teal" @click="demoMode = false">主揪活動管理</button>
      <button type="button" :aria-pressed="demoMode" class="rounded-xl border px-4 py-2" @click="demoMode = true">體驗投票 Demo（不會儲存到伺服器）</button>
    </nav>

    <main v-if="!demoMode" class="relative z-10 mx-auto max-w-3xl px-5 py-10"><HostWorkspace /></main>

    <main v-else id="top" class="relative z-10 mx-auto grid min-h-[650px] w-[min(1180px,calc(100%-2.5rem))] items-center gap-12 py-12 lg:grid-cols-[0.85fr_1.15fr] lg:gap-20 lg:py-16">
      <section class="text-center lg:text-left">
        <p class="mb-4 text-xs font-black tracking-[0.2em] text-teal">{{ heroContent.eyebrow }}</p>
        <h1 class="text-[clamp(2.8rem,5.2vw,4.7rem)] font-black leading-[1.1] tracking-[-0.065em]">
          {{ heroContent.titleBefore }}<span class="relative whitespace-nowrap text-coral after:absolute after:inset-x-0 after:bottom-0 after:-z-10 after:h-2 after:rounded-full after:bg-coral/20">{{ heroContent.titleAccent }}</span>{{ heroContent.titleAfter }}
        </h1>
        <p class="mx-auto my-7 max-w-xl text-lg leading-8 text-slate-600 lg:mx-0">{{ heroContent.description }}</p>
        <div class="flex flex-wrap justify-center gap-2 lg:justify-start" aria-label="產品特色">
          <span v-for="promise in ['免註冊投票', 'Deadline 自動結算', '未回覆也不阻塞']" :key="promise" class="rounded-xl bg-cream px-3 py-2 text-xs font-bold text-stone-600">{{ promise }}</span>
        </div>
      </section>

      <div>
        <ActivityProgress :current-step="currentStep" />
        <ActivitySetupForm v-show="currentStep === 1" v-model="activityDraft" @continue="goToStep(2)" />
        <PlaceSelectionStep v-show="currentStep === 2" :activity="activityDraft" @back="goToStep(1)" @confirm="confirmPlaces" />
        <ActivityShareStep
          v-if="currentStep === 3 && stepThreeView === 'share'"
          :activity="activityDraft"
          :places="selectedPlaces"
          @back="goToStep(2)"
          @open-join="openJoinView"
          @open-manage="openManageView"
        />
        <HostResponseManagementStep
          v-if="currentStep === 3 && stepThreeView === 'manage'"
          :activity="activityDraft"
          :participants="demoParticipantResponses"
          :places="selectedPlaces"
          @back="returnToShareView"
        />
        <ParticipantEntryStep
          v-if="currentStep === 3 && stepThreeView === 'join'"
          v-model="participantName"
          :activity="activityDraft"
          @back="returnToShareView"
          @continue="openDateVote"
        />
        <DateVotingStep
          v-if="currentStep === 3 && stepThreeView === 'date-vote'"
          v-model="datePreferences"
          :activity="activityDraft"
          :participant-name="participantName"
          @back="returnToJoinView"
          @continue="openPlaceVote"
        />
        <PlaceVotingStep
          v-if="currentStep === 3 && stepThreeView === 'place-vote'"
          v-model="placePreferences"
          :participant-name="participantName"
          :places="selectedPlaces"
          @back="returnToDateVote"
          @continue="openResponseProgress"
          @submitted="saveDemoParticipantSession"
        />
        <ResponseProgressStep
          v-if="currentStep === 3 && stepThreeView === 'response-progress'"
          :activity="activityDraft"
          :date-preferences="datePreferences"
          :participant-name="participantName"
          :place-preferences="placePreferences"
          :places="selectedPlaces"
          @back="returnToPlaceVote"
          @reset-demo-session="resetDemoParticipantSession"
        />
      </div>
    </main>
  </div>
</template>
