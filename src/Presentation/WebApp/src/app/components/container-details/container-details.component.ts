import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { ContainerService } from '../../services/container.service';
import { ContainerResponse, UpdateContainerRequest, ValidationProblemDetails } from '../../models/container.model';
import { BreadcrumbService } from '../../services/breadcrumb.service';

@Component({
  selector: 'app-container-details',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './container-details.component.html',
  styleUrl: './container-details.component.scss'
})
export class ContainerDetailsComponent implements OnInit, OnDestroy {
  container: ContainerResponse | null = null;
  isLoading = true;
  notFound = false;

  // Edit mode properties
  isEditMode = false;
  editName = '';
  editDescription = '';
  isSaving = false;
  validationErrors: { name?: string[]; description?: string[] } = {};
  generalError = '';

  // Delete modal properties
  isDeleteModalVisible = false;
  isDeleting = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private containerService: ContainerService,
    private titleService: Title,
    private breadcrumbService: BreadcrumbService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadContainer(+id);
    }
  }

  ngOnDestroy(): void {
    this.breadcrumbService.clearBreadcrumbData();
  }

  loadContainer(id: number): void {
    this.isLoading = true;
    this.containerService.getById(id).subscribe({
      next: (container) => {
        this.container = container;
        this.titleService.setTitle(`${container.name} - Ivan`);
        this.breadcrumbService.setBreadcrumbData({ containerName: container.name });
        this.isLoading = false;
      },
      error: (error) => {
        this.notFound = true;
        this.isLoading = false;
        this.titleService.setTitle('Container Not Found - Ivan');
      }
    });
  }

  enterEditMode(): void {
    if (!this.container) return;

    this.isEditMode = true;
    this.editName = this.container.name;
    this.editDescription = this.container.description;
    this.validationErrors = {};
    this.generalError = '';
  }

  cancelEdit(): void {
    this.isEditMode = false;
    this.editName = '';
    this.editDescription = '';
    this.validationErrors = {};
    this.generalError = '';
  }

  saveChanges(): void {
    if (!this.container) return;

    // Clear previous errors
    this.validationErrors = {};
    this.generalError = '';

    // Client-side validation
    if (!this.editName || this.editName.trim() === '') {
      this.validationErrors.name = ['Name is required'];
      return;
    }

    this.isSaving = true;
    const request: UpdateContainerRequest = {
      name: this.editName,
      description: this.editDescription
    };

    this.containerService.update(this.container.containerId, request).subscribe({
      next: (updated) => {
        this.container = updated;
        this.titleService.setTitle(`${updated.name} - Ivan`);
        this.breadcrumbService.setBreadcrumbData({ containerName: updated.name });
        this.isEditMode = false;
        this.isSaving = false;
      },
      error: (error: ValidationProblemDetails) => {
        this.isSaving = false;

        if (error.errors) {
          // Map API validation errors to our format
          if (error.errors['Name']) {
            this.validationErrors.name = error.errors['Name'];
          }
          if (error.errors['Description']) {
            this.validationErrors.description = error.errors['Description'];
          }
        }

        if (error.title) {
          this.generalError = error.title;
        } else {
          this.generalError = 'Failed to update container. Please try again.';
        }
      }
    });
  }

  openDeleteModal(): void {
    this.isDeleteModalVisible = true;
    this.generalError = '';
  }

  closeDeleteModal(): void {
    this.isDeleteModalVisible = false;
    this.generalError = '';
  }

  confirmDelete(): void {
    if (!this.container) return;

    this.isDeleting = true;
    this.generalError = '';

    this.containerService.delete(this.container.containerId).subscribe({
      next: () => {
        // Navigate to containers list on success
        this.router.navigate(['/']);
      },
      error: (error: ValidationProblemDetails) => {
        this.isDeleting = false;

        if (error.title) {
          this.generalError = error.title;
        } else {
          this.generalError = 'Failed to delete container. Please try again.';
        }
      }
    });
  }

  hasNameError(): boolean {
    return !!this.validationErrors.name && this.validationErrors.name.length > 0;
  }

  getNameErrors(): string[] {
    return this.validationErrors.name || [];
  }

  hasDescriptionError(): boolean {
    return !!this.validationErrors.description && this.validationErrors.description.length > 0;
  }

  getDescriptionErrors(): string[] {
    return this.validationErrors.description || [];
  }
}
