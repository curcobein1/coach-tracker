import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { BackendSyncService } from './core/services/backend-sync.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrls: ['./app.scss'],
})
export class AppComponent {
  constructor(private sync: BackendSyncService) {}

  syncToBackend(): void {
    this.sync.importLocalStorageSnapshot().subscribe({
      next: () => {
        console.log('Synced localStorage → SQLite DB');
      },
      error: (e) => {
        console.error('Sync failed', e);
      },
    });
  }
}
