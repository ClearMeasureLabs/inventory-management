import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { DeleteWorkOrderModalComponent } from './delete-work-order-modal.component';
import { WorkOrderService } from '../../services/work-order.service';
import { environment } from '../../../environments/environment';

describe('DeleteWorkOrderModalComponent', () => {
  let component: DeleteWorkOrderModalComponent;
  let fixture: ComponentFixture<DeleteWorkOrderModalComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeleteWorkOrderModalComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        WorkOrderService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DeleteWorkOrderModalComponent);
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

  it('should become visible when open() is called with a work order', () => {
    const workOrder = { workOrderId: 'abc-123', title: 'Test Work Order' };
    component.open(workOrder);
    expect(component.isVisible).toBeTrue();
    expect(component.workOrder).toEqual(workOrder);
  });

  it('should hide when close() is called', () => {
    const workOrder = { workOrderId: 'abc-123', title: 'Test Work Order' };
    component.open(workOrder);
    component.close();
    expect(component.isVisible).toBeFalse();
    expect(component.workOrder).toBeNull();
  });

  it('should emit workOrderDeleted event on successful deletion', fakeAsync(() => {
    const workOrder = { workOrderId: 'abc-123', title: 'Test Work Order' };
    let deletedId: string | undefined;
    
    component.workOrderDeleted.subscribe((id: string) => {
      deletedId = id;
    });
    
    component.open(workOrder);
    component.confirm();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders/abc-123`);
    req.flush(null);
    tick();
    
    expect(deletedId).toBe('abc-123');
    expect(component.isVisible).toBeFalse();
  }));

  it('should display error message on deletion failure', fakeAsync(() => {
    const workOrder = { workOrderId: 'abc-123', title: 'Test Work Order' };
    
    component.open(workOrder);
    component.confirm();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders/abc-123`);
    req.flush(
      { errors: { WorkOrderId: ['Work order not found'] } },
      { status: 400, statusText: 'Bad Request' }
    );
    tick();
    
    expect(component.generalError).toBeTruthy();
    expect(component.isVisible).toBeTrue();
  }));

  it('should show general error on server error', fakeAsync(() => {
    const workOrder = { workOrderId: 'abc-123', title: 'Test Work Order' };
    
    component.open(workOrder);
    component.confirm();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders/abc-123`);
    req.flush(null, { status: 500, statusText: 'Server Error' });
    tick();
    
    expect(component.generalError).toBeTruthy();
    expect(component.isVisible).toBeTrue();
  }));

  it('should disable delete button while deleting', () => {
    const workOrder = { workOrderId: 'abc-123', title: 'Test Work Order' };
    component.open(workOrder);
    component.isDeleting = true;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const deleteButton = compiled.querySelector('button.btn-danger') as HTMLButtonElement;
    expect(deleteButton?.disabled).toBeTrue();
  });

  it('should display work order title in confirmation message', () => {
    const workOrder = { workOrderId: 'abc-123', title: 'My Important Work Order' };
    component.open(workOrder);
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('My Important Work Order');
  });
});
