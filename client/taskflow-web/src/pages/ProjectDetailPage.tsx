import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Plus, UserPlus, X, CheckSquare, Users, AlertCircle } from 'lucide-react';
import { projectsApi } from '../api/projectsApi';
import { tasksApi } from '../api/tasksApi';
import type { ProjectDetailsResponse, ProjectMemberDto, TaskResponse } from '../types';
import { TaskStatusLabel, TaskPriorityLabel, TaskStatus, TaskPriority } from '../types';
import { useAuth } from '../contexts/AuthContext';

const STATUS_CLASS: Record<TaskStatus, string> = {
  [TaskStatus.ToDo]: 'status-todo',
  [TaskStatus.InProgress]: 'status-inprogress',
  [TaskStatus.Completed]: 'status-completed',
  [TaskStatus.Cancelled]: 'status-cancelled',
};

const PRIORITY_CLASS: Record<TaskPriority, string> = {
  [TaskPriority.Low]: 'priority-low',
  [TaskPriority.Medium]: 'priority-medium',
  [TaskPriority.High]: 'priority-high',
  [TaskPriority.Critical]: 'priority-critical',
};

const KANBAN_COLUMNS = [
  TaskStatus.ToDo,
  TaskStatus.InProgress,
  TaskStatus.Completed,
] as const;

