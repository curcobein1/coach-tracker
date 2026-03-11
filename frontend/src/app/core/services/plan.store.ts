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

  loadActivePlan(): void {
    this.api.getActivePlan().subscribe({
      next: (plan) => {
        this.state = this.mapFromDto(plan);
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
    return {
      id: dto.id,
      name: dto.name,
      days: dto.days.map((d) => ({
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

          exerciseName: `Exercise ${i.exerciseId}`,
          sets: i.targetSets,
          repRange: i.targetReps ?? null,
          rir: null,
          tags: [],
          group: null
        }))
      }))
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
          orderIndex: i.orderIndex,
          targetSets: i.targetSets ?? i.sets,
          targetReps: i.targetReps ?? i.repRange ?? null,
          targetRestSeconds: i.targetRestSeconds ?? null,
          notes: i.notes ?? null
        }))
      }))
    };
  }
}
