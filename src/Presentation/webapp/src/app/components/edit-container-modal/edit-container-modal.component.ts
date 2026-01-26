import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContainerService } from '../../services/container.service';
import { UpdateContainerRequest, ContainerResponse, ValidationProblemDetails } from '../../models/container.model';

@Component({
  selector: 'app-edit-container-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-container-modal.component.html',
  styleUrl: './edit-container-modal.component.scss'
})
export class EditContainerModalComponent {
  @Output() containerUpdated = new EventEmitter<ContainerResponse>();
  @Output() modalClosed = new EventEmitter<void>();

  isVisible = false;
  isSubmitting = false;
  container: ContainerResponse | null = null;
  containerName = '';
  containerDescription = '';
  validationErrors: { [key: string]: string[] } = {};
  generalError: string | null = null;

  constructor(private containerService: ContainerService) {}

  open(container: ContainerResponse): void {
    this.container = container;
    this.containerName = container.name;
    this.containerDescription = container.description;
    this.validationErrors = {};
    this.generalError = null;
    this.isSubmitting = false;
    this.isVisible = true;
  }

  close(): void {
    this.isVisible = false;
    this.container = null;
    this.modalClosed.emit();
  }

  submit(): void {
    if (!this.container) {
      return;
    }

    this.validationErrors = {};
    this.generalError = null;
    this.isSubmitting = true;

    const request: UpdateContainerRequest = {
      name: this.containerName,
      description: this.containerDescription
    };

    this.containerService.update(this.container.containerId, request).subscribe({
      next: (updatedContainer) => {
        this.isSubmitting = false;
        this.containerUpdated.emit(updatedContainer);
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

  hasNameError(): boolean {
    return !!this.validationErrors['Name'] || !!this.validationErrors['name'];
  }

  getNameErrors(): string[] {
    return this.validationErrors['Name'] || this.validationErrors['name'] || [];
  }

  hasDescriptionError(): boolean {
    return !!this.validationErrors['Description'] || !!this.validationErrors['description'];
  }

  getDescriptionErrors(): string[] {
    return this.validationErrors['Description'] || this.validationErrors['description'] || [];
  }
}
