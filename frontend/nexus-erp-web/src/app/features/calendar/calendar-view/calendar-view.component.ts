import { Component, inject, OnInit, signal } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { map } from 'rxjs';
import { TaskService } from '../../../core/services/api.service';
import { Task, TaskStatus } from '../../../core/models/project.model';
import { StatusLabelPipe } from '../../../shared/pipes/app.pipes';

@Component({
  selector: 'app-calendar-view',
  standalone: true,
  imports: [MatCardModule, MatButtonModule, MatIconModule, StatusLabelPipe],
  templateUrl: './calendar-view.component.html',
  styleUrl: './calendar-view.component.scss',
})
export class CalendarViewComponent implements OnInit {
  private readonly taskService = inject(TaskService);
  private readonly breakpointObserver = inject(BreakpointObserver);

  readonly tasks = signal<Task[]>([]);
  readonly currentMonth = signal(new Date());
  readonly isMobile = toSignal(
    this.breakpointObserver.observe([Breakpoints.Handset, Breakpoints.TabletPortrait])
      .pipe(map(r => r.matches)),
    { initialValue: false },
  );

  readonly weekDays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

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

  goToToday(): void {
    this.currentMonth.set(new Date(new Date().getFullYear(), new Date().getMonth(), 1));
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

  getCalendarCells(): (number | null)[] {
    const month = this.currentMonth();
    const offset = new Date(month.getFullYear(), month.getMonth(), 1).getDay();
    return [...Array.from({ length: offset }, () => null), ...this.getDaysInMonth()];
  }

  getAgendaDays(): number[] {
    return this.getDaysInMonth().filter(day => this.tasksForDay(day).length > 0);
  }

  tasksForDay(day: number): Task[] {
    const month = this.currentMonth();
    return this.tasks().filter(t => {
      if (!t.dueDate) return false;
      const d = new Date(t.dueDate);
      return d.getDate() === day && d.getMonth() === month.getMonth() && d.getFullYear() === month.getFullYear();
    });
  }

  dateForDay(day: number): Date {
    const month = this.currentMonth();
    return new Date(month.getFullYear(), month.getMonth(), day);
  }

  weekdayLabel(day: number): string {
    return this.dateForDay(day).toLocaleDateString('en-US', { weekday: 'short' });
  }

  fullDateLabel(day: number): string {
    return this.dateForDay(day).toLocaleDateString('en-US', { weekday: 'long', month: 'short', day: 'numeric' });
  }

  isToday(day: number): boolean {
    const today = new Date();
    const month = this.currentMonth();
    return today.getDate() === day
      && today.getMonth() === month.getMonth()
      && today.getFullYear() === month.getFullYear();
  }

  isWeekend(day: number): boolean {
    const dow = this.dateForDay(day).getDay();
    return dow === 0 || dow === 6;
  }

  taskStatusClass(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.InProgress:
        return 'status-progress';
      case TaskStatus.InReview:
        return 'status-review';
      case TaskStatus.Done:
        return 'status-done';
      case TaskStatus.Cancelled:
        return 'status-cancelled';
      default:
        return 'status-todo';
    }
  }

}
