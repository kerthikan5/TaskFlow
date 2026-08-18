import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { FolderKanban, CheckSquare, Users, TrendingUp, AlertCircle } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { projectsApi } from '../api/projectsApi';
import { tasksApi } from '../api/tasksApi';
import type { ProjectResponse, TaskResponse } from '../types';
import { TaskStatusLabel, TaskPriorityLabel, TaskStatus } from '../types';

// ─── Lookup tables ───────────────────────────────────────────────────────────

const PROJECT_STATUS_CLASS: Record<number, string> = {
  0: 'badge-active',
  1: 'badge-warning',
  2: 'badge-success',
  3: 'badge-muted',
};

const PROJECT_STATUS_LABEL: Record<number, string> = {
  0: 'Active',
  1: 'On Hold',
  2: 'Completed',
  3: 'Archived',
};

const PRIORITY_CLASS: Record<number, string> = {
  0: 'priority-low',
  1: 'priority-medium',
  2: 'priority-high',
  3: 'priority-critical',
};

const STATUS_CLASS: Record<number, string> = {
  0: 'status-todo',
  1: 'status-inprogress',
  2: 'status-completed',
  3: 'status-cancelled',
};

// ─── Component ───────────────────────────────────────────────────────────────

export function DashboardPage() {
  const { user } = useAuth();
  const [projects, setProjects] = useState<ProjectResponse[]>([]);
  const [tasks, setTasks] = useState<TaskResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    Promise.all([
      projectsApi.getAll(),
      tasksApi.getMyAssigned({ pageSize: 5 }),
    ])
      .then(([pRes, tRes]) => {
        setProjects(pRes.data);
        setTasks(tRes.data.items);
      })
      .catch(() => setError('Could not load dashboard data. Is the API running?'))
      .finally(() => setLoading(false));
  }, []);

  const activeProjects = projects.filter(p => p.status === 0).length;
  const completedTasks = tasks.filter(t => t.status === TaskStatus.Completed).length;
  const pendingTasks = tasks.filter(
    t => t.status === TaskStatus.ToDo || t.status === TaskStatus.InProgress,
  ).length;

  if (loading) {
    return <div className="loading-inline"><div className="spinner" /></div>;
  }

  if (error) {
    return (
      <div className="page">
        <div className="alert alert-error">
          <AlertCircle size={16} />
          {error}
        </div>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Welcome back, {user?.firstName} 👋</h1>
          <p className="page-subtitle">Here's what's happening across your projects.</p>
        </div>
      </div>

      {/* ── Stats row ─────────────────────────────────────────────────────── */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon" style={{ background: 'rgba(99,102,241,0.15)' }}>
            <FolderKanban size={22} color="#6366f1" />
          </div>
          <div>
            <p className="stat-label">Total Projects</p>
            <p className="stat-value">{projects.length}</p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon" style={{ background: 'rgba(16,185,129,0.15)' }}>
            <TrendingUp size={22} color="#10b981" />
          </div>
          <div>
            <p className="stat-label">Active Projects</p>
            <p className="stat-value">{activeProjects}</p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon" style={{ background: 'rgba(245,158,11,0.15)' }}>
            <CheckSquare size={22} color="#f59e0b" />
          </div>
          <div>
            <p className="stat-label">Pending Tasks</p>
            <p className="stat-value">{pendingTasks}</p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon" style={{ background: 'rgba(59,130,246,0.15)' }}>
            <Users size={22} color="#3b82f6" />
          </div>
          <div>
            <p className="stat-label">Completed Tasks</p>
            <p className="stat-value">{completedTasks}</p>
          </div>
        </div>
      </div>

      {/* ── Two-column overview ────────────────────────────────────────────── */}
      <div className="dashboard-grid">
        {/* Recent Projects */}
        <div className="card">
          <div className="card-header">
            <h2 className="card-title">Recent Projects</h2>
            <Link to="/projects" className="card-link">View all →</Link>
          </div>
          <div className="list">
            {projects.slice(0, 5).map(p => (
              <Link to={`/projects/${p.id}`} key={p.id} className="list-item">
                <div className="list-item-icon"><FolderKanban size={16} /></div>
                <div className="list-item-body">
                  <p className="list-item-title">{p.name}</p>
                  <p className="list-item-meta">{p.memberCount} members · {p.taskCount} tasks</p>
                </div>
                <span className={`badge ${PROJECT_STATUS_CLASS[p.status]}`}>
                  {PROJECT_STATUS_LABEL[p.status]}
                </span>
              </Link>
            ))}
            {projects.length === 0 && (
              <p className="empty-state">
                No projects yet. <Link to="/projects">Create one →</Link>
              </p>
            )}
          </div>
        </div>

        {/* My Assigned Tasks */}
        <div className="card">
          <div className="card-header">
            <h2 className="card-title">My Assigned Tasks</h2>
            <Link to="/my-tasks" className="card-link">View all →</Link>
          </div>
          <div className="list">
            {tasks.map(t => (
              <div key={t.id} className="list-item">
                <div className="list-item-body">
                  <p className="list-item-title">{t.title}</p>
                  <p className="list-item-meta">{t.projectName}</p>
                </div>
                <div className="badge-group">
                  <span className={`badge ${PRIORITY_CLASS[t.priority]}`}>
                    {TaskPriorityLabel[t.priority]}
                  </span>
                  <span className={`badge ${STATUS_CLASS[t.status]}`}>
                    {TaskStatusLabel[t.status]}
                  </span>
                </div>
              </div>
            ))}
            {tasks.length === 0 && (
              <p className="empty-state">No tasks assigned to you.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
