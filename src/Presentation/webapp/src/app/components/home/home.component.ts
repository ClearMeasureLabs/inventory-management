import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { ContainerService } from '../../services/container.service';
import { ContainerResponse } from '../../models/container.model';
import { AddContainerModalComponent } from '../add-container-modal/add-container-modal.component';
import { DeleteContainerModalComponent } from '../delete-container-modal/delete-container-modal.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, AddContainerModalComponent, DeleteContainerModalComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  @ViewChild('addContainerModal') addContainerModal!: AddContainerModalComponent;
  @ViewChild('deleteContainerModal') deleteContainerModal!: DeleteContainerModalComponent;

  containers: ContainerResponse[] | null = null;
  isLoading = true;

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

  openDeleteModal(container: ContainerResponse): void {
    this.deleteContainerModal.open(container);
  }

  onContainerCreated(container: ContainerResponse): void {
    this.loadContainers();
  }

  onContainerDeleted(containerId: number): void {
    this.loadContainers();
  }
}
