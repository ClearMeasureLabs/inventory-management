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

  describe('delete', () => {
    it('should delete container via DELETE', () => {
      service.delete(1).subscribe(() => {
        expect(true).toBeTrue();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should call correct endpoint with container ID', () => {
      const containerId = 42;

      service.delete(containerId).subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/42`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should handle validation error response', () => {
      const validationError = {
        errors: { ContainerId: ['Cannot delete a container that has items'] }
      };

      service.delete(1).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.errors).toBeTruthy();
          expect(error.errors['ContainerId']).toContain('Cannot delete a container that has items');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
      req.flush(validationError, { status: 400, statusText: 'Bad Request' });
    });

    it('should handle not found error response', () => {
      const validationError = {
        errors: { ContainerId: ['Container not found'] }
      };

      service.delete(999).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.errors).toBeTruthy();
          expect(error.errors['ContainerId']).toContain('Container not found');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/999`);
      req.flush(validationError, { status: 400, statusText: 'Bad Request' });
    });

    it('should handle server error response', () => {
      service.delete(1).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.title).toBe('An unexpected error occurred. Please try again.');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
      req.flush(null, { status: 500, statusText: 'Server Error' });
    });
  });

  describe('update', () => {
    it('should update container via PUT', () => {
      const containerId = 1;
      const request = { name: 'Updated Container', description: 'Updated description' };
      const mockResponse = { containerId: 1, name: 'Updated Container', description: 'Updated description' };

      service.update(containerId, request).subscribe(container => {
        expect(container).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(request);
      req.flush(mockResponse);
    });

    it('should call correct endpoint with container ID', () => {
      const containerId = 42;
      const request = { name: 'Test', description: '' };

      service.update(containerId, request).subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/42`);
      expect(req.request.method).toBe('PUT');
      req.flush({ containerId: 42, name: 'Test', description: '' });
    });

    it('should handle validation error response for empty name', () => {
      const request = { name: '', description: '' };
      const validationError = {
        errors: { Name: ['Name is required'] }
      };

      service.update(1, request).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.errors).toBeTruthy();
          expect(error.errors['Name']).toContain('Name is required');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
      req.flush(validationError, { status: 400, statusText: 'Bad Request' });
    });

    it('should handle validation error response for duplicate name', () => {
      const request = { name: 'Duplicate Name', description: '' };
      const validationError = {
        errors: { Name: ['A container with this name already exists'] }
      };

      service.update(1, request).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.errors).toBeTruthy();
          expect(error.errors['Name']).toContain('A container with this name already exists');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
      req.flush(validationError, { status: 400, statusText: 'Bad Request' });
    });

    it('should handle not found error response', () => {
      const request = { name: 'Test', description: '' };
      const validationError = {
        errors: { ContainerId: ['Container not found'] }
      };

      service.update(999, request).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.errors).toBeTruthy();
          expect(error.errors['ContainerId']).toContain('Container not found');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/999`);
      req.flush(validationError, { status: 400, statusText: 'Bad Request' });
    });

    it('should handle server error response', () => {
      const request = { name: 'Test', description: '' };

      service.update(1, request).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.title).toBe('An unexpected error occurred. Please try again.');
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
      req.flush(null, { status: 500, statusText: 'Server Error' });
    });
  });
});
