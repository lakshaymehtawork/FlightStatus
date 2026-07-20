import { Component, inject, output, input } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';

export interface SearchQuery {
  flightNumber: string;
  date: string;
}

@Component({
  selector: 'app-search-form',
  imports: [ReactiveFormsModule],
  templateUrl: './search-form.component.html',
  styleUrl: './search-form.component.scss'
})
export class SearchFormComponent {
  readonly minimumDate = '01-01-1900';
  private readonly fb = inject(FormBuilder);

  readonly isLoading = input<boolean>(false);
  readonly searchSubmitted = output<SearchQuery>();

  readonly form = this.fb.nonNullable.group({
    flightNumber: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(10), Validators.pattern(/^[a-zA-Z0-9]+$/)]],
    date: ['', [Validators.required, SearchFormComponent.minimumDateValidator]]
  });

  get flightNumberControl() {
    return this.form.controls.flightNumber;
  }

  get dateControl() {
    return this.form.controls.date;
  }

  formatFlightNumberInput(rawValue: string): void {
    const formattedValue = rawValue.replace(/[^a-zA-Z0-9]/g, '').slice(0, 10);

    if (formattedValue !== this.flightNumberControl.value) {
      this.flightNumberControl.setValue(formattedValue);
    }
  }

  formatDateInput(rawValue: string): void {
    const digits = rawValue.replace(/\D/g, '').slice(0, 8);
    const parts = [digits.slice(0, 2), digits.slice(2, 4), digits.slice(4, 8)].filter(Boolean);
    const formattedValue = parts.join('-');

    if (formattedValue !== this.dateControl.value) {
      this.dateControl.setValue(formattedValue);
    }
  }

  static minimumDateValidator(control: AbstractControl<string>): ValidationErrors | null {
    const value = control.value;

    if (!value) {
      return null;
    }

    if (!/^\d{2}-\d{2}-\d{4}$/.test(value)) {
      return { dateFormat: true };
    }

    const [dayText, monthText, yearText] = value.split('-');
    const day = Number(dayText);
    const month = Number(monthText);
    const year = Number(yearText);

    const enteredDate = new Date(Date.UTC(year, month - 1, day));
    const isValidCalendarDate = enteredDate.getUTCFullYear() === year
      && enteredDate.getUTCMonth() === month - 1
      && enteredDate.getUTCDate() === day;

    if (!isValidCalendarDate) {
      return { dateFormat: true };
    }

    const minimumDate = new Date(Date.UTC(1900, 0, 1));
    return enteredDate < minimumDate ? { minDate: true } : null;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.searchSubmitted.emit({
      flightNumber: this.form.controls.flightNumber.value,
      date: this.form.controls.date.value
    });
  }
}
