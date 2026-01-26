import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { Title } from '@angular/platform-browser';

import { WorkOrdersComponent } from './work-orders.component';
import { WorkOrderService } from '../../services/work-order.service';
import { environment } from '../../../environments/environment';

describe('WorkOrdersComponent', () => {
  let component: WorkOrdersComponent;
  let fixture: ComponentFixture<WorkOrdersComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WorkOrdersComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        WorkOrderService,
        Title
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(WorkOrdersComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set page title to Work Orders | Ivan', () => {
    const titleService = TestBed.inject(Title);
    expect(titleService.getTitle()).toBe('Work Orders | Ivan');
  });

  it('should show loading spinner initially', () => {
    expect(component.isLoading).toBeTrue();
    const compiled = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
    expect(compiled.querySelector('.spinner-border')).toBeTruthy();
    
    // Complete the HTTP request
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req.flush([]);
  });

  it('should display empty state when no work orders exist', fakeAsync(() => {
    fixture.detectChanges();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req.flush([]);
    
    tick();
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.bg-light.rounded-3')).toBeTruthy();
    expect(compiled.textContent).toContain('No Work Orders');
    expect(compiled.querySelector('button.btn-primary')).toBeTruthy();
  }));

  it('should display work orders table when work orders exist', fakeAsync(() => {
    const mockWorkOrders = [
      { workOrderId: 'abc-123', title: 'Work Order 1' },
      { workOrderId: 'def-456', title: 'Work Order 2' }
    ];
    
    fixture.detectChanges();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req.flush(mockWorkOrders);
    
    tick();
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('table.table-striped')).toBeTruthy();
    expect(compiled.textContent).toContain('Work Order 1');
    expect(compiled.textContent).toContain('Work Order 2');
  }));

  it('should have Add Work Order button', fakeAsync(() => {
    fixture.detectChanges();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req.flush([]);
    
    tick();
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const addButton = compiled.querySelector('button.btn-primary');
    expect(addButton).toBeTruthy();
    expect(addButton?.textContent).toContain('Add Work Order');
  }));

  it('should reload work orders when a new work order is created', fakeAsync(() => {
    fixture.detectChanges();
    
    // Initial load
    const req1 = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req1.flush([]);
    tick();
    
    // Simulate work order creation
    const newWorkOrder = { workOrderId: 'abc-123', title: 'New Work Order' };
    component.onWorkOrderCreated(newWorkOrder);
    
    // Should trigger reload
    const req2 = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req2.flush([newWorkOrder]);
    tick();
    fixture.detectChanges();
    
    expect(component.workOrders?.length).toBe(1);
  }));

  it('should display delete button for each work order in table', fakeAsync(() => {
    const mockWorkOrders = [
      { workOrderId: 'abc-123', title: 'Work Order 1' },
      { workOrderId: 'def-456', title: 'Work Order 2' }
    ];
    
    fixture.detectChanges();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req.flush(mockWorkOrders);
    
    tick();
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const deleteButtons = compiled.querySelectorAll('button.btn-outline-danger');
    expect(deleteButtons.length).toBe(2);
  }));

  it('should display Actions column header in table', fakeAsync(() => {
    const mockWorkOrders = [
      { workOrderId: 'abc-123', title: 'Work Order 1' }
    ];
    
    fixture.detectChanges();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req.flush(mockWorkOrders);
    
    tick();
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const headers = compiled.querySelectorAll('th');
    const headerTexts = Array.from(headers).map(h => h.textContent);
    expect(headerTexts).toContain('Actions');
  }));

  it('should reload work orders when a work order is deleted', fakeAsync(() => {
    const mockWorkOrders = [
      { workOrderId: 'abc-123', title: 'Work Order 1' },
      { workOrderId: 'def-456', title: 'Work Order 2' }
    ];
    
    fixture.detectChanges();
    
    // Initial load
    const req1 = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req1.flush(mockWorkOrders);
    tick();
    
    // Simulate work order deletion
    component.onWorkOrderDeleted('abc-123');
    
    // Should trigger reload
    const req2 = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req2.flush([{ workOrderId: 'def-456', title: 'Work Order 2' }]);
    tick();
    fixture.detectChanges();
    
    expect(component.workOrders?.length).toBe(1);
  }));

  it('should have delete work order modal component', fakeAsync(() => {
    fixture.detectChanges();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
    req.flush([]);
    tick();
    fixture.detectChanges();
    
    expect(component.deleteWorkOrderModal).toBeTruthy();
  }));
});
