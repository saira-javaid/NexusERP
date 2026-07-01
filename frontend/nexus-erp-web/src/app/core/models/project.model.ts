export enum ProjectStatus {
  Planning = 0,
  Active = 1,
  OnHold = 2,
  Completed = 3,
  Cancelled = 4,
}

export enum TaskStatus {
  Backlog = 0,
  Todo = 1,
  InProgress = 2,
  InReview = 3,
  Done = 4,
  Cancelled = 5,
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}

export interface Project {
  id: string;
  name: string;
  description?: string;
  code: string;
  status: ProjectStatus;
  startDate?: string;
  endDate?: string;
  budget: number;
  managerId?: string;
  managerName?: string;
  taskCount: number;
  memberCount: number;
  createdAt: string;
}

export interface Task {
  id: string;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  order: number;
  dueDate?: string;
  startDate?: string;
  estimatedHours?: number;
  actualHours?: number;
  projectId: string;
  projectName: string;
  assigneeId?: string;
  assigneeName?: string;
  parentTaskId?: string;
  tags?: string;
  createdAt: string;
}

export interface KanbanColumn {
  status: TaskStatus;
  label: string;
  tasks: Task[];
}

export interface DashboardStats {
  totalProjects: number;
  totalTasks: number;
  unreadNotifications: number;
  tasksByStatus: {
    todo: number;
    inProgress: number;
    done: number;
  };
}
