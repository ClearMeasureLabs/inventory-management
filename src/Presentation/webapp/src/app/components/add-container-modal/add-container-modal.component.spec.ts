import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';

import { AddContainerModalComponent } from './add-container-modal.component';
import { ContainerService } from '../../services/container.service';
import { environment } from '../../../environments/environment';

describe('AddContainerModalComponent', () => {
  let component: AddContainerModalComponent;
  let fixture: ComponentFixture<AddContainerModalComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddContainerModalComponent, FormsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ContainerService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AddContainerModalComponent);
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
    component.open();
    expect(component.isVisible).toBeTrue();
  });

  it('should hide modal when close() is called', () => {
    component.open();
    component.close();
    expect(component.isVisible).toBeFalse();
  });

  it('should emit modalClosed when closed', () => {
    spyOn(component.modalClosed, 'emit');
    component.open();
    component.close();
    expect(component.modalClosed.emit).toHaveBeenCalled();
  });

  it('should reset form when opened', () => {
    component.containerName = 'Test';
    component.validationErrors = { Name: ['Error'] };
    component.generalError = 'General error';
    
    component.open();
    
    expect(component.containerName).toBe('');
    expect(component.validationErrors).toEqual({});
    expect(component.generalError).toBeNull();
  });

  it('should submit container and emit containerCreated on success', fakeAsync(() => {
    spyOn(component.containerCreated, 'emit');
    
    component.open();
    component.containerName = 'New Container';
    component.submit();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ name: 'New Container', description: '' });
    
    const mockResponse = { containerId: 1, name: 'New Container', description: '' };
    req.flush(mockResponse);
    
    tick();
    
    expect(component.containerCreated.emit).toHaveBeenCalledWith(mockResponse);
    expect(component.isVisible).toBeFalse();
  }));

  it('should display validation errors on 400 response', fakeAsync(() => {
    component.open();
    component.containerName = '';
    component.submit();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
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

  it('should show general error on non-validation error', fakeAsync(() => {
    component.open();
    component.containerName = 'Test';
    component.submit();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
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
    component.open();
    fixture.detectChanges();
    
    expect(component.isSubmitting).toBeFalse();
    
    component.containerName = 'Test';
    component.submit();
    
    expect(component.isSubmitting).toBeTrue();
    
    // Complete the request
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
    req.flush({ containerId: 1, name: 'Test', description: '' });
  });
});
