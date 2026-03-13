import { Injectable } from '@angular/core';
import { PlansApiService, TrainingPlanDto } from './plans-api.service';

export interface PlanItem {
  id: number;
  exerciseId: number;
  orderIndex: number;

  targetSets: number;
  targetReps?: string | null;
  targetRestSeconds?: number | null;
  notes?: string | null;

  // legacy UI compatibility
  exerciseName: string;
  sets: number;
  repRange?: string | null;
  rir: number | null;
  tags: string[];
  group?: string | null;
}

export interface PlanDay {
  id: number;
  dayOfWeek: number;
  name: string;

  // legacy UI compatibility
  title: string;

  items: PlanItem[];
}

export interface PlanState {
  id: number;
  name: string;
  days: PlanDay[];
}

@Injectable({
  providedIn: 'root'
})
export class PlanStore {
  private state: PlanState = {
    id: 0,
    name: 'Active Plan',
    days: []
  };

  constructor(private api: PlansApiService) {}

  get(): PlanState {
    return structuredClone(this.state);
  }

  loadActivePlan(done?: () => void): void {
    this.api.getActivePlan().subscribe({
      next: (plan) => {
        this.state = this.mapFromDto(plan);
        if (this.state.days.length === 0) {
          this.state.days = [
            { id: 1, dayOfWeek: 1, name: 'Monday',    title: 'Monday',    items: [] },
            { id: 2, dayOfWeek: 2, name: 'Tuesday',   title: 'Tuesday',   items: [] },
            { id: 3, dayOfWeek: 3, name: 'Wednesday', title: 'Wednesday', items: [] },
            { id: 4, dayOfWeek: 4, name: 'Thursday',  title: 'Thursday',  items: [] },
            { id: 5, dayOfWeek: 5, name: 'Friday',    title: 'Friday',    items: [] },
            { id: 6, dayOfWeek: 6, name: 'Saturday',  title: 'Saturday',  items: [] },
            { id: 7, dayOfWeek: 7, name: 'Sunday',    title: 'Sunday',    items: [] },
          ];
        }
        if (done) done();
      },
      error: (err) => {
        console.error('Failed to load active plan', err);
      }
    });
  }

  saveActivePlan(): void {
    const dto = this.mapToDto(this.state);

    this.api.saveActivePlan(dto).subscribe({
      next: (saved) => {
        this.state = this.mapFromDto(saved);
      },
      error: (err) => {
        console.error('Failed to save active plan', err);
      }
    });
  }

  addItem(dayId: number, item: Partial<PlanItem>): void {
    const day = this.state.days.find((d) => d.id === dayId);
    if (!day) return;

    const nextId =
      day.items.length > 0 ? Math.max(...day.items.map((i) => i.id)) + 1 : 1;

    const nextOrder =
      day.items.length > 0 ? Math.max(...day.items.map((i) => i.orderIndex)) + 1 : 0;

    day.items.push({
      id: item.id ?? nextId,
      exerciseId: item.exerciseId ?? 0,
      orderIndex: item.orderIndex ?? nextOrder,

      targetSets: item.targetSets ?? 3,
      targetReps: item.targetReps ?? '8-12',
      targetRestSeconds: item.targetRestSeconds ?? null,
      notes: item.notes ?? null,

      exerciseName: item.exerciseName ?? `Exercise ${item.exerciseId ?? 0}`,
      sets: item.sets ?? item.targetSets ?? 3,
      repRange: item.repRange ?? item.targetReps ?? '8-12',
      rir: item.rir ?? null,
      tags: item.tags ?? [],
      group: item.group ?? null
    });
  }

  removeItem(dayId: number, itemId: number): void {
    const day = this.state.days.find((d) => d.id === dayId);
    if (!day) return;

    day.items = day.items.filter((i) => i.id !== itemId);
    this.reindex(day);
  }

