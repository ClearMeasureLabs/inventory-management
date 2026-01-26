import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkOrderService } from '../../services/work-order.service';
import { WorkOrderResponse, ValidationProblemDetails } from '../../models/work-order.model';

@Component({
  selector: 'app-delete-work-order-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './delete-work-order-modal.component.html',
  styleUrl: './delete-work-order-modal.component.scss'
})
export class DeleteWorkOrderModalComponent {
  @Output() workOrderDeleted = new EventEmitter<string>();
  @Output() modalClosed = new EventEmitter<void>();

  isVisible = false;
  isDeleting = false;
  workOrder: WorkOrderResponse | null = null;
  generalError: string | null = null;

  constructor(private workOrderService: WorkOrderService) {}

  open(workOrder: WorkOrderResponse): void {
    this.workOrder = workOrder;
    this.generalError = null;
    this.isDeleting = false;
    this.isVisible = true;
  }

  close(): void {
    this.isVisible = false;
    this.workOrder = null;
    this.modalClosed.emit();
  }

  confirm(): void {
    if (!this.workOrder) {
      return;
    }

    this.generalError = null;
    this.isDeleting = true;

    this.workOrderService.delete(this.workOrder.workOrderId).subscribe({
      next: () => {
        this.isDeleting = false;
        const deletedId = this.workOrder!.workOrderId;
        this.workOrderDeleted.emit(deletedId);
        this.close();
      },
      error: (error: ValidationProblemDetails) => {
        this.isDeleting = false;
        if (error.errors) {
          const errorMessages = Object.values(error.errors).flat();
          this.generalError = errorMessages.join(', ');
        } else if (error.title) {
          this.generalError = error.title;
        } else {
          this.generalError = 'An unexpected error occurred. Please try again.';
        }
      }
    });
  }
}
