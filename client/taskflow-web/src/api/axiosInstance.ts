import axios from 'axios';

/**
 * Configured Axios instance for all TaskFlow API requests.
 *
 * Base URL is relative ('/api') so that Vite's dev proxy forwards
 * requests to the .NET backend without triggering browser CORS errors.
 * In production, set VITE_API_BASE_URL in your environment.
 */
const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 15_000,
});

// ── Request interceptor ──────────────────────────────────────────────────────
// Reads the JWT from localStorage and attaches it as a Bearer token.
// All protected endpoints require this header.
axiosInstance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error),
);

// ── Response interceptor ─────────────────────────────────────────────────────
// On 401 Unauthorized, clear local auth state and redirect to /login.
// This handles token expiry transparently without every page needing to check.
axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  },
);

export default axiosInstance;
