import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TrainingPlanItemDto {
  id: number;
  exerciseId: number;
  exerciseName?: string | null;
  orderIndex: number;
  targetSets: number;
  targetReps?: string | null;
  targetRestSeconds?: number | null;
  notes?: string | null;
}

export interface TrainingPlanDayDto {
  id: number;
  dayOfWeek: number;
  name: string;
  items: TrainingPlanItemDto[];
}

export interface TrainingPlanDto {
  id: number;
  name: string;
  days: TrainingPlanDayDto[];
}

@Injectable({
  providedIn: 'root'
})
export class PlansApiService {
  private baseUrl = 'http://localhost:5106/api/plans';

  constructor(private http: HttpClient) {}

  getActivePlan(): Observable<TrainingPlanDto> {
    return this.http.get<TrainingPlanDto>(`${this.baseUrl}/active`);
  }

  saveActivePlan(plan: TrainingPlanDto): Observable<TrainingPlanDto> {
    return this.http.put<TrainingPlanDto>(`${this.baseUrl}/active`, plan);
  }
}
