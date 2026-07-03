import { Component, inject, OnInit, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { PageEvent } from '@angular/material/paginator';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { ReportService } from '../../../core/services/api.service';
import { UserPreferencesService } from '../../../core/services/user-preferences.service';
import { ExportService } from '../../../shared/services/export.service';
import { ReportOverview, ReportProjectRow, StatusCount } from '../../../core/models/report.model';
import { StatusLabelPipe } from '../../../shared/pipes/app.pipes';
import { ListPaginationComponent } from '../../../shared/components/list-pagination/list-pagination.component';

@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [
    CurrencyPipe, MatCardModule, MatButtonModule, MatTableModule,
    MatIconModule, MatChipsModule, BaseChartDirective, StatusLabelPipe,
    ListPaginationComponent,
  ],
  templateUrl: './reports-page.component.html',
  styleUrl: './reports-page.component.scss',
})
export class ReportsPageComponent implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly exportService = inject(ExportService);
  private readonly preferences = inject(UserPreferencesService);

  readonly overview = signal<ReportOverview | null>(null);
  readonly allProjects = signal<ReportProjectRow[]>([]);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(this.preferences.getPageSize());
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly chartsReady = signal(false);

  readonly projectDataSource = new MatTableDataSource<ReportProjectRow>([]);
  readonly projectsByStatus = signal<StatusCount[]>([]);
  readonly taskStatusRows = signal<{ label: string; count: number; color: string }[]>([]);

  projectChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [{ data: [], backgroundColor: '#3f51b5', label: 'Projects' }],
  };

  taskChartData: ChartConfiguration<'doughnut'>['data'] = {
    labels: ['To Do', 'In Progress', 'In Review', 'Done'],
    datasets: [{ data: [0, 0, 0, 0], backgroundColor: ['#ff9800', '#2196f3', '#9c27b0', '#4caf50'] }],
  };

  barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } },
  };

  doughnutOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' } },
  };

  displayedColumns = ['code', 'name', 'status', 'tasks', 'budget', 'manager'];

  ngOnInit(): void {
    this.loadOverview();
  }

  loadOverview(): void {
    this.loading.set(true);
    this.error.set(null);
    this.chartsReady.set(false);

    this.reportService.getOverview().subscribe({
      next: data => {
        this.overview.set(data);
        this.allProjects.set(data.projects ?? []);
        this.pageIndex.set(0);
        this.updateProjectPage();
        this.projectsByStatus.set(data.projectsByStatus ?? []);
        this.taskStatusRows.set([
          { label: 'To Do', count: data.tasksByStatus.todo, color: '#ff9800' },
          { label: 'In Progress', count: data.tasksByStatus.inProgress, color: '#2196f3' },
          { label: 'In Review', count: data.tasksByStatus.inReview, color: '#9c27b0' },
          { label: 'Done', count: data.tasksByStatus.done, color: '#4caf50' },
        ]);
        this.updateCharts(data);
        this.chartsReady.set(true);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load report data. Please try again.');
        this.loading.set(false);
      },
    });
  }

  private updateCharts(data: ReportOverview): void {
    const statusColors: Record<string, string> = {
      Planning: '#3f51b5',
      Active: '#009688',
      OnHold: '#ff9800',
      Completed: '#4caf50',
      Cancelled: '#9e9e9e',
    };

    this.projectChartData = {
      labels: data.projectsByStatus.map(s => s.label),
      datasets: [{
        label: 'Projects',
        data: data.projectsByStatus.map(s => s.count),
        backgroundColor: data.projectsByStatus.map(s => statusColors[s.label] ?? '#3f51b5'),
      }],
    };

    const t = data.tasksByStatus;
    this.taskChartData = {
      labels: ['To Do', 'In Progress', 'In Review', 'Done'],
      datasets: [{
        data: [t.todo, t.inProgress, t.inReview, t.done],
        backgroundColor: ['#ff9800', '#2196f3', '#9c27b0', '#4caf50'],
      }],
    };
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    if (event.pageSize !== this.preferences.getPageSize()) {
      this.preferences.update({ pageSize: event.pageSize });
    }
    this.updateProjectPage();
  }

  private updateProjectPage(): void {
    const start = this.pageIndex() * this.pageSize();
    this.projectDataSource.data = this.allProjects().slice(start, start + this.pageSize());
  }

  exportReport(): void {
    const data = this.overview();
    if (!data) return;

    this.exportService.exportToExcel(
      this.allProjects().map(p => ({
        Code: p.code,
        Name: p.name,
        Status: p.status,
        Tasks: p.taskCount,
        Budget: p.budget,
        Manager: p.managerName ?? '',
      })),
      'project-overview-report',
    );
  }
}
