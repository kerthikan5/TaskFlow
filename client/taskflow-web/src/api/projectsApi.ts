import axiosInstance from './axiosInstance';
import type {
  CreateProjectRequest,
  ProjectDetailsResponse,
  ProjectMemberDto,
  ProjectResponse,
} from '../types';

export const projectsApi = {
  getAll: () =>
    axiosInstance.get<ProjectResponse[]>('/projects'),

  getById: (id: string) =>
    axiosInstance.get<ProjectDetailsResponse>(`/projects/${id}`),

  create: (data: CreateProjectRequest) =>
    axiosInstance.post<ProjectResponse>('/projects', data),

  update: (id: string, data: Partial<CreateProjectRequest>) =>
    axiosInstance.put<ProjectResponse>(`/projects/${id}`, data),

  delete: (id: string) =>
    axiosInstance.delete(`/projects/${id}`),

  getMembers: (projectId: string) =>
    axiosInstance.get<ProjectMemberDto[]>(`/projects/${projectId}/members`),

  addMember: (projectId: string, email: string) =>
    axiosInstance.post<ProjectMemberDto>(`/projects/${projectId}/members`, { email }),

  removeMember: (projectId: string, userId: string) =>
    axiosInstance.delete(`/projects/${projectId}/members/${userId}`),
};
