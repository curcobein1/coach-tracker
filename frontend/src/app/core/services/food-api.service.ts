import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface FoodSearchRequest {
  query: string;
  pageSize?: number;
}

export interface FoodSearchItem {
  fdcId: number;
  description: string;
  foodCategory?: string | null;
  brandName?: string | null;
  calories?: number | null;
  protein?: number | null;
  carbs?: number | null;
  fat?: number | null;
}

export interface FoodSearchResponse {
  query: string;
  pageSize: number;
  foods: FoodSearchItem[];
}

export interface NutrientItem {
  key: string;
  label: string;
  unit: string;
  value?: number | null;
}

export interface FoodSummary {
  calories?: number | null;
  protein?: number | null;
  carbs?: number | null;
  fat?: number | null;
  fiber?: number | null;
  sugar?: number | null;
  sodium?: number | null;
  potassium?: number | null;
}

export interface FoodDetail {
  fdcId: number;
  description: string;
  foodCategory?: string | null;
  summary: FoodSummary;
  micronutrients: {
    vitamins: NutrientItem[];
    majorMinerals: NutrientItem[];
    traceMinerals: NutrientItem[];
  };
  other: NutrientItem[];
}

@Injectable({
  providedIn: 'root',
})
export class FoodApiService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api/foods`;

  searchFoods(request: FoodSearchRequest) {
    return this.http.post<FoodSearchResponse>(`${this.baseUrl}/search`, request);
  }

  getFoodDetail(fdcId: number) {
    return this.http.get<FoodDetail>(`${this.baseUrl}/${fdcId}`);
  }
}
