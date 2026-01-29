import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { ContainerResponse, CreateContainerRequest, UpdateContainerRequest, ValidationProblemDetails } from '../models/container.model';

@Injectable({
  providedIn: 'root'
})
export class ContainerService {
  private readonly apiUrl = `${environment.apiUrl}/api/containers`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ContainerResponse[]> {
    return this.http.get<ContainerResponse[]>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }

  getById(id: number): Observable<ContainerResponse> {
    return this.http.get<ContainerResponse>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  create(request: CreateContainerRequest): Observable<ContainerResponse> {
    return this.http.post<ContainerResponse>(this.apiUrl, request).pipe(
      catchError(this.handleError)
    );
  }

  update(id: number, request: UpdateContainerRequest): Observable<ContainerResponse> {
    return this.http.put<ContainerResponse>(`${this.apiUrl}/${id}`, request).pipe(
      catchError(this.handleError)
    );
  }

  delete(id: number): Observable<void> {
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
