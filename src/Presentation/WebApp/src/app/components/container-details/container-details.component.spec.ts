import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { Title } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { ActivatedRoute } from '@angular/router';

import { ContainerDetailsComponent } from './container-details.component';
import { ContainerService } from '../../services/container.service';
import { environment } from '../../../environments/environment';
import { of } from 'rxjs';

describe('ContainerDetailsComponent', () => {
  let component: ContainerDetailsComponent;
  let fixture: ComponentFixture<ContainerDetailsComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContainerDetailsComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        ContainerService,
        Title,
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: () => '1'
              }
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ContainerDetailsComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show loading spinner initially', () => {
    expect(component.isLoading).toBeTrue();
    const compiled = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
    expect(compiled.querySelector('.spinner-border')).toBeTruthy();

    // Complete the HTTP request
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description' });
  });

  it('should display container details when loaded', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description' });

    tick();
    fixture.detectChanges();

    expect(component.isLoading).toBeFalse();
    expect(component.container).toBeTruthy();
    expect(component.container?.name).toBe('Test Container');

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Test Container');
    expect(compiled.textContent).toContain('Test Description');
  }));

  it('should display empty state for items table when container has no items', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description', inventoryItems: [] });

    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('No items in this container');
  }));

  it('should display not found message when container does not exist', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush(null, { status: 404, statusText: 'Not Found' });

    tick();
    fixture.detectChanges();

    expect(component.isLoading).toBeFalse();
    expect(component.notFound).toBeTrue();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Container not found');
  }));

  it('should set page title with container name', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'My Container', description: 'Test Description' });

    tick();

    const titleService = TestBed.inject(Title);
    expect(titleService.getTitle()).toBe('My Container - Ivan');
  }));

  it('should not display back link in success state', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description' });

    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const backLink = compiled.querySelector('a[routerLink="/"]');
    expect(backLink).toBeNull();
  }));
});
