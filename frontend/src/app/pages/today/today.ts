import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ExerciseStatsStore } from '../../core/services/exercise-stats.store';
import { LogsStore } from '../../core/services/logs.store';
import { ExerciseAnalyticsService } from '../../core/services/exercise-analytics.service';
import { NutritionStore } from '../../core/services/nutrition.store';
import { DayNutritionLog, LoggedFood } from '../../core/models/nutrition.models';
import { getDayOfWeek, todayKey } from '../../core/utils/training-math';
import { parseKgInput } from '../../core/utils/weight-parser';
import {SHARED_IMPORTS} from '../../shared/shared-imports';
import { WorkoutsApiService } from '../../core/services/workouts-api.service';
import { PlansApiService, TrainingPlanDayDto } from '../../core/services/plans-api.service';

@Component({
  imports: SHARED_IMPORTS,
  selector: 'app-today',
  standalone: true,
  templateUrl: './today.html',
  styleUrls: ['./today.scss'],
})
export class TodayComponent implements OnInit, OnDestroy {
  date = todayKey();
  exerciseName = '';
  kg = ''; // string input
  kgError: string | null = null;
  barKg = 20;

  reps = 0;
  rir: number | null = 2;

  // diet for today
  dietDay: DayNutritionLog | null = null;
  dietTotals = { grams: 0, kcal: 0, p: 0, c: 0, f: 0 };


  // today workout snapshot
  todayWorkout: any | null = null;

  // today's planned split (from active plan in backend)
  todayPlanDay: TrainingPlanDayDto | null = null;

  constructor(
    private workoutsApi: WorkoutsApiService,
    private plansApi: PlansApiService,
    public stats: ExerciseStatsStore,
    private logs: LogsStore,
    public analytics: ExerciseAnalyticsService,
    private nutrition: NutritionStore
  ) {
    this.analytics.weeklyUpdateIfNeeded(new Date());
  }

  ngOnInit(): void {
    this.refreshDiet();
    document.addEventListener('nutrition:day-updated', this.onNutritionUpdate);
    this.refreshTodayWorkout();
    this.loadTodayPlan();
  }

  private refreshDiet(): void {
    this.dietDay = this.nutrition.getDay(this.date);
    const foods = this.dietDay?.foods ?? [];
    this.dietTotals = foods.reduce(
      (acc, f) => ({
        grams: acc.grams + f.grams,
        kcal: acc.kcal + f.kcal,
        p: acc.p + f.p,
        c: acc.c + f.c,
        f: acc.f + f.f,
      }),
      { grams: 0, kcal: 0, p: 0, c: 0, f: 0 }
    );
  }

  private onNutritionUpdate = (e: Event) => {
    const anyEvent = e as CustomEvent<{ date: string }>;
    if (!anyEvent.detail || anyEvent.detail.date !== this.date) return;
    this.refreshDiet();
  };

  ngOnDestroy(): void {
    document.removeEventListener('nutrition:day-updated', this.onNutritionUpdate);
  }

  deleteFood(f: LoggedFood): void {
    this.nutrition.removeFood(this.date, f.at);
  }

  onKgInput(v: string): void {
    let s = '';
    for (const ch of v ?? '') {
      if (ch.charCodeAt(0) <= 127) s += ch;
    }
    s = s.replace(/[^0-9.+ a-zA-Z]/g, '');
    s = s.toLowerCase();
    s = s.replace(/\s+/g, ' ');

    this.kg = s;
    this.kgError = null;
  }

  logSet(): void {
    const name = this.exerciseName.trim();
    if (!name || !this.reps) return;

    const parsed = parseKgInput(this.kg, this.barKg);
    if (!parsed.ok) {
      this.kgError = parsed.error ?? 'Invalid weight';
      return;
    }
    this.kgError = null;

    this.logs.addSet(this.date, name, {
      kg: parsed.kg!,
      reps: Number(this.reps),
      rir: this.rir ? Number(this.rir) : 0,
      createdAt: new Date().toISOString(),
    }).subscribe({
      next: () => {
        this.reps = 0;
        this.refreshTodayWorkout();
      },
      error: (e) => {
        console.error('Failed to log set', e);
      },
    });
  }

  snap(name: string) {
    return this.analytics.snapshot(name, new Date());
  }

  deleteSetById(setId: number): void {
    this.workoutsApi.deleteSet(setId).subscribe({
      next: () => {
        this.refreshTodayWorkout();
      },
      error: (e) => console.error('Failed to delete set', e),
    });
  }

  private refreshTodayWorkout(): void {
    this.workoutsApi.getTodayWorkout().subscribe({
      next: (data: any) => {
        this.todayWorkout = data;
      },
      error: (e) => console.error('Failed to load today workout', e),
    });
  }

  private loadTodayPlan(): void {
    const planDayOfWeek = getDayOfWeek(new Date());

    this.plansApi.getActivePlan().subscribe({
      next: (plan) => {
        this.todayPlanDay = plan.days.find((d) => d.dayOfWeek === planDayOfWeek) ?? null;
      },
      error: (e) => console.error('Failed to load today plan', e),
    });
  }
}




