import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SplitDayExerciseDto {
  id: number;
  exerciseId: number;
  exerciseName: string;
  orderIndex: number;
  targetSets: number;
  targetRepRange?: string | null;
  notes?: string | null;
}

export interface SplitDayDto {
  id: number;
  name: string;
  orderIndex: number;
  exercises: SplitDayExerciseDto[];
}

export interface SplitDetailDto {
  id: number;
  name: string;
  days: SplitDayDto[];
}

export interface SplitListItemDto {
  id: number;
  name: string;
  dayCount: number;
}

@Injectable({ providedIn: 'root' })
export class SplitsApiService {
  private base = 'http://localhost:5106/api/splits';

  constructor(private http: HttpClient) {}

  list(): Observable<SplitListItemDto[]> {
    return this.http.get<SplitListItemDto[]>(this.base);
  }

  getById(id: number): Observable<SplitDetailDto> {
    return this.http.get<SplitDetailDto>(`${this.base}/${id}`);
  }

  create(name: string): Observable<SplitListItemDto> {
    return this.http.post<SplitListItemDto>(this.base, { name: name.trim() });
  }

  addDay(splitId: number, name: string): Observable<SplitDayDto> {
    return this.http.post<SplitDayDto>(`${this.base}/${splitId}/days`, { name: name.trim() });
  }

  addExerciseToDay(dayId: number, exerciseId: number, targetSets: number, targetRepRange?: string, notes?: string): Observable<SplitDayExerciseDto> {
    return this.http.post<SplitDayExerciseDto>(`${this.base}/days/${dayId}/exercises`, {
      exerciseId,
      targetSets,
      targetRepRange: targetRepRange ?? null,
      notes: notes ?? null
    });
  }

  removeDayExercise(id: number): Observable<{ ok: boolean }> {
    return this.http.delete<{ ok: boolean }>(`${this.base}/day-exercises/${id}`);
  }
}
