import axiosInstance from './axiosInstance';
import type { AuthResponse, LoginRequest, RegisterRequest, User } from '../types';

export const authApi = {
  register: (data: RegisterRequest) =>
    axiosInstance.post<AuthResponse>('/auth/register', data),

  login: (data: LoginRequest) =>
    axiosInstance.post<AuthResponse>('/auth/login', data),

  me: () =>
    axiosInstance.get<User>('/auth/me'),
};
