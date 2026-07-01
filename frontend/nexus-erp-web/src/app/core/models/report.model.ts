export interface ReportSummary {
  totalProjects: number;
  totalTasks: number;
  activeProjects: number;
  completedProjects: number;
  totalBudget: number;
}

export interface StatusCount {
  status: number;
  label: string;
  count: number;
}

export interface ReportProjectRow {
  id: string;
  code: string;
  name: string;
  status: number;
  budget: number;
  taskCount: number;
  managerName?: string;
}

export interface ReportOverview {
  summary: ReportSummary;
  projectsByStatus: StatusCount[];
  tasksByStatus: {
    todo: number;
    inProgress: number;
    inReview: number;
    done: number;
    backlog: number;
  };
  projects: ReportProjectRow[];
}
