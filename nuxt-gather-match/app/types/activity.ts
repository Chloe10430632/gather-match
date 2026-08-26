export interface ActivityDraft {
  activityName: string;
  selectedDates: string[];
  budget: string;
  activityType: string;
  city: string;
  district: string;
}

export interface PlaceOption {
  id: string;
  name: string;
  category: string;
  price: string;
  description: string;
  source: "recommended" | "custom";
  accent: string;
}

export type DatePreference = "available" | "maybe" | "unavailable";

export type PlacePreference = "preferred" | "acceptable" | "avoid";

export interface DemoParticipantSession {
  participantToken: string;
  participantName: string;
  datePreferences: Record<string, DatePreference>;
  placePreferences: Record<string, PlacePreference>;
}

export interface ParticipantResponse {
  id: string;
  displayName: string;
  submittedAt: string;
  datePreferences: Record<string, DatePreference>;
  placePreferences: Record<string, PlacePreference>;
}
