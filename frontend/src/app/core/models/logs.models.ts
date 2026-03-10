export interface LoggedSet {
  kg: number;
  reps: number;
  rir?: number;       // optional
  createdAt: string;  // ISO
}

export interface ExerciseSessionLog {
  exerciseName: string;
  sets: LoggedSet[];
}

export interface DayLog {
  date: string; // YYYY-MM-DD
  exercises: ExerciseSessionLog[];
}
