import { Injectable, signal } from '@angular/core';
import { DayNutritionLog, LoggedFood, UsualFood } from '../models/nutrition.models';
import { todayKey } from '../utils/training-math';
import { NutritionApiService } from './nutrition-api.service';

@Injectable({ providedIn: 'root' })
export class NutritionStore {
  private dayCache = new Map<string, DayNutritionLog>();

  public usuals = signal<UsualFood[]>([]);

  constructor(private api: NutritionApiService) {
    this.refreshUsuals();
  }

  getDay(date = todayKey()): DayNutritionLog {
    const cached = this.dayCache.get(date);
    if (!cached) {
      const empty = { date, foods: [] as any[] };
      this.dayCache.set(date, empty);
      this.refreshDay(date);
      return empty;
    }
    return cached;
  }

  private refreshDay(date: string): void {
    this.api.getDay(date).subscribe({
      next: (r) => {
        const foods: LoggedFood[] = (r.foods ?? []).map((f) => ({
          fdcId: f.fdcId,
          name: f.name,
          grams: f.grams,
          kcal: f.kcal,
          p: f.p,
          c: f.c,
          f: f.f,
          at: f.id, // map backend id into 'at' for existing UI delete calls
        }));
        this.dayCache.set(date, { date, foods });
        document.dispatchEvent(new CustomEvent('nutrition:day-updated', { detail: { date } }));
      },
      error: (e) => console.error('Failed to load nutrition day', e),
    });
  }

  addFood(food: Omit<LoggedFood, 'at'>, date = todayKey()): void {
    this.api.addFood(date, {
      fdcId: food.fdcId,
      name: food.name,
      grams: food.grams,
      kcal: food.kcal,
      p: food.p,
      c: food.c,
      f: food.f,
    }).subscribe({
      next: () => this.refreshDay(date),
      error: (e) => console.error('Failed to add food', e),
    });
  }

  removeFood(date: string, at: string): void {
    // 'at' is mapped to backend Guid id string
    this.api.deleteFood(date, at).subscribe({
      next: () => this.refreshDay(date),
      error: (e) => console.error('Failed to delete food', e),
    });
  }

  getUsuals(): UsualFood[] {
    return this.usuals();
  }

  refreshUsuals(): void {
    this.api.getUsuals().subscribe({
      next: (r) => {
        this.usuals.set((r.usuals ?? []).map((u) => ({
          fdcId: u.fdcId,
          name: u.name,
          grams: u.grams,
          kcal: u.kcal,
          p: u.p,
          c: u.c,
          f: u.f,
        })));
      },
      error: (e) => console.error('Failed to load usuals', e),
    });
  }

  addOrUpdateUsual(food: UsualFood): void {
    this.api.upsertUsual(food.fdcId, {
      fdcId: food.fdcId,
      name: food.name,
      grams: food.grams,
      kcal: food.kcal,
      p: food.p,
      c: food.c,
      f: food.f,
    }).subscribe({
      next: () => this.refreshUsuals(),
      error: (e) => console.error('Failed to save usual', e),
    });
  }

  removeUsual(fdcId: number): void {
    this.api.deleteUsual(fdcId).subscribe({
      next: () => this.refreshUsuals(),
      error: (e) => console.error('Failed to delete usual', e),
    });
  }
}

