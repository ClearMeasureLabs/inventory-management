import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { ContainerService } from './container.service';
import { environment } from '../../environments/environment';

describe('ContainerService', () => {
  let service: ContainerService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ContainerService
      ]
    });
    service = TestBed.inject(ContainerService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll', () => {
    it('should return containers from API', () => {
      const mockContainers = [
        { containerId: 1, name: 'Container 1', description: '' },
        { containerId: 2, name: 'Container 2', description: 'Desc' }
      ];

      service.getAll().subscribe(containers => {
        expect(containers.length).toBe(2);
        expect(containers).toEqual(mockContainers);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
      expect(req.request.method).toBe('GET');
      req.flush(mockContainers);
    });

    it('should return empty array when no containers exist', () => {
      service.getAll().subscribe(containers => {
        expect(containers).toEqual([]);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
      req.flush([]);
    });

    it('should handle error response', () => {
      service.getAll().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.title).toBeTruthy();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
      req.flush(null, { status: 500, statusText: 'Server Error' });
    });
  });

  describe('create', () => {
    it('should create container via POST', () => {
      const request = { name: 'New Container', description: '' };
      const mockResponse = { containerId: 1, name: 'New Container', description: '' };

      service.create(request).subscribe(container => {
        expect(container).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush(mockResponse);
    });

    it('should handle validation error response', () => {
      const request = { name: '', description: '' };
      const validationError = {
        errors: { Name: ['Name is required', 'Name cannot be empty'] }
      };

      service.create(request).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.errors).toBeTruthy();
          expect(error.errors['Name']).toContain('Name is required');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
      req.flush(validationError, { status: 400, statusText: 'Bad Request' });
    });

    it('should handle server error response', () => {
      const request = { name: 'Test', description: '' };

      service.create(request).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.title).toBe('An unexpected error occurred. Please try again.');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers`);
      req.flush(null, { status: 500, statusText: 'Server Error' });
    });
  });
});
