import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContainerService } from '../../services/container.service';
import { CreateContainerRequest, ContainerResponse, ValidationProblemDetails } from '../../models/container.model';

@Component({
  selector: 'app-add-container-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-container-modal.component.html',
  styleUrl: './add-container-modal.component.scss'
})
export class AddContainerModalComponent {
  @Output() containerCreated = new EventEmitter<ContainerResponse>();
  @Output() modalClosed = new EventEmitter<void>();

  isVisible = false;
  isSubmitting = false;
  containerName = '';
  containerDescription = '';
  validationErrors: { [key: string]: string[] } = {};
  generalError: string | null = null;

  readonly descriptionMaxLength = 250;

  constructor(private containerService: ContainerService) {}

  open(): void {
    this.containerName = '';
    this.containerDescription = '';
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

    const request: CreateContainerRequest = {
      name: this.containerName,
      description: this.containerDescription
    };

    this.containerService.create(request).subscribe({
      next: (container) => {
        this.isSubmitting = false;
        this.containerCreated.emit(container);
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
