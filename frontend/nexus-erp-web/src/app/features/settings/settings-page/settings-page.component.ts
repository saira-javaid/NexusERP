import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ThemeService } from '../../../core/services/theme.service';
import {
  DashboardRefreshPreference,
  DateFormatPreference,
  ExportFormatPreference,
  UserPreferencesService,
} from '../../../core/services/user-preferences.service';
import { PAGE_SIZE_OPTIONS } from '../../../core/constants/pagination';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatCardModule, MatSlideToggleModule, MatFormFieldModule,
    MatSelectModule, MatButtonModule, MatDividerModule, MatSnackBarModule,
  ],
  templateUrl: './settings-page.component.html',
  styleUrl: './settings-page.component.scss',
})
export class SettingsPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly themeService = inject(ThemeService);
  private readonly preferences = inject(UserPreferencesService);
  private readonly snackBar = inject(MatSnackBar);

  readonly pageSizeOptions = PAGE_SIZE_OPTIONS;
  readonly dateFormats: { value: DateFormatPreference; label: string }[] = [
    { value: 'short', label: 'Short (01/07/26)' },
    { value: 'medium', label: 'Medium (Jan 7, 2026)' },
    { value: 'long', label: 'Long (January 7, 2026)' },
  ];
  readonly refreshOptions: { value: DashboardRefreshPreference; label: string }[] = [
    { value: 'off', label: 'Off' },
    { value: '5', label: 'Every 5 minutes' },
    { value: '15', label: 'Every 15 minutes' },
  ];
  readonly exportFormats: { value: ExportFormatPreference; label: string }[] = [
    { value: 'excel', label: 'Excel (.xlsx)' },
    { value: 'pdf', label: 'PDF' },
  ];

  form = this.fb.nonNullable.group({
    darkMode: false,
    compactDensity: false,
    sidebarCollapsed: false,
    dateFormat: 'medium' as DateFormatPreference,
    pageSize: 12,
    emailNotifications: true,
    desktopNotifications: false,
    dashboardAutoRefresh: 'off' as DashboardRefreshPreference,
    defaultExportFormat: 'excel' as ExportFormatPreference,
    showKanbanAvatars: true,
    confirmBeforeDelete: true,
  });

  ngOnInit(): void {
    const prefs = this.preferences.preferences();
    this.form.patchValue({
      ...prefs,
      darkMode: this.themeService.mode() === 'dark',
    });
  }

  save(): void {
    const value = this.form.getRawValue();
    this.themeService.setTheme(value.darkMode ? 'dark' : 'light');
    this.preferences.update({
      darkMode: value.darkMode,
      compactDensity: value.compactDensity,
      sidebarCollapsed: value.sidebarCollapsed,
      dateFormat: value.dateFormat,
      pageSize: value.pageSize,
      emailNotifications: value.emailNotifications,
      desktopNotifications: value.desktopNotifications,
      dashboardAutoRefresh: value.dashboardAutoRefresh,
      defaultExportFormat: value.defaultExportFormat,
      showKanbanAvatars: value.showKanbanAvatars,
      confirmBeforeDelete: value.confirmBeforeDelete,
    });

    if (value.desktopNotifications && 'Notification' in window && Notification.permission === 'default') {
      Notification.requestPermission();
    }

    this.snackBar.open('Settings saved', 'Close', { duration: 3000 });
  }

  reset(): void {
    this.preferences.reset();
    const prefs = this.preferences.preferences();
    this.form.patchValue({ ...prefs, darkMode: prefs.darkMode });
    this.themeService.setTheme(prefs.darkMode ? 'dark' : 'light');
    this.snackBar.open('Settings reset to defaults', 'Close', { duration: 3000 });
  }
}
