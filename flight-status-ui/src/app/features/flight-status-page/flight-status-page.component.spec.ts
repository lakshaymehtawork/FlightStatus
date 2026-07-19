import { TestBed } from '@angular/core/testing';
import { Subject, throwError } from 'rxjs';
import { FlightStatusPageComponent } from './flight-status-page.component';
import { FlightStatusApiService } from '../../services/flight-status-api.service';
import { FlightStatusResult } from '../../models/flight-status-result';
import { UnifiedFlightStatus } from '../../models/unified-flight-status';

function buildResult(overrides?: Partial<FlightStatusResult>): FlightStatusResult {
  return {
    flightNumber: 'SR100',
    date: '2026-07-19',
    status: UnifiedFlightStatus.OnTime,
    message: 'Flight is on schedule.',
    providerUsed: 'AeroTrack',
    lastUpdatedUtc: '2026-07-19T08:00:00Z',
    scheduledDepartureUtc: '2026-07-19T08:00:00Z',
    scheduledArrivalUtc: '2026-07-19T10:00:00Z',
    actualDepartureUtc: null,
    actualArrivalUtc: null,
    terminal: null,
    gate: null,
    delayReason: null,
    ...overrides
  };
}

describe('FlightStatusPageComponent', () => {
  it('search_ValidRequest_SetsLoadingAndRendersResultOnSuccess', () => {
    const response$ = new Subject<FlightStatusResult>();
    const apiMock = {
      getFlightStatus: vi.fn(() => response$.asObservable())
    };

    TestBed.configureTestingModule({
      imports: [FlightStatusPageComponent],
      providers: [{ provide: FlightStatusApiService, useValue: apiMock }]
    });

    const fixture = TestBed.createComponent(FlightStatusPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.search({ flightNumber: 'SR100', date: '2026-07-19' });
    fixture.detectChanges();

    expect(component.isLoading()).toBe(true);
    expect(apiMock.getFlightStatus).toHaveBeenCalledWith('SR100', '2026-07-19');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Fetching flight status');

    response$.next(buildResult({ status: UnifiedFlightStatus.Delayed, message: 'Delayed by weather' }));
    response$.complete();
    fixture.detectChanges();

    expect(component.isLoading()).toBe(false);
    expect(component.error()).toBeNull();
    expect(component.result()?.message).toBe('Delayed by weather');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Delayed by weather');
  });

  it('search_ApiReturnsNon2xx_ShowsErrorBannerAndStopsLoading', () => {
    const apiMock = {
      getFlightStatus: vi.fn(() => throwError(() => ({
        status: 503,
        statusText: 'Service Unavailable',
        error: { message: 'Downstream unavailable' }
      })))
    };

    TestBed.configureTestingModule({
      imports: [FlightStatusPageComponent],
      providers: [{ provide: FlightStatusApiService, useValue: apiMock }]
    });

    const fixture = TestBed.createComponent(FlightStatusPageComponent);
    const component = fixture.componentInstance;

    component.search({ flightNumber: 'SR999', date: '2026-07-19' });
    fixture.detectChanges();

    expect(component.isLoading()).toBe(false);
    expect(component.result()).toBeNull();
    expect(component.error()).toContain('Downstream unavailable');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Error:');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Downstream unavailable');
  });

  it('search_ApiErrorWithoutMessage_UsesFallbackErrorText', () => {
    const apiMock = {
      getFlightStatus: vi.fn(() => throwError(() => ({
        status: 500,
        statusText: 'Internal Server Error',
        error: {}
      })))
    };

    TestBed.configureTestingModule({
      imports: [FlightStatusPageComponent],
      providers: [{ provide: FlightStatusApiService, useValue: apiMock }]
    });

    const fixture = TestBed.createComponent(FlightStatusPageComponent);
    const component = fixture.componentInstance;

    component.search({ flightNumber: 'SR700', date: '2026-07-19' });

    expect(component.error()).toBe('Request failed: 500 Internal Server Error');
    expect(component.isLoading()).toBe(false);
  });
});
