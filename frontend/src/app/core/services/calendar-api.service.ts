import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CalendarDayResponse {
  splitDay: { id: number; name: string; splitName: string } | null;
  exercises: Array<{ exerciseId: number; exerciseName: string; targetSets: number; targetRepRange?: string }>;
}

@Injectable({ providedIn: 'root' })
export class CalendarApiService {
  private base = 'http://localhost:5106/api/calendar';

  constructor(private http: HttpClient) {}

  getDay(date: string): Observable<CalendarDayResponse> {
    return this.http.get<CalendarDayResponse>(`${this.base}/day/${date}`);
  }

  assignDay(date: string, splitDayId: number): Observable<{ date: string; splitDayId: number }> {
    return this.http.put<{ date: string; splitDayId: number }>(`${this.base}/day/${date}?splitDayId=${splitDayId}`, {});
  }

  clearDay(date: string): Observable<{ date: string; cleared: boolean }> {
    return this.http.delete<{ date: string; cleared: boolean }>(`${this.base}/day/${date}`);
  }
}
