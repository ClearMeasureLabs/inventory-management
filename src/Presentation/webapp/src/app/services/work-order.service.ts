import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { WorkOrderResponse, CreateWorkOrderRequest, ValidationProblemDetails } from '../models/work-order.model';

@Injectable({
  providedIn: 'root'
})
export class WorkOrderService {
  private readonly apiUrl = `${environment.apiUrl}/api/workorders`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<WorkOrderResponse[]> {
    return this.http.get<WorkOrderResponse[]>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }

  create(request: CreateWorkOrderRequest): Observable<WorkOrderResponse> {
    return this.http.post<WorkOrderResponse>(this.apiUrl, request).pipe(
      catchError(this.handleError)
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    if (error.status === 400 && error.error) {
      // Validation error - pass through the error body
      return throwError(() => error.error as ValidationProblemDetails);
    }
    // Generic error
    return throwError(() => ({ title: 'An unexpected error occurred. Please try again.' }));
  }
}
