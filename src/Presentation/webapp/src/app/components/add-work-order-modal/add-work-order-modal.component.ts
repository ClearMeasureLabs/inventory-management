import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WorkOrderService } from '../../services/work-order.service';
import { CreateWorkOrderRequest, WorkOrderResponse, ValidationProblemDetails } from '../../models/work-order.model';

@Component({
  selector: 'app-add-work-order-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-work-order-modal.component.html',
  styleUrl: './add-work-order-modal.component.scss'
})
export class AddWorkOrderModalComponent {
  @Output() workOrderCreated = new EventEmitter<WorkOrderResponse>();
  @Output() modalClosed = new EventEmitter<void>();

  isVisible = false;
  isSubmitting = false;
  workOrderTitle = '';
  validationErrors: { [key: string]: string[] } = {};
  generalError: string | null = null;

  constructor(private workOrderService: WorkOrderService) {}

  open(): void {
    this.workOrderTitle = '';
    this.validationErrors = {};
    this.generalError = null;
    this.isSubmitting = false;
    this.isVisible = true;
  }

  close(): void {
    this.isVisible = false;
    this.modalClosed.emit();
  }

  submit(): void {
    this.validationErrors = {};
    this.generalError = null;
    this.isSubmitting = true;

    const request: CreateWorkOrderRequest = {
      title: this.workOrderTitle
    };

    this.workOrderService.create(request).subscribe({
      next: (workOrder) => {
        this.isSubmitting = false;
        this.workOrderCreated.emit(workOrder);
        this.close();
      },
      error: (error: ValidationProblemDetails) => {
        this.isSubmitting = false;
        if (error.errors) {
          this.validationErrors = error.errors;
        } else if (error.title) {
          this.generalError = error.title;
        } else {
          this.generalError = 'An unexpected error occurred. Please try again.';
        }
      }
    });
  }

  hasTitleError(): boolean {
    return !!this.validationErrors['Title'] || !!this.validationErrors['title'];
  }

  getTitleErrors(): string[] {
    return this.validationErrors['Title'] || this.validationErrors['title'] || [];
  }
}
