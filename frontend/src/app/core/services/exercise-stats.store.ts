import { Injectable } from '@angular/core';
import { ExerciseStats } from '../models/exercise-stats.models';
import { epleyE1RM, round1 } from '../utils/training-math';
import { isoWeekKey } from '../utils/iso-week';

const KEY = 'coach-tracker.exerciseStats.v1';

function read(): ExerciseStats[] {
  const raw = localStorage.getItem(KEY);
  if (!raw) return [];
  try { return JSON.parse(raw) as ExerciseStats[]; } catch { return []; }
}

function write(items: ExerciseStats[]): void {
  localStorage.setItem(KEY, JSON.stringify(items));
}

@Injectable({ providedIn: 'root' })
export class ExerciseStatsStore {
  list(): ExerciseStats[] {
    return read().sort((a, b) => a.exerciseName.localeCompare(b.exerciseName));
  }

  get(exerciseName: string): ExerciseStats | undefined {
    return read().find(x => x.exerciseName.toLowerCase() === exerciseName.toLowerCase());
  }

  upsertBaseline(exerciseName: string, kg: number, reps: number, rir: number, alpha = 0.2): void {
    const name = exerciseName.trim();
    if (!name) return;

    const items = read();
    const existingIdx = items.findIndex(x => x.exerciseName.toLowerCase() === name.toLowerCase());

    const baselineE1RM = round1(epleyE1RM(kg, reps));
    const nowWeek = isoWeekKey(new Date());

    const next: ExerciseStats = {
      exerciseName: name,
      baseline: { kg, reps, rir },
      baselineE1RM,
      currentAvgE1RM: existingIdx >= 0 ? items[existingIdx].currentAvgE1RM : baselineE1RM,
      lastUpdatedIsoWeek: existingIdx >= 0 ? items[existingIdx].lastUpdatedIsoWeek : nowWeek,
      alpha: existingIdx >= 0 ? items[existingIdx].alpha : alpha,
    };

    if (existingIdx >= 0) items[existingIdx] = next;
    else items.push(next);

    write(items);
  }

  setCurrentAvg(exerciseName: string, newAvgE1RM: number, weekKey: string): void {
    const items = read();
    const idx = items.findIndex(x => x.exerciseName.toLowerCase() === exerciseName.toLowerCase());
    if (idx < 0) return;
    items[idx] = { ...items[idx], currentAvgE1RM: round1(newAvgE1RM), lastUpdatedIsoWeek: weekKey };
    write(items);
  }

  remove(exerciseName: string): void {
    const items = read().filter(x => x.exerciseName.toLowerCase() !== exerciseName.toLowerCase());
    write(items);
  }
}
