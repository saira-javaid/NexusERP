import { Pipe, PipeTransform } from '@angular/core';
import { ProjectStatus, TaskStatus, TaskPriority } from '../../core/models/project.model';

@Pipe({ name: 'statusLabel', standalone: true })
export class StatusLabelPipe implements PipeTransform {
  private readonly projectLabels: Record<ProjectStatus, string> = {
    [ProjectStatus.Planning]: 'Planning',
    [ProjectStatus.Active]: 'Active',
    [ProjectStatus.OnHold]: 'On Hold',
    [ProjectStatus.Completed]: 'Completed',
    [ProjectStatus.Cancelled]: 'Cancelled',
  };

  private readonly taskLabels: Record<TaskStatus, string> = {
    [TaskStatus.Backlog]: 'Backlog',
    [TaskStatus.Todo]: 'To Do',
    [TaskStatus.InProgress]: 'In Progress',
    [TaskStatus.InReview]: 'In Review',
    [TaskStatus.Done]: 'Done',
    [TaskStatus.Cancelled]: 'Cancelled',
  };

  transform(value: number, type: 'project' | 'task' = 'project'): string {
    if (type === 'task') return this.taskLabels[value as TaskStatus] ?? 'Unknown';
    return this.projectLabels[value as ProjectStatus] ?? 'Unknown';
  }
}

@Pipe({ name: 'priorityLabel', standalone: true })
export class PriorityLabelPipe implements PipeTransform {
  private readonly labels: Record<TaskPriority, string> = {
    [TaskPriority.Low]: 'Low',
    [TaskPriority.Medium]: 'Medium',
    [TaskPriority.High]: 'High',
    [TaskPriority.Critical]: 'Critical',
  };

  transform(value: TaskPriority): string {
    return this.labels[value] ?? 'Unknown';
  }
}

@Pipe({ name: 'meetingStatusLabel', standalone: true })
export class MeetingStatusLabelPipe implements PipeTransform {
  private readonly labels: Record<number, string> = {
    0: 'Scheduled',
    1: 'In Progress',
    2: 'Completed',
    3: 'Cancelled',
  };

  transform(value: number): string {
    return this.labels[value] ?? 'Unknown';
  }
}

@Pipe({ name: 'truncate', standalone: true })
export class TruncatePipe implements PipeTransform {
  transform(value: string, limit = 50): string {
    if (!value) return '';
    return value.length > limit ? value.substring(0, limit) + '...' : value;
  }
}

@Pipe({ name: 'fileSize', standalone: true })
export class FileSizePipe implements PipeTransform {
  transform(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
  }
}
