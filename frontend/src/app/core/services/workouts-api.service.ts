import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WorkoutsApiService {
  private baseUrl = 'http://localhost:5106/api/workouts';

  constructor(private http: HttpClient) {}

  getWorkoutByDate(date: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/${date}`);
  }

  finalizeWorkout(dto: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/finalize`, dto);
  }
}
