import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ContainerService } from '../../services/container.service';
import { ContainerResponse } from '../../models/container.model';

@Component({
  selector: 'app-container-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './container-details.component.html',
  styleUrl: './container-details.component.scss'
})
export class ContainerDetailsComponent implements OnInit {
  container: ContainerResponse | null = null;
  isLoading = true;
  notFound = false;

  constructor(
    private route: ActivatedRoute,
    private containerService: ContainerService,
    private titleService: Title
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadContainer(+id);
    }
  }

  loadContainer(id: number): void {
    this.isLoading = true;
    this.containerService.getById(id).subscribe({
      next: (container) => {
        this.container = container;
        this.titleService.setTitle(`${container.name} - Ivan`);
        this.isLoading = false;
      },
      error: (error) => {
        this.notFound = true;
        this.isLoading = false;
        this.titleService.setTitle('Container Not Found - Ivan');
      }
    });
  }
}
