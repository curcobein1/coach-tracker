import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class BackendSyncService {
  private base = `${window.location.protocol}//${window.location.hostname}:5050/api`;

  constructor(private http: HttpClient) {}

  importLocalStorageSnapshot() {
    const kv: Record<string, any> = {};
    const keys = [
      'coach-tracker.dayLogs.v1',
      'coach-tracker.exerciseStats.v1',
      'coach-tracker.plan.v1',
    ];

    for (const k of keys) {
      const raw = localStorage.getItem(k);
      if (!raw) continue;
      try {
        kv[k] = JSON.parse(raw);
      } catch {
        // ignore
      }
    }

    const nutritionDaysRaw = localStorage.getItem('coach-tracker.nutrition.days.v1');
    const nutritionUsualsRaw = localStorage.getItem('coach-tracker.nutrition.usuals.v1');

    let nutritionDays: any = {};
    let nutritionUsuals: any[] = [];

    try {
      nutritionDays = nutritionDaysRaw ? JSON.parse(nutritionDaysRaw) : {};
    } catch {
      nutritionDays = {};
    }
    try {
      nutritionUsuals = nutritionUsualsRaw ? JSON.parse(nutritionUsualsRaw) : [];
    } catch {
      nutritionUsuals = [];
    }

    return this.http.post(`${this.base}/sync/import`, {
      kv,
      nutritionDays,
      nutritionUsuals,
    });
  }
}

