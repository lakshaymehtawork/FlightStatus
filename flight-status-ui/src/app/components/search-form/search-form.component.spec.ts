import { TestBed } from '@angular/core/testing';
import { SearchFormComponent } from './search-form.component';

describe('SearchFormComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SearchFormComponent]
    }).compileComponents();
  });

  it('submit_InvalidForm_DoesNotEmitSearchEvent', () => {
    const fixture = TestBed.createComponent(SearchFormComponent);
    const component = fixture.componentInstance;
    const emitSpy = vi.spyOn(component.searchSubmitted, 'emit');

    component.submit();

    expect(emitSpy).not.toHaveBeenCalled();
    expect(component.flightNumberControl.touched).toBe(true);
    expect(component.dateControl.touched).toBe(true);
  });

  it('submit_ValidForm_EmitsSearchEventPayload', () => {
    const fixture = TestBed.createComponent(SearchFormComponent);
    const component = fixture.componentInstance;
    const emitSpy = vi.spyOn(component.searchSubmitted, 'emit');

    component.form.setValue({
      flightNumber: 'SR100',
      date: '19-07-2026'
    });

    component.submit();

    expect(emitSpy).toHaveBeenCalledWith({
      flightNumber: 'SR100',
      date: '19-07-2026'
    });
  });

  it('formatFlightNumberInput_DigitAndLetterOnlyValue_StripsSymbols', () => {
    const fixture = TestBed.createComponent(SearchFormComponent);
    const component = fixture.componentInstance;

    component.formatFlightNumberInput('sr&100');

    expect(component.flightNumberControl.value).toBe('sr100');
  });

  it('formatDateInput_DigitOnlyValue_AddsSeparators', () => {
    const fixture = TestBed.createComponent(SearchFormComponent);
    const component = fixture.componentInstance;

    component.formatDateInput('10012001');

    expect(component.dateControl.value).toBe('10-01-2001');
  });

  it('submit_DateBeforeSupportedRange_DoesNotEmitSearchEvent', () => {
    const fixture = TestBed.createComponent(SearchFormComponent);
    const component = fixture.componentInstance;
    const emitSpy = vi.spyOn(component.searchSubmitted, 'emit');

    component.form.setValue({
      flightNumber: 'SR100',
      date: '20-03-1000'
    });

    component.submit();

    expect(emitSpy).not.toHaveBeenCalled();
    expect(component.dateControl.hasError('minDate')).toBe(true);
  });

  it('template_RequiredFieldsMissing_ShowsInlineValidationMessages', () => {
    const fixture = TestBed.createComponent(SearchFormComponent);
    const component = fixture.componentInstance;

    component.submit();
    fixture.detectChanges();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Flight number is required.');
    expect(html.textContent).toContain('Date is required.');
  });

  it('template_FlightNumberWithSymbols_ShowsInlineValidationMessage', () => {
    const fixture = TestBed.createComponent(SearchFormComponent);
    const component = fixture.componentInstance;

    component.form.setValue({
      flightNumber: 'SR&100',
      date: '19-07-2026'
    });
    component.submit();
    fixture.detectChanges();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Flight number must contain only letters and digits.');
  });

  it('template_DateBeforeSupportedRange_ShowsInlineValidationMessage', () => {
    const fixture = TestBed.createComponent(SearchFormComponent);
    const component = fixture.componentInstance;

    component.form.setValue({
      flightNumber: 'SR100',
      date: '20-03-1000'
    });
    component.submit();
    fixture.detectChanges();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Date must be on or after 01-01-1900.');
  });

  it('template_LoadingTrue_DisablesSubmitButton', () => {
    const fixture = TestBed.createComponent(SearchFormComponent);
    fixture.componentRef.setInput('isLoading', true);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button[type="submit"]') as HTMLButtonElement;
    expect(button.disabled).toBe(true);
    expect(button.textContent).toContain('Searching');
  });
});
