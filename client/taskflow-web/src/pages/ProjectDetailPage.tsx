import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Plus, UserPlus, X, CheckSquare, Users } from 'lucide-react';
import { projectsApi } from '../api/projectsApi';
import { tasksApi } from '../api/tasksApi';
import type { ProjectDetailsResponse, ProjectMemberDto, TaskResponse } from '../types';
import { TaskStatusLabel, TaskPriorityLabel, TaskStatus, TaskPriority } from '../types';
import { useAuth } from '../contexts/AuthContext';

export function ProjectDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const [project, setProject] = useState<ProjectDetailsResponse | null>(null);
  const [members, setMembers] = useState<ProjectMemberDto[]>([]);
  const [tasks, setTasks] = useState<TaskResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'tasks' | 'members'>('tasks');

  // Task modal
  const [showTaskModal, setShowTaskModal] = useState(false);
  const [taskForm, setTaskForm] = useState({ title: '', description: '', priority: TaskPriority.Medium, assigneeId: '' });
  const [taskCreating, setTaskCreating] = useState(false);

  // Member modal
  const [showMemberModal, setShowMemberModal] = useState(false);
  const [memberEmail, setMemberEmail] = useState('');
  const [memberAdding, setMemberAdding] = useState(false);

  const isOwner = project?.owner.id === user?.id;

  const load = () => {
    if (!id) return;
    Promise.all([
      projectsApi.getById(id),
      projectsApi.getMembers(id),
      tasksApi.getProjectTasks(id, { pageSize: 50 }),
    ]).then(([pRes, mRes, tRes]) => {
      setProject(pRes.data);
      setMembers(mRes.data);
      setTasks(tRes.data.items);
    }).finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, [id]);

  const handleCreateTask = async () => {
    if (!id || !taskForm.title.trim()) return;
    setTaskCreating(true);
    try {
      await tasksApi.create(id, {
        title: taskForm.title,
        description: taskForm.description || undefined,
        priority: taskForm.priority,
        assigneeId: taskForm.assigneeId || undefined,
      });
      setShowTaskModal(false);
      setTaskForm({ title: '', description: '', priority: TaskPriority.Medium, assigneeId: '' });
      load();
    } finally {
      setTaskCreating(false);
    }
  };

  const handleAddMember = async () => {
    if (!id || !memberEmail.trim()) return;
    setMemberAdding(true);
    try {
      await projectsApi.addMember(id, memberEmail.trim());
      setShowMemberModal(false);
      setMemberEmail('');
      load();
    } finally {
      setMemberAdding(false);
    }
  };

  const handleStatusChange = async (taskId: string, status: TaskStatus) => {
    await tasksApi.updateStatus(taskId, status);
    load();
  };

  const handleRemoveMember = async (userId: string) => {
    if (!id) return;
    if (!confirm('Remove this member from the project?')) return;
    await projectsApi.removeMember(id, userId);
    load();
  };

  const priorityColors: Record<number, string> = { 0: 'priority-low', 1: 'priority-medium', 2: 'priority-high', 3: 'priority-critical' };
  const statusColors: Record<number, string> = { 0: 'status-todo', 1: 'status-inprogress', 2: 'status-completed', 3: 'status-cancelled' };

  if (loading) return <div className="loading-inline"><div className="spinner" /></div>;
  if (!project) return <div className="page"><p>Project not found.</p></div>;

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">{project.name}</h1>
          {project.description && <p className="page-subtitle">{project.description}</p>}
        </div>
        <div className="header-actions">
          {isOwner && (
            <button className="btn btn-ghost" onClick={() => setShowMemberModal(true)}>
              <UserPlus size={16} /> Invite Member
            </button>
          )}
          <button className="btn btn-primary" onClick={() => setShowTaskModal(true)}>
            <Plus size={16} /> Add Task
          </button>
        </div>
      </div>

      <div className="tabs">
        <button className={`tab ${activeTab === 'tasks' ? 'active' : ''}`} onClick={() => setActiveTab('tasks')}>
          <CheckSquare size={15} /> Tasks ({tasks.length})
        </button>
        <button className={`tab ${activeTab === 'members' ? 'active' : ''}`} onClick={() => setActiveTab('members')}>
          <Users size={15} /> Members ({members.length})
        </button>
      </div>

      {activeTab === 'tasks' && (
        <div className="task-board">
          {[TaskStatus.ToDo, TaskStatus.InProgress, TaskStatus.Completed].map(col => (
            <div key={col} className="task-column">
              <div className="column-header">
                <span className={`column-dot ${statusColors[col]}`} />
                <h3>{TaskStatusLabel[col]}</h3>
                <span className="column-count">{tasks.filter(t => t.status === col).length}</span>
              </div>
              <div className="task-list">
                {tasks.filter(t => t.status === col).map(task => (
                  <div key={task.id} className="task-card">
                    <p className="task-title">{task.title}</p>
                    {task.description && <p className="task-desc">{task.description}</p>}
                    <div className="task-badges">
                      <span className={`badge ${priorityColors[task.priority]}`}>{TaskPriorityLabel[task.priority]}</span>
                      {task.assigneeName && <span className="badge badge-assignee">👤 {task.assigneeName}</span>}
                    </div>
                    <select
                      className="status-select"
                      value={task.status}
                      onChange={e => handleStatusChange(task.id, Number(e.target.value) as TaskStatus)}
                    >
                      <option value={TaskStatus.ToDo}>To Do</option>
                      <option value={TaskStatus.InProgress}>In Progress</option>
                      <option value={TaskStatus.Completed}>Completed</option>
                      <option value={TaskStatus.Cancelled}>Cancelled</option>
                    </select>
                  </div>
                ))}
                {tasks.filter(t => t.status === col).length === 0 && (
                  <p className="column-empty">No tasks</p>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {activeTab === 'members' && (
        <div className="members-list">
          {members.map(m => (
            <div key={m.userId} className="member-row">
              <div className="avatar">{m.firstName[0]}{m.lastName[0]}</div>
              <div className="member-info">
                <p className="member-name">{m.firstName} {m.lastName} {m.isOwner && <span className="owner-badge">Owner</span>}</p>
                <p className="member-email">{m.email}</p>
              </div>
              {isOwner && !m.isOwner && (
                <button className="icon-btn danger" onClick={() => handleRemoveMember(m.userId)} title="Remove">
                  <X size={15} />
                </button>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Task Modal */}
      {showTaskModal && (
        <div className="modal-backdrop" onClick={() => setShowTaskModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h2>New Task</h2>
              <button className="icon-btn" onClick={() => setShowTaskModal(false)}><X size={18} /></button>
            </div>
            <div className="form-group">
              <label>Title *</label>
              <input type="text" value={taskForm.title} onChange={e => setTaskForm(f => ({ ...f, title: e.target.value }))} placeholder="Task title" />
            </div>
            <div className="form-group">
              <label>Description</label>
              <textarea value={taskForm.description} onChange={e => setTaskForm(f => ({ ...f, description: e.target.value }))} rows={2} placeholder="Optional..." />
            </div>
            <div className="form-row">
              <div className="form-group">
                <label>Priority</label>
                <select value={taskForm.priority} onChange={e => setTaskForm(f => ({ ...f, priority: Number(e.target.value) as TaskPriority }))}>
                  <option value={TaskPriority.Low}>Low</option>
                  <option value={TaskPriority.Medium}>Medium</option>
                  <option value={TaskPriority.High}>High</option>
                  <option value={TaskPriority.Critical}>Critical</option>
                </select>
              </div>
              <div className="form-group">
                <label>Assign To</label>
                <select value={taskForm.assigneeId} onChange={e => setTaskForm(f => ({ ...f, assigneeId: e.target.value }))}>
                  <option value="">Unassigned</option>
                  {members.map(m => (
                    <option key={m.userId} value={m.userId}>{m.firstName} {m.lastName}</option>
                  ))}
                </select>
              </div>
            </div>
            <div className="modal-footer">
              <button className="btn btn-ghost" onClick={() => setShowTaskModal(false)}>Cancel</button>
              <button className="btn btn-primary" onClick={handleCreateTask} disabled={taskCreating}>
                {taskCreating ? 'Creating...' : 'Create Task'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Member Modal */}
      {showMemberModal && (
        <div className="modal-backdrop" onClick={() => setShowMemberModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Invite Member</h2>
              <button className="icon-btn" onClick={() => setShowMemberModal(false)}><X size={18} /></button>
            </div>
            <div className="form-group">
              <label>Email Address</label>
              <input type="email" value={memberEmail} onChange={e => setMemberEmail(e.target.value)} placeholder="colleague@example.com" />
            </div>
            <div className="modal-footer">
              <button className="btn btn-ghost" onClick={() => setShowMemberModal(false)}>Cancel</button>
              <button className="btn btn-primary" onClick={handleAddMember} disabled={memberAdding}>
                {memberAdding ? 'Inviting...' : 'Invite'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
