import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MeetingService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { UserPreferencesService } from '../../../core/services/user-preferences.service';
import { MeetingDetail } from '../../../core/models/meeting.model';
import { MeetingStatusLabelPipe } from '../../../shared/pipes/app.pipes';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';

@Component({
  selector: 'app-meeting-detail',
  standalone: true,
  imports: [
    RouterLink, DatePipe, MatCardModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatSnackBarModule, MeetingStatusLabelPipe,
  ],
  templateUrl: './meeting-detail.component.html',
  styleUrl: './meeting-detail.component.scss',
})
export class MeetingDetailComponent implements OnInit {
  private readonly meetingService = inject(MeetingService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly meeting = signal<MeetingDetail | null>(null);
  readonly canEdit = this.authService.hasPermission('meetings.edit');
  readonly canDelete = this.authService.hasPermission('meetings.delete');

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.meetingService.getById(id).subscribe({
      next: m => this.meeting.set(m),
      error: () => this.router.navigate(['/meetings']),
    });
  }

  deleteMeeting(): void {
    const m = this.meeting();
    if (!m) return;
    this.confirmDialog.confirmIfEnabled({
      title: 'Cancel meeting',
      message: `Cancel meeting "${m.title}"? Attendees will no longer see this meeting on their schedule.`,
      confirmText: 'Cancel meeting',
      confirmColor: 'warn',
      icon: 'event_busy',
    }).subscribe(confirmed => {
      if (!confirmed) return;
      this.meetingService.delete(m.id).subscribe({
        next: () => {
          this.snackBar.open('Meeting cancelled', 'Close', { duration: 3000 });
          this.router.navigate(['/meetings']);
        },
        error: err => this.snackBar.open(err.error?.message ?? 'Delete failed', 'Close', { duration: 5000 }),
      });
    });
  }
}
