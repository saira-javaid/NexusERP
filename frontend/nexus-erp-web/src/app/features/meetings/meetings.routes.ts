import { Routes } from '@angular/router';

export const MEETINGS_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./meeting-list/meeting-list.component').then(m => m.MeetingListComponent) },
  { path: 'new', loadComponent: () => import('./meeting-form/meeting-form.component').then(m => m.MeetingFormComponent) },
  { path: ':id/edit', loadComponent: () => import('./meeting-form/meeting-form.component').then(m => m.MeetingFormComponent) },
  { path: ':id', loadComponent: () => import('./meeting-detail/meeting-detail.component').then(m => m.MeetingDetailComponent) },
];
