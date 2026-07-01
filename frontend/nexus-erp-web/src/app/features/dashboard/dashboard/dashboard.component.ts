import { Component, inject, OnInit, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { DashboardService } from '../../../core/services/api.service';
import { DashboardStats } from '../../../core/models/project.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatCardModule, MatIconModule, BaseChartDirective],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);

  readonly stats = signal<DashboardStats | null>(null);

  chartData: ChartConfiguration<'doughnut'>['data'] = {
    labels: ['To Do', 'In Progress', 'Done'],
    datasets: [{ data: [0, 0, 0], backgroundColor: ['#ff9800', '#2196f3', '#4caf50'] }],
  };

  chartOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    plugins: { legend: { position: 'bottom' } },
  };

  ngOnInit(): void {
    this.dashboardService.getStats().subscribe(stats => {
      this.stats.set(stats);
      this.chartData = {
        ...this.chartData,
        datasets: [{
          data: [stats.tasksByStatus.todo, stats.tasksByStatus.inProgress, stats.tasksByStatus.done],
          backgroundColor: ['#ff9800', '#2196f3', '#4caf50'],
        }],
      };
    });
  }
}
