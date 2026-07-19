import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { FlightStatusApiService } from './flight-status-api.service';
import { UnifiedFlightStatus } from '../models/unified-flight-status';

describe('FlightStatusApiService', () => {
  let service: FlightStatusApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        FlightStatusApiService
      ]
    });

    service = TestBed.inject(FlightStatusApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getFlightStatus_ValidInput_CallsExpectedEndpointWithQueryParams', () => {
    service.getFlightStatus('SR100', '2026-07-19').subscribe();

    const request = httpMock.expectOne((req) =>
      req.method === 'GET' &&
      req.url.endsWith('/flights/status') &&
      req.params.get('flightNumber') === 'SR100' &&
      req.params.get('date') === '2026-07-19');

    expect(request.request.method).toBe('GET');
    request.flush({
      flightNumber: 'SR100',
      date: '2026-07-19',
      status: UnifiedFlightStatus.OnTime,
      message: 'On time',
      providerUsed: 'AeroTrack',
      lastUpdatedUtc: '2026-07-19T08:00:00Z',
      scheduledDepartureUtc: '2026-07-19T08:00:00Z',
      scheduledArrivalUtc: '2026-07-19T10:00:00Z',
      actualDepartureUtc: null,
      actualArrivalUtc: null,
      terminal: null,
      gate: null,
      delayReason: null
    });
  });

  it('getFlightStatus_ServerError_PropagatesErrorToSubscriber', () => {
    let capturedError: unknown = null;

    service.getFlightStatus('SR999', '2026-07-19').subscribe({
      next: () => {
        throw new Error('Expected error path');
      },
      error: (error) => {
        capturedError = error;
      }
    });

    const request = httpMock.expectOne((req) =>
      req.method === 'GET' && req.url.endsWith('/flights/status'));

    request.flush(
      { message: 'Remote provider unavailable' },
      { status: 503, statusText: 'Service Unavailable' }
    );

    expect(capturedError).toBeTruthy();
  });
});
