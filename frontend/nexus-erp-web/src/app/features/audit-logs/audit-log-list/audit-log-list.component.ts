import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { AuditLogItem, AuditLogService } from '../../../core/services/api.service';
import { UserPreferencesService } from '../../../core/services/user-preferences.service';
import { ListPaginationComponent } from '../../../shared/components/list-pagination/list-pagination.component';

@Component({
  selector: 'app-audit-log-list',
  standalone: true,
  imports: [
    DatePipe, ReactiveFormsModule, MatTableModule, MatFormFieldModule,
    MatSelectModule, ListPaginationComponent,
  ],
  templateUrl: './audit-log-list.component.html',
  styleUrl: './audit-log-list.component.scss',
})
export class AuditLogListComponent implements OnInit {
  private readonly auditLogService = inject(AuditLogService);
  private readonly preferences = inject(UserPreferencesService);

  readonly logs = signal<AuditLogItem[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(this.preferences.getPageSize());

  entityControl = new FormControl<string | null>(null);
  entityTypes = ['Project', 'TaskItem', 'ApplicationUser', 'ApplicationRole', 'Auth', 'Report'];

  cols = ['user', 'action', 'entity', 'date'];

  ngOnInit(): void {
    this.loadLogs();
    this.entityControl.valueChanges.subscribe(() => {
      this.pageIndex.set(0);
      this.loadLogs();
    });
  }

  loadLogs(): void {
    this.auditLogService.getAll(
      this.pageIndex() + 1,
      this.pageSize(),
      this.entityControl.value ?? undefined,
    ).subscribe(result => {
      this.logs.set(result.items);
      this.totalCount.set(result.totalCount);
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    if (event.pageSize !== this.preferences.getPageSize()) {
      this.preferences.update({ pageSize: event.pageSize });
    }
    this.loadLogs();
  }
}
