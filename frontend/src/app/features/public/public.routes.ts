import { Routes } from '@angular/router';

export const PUBLIC_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    title: 'Aurora Hoteles',
    loadComponent: () => import('./landing/landing.component').then((m) => m.LandingComponent),
  },
];
