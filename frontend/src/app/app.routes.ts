import { Routes } from '@angular/router';
import { TodayComponent } from './pages/today/today';
import { PlanComponent } from './pages/plan/plan';
import { NutritionComponent} from './pages/nutrition/nutrition';
import { SettingsComponent } from './pages/settings/settings';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'today' },
  { path: 'today', component: TodayComponent, title: 'Today' },
  { path: 'plan', component: PlanComponent, title: 'Plan' },
  { path: 'nutrition', component: NutritionComponent, title: 'Nutrition' },
  { path: 'settings', component: SettingsComponent, title: 'Settings' },
  { path: '**', redirectTo: 'today' },
];
