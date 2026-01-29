import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, NavigationEnd, RouterModule } from '@angular/router';
import { filter, Subject, takeUntil } from 'rxjs';

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

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Build breadcrumbs on init
    this.buildBreadcrumbs();

    // Subscribe to router events to rebuild breadcrumbs on navigation
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.buildBreadcrumbs();
      });
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
    // Try to get container name from route data
    let route = this.activatedRoute.firstChild;
    while (route) {
      if (route.snapshot.data['containerName']) {
        return route.snapshot.data['containerName'];
      }
      route = route.firstChild;
    }

    // Fallback to extracting ID from URL
    const segments = this.router.url.split('/');
    const containerId = segments[segments.length - 1];
    return `Container ${containerId}`;
  }
}
