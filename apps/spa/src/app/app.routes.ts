import { Routes } from '@angular/router';
import { Shell } from './layout/shell/shell';

export const routes: Routes = [
  {
    path: '',
    component: Shell,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        title: 'Dashboard',
        loadComponent: () => import('./pages/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'biblioteca',
        title: 'Biblioteca',
        loadComponent: () => import('./pages/biblioteca/biblioteca').then((m) => m.Biblioteca),
      },
      {
        path: 'buscar',
        title: 'Buscar',
        loadComponent: () => import('./pages/buscar/buscar').then((m) => m.Buscar),
      },
      {
        path: 'calendario',
        title: 'Calendario',
        loadComponent: () => import('./pages/calendario/calendario').then((m) => m.Calendario),
      },
      {
        path: '**',
        title: 'No encontrado',
        loadComponent: () => import('./pages/not-found/not-found').then((m) => m.NotFound),
      },
    ],
  },
];
