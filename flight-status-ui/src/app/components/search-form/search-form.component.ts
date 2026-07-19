import { Component, inject, output, input } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

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
  private readonly fb = inject(FormBuilder);

  readonly isLoading = input<boolean>(false);
  readonly searchSubmitted = output<SearchQuery>();

  readonly form = this.fb.nonNullable.group({
    flightNumber: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(10)]],
    date: ['', [Validators.required]]
  });

  get flightNumberControl() {
    return this.form.controls.flightNumber;
  }

  get dateControl() {
    return this.form.controls.date;
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
