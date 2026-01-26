import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ContainerService } from '../../services/container.service';
import { ContainerResponse, ValidationProblemDetails } from '../../models/container.model';

@Component({
  selector: 'app-delete-container-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './delete-container-modal.component.html',
  styleUrl: './delete-container-modal.component.scss'
})
export class DeleteContainerModalComponent {
  @Output() containerDeleted = new EventEmitter<number>();
  @Output() modalClosed = new EventEmitter<void>();

  isVisible = false;
  isDeleting = false;
  container: ContainerResponse | null = null;
  generalError: string | null = null;

  constructor(private containerService: ContainerService) {}

  open(container: ContainerResponse): void {
    this.container = container;
    this.generalError = null;
    this.isDeleting = false;
    this.isVisible = true;
  }

  close(): void {
    this.isVisible = false;
    this.container = null;
    this.modalClosed.emit();
  }

  confirm(): void {
    if (!this.container) {
      return;
    }

    this.generalError = null;
    this.isDeleting = true;

    this.containerService.delete(this.container.containerId).subscribe({
      next: () => {
        this.isDeleting = false;
        const deletedId = this.container!.containerId;
        this.containerDeleted.emit(deletedId);
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
