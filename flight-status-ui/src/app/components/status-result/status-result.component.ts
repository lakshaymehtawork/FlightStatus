import { Component, input } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { FlightStatusResult } from '../../models/flight-status-result';
import { UnifiedFlightStatus } from '../../models/unified-flight-status';

const STATUS_CSS_CLASS: Record<UnifiedFlightStatus, string> = {
  [UnifiedFlightStatus.OnTime]: 'status--on-time',
  [UnifiedFlightStatus.Delayed]: 'status--delayed',
  [UnifiedFlightStatus.Cancelled]: 'status--cancelled',
  [UnifiedFlightStatus.Diverted]: 'status--diverted',
  [UnifiedFlightStatus.Unknown]: 'status--unknown'
};

@Component({
  selector: 'app-status-result',
  imports: [DatePipe, NgClass],
  templateUrl: './status-result.component.html',
  styleUrl: './status-result.component.scss'
})
export class StatusResultComponent {
  readonly result = input<FlightStatusResult | null>(null);

  statusClass(status: UnifiedFlightStatus): string {
    return STATUS_CSS_CLASS[status] ?? 'status--unknown';
  }
}
