import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { DeleteContainerModalComponent } from './delete-container-modal.component';
import { ContainerService } from '../../services/container.service';
import { environment } from '../../../environments/environment';

describe('DeleteContainerModalComponent', () => {
  let component: DeleteContainerModalComponent;
  let fixture: ComponentFixture<DeleteContainerModalComponent>;
  let httpMock: HttpTestingController;

  const mockContainer = {
    containerId: 1,
    name: 'Test Container',
    description: 'Test Description'
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeleteContainerModalComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ContainerService
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DeleteContainerModalComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
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

  it('should become visible when open is called', () => {
    component.open(mockContainer);
    expect(component.isVisible).toBeTrue();
    expect(component.container).toEqual(mockContainer);
  });

  it('should display container name in modal', fakeAsync(() => {
    component.open(mockContainer);
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Test Container');
  }));

  it('should close modal when close is called', () => {
    component.open(mockContainer);
    expect(component.isVisible).toBeTrue();
    
    component.close();
    expect(component.isVisible).toBeFalse();
    expect(component.container).toBeNull();
  });

  it('should emit modalClosed event when closed', () => {
    spyOn(component.modalClosed, 'emit');
    
    component.open(mockContainer);
    component.close();
    
    expect(component.modalClosed.emit).toHaveBeenCalled();
  });

  it('should call delete API when confirm is clicked', fakeAsync(() => {
    component.open(mockContainer);
    fixture.detectChanges();
    
    component.confirm();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
    tick();
  }));

  it('should emit containerDeleted event on successful delete', fakeAsync(() => {
    spyOn(component.containerDeleted, 'emit');
    
    component.open(mockContainer);
    fixture.detectChanges();
    
    component.confirm();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush(null);
    tick();
    
    expect(component.containerDeleted.emit).toHaveBeenCalledWith(1);
  }));

  it('should close modal on successful delete', fakeAsync(() => {
    component.open(mockContainer);
    fixture.detectChanges();
    
    component.confirm();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush(null);
    tick();
    
    expect(component.isVisible).toBeFalse();
  }));

  it('should display error message on delete failure', fakeAsync(() => {
    component.open(mockContainer);
    fixture.detectChanges();
    
    component.confirm();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush(
      { errors: { ContainerId: ['Cannot delete a container that has items'] } },
      { status: 400, statusText: 'Bad Request' }
    );
    tick();
    fixture.detectChanges();
    
    expect(component.generalError).toBe('Cannot delete a container that has items');
    expect(component.isVisible).toBeTrue();
  }));

  it('should show spinner while deleting', fakeAsync(() => {
    component.open(mockContainer);
    fixture.detectChanges();
    
    component.confirm();
    expect(component.isDeleting).toBeTrue();
    
    const req = httpMock.expectOne(`${environment.apiUrl}/api/containers/1`);
    req.flush(null);
    tick();
    
    expect(component.isDeleting).toBeFalse();
  }));

  it('should reset error state when modal is opened', () => {
    component.generalError = 'Previous error';
    component.open(mockContainer);
    
    expect(component.generalError).toBeNull();
  });

  it('should display cancel and delete buttons', fakeAsync(() => {
    component.open(mockContainer);
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement as HTMLElement;
    const cancelButton = compiled.querySelector('button.btn-outline-success');
    const deleteButton = compiled.querySelector('button.btn-success');
    
    expect(cancelButton?.textContent).toContain('Cancel');
    expect(deleteButton?.textContent).toContain('Delete');
  }));
});
