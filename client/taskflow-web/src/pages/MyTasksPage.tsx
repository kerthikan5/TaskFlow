import { useEffect, useState } from 'react';
import { CheckSquare, AlertCircle } from 'lucide-react';
import { tasksApi } from '../api/tasksApi';
import type { TaskResponse } from '../types';
import { TaskStatusLabel, TaskPriorityLabel, TaskStatus, TaskPriority } from '../types';

const PRIORITY_CLASS: Record<TaskPriority, string> = {
  [TaskPriority.Low]: 'priority-low',
  [TaskPriority.Medium]: 'priority-medium',
  [TaskPriority.High]: 'priority-high',
  [TaskPriority.Critical]: 'priority-critical',
};

const FILTER_OPTIONS: [number, string][] = [
  [-1, 'All'],
  [TaskStatus.ToDo, 'To Do'],
  [TaskStatus.InProgress, 'In Progress'],
  [TaskStatus.Completed, 'Completed'],
  [TaskStatus.Cancelled, 'Cancelled'],
];

export function MyTasksPage() {
  const [tasks, setTasks] = useState<TaskResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filter, setFilter] = useState<TaskStatus | -1>(-1);

  const loadTasks = () => {
    setLoading(true);
    setError('');
    tasksApi
      .getMyAssigned({ pageSize: 100 })
      .then(r => setTasks(r.data.items))
      .catch(() => setError('Failed to load your tasks. Please refresh.'))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadTasks();
  }, []);

  const handleStatusChange = async (taskId: string, status: TaskStatus) => {
    try {
      await tasksApi.updateStatus(taskId, status);
      setTasks(prev =>
        prev.map(t => (t.id === taskId ? { ...t, status } : t)),
      );
    } catch {
      alert('Could not update task status. Please refresh and try again.');
    }
  };

  const filtered = filter === -1 ? tasks : tasks.filter(t => t.status === filter);

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">My Tasks</h1>
          <p className="page-subtitle">
            All tasks assigned to you, across every project you're a member of.
          </p>
        </div>
      </div>

      <div className="filter-bar">
        {FILTER_OPTIONS.map(([val, label]) => (
          <button
            key={val}
            className={`filter-chip ${filter === val ? 'active' : ''}`}
            onClick={() => setFilter(val as TaskStatus | -1)}
          >
            {label}
            {val === -1 && ` (${tasks.length})`}
            {val !== -1 && ` (${tasks.filter(t => t.status === val).length})`}
          </button>
        ))}
      </div>

      {error && (
        <div className="alert alert-error" style={{ marginBottom: 20 }}>
          <AlertCircle size={16} />
          {error}
        </div>
      )}

      {loading && (
        <div className="loading-inline"><div className="spinner" /></div>
      )}

      {!loading && !error && filtered.length === 0 && (
        <div className="empty-hero">
          <CheckSquare size={48} opacity={0.3} />
          <p>
            {filter === -1
              ? 'No tasks have been assigned to you yet.'
              : `No ${TaskStatusLabel[filter as TaskStatus]} tasks.`}
          </p>
        </div>
      )}

      {!loading && !error && filtered.length > 0 && (
        <div className="task-table">
          <div className="task-table-header">
            <span>Task</span>
            <span>Project</span>
            <span>Priority</span>
            <span>Status</span>
            <span>Due Date</span>
          </div>

          {filtered.map(task => (
            <div key={task.id} className="task-row">
              <div>
                <p className="task-title">{task.title}</p>
                {task.description && (
                  <p className="task-desc-sm">{task.description}</p>
                )}
              </div>

              <span className="project-link">{task.projectName}</span>

              <span className={`badge ${PRIORITY_CLASS[task.priority as TaskPriority]}`}>
                {TaskPriorityLabel[task.priority as TaskPriority]}
              </span>

              <select
                className="status-select inline"
                value={task.status}
                onChange={e => handleStatusChange(task.id, Number(e.target.value) as TaskStatus)}
                aria-label={`Change status for "${task.title}"`}
              >
                <option value={TaskStatus.ToDo}>To Do</option>
                <option value={TaskStatus.InProgress}>In Progress</option>
                <option value={TaskStatus.Completed}>Completed</option>
                <option value={TaskStatus.Cancelled}>Cancelled</option>
              </select>

              <span className="due-date">
                {task.dueDate
                  ? new Date(task.dueDate).toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' })
                  : '—'}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
