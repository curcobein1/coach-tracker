export interface LoggedFood {
  fdcId: number;
  name: string;
  grams: number;
  kcal: number;
  p: number;
  c: number;
  f: number;
  micros?: Record<string, { label: string; unit: string; value: number }> | null;
  at: string; // ISO
}

export interface DayNutritionLog {
  date: string; // YYYY-MM-DD
  foods: LoggedFood[];
}

export interface UsualFood {
  fdcId: number;
  name: string;
  grams: number;
  kcal: number;
  p: number;
  c: number;
  f: number;
}

