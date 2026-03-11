import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PlanStore, PlanDay } from '../../core/services/plan.store';

@Component({
  selector: 'app-plan',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './plan.html',
  styleUrls: ['./plan.scss'],
})
export class PlanComponent implements OnInit {
  plan;
  selectedDayId: number;
  templates = [
    {
      kind: 'push' as const,
      title: 'Push',
      preview: ['Bench Press', 'Incline DB Press', 'Overhead Press', 'Lateral Raise', 'Triceps Pushdown'],
    },
    {
      kind: 'pull' as const,
      title: 'Pull',
      preview: ['Pull-up / Lat Pulldown', 'Chest Supported Row', 'One-Arm Row', 'Rear Delt Fly', 'Biceps Curl'],
    },
    {
      kind: 'legs' as const,
      title: 'Legs',
      preview: ['Squat / Leg Press', 'Romanian Deadlift', 'Leg Curl', 'Leg Extension', 'Calf Raise'],
    },
    {
      kind: 'upper' as const,
      title: 'Upper',
      preview: ['Bench Press', 'Row', 'Lat Pulldown', 'Overhead Press', 'Curl', 'Triceps Pushdown'],
    },
    {
      kind: 'lower' as const,
      title: 'Lower',
      preview: ['Squat / Leg Press', 'RDL', 'Split Squat', 'Calf Raise'],
    },
    {
      kind: 'torso' as const,
      title: 'Torso',
      preview: ['Incline Press', 'Row', 'Lat Pulldown', 'RDL', 'Lateral Raise', 'Abs'],
    },
    {
      kind: 'limbs' as const,
      title: 'Limbs',
      preview: ['Leg Press', 'Leg Curl', 'Lateral Raise', 'Biceps Curl', 'Triceps Pushdown', 'Calf Raise'],
    },
  ];

  get filteredItems() {
    if (this.filterGroup === 'All') return this.selectedDay.items;
    return this.selectedDay.items.filter((it) => it.group === this.filterGroup);
  }

  exerciseName = '';
  sets = 3;
  repRange = '6-10';
  rir: number | null = 2;
  group = 'Back';
  tagsText = '';
  filterGroup = 'All';

  constructor(private store: PlanStore) {
    this.plan = this.store.get();
    this.selectedDayId = this.plan.days[0]?.id ?? 0;
  }

  ngOnInit(): void {
      this.store.loadActivePlan();
    }

  get selectedDay(): PlanDay {
    return this.plan.days.find((d) => d.id === this.selectedDayId)!;
  }

  selectDay(id: number) {
    this.selectedDayId = id;
  }

  addItem() {
    const name = this.exerciseName.trim();
    if (!name) return;

    const tags = this.tagsText
      .split(',')
      .map((t) => t.trim())
      .filter(Boolean);

    this.store.addItem(this.selectedDayId, {
      exerciseName: name,
      sets: Number(this.sets) || 0,
      repRange: this.repRange.trim(),
      rir: this.rir === null ? null : Number(this.rir),
      group: this.group,
      tags,
    });

    this.exerciseName = '';
    this.tagsText = '';
    this.plan = this.store.get();
  }

  remove(itemId: number) {
    this.store.removeItem(this.selectedDayId, itemId);
    this.plan = this.store.get();
  }

  moveUp(itemId: number) {
    this.store.moveItem(this.selectedDayId, itemId, -1);
    this.plan = this.store.get();
  }

  moveDown(itemId: number) {
    this.store.moveItem(this.selectedDayId, itemId, 1);
    this.plan = this.store.get();
  }

  reset() {
    this.store.reset();
    this.plan = this.store.get();
    this.selectedDayId = this.plan.days[0]?.id ?? 0;
  }

  edit(itemId: number, patch: any) {
    this.store.updateItem(this.selectedDayId, itemId, patch);
    this.plan = this.store.get();
  }

  clearDay() {
    const ids = [...this.selectedDay.items.map((it) => it.id)];
    for (const id of ids) {
      this.store.removeItem(this.selectedDayId, id);
    }
    this.plan = this.store.get();
  }

