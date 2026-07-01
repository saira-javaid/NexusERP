import { Routes } from '@angular/router';

export const PROJECTS_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./project-list/project-list.component').then(m => m.ProjectListComponent) },
  { path: 'new', loadComponent: () => import('./project-form/project-form.component').then(m => m.ProjectFormComponent) },
  { path: ':id/edit', loadComponent: () => import('./project-form/project-form.component').then(m => m.ProjectFormComponent) },
];
