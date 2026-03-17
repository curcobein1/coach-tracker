import { Component, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  FoodApiService,
  FoodSearchItem,
  FoodDetail,
} from '../../core/services/food-api.service';
import { NutritionStore } from '../../core/services/nutrition.store';
import { UsualFood } from '../../core/models/nutrition.models';

@Component({
  selector: 'app-nutrition',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './nutrition.html',
  styleUrl: './nutrition.scss',
})
export class NutritionComponent {
  private foodApi = inject(FoodApiService);
  private cdr = inject(ChangeDetectorRef);
  private nutrition = inject(NutritionStore);

  query = '';
  results: FoodSearchItem[] = [];
  selectedFood: FoodDetail | null = null;
  selectedFoodId: number | null = null;

  isSearching = false;
  isLoadingDetail = false;
  error = '';

  grams = 100;

  get usuals(): UsualFood[] {
    return this.nutrition.getUsuals();
  }

  searchFoods() {
    const trimmed = this.query.trim();

    if (!trimmed) {
      this.results = [];
      this.selectedFood = null;
      this.error = '';
      this.cdr.detectChanges();
      return;
    }

    this.isSearching = true;
    this.error = '';
    this.results = [];
    this.selectedFood= null;
    this.selectedFoodId = null ;
    this.cdr.detectChanges();

    this.foodApi.searchFoods({ query: trimmed, pageSize: 20 }).subscribe({
      next: (response) => {
        this.results = [...(response.foods ?? [])];
        this.isSearching = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('food search failed', err);
        this.error = 'Food search failed.';
        this.results = [];
        this.isSearching = false;
        this.cdr.detectChanges();
      },
    });
  }

  selectFood(food: FoodSearchItem) {
    this.selectedFoodId = food.fdcId;
    this.selectedFood = null;
    this.isLoadingDetail = true;
    this.error = '';
    this.cdr.detectChanges();

    this.foodApi.getFoodDetail(food.fdcId).subscribe({
      next: (detail) => {
        this.selectedFood = { ...detail };
        this.isLoadingDetail = false;
        this.grams = 100;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('food detail failed', err);

        if(err.status === 404){
          this.error = 'Details are not available for this food item.';
        } else {
          this.error ='Food details could not be loaded';
          }

        this.selectedFood = null;
        this.isLoadingDetail = false;
        this.cdr.detectChanges();
      },
    });
  }

  trackByFoodId(_: number, item: FoodSearchItem) {
    return item.fdcId;
  }

  trackByNutrientKey(_: number, item: { key: string }) {
    return item.key;
  }

  get isSelectedUsual(): boolean {
    if (!this.selectedFood) return false;
    const id = this.selectedFood.fdcId;
    return this.nutrition.getUsuals().some((u: UsualFood) => u.fdcId === id);
  }

  addSelectedToToday(): void {
    if (!this.selectedFood) return;
    const g = Number(this.grams) || 0;
    const s = this.selectedFood.summary;

    const scale = (v?: number | null) => (v ?? 0) * (g / 100);

    this.nutrition.addFood(
      {
        fdcId: this.selectedFood.fdcId,
        name: this.selectedFood.description,
        grams: g,
        kcal: scale(s.calories ?? undefined),
        p: scale(s.protein ?? undefined),
        c: scale(s.carbs ?? undefined),
        f: scale(s.fat ?? undefined),
      },
      undefined
    );
  }

  toggleSelectedUsual(): void {
    if (!this.selectedFood) return;
    const id = this.selectedFood.fdcId;
    const current = this.nutrition.getUsuals();

    if (current.some((u) => u.fdcId === id)) {
      this.nutrition.removeUsual(id);
      return;
    }

    const s = this.selectedFood.summary;
    const g = 100;
    const scale = (v?: number | null) => (v ?? 0);

    this.nutrition.addOrUpdateUsual({
      fdcId: id,
      name: this.selectedFood.description,
      grams: g,
      kcal: scale(s.calories ?? undefined),
      p: scale(s.protein ?? undefined),
      c: scale(s.carbs ?? undefined),
      f: scale(s.fat ?? undefined),
    });
  }

  useUsual(u: UsualFood): void {
    const scale = (grams: number, base: number) =>
      (base ?? 0) * (grams / (u.grams || 100));

    const g = u.grams || 100;

    this.nutrition.addFood(
      {
        fdcId: u.fdcId,
        name: u.name,
        grams: g,
        kcal: scale(g, u.kcal),
        p: scale(g, u.p),
        c: scale(g, u.c),
        f: scale(g, u.f),
      },
      undefined
    );
  }
}

