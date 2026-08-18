import axiosInstance from './axiosInstance';
import type { CreateTaskRequest, PagedResponse, TaskResponse, TaskStatus } from '../types';

export const tasksApi = {
  getProjectTasks: (projectId: string, params?: { pageNumber?: number; pageSize?: number; status?: TaskStatus }) =>
    axiosInstance.get<PagedResponse<TaskResponse>>(`/projects/${projectId}/tasks`, { params }),

  getMyAssigned: (params?: { pageNumber?: number; pageSize?: number }) =>
    axiosInstance.get<PagedResponse<TaskResponse>>('/tasks/my-assigned', { params }),

  getById: (id: string) =>
    axiosInstance.get<TaskResponse>(`/tasks/${id}`),

  create: (projectId: string, data: CreateTaskRequest) =>
    axiosInstance.post<TaskResponse>(`/projects/${projectId}/tasks`, data),

  updateStatus: (taskId: string, status: TaskStatus) =>
    axiosInstance.patch<TaskResponse>(`/tasks/${taskId}/status`, { status }),

  delete: (taskId: string) =>
    axiosInstance.delete(`/tasks/${taskId}`),
};
