import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ContainerService } from '../../services/container.service';
import { ContainerResponse, ValidationProblemDetails } from '../../models/container.model';
import { BreadcrumbService } from '../../services/breadcrumb.service';
import { EditContainerModalComponent } from '../edit-container-modal/edit-container-modal.component';

@Component({
  selector: 'app-container-details',
  standalone: true,
  imports: [CommonModule, RouterModule, EditContainerModalComponent],
  templateUrl: './container-details.component.html',
  styleUrl: './container-details.component.scss'
})
export class ContainerDetailsComponent implements OnInit, OnDestroy {
  @ViewChild('editContainerModal') editContainerModal!: EditContainerModalComponent;

  container: ContainerResponse | null = null;
  isLoading = true;
  notFound = false;

  // Delete modal properties
  isDeleteModalVisible = false;
  isDeleting = false;
  generalError = '';

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

  openEditModal(): void {
    if (!this.container) return;
    this.editContainerModal.open(this.container);
  }

  onContainerUpdated(updated: ContainerResponse): void {
    this.container = updated;
    this.titleService.setTitle(`${updated.name} - Ivan`);
    this.breadcrumbService.setBreadcrumbData({ containerName: updated.name });
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
}
