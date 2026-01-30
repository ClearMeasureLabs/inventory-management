import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, NavigationEnd, RouterModule } from '@angular/router';
import { filter, Subject, takeUntil } from 'rxjs';
import { BreadcrumbService, BreadcrumbData } from '../../services/breadcrumb.service';

interface BreadcrumbItem {
  label: string;
  url: string;
  active: boolean;
}

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './breadcrumb.component.html',
  styleUrl: './breadcrumb.component.scss'
})
export class BreadcrumbComponent implements OnInit, OnDestroy {
  breadcrumbs: BreadcrumbItem[] = [];
  private destroy$ = new Subject<void>();
  private breadcrumbData: BreadcrumbData = {};

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private breadcrumbService: BreadcrumbService
  ) {}

  ngOnInit(): void {
    // Subscribe to breadcrumb data changes
    this.breadcrumbService.getBreadcrumbData()
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => {
        this.breadcrumbData = data;
        this.buildBreadcrumbs();
      });

    // Subscribe to router events to rebuild breadcrumbs on navigation
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.buildBreadcrumbs();
      });

    // Build breadcrumbs on init
    this.buildBreadcrumbs();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildBreadcrumbs(): void {
    const url = this.router.url;
    this.breadcrumbs = [];

    if (url === '/' || url === '') {
      // Home page - just "Containers" (active/not clickable)
      this.breadcrumbs.push({
        label: 'Containers',
        url: '/',
        active: true
      });
    } else if (url.startsWith('/containers/')) {
      // Container details page - "Containers > [Container Name]"
      this.breadcrumbs.push({
        label: 'Containers',
        url: '/',
        active: false
      });

      // Get container name from route data or use placeholder
      const containerName = this.getContainerName();
      this.breadcrumbs.push({
        label: containerName,
        url: '',
        active: true
      });
    }
  }

  private getContainerName(): string {
    // Use container name from breadcrumb service if available
    if (this.breadcrumbData.containerName) {
      return this.breadcrumbData.containerName;
    }

    // Fallback to extracting ID from URL
    const segments = this.router.url.split('/');
    const containerId = segments[segments.length - 1];
    return `Container ${containerId}`;
  }
}
