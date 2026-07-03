import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TaskService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { UserPreferencesService } from '../../../core/services/user-preferences.service';
import { Task, TaskStatus } from '../../../core/models/project.model';
import { StatusLabelPipe, PriorityLabelPipe } from '../../../shared/pipes/app.pipes';
import { ListPaginationComponent } from '../../../shared/components/list-pagination/list-pagination.component';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [
    RouterLink, ReactiveFormsModule, MatTableModule, MatFormFieldModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatSnackBarModule,
    StatusLabelPipe, PriorityLabelPipe, ListPaginationComponent,
  ],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss',
})
export class TaskListComponent implements OnInit {
  private readonly taskService = inject(TaskService);
  private readonly authService = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly preferences = inject(UserPreferencesService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly tasks = signal<Task[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(this.preferences.getPageSize());
  readonly canCreate = this.authService.hasPermission('tasks.create');
  readonly canEdit = this.authService.hasPermission('tasks.edit');
  readonly canDelete = this.authService.hasPermission('tasks.delete');

  statusControl = new FormControl<TaskStatus | null>(null);
  taskStatuses = Object.values(TaskStatus).filter(v => typeof v === 'number') as TaskStatus[];
  cols = ['title', 'project', 'status', 'priority', 'assignee', 'actions'];

  ngOnInit(): void {
    this.loadTasks();
    this.statusControl.valueChanges.subscribe(() => {
      this.pageIndex.set(0);
      this.loadTasks();
    });
  }

  loadTasks(): void {
    this.taskService.getAll(
      undefined,
      this.pageIndex() + 1,
      this.pageSize(),
      this.statusControl.value ?? undefined,
    ).subscribe(result => {
      this.tasks.set(result.items);
      this.totalCount.set(result.totalCount);
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    if (event.pageSize !== this.preferences.getPageSize()) {
      this.preferences.update({ pageSize: event.pageSize });
    }
    this.loadTasks();
  }

  deleteTask(task: Task): void {
    this.confirmDialog.confirmIfEnabled({
      message: `Delete task "${task.title}"?`,
      confirmText: 'OK',
      cancelText: 'Cancel',
      confirmColor: 'warn',
    }).subscribe(confirmed => {
      if (!confirmed) return;
      this.taskService.delete(task.id).subscribe({
        next: () => {
          this.snackBar.open('Task deleted', 'Close', { duration: 3000 });
          this.loadTasks();
        },
        error: err => this.snackBar.open(err.error?.message ?? 'Failed to delete task', 'Close', { duration: 5000 }),
      });
    });
  }
}
