import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { WorkOrderService } from '../../services/work-order.service';
import { WorkOrderResponse } from '../../models/work-order.model';
import { AddWorkOrderModalComponent } from '../add-work-order-modal/add-work-order-modal.component';
import { DeleteWorkOrderModalComponent } from '../delete-work-order-modal/delete-work-order-modal.component';

@Component({
  selector: 'app-work-orders',
  standalone: true,
  imports: [CommonModule, AddWorkOrderModalComponent, DeleteWorkOrderModalComponent],
  templateUrl: './work-orders.component.html',
  styleUrl: './work-orders.component.scss'
})
export class WorkOrdersComponent implements OnInit {
  @ViewChild('addWorkOrderModal') addWorkOrderModal!: AddWorkOrderModalComponent;
  @ViewChild('deleteWorkOrderModal') deleteWorkOrderModal!: DeleteWorkOrderModalComponent;

  workOrders: WorkOrderResponse[] | null = null;
  isLoading = true;

  constructor(
    private workOrderService: WorkOrderService,
    private titleService: Title
  ) {
    this.titleService.setTitle('Work Orders | Ivan');
  }

  ngOnInit(): void {
    this.loadWorkOrders();
  }

  loadWorkOrders(): void {
    this.isLoading = true;
    this.workOrderService.getAll().subscribe({
      next: (workOrders) => {
        this.workOrders = workOrders;
        this.isLoading = false;
      },
      error: () => {
        this.workOrders = [];
        this.isLoading = false;
      }
    });
  }

  openAddModal(): void {
    this.addWorkOrderModal.open();
  }

  openDeleteModal(workOrder: WorkOrderResponse): void {
    this.deleteWorkOrderModal.open(workOrder);
  }

  onWorkOrderCreated(workOrder: WorkOrderResponse): void {
    this.loadWorkOrders();
  }

  onWorkOrderDeleted(workOrderId: string): void {
    this.loadWorkOrders();
  }
}
