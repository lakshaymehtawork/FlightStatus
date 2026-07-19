import { Component } from '@angular/core';
import { FlightStatusPageComponent } from './features/flight-status-page/flight-status-page.component';

@Component({
  selector: 'app-root',
  imports: [FlightStatusPageComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {}