export function ProjectDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();

  const [project, setProject] = useState<ProjectDetailsResponse | null>(null);
  const [members, setMembers] = useState<ProjectMemberDto[]>([]);
  const [tasks, setTasks] = useState<TaskResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState<'tasks' | 'members'>('tasks');

  const [showTaskModal, setShowTaskModal] = useState(false);
  const [taskForm, setTaskForm] = useState({
    title: '',
    description: '',
    priority: TaskPriority.Medium,
    assigneeId: '',
  });
  const [taskCreating, setTaskCreating] = useState(false);
  const [taskError, setTaskError] = useState('');

  const [showMemberModal, setShowMemberModal] = useState(false);
  const [memberEmail, setMemberEmail] = useState('');
  const [memberAdding, setMemberAdding] = useState(false);
  const [memberError, setMemberError] = useState('');

  const isOwner = project?.owner.id === user?.id;

  const loadAll = () => {
    if (!id) return;

    Promise.all([
      projectsApi.getById(id),
      projectsApi.getMembers(id),
      tasksApi.getProjectTasks(id, { pageSize: 100 }),
    ])
      .then(([pRes, mRes, tRes]) => {
        setProject(pRes.data);
        setMembers(mRes.data);
        setTasks(tRes.data.items);
      })
      .catch(() => setError('Failed to load project details. Please refresh.'))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadAll();
  }, [id]);

  const handleCreateTask = async () => {
    if (!id || !taskForm.title.trim()) {
      setTaskError('Task title is required.');
      return;
    }

    setTaskCreating(true);
    setTaskError('');

    try {
      await tasksApi.create(id, {
        title: taskForm.title.trim(),
        description: taskForm.description.trim() || undefined,
        priority: taskForm.priority,
        assigneeId: taskForm.assigneeId || undefined,
      });
      setShowTaskModal(false);
      setTaskForm({ title: '', description: '', priority: TaskPriority.Medium, assigneeId: '' });
      loadAll();
    } catch {
      setTaskError('Failed to create task. Please try again.');
    } finally {
      setTaskCreating(false);
    }
  };

  const handleStatusChange = async (taskId: string, status: TaskStatus) => {
    try {
      await tasksApi.updateStatus(taskId, status);
      setTasks(prev =>
        prev.map(t => (t.id === taskId ? { ...t, status } : t)),
      );
    } catch {
      alert('Could not update task status. Please try again.');
    }
  };

  const handleAddMember = async () => {
    if (!id || !memberEmail.trim()) {
      setMemberError('Email address is required.');
      return;
    }

    setMemberAdding(true);
    setMemberError('');

    try {
      await projectsApi.addMember(id, memberEmail.trim().toLowerCase());
      setShowMemberModal(false);
      setMemberEmail('');
      loadAll();
    } catch (err: unknown) {
      const status = (err as { response?: { status?: number } }).response?.status;
      if (status === 409) {
        setMemberError('This user is already a member of the project.');
      } else if (status === 404) {
        setMemberError('No account found with that email address.');
      } else {
        setMemberError('Failed to invite member. Please try again.');
      }
    } finally {
      setMemberAdding(false);
    }
  };

  const handleRemoveMember = async (userId: string, name: string) => {
    if (!id) return;
    if (!confirm(`Remove ${name} from this project?`)) return;

    try {
      await projectsApi.removeMember(id, userId);
      loadAll();
    } catch {
      alert('Failed to remove member. Please try again.');
    }
  };

  if (loading) {
    return <div className="loading-inline"><div className="spinner" /></div>;
  }

  if (error || !project) {
    return (
      <div className="page">
        <div className="alert alert-error">
          <AlertCircle size={16} />
          {error || 'Project not found.'}
        </div>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">{project.name}</h1>
          {project.description && (
            <p className="page-subtitle">{project.description}</p>
          )}
        </div>
        <div className="header-actions">
          {isOwner && (
            <button className="btn btn-ghost" onClick={() => setShowMemberModal(true)}>
              <UserPlus size={16} />
              Invite Member
            </button>
          )}
          <button className="btn btn-primary" onClick={() => setShowTaskModal(true)}>
            <Plus size={16} />
            Add Task
          </button>
        </div>
      </div>

      <div className="tabs">
        <button
          className={`tab ${activeTab === 'tasks' ? 'active' : ''}`}
          onClick={() => setActiveTab('tasks')}
        >
          <CheckSquare size={15} />
          Tasks ({tasks.length})
        </button>
        <button
          className={`tab ${activeTab === 'members' ? 'active' : ''}`}
          onClick={() => setActiveTab('members')}
        >
          <Users size={15} />
          Members ({members.length})
        </button>
      </div>

      {activeTab === 'tasks' && (
        <div className="task-board">
          {KANBAN_COLUMNS.map(col => {
            const columnTasks = tasks.filter(t => t.status === col);
            return (
              <div key={col} className="task-column">
                <div className="column-header">
                  <span className={`column-dot ${STATUS_CLASS[col]}`} />
                  <h3>{TaskStatusLabel[col]}</h3>
                  <span className="column-count">{columnTasks.length}</span>
                </div>

                <div className="task-list">
                  {columnTasks.map(task => (
                    <div key={task.id} className="task-card">
                      <p className="task-title">{task.title}</p>
                      {task.description && (
                        <p className="task-desc">{task.description}</p>
                      )}
                      <div className="task-badges">
                        <span className={`badge ${PRIORITY_CLASS[task.priority as TaskPriority]}`}>
                          {TaskPriorityLabel[task.priority as TaskPriority]}
                        </span>
                        {task.assigneeName && (
                          <span className="badge badge-assignee">
                            👤 {task.assigneeName}
                          </span>
                        )}
                      </div>
                      <select
                        className="status-select"
                        value={task.status}
                        onChange={e => handleStatusChange(task.id, Number(e.target.value) as TaskStatus)}
                        aria-label="Update task status"
                      >
                        <option value={TaskStatus.ToDo}>To Do</option>
                        <option value={TaskStatus.InProgress}>In Progress</option>
                        <option value={TaskStatus.Completed}>Completed</option>
                        <option value={TaskStatus.Cancelled}>Cancelled</option>
                      </select>
                    </div>
                  ))}

                  {columnTasks.length === 0 && (
                    <p className="column-empty">No tasks here</p>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {activeTab === 'members' && (
        <div className="members-list">
          {members.map(m => (
            <div key={m.userId} className="member-row">
              <div className="avatar">
                {m.firstName[0]}{m.lastName[0]}
              </div>
              <div className="member-info">
                <p className="member-name">
                  {m.firstName} {m.lastName}
                  {m.isOwner && <span className="owner-badge">Owner</span>}
                </p>
                <p className="member-email">{m.email}</p>
              </div>
              {isOwner && !m.isOwner && (
                <button
                  className="icon-btn danger"
                  onClick={() => handleRemoveMember(m.userId, `${m.firstName} ${m.lastName}`)}
                  title="Remove from project"
                >
                  <X size={15} />
                </button>
              )}
            </div>
          ))}
        </div>
      )}

      {showTaskModal && (
        <div className="modal-backdrop" onClick={() => { setShowTaskModal(false); setTaskError(''); }}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h2>New Task</h2>
              <button className="icon-btn" onClick={() => { setShowTaskModal(false); setTaskError(''); }}>
                <X size={18} />
              </button>
            </div>

            {taskError && (
              <div className="alert alert-error">
                <AlertCircle size={14} />
                {taskError}
              </div>
            )}

            <div className="form-group">
              <label htmlFor="task-title">Title *</label>
              <input
                id="task-title"
                type="text"
                value={taskForm.title}
                onChange={e => setTaskForm(f => ({ ...f, title: e.target.value }))}
                placeholder="What needs to be done?"
                autoFocus
              />
            </div>

            <div className="form-group">
              <label htmlFor="task-desc">Description</label>
              <textarea
                id="task-desc"
                value={taskForm.description}
                onChange={e => setTaskForm(f => ({ ...f, description: e.target.value }))}
                rows={2}
                placeholder="Additional context (optional)"
              />
            </div>

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="task-priority">Priority</label>
                <select
                  id="task-priority"
                  value={taskForm.priority}
                  onChange={e => setTaskForm(f => ({ ...f, priority: Number(e.target.value) as TaskPriority }))}
                >
                  <option value={TaskPriority.Low}>Low</option>
                  <option value={TaskPriority.Medium}>Medium</option>
                  <option value={TaskPriority.High}>High</option>
                  <option value={TaskPriority.Critical}>Critical</option>
                </select>
              </div>

              <div className="form-group">
                <label htmlFor="task-assignee">Assign To</label>
                <select
                  id="task-assignee"
                  value={taskForm.assigneeId}
                  onChange={e => setTaskForm(f => ({ ...f, assigneeId: e.target.value }))}
                >
                  <option value="">Unassigned</option>
                  {members.map(m => (
                    <option key={m.userId} value={m.userId}>
                      {m.firstName} {m.lastName}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            <div className="modal-footer">
              <button className="btn btn-ghost" onClick={() => { setShowTaskModal(false); setTaskError(''); }}>
                Cancel
              </button>
              <button className="btn btn-primary" onClick={handleCreateTask} disabled={taskCreating}>
                {taskCreating ? 'Creating...' : 'Create Task'}
              </button>
            </div>
          </div>
        </div>
      )}

      {showMemberModal && (
        <div className="modal-backdrop" onClick={() => { setShowMemberModal(false); setMemberError(''); }}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Invite Team Member</h2>
              <button className="icon-btn" onClick={() => { setShowMemberModal(false); setMemberError(''); }}>
                <X size={18} />
              </button>
            </div>

            {memberError && (
              <div className="alert alert-error">
                <AlertCircle size={14} />
                {memberError}
              </div>
            )}

            <div className="form-group">
              <label htmlFor="member-email">Email Address</label>
              <input
                id="member-email"
                type="email"
                value={memberEmail}
                onChange={e => setMemberEmail(e.target.value)}
                placeholder="colleague@company.com"
                autoFocus
              />
              <p style={{ fontSize: 12, color: 'var(--text-3)', marginTop: 4 }}>
                The user must have a TaskFlow account before you can invite them.
              </p>
            </div>

            <div className="modal-footer">
              <button className="btn btn-ghost" onClick={() => { setShowMemberModal(false); setMemberError(''); }}>
                Cancel
              </button>
              <button className="btn btn-primary" onClick={handleAddMember} disabled={memberAdding}>
                {memberAdding ? 'Inviting...' : 'Send Invite'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
