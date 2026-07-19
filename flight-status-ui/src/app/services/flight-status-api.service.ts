import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FlightStatusResult } from '../models/flight-status-result';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class FlightStatusApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  getFlightStatus(flightNumber: string, date: string): Observable<FlightStatusResult> {
    const params = new HttpParams()
      .set('flightNumber', flightNumber)
      .set('date', date);

    return this.http.get<FlightStatusResult>(`${this.baseUrl}/flights/status`, { params });
  }
}
