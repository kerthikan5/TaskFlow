// Auth models
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: number;
  isActive: boolean;
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

// Project models
export interface ProjectResponse {
  id: string;
  name: string;
  description?: string;
  ownerId: string;
  ownerName: string;
  status: number;
  dueDate?: string;
  createdAt: string;
  updatedAt: string;
  memberCount: number;
  taskCount: number;
}

export interface ProjectDetailsResponse {
  id: string;
  name: string;
  description?: string;
  owner: User;
  status: number;
  dueDate?: string;
  createdAt: string;
  updatedAt: string;
  members: User[];
}

export interface CreateProjectRequest {
  name: string;
  description?: string;
  status: number;
  dueDate?: string;
}

// Member models
export interface ProjectMemberDto {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: number;
  joinedAt: string;
  isOwner: boolean;
}

// Task models
export interface TaskResponse {
  id: string;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  projectId: string;
  projectName: string;
  createdById: string;
  createdByName: string;
  assigneeId?: string;
  assigneeName?: string;
  dueDate?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  priority: TaskPriority;
  assigneeId?: string;
  dueDate?: string;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Enums
export enum TaskStatus {
  ToDo = 0,
  InProgress = 1,
  Completed = 2,
  Cancelled = 3,
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}

export enum ProjectStatus {
  Active = 0,
  OnHold = 1,
  Completed = 2,
  Archived = 3,
}

// Display labels
export const TaskStatusLabel: Record<TaskStatus, string> = {
  [TaskStatus.ToDo]: 'To Do',
  [TaskStatus.InProgress]: 'In Progress',
  [TaskStatus.Completed]: 'Completed',
  [TaskStatus.Cancelled]: 'Cancelled',
};

export const TaskPriorityLabel: Record<TaskPriority, string> = {
  [TaskPriority.Low]: 'Low',
  [TaskPriority.Medium]: 'Medium',
  [TaskPriority.High]: 'High',
  [TaskPriority.Critical]: 'Critical',
};

export const ProjectStatusLabel: Record<ProjectStatus, string> = {
  [ProjectStatus.Active]: 'Active',
  [ProjectStatus.OnHold]: 'On Hold',
  [ProjectStatus.Completed]: 'Completed',
  [ProjectStatus.Archived]: 'Archived',
};
