import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { NavComponent } from './nav.component';

describe('NavComponent', () => {
  let component: NavComponent;
  let fixture: ComponentFixture<NavComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavComponent],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(NavComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display navbar with Clear Measure blue theme', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const navbar = compiled.querySelector('nav.navbar');
    expect(navbar).toBeTruthy();
    expect(navbar?.classList.contains('navbar-dark')).toBeTrue();
    expect(navbar?.classList.contains('bg-clearmeasure-blue')).toBeTrue();
  });

  it('should display branding with correct text', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const brand = compiled.querySelector('.navbar-brand');
    expect(brand).toBeTruthy();
    expect(brand?.textContent).toContain('Ivan');
    expect(brand?.textContent).toContain('Inventory Management');
  });

  it('should have header brand as home link', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const brandLink = compiled.querySelector('.navbar-brand');
    expect(brandLink).toBeTruthy();
    expect(brandLink?.getAttribute('routerLink')).toBe('/');
  });

  it('should have collapsible menu for mobile', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const toggler = compiled.querySelector('.navbar-toggler');
    expect(toggler).toBeTruthy();
    expect(toggler?.getAttribute('data-bs-toggle')).toBe('collapse');
  });
});
