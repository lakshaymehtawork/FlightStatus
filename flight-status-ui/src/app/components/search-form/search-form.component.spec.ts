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
      date: '2026-07-19'
    });

    component.submit();

    expect(emitSpy).toHaveBeenCalledWith({
      flightNumber: 'SR100',
      date: '2026-07-19'
    });
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

  it('template_LoadingTrue_DisablesSubmitButton', () => {
    const fixture = TestBed.createComponent(SearchFormComponent);
    fixture.componentRef.setInput('isLoading', true);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button[type="submit"]') as HTMLButtonElement;
    expect(button.disabled).toBe(true);
    expect(button.textContent).toContain('Searching');
  });
});
