import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { WorkOrderService } from './work-order.service';
import { environment } from '../../environments/environment';

describe('WorkOrderService', () => {
  let service: WorkOrderService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        WorkOrderService
      ]
    });
    service = TestBed.inject(WorkOrderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll', () => {
    it('should return work orders from API', () => {
      const mockWorkOrders = [
        { workOrderId: 'abc-123', title: 'Work Order 1' },
        { workOrderId: 'def-456', title: 'Work Order 2' }
      ];

      service.getAll().subscribe(workOrders => {
        expect(workOrders.length).toBe(2);
        expect(workOrders).toEqual(mockWorkOrders);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
      expect(req.request.method).toBe('GET');
      req.flush(mockWorkOrders);
    });

    it('should return empty array when no work orders exist', () => {
      service.getAll().subscribe(workOrders => {
        expect(workOrders).toEqual([]);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
      req.flush([]);
    });

    it('should handle error response', () => {
      service.getAll().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.title).toBeTruthy();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
      req.flush(null, { status: 500, statusText: 'Server Error' });
    });
  });

  describe('create', () => {
    it('should create work order via POST', () => {
      const request = { title: 'New Work Order' };
      const mockResponse = { workOrderId: 'abc-123', title: 'New Work Order' };

      service.create(request).subscribe(workOrder => {
        expect(workOrder).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush(mockResponse);
    });

    it('should handle validation error response', () => {
      const request = { title: '' };
      const validationError = {
        errors: { Title: ['Title is required'] }
      };

      service.create(request).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.errors).toBeTruthy();
          expect(error.errors['Title']).toContain('Title is required');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
      req.flush(validationError, { status: 400, statusText: 'Bad Request' });
    });

    it('should handle server error response', () => {
      const request = { title: 'Test' };

      service.create(request).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.title).toBe('An unexpected error occurred. Please try again.');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders`);
      req.flush(null, { status: 500, statusText: 'Server Error' });
    });
  });

  describe('delete', () => {
    it('should delete work order via DELETE', () => {
      const workOrderId = 'abc-123';
      
      service.delete(workOrderId).subscribe(() => {
        expect(true).toBeTrue();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders/${workOrderId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should call correct endpoint with work order ID', () => {
      const workOrderId = 'xyz-789';

      service.delete(workOrderId).subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders/${workOrderId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should handle validation error response', () => {
      const validationError = {
        errors: { WorkOrderId: ['Work order not found'] }
      };

      service.delete('abc-123').subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.errors).toBeTruthy();
          expect(error.errors['WorkOrderId']).toContain('Work order not found');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders/abc-123`);
      req.flush(validationError, { status: 400, statusText: 'Bad Request' });
    });

    it('should handle server error response', () => {
      service.delete('abc-123').subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.title).toBe('An unexpected error occurred. Please try again.');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/workorders/abc-123`);
      req.flush(null, { status: 500, statusText: 'Server Error' });
    });
  });
});
