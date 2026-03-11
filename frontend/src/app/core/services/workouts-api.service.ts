import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WorkoutsApiService {
  private baseUrl = 'http://localhost:5106/api/workouts';

  constructor(private http: HttpClient) {}

  getTodayWorkout(): Observable<any> {
    return this.http.get(`${this.baseUrl}/today`);
  }

  logSet(
    exerciseId: number,
    kg: number,
    reps: number,
    rir: number
  ): Observable<any> {
    return this.http.post(`${this.baseUrl}/log-set`, {
      exerciseId: exerciseId,
      weight: kg,
      reps: reps,
      rir: rir
    });
  }
}
