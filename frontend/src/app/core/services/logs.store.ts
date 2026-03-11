import { Injectable, signal } from '@angular/core';
import { WorkoutsApiService } from './workouts-api.service';

@Injectable({ providedIn: 'root' })
export class LogsStore {
  public sessions = signal<any[]>([]);

  constructor(private api: WorkoutsApiService) {}

  listAllDays() { return this.sessions(); }

  addSet(date: string, exerciseName: string, data: { kg: number, reps: number, rir?: number, createdAt?: string }) {
    const exerciseId = 1; // Temporary: logic to find ID by Name goes here later

    // Explicitly type response and err as 'any' to satisfy strict compiler
    this.api.logSet(exerciseId, data.kg, data.reps, data.rir ?? 0).subscribe({
      next: (response: any) => console.log('Success!', response),
      error: (err: any) => console.error('404 or Connection Error:', err)
    });
  }
}
