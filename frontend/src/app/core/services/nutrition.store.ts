import { Injectable } from '@angular/core';
import { DayNutritionLog, LoggedFood, UsualFood } from '../models/nutrition.models';
import { todayKey } from '../utils/training-math';

const DAY_KEY = 'coach-tracker.nutrition.days.v1';
const USUALS_KEY = 'coach-tracker.nutrition.usuals.v1';

function readDays(): Record<string, DayNutritionLog> {
  const raw = localStorage.getItem(DAY_KEY);
  if (!raw) return {};
  try {
    return JSON.parse(raw) as Record<string, DayNutritionLog>;
  } catch {
    return {};
  }
}

function writeDays(map: Record<string, DayNutritionLog>): void {
  localStorage.setItem(DAY_KEY, JSON.stringify(map));
}

function readUsuals(): UsualFood[] {
  const raw = localStorage.getItem(USUALS_KEY);
  if (!raw) return [];
  try {
    return JSON.parse(raw) as UsualFood[];
  } catch {
    return [];
  }
}

function writeUsuals(list: UsualFood[]): void {
  localStorage.setItem(USUALS_KEY, JSON.stringify(list));
}

@Injectable({ providedIn: 'root' })
export class NutritionStore {
  getDay(date = todayKey()): DayNutritionLog {
    const map = readDays();
    return map[date] ?? { date, foods: [] };
  }

  saveDay(day: DayNutritionLog): void {
    const map = readDays();
    map[day.date] = day;
    writeDays(map);
  }

  addFood(food: Omit<LoggedFood, 'at'>, date = todayKey()): void {
    const day = this.getDay(date);
    const logged: LoggedFood = {
      ...food,
      at: new Date().toISOString(),
    };
    day.foods = [logged, ...(day.foods ?? [])];
    this.saveDay(day);
    // fire-and-forget event so Today page can react if desired
    document.dispatchEvent(new CustomEvent('nutrition:day-updated', { detail: { date: day.date } }));
  }

  removeFood(date: string, at: string): void {
    const day = this.getDay(date);
    day.foods = (day.foods ?? []).filter(f => f.at !== at);
    this.saveDay(day);
    document.dispatchEvent(new CustomEvent('nutrition:day-updated', { detail: { date: day.date } }));
  }

  getUsuals(): UsualFood[] {
    return readUsuals();
  }

  addOrUpdateUsual(food: UsualFood): void {
    const list = readUsuals();
    const idx = list.findIndex((u) => u.fdcId === food.fdcId);
    if (idx >= 0) {
      const updated = list.map((u, i) => (i === idx ? food : u));
      writeUsuals(updated);
      return;
    }
    writeUsuals([food, ...list]);
  }

  removeUsual(fdcId: number): void {
    const list = readUsuals();
    writeUsuals(list.filter((u) => u.fdcId !== fdcId));
  }
}

