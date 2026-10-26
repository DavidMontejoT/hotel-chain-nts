import { Routes } from '@angular/router';

// Each feature area owns its routes and is lazy loaded: the admin bundle is
// only downloaded when the user navigates to /admin.
export const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./features/public/public.routes').then((m) => m.PUBLIC_ROUTES),
  },
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin.routes').then((m) => m.ADMIN_ROUTES),
  },
  { path: '**', redirectTo: '' },
];
