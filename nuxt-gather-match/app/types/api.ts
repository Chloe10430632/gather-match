export interface ApiResponse<T> {
  success: boolean;
  data: T;
  error?: { code: string; message: string; details?: Record<string, string[]> };
  traceId: string;
}
export interface HostUser { id: number; email: string; displayName: string }
export interface ReferenceData {
  activityTypes: { id: number; name: string }[];
  cities: { id: number; name: string; districts: { id: number; name: string }[] }[];
}
export interface HostActivity {
  id: number; title: string; activityTypeId: number;
  cityId: number; districtId: number | null;
  budgetMin: number | null; budgetMax: number | null; currencyCode: string;
  deadlineAt: string; status: string;
  dateOptions: { id: number; optionDate: string; startTime: string | null; endTime: string | null }[];
  placeOptions: { id: number; displayLabel: string; customAddress: string | null }[];
}
