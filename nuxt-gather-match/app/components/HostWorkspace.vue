<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import type { HostActivity, HostUser, ReferenceData } from "~/types/api";

const api = useHostApi();
const user = ref<HostUser | null>(null);
const references = ref<ReferenceData | null>(null);
const busy = ref(false);
const ready = ref(false);
const error = ref("");
const notice = ref("");
const register = ref(false);
const email = ref("");
const password = ref("");
const displayName = ref("");
const activity = ref<HostActivity | null>(null);
const lookupId = ref("");
const shareToken = ref("");
const title = ref("");
const dates = ref<string[]>([]);
const activityTypeId = ref<number | "">("");
const cityId = ref<number | "">("");
const districtId = ref<number | "">("");
const budgetMin = ref<number | "">(500);
const budgetMax = ref<number | "">(800);
const deadline = ref("");
const places = ref([{ displayLabel: "", customAddress: "" }]);
const editTitle = ref("");
const editDates = ref<HostActivity["dateOptions"]>([]);
const clock = ref(Date.now());
let timer: ReturnType<typeof setInterval> | undefined;
const districts = computed(() => references.value?.cities.find(x => x.id === cityId.value)?.districts ?? []);
const editable = computed(() => activity.value?.status === "open" && Date.parse(activity.value.deadlineAt) > clock.value);
watch(cityId, () => { districtId.value = ""; });

function showActivity(value: HostActivity) {
  activity.value = value;
  editTitle.value = value.title;
  editDates.value = value.dateOptions.map(x => ({ ...x }));
  lookupId.value = String(value.id);
  const url = new URL(window.location.href);
  url.searchParams.set("activity", String(value.id));
  window.history.replaceState(null, "", url);
}

async function run(action: () => Promise<void>) {
  if (busy.value) return;
  busy.value = true;
  error.value = "";
  notice.value = "";
  try { await action(); }
  catch (cause: unknown) {
    const failure = cause as { statusCode?: number; data?: { error?: { message: string; details?: Record<string, string[]> } } };
    const detail = failure.data?.error;
    error.value = detail ? [detail.message, ...Object.values(detail.details ?? {}).flat()].join(" ") : "無法完成請求，請確認後端服務已啟動後重試。";
    if (failure.statusCode === 401) {
      user.value = null;
      activity.value = null;
      shareToken.value = "";
      error.value = detail?.message ?? "登入已失效，請重新登入。";
    }
  } finally { busy.value = false; }
}

async function loadActivity() {
  if (!/^[1-9]\d*$/.test(lookupId.value)) { error.value = "請輸入有效的活動編號。"; return; }
  const value = await api<HostActivity>(`activities/${lookupId.value}`);
  if (activity.value?.id !== value.id) shareToken.value = "";
  showActivity(value);
}

async function authenticate() {
  await run(async () => {
    if (register.value) {
      await api("auth/register", "POST", { email: email.value, password: password.value, displayName: displayName.value });
      register.value = false;
      notice.value = "帳號已建立。";
    }
    user.value = await api<HostUser>("auth/login", "POST", { email: email.value, password: password.value });
    password.value = "";
    if (lookupId.value) await loadActivity();
  });
}

async function logout() {
  await run(async () => {
    await api("auth/logout", "POST", {});
    user.value = null;
    activity.value = null;
    shareToken.value = "";
    password.value = "";
    notice.value = "已登出。";
  });
}

async function createActivity() {
  if (!dates.value.length) { error.value = "請至少選擇一個候選日期。"; return; }
  if (!deadline.value || Date.parse(deadline.value) <= Date.now()) { error.value = "截止時間必須晚於現在。"; return; }
  await run(async () => {
    const created = await api<{ id: number; shareToken: string }>("activities", "POST", {
      title: title.value, activityTypeId: activityTypeId.value, cityId: cityId.value,
      districtId: districtId.value === "" ? null : districtId.value,
      budgetMin: budgetMin.value === "" ? null : budgetMin.value,
      budgetMax: budgetMax.value === "" ? null : budgetMax.value,
      deadlineAt: new Date(deadline.value).toISOString(),
      dateOptions: dates.value.map(optionDate => ({ optionDate })),
      placeOptions: places.value,
    });
    shareToken.value = created.shareToken;
    lookupId.value = String(created.id);
    editDates.value = [];
    const url = new URL(window.location.href);
    url.searchParams.set("activity", String(created.id));
    window.history.replaceState(null, "", url);
    // A successful create is never retried if the following read fails.
    activity.value = { id: created.id, title: title.value, activityTypeId: Number(activityTypeId.value),
      cityId: Number(cityId.value), districtId: districtId.value === "" ? null : districtId.value,
      budgetMin: budgetMin.value === "" ? null : budgetMin.value, budgetMax: budgetMax.value === "" ? null : budgetMax.value,
      currencyCode: "TWD", deadlineAt: new Date(deadline.value).toISOString(), status: "open", dateOptions: [], placeOptions: [] };
    notice.value = `活動 #${created.id} 已建立。`;
    await loadActivity();
  });
}

