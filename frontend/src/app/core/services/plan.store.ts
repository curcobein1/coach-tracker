import { Injectable } from '@angular/core';
import { PlansApiService } from './plans-api.service';


export type PlanItem = {
  id: string;
  exerciseName: string;
  sets: number;
  repRange: string;
  rir: number | null;
  group: string;
  tags: string[];
  notes?: string;
};

export type PlanDay = {
  id: string;
  title: string;
  items: PlanItem[];
};

export type WeeklyPlan = {
  version: number;
  days: PlanDay[];
  updatedAt: string;
};

function uid(): string {
  return Math.random().toString(36).slice(2, 9) + Date.now().toString(36);
}


const KEY = 'coach_tracker_weekly_plan_v1';

function defaultPlan(): WeeklyPlan {
  const dayTitles = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
  return {
    version: 1,
    updatedAt: new Date().toISOString(),
    days: dayTitles.map((t) => ({
      id: t.toLowerCase(),
      title: t,
      items: [],
    })),
  };
}

@Injectable({ providedIn: 'root' })
export class PlanStore {
  activePlan: any = null;
  private plan: WeeklyPlan = this.load();

  constructor(private plansApi: PlansApiService) {}

  loadActivePlan() {
    this.plansApi.getActivePlan().subscribe((data: any) => {
      console.log('[FRONTEND] active plan response', data);
      this.activePlan = data;
    });
  }

  get(): WeeklyPlan {
    return this.plan;
  }

  set(next: WeeklyPlan) {
    this.plan = { ...next, updatedAt: new Date().toISOString() };
    localStorage.setItem(KEY, JSON.stringify(this.plan));
  }

  reset() {
    this.set(defaultPlan());
  }

  addItem(dayId: string, item: Omit<PlanItem, 'id'>) {
    const plan = this.get();
    const days = plan.days.map((d) => {
      if (d.id !== dayId) return d;
      return {
        ...d,
        items: [...d.items, { id: uid(), ...item }],
      };
    });
    this.set({ ...plan, days });
  }

  updateItem(dayId: string, itemId: string, patch: Partial<PlanItem>) {
    const plan = this.get();
    const days = plan.days.map((d) => {
      if (d.id !== dayId) return d;
      return {
        ...d,
        items: d.items.map((it) => (it.id === itemId ? { ...it, ...patch } : it)),
      };
    });
    this.set({ ...plan, days });
  }

  removeItem(dayId: string, itemId: string) {
    const plan = this.get();
    const days = plan.days.map((d) => {
      if (d.id !== dayId) return d;
      return {
        ...d,
        items: d.items.filter((it) => it.id !== itemId),
      };
    });
    this.set({ ...plan, days });
  }

  moveItem(dayId: string, itemId: string, dir: -1 | 1) {
    const plan = this.get();
    const days = plan.days.map((d) => {
      if (d.id !== dayId) return d;

      const idx = d.items.findIndex((x) => x.id === itemId);
      if (idx < 0) return d;

      const nextIdx = idx + dir;
      if (nextIdx < 0 || nextIdx >= d.items.length) return d;

      const items = [...d.items];
      const [removed] = items.splice(idx, 1);
      items.splice(nextIdx, 0, removed);

      return { ...d, items };
    });

    this.set({ ...plan, days });
  }

  getDayByDate(date: Date): PlanDay {
    const map = ['sun', 'mon', 'tue', 'wed', 'thu', 'fri', 'sat'];
    const id = map[date.getDay()];
    return this.plan.days.find((d) => d.id === id) ?? this.plan.days[0];
  }

  private load(): WeeklyPlan {
    try {
      const raw = localStorage.getItem(KEY);
      if (!raw) return defaultPlan();
      const parsed = JSON.parse(raw);
      if (!parsed?.days?.length) return defaultPlan();
      return parsed as WeeklyPlan;
    } catch {
      return defaultPlan();
    }
  }
}
