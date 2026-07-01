import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MeetingService, ProjectService, UserService } from '../../../core/services/api.service';
import { MeetingAttendeeRole, MeetingStatus } from '../../../core/models/meeting.model';
import { Project } from '../../../core/models/project.model';
import { UserListItem } from '../../../core/models/user.model';
import { MeetingStatusLabelPipe } from '../../../shared/pipes/app.pipes';

interface AttendeeGroup {
  label: string;
  users: UserListItem[];
}

@Component({
  selector: 'app-meeting-form',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatDatepickerModule, MatNativeDateModule,
    MatSnackBarModule, MeetingStatusLabelPipe,
  ],
  templateUrl: './meeting-form.component.html',
})
export class MeetingFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly meetingService = inject(MeetingService);
  private readonly projectService = inject(ProjectService);
  private readonly userService = inject(UserService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  isEdit = false;
  meetingId?: string;
  projects: Project[] = [];
  attendeeGroups: AttendeeGroup[] = [];
  meetingStatuses = Object.values(MeetingStatus).filter(v => typeof v === 'number') as MeetingStatus[];

  form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    description: [''],
    location: [''],
    status: [MeetingStatus.Scheduled, Validators.required],
    projectId: [null as string | null],
    startDate: [null as Date | null, Validators.required],
    startTime: ['09:00', Validators.required],
    endDate: [null as Date | null, Validators.required],
    endTime: ['10:00', Validators.required],
    attendeeIds: [[] as string[], Validators.required],
  });

  ngOnInit(): void {
    this.projectService.getAll(1, 100).subscribe(result => {
      this.projects = result.items;
    });

    this.userService.getAll(1, 200, undefined, true).subscribe(result => {
      const groupMap = new Map<string, UserListItem[]>();
      const roleOrder = ['Manager', 'ProjectLead', 'Developer', 'Member', 'Client', 'QA', 'Finance', 'SupportAgent', 'DevOps', 'Viewer'];
      for (const user of result.items) {
        const primaryRole = user.roles[0] ?? 'Other';
        if (!groupMap.has(primaryRole)) groupMap.set(primaryRole, []);
        groupMap.get(primaryRole)!.push(user);
      }
      this.attendeeGroups = roleOrder
        .filter(role => groupMap.has(role))
        .map(role => ({ label: role, users: groupMap.get(role)! }));
      const otherRoles = [...groupMap.keys()].filter(r => !roleOrder.includes(r));
      for (const role of otherRoles) {
        this.attendeeGroups.push({ label: role, users: groupMap.get(role)! });
      }
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id && this.route.snapshot.url.some(s => s.path === 'edit')) {
      this.isEdit = true;
      this.meetingId = id;
      this.meetingService.getById(id).subscribe(meeting => {
        const start = new Date(meeting.startAt);
        const end = new Date(meeting.endAt);
        this.form.patchValue({
          title: meeting.title,
          description: meeting.description ?? '',
          location: meeting.location ?? '',
          status: meeting.status,
          projectId: meeting.projectId ?? null,
          startDate: start,
          startTime: this.toTimeInput(start),
          endDate: end,
          endTime: this.toTimeInput(end),
          attendeeIds: meeting.attendees.map(a => a.userId),
        });
      });
    }
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    const value = this.form.getRawValue();
    const startAt = this.combineDateTime(value.startDate!, value.startTime);
    const endAt = this.combineDateTime(value.endDate!, value.endTime);

    if (endAt <= startAt) {
      this.snackBar.open('End time must be after start time', 'Close', { duration: 4000 });
      return;
    }

    const payload = {
      title: value.title,
      description: value.description || undefined,
      location: value.location || undefined,
      status: value.status,
      projectId: value.projectId || undefined,
      startAt: startAt.toISOString(),
      endAt: endAt.toISOString(),
      attendees: value.attendeeIds.map(userId => ({
        userId,
        role: MeetingAttendeeRole.Required,
      })),
    };

    if (this.isEdit && this.meetingId) {
      this.meetingService.update(this.meetingId, { ...payload, id: this.meetingId }).subscribe({
        next: () => this.router.navigate(['/meetings', this.meetingId]),
        error: err => this.snackBar.open(err.error?.message ?? 'Update failed', 'Close', { duration: 5000 }),
      });
    } else {
      this.meetingService.create(payload).subscribe({
        next: created => this.router.navigate(['/meetings', created.id]),
        error: err => this.snackBar.open(err.error?.message ?? 'Create failed', 'Close', { duration: 5000 }),
      });
    }
  }

  private combineDateTime(date: Date, time: string): Date {
    const [hours, minutes] = time.split(':').map(Number);
    const result = new Date(date);
    result.setHours(hours, minutes, 0, 0);
    return result;
  }

  private toTimeInput(date: Date): string {
    return `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;
  }
}
