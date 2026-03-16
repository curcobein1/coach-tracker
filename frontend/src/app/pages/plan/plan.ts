import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SplitsApiService, SplitDetailDto, SplitDayDto, SplitDayExerciseDto, SplitListItemDto } from '../../core/services/splits-api.service';
import { CalendarApiService } from '../../core/services/calendar-api.service';
import { ExercisesApiService, ExerciseDto } from '../../core/services/exercises-api.service';
import { todayKey } from '../../core/utils/training-math';

import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-plan',
  standalone: true,
  imports: [CommonModule, FormsModule, SelectModule, ButtonModule, CardModule],
  templateUrl: './plan.html',
  styleUrls: ['./plan.scss'],
})
export class PlanComponent implements OnInit {
  splits: SplitListItemDto[] = [];
  selectedSplit: SplitDetailDto | null = null;
  selectedDay: SplitDayDto | null = null;
  exercises: ExerciseDto[] = [];
  todayKey = todayKey();
  loading = false;
  error: string | null = null;

  constructor(
    private splitsApi: SplitsApiService,
    private calendarApi: CalendarApiService,
    private exercisesApi: ExercisesApiService
  ) {}

  ngOnInit(): void {
    this.loadSplits();
    this.loadExercises();
  }

  loadSplits(): void {
    this.splitsApi.list().subscribe({
      next: (list) => { this.splits = list; },
      error: (e) => { this.error = 'Failed to load splits'; console.error(e); }
    });
  }

  loadExercises(): void {
    this.exercisesApi.list().subscribe({
      next: (list) => { this.exercises = list; },
      error: (e) => { console.error('Failed to load exercises', e); }
    });
  }

  onSelectSplit(splitId: number): void {
    if (!splitId) { this.selectedSplit = null; this.selectedDay = null; return; }
    this.loading = true;
    this.splitsApi.getById(splitId).subscribe({
      next: (detail) => { this.selectedSplit = detail; this.selectedDay = detail.days[0] ?? null; this.loading = false; },
      error: (e) => { this.error = 'Failed to load split'; this.loading = false; }
    });
  }

  selectDay(day: SplitDayDto): void {
    this.selectedDay = day;
  }

  assignDayToToday(): void {
    if (!this.selectedDay) return;
    this.calendarApi.assignDay(this.todayKey, this.selectedDay.id).subscribe({
      next: () => { this.error = null; },
      error: (e) => { this.error = 'Failed to assign'; console.error(e); }
    });
  }

  removeExercise(item: SplitDayExerciseDto): void {
    if (!this.selectedSplit || !this.selectedDay) return;
    this.splitsApi.removeDayExercise(item.id).subscribe({
      next: () => {
        this.selectedDay!.exercises = this.selectedDay!.exercises.filter(e => e.id !== item.id);
      },
      error: (e) => { console.error(e); }
    });
  }
}
