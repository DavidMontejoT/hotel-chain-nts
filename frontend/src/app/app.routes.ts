import { Routes } from '@angular/router';

import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    title: 'Aurora Hoteles',
    loadComponent: () => import('./features/public/landing/landing.component').then((m) => m.LandingComponent),
  },
  {
    path: 'admin/login',
    title: 'Ingreso administrador',
    loadComponent: () => import('./features/admin/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'hotels' },
      {
        path: 'hotels',
        title: 'Administrar hoteles',
        loadComponent: () =>
          import('./features/admin/hotel-list/hotel-list.component').then((m) => m.HotelListComponent),
      },
      {
        path: 'hotels/new',
        title: 'Nuevo hotel',
        loadComponent: () =>
          import('./features/admin/hotel-form/hotel-form.component').then((m) => m.HotelFormComponent),
      },
      {
        path: 'hotels/:id/edit',
        title: 'Editar hotel',
        loadComponent: () =>
          import('./features/admin/hotel-form/hotel-form.component').then((m) => m.HotelFormComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
