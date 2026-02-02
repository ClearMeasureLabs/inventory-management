import { TestBed } from '@angular/core/testing';
import { BreadcrumbService } from './breadcrumb.service';

describe('BreadcrumbService', () => {
  let service: BreadcrumbService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BreadcrumbService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should emit empty data initially', (done) => {
    service.getBreadcrumbData().subscribe(data => {
      expect(data).toEqual({});
      done();
    });
  });

  it('should update breadcrumb data', (done) => {
    const testData = { containerName: 'Test Container' };
    let firstCall = true;

    service.getBreadcrumbData().subscribe(data => {
      if (firstCall) {
        firstCall = false;
        return;
      }
      expect(data).toEqual(testData);
      done();
    });

    service.setBreadcrumbData(testData);
  });

  it('should clear breadcrumb data', (done) => {
    let callCount = 0;
    service.getBreadcrumbData().subscribe(data => {
      callCount++;
      // Skip first emission (initial empty {})
      if (callCount === 1) return;
      // Skip second emission (set data)
      if (callCount === 2) return;
      // Third emission should be cleared data
      if (callCount === 3) {
        expect(data).toEqual({});
        done();
      }
    });

    service.setBreadcrumbData({ containerName: 'Test Container' });
    service.clearBreadcrumbData();
  });
});
