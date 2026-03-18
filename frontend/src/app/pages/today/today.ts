import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ExerciseStatsStore } from '../../core/services/exercise-stats.store';
import { NutritionStore } from '../../core/services/nutrition.store';
import { DayNutritionLog, LoggedFood } from '../../core/models/nutrition.models';
import { todayKey } from '../../core/utils/training-math';
import { parseKgInput } from '../../core/utils/weight-parser';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { WorkoutsApiService } from '../../core/services/workouts-api.service';
import { CalendarApiService } from '../../core/services/calendar-api.service';
import { MicronutrientTrackerService, MicroProgressItem } from '../../core/services/micronutrient-tracker.service';

type TodayProgramPayload = {
  date: string;
  splitId: number | null;
  splitName: string | null;
  splitDayId: number | null;
  splitDayName: string | null;
  exercises: Array<{
    exerciseId: number | null;
    exerciseName: string;
    targetSets: number;
    targetRepRange?: string | null;
  }>;
};

const todayProgramStorageKey = (date: string) => `coachtracker:todayProgram:${date}`;

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
  kg = '';
  kgError: string | null = null;
  barKg = 20;
  reps = 0;
  rir: number | null = 2;
  formQuality: string | null = null;

  dietDay: DayNutritionLog | null = null;
  dietTotals = { grams: 0, kcal: 0, p: 0, c: 0, f: 0 };
  microProgressList: MicroProgressItem[] = [];
  
  // Local Memory Draft
  todayWorkout: { date: string, notes: string, sets: any[] } = {
    date: this.date,
    notes: '',
    sets: []
  };
  
  todayPlannedFromCalendar: { splitDayName: string; splitName: string; exercises: Array<{ exerciseName: string; targetSets: number; targetRepRange?: string | null }> } | null = null;
  todayProgramFromPlan: TodayProgramPayload | null = null;
  plannedSplitError: string | null = null;

  constructor(
    private workoutsApi: WorkoutsApiService,
    private calendarApi: CalendarApiService,
    public stats: ExerciseStatsStore,
    private nutrition: NutritionStore,
    private microTracker: MicronutrientTrackerService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.refreshDiet();
    document.addEventListener('nutrition:day-updated', this.onNutritionUpdate);
    document.addEventListener('today:program-updated', this.onTodayProgramUpdate);
    this.loadTodayProgramFromPlan();
    this.loadTodayCalendar();
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
    const microResult = this.microTracker.computeForDay(foods as any);
    this.microProgressList = microResult.progressList;
    this.cdr.markForCheck();
  }

  private onNutritionUpdate = (e: Event) => {
    const ev = e as CustomEvent<{ date: string }>;
    if (!ev.detail || ev.detail.date !== this.date) return;
    this.refreshDiet();
    this.cdr.detectChanges();
  };

  ngOnDestroy(): void {
    document.removeEventListener('nutrition:day-updated', this.onNutritionUpdate);
    document.removeEventListener('today:program-updated', this.onTodayProgramUpdate);
  }

  deleteFood(f: LoggedFood): void {
    this.nutrition.removeFood(this.date, f.at);
  }

  onKgInput(v: string): void {
    let s = '';
    for (const ch of v ?? '') {
      if (ch.charCodeAt(0) <= 127) s += ch;
    }
    s = s.replace(/[^0-9.+ a-zA-Z]/g, '').toLowerCase().replace(/\s+/g, ' ');
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

    const existingSets = this.todayWorkout.sets.filter(s => s.exerciseName.toLowerCase() === name.toLowerCase());
    const setNum = existingSets.length > 0 ? Math.max(...existingSets.map(s => s.setNumber)) + 1 : 1;

    this.todayWorkout.sets.push({
      id: -Date.now(),
      exerciseName: name,
      setNumber: setNum,
      weight: parsed.kg,
      reps: Number(this.reps),
      rir: this.rir ?? 0,
      formQuality: this.formQuality
    });

    this.reps = 0;
  }

  snap(name: string): { lastSessionBestE1RM?: number; thisWeekAvgE1RM?: number } | undefined {
    const st = this.stats.get(name);
    if (!st) return undefined;
    return { lastSessionBestE1RM: undefined, thisWeekAvgE1RM: undefined };
  }

  deleteSet(setToRemove: any): void {
    this.todayWorkout.sets = this.todayWorkout.sets.filter(s => s !== setToRemove);
    // Recalculate set numbers for the remaining sets of this exercise
    const remainingSets = this.todayWorkout.sets.filter(s => s.exerciseName === setToRemove.exerciseName);
    remainingSets.sort((a, b) => a.setNumber - b.setNumber).forEach((s, idx) => {
        s.setNumber = idx + 1;
    });
  }

  private loadDailyWorkout(): void {
    this.workoutsApi.getWorkoutByDate(this.date).subscribe({
      next: (data: any) => { 
          if(data) {
              this.todayWorkout = {
                  date: data.date || this.date,
                  notes: data.notes || '',
                  sets: data.sets || []
              };
          }
          this.cdr.detectChanges();
      },
      error: (e) => {
        console.error('Failed to load today workout', e);
        this.cdr.markForCheck();
      },
    });
  }

  private loadTodayCalendar(): void {
    this.calendarApi.getDay(this.date).subscribe({
      next: (data: any) => {
        if (data?.splitDay && data?.exercises?.length > 0) {
          this.todayPlannedFromCalendar = {
            splitDayName: data.splitDay.name,
            splitName: data.splitDay.splitName ?? '',
            exercises: data.exercises.map((e: any) => ({
              exerciseName: e.exerciseName ?? `#${e.exerciseId}`,
              targetSets: e.targetSets ?? 0,
              targetRepRange: e.targetRepRange ?? null
            }))
          };
        } else {
          this.todayPlannedFromCalendar = null;
        }
        this.plannedSplitError = null;
        this.cdr.detectChanges();
      },
      error: () => {
        this.todayPlannedFromCalendar = null;
        this.plannedSplitError = null;
        this.cdr.markForCheck();
      }
    });
  }

  clearTodaysPlannedSplit(): void {
    this.plannedSplitError = null;
    this.calendarApi.clearDay(this.date).subscribe({
      next: () => {
        this.todayPlannedFromCalendar = null;
        this.loadTodayCalendar();
        this.cdr.detectChanges();
      },
      error: (e) => {
        console.error('Failed to clear planned day', e);
        this.plannedSplitError = 'Clear failed. Restart the backend to enable this endpoint.';
        this.cdr.markForCheck();
      }
    });
  }

  clearTodaysProgram(): void {
    // If a Plan-selected program is active, clear it in one click
    if (this.todayProgramFromPlan?.date === this.date && (this.todayProgramFromPlan.exercises?.length ?? 0) > 0) {
      localStorage.removeItem(todayProgramStorageKey(this.date));
      this.todayProgramFromPlan = null;
      document.dispatchEvent(new CustomEvent('today:program-updated', { detail: { date: this.date } }));
      this.cdr.detectChanges();
      return;
    }

    // Otherwise clear the calendar planned split (if present)
    if (this.todayPlannedFromCalendar) {
      this.clearTodaysPlannedSplit();
    }
  }

  get todaysProgramExercises(): Array<{ exerciseName: string; targetSets: number; targetRepRange?: string | null }> {
    if (this.todayProgramFromPlan?.date === this.date && this.todayProgramFromPlan.exercises.length > 0) {
      return this.todayProgramFromPlan.exercises.map(e => ({
        exerciseName: e.exerciseName,
        targetSets: e.targetSets,
        targetRepRange: e.targetRepRange ?? null
      }));
    }
    return this.todayPlannedFromCalendar?.exercises ?? [];
  }

  get todaysProgramLabel(): string | null {
    if (this.todayProgramFromPlan?.date === this.date && this.todayProgramFromPlan.exercises.length > 0) {
      const split = this.todayProgramFromPlan.splitName ?? 'Custom';
      const day = this.todayProgramFromPlan.splitDayName ?? 'Program';
      return `${split} – ${day}`;
    }
    if (this.todayPlannedFromCalendar) {
      return `${this.todayPlannedFromCalendar.splitName} – ${this.todayPlannedFromCalendar.splitDayName}`;
    }
    return null;
  }

  private loadTodayProgramFromPlan(): void {
    const raw = localStorage.getItem(todayProgramStorageKey(this.date));
    if (!raw) {
      this.todayProgramFromPlan = null;
      this.cdr.markForCheck();
      return;
    }
    try {
      const parsed = JSON.parse(raw) as TodayProgramPayload;
      if (!parsed || parsed.date !== this.date) {
        this.todayProgramFromPlan = null;
      } else {
        this.todayProgramFromPlan = parsed;
      }
    } catch {
      this.todayProgramFromPlan = null;
    }
    this.cdr.markForCheck();
  }

  private onTodayProgramUpdate = (e: Event) => {
    const ev = e as CustomEvent<{ date: string }>;
    if (!ev.detail || ev.detail.date !== this.date) return;
    this.loadTodayProgramFromPlan();
    this.cdr.detectChanges();
  };
  
  prefillExercise(name: string): void {
      this.exerciseName = name;
  }

  finalizeWorkout(): void {
    const payload = {
        date: this.todayWorkout.date,
        notes: this.todayWorkout.notes,
        sets: this.todayWorkout.sets.map(s => ({
            exerciseId: s.exerciseId || null,
            exerciseName: s.exerciseName,
            setNumber: s.setNumber,
            weight: s.weight,
            reps: s.reps,
            rir: s.rir,
            formQuality: s.formQuality ?? null
        }))
    };
    
    this.workoutsApi.finalizeWorkout(payload).subscribe({
        next: () => {},
        error: (e) => console.error('Failed to finalize workout', e)
    });
  }

  removeFromTodaysProgram(idx: number): void {
    if (!this.todayProgramFromPlan || this.todayProgramFromPlan.date !== this.date) return;
    if (idx < 0 || idx >= this.todayProgramFromPlan.exercises.length) return;
    this.todayProgramFromPlan.exercises.splice(idx, 1);
    localStorage.setItem(todayProgramStorageKey(this.date), JSON.stringify(this.todayProgramFromPlan));
    document.dispatchEvent(new CustomEvent('today:program-updated', { detail: { date: this.date } }));
    this.cdr.detectChanges();
  }

  clearSessionNotes(): void {
    this.todayWorkout.notes = '';
    this.cdr.detectChanges();
  }

  removeBaseline(exerciseName: string): void {
    this.stats.remove(exerciseName);
    this.cdr.detectChanges();
  }
}
