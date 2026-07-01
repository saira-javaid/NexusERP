import { Injectable, signal, effect } from '@angular/core';
import { DEFAULT_PAGE_SIZE, PAGE_SIZE_OPTIONS } from '../constants/pagination';

export type DateFormatPreference = 'short' | 'medium' | 'long';
export type ExportFormatPreference = 'excel' | 'pdf';
export type DashboardRefreshPreference = 'off' | '5' | '15';

export interface UserPreferences {
  darkMode: boolean;
  compactDensity: boolean;
  sidebarCollapsed: boolean;
  dateFormat: DateFormatPreference;
  pageSize: number;
  emailNotifications: boolean;
  desktopNotifications: boolean;
  dashboardAutoRefresh: DashboardRefreshPreference;
  defaultExportFormat: ExportFormatPreference;
  showKanbanAvatars: boolean;
  confirmBeforeDelete: boolean;
}

const STORAGE_KEY = 'nexus_preferences';

const DEFAULTS: UserPreferences = {
  darkMode: false,
  compactDensity: false,
  sidebarCollapsed: false,
  dateFormat: 'medium',
  pageSize: DEFAULT_PAGE_SIZE,
  emailNotifications: true,
  desktopNotifications: false,
  dashboardAutoRefresh: 'off',
  defaultExportFormat: 'excel',
  showKanbanAvatars: true,
  confirmBeforeDelete: true,
};

@Injectable({ providedIn: 'root' })
export class UserPreferencesService {
  readonly preferences = signal<UserPreferences>(this.load());

  readonly pageSizeOptions = PAGE_SIZE_OPTIONS;

  constructor() {
    effect(() => this.persist(this.preferences()));
  }

  update(partial: Partial<UserPreferences>): void {
    this.preferences.update(current => ({ ...current, ...partial }));
  }

  reset(): void {
    this.preferences.set({ ...DEFAULTS });
  }

  getPageSize(): number {
    return this.preferences().pageSize;
  }

  private load(): UserPreferences {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return { ...DEFAULTS };
      const parsed = JSON.parse(raw) as Partial<UserPreferences>;
      return { ...DEFAULTS, ...parsed };
    } catch {
      return { ...DEFAULTS };
    }
  }

  private persist(prefs: UserPreferences): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(prefs));
    document.body.classList.toggle('compact-density', prefs.compactDensity);
  }
}
