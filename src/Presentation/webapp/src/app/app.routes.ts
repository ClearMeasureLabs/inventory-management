import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { ContainerDetailsComponent } from './components/container-details/container-details.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'containers/:id', component: ContainerDetailsComponent },
  { path: '**', redirectTo: '' }
];
