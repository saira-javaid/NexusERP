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
import { MatChipsModule } from '@angular/material/chips';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { ProjectService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { ExportService } from '../../../shared/services/export.service';
import { UserPreferencesService } from '../../../core/services/user-preferences.service';
import { Project, ProjectStatus } from '../../../core/models/project.model';
import { StatusLabelPipe } from '../../../shared/pipes/app.pipes';
import { ListPaginationComponent } from '../../../shared/components/list-pagination/list-pagination.component';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [
    RouterLink, ReactiveFormsModule, MatTableModule, MatButtonModule,
    MatIconModule, MatInputModule, MatFormFieldModule, MatSelectModule,
    MatChipsModule, StatusLabelPipe, ListPaginationComponent,
  ],
  templateUrl: './project-list.component.html',
  styleUrl: './project-list.component.scss',
})
export class ProjectListComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly authService = inject(AuthService);
  private readonly exportService = inject(ExportService);
  private readonly preferences = inject(UserPreferencesService);

  readonly projects = signal<Project[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(this.preferences.getPageSize());
  readonly canEdit = this.authService.hasPermission('projects.edit');

  searchControl = new FormControl('');
  statusControl = new FormControl<ProjectStatus | null>(null);

  displayedColumns = ['code', 'name', 'status', 'manager', 'tasks', 'actions'];
  projectStatuses = Object.values(ProjectStatus).filter(v => typeof v === 'number') as ProjectStatus[];

  ngOnInit(): void {
    this.loadProjects();
    this.searchControl.valueChanges.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => {
      this.pageIndex.set(0);
      this.loadProjects();
    });
    this.statusControl.valueChanges.subscribe(() => {
      this.pageIndex.set(0);
      this.loadProjects();
    });
  }

  loadProjects(): void {
    this.projectService.getAll(
      this.pageIndex() + 1,
      this.pageSize(),
      this.searchControl.value ?? undefined,
      this.statusControl.value ?? undefined,
    ).subscribe(result => {
      this.projects.set(result.items);
      this.totalCount.set(result.totalCount);
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    if (event.pageSize !== this.preferences.getPageSize()) {
      this.preferences.update({ pageSize: event.pageSize });
    }
    this.loadProjects();
  }

  exportExcel(): void {
    const data = this.projects().map(p => ({
      Code: p.code, Name: p.name, Status: p.status, Budget: p.budget, Tasks: p.taskCount,
    }));
    this.exportService.exportToExcel(data, 'projects');
  }

  exportPdf(): void {
    const data = this.projects().map(p => ({
      code: p.code, name: p.name, status: p.status, budget: p.budget,
    }));
    this.exportService.exportToPdf(data,
      [{ header: 'Code', key: 'code' }, { header: 'Name', key: 'name' }, { header: 'Budget', key: 'budget' }],
      'projects', 'Projects Report');
  }
}