  applyTemplate(kind: 'push' | 'pull' | 'legs' | 'upper' | 'lower' | 'torso' | 'limbs') {
    this.clearDay();

    const add = (
      exerciseName: string,
      sets: number,
      repRange: string,
      rir: number | null,
      group: string,
      tags: string[]
    ) => {
      this.store.addItem(this.selectedDayId, {
        exerciseName,
        sets,
        repRange,
        rir,
        group,
        tags,
      });
    };

    if (kind === 'pull') {
      add('Pull-up / Lat Pulldown', 4, '6-10', 2, 'Back', ['vertical_pull', 'lats']);
      add('Chest Supported Row', 4, '8-12', 2, 'Back', ['horizontal_pull', 'upper_back']);
      add('One-Arm Row', 3, '8-12', 2, 'Back', ['horizontal_pull', 'lats']);
      add('Rear Delt Fly', 3, '12-20', 2, 'Shoulders', ['rear_delts', 'upper_back']);
      add('Biceps Curl', 3, '10-15', 1, 'Arms', ['biceps', 'isolation']);
    }

    if (kind === 'push') {
      add('Bench Press', 4, '5-8', 2, 'Chest', ['press_horizontal', 'compound']);
      add('Incline Dumbbell Press', 3, '8-12', 2, 'Chest', ['upper_chest']);
      add('Overhead Press', 3, '6-10', 2, 'Shoulders', ['press_vertical']);
      add('Lateral Raise', 4, '12-20', 2, 'Shoulders', ['side_delts']);
      add('Triceps Pushdown', 3, '10-15', 1, 'Arms', ['triceps']);
    }

    if (kind === 'legs') {
      add('Squat / Leg Press', 4, '6-10', 2, 'Legs', ['squat_pattern', 'quads']);
      add('Romanian Deadlift', 4, '6-10', 2, 'Legs', ['hinge', 'hamstrings', 'glutes']);
      add('Leg Curl', 3, '10-15', 1, 'Legs', ['hamstrings']);
      add('Leg Extension', 3, '10-15', 1, 'Legs', ['quads']);
      add('Calf Raise', 4, '10-20', 1, 'Legs', ['calves']);
    }

    if (kind === 'upper') {
      add('Bench Press', 3, '5-8', 2, 'Chest', ['press_horizontal']);
      add('Row', 3, '6-10', 2, 'Back', ['horizontal_pull']);
      add('Lat Pulldown', 3, '8-12', 2, 'Back', ['vertical_pull']);
      add('Overhead Press', 2, '6-10', 2, 'Shoulders', ['press_vertical']);
      add('Curl', 2, '10-15', 1, 'Arms', ['biceps']);
      add('Triceps Pushdown', 2, '10-15', 1, 'Arms', ['triceps']);
    }

    if (kind === 'lower') {
      add('Squat / Leg Press', 4, '6-10', 2, 'Legs', ['squat_pattern', 'quads']);
      add('RDL', 4, '6-10', 2, 'Legs', ['hinge', 'hamstrings']);
      add('Split Squat', 3, '8-12', 2, 'Legs', ['unilateral', 'quads', 'glutes']);
      add('Calf Raise', 4, '10-20', 1, 'Legs', ['calves']);
    }

    if (kind === 'torso') {
      add('Incline Dumbbell Press', 4, '6-10', 2, 'Chest', ['upper_chest', 'press_horizontal']);
      add('Row', 4, '6-10', 2, 'Back', ['horizontal_pull', 'upper_back']);
      add('Lat Pulldown / Pull-up', 3, '8-12', 2, 'Back', ['vertical_pull', 'lats']);
      add('Romanian Deadlift', 3, '6-10', 2, 'Legs', ['hinge', 'hamstrings', 'glutes']);
      add('Lateral Raise', 4, '12-20', 2, 'Shoulders', ['side_delts']);
      add('Hanging Leg Raise / Cable Crunch', 3, '10-15', 1, 'Core', ['abs']);
    }

    if (kind === 'limbs') {
      add('Leg Press / Hack Squat', 4, '8-12', 2, 'Legs', ['quads']);
      add('Leg Curl', 4, '10-15', 1, 'Legs', ['hamstrings']);
      add('Lateral Raise', 4, '12-20', 2, 'Shoulders', ['side_delts']);
      add('Biceps Curl', 4, '8-12', 1, 'Arms', ['biceps']);
      add('Triceps Pushdown', 4, '8-12', 1, 'Arms', ['triceps']);
      add('Calf Raise', 4, '10-20', 1, 'Legs', ['calves']);
    }

    this.plan = this.store.get();
  }
}
