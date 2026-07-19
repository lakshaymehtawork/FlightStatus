import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App]
    }).compileComponents();
  });

  it('create_AppRoot_RendersFeatureContainer', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    const app = fixture.componentInstance;
    const html = fixture.nativeElement as HTMLElement;

    expect(app).toBeTruthy();
    expect(html.querySelector('app-flight-status-page')).toBeTruthy();
  });
});
