import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, FolderKanban, Trash2, X, AlertCircle } from 'lucide-react';
import { projectsApi } from '../api/projectsApi';
import type { ProjectResponse } from '../types';
import { ProjectStatus, ProjectStatusLabel } from '../types';

// ─── Lookup tables ───────────────────────────────────────────────────────────

const STATUS_CLASS: Record<ProjectStatus, string> = {
  [ProjectStatus.Active]: 'badge-active',
  [ProjectStatus.OnHold]: 'badge-warning',
  [ProjectStatus.Completed]: 'badge-success',
  [ProjectStatus.Archived]: 'badge-muted',
};

// ─── Component ───────────────────────────────────────────────────────────────

export function ProjectsPage() {
  const [projects, setProjects] = useState<ProjectResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState('');

  // Create modal state
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState({ name: '', description: '', status: ProjectStatus.Active });
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState('');

  const load = () => {
    setLoading(true);
    setLoadError('');
    projectsApi
      .getAll()
      .then(r => setProjects(r.data))
      .catch(() => setLoadError('Failed to load projects. Please refresh.'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const handleCreate = async () => {
    if (!form.name.trim()) {
      setCreateError('Project name is required.');
      return;
    }

    setCreating(true);
    setCreateError('');

    try {
      await projectsApi.create({
        name: form.name.trim(),
        description: form.description.trim() || undefined,
        status: form.status,
      });
      setShowModal(false);
      setForm({ name: '', description: '', status: ProjectStatus.Active });
      load();
    } catch {
      setCreateError('Failed to create project. Please try again.');
    } finally {
      setCreating(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Delete "${name}" and all its tasks? This cannot be undone.`)) return;

    try {
      await projectsApi.delete(id);
      load();
    } catch {
      alert('Failed to delete project. You may not have permission.');
    }
  };

  const closeModal = () => {
    setShowModal(false);
    setCreateError('');
    setForm({ name: '', description: '', status: ProjectStatus.Active });
  };

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Projects</h1>
          <p className="page-subtitle">
            {projects.length === 0
              ? 'Create your first project to get started.'
              : `${projects.length} project${projects.length === 1 ? '' : 's'} — you own or are a member of.`}
          </p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={16} />
          New Project
        </button>
      </div>

      {/* ── Error banner ──────────────────────────────────────────────────── */}
      {loadError && (
        <div className="alert alert-error" style={{ marginBottom: 20 }}>
          <AlertCircle size={16} />
          {loadError}
        </div>
      )}

      {/* ── Project grid ──────────────────────────────────────────────────── */}
      {loading ? (
        <div className="loading-inline"><div className="spinner" /></div>
      ) : (
        <div className="project-grid">
          {projects.map(p => (
            <div key={p.id} className="project-card">
              <div className="project-card-header">
                <div className="project-icon"><FolderKanban size={20} /></div>
                <span className={`badge ${STATUS_CLASS[p.status as ProjectStatus]}`}>
                  {ProjectStatusLabel[p.status as ProjectStatus]}
                </span>
              </div>

              <Link to={`/projects/${p.id}`} className="project-name">{p.name}</Link>

              {p.description && (
                <p className="project-desc">{p.description}</p>
              )}

              <div className="project-meta">
                <span>{p.memberCount} member{p.memberCount !== 1 ? 's' : ''}</span>
                <span>{p.taskCount} task{p.taskCount !== 1 ? 's' : ''}</span>
              </div>

              <div className="project-footer">
                <span className="project-owner">by {p.ownerName}</span>
                <button
                  className="icon-btn danger"
                  onClick={() => handleDelete(p.id, p.name)}
                  title="Delete project"
                >
                  <Trash2 size={15} />
                </button>
              </div>
            </div>
          ))}

          {projects.length === 0 && !loadError && (
            <div className="empty-hero">
              <FolderKanban size={48} opacity={0.3} />
              <p>No projects yet. Hit "New Project" to get started.</p>
            </div>
          )}
        </div>
      )}

      {/* ── Create project modal ───────────────────────────────────────────── */}
      {showModal && (
        <div className="modal-backdrop" onClick={closeModal}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h2>New Project</h2>
              <button className="icon-btn" onClick={closeModal} aria-label="Close modal">
                <X size={18} />
              </button>
            </div>

            {createError && (
              <div className="alert alert-error">
                <AlertCircle size={14} />
                {createError}
              </div>
            )}

            <div className="form-group">
              <label htmlFor="proj-name">Project Name *</label>
              <input
                id="proj-name"
                type="text"
                value={form.name}
                onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                placeholder="e.g. Website Redesign"
                autoFocus
              />
            </div>

            <div className="form-group">
              <label htmlFor="proj-desc">Description</label>
              <textarea
                id="proj-desc"
                value={form.description}
                onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
                placeholder="What is this project about?"
                rows={3}
              />
            </div>

            <div className="modal-footer">
              <button className="btn btn-ghost" onClick={closeModal}>Cancel</button>
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
