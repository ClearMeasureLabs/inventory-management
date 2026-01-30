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

    service.getBreadcrumbData().subscribe(data => {
      if (data.containerName) {
        expect(data).toEqual(testData);
        done();
      }
    });

    service.setBreadcrumbData(testData);
  });

  it('should clear breadcrumb data', (done) => {
    service.setBreadcrumbData({ containerName: 'Test Container' });

    let callCount = 0;
    service.getBreadcrumbData().subscribe(data => {
      callCount++;
      if (callCount === 3) {
        expect(data).toEqual({});
        done();
      }
    });

    service.clearBreadcrumbData();
  });
});
