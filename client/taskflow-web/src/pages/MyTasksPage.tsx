import { useEffect, useState } from 'react';
import { CheckSquare } from 'lucide-react';
import { tasksApi } from '../api/tasksApi';
import type { TaskResponse } from '../types';
import { TaskStatusLabel, TaskPriorityLabel, TaskStatus } from '../types';

export function MyTasksPage() {
  const [tasks, setTasks] = useState<TaskResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<TaskStatus | -1>(-1);

  const load = () => {
    setLoading(true);
    tasksApi.getMyAssigned({ pageSize: 50 })
      .then(r => setTasks(r.data.items))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const handleStatusChange = async (taskId: string, status: TaskStatus) => {
    await tasksApi.updateStatus(taskId, status);
    load();
  };

  const filtered = filter === -1 ? tasks : tasks.filter(t => t.status === filter);

  const priorityColors: Record<number, string> = { 0: 'priority-low', 1: 'priority-medium', 2: 'priority-high', 3: 'priority-critical' };
  const statusColors: Record<number, string> = { 0: 'status-todo', 1: 'status-inprogress', 2: 'status-completed', 3: 'status-cancelled' };

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">My Tasks</h1>
          <p className="page-subtitle">All tasks assigned to you across every project.</p>
        </div>
      </div>

      <div className="filter-bar">
        {([[-1, 'All'], [TaskStatus.ToDo, 'To Do'], [TaskStatus.InProgress, 'In Progress'], [TaskStatus.Completed, 'Completed']] as [number, string][]).map(([val, label]) => (
          <button
            key={val}
            className={`filter-chip ${filter === val ? 'active' : ''}`}
            onClick={() => setFilter(val as TaskStatus | -1)}
          >
            {label}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="loading-inline"><div className="spinner" /></div>
      ) : filtered.length === 0 ? (
        <div className="empty-hero">
          <CheckSquare size={48} opacity={0.3} />
          <p>No tasks found.</p>
        </div>
      ) : (
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
                {task.description && <p className="task-desc-sm">{task.description}</p>}
              </div>
              <span className="project-link">{task.projectName}</span>
              <span className={`badge ${priorityColors[task.priority]}`}>{TaskPriorityLabel[task.priority]}</span>
              <select
                className={`status-select inline ${statusColors[task.status]}`}
                value={task.status}
                onChange={e => handleStatusChange(task.id, Number(e.target.value) as TaskStatus)}
              >
                <option value={TaskStatus.ToDo}>To Do</option>
                <option value={TaskStatus.InProgress}>In Progress</option>
                <option value={TaskStatus.Completed}>Completed</option>
                <option value={TaskStatus.Cancelled}>Cancelled</option>
              </select>
              <span className="due-date">
                {task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '—'}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
