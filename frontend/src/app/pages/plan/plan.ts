import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SplitsApiService, SplitDetailDto, SplitDayDto, SplitDayExerciseDto, SplitListItemDto } from '../../core/services/splits-api.service';
import { ExercisesApiService, ExerciseDto } from '../../core/services/exercises-api.service';
import { todayKey } from '../../core/utils/training-math';
import { Router } from '@angular/router';

type PlannedExercise = {
  exerciseId: number | null;
  exerciseName: string;
  targetSets: number;
  targetRepRange?: string | null;
  source: 'split' | 'extra';
};

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
  selector: 'app-plan',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './plan.html',
  styleUrls: ['./plan.scss'],
})
export class PlanComponent implements OnInit {
  splits: SplitListItemDto[] = [];
  selectedSplit: SplitDetailDto | null = null;
  selectedDay: SplitDayDto | null = null;
  exercises: ExerciseDto[] = [];
  selectedExerciseId: number | null = null;
  extraRepRange: string | null = '8-12';
  extraSets: number | null = 3;

  chosenSplitDayId: number | null = null;
  plannedFromSplit: PlannedExercise[] = [];
  plannedExtras: PlannedExercise[] = [];

  todayKey = todayKey();
  loading = false;
  error: string | null = null;

  constructor(
    private splitsApi: SplitsApiService,
    private exercisesApi: ExercisesApiService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadSplits();
    this.loadExercises();
  }

  loadSplits(): void {
    this.splitsApi.list().subscribe({
      next: (list) => {
        this.splits = list;
        this.cdr.markForCheck();
      },
      error: (e) => {
        this.error = 'Failed to load splits';
        console.error(e);
        this.cdr.markForCheck();
      }
    });
  }

  loadExercises(): void {
    this.exercisesApi.list().subscribe({
      next: (list) => {
        this.exercises = list;
        this.cdr.markForCheck();
      },
      error: (e) => {
        console.error('Failed to load exercises', e);
        this.cdr.markForCheck();
      }
    });
  }

  onSelectSplit(splitId: number): void {
    if (!splitId) {
      this.selectedSplit = null;
      this.selectedDay = null;
      this.chosenSplitDayId = null;
      this.plannedFromSplit = [];
      this.cdr.markForCheck();
      return;
    }
    this.loading = true;
    this.splitsApi.getById(splitId).subscribe({
      next: (detail) => {
        this.selectedSplit = detail;
        this.selectedDay = detail.days[0] ?? null;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (e) => {
        this.error = 'Failed to load split';
        this.loading = false;
        console.error(e);
        this.cdr.markForCheck();
      }
    });
  }

  selectDay(day: SplitDayDto): void {
    this.selectedDay = day;
  }

  chooseThisSplit(): void {
    if (!this.selectedSplit || !this.selectedDay) return;

    this.chosenSplitDayId = this.selectedDay.id;
    this.plannedFromSplit = (this.selectedDay.exercises ?? []).map((ex): PlannedExercise => ({
      exerciseId: ex.exerciseId ?? null,
      exerciseName: ex.exerciseName,
      targetSets: ex.targetSets ?? 0,
      targetRepRange: ex.targetRepRange ?? null,
      source: 'split'
    }));

    this.error = null;
    this.cdr.detectChanges();
  }

  get selectedExerciseName(): string | null {
    if (!this.selectedExerciseId) return null;
    return this.exercises.find(e => e.id === this.selectedExerciseId)?.name ?? null;
  }

  get plannedForDay(): PlannedExercise[] {
    return [...this.plannedFromSplit, ...this.plannedExtras];
  }

  addExtraExercise(): void {
    if (!this.selectedExerciseId) return;
    const ex = this.exercises.find(e => e.id === this.selectedExerciseId);
    if (!ex) return;

    const exists = this.plannedForDay.some(p => (p.exerciseId ?? null) === ex.id);
    if (exists) return;

    const sets = Number(this.extraSets ?? ex.defaultSets ?? 3);
    this.plannedExtras.push({
      exerciseId: ex.id,
      exerciseName: ex.name,
      targetSets: Number.isFinite(sets) ? sets : 3,
      targetRepRange: (this.extraRepRange ?? '8-12') || '8-12',
      source: 'extra'
    });

    this.selectedExerciseId = null;
    this.cdr.markForCheck();
  }

  removePlanned(p: PlannedExercise): void {
    if (p.source === 'split') {
      this.plannedFromSplit = this.plannedFromSplit.filter(x => x !== p);
    } else {
      this.plannedExtras = this.plannedExtras.filter(x => x !== p);
    }
    this.cdr.markForCheck();
  }

  selectAsTodaysProgram(): void {
    const payload: TodayProgramPayload = {
      date: this.todayKey,
      splitId: this.selectedSplit?.id ?? null,
      splitName: this.selectedSplit?.name ?? null,
      splitDayId: this.chosenSplitDayId,
      splitDayName: this.selectedDay?.name ?? null,
      exercises: this.plannedForDay.map(p => ({
        exerciseId: p.exerciseId,
        exerciseName: p.exerciseName,
        targetSets: Number(p.targetSets ?? 0),
        targetRepRange: p.targetRepRange ?? null
      }))
    };

    localStorage.setItem(todayProgramStorageKey(this.todayKey), JSON.stringify(payload));
    document.dispatchEvent(new CustomEvent('today:program-updated', { detail: { date: this.todayKey } }));
    void this.router.navigateByUrl('/today');
  }

  removeExercise(item: SplitDayExerciseDto): void {
    if (!this.selectedSplit || !this.selectedDay) return;
    this.splitsApi.removeDayExercise(item.id).subscribe({
      next: () => {
        this.selectedDay!.exercises = this.selectedDay!.exercises.filter(e => e.id !== item.id);
        this.cdr.markForCheck();
      },
      error: (e) => {
        console.error(e);
        this.cdr.markForCheck();
      }
    });
  }
}
