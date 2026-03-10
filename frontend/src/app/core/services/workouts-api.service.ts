import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WorkoutsApiService {

  private baseUrl = `${environment.apiUrl}/api/workouts`;

  constructor(private http: HttpClient) {}

  getTodayWorkout(): Observable<any> {
    return this.http.get(`${this.baseUrl}/today`);
  }

}
