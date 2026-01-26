import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { AddWorkOrderModalComponent } from './add-work-order-modal.component';
import { WorkOrderService } from '../../services/work-order.service';
import { environment } from '../../../environments/environment';

describe('AddWorkOrderModalComponent', () => {
  let component: AddWorkOrderModalComponent;
  let fixture: ComponentFixture<AddWorkOrderModalComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddWorkOrderModalComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        WorkOrderService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AddWorkOrderModalComponent);
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

  it('should not be visible initially', () => {
    expect(component.isVisible).toBeFalse();
  });

  it('should become visible when open() is called', () => {
    component.open();
    expect(component.isVisible).toBeTrue();
  });

  it('should hide when close() is called', () => {
    component.open();
    component.close();
    expect(component.isVisible).toBeFalse();
  });

  it('should reset form when opened', () => {
    component.workOrderTitle = 'Test';
    component.validationErrors = { Title: ['Error'] };
    component.generalError = 'General error';
    
    component.open();
    
    expect(component.workOrderTitle).toBe('');
    expect(component.validationErrors).toEqual({});
    expect(component.generalError).toBeNull();
  });

  it('should emit workOrderCreated event on successful creation', fakeAsync(() => {
    const mockResponse = { workOrderId: 'abc-123', title: 'New Work Order' };
    let emittedWorkOrder: any;
    
    component.workOrderCreated.subscribe((workOrder: any) => {
      emittedWorkOrder = workOrder;
    });
    
    component.open();
    component.workOrderTitle = 'New Work Order';
    component.submit();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req.flush(mockResponse);
    tick();
    
    expect(emittedWorkOrder).toEqual(mockResponse);
    expect(component.isVisible).toBeFalse();
  }));

  it('should display validation errors from server', fakeAsync(() => {
    component.open();
    component.workOrderTitle = '';
    component.submit();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req.flush(
      { errors: { Title: ['Title is required'] } },
      { status: 400, statusText: 'Bad Request' }
    );
    tick();
    
    expect(component.hasTitleError()).toBeTrue();
    expect(component.getTitleErrors()).toContain('Title is required');
    expect(component.isVisible).toBeTrue();
  }));

  it('should show general error on server error', fakeAsync(() => {
    component.open();
    component.workOrderTitle = 'Test';
    component.submit();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req.flush(null, { status: 500, statusText: 'Server Error' });
    tick();
    
    expect(component.generalError).toBeTruthy();
    expect(component.isVisible).toBeTrue();
  }));

  it('should disable submit button while submitting', () => {
    component.open();
    component.isSubmitting = true;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const submitButton = compiled.querySelector('button.btn-primary') as HTMLButtonElement;
    expect(submitButton?.disabled).toBeTrue();
  });
});
