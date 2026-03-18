import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ExercisesApiService, ExerciseDto } from '../../core/services/exercises-api.service';
import { DbAdminApiService, DbRowsResponse, DbSchemaResponse, DbSqlResponse } from '../../core/services/db-admin-api.service';

@Component({
  selector: 'app-db',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './db.html',
  styleUrl: './db.scss',
})
export class DbComponent implements OnInit {
  exercises: ExerciseDto[] = [];
  loading = false;
  error: string | null = null;

  tables: string[] = [];
  selectedTable: string | null = null;
  tableSchema: DbSchemaResponse | null = null;
  tableRows: DbRowsResponse | null = null;
  tableLimit = 100;
  tableOffset = 0;

  sqlText = 'select name from sqlite_master where type = \"table\";';
  sqlResult: DbSqlResponse | null = null;

  newName = '';
  newDefaultSets: number | null = 3;

  editingId: number | null = null;
  editName = '';
  editDefaultSets: number | null = null;

  constructor(
    private exercisesApi: ExercisesApiService,
    private dbAdmin: DbAdminApiService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.refresh();
    this.loadTables();
  }

  refresh(): void {
    this.loading = true;
    this.error = null;
    this.exercisesApi.list().subscribe({
      next: (list) => {
        this.exercises = list ?? [];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (e) => {
        console.error(e);
        this.error = 'Failed to load exercises';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  startEdit(e: ExerciseDto): void {
    this.editingId = e.id;
    this.editName = e.name;
    this.editDefaultSets = e.defaultSets ?? null;
    this.cdr.markForCheck();
  }

  cancelEdit(): void {
    this.editingId = null;
    this.editName = '';
    this.editDefaultSets = null;
    this.cdr.markForCheck();
  }

  saveEdit(): void {
    if (!this.editingId) return;
    const name = this.editName.trim();
    if (!name) return;
    const id = this.editingId;
    const payload = { name, defaultSets: this.editDefaultSets };
    this.exercisesApi.update(id, payload).subscribe({
      next: () => {
        this.cancelEdit();
        this.refresh();
      },
      error: (e) => {
        console.error(e);
        this.error = 'Failed to update exercise';
        this.cdr.detectChanges();
      }
    });
  }

  addExercise(): void {
    const name = this.newName.trim();
    if (!name) return;
    const payload = { name, defaultSets: this.newDefaultSets };
    this.exercisesApi.create(payload).subscribe({
      next: () => {
        this.newName = '';
        this.refresh();
      },
      error: (e) => {
        console.error(e);
        this.error = 'Failed to create exercise';
        this.cdr.detectChanges();
      }
    });
  }

  deleteExercise(id: number): void {
    this.exercisesApi.delete(id).subscribe({
      next: () => this.refresh(),
      error: (e) => {
        console.error(e);
        this.error = 'Failed to delete exercise';
        this.cdr.detectChanges();
      }
    });
  }

  loadTables(): void {
    this.dbAdmin.tables().subscribe({
      next: (r) => {
        this.tables = r.tables ?? [];
        if (!this.selectedTable && this.tables.length > 0) {
          this.selectTable(this.tables[0]);
        }
        this.cdr.detectChanges();
      },
      error: (e) => {
        console.error(e);
        this.error = 'Failed to load table list';
        this.cdr.detectChanges();
      }
    });
  }

  selectTable(name: string): void {
    this.selectedTable = name;
    this.tableOffset = 0;
    this.loadSelectedTable();
  }

  loadSelectedTable(): void {
    if (!this.selectedTable) return;
    const table = this.selectedTable;
    this.dbAdmin.schema(table).subscribe({
      next: (s) => {
        this.tableSchema = s;
        this.cdr.markForCheck();
      },
      error: (e) => {
        console.error(e);
        this.tableSchema = null;
        this.cdr.markForCheck();
      }
    });
    this.dbAdmin.rows(table, this.tableLimit, this.tableOffset).subscribe({
      next: (r) => {
        this.tableRows = r;
        this.cdr.detectChanges();
      },
      error: (e) => {
        console.error(e);
        this.tableRows = null;
        this.cdr.detectChanges();
      }
    });
  }

  nextPage(): void {
    this.tableOffset += this.tableLimit;
    this.loadSelectedTable();
  }

  prevPage(): void {
    this.tableOffset = Math.max(0, this.tableOffset - this.tableLimit);
    this.loadSelectedTable();
  }

  runSql(): void {
    const sql = (this.sqlText ?? '').trim();
    if (!sql) return;
    this.dbAdmin.sql(sql).subscribe({
      next: (r) => {
        this.sqlResult = r;
        this.error = null;
        this.cdr.detectChanges();
      },
      error: (e) => {
        console.error(e);
        this.sqlResult = null;
        this.error = 'SQL failed';
        this.cdr.detectChanges();
      }
    });
  }

  get sqlRowsAffected(): number | null {
    if (!this.sqlResult || this.sqlResult.kind !== 'nonQuery') return null;
    return this.sqlResult.rowsAffected;
  }

  get sqlColumns(): string[] {
    if (!this.sqlResult || this.sqlResult.kind !== 'rows') return [];
    return this.sqlResult.columns;
  }

  get sqlRows(): any[][] {
    if (!this.sqlResult || this.sqlResult.kind !== 'rows') return [];
    return this.sqlResult.rows;
  }
}

