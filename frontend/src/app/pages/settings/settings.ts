import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ExerciseStatsStore } from '../../core/services/exercise-stats.store';
import {SHARED_IMPORTS} from '../../shared/shared-imports';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: SHARED_IMPORTS,
  templateUrl: './settings.html',
  styleUrls: ['./settings.scss'],
})
export class SettingsComponent {
  name = '';
  kg = 0;
  reps = 0;
  rir = 2;

  constructor(public stats: ExerciseStatsStore) {}

  addOrUpdate(): void {
    this.stats.upsertBaseline(this.name, Number(this.kg), Number(this.reps), Number(this.rir), 0.2);
    this.name = '';
    this.kg = 0;
    this.reps = 0;
    this.rir = 2;
  }

  remove(name: string): void {
    this.stats.remove(name);
  }
}
