import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { NotificationItem, NotificationService } from '../../../core/services/api.service';
import { UserPreferencesService } from '../../../core/services/user-preferences.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { ListPaginationComponent } from '../../../shared/components/list-pagination/list-pagination.component';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [
    DatePipe, ReactiveFormsModule, MatIconModule, MatButtonModule,
    MatFormFieldModule, MatSelectModule, MatSnackBarModule, ListPaginationComponent,
  ],
  templateUrl: './notification-list.component.html',
  styleUrl: './notification-list.component.scss',
})
export class NotificationListComponent implements OnInit {
  private readonly notificationService = inject(NotificationService);
  private readonly preferences = inject(UserPreferencesService);
  private readonly snackBar = inject(MatSnackBar);
  readonly signalR = inject(SignalRService);
  private readonly router = inject(Router);

  readonly notifications = signal<NotificationItem[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(this.preferences.getPageSize());
  readonly unreadOnPage = signal(0);

  filterControl = new FormControl<'all' | 'unread'>('all');

  ngOnInit(): void {
    this.loadNotifications();
    this.filterControl.valueChanges.subscribe(() => {
      this.pageIndex.set(0);
      this.loadNotifications();
    });
  }

  loadNotifications(): void {
    const unreadOnly = this.filterControl.value === 'unread' ? true : undefined;
    this.notificationService.getAll(this.pageIndex() + 1, this.pageSize(), unreadOnly)
      .subscribe(result => {
        this.notifications.set(result.items);
        this.totalCount.set(result.totalCount);
        this.unreadOnPage.set(result.items.filter(n => !n.isRead).length);
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    if (event.pageSize !== this.preferences.getPageSize()) {
      this.preferences.update({ pageSize: event.pageSize });
    }
    this.loadNotifications();
  }

  markAsRead(notification: NotificationItem, navigate = false): void {
    if (notification.isRead) {
      if (navigate && notification.actionUrl) this.router.navigateByUrl(notification.actionUrl);
      return;
    }

    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        this.notifications.update(list =>
          list.map(n => n.id === notification.id ? { ...n, isRead: true } : n));
        this.signalR.decrementUnread();
        if (navigate && notification.actionUrl) this.router.navigateByUrl(notification.actionUrl);
      },
    });
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.update(list => list.map(n => ({ ...n, isRead: true })));
        this.signalR.clearUnread();
        this.snackBar.open('All notifications marked as read', 'Close', { duration: 3000 });
      },
    });
  }
}