  moveItem(dayId: number, itemId: number, direction: number): void {
    const day = this.state.days.find((d) => d.id === dayId);
    if (!day) return;

    const index = day.items.findIndex((i) => i.id === itemId);
    if (index === -1) return;

    const newIndex = index + direction;
    if (newIndex < 0 || newIndex >= day.items.length) return;

    [day.items[index], day.items[newIndex]] = [day.items[newIndex], day.items[index]];
    this.reindex(day);
  }

  updateItem(dayId: number, itemId: number, patch: Partial<PlanItem>): void {
    const day = this.state.days.find((d) => d.id === dayId);
    if (!day) return;

    const item = day.items.find((i) => i.id === itemId);
    if (!item) return;

    Object.assign(item, patch);

    // keep legacy + backend fields aligned
    if (patch.targetSets !== undefined) item.sets = patch.targetSets;
    if (patch.targetReps !== undefined) item.repRange = patch.targetReps;
    if (patch.sets !== undefined) item.targetSets = patch.sets;
    if (patch.repRange !== undefined) item.targetReps = patch.repRange;
  }

  reset(): void {
    this.state = {
      id: this.state.id,
      name: this.state.name,
      days: this.state.days.map((d) => ({
        ...d,
        items: []
      }))
    };
  }

  private reindex(day: PlanDay): void {
    day.items.forEach((item, index) => {
      item.orderIndex = index;
    });
  }

  private mapFromDto(dto: TrainingPlanDto): PlanState {
    const dayNames: Record<number, string> = {
      1: 'Monday', 2: 'Tuesday', 3: 'Wednesday', 4: 'Thursday',
      5: 'Friday', 6: 'Saturday', 7: 'Sunday'
    };
    const fromDto = dto.days.map((d) => ({
      id: d.id,
      dayOfWeek: d.dayOfWeek,
      name: d.name,
      title: d.name,
      items: d.items.map((i) => ({
        id: i.id,
        exerciseId: i.exerciseId,
        orderIndex: i.orderIndex,
        targetSets: i.targetSets,
        targetReps: i.targetReps ?? null,
        targetRestSeconds: i.targetRestSeconds ?? null,
        notes: i.notes ?? null,

        exerciseName: i.exerciseName && i.exerciseName.trim().length > 0
          ? i.exerciseName
          : `Exercise ${i.exerciseId}`,
        sets: i.targetSets,
        repRange: i.targetReps ?? null,
        rir: null,
        tags: [],
        group: null
      }))
    }));
    // Ensure exactly 7 days (1=Mon..7=Sun) so Plan and Today stay in sync
    const byDayOfWeek = new Map(fromDto.map((d) => [d.dayOfWeek, d]));
    const days: PlanDay[] = [];
    for (let dow = 1; dow <= 7; dow++) {
      const existing = byDayOfWeek.get(dow);
      days.push(existing ?? {
        id: 0,
        dayOfWeek: dow,
        name: dayNames[dow],
        title: dayNames[dow],
        items: []
      });
    }
    return {
      id: dto.id,
      name: dto.name,
      days
    };
  }

  private mapToDto(state: PlanState): TrainingPlanDto {
    return {
      id: state.id,
      name: state.name,
      days: state.days.map((d) => ({
        id: d.id,
        dayOfWeek: d.dayOfWeek,
        name: d.name || d.title,
        items: d.items.map((i) => ({
          id: i.id,
          exerciseId: i.exerciseId,
          exerciseName: i.exerciseName,
          orderIndex: i.orderIndex,
          targetSets: i.targetSets ?? i.sets,
          // Backend expects numeric reps (int?). Our UI uses string ranges like "6-10",
          // so we currently persist reps only in the UI fields and leave TargetReps null
          // to avoid model-binding errors.
          targetReps: null,
          targetRestSeconds: i.targetRestSeconds ?? null,
          notes: i.notes ?? null
        }))
      }))
    };
  }
}