async function saveActivity() {
  await run(async () => {
    const time = (value: string | null) => value ? (value.length === 5 ? `${value}:00` : value) : null;
    showActivity(await api<HostActivity>(`activities/${activity.value!.id}`, "PATCH", {
      title: editTitle.value,
      dateOptions: editDates.value.map(x => ({ id: x.id, optionDate: x.optionDate, startTime: time(x.startTime), endTime: time(x.endTime) })),
    }));
    notice.value = "修改已儲存。";
  });
}

function newActivity() {
  activity.value = null;
  editDates.value = [];
  shareToken.value = "";
  lookupId.value = "";
  title.value = ""; dates.value = []; deadline.value = "";
  places.value = [{ displayLabel: "", customAddress: "" }];
  error.value = ""; notice.value = "";
  const url = new URL(window.location.href);
  url.searchParams.delete("activity");
  window.history.replaceState(null, "", url);
}

async function initialize() {
  await run(async () => {
    references.value = await api<ReferenceData>("reference-data");
    try { user.value = await api<HostUser>("auth/me"); }
    catch (cause) { if ((cause as { statusCode?: number }).statusCode !== 401) throw cause; }
    if (user.value && lookupId.value) await loadActivity();
  });
  ready.value = true;
}
onMounted(() => {
  lookupId.value = new URL(window.location.href).searchParams.get("activity") ?? "";
  timer = setInterval(() => { clock.value = Date.now(); }, 1000);
  initialize();
});
onUnmounted(() => { if (timer) clearInterval(timer); });
</script>

