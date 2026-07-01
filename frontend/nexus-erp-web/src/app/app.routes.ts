import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES),
  },
  {
    path: '',
    loadComponent: () => import('./core/layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadChildren: () => import('./features/dashboard/dashboard.routes').then(m => m.DASHBOARD_ROUTES),
      },
      {
        path: 'projects',
        loadChildren: () => import('./features/projects/projects.routes').then(m => m.PROJECTS_ROUTES),
        canActivate: [permissionGuard],
        data: { permission: 'projects.view' },
      },
      {
        path: 'tasks',
        loadChildren: () => import('./features/tasks/tasks.routes').then(m => m.TASKS_ROUTES),
        canActivate: [permissionGuard],
        data: { permission: 'tasks.view' },
      },
      {
        path: 'kanban/:projectId',
        loadComponent: () => import('./features/kanban/kanban-board/kanban-board.component').then(m => m.KanbanBoardComponent),
        canActivate: [permissionGuard],
        data: { permission: 'tasks.view' },
      },
      {
        path: 'calendar',
        loadComponent: () => import('./features/calendar/calendar-view/calendar-view.component').then(m => m.CalendarViewComponent),
        canActivate: [permissionGuard],
        data: { permission: 'tasks.view' },
      },
      {
        path: 'meetings',
        loadChildren: () => import('./features/meetings/meetings.routes').then(m => m.MEETINGS_ROUTES),
        canActivate: [permissionGuard],
        data: { permission: 'meetings.view' },
      },
      {
        path: 'users',
        loadChildren: () => import('./features/users/users.routes').then(m => m.USERS_ROUTES),
        canActivate: [permissionGuard],
        data: { permission: 'users.view' },
      },
      {
        path: 'roles',
        loadChildren: () => import('./features/roles/roles.routes').then(m => m.ROLES_ROUTES),
        canActivate: [permissionGuard],
        data: { permission: 'roles.view' },
      },
      {
        path: 'notifications',
        loadComponent: () => import('./features/notifications/notification-list/notification-list.component').then(m => m.NotificationListComponent),
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports-page/reports-page.component').then(m => m.ReportsPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'reports.view' },
      },
      {
        path: 'audit-logs',
        loadComponent: () => import('./features/audit-logs/audit-log-list/audit-log-list.component').then(m => m.AuditLogListComponent),
        canActivate: [permissionGuard],
        data: { permission: 'audit.view' },
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings-page/settings-page.component').then(m => m.SettingsPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'settings.manage' },
      },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
