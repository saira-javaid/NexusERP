import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';
import { LoadingService } from '../../services/loading.service';
import { SignalRService } from '../../services/signalr.service';
import { ChatWidgetComponent } from '../../components/chat-widget/chat-widget.component';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  permission?: string;
}

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    RouterOutlet, RouterLink, RouterLinkActive,
    MatSidenavModule, MatToolbarModule, MatListModule,
    MatIconModule, MatButtonModule, MatBadgeModule, MatProgressBarModule,
    ChatWidgetComponent,
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly themeService = inject(ThemeService);
  private readonly loadingService = inject(LoadingService);
  private readonly signalR = inject(SignalRService);
  private readonly breakpointObserver = inject(BreakpointObserver);

  readonly user = this.authService.user;
  readonly isLoading = this.loadingService.isLoading;
  readonly theme = this.themeService.mode;
  readonly unreadCount = this.signalR.unreadCount;

  readonly isHandset = toSignal(
    this.breakpointObserver.observe(Breakpoints.Handset).pipe(map(r => r.matches)),
    { initialValue: false }
  );

  sidenavOpened = true;

  readonly navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    { label: 'Projects', icon: 'folder', route: '/projects', permission: 'projects.view' },
    { label: 'Tasks', icon: 'task_alt', route: '/tasks', permission: 'tasks.view' },
    { label: 'Calendar', icon: 'calendar_month', route: '/calendar', permission: 'tasks.view' },
    { label: 'Meetings', icon: 'groups', route: '/meetings', permission: 'meetings.view' },
    { label: 'Users', icon: 'people', route: '/users', permission: 'users.view' },
    { label: 'Roles', icon: 'admin_panel_settings', route: '/roles', permission: 'roles.view' },
    { label: 'Reports', icon: 'assessment', route: '/reports', permission: 'reports.view' },
    { label: 'Audit Logs', icon: 'history', route: '/audit-logs', permission: 'audit.view' },
    { label: 'Settings', icon: 'settings', route: '/settings', permission: 'settings.manage' },
  ];

  ngOnInit(): void {
    this.signalR.start();
  }

  visibleNavItems(): NavItem[] {
    return this.navItems.filter(item =>
      !item.permission || this.authService.hasPermission(item.permission)
    );
  }

  toggleTheme(): void {
    this.themeService.toggle();
  }

  logout(): void {
    this.signalR.stop();
    this.authService.logout();
  }
}
