import { TestBed } from '@angular/core/testing';
import { StatusResultComponent } from './status-result.component';
import { FlightStatusResult } from '../../models/flight-status-result';
import { UnifiedFlightStatus } from '../../models/unified-flight-status';

function makeResult(status: UnifiedFlightStatus, overrides?: Partial<FlightStatusResult>): FlightStatusResult {
  return {
    flightNumber: 'SR100',
    date: '2026-07-19',
    status,
    message: 'Sample message',
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

describe('StatusResultComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatusResultComponent]
    }).compileComponents();
  });

  it('statusClass_EachEnumValue_ReturnsExpectedCssClass', () => {
    const fixture = TestBed.createComponent(StatusResultComponent);
    const component = fixture.componentInstance;

    expect(component.statusClass(UnifiedFlightStatus.OnTime)).toBe('status--on-time');
    expect(component.statusClass(UnifiedFlightStatus.Delayed)).toBe('status--delayed');
    expect(component.statusClass(UnifiedFlightStatus.Cancelled)).toBe('status--cancelled');
    expect(component.statusClass(UnifiedFlightStatus.Diverted)).toBe('status--diverted');
    expect(component.statusClass(UnifiedFlightStatus.Unknown)).toBe('status--unknown');
  });

  it('template_WithOptionalFields_RendersOnlyPresentFields', () => {
    const fixture = TestBed.createComponent(StatusResultComponent);
    fixture.componentRef.setInput('result', makeResult(UnifiedFlightStatus.Delayed, {
      terminal: 'T1',
      gate: 'G7',
      delayReason: 'Weather',
      actualDepartureUtc: '2026-07-19T08:30:00Z'
    }));
    fixture.detectChanges();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Terminal');
    expect(html.textContent).toContain('T1');
    expect(html.textContent).toContain('Gate');
    expect(html.textContent).toContain('G7');
    expect(html.textContent).toContain('Delay Reason');
    expect(html.textContent).toContain('Weather');
    expect(html.textContent).toContain('Actual Departure');
  });

  it('template_WithNullOptionalFields_HidesConditionalRows', () => {
    const fixture = TestBed.createComponent(StatusResultComponent);
    fixture.componentRef.setInput('result', makeResult(UnifiedFlightStatus.OnTime, {
      providerUsed: null,
      terminal: null,
      gate: null,
      delayReason: null,
      actualDepartureUtc: null,
      actualArrivalUtc: null
    }));
    fixture.detectChanges();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).not.toContain('Terminal');
    expect(html.textContent).not.toContain('Gate');
    expect(html.textContent).not.toContain('Delay Reason');
    expect(html.textContent).not.toContain('Actual Departure');
    expect(html.textContent).not.toContain('Actual Arrival');
    expect(html.textContent).not.toContain('Provider');
  });

  it('template_UnknownStatus_AppliesUnknownStatusClass', () => {
    const fixture = TestBed.createComponent(StatusResultComponent);
    fixture.componentRef.setInput('result', makeResult(UnifiedFlightStatus.Unknown));
    fixture.detectChanges();

    const card = fixture.nativeElement.querySelector('.result-card') as HTMLElement;
    const badge = fixture.nativeElement.querySelector('.status-badge') as HTMLElement;

    expect(card.classList.contains('status--unknown')).toBe(true);
    expect(badge.classList.contains('status--unknown')).toBe(true);
  });
});
