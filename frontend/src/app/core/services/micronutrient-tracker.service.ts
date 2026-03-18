import { Injectable } from '@angular/core';
import { LoggedFood } from '../models/nutrition.models';
import { DEFAULT_MICRO_TARGETS, MicroTarget } from '../utils/micronutrient-requirements';

export type MicroTotals = Record<string, { label: string; unit: string; value: number }>;
export type MicroProgressItem = { key: string; label: string; unit: string; value: number; target: number; pct: number };

export interface MicroDayResult {
  totals: MicroTotals;
  progressList: MicroProgressItem[];
}

@Injectable({ providedIn: 'root' })
export class MicronutrientTrackerService {
  private targets: MicroTarget[] = DEFAULT_MICRO_TARGETS;

  computeForDay(foods: LoggedFood[]): MicroDayResult {
    const totals: MicroTotals = {};

    for (const food of foods) {
      const micros = food.micros ?? null;
      if (!micros) continue;
      for (const [k, m] of Object.entries(micros)) {
        if (!m) continue;
        const v = Number(m.value) || 0;
        if (!Number.isFinite(v) || v === 0) continue;
        const existing = totals[k];
        totals[k] = {
          label: m.label,
          unit: m.unit,
          value: (existing?.value ?? 0) + v,
        };
      }
    }

    const progressList: MicroProgressItem[] = this.targets
      .map(t => {
        const v = totals[t.key]?.value ?? 0;
        const pct = t.target > 0 ? (v / t.target) * 100 : 0;
        return { key: t.key, label: t.label, unit: t.unit, value: v, target: t.target, pct };
      })
      .sort((a, b) => b.pct - a.pct);

    return { totals, progressList };
  }
}

