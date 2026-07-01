import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { TaskService } from '../../../core/services/api.service';
import { KanbanColumn, Task, TaskStatus } from '../../../core/models/project.model';
import { PriorityLabelPipe, StatusLabelPipe } from '../../../shared/pipes/app.pipes';

@Component({
  selector: 'app-kanban-board',
  standalone: true,
  imports: [DragDropModule, MatCardModule, MatChipsModule, MatIconModule, PriorityLabelPipe],
  templateUrl: './kanban-board.component.html',
  styleUrl: './kanban-board.component.scss',
})
export class KanbanBoardComponent implements OnInit {
  private readonly taskService = inject(TaskService);
  private readonly route = inject(ActivatedRoute);

  readonly columns = signal<KanbanColumn[]>([]);
  projectId = '';

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('projectId') ?? '';
    this.loadBoard();
  }

  loadBoard(): void {
    this.taskService.getKanban(this.projectId).subscribe(cols => this.columns.set(cols));
  }

  get connectedLists(): string[] {
    return this.columns().map(c => `list-${c.status}`);
  }

  drop(event: CdkDragDrop<Task[]>, column: KanbanColumn): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);
      const task = event.container.data[event.currentIndex];
      this.taskService.move(task.id, column.status, event.currentIndex).subscribe();
    }
  }
}
