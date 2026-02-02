import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface BreadcrumbData {
  containerName?: string;
}

@Injectable({
  providedIn: 'root'
})
export class BreadcrumbService {
  private breadcrumbData$ = new BehaviorSubject<BreadcrumbData>({});

  setBreadcrumbData(data: BreadcrumbData): void {
    this.breadcrumbData$.next(data);
  }

  getBreadcrumbData(): Observable<BreadcrumbData> {
    return this.breadcrumbData$.asObservable();
  }

  clearBreadcrumbData(): void {
    this.breadcrumbData$.next({});
  }
}
