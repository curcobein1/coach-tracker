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
    exerciseName: string,
    kg: number,
    reps: number,
    rir: number
  ): Observable<any> {
    return this.http.post(`${this.baseUrl}/log-set`, {
      exerciseName,
      weight: kg,
      reps: reps,
      rir: rir
    });
  }

  deleteSet(setId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/sets/${setId}`);
  }
}
