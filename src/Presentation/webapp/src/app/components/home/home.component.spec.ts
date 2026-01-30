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

  // Sorting and Filtering Tests
  it('should filter containers by name (case-insensitive)', fakeAsync(() => {
    const mockContainers = [
      { containerId: 1, name: 'Alpha Container', description: '' },
      { containerId: 2, name: 'Beta Container', description: '' },
      { containerId: 3, name: 'Gamma', description: '' }
    ];

    fixture.detectChanges();
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush(mockContainers);
    tick();

    component.searchText = 'container';
    fixture.detectChanges();

    const filtered = component.filteredAndSortedContainers;
    expect(filtered.length).toBe(2);
    expect(filtered[0].name).toBe('Alpha Container');
    expect(filtered[1].name).toBe('Beta Container');
  }));

  it('should sort containers by ID ascending', fakeAsync(() => {
    const mockContainers = [
      { containerId: 3, name: 'Container C', description: '' },
      { containerId: 1, name: 'Container A', description: '' },
      { containerId: 2, name: 'Container B', description: '' }
    ];

    fixture.detectChanges();
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush(mockContainers);
    tick();

    component.onSort('containerId');
    fixture.detectChanges();

    const sorted = component.filteredAndSortedContainers;
    expect(sorted[0].containerId).toBe(1);
    expect(sorted[1].containerId).toBe(2);
    expect(sorted[2].containerId).toBe(3);
    expect(component.sortDirection).toBe('asc');
  }));

  it('should sort containers by ID descending', fakeAsync(() => {
    const mockContainers = [
      { containerId: 1, name: 'Container A', description: '' },
      { containerId: 2, name: 'Container B', description: '' },
      { containerId: 3, name: 'Container C', description: '' }
    ];

    fixture.detectChanges();
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush(mockContainers);
    tick();

    component.onSort('containerId'); // First click: ascending
    component.onSort('containerId'); // Second click: descending
    fixture.detectChanges();

    const sorted = component.filteredAndSortedContainers;
    expect(sorted[0].containerId).toBe(3);
    expect(sorted[1].containerId).toBe(2);
    expect(sorted[2].containerId).toBe(1);
    expect(component.sortDirection).toBe('desc');
  }));

  it('should sort containers by name ascending', fakeAsync(() => {
    const mockContainers = [
      { containerId: 1, name: 'Charlie', description: '' },
      { containerId: 2, name: 'Alpha', description: '' },
      { containerId: 3, name: 'Beta', description: '' }
    ];

    fixture.detectChanges();
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush(mockContainers);
    tick();

    component.onSort('name');
    fixture.detectChanges();

    const sorted = component.filteredAndSortedContainers;
    expect(sorted[0].name).toBe('Alpha');
    expect(sorted[1].name).toBe('Beta');
    expect(sorted[2].name).toBe('Charlie');
    expect(component.sortDirection).toBe('asc');
  }));

  it('should sort containers by name descending', fakeAsync(() => {
    const mockContainers = [
      { containerId: 1, name: 'Alpha', description: '' },
      { containerId: 2, name: 'Beta', description: '' },
      { containerId: 3, name: 'Charlie', description: '' }
    ];

    fixture.detectChanges();
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush(mockContainers);
    tick();

    component.onSort('name'); // First click: ascending
    component.onSort('name'); // Second click: descending
    fixture.detectChanges();

    const sorted = component.filteredAndSortedContainers;
    expect(sorted[0].name).toBe('Charlie');
    expect(sorted[1].name).toBe('Beta');
    expect(sorted[2].name).toBe('Alpha');
    expect(component.sortDirection).toBe('desc');
  }));

  it('should filter and sort together correctly', fakeAsync(() => {
    const mockContainers = [
      { containerId: 5, name: 'Alpha Container', description: '' },
      { containerId: 3, name: 'Beta Container', description: '' },
      { containerId: 7, name: 'Gamma', description: '' }
    ];

    fixture.detectChanges();
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush(mockContainers);
    tick();

    // Filter by "Container"
    component.searchText = 'Container';
    // Sort by ID ascending
    component.onSort('containerId');
    fixture.detectChanges();

    const result = component.filteredAndSortedContainers;
    expect(result.length).toBe(2);
    expect(result[0].containerId).toBe(3);
    expect(result[0].name).toBe('Beta Container');
    expect(result[1].containerId).toBe(5);
    expect(result[1].name).toBe('Alpha Container');
  }));

  it('should clear search when clearSearch is called', fakeAsync(() => {
    const mockContainers = [
      { containerId: 1, name: 'Alpha', description: '' },
      { containerId: 2, name: 'Beta', description: '' }
    ];

    fixture.detectChanges();
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush(mockContainers);
    tick();

    component.searchText = 'Alpha';
    expect(component.filteredAndSortedContainers.length).toBe(1);

    component.clearSearch();
    expect(component.searchText).toBe('');
    expect(component.filteredAndSortedContainers.length).toBe(2);
  }));

  it('should show no results message when filter matches nothing', fakeAsync(() => {
    const mockContainers = [
      { containerId: 1, name: 'Alpha', description: '' },
      { containerId: 2, name: 'Beta', description: '' }
    ];

    fixture.detectChanges();
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush(mockContainers);
    tick();

    component.searchText = 'Nonexistent';
    fixture.detectChanges();

    expect(component.filteredAndSortedContainers.length).toBe(0);

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('No containers found matching');
  }));
});
