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
import { ProjectService } from '../../../core/services/api.service';
import { ProjectStatus } from '../../../core/models/project.model';
import { StatusLabelPipe } from '../../../shared/pipes/app.pipes';

@Component({
  selector: 'app-project-form',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatDatepickerModule, MatNativeDateModule,
    MatSnackBarModule, StatusLabelPipe,
  ],
  templateUrl: './project-form.component.html',
})
export class ProjectFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly projectService = inject(ProjectService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  isEdit = false;
  projectId?: string;
  projectStatuses = Object.values(ProjectStatus).filter(v => typeof v === 'number') as ProjectStatus[];

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: [''],
    code: ['', Validators.required],
    status: [ProjectStatus.Planning, Validators.required],
    startDate: [null as Date | null],
    endDate: [null as Date | null],
    budget: [0, [Validators.required, Validators.min(0)]],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.projectId = id;
      this.projectService.getById(id).subscribe(project => {
        this.form.patchValue({
          name: project.name,
          description: project.description ?? '',
          code: project.code,
          status: project.status,
          startDate: project.startDate ? new Date(project.startDate) : null,
          endDate: project.endDate ? new Date(project.endDate) : null,
          budget: project.budget,
        });
      });
    }
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    const value = this.form.getRawValue();
    const payload = {
      ...value,
      startDate: value.startDate?.toISOString(),
      endDate: value.endDate?.toISOString(),
    };

    if (this.isEdit && this.projectId) {
      this.projectService.update(this.projectId, { ...payload, id: this.projectId }).subscribe({
        next: () => this.router.navigate(['/projects']),
        error: err => this.snackBar.open(err.error?.message ?? 'Update failed', 'Close', { duration: 5000 }),
      });
    } else {
      this.projectService.create(payload).subscribe({
        next: () => this.router.navigate(['/projects']),
        error: err => this.snackBar.open(err.error?.message ?? 'Create failed', 'Close', { duration: 5000 }),
      });
    }
  }
}
