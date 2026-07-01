import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { DatePipe } from '@angular/common';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { MeetingService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { UserPreferencesService } from '../../../core/services/user-preferences.service';
import { Meeting, MeetingStatus } from '../../../core/models/meeting.model';
import { MeetingStatusLabelPipe } from '../../../shared/pipes/app.pipes';
import { ListPaginationComponent } from '../../../shared/components/list-pagination/list-pagination.component';

@Component({
  selector: 'app-meeting-list',
  standalone: true,
  imports: [
    RouterLink, ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatSelectModule, DatePipe,
    MeetingStatusLabelPipe, ListPaginationComponent,
  ],
  templateUrl: './meeting-list.component.html',
  styleUrl: './meeting-list.component.scss',
})
export class MeetingListComponent implements OnInit {
  private readonly meetingService = inject(MeetingService);
  private readonly authService = inject(AuthService);
  private readonly preferences = inject(UserPreferencesService);

  readonly meetings = signal<Meeting[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(this.preferences.getPageSize());
  readonly canCreate = this.authService.hasPermission('meetings.create');

  searchControl = new FormControl('');
  statusControl = new FormControl<MeetingStatus | null>(null);
  meetingStatuses = Object.values(MeetingStatus).filter(v => typeof v === 'number') as MeetingStatus[];
  cols = ['title', 'startAt', 'location', 'organizer', 'attendees', 'status', 'actions'];

  ngOnInit(): void {
    this.loadMeetings();
    this.searchControl.valueChanges.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => {
      this.pageIndex.set(0);
      this.loadMeetings();
    });
    this.statusControl.valueChanges.subscribe(() => {
      this.pageIndex.set(0);
      this.loadMeetings();
    });
  }

  loadMeetings(): void {
    this.meetingService.getAll(
      this.pageIndex() + 1,
      this.pageSize(),
      this.searchControl.value ?? undefined,
      this.statusControl.value ?? undefined,
    ).subscribe(result => {
      this.meetings.set(result.items);
      this.totalCount.set(result.totalCount);
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    if (event.pageSize !== this.preferences.getPageSize()) {
      this.preferences.update({ pageSize: event.pageSize });
    }
    this.loadMeetings();
  }
}
