import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';

import { EditContainerModalComponent } from './edit-container-modal.component';
import { ContainerService } from '../../services/container.service';
import { environment } from '../../../environments/environment';
import { ContainerResponse } from '../../models/container.model';

describe('EditContainerModalComponent', () => {
  let component: EditContainerModalComponent;
  let fixture: ComponentFixture<EditContainerModalComponent>;
  let httpMock: HttpTestingController;

  const mockContainer: ContainerResponse = {
    containerId: 1,
    name: 'Test Container',
    description: 'Test Description'
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditContainerModalComponent, FormsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ContainerService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EditContainerModalComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should be hidden by default', () => {
    expect(component.isVisible).toBeFalse();
  });

  it('should show modal when open() is called', () => {
    component.open(mockContainer);
    expect(component.isVisible).toBeTrue();
  });

  it('should hide modal when close() is called', () => {
    component.open(mockContainer);
    component.close();
    expect(component.isVisible).toBeFalse();
  });

  it('should emit modalClosed when closed', () => {
    spyOn(component.modalClosed, 'emit');
    component.open(mockContainer);
    component.close();
    expect(component.modalClosed.emit).toHaveBeenCalled();
  });

  it('should pre-populate form with container data when opened', () => {
    component.open(mockContainer);
    
    expect(component.containerName).toBe(mockContainer.name);
    expect(component.containerDescription).toBe(mockContainer.description);
    expect(component.container).toEqual(mockContainer);
  });

  it('should reset errors when opened', () => {
    component.validationErrors = { Name: ['Error'] };
    component.generalError = 'General error';
    
    component.open(mockContainer);
    
    expect(component.validationErrors).toEqual({});
    expect(component.generalError).toBeNull();
  });

  it('should clear container when closed', () => {
    component.open(mockContainer);
    component.close();
    
    expect(component.container).toBeNull();
  });

  it('should submit update and emit containerUpdated on success', fakeAsync(() => {
    spyOn(component.containerUpdated, 'emit');
    
    component.open(mockContainer);
    component.containerName = 'Updated Container';
    component.containerDescription = 'Updated Description';
    component.submit();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/${mockContainer.containerId}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ name: 'Updated Container', description: 'Updated Description' });
    
    const mockResponse = { containerId: 1, name: 'Updated Container', description: 'Updated Description' };
    req.flush(mockResponse);
    
    tick();
    
    expect(component.containerUpdated.emit).toHaveBeenCalledWith(mockResponse);
    expect(component.isVisible).toBeFalse();
  }));

  it('should display validation errors on 400 response', fakeAsync(() => {
    component.open(mockContainer);
    component.containerName = '';
    component.submit();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/${mockContainer.containerId}`);
    req.flush(
      { errors: { Name: ['Name is required'] } },
      { status: 400, statusText: 'Bad Request' }
    );
    
    tick();
    fixture.detectChanges();
    
    expect(component.hasNameError()).toBeTrue();
    expect(component.getNameErrors()).toContain('Name is required');
    expect(component.isVisible).toBeTrue();
  }));

  it('should display duplicate name validation error', fakeAsync(() => {
    component.open(mockContainer);
    component.containerName = 'Duplicate Name';
    component.submit();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/${mockContainer.containerId}`);
    req.flush(
      { errors: { Name: ['A container with this name already exists'] } },
      { status: 400, statusText: 'Bad Request' }
    );
    
    tick();
    fixture.detectChanges();
    
    expect(component.hasNameError()).toBeTrue();
    expect(component.getNameErrors()).toContain('A container with this name already exists');
    expect(component.isVisible).toBeTrue();
  }));

  it('should show general error on non-validation error', fakeAsync(() => {
    component.open(mockContainer);
    component.containerName = 'Test';
    component.submit();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/${mockContainer.containerId}`);
    req.flush(
      { title: 'Server error' },
      { status: 500, statusText: 'Server Error' }
    );
    
    tick();
    fixture.detectChanges();
    
    expect(component.generalError).toBeTruthy();
    expect(component.isVisible).toBeTrue();
  }));

  it('should disable submit button while submitting', () => {
    component.open(mockContainer);
    fixture.detectChanges();
    
    expect(component.isSubmitting).toBeFalse();
    
    component.containerName = 'Test';
    component.submit();
    
    expect(component.isSubmitting).toBeTrue();
    
    // Complete the request
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/${mockContainer.containerId}`);
    req.flush({ containerId: 1, name: 'Test', description: '' });
  });

  it('should not submit if container is null', () => {
    component.container = null;
    component.submit();
    
    // No HTTP requests should be made
    httpMock.expectNone(`${environment.apiUrl}/api/containers`);
    expect(component.isSubmitting).toBeFalse();
  });

  it('should handle hasNameError for uppercase key', () => {
    component.validationErrors = { Name: ['Error'] };
    expect(component.hasNameError()).toBeTrue();
  });

  it('should handle hasNameError for lowercase key', () => {
    component.validationErrors = { name: ['Error'] };
    expect(component.hasNameError()).toBeTrue();
  });

  it('should handle getNameErrors for uppercase key', () => {
    component.validationErrors = { Name: ['Error 1', 'Error 2'] };
    expect(component.getNameErrors()).toEqual(['Error 1', 'Error 2']);
  });

  it('should handle getNameErrors for lowercase key', () => {
    component.validationErrors = { name: ['Error 1'] };
    expect(component.getNameErrors()).toEqual(['Error 1']);
  });

  it('should return empty array from getNameErrors when no errors', () => {
    component.validationErrors = {};
    expect(component.getNameErrors()).toEqual([]);
  });

  it('should handle hasDescriptionError for uppercase key', () => {
    component.validationErrors = { Description: ['Error'] };
    expect(component.hasDescriptionError()).toBeTrue();
  });

  it('should handle hasDescriptionError for lowercase key', () => {
    component.validationErrors = { description: ['Error'] };
    expect(component.hasDescriptionError()).toBeTrue();
  });

  it('should handle getDescriptionErrors for uppercase key', () => {
    component.validationErrors = { Description: ['Error 1'] };
    expect(component.getDescriptionErrors()).toEqual(['Error 1']);
  });

  it('should return empty array from getDescriptionErrors when no errors', () => {
    component.validationErrors = {};
    expect(component.getDescriptionErrors()).toEqual([]);
  });
});
