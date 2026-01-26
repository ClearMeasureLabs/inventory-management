import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { WorkOrdersComponent } from './components/work-orders/work-orders.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'work-orders', component: WorkOrdersComponent },
  { path: '**', redirectTo: '' }
];
