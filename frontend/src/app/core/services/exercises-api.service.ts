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
}
