import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { provideRouter } from '@angular/router';

import { BreadcrumbComponent } from './breadcrumb.component';
import { BreadcrumbService } from '../../services/breadcrumb.service';
import { HomeComponent } from '../home/home.component';
import { ContainerDetailsComponent } from '../container-details/container-details.component';

describe('BreadcrumbComponent', () => {
  let component: BreadcrumbComponent;
  let fixture: ComponentFixture<BreadcrumbComponent>;
  let router: Router;
  let location: Location;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BreadcrumbComponent],
      providers: [
        provideRouter([
          { path: '', component: HomeComponent },
          { path: 'containers/:id', component: ContainerDetailsComponent }
        ])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BreadcrumbComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    location = TestBed.inject(Location);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display "Containers" on home route', async () => {
    await router.navigate(['/']);
    fixture.detectChanges();

    expect(component.breadcrumbs.length).toBe(1);
    expect(component.breadcrumbs[0].label).toBe('Containers');
    expect(component.breadcrumbs[0].active).toBeTrue();
  });

  it('should display "Containers > Container [id]" on container details route', async () => {
    await router.navigate(['/containers/1']);
    fixture.detectChanges();

    expect(component.breadcrumbs.length).toBe(2);
    expect(component.breadcrumbs[0].label).toBe('Containers');
    expect(component.breadcrumbs[0].active).toBeFalse();
    expect(component.breadcrumbs[1].label).toContain('Container');
    expect(component.breadcrumbs[1].active).toBeTrue();
  });

  it('should make "Containers" link clickable on details page', async () => {
    await router.navigate(['/containers/1']);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const containersLink = compiled.querySelector('a');
    expect(containersLink).toBeTruthy();
    expect(containersLink?.textContent?.trim()).toBe('Containers');
  });

  it('should mark current page as active with aria-current', async () => {
    await router.navigate(['/']);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const activeItem = compiled.querySelector('.breadcrumb-item.active');
    expect(activeItem).toBeTruthy();
    expect(activeItem?.getAttribute('aria-current')).toBe('page');
  });

  it('should not have link on current page', async () => {
    await router.navigate(['/']);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const links = compiled.querySelectorAll('a');
    expect(links.length).toBe(0);
  });

  it('should have aria-label for accessibility', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const nav = compiled.querySelector('nav');
    expect(nav?.getAttribute('aria-label')).toBe('breadcrumb');
  });

  it('should update breadcrumbs on navigation', async () => {
    // Start at home
    await router.navigate(['/']);
    fixture.detectChanges();
    expect(component.breadcrumbs.length).toBe(1);

    // Navigate to details
    await router.navigate(['/containers/1']);
    fixture.detectChanges();
    expect(component.breadcrumbs.length).toBe(2);

    // Navigate back to home
    await router.navigate(['/']);
    fixture.detectChanges();
    expect(component.breadcrumbs.length).toBe(1);
  });

  it('should display container name from service', async () => {
    const breadcrumbService = TestBed.inject(BreadcrumbService);

    await router.navigate(['/containers/1']);
    fixture.detectChanges();

    // Initially shows fallback "Container 1"
    expect(component.breadcrumbs[1].label).toContain('Container');

    // Set container name via service
    breadcrumbService.setBreadcrumbData({ containerName: 'My Test Container' });
    fixture.detectChanges();

    // Should now show actual container name
    expect(component.breadcrumbs[1].label).toBe('My Test Container');
  });
});
