import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FlightStatusApiService } from '../../services/flight-status-api.service';
import { FlightStatusResult } from '../../models/flight-status-result';
import { SearchFormComponent, SearchQuery } from '../../components/search-form/search-form.component';
import { StatusResultComponent } from '../../components/status-result/status-result.component';

@Component({
  selector: 'app-flight-status-page',
  imports: [SearchFormComponent, StatusResultComponent],
  templateUrl: './flight-status-page.component.html',
  styleUrl: './flight-status-page.component.scss'
})
export class FlightStatusPageComponent {
  private readonly flightStatusApi = inject(FlightStatusApiService);

  readonly result = signal<FlightStatusResult | null>(null);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);

  search(query: SearchQuery): void {
    this.isLoading.set(true);
    this.error.set(null);
    this.result.set(null);

    this.flightStatusApi.getFlightStatus(query.flightNumber, query.date).subscribe({
      next: (data) => {
        this.result.set(data);
        this.isLoading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        const msg = (err.error as { message?: string })?.message;
        this.error.set(msg ?? `Request failed: ${err.status} ${err.statusText}`);
        this.isLoading.set(false);
      }
    });
  }
}
