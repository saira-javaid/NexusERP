import { Component, inject, OnInit, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { TaskService } from '../../../core/services/api.service';
import { Task } from '../../../core/models/project.model';
import { StatusLabelPipe } from '../../../shared/pipes/app.pipes';

@Component({
  selector: 'app-calendar-view',
  standalone: true,
  imports: [MatCardModule, MatChipsModule, StatusLabelPipe],
  templateUrl: './calendar-view.component.html',
  styleUrl: './calendar-view.component.scss',
})
export class CalendarViewComponent implements OnInit {
  private readonly taskService = inject(TaskService);

  readonly tasks = signal<Task[]>([]);
  readonly currentMonth = signal(new Date());

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    const month = this.currentMonth();
    const from = new Date(month.getFullYear(), month.getMonth(), 1).toISOString();
    const to = new Date(month.getFullYear(), month.getMonth() + 1, 0).toISOString();
    this.taskService.getCalendar(from, to).subscribe(tasks => this.tasks.set(tasks));
  }

  prevMonth(): void {
    const d = this.currentMonth();
    this.currentMonth.set(new Date(d.getFullYear(), d.getMonth() - 1, 1));
    this.loadTasks();
  }

  nextMonth(): void {
    const d = this.currentMonth();
    this.currentMonth.set(new Date(d.getFullYear(), d.getMonth() + 1, 1));
    this.loadTasks();
  }

  monthLabel(): string {
    return this.currentMonth().toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  }

  getDaysInMonth(): number[] {
    const month = this.currentMonth();
    const days = new Date(month.getFullYear(), month.getMonth() + 1, 0).getDate();
    return Array.from({ length: days }, (_, i) => i + 1);
  }

  tasksForDay(day: number): Task[] {
    const month = this.currentMonth();
    return this.tasks().filter(t => {
      if (!t.dueDate) return false;
      const d = new Date(t.dueDate);
      return d.getDate() === day && d.getMonth() === month.getMonth() && d.getFullYear() === month.getFullYear();
    });
  }
}
