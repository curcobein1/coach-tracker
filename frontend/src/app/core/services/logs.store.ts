import { Injectable, signal } from '@angular/core';
import { WorkoutsApiService } from './workouts-api.service';

@Injectable({ providedIn: 'root' })
export class LogsStore {
  public sessions = signal<any[]>([]);

  constructor(private api: WorkoutsApiService) {}

  listAllDays() { return this.sessions(); }

  addSet(date: string, exerciseName: string, data: { kg: number; reps: number; rir?: number; createdAt?: string }) {
    // Return observable so callers can react (e.g. refresh today workout)
    return this.api.logSet(exerciseName, data.kg, data.reps, data.rir ?? 0);
  }
}
