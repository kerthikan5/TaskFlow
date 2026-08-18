import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, FolderKanban, Trash2, X } from 'lucide-react';
import { projectsApi } from '../api/projectsApi';
import type { ProjectResponse } from '../types';
import { ProjectStatusLabel, ProjectStatus } from '../types';

export function ProjectsPage() {
  const [projects, setProjects] = useState<ProjectResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState({ name: '', description: '', status: ProjectStatus.Active });
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState('');

  const load = () => {
    setLoading(true);
    projectsApi.getAll()
      .then(r => setProjects(r.data))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const handleCreate = async () => {
    if (!form.name.trim()) { setError('Project name is required.'); return; }
    setCreating(true);
    setError('');
    try {
      await projectsApi.create({ name: form.name, description: form.description, status: form.status });
      setShowModal(false);
      setForm({ name: '', description: '', status: ProjectStatus.Active });
      load();
    } catch {
      setError('Failed to create project.');
    } finally {
      setCreating(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this project and all its tasks?')) return;
    await projectsApi.delete(id);
    load();
  };

  const statusColors: Record<number, string> = { 0: 'badge-active', 1: 'badge-warning', 2: 'badge-success', 3: 'badge-muted' };

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Projects</h1>
          <p className="page-subtitle">Manage your projects and teams.</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={16} /> New Project
        </button>
      </div>

      {loading ? (
        <div className="loading-inline"><div className="spinner" /></div>
      ) : (
        <div className="project-grid">
          {projects.map(p => (
            <div key={p.id} className="project-card">
              <div className="project-card-header">
                <div className="project-icon"><FolderKanban size={20} /></div>
                <span className={`badge ${statusColors[p.status]}`}>{ProjectStatusLabel[p.status as ProjectStatus]}</span>
              </div>
              <Link to={`/projects/${p.id}`} className="project-name">{p.name}</Link>
              {p.description && <p className="project-desc">{p.description}</p>}
              <div className="project-meta">
                <span>{p.memberCount} members</span>
                <span>{p.taskCount} tasks</span>
              </div>
              <div className="project-footer">
                <span className="project-owner">by {p.ownerName}</span>
                <button className="icon-btn danger" onClick={() => handleDelete(p.id)} title="Delete">
                  <Trash2 size={15} />
                </button>
              </div>
            </div>
          ))}
          {projects.length === 0 && (
            <div className="empty-hero">
              <FolderKanban size={48} opacity={0.3} />
              <p>No projects yet. Create your first project!</p>
            </div>
          )}
        </div>
      )}

      {showModal && (
        <div className="modal-backdrop" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h2>New Project</h2>
              <button className="icon-btn" onClick={() => setShowModal(false)}><X size={18} /></button>
            </div>
            {error && <p className="alert alert-error">{error}</p>}
            <div className="form-group">
              <label>Project Name *</label>
              <input type="text" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} placeholder="e.g. Website Redesign" />
            </div>
            <div className="form-group">
              <label>Description</label>
              <textarea value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} placeholder="Optional description..." rows={3} />
            </div>
            <div className="modal-footer">
              <button className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancel</button>
              <button className="btn btn-primary" onClick={handleCreate} disabled={creating}>
                {creating ? 'Creating...' : 'Create Project'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