<template>
  <section class="host-workspace rounded-[2rem] border border-stone-200 bg-white/95 p-6 shadow-card sm:p-9">
    <p class="text-xs font-black tracking-widest text-teal">主揪活動管理</p>
    <h2 class="my-3 text-3xl font-black">{{ activity ? '活動內容' : user ? '建立活動' : '登入，開始揪人' }}</h2>
    <p v-if="error" role="alert" class="my-4 rounded-xl bg-red-50 p-3 text-red-800">{{ error }}</p>
    <p v-if="notice" role="status" class="my-4 rounded-xl bg-teal/10 p-3 text-teal">{{ notice }}</p>
    <p v-if="!ready" role="status">正在載入…</p>
    <button v-if="ready && !references" :disabled="busy" @click="initialize">重新連線</button>
    <template v-if="ready && references">
      <form v-if="!user" class="space-y-4" @submit.prevent="authenticate">
        <fieldset :disabled="busy" class="space-y-4">
          <label v-if="register">顯示名稱<input v-model="displayName" required maxlength="50" autocomplete="nickname"></label>
          <label>Email<input v-model="email" type="email" required autocomplete="username"></label>
          <label>密碼<input v-model="password" type="password" required :autocomplete="register ? 'new-password' : 'current-password'"></label>
          <p v-if="register" class="text-sm text-stone-600">密碼至少 6 字元，包含大小寫英文字母、數字與符號。</p>
          <button class="primary" type="submit">{{ busy ? '處理中…' : register ? '註冊並登入' : '登入' }}</button>
          <button type="button" @click="register = !register; error = ''">{{ register ? '已有帳號，前往登入' : '第一次使用？建立帳號' }}</button>
        </fieldset>
      </form>
      <template v-else>
        <div class="my-4 flex items-center justify-between gap-3"><span>{{ user.displayName }}</span><button :disabled="busy" @click="logout">登出</button></div>
        <form class="mb-6 flex items-end gap-2" @submit.prevent="run(loadActivity)">
          <label class="min-w-0 flex-1">開啟既有活動<input v-model="lookupId" inputmode="numeric" pattern="[1-9][0-9]*" required placeholder="活動編號" :disabled="busy"></label>
          <button type="submit" :disabled="busy">開啟</button>
        </form>
        <form v-if="!activity" @submit.prevent="createActivity">
          <fieldset :disabled="busy" class="space-y-4">
            <label>活動名稱<input v-model="title" required maxlength="100" placeholder="例如：週末朋友聚會"></label>
            <DateMultiPicker v-model="dates" />
            <label>活動類型<select v-model="activityTypeId" required><option value="" disabled>請選擇</option><option v-for="item in references.activityTypes" :key="item.id" :value="item.id">{{ item.name }}</option></select></label>
            <div class="grid grid-cols-2 gap-3">
              <label>縣市<select v-model="cityId" required><option value="" disabled>請選擇</option><option v-for="item in references.cities" :key="item.id" :value="item.id">{{ item.name }}</option></select></label>
              <label>行政區<select v-model="districtId"><option value="">不限行政區</option><option v-for="item in districts" :key="item.id" :value="item.id">{{ item.name }}</option></select></label>
              <label>最低預算（NT$）<input v-model="budgetMin" type="number" min="0" max="99999999.99" step="0.01"></label>
              <label>最高預算（NT$）<input v-model="budgetMax" type="number" :min="budgetMin || 0" max="99999999.99" step="0.01"></label>
            </div>
            <label>截止時間（本地時間）<input v-model="deadline" type="datetime-local" required></label>
            <p class="text-sm text-stone-600">建立後截止時間固定。候選地點請自行填入，最多 5 個。</p>
            <div v-for="(place, index) in places" :key="index" class="space-y-2 rounded-xl bg-paper p-3">
              <label>候選地點 {{ index + 1 }}<input v-model="place.displayLabel" required maxlength="200" placeholder="地點名稱"></label>
              <label>地址或地圖連結（選填）<input v-model="place.customAddress"></label>
              <button v-if="places.length > 1" type="button" @click="places.splice(index, 1)">移除地點 {{ index + 1 }}</button>
            </div>
            <button v-if="places.length < 5" type="button" @click="places.push({ displayLabel: '', customAddress: '' })">＋ 加入地點</button>
            <button class="primary" type="submit">{{ busy ? '儲存中…' : '建立並儲存活動' }}</button>
          </fieldset>
        </form>
        <template v-else>
          <p class="mb-4 text-sm text-stone-600">活動 #{{ activity.id }} · {{ activity.status }} · 截止 {{ new Date(activity.deadlineAt).toLocaleString('zh-TW') }}</p>
          <p class="mb-4 text-sm">{{ references.activityTypes.find(x => x.id === activity!.activityTypeId)?.name }} · {{ references.cities.find(x => x.id === activity!.cityId)?.name }} · 預算 {{ activity.budgetMin ?? '不限' }} ～ {{ activity.budgetMax ?? '不限' }} {{ activity.currencyCode }}</p>
          <form @submit.prevent="saveActivity">
            <fieldset :disabled="busy || !editable || !editDates.length" class="space-y-4">
              <label>活動名稱<input v-model="editTitle" required maxlength="100"></label>
              <div v-for="(date, index) in editDates" :key="date.id" class="grid gap-2 rounded-xl bg-paper p-3 sm:grid-cols-3">
                <label>候選日期 {{ index + 1 }}<input v-model="date.optionDate" type="date" required></label>
                <label>開始時間<input v-model="date.startTime" type="time" step="1"></label>
                <label>結束時間<input v-model="date.endTime" type="time" step="1"></label>
              </div>
              <button class="primary" type="submit">{{ busy ? '儲存中…' : '儲存名稱與日期修改' }}</button>
            </fieldset>
          </form>
          <p v-if="!editable" class="my-3 text-sm text-stone-600">活動已截止或未開放，無法修改。</p>
          <ul class="my-4 space-y-2"><li v-for="place in activity.placeOptions" :key="place.id">{{ place.displayLabel }}<span v-if="place.customAddress" class="block break-all text-sm text-stone-600">{{ place.customAddress }}</span></li></ul>
          <p class="my-4 rounded-xl bg-amber-50 p-3 text-sm">活動已保存。朋友投票與公開分享頁尚未開放，目前不能傳送投票連結。</p>
          <details v-if="shareToken" class="my-4 text-sm"><summary>保存本次建立的分享碼</summary><p class="my-2">分享碼只回傳一次；重新整理後無法再次取得，請自行保存供未來功能使用。</p><code class="block break-all">{{ shareToken }}</code></details>
          <p class="my-3 text-sm">可收藏目前網址，登入後再次查看此活動。</p>
          <button :disabled="busy" @click="newActivity">建立另一場活動</button>
        </template>
      </template>
    </template>
  </section>
</template>

<style scoped>
.host-workspace label { display: block; font-size: 0.875rem; font-weight: 700; }
.host-workspace input, .host-workspace select { display: block; width: 100%; margin-top: 0.4rem; padding: 0.75rem; border: 1px solid #d6d3d1; border-radius: 0.75rem; background: #fffdf9; }
.host-workspace button { border-radius: 0.75rem; padding: 0.75rem 1rem; font-weight: 700; border: 1px solid #d6d3d1; }
.host-workspace .primary { display: block; width: 100%; background: #e96d5b; color: white; border: none; }
.host-workspace :disabled { opacity: 0.6; cursor: not-allowed; }
</style>
