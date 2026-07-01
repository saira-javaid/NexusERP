import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { NotificationService } from './api.service';

export interface AppNotification {
  id: string;
  title: string;
  message: string;
  type: number;
  actionUrl?: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private hubConnection?: signalR.HubConnection;

  readonly notifications = signal<AppNotification[]>([]);
  readonly unreadCount = signal(0);

  async start(): Promise<void> {
    const token = this.authService.getAccessToken();
    if (!token) return;

    await this.refreshUnreadCount();

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: AppNotification) => {
      this.notifications.update(list => [notification, ...list]);
      this.unreadCount.update(c => c + 1);
    });

    try {
      await this.hubConnection.start();
    } catch (err) {
      console.error('SignalR connection failed:', err);
    }
  }

  refreshUnreadCount(): Promise<void> {
    return new Promise(resolve => {
      this.notificationService.getUnreadCount().subscribe({
        next: ({ count }) => {
          this.unreadCount.set(count);
          resolve();
        },
        error: () => resolve(),
      });
    });
  }

  decrementUnread(): void {
    this.unreadCount.update(c => Math.max(0, c - 1));
  }

  clearUnread(): void {
    this.unreadCount.set(0);
  }

  async stop(): Promise<void> {
    await this.hubConnection?.stop();
  }
}
