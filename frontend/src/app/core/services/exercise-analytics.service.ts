import { Injectable } from '@angular/core';
import { ExerciseStatsStore } from './exercise-stats.store';
import { LogsStore } from './logs.store';
import { epleyE1RM, round1 } from '../utils/training-math';
import { isoWeekKey } from '../utils/iso-week';
import { DayLog, LoggedSet } from '../models/logs.models';

export interface ExerciseSnapshot {
  exerciseName: string;
  baselineE1RM: number;
  currentAvgE1RM: number;
  lastSessionBestE1RM?: number;
  thisWeekAvgE1RM?: number;
}

function bestSetE1RM(sets: LoggedSet[]): number | undefined {
  const vals = sets.map(s => epleyE1RM(s.kg, s.reps)).filter(x => x > 0);
  if (vals.length === 0) return undefined;
  return Math.max(...vals);
}

@Injectable({ providedIn: 'root' })
export class ExerciseAnalyticsService {
  constructor(private stats: ExerciseStatsStore, private logs: LogsStore) {}

  /** Call this when Today loads: updates currentAvg once per new ISO week (if last week has data). */
  weeklyUpdateIfNeeded(now = new Date()): void {
    const thisWeek = isoWeekKey(now);
    const allDays = this.logs.listAllDays();

    for (const st of this.stats.list()) {
      if (st.lastUpdatedIsoWeek === thisWeek) continue;

      // compute previous week key = the week we are "closing"
      // Simple approach: update using the most recent week with data since last update.
      // We'll use current week data so far only when week changes (works fine for personal use).
      const targetWeek = thisWeek;

      const weekAvg = this.computeWeekAvgE1RM(st.exerciseName, allDays, targetWeek);
      if (weekAvg === undefined) {
        // no data for that week -> just mark updated so we don't spam update attempts
        this.stats.setCurrentAvg(st.exerciseName, st.currentAvgE1RM, thisWeek);
        continue;
      }

      const alpha = st.alpha ?? 0.2;
      const newAvg = (1 - alpha) * st.currentAvgE1RM + alpha * weekAvg;
      this.stats.setCurrentAvg(st.exerciseName, newAvg, thisWeek);
    }
  }

  snapshot(exerciseName: string, now = new Date()): ExerciseSnapshot | undefined {
    const st = this.stats.get(exerciseName);
    if (!st) return undefined;

    const allDays = this.logs.listAllDays();
    const last = this.lastSessionBest(exerciseName, allDays);
    const week = this.computeWeekAvgE1RM(exerciseName, allDays, isoWeekKey(now));

    return {
      exerciseName: st.exerciseName,
      baselineE1RM: st.baselineE1RM,
      currentAvgE1RM: st.currentAvgE1RM,
      lastSessionBestE1RM: last,
      thisWeekAvgE1RM: week,
    };
  }

  private lastSessionBest(exerciseName: string, days: DayLog[]): number | undefined {
    for (const d of days) {
      const ex = d.exercises.find(e => e.exerciseName.toLowerCase() === exerciseName.toLowerCase());
      if (!ex) continue;
      const best = bestSetE1RM(ex.sets);
      if (best !== undefined) return round1(best);
    }
    return undefined;
  }

  private computeWeekAvgE1RM(exerciseName: string, days: DayLog[], weekKey: string): number | undefined {
    const bests: number[] = [];

    for (const d of days) {
      // Convert day string to Date to get isoWeek
      const dt = new Date(d.date + 'T12:00:00');
      if (isoWeekKey(dt) !== weekKey) continue;

      const ex = d.exercises.find(e => e.exerciseName.toLowerCase() === exerciseName.toLowerCase());
      if (!ex) continue;

      const best = bestSetE1RM(ex.sets);
      if (best !== undefined) bests.push(best);
    }

    if (bests.length === 0) return undefined;
    const avg = bests.reduce((a, b) => a + b, 0) / bests.length;
    return round1(avg);
  }
}
