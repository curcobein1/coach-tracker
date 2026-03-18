import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export type DbTablesResponse = { tables: string[] };
export type DbSchemaResponse = { table: string; columns: Array<{ cid: number; name: string; type: string; notnull: boolean; dflt_value: string | null; pk: boolean }> };
export type DbRowsResponse = { table: string; columns: string[]; rows: any[][]; limit: number; offset: number };
export type DbSqlResponse =
  | { kind: 'rows'; columns: string[]; rows: any[][] }
  | { kind: 'nonQuery'; rowsAffected: number };

@Injectable({ providedIn: 'root' })
export class DbAdminApiService {
  private base = 'http://localhost:5106/api/admin/db';

  constructor(private http: HttpClient) {}

  tables(): Observable<DbTablesResponse> {
    return this.http.get<DbTablesResponse>(`${this.base}/tables`);
  }

  schema(table: string): Observable<DbSchemaResponse> {
    return this.http.get<DbSchemaResponse>(`${this.base}/table/${encodeURIComponent(table)}/schema`);
  }

  rows(table: string, limit = 100, offset = 0): Observable<DbRowsResponse> {
    return this.http.get<DbRowsResponse>(`${this.base}/table/${encodeURIComponent(table)}/rows?limit=${limit}&offset=${offset}`);
  }

  sql(sql: string): Observable<DbSqlResponse> {
    return this.http.post<DbSqlResponse>(`${this.base}/sql`, { sql });
  }
}

