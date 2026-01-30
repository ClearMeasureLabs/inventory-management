import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { ContainerService } from '../../services/container.service';
import { ContainerResponse } from '../../models/container.model';
import { AddContainerModalComponent } from '../add-container-modal/add-container-modal.component';
import { DeleteContainerModalComponent } from '../delete-container-modal/delete-container-modal.component';
import { EditContainerModalComponent } from '../edit-container-modal/edit-container-modal.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, AddContainerModalComponent, DeleteContainerModalComponent, EditContainerModalComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  @ViewChild('addContainerModal') addContainerModal!: AddContainerModalComponent;
  @ViewChild('deleteContainerModal') deleteContainerModal!: DeleteContainerModalComponent;
  @ViewChild('editContainerModal') editContainerModal!: EditContainerModalComponent;

  containers: ContainerResponse[] | null = null;
  isLoading = true;
  searchText: string = '';
  sortColumn: 'containerId' | 'name' | null = 'name';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private containerService: ContainerService,
    private titleService: Title
  ) {
    this.titleService.setTitle('Ivan');
  }

  ngOnInit(): void {
    this.loadContainers();
  }

  loadContainers(): void {
    this.isLoading = true;
    this.containerService.getAll().subscribe({
      next: (containers) => {
        this.containers = containers;
        this.isLoading = false;
      },
      error: () => {
        this.containers = [];
        this.isLoading = false;
      }
    });
  }

  openAddModal(): void {
    this.addContainerModal.open();
  }

  openEditModal(container: ContainerResponse): void {
    this.editContainerModal.open(container);
  }

  openDeleteModal(container: ContainerResponse): void {
    this.deleteContainerModal.open(container);
  }

  onContainerCreated(container: ContainerResponse): void {
    this.loadContainers();
  }

  onContainerUpdated(container: ContainerResponse): void {
    this.loadContainers();
  }

  onContainerDeleted(containerId: number): void {
    this.loadContainers();
  }

  get filteredAndSortedContainers(): ContainerResponse[] {
    if (!this.containers) {
      return [];
    }

    let result = [...this.containers];

    // Apply filter
    if (this.searchText) {
      const searchLower = this.searchText.toLowerCase();
      result = result.filter(container =>
        container.name.toLowerCase().includes(searchLower)
      );
    }

    // Apply sort
    if (this.sortColumn) {
      result.sort((a, b) => {
        let aValue: string | number;
        let bValue: string | number;

        if (this.sortColumn === 'containerId') {
          aValue = a.containerId;
          bValue = b.containerId;
        } else {
          aValue = a.name.toLowerCase();
          bValue = b.name.toLowerCase();
        }

        let comparison = 0;
        if (aValue < bValue) {
          comparison = -1;
        } else if (aValue > bValue) {
          comparison = 1;
        }

        return this.sortDirection === 'asc' ? comparison : -comparison;
      });
    }

    return result;
  }

  onSort(column: 'containerId' | 'name'): void {
    if (this.sortColumn === column) {
      // Toggle direction
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      // New column, start with ascending
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
  }

  clearSearch(): void {
    this.searchText = '';
  }
}
