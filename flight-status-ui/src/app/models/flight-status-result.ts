import { UnifiedFlightStatus } from './unified-flight-status';

export interface FlightStatusResult {
  flightNumber: string;
  date: string;
  status: UnifiedFlightStatus;
  message: string;
  providerUsed: string | null;
  lastUpdatedUtc: string | null;
  scheduledDepartureUtc: string | null;
  scheduledArrivalUtc: string | null;
  actualDepartureUtc: string | null;
  actualArrivalUtc: string | null;
  terminal: string | null;
  gate: string | null;
  delayReason: string | null;
}
