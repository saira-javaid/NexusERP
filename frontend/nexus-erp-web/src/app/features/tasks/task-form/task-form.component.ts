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
import { ProjectService, TaskService, UserService } from '../../../core/services/api.service';
import { Project, TaskPriority, TaskStatus } from '../../../core/models/project.model';
import { UserListItem } from '../../../core/models/user.model';
import { PriorityLabelPipe, StatusLabelPipe } from '../../../shared/pipes/app.pipes';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatDatepickerModule, MatNativeDateModule,
    MatSnackBarModule, StatusLabelPipe, PriorityLabelPipe,
  ],
  templateUrl: './task-form.component.html',
})
export class TaskFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly taskService = inject(TaskService);
  private readonly projectService = inject(ProjectService);
  private readonly userService = inject(UserService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  isEdit = false;
  taskId?: string;
  projects: Project[] = [];
  users: UserListItem[] = [];
  taskStatuses = Object.values(TaskStatus).filter(v => typeof v === 'number') as TaskStatus[];
  taskPriorities = Object.values(TaskPriority).filter(v => typeof v === 'number') as TaskPriority[];

  form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    description: [''],
    status: [TaskStatus.Todo, Validators.required],
    priority: [TaskPriority.Medium, Validators.required],
    order: [0],
    projectId: ['', Validators.required],
    assigneeId: [null as string | null],
    startDate: [null as Date | null],
    dueDate: [null as Date | null],
    estimatedHours: [null as number | null],
    actualHours: [null as number | null],
    tags: [''],
  });

  ngOnInit(): void {
    this.projectService.getAll(1, 100).subscribe(result => {
      this.projects = result.items;
    });
    this.userService.getAll(1, 100, undefined, true).subscribe(result => {
      this.users = result.items;
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.taskId = id;
      this.taskService.getById(id).subscribe(task => {
        this.form.patchValue({
          title: task.title,
          description: task.description ?? '',
          status: task.status,
          priority: task.priority,
          order: task.order,
          projectId: task.projectId,
          assigneeId: task.assigneeId ?? null,
          startDate: task.startDate ? new Date(task.startDate) : null,
          dueDate: task.dueDate ? new Date(task.dueDate) : null,
          estimatedHours: task.estimatedHours ?? null,
          actualHours: task.actualHours ?? null,
          tags: task.tags ?? '',
        });
      });
    }
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    const value = this.form.getRawValue();
    const payload = {
      title: value.title,
      description: value.description || undefined,
      status: value.status,
      priority: value.priority,
      order: value.order,
      projectId: value.projectId,
      assigneeId: value.assigneeId || undefined,
      startDate: value.startDate?.toISOString(),
      dueDate: value.dueDate?.toISOString(),
      estimatedHours: value.estimatedHours ?? undefined,
      actualHours: value.actualHours ?? undefined,
      tags: value.tags || undefined,
    };

    if (this.isEdit && this.taskId) {
      this.taskService.update(this.taskId, { ...payload, id: this.taskId }).subscribe({
        next: () => this.router.navigate(['/tasks']),
        error: err => this.snackBar.open(err.error?.message ?? 'Update failed', 'Close', { duration: 5000 }),
      });
    } else {
      this.taskService.create(payload).subscribe({
        next: () => this.router.navigate(['/tasks']),
        error: err => this.snackBar.open(err.error?.message ?? 'Create failed', 'Close', { duration: 5000 }),
      });
    }
  }
}
