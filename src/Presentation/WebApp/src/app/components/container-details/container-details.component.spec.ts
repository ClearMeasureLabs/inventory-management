import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { Title } from '@angular/platform-browser';
import { provideRouter, Router } from '@angular/router';
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

  it('should show edit and delete buttons when container loaded', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description' });

    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const editButton = compiled.querySelector('button[aria-label="Edit container"]');
    const deleteButton = compiled.querySelector('button[aria-label="Delete container"]');

    expect(editButton).toBeTruthy();
    expect(deleteButton).toBeTruthy();
  }));

  it('should enter edit mode when edit button clicked', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description' });

    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const editButton = compiled.querySelector('button[aria-label="Edit container"]') as HTMLButtonElement;
    editButton.click();

    fixture.detectChanges();

    expect(component.isEditMode).toBeTrue();
    expect(component.editName).toBe('Test Container');
    expect(component.editDescription).toBe('Test Description');

    const nameInput = compiled.querySelector('input#containerName') as HTMLInputElement;
    const descriptionTextarea = compiled.querySelector('textarea#containerDescription') as HTMLTextAreaElement;

    expect(nameInput).toBeTruthy();
    expect(descriptionTextarea).toBeTruthy();
  }));

  it('should show save and cancel buttons in edit mode', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description' });

    tick();
    fixture.detectChanges();

    component.enterEditMode();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const saveButton = compiled.querySelector('button[aria-label="Save changes"]');
    const cancelButton = compiled.querySelector('button[aria-label="Cancel editing"]');

    expect(saveButton).toBeTruthy();
    expect(cancelButton).toBeTruthy();

    const editButton = compiled.querySelector('button[aria-label="Edit container"]');
    const deleteButton = compiled.querySelector('button[aria-label="Delete container"]');

    expect(editButton).toBeNull();
    expect(deleteButton).toBeNull();
  }));

  it('should save changes and exit edit mode on save', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description' });

    tick();
    fixture.detectChanges();

    component.enterEditMode();
    component.editName = 'Updated Container';
    component.editDescription = 'Updated Description';

    component.saveChanges();

    const updateReq = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    expect(updateReq.request.method).toBe('PUT');
    expect(updateReq.request.body).toEqual({
      name: 'Updated Container',
      description: 'Updated Description'
    });

    updateReq.flush({ containerId: 1, name: 'Updated Container', description: 'Updated Description' });

    tick();
    fixture.detectChanges();

    expect(component.isEditMode).toBeFalse();
    expect(component.container?.name).toBe('Updated Container');
    expect(component.container?.description).toBe('Updated Description');
  }));

  it('should cancel changes and exit edit mode on cancel', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description' });

    tick();
    fixture.detectChanges();

    component.enterEditMode();
    component.editName = 'Modified Name';
    component.editDescription = 'Modified Description';

    component.cancelEdit();
    fixture.detectChanges();

    expect(component.isEditMode).toBeFalse();
    expect(component.container?.name).toBe('Test Container');
    expect(component.container?.description).toBe('Test Description');
  }));

  it('should show validation error for empty name', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description' });

    tick();
    fixture.detectChanges();

    component.enterEditMode();
    component.editName = '';
    component.saveChanges();

    fixture.detectChanges();

    expect(component.hasNameError()).toBeTrue();
    expect(component.getNameErrors()).toContain('Name is required');
    expect(component.isEditMode).toBeTrue();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Name is required');
  }));

  it('should open delete modal when delete button clicked', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description' });

    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const deleteButton = compiled.querySelector('button[aria-label="Delete container"]') as HTMLButtonElement;
    deleteButton.click();

    fixture.detectChanges();

    expect(component.isDeleteModalVisible).toBeTrue();

    const modal = compiled.querySelector('#deleteContainerModal');
    expect(modal).toBeTruthy();
  }));

  it('should close delete modal when cancel clicked', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description' });

    tick();
    fixture.detectChanges();

    component.openDeleteModal();
    fixture.detectChanges();

    expect(component.isDeleteModalVisible).toBeTrue();

    component.closeDeleteModal();
    fixture.detectChanges();

    expect(component.isDeleteModalVisible).toBeFalse();

    const compiled = fixture.nativeElement as HTMLElement;
    const modal = compiled.querySelector('#deleteContainerModal');
    expect(modal).toBeNull();
  }));

  it('should delete container and navigate to home on confirm', fakeAsync(() => {
    const router = TestBed.inject(Router);
    spyOn(router, 'navigate');

    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush({ containerId: 1, name: 'Test Container', description: 'Test Description' });

    tick();
    fixture.detectChanges();

    component.openDeleteModal();
    component.confirmDelete();

    const deleteReq = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    expect(deleteReq.request.method).toBe('DELETE');

    deleteReq.flush(null);

    tick();

    expect(router.navigate).toHaveBeenCalledWith(['/']);
  }));

  it('should not show edit/delete buttons when container not found', fakeAsync(() => {
    fixture.detectChanges();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush(null, { status: 404, statusText: 'Not Found' });

    tick();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const editButton = compiled.querySelector('button[aria-label="Edit container"]');
    const deleteButton = compiled.querySelector('button[aria-label="Delete container"]');

    expect(editButton).toBeNull();
    expect(deleteButton).toBeNull();
  }));
});
