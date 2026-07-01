import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DEFAULT_PAGE_SIZE } from '../constants/pagination';
import { DashboardStats, KanbanColumn, Project, Task, TaskStatus } from '../models/project.model';
import { PagedResult } from '../models/auth.model';
import { CreateUserRequest, UpdateUserRequest, UserDetail, UserListItem } from '../models/user.model';
import { CreateRoleRequest, Permission, Role, RoleDetail, UpdateRolePermissionsRequest, UpdateRoleRequest } from '../models/role.model';
import { ReportOverview } from '../models/report.model';
import { Meeting, MeetingDetail, MeetingStatus, SaveMeetingRequest } from '../models/meeting.model';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/projects`;

  getAll(page = 1, pageSize = DEFAULT_PAGE_SIZE, search?: string, status?: number): Observable<PagedResult<Project>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    if (status !== undefined) params = params.set('status', status);
    return this.http.get<PagedResult<Project>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Project> {
    return this.http.get<Project>(`${this.baseUrl}/${id}`);
  }

  create(project: Partial<Project>): Observable<Project> {
    return this.http.post<Project>(this.baseUrl, project);
  }

  update(id: string, project: Partial<Project>): Observable<Project> {
    return this.http.put<Project>(`${this.baseUrl}/${id}`, { ...project, id });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/tasks`;

  getAll(projectId?: string, page = 1, pageSize = DEFAULT_PAGE_SIZE, status?: TaskStatus): Observable<PagedResult<Task>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (projectId) params = params.set('projectId', projectId);
    if (status !== undefined) params = params.set('status', status);
    return this.http.get<PagedResult<Task>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Task> {
    return this.http.get<Task>(`${this.baseUrl}/${id}`);
  }

  getKanban(projectId: string): Observable<KanbanColumn[]> {
    return this.http.get<KanbanColumn[]>(`${this.baseUrl}/kanban/${projectId}`);
  }

  getCalendar(from: string, to: string, projectId?: string): Observable<Task[]> {
    let params = new HttpParams().set('from', from).set('to', to);
    if (projectId) params = params.set('projectId', projectId);
    return this.http.get<Task[]>(`${this.baseUrl}/calendar`, { params });
  }

  create(task: Partial<Task>): Observable<Task> {
    return this.http.post<Task>(this.baseUrl, task);
  }

  update(id: string, task: Partial<Task>): Observable<Task> {
    return this.http.put<Task>(`${this.baseUrl}/${id}`, { ...task, id });
  }

  move(taskId: string, newStatus: TaskStatus, newOrder: number): Observable<Task> {
    return this.http.patch<Task>(`${this.baseUrl}/move`, { taskId, newStatus, newOrder });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class MeetingService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/meetings`;

  getAll(
    page = 1,
    pageSize = DEFAULT_PAGE_SIZE,
    search?: string,
    status?: MeetingStatus,
    from?: string,
    to?: string,
  ): Observable<PagedResult<Meeting>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    if (status !== undefined) params = params.set('status', status);
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return this.http.get<PagedResult<Meeting>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<MeetingDetail> {
    return this.http.get<MeetingDetail>(`${this.baseUrl}/${id}`);
  }

  create(meeting: SaveMeetingRequest): Observable<MeetingDetail> {
    return this.http.post<MeetingDetail>(this.baseUrl, meeting);
  }

  update(id: string, meeting: SaveMeetingRequest & { id: string }): Observable<MeetingDetail> {
    return this.http.put<MeetingDetail>(`${this.baseUrl}/${id}`, meeting);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);

  getStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${environment.apiUrl}/dashboard/stats`);
  }
}

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly http = inject(HttpClient);

  getOverview(): Observable<ReportOverview> {
    return this.http.get<ReportOverview>(`${environment.apiUrl}/reports/overview`);
  }
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/users`;

  getAll(page = 1, pageSize = DEFAULT_PAGE_SIZE, search?: string, isActive?: boolean): Observable<PagedResult<UserListItem>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    if (isActive !== undefined) params = params.set('isActive', isActive);
    return this.http.get<PagedResult<UserListItem>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<UserDetail> {
    return this.http.get<UserDetail>(`${this.baseUrl}/${id}`);
  }

  create(user: CreateUserRequest): Observable<UserDetail> {
    return this.http.post<UserDetail>(this.baseUrl, user);
  }

  update(id: string, user: UpdateUserRequest): Observable<UserDetail> {
    return this.http.put<UserDetail>(`${this.baseUrl}/${id}`, { ...user, id });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class RoleService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/roles`;

  getAll(page = 1, pageSize = DEFAULT_PAGE_SIZE, search?: string): Observable<PagedResult<Role>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<Role>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<RoleDetail> {
    return this.http.get<RoleDetail>(`${this.baseUrl}/${id}`);
  }

  getPermissions(): Observable<Permission[]> {
    return this.http.get<Permission[]>(`${this.baseUrl}/permissions`);
  }

  create(role: CreateRoleRequest): Observable<RoleDetail> {
    return this.http.post<RoleDetail>(this.baseUrl, role);
  }

  update(id: string, role: UpdateRoleRequest): Observable<RoleDetail> {
    return this.http.put<RoleDetail>(`${this.baseUrl}/${id}`, { ...role, id });
  }

  updatePermissions(id: string, request: UpdateRolePermissionsRequest): Observable<RoleDetail> {
    return this.http.put<RoleDetail>(`${this.baseUrl}/${id}/permissions`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/notifications`;

  getAll(page = 1, pageSize = DEFAULT_PAGE_SIZE, unreadOnly?: boolean): Observable<PagedResult<NotificationItem>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (unreadOnly !== undefined) params = params.set('unreadOnly', unreadOnly);
    return this.http.get<PagedResult<NotificationItem>>(this.baseUrl, { params });
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.baseUrl}/unread-count`);
  }

  markAsRead(id: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/read`, null);
  }

  markAllAsRead(): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/read-all`, null);
  }
}

export interface NotificationItem {
  id: string;
  title: string;
  message: string;
  type: number;
  isRead: boolean;
  actionUrl?: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class AuditLogService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/auditlogs`;

  getAll(page = 1, pageSize = DEFAULT_PAGE_SIZE, entityType?: string): Observable<PagedResult<AuditLogItem>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (entityType) params = params.set('entityType', entityType);
    return this.http.get<PagedResult<AuditLogItem>>(this.baseUrl, { params });
  }
}

export interface AuditLogItem {
  id: string;
  userId: string;
  userName: string;
  action: number;
  entityType: string;
  entityId?: string;
  createdAt: string;
}
