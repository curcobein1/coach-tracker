import { Injectable } from '@angular/core';
import { DayLog, ExerciseSessionLog, LoggedSet } from '../models/logs.models';
import { todayKey } from '../utils/training-math';

const KEY = 'coach-tracker.dayLogs.v1';

function readAll(): Record<string, DayLog> {
  const raw = localStorage.getItem(KEY);
  if (!raw) return {};
  try { return JSON.parse(raw) as Record<string, DayLog>; } catch { return {}; }
}

function writeAll(map: Record<string, DayLog>): void {
  localStorage.setItem(KEY, JSON.stringify(map));
}

@Injectable({ providedIn: 'root' })
export class LogsStore {
  getDay(date = todayKey()): DayLog {
    const map = readAll();
    return map[date] ?? { date, exercises: [] };
  }

  saveDay(day: DayLog): void {
    const map = readAll();
    map[day.date] = day;
    writeAll(map);
  }

  addSet(date: string, exerciseName: string, set: LoggedSet): void {
    const day = this.getDay(date);
    const exIdx = day.exercises.findIndex(e => e.exerciseName.toLowerCase() === exerciseName.toLowerCase());
    if (exIdx < 0) {
      const ex: ExerciseSessionLog = { exerciseName, sets: [set] };
      day.exercises = [ex, ...day.exercises];
    } else {
      const ex = day.exercises[exIdx];
      const updated = { ...ex, sets: [set, ...ex.sets] };
      day.exercises = day.exercises.map((e, i) => (i === exIdx ? updated : e));
    }
    this.saveDay(day);
  }

  listAllDays(): DayLog[] {
    const map = readAll();
    return Object.values(map).sort((a, b) => b.date.localeCompare(a.date));
  }
}
