import { Routes } from '@angular/router';

import { adminGuard } from '../../core/guards/admin.guard';

export const ADMIN_ROUTES: Routes = [
  {
    path: 'login',
    title: 'Ingreso administrador',
    loadComponent: () => import('./login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    canActivate: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'hotels' },
      {
        path: 'hotels',
        title: 'Administrar hoteles',
        loadComponent: () => import('./hotel-list/hotel-list.component').then((m) => m.HotelListComponent),
      },
      {
        path: 'hotels/new',
        title: 'Nuevo hotel',
        loadComponent: () => import('./hotel-form/hotel-form.component').then((m) => m.HotelFormComponent),
      },
      {
        path: 'hotels/:id/edit',
        title: 'Editar hotel',
        loadComponent: () => import('./hotel-form/hotel-form.component').then((m) => m.HotelFormComponent),
      },
    ],
  },
];
