import { Component, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  FoodApiService,
  FoodSearchItem,
  FoodDetail,
} from '../../core/services/food-api.service';

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

  query = '';
  results: FoodSearchItem[] = [];
  selectedFood: FoodDetail | null = null;
  selectedFoodId: number | null = null;

  isSearching = false;
  isLoadingDetail = false;
  error = '';

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
}

