import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { Title } from '@angular/platform-browser';

import { HomeComponent } from './home.component';
import { ContainerService } from '../../services/container.service';
import { environment } from '../../../environments/environment';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        ContainerService,
        Title
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set page title to Ivan', () => {
    const titleService = TestBed.inject(Title);
    expect(titleService.getTitle()).toBe('Ivan');
  });

  it('should show loading spinner initially', () => {
    expect(component.isLoading).toBeTrue();
    const compiled = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
    expect(compiled.querySelector('.spinner-border')).toBeTruthy();
    
    // Complete the HTTP request
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush([]);
  });

  it('should display empty state when no containers exist', fakeAsync(() => {
    fixture.detectChanges();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush([]);
    
    tick();
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.bg-light.rounded-3')).toBeTruthy();
    expect(compiled.textContent).toContain('No Containers');
    expect(compiled.querySelector('button.btn-primary')).toBeTruthy();
  }));

  it('should display containers table when containers exist', fakeAsync(() => {
    const mockContainers = [
      { containerId: 1, name: 'Container 1', description: '' },
      { containerId: 2, name: 'Container 2', description: '' }
    ];
    
    fixture.detectChanges();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush(mockContainers);
    
    tick();
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('table.table-striped')).toBeTruthy();
    expect(compiled.textContent).toContain('Container 1');
    expect(compiled.textContent).toContain('Container 2');
  }));

  it('should have Add Container button', fakeAsync(() => {
    fixture.detectChanges();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush([]);
    
    tick();
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const addButton = compiled.querySelector('button.btn-primary');
    expect(addButton).toBeTruthy();
    expect(addButton?.textContent).toContain('Add Container');
  }));

  it('should reload containers when a new container is created', fakeAsync(() => {
    fixture.detectChanges();
    
    // Initial load
    const req1 = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req1.flush([]);
    tick();
    
    // Simulate container creation
    const newContainer = { containerId: 1, name: 'New Container', description: '' };
    component.onContainerCreated(newContainer);
    
    // Should trigger reload
    const req2 = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req2.flush([newContainer]);
    tick();
    fixture.detectChanges();
    
    expect(component.containers?.length).toBe(1);
  }));

  it('should display delete button for each container in table', fakeAsync(() => {
    const mockContainers = [
      { containerId: 1, name: 'Container 1', description: '' },
      { containerId: 2, name: 'Container 2', description: '' }
    ];
    
    fixture.detectChanges();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush(mockContainers);
    
    tick();
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const deleteButtons = compiled.querySelectorAll('button.btn-outline-danger');
    expect(deleteButtons.length).toBe(2);
  }));

  it('should display Actions column header in table', fakeAsync(() => {
    const mockContainers = [
      { containerId: 1, name: 'Container 1', description: '' }
    ];
    
    fixture.detectChanges();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush(mockContainers);
    
    tick();
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const headers = compiled.querySelectorAll('th');
    const headerTexts = Array.from(headers).map(h => h.textContent);
    expect(headerTexts).toContain('Actions');
  }));

  it('should reload containers when a container is deleted', fakeAsync(() => {
    const mockContainers = [
      { containerId: 1, name: 'Container 1', description: '' },
      { containerId: 2, name: 'Container 2', description: '' }
    ];
    
    fixture.detectChanges();
    
    // Initial load
    const req1 = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req1.flush(mockContainers);
    tick();
    
    // Simulate container deletion
    component.onContainerDeleted(1);
    
    // Should trigger reload
    const req2 = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req2.flush([{ containerId: 2, name: 'Container 2', description: '' }]);
    tick();
    fixture.detectChanges();
    
    expect(component.containers?.length).toBe(1);
  }));

  it('should have delete container modal component', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush([]);
    tick();
    fixture.detectChanges();

    expect(component.deleteContainerModal).toBeTruthy();
  }));

  it('should render container ID as link to details page', fakeAsync(() => {
    const mockContainers = [
      { containerId: 1, name: 'Container 1', description: 'Test' },
      { containerId: 2, name: 'Container 2', description: 'Test' }
    ];

    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush(mockContainers);

    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const idLinks = compiled.querySelectorAll('td a[href^="/containers/"]');
    expect(idLinks.length).toBe(2);
    expect(idLinks[0].getAttribute('href')).toBe('/containers/1');
    expect(idLinks[1].getAttribute('href')).toBe('/containers/2');
  }));
});
