import { Component, inject, ChangeDetectorRef, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
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

  constructor() {
    effect(() => {
      // Ensuring the view refresh when NutritionStore signals update
      this.nutrition.usuals();
      this.cdr.markForCheck();
    });
  }

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

  isUsualFood(food: FoodSearchItem): boolean {
    return this.usuals.some((u) => u.fdcId === food.fdcId);
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

    this.foodApi
      .getFoodDetail(food.fdcId)
      .pipe(
        finalize(() => {
          this.isLoadingDetail = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (detail) => {
          if (!detail) {
            this.selectedFood = null;
            this.error = 'Food details could not be loaded';
            return;
          }
          this.selectedFood = { ...detail };
          this.grams = 100;
        },
        error: (err) => {
          console.error('food detail failed', err);

          if (err.status === 404) {
            this.error = 'Details are not available for this food item.';
          } else {
            this.error = 'Food details could not be loaded';
          }

          this.selectedFood = null;
        },
      });
  }

  toggleResultUsual(food: FoodSearchItem): void {
    if (this.isUsualFood(food)) {
      this.nutrition.removeUsual(food.fdcId);
      this.cdr.markForCheck();
      return;
    }

    const g = 100;
    const scale = (v?: number | null) => v ?? 0;

    this.nutrition.addOrUpdateUsual({
      fdcId: food.fdcId,
      name: food.description,
      grams: g,
      kcal: scale(food.calories),
      p: scale(food.protein),
      c: scale(food.carbs),
      f: scale(food.fat),
    });
    this.cdr.markForCheck();
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

    const micros: Record<string, { label: string; unit: string; value: number }> = {};
    const addMicro = (key: string, label: string, unit: string, value?: number | null) => {
      const v = scale(value);
      if (!Number.isFinite(v) || v === 0) return;
      micros[key] = { label, unit, value: v };
    };

    for (const item of (this.selectedFood.micronutrients?.vitamins ?? [])) {
      addMicro(item.key, item.label, item.unit, item.value);
    }
    for (const item of (this.selectedFood.micronutrients?.majorMinerals ?? [])) {
      addMicro(item.key, item.label, item.unit, item.value);
    }
    for (const item of (this.selectedFood.micronutrients?.traceMinerals ?? [])) {
      addMicro(item.key, item.label, item.unit, item.value);
    }

    // Helpful "other" targets we commonly care about
    addMicro('fiber', 'Fiber', 'g', s.fiber ?? undefined);
    addMicro('sodium', 'Sodium', 'mg', s.sodium ?? undefined);
    addMicro('potassium', 'Potassium', 'mg', s.potassium ?? undefined);

    this.nutrition.addFood(
      {
        fdcId: this.selectedFood.fdcId,
        name: this.selectedFood.description,
        grams: g,
        kcal: scale(s.calories ?? undefined),
        p: scale(s.protein ?? undefined),
        c: scale(s.carbs ?? undefined),
        f: scale(s.fat ?? undefined),
        micros,
      },
      undefined
    );
    this.cdr.markForCheck();
  }

  toggleSelectedUsual(): void {
    if (!this.selectedFood) return;
    const id = this.selectedFood.fdcId;
    const current = this.nutrition.getUsuals();

    if (current.some((u) => u.fdcId === id)) {
      this.nutrition.removeUsual(id);
      this.cdr.markForCheck();
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
    this.cdr.markForCheck();
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
    this.cdr.markForCheck();
  }

  toggleUsualFromList(u: UsualFood): void {
    this.nutrition.removeUsual(u.fdcId);
    this.cdr.markForCheck();
  }
}

