import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PlansApiService {
  private baseUrl = `${environment.apiUrl}/api/plans`;

  constructor(private http: HttpClient) {}

  getActivePlan() {
    return this.http.get(`${this.baseUrl}/active`);
  }
}
