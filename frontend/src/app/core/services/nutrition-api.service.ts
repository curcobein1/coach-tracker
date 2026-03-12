import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface NutritionFoodLogDto {
  id: string;
  dayKey: string;
  fdcId: number;
  name: string;
  grams: number;
  kcal: number;
  p: number;
  c: number;
  f: number;
  loggedAtUtc: string;
}

export interface NutritionUsualDto {
  fdcId: number;
  name: string;
  grams: number;
  kcal: number;
  p: number;
  c: number;
  f: number;
  updatedAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class NutritionApiService {
  private baseUrl = 'http://localhost:5106/api/nutrition';

  constructor(private http: HttpClient) {}

  getDay(dayKey: string): Observable<{ dayKey: string; foods: NutritionFoodLogDto[] }> {
    return this.http.get<{ dayKey: string; foods: NutritionFoodLogDto[] }>(`${this.baseUrl}/days/${dayKey}`);
  }

  addFood(dayKey: string, body: Omit<NutritionFoodLogDto, 'id' | 'dayKey' | 'loggedAtUtc'>) {
    return this.http.post<NutritionFoodLogDto>(`${this.baseUrl}/days/${dayKey}/foods`, { dayKey, ...body });
  }

  deleteFood(dayKey: string, id: string) {
    return this.http.delete(`${this.baseUrl}/days/${dayKey}/foods/${id}`);
  }

  getUsuals(): Observable<{ usuals: NutritionUsualDto[] }> {
    return this.http.get<{ usuals: NutritionUsualDto[] }>(`${this.baseUrl}/usuals`);
  }

  upsertUsual(fdcId: number, body: Omit<NutritionUsualDto, 'updatedAtUtc'>) {
    return this.http.put<NutritionUsualDto>(`${this.baseUrl}/usuals/${fdcId}`, body);
  }

  deleteUsual(fdcId: number) {
    return this.http.delete(`${this.baseUrl}/usuals/${fdcId}`);
  }
}

