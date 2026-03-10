export interface BaselineSet {
  kg: number;
  reps: number;
  rir: number; // 0..6 typically
}

export interface ExerciseStats {
  exerciseName: string;       // "Bench Press"
  baseline: BaselineSet;      // your starting numbers
  baselineE1RM: number;       // derived
  currentAvgE1RM: number;     // updated weekly (EMA)
  lastUpdatedIsoWeek: string; // e.g., "2026-W10"
  alpha: number;              // e.g., 0.2
}

//

export interface LoggedSet {
  kg: number;
  reps: number;
  rir?: number;
  createdAt: string; // ISO date-time
}

export interface ExerciseSessionLog {
  exerciseName: string;
  sets: LoggedSet[];
}

export interface DayLog {
  date: string; // YYYY-MM-DD
  exercises: ExerciseSessionLog[];
}
