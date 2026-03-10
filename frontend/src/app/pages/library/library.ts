import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ExerciseLibraryStore, MuscleGroup, Exercise } from '../../core/services/exercise-library.store';

@Component({
  selector: 'app-library',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './library.html',
  styleUrls: ['./library.scss'],
})
export class LibraryComponent {
  q = '';
  group: MuscleGroup | 'All' = 'All';

  name = '';
  newGroup: MuscleGroup = 'Back';
  tagsText = '';

  constructor(public lib: ExerciseLibraryStore) {}

  get list(): Exercise[] {
    const all = this.lib.getAll();
    const q = this.q.trim().toLowerCase();

    return all.filter((x) => {
      if (this.group !== 'All' && x.group !== this.group) return false;
      if (!q) return true;

      return (
        x.name.toLowerCase().includes(q) ||
        x.tags.some((t) => t.toLowerCase().includes(q))
      );
    });
  }

  add() {
    const name = this.name.trim();
    if (!name) return;

    const tags = this.tagsText
      .split(',')
      .map((t) => t.trim())
      .filter(Boolean);

    this.lib.add(name, this.newGroup, tags);
    this.name = '';
    this.tagsText = '';
  }

  remove(id: string) {
    this.lib.remove(id);
  }

  resetSeed() {
    this.lib.resetSeed();
  }
}
