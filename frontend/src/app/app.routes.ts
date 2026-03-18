import { Routes } from '@angular/router';
import { TodayComponent } from './pages/today/today';
import { PlanComponent } from './pages/plan/plan';
import { NutritionComponent } from './pages/nutrition/nutrition';
import { DbComponent } from './pages/db/db';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'today' },
  { path: 'today', component: TodayComponent, title: 'Today' },
  { path: 'plan', component: PlanComponent, title: 'Plan' },
  { path: 'nutrition', component: NutritionComponent, title: 'Nutrition' },
  { path: 'db', component: DbComponent, title: 'Database' },
  { path: '**', redirectTo: 'today' },
];
