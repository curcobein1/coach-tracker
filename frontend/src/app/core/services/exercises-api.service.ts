import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ExerciseDto {
  id: number;
  name: string;
  group: string;
  category: string;
  equipment?: string | null;
  tags?: string | null;
  notes?: string | null;
  primaryMuscleGroup?: string | null;
  defaultSets?: number | null;
}

@Injectable({ providedIn: 'root' })
export class ExercisesApiService {
  private base = 'http://localhost:5106/api/Exercises';

  constructor(private http: HttpClient) {}

  list(): Observable<ExerciseDto[]> {
    return this.http.get<ExerciseDto[]>(this.base);
  }

  create(exercise: Partial<ExerciseDto> & { name: string }): Observable<ExerciseDto> {
    return this.http.post<ExerciseDto>(this.base, exercise);
  }

  update(id: number, exercise: Partial<ExerciseDto> & { name: string }): Observable<ExerciseDto> {
    return this.http.put<ExerciseDto>(`${this.base}/${id}`, exercise);
  }

  delete(id: number): Observable<{ deleted: boolean }> {
    return this.http.delete<{ deleted: boolean }>(`${this.base}/${id}`);
  }
}
