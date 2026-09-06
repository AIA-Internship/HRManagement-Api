/* ============================================================
   MASTER PROJECT MODULE — JS
   Handles: Project List page + Edit Project page
   API: /api/timesheet/projects
   ============================================================ */

'use strict';

// ── Shared Utilities ─────────────────────────────────────────

function getAuthHeaders() {
    const token = localStorage.getItem('aia_jwt_token');
    const headers = { 'Content-Type': 'application/json' };
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    return headers;
}

function showMasterToast(message, type = 'success') {
    let toast = document.getElementById('master_toast');
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'master_toast';
        toast.className = 'master-toast';
        document.body.appendChild(toast);
    }
    toast.className = `master-toast ${type}`;
    toast.innerHTML = `<i class="bi bi-${type === 'success' ? 'check-circle-fill' : 'exclamation-triangle-fill'}"></i> ${message}`;
    toast.classList.add('show');
    clearTimeout(toast._timer);
    toast._timer = setTimeout(() => toast.classList.remove('show'), 3000);
}

// ── Confirmation Modal ───────────────────────────────────────

let _confirmCallback = null;

function showConfirmModal(message, onConfirm) {
    const backdrop = document.getElementById('confirm_modal_backdrop');
    const desc = document.getElementById('confirm_modal_desc');
    if (!backdrop || !desc) return;
    desc.textContent = message;
    _confirmCallback = onConfirm;
    backdrop.classList.add('active');
}

function closeConfirmModal() {
    const backdrop = document.getElementById('confirm_modal_backdrop');
    if (backdrop) backdrop.classList.remove('active');
    _confirmCallback = null;
}

function onConfirmModalConfirm() {
    if (typeof _confirmCallback === 'function') _confirmCallback();
    closeConfirmModal();
}

// ── PROJECT LIST PAGE ────────────────────────────────────────

async function initProjectList() {
    await loadProjectList();
}

async function loadProjectList() {
    const tbody = document.getElementById('project_list_tbody');
    const emptyState = document.getElementById('project_empty_state');
    const tableWrap = document.getElementById('project_table_wrap');
    if (!tbody) return;

    try {
        const res = await fetch('https://localhost:7089/api/timesheet/projects', { headers: getAuthHeaders() });
        if (res.status === 401) {
            console.error('401 Unauthorized fetching projects');
            // localStorage.removeItem('aia_jwt_token');
            // window.location.href = '/Account/Login';
            // return;
        }
        const data = await res.json();
        const projects = data?.content || data?.Content || data?.data || data || [];

        if (!Array.isArray(projects) || projects.length === 0) {
            tbody.innerHTML = '';
            if (emptyState) emptyState.style.display = 'block';
            return;
        }

        if (emptyState) emptyState.style.display = 'none';

        tbody.innerHTML = projects.map((p, i) => `
            <tr>
                <td class="row-num">${String(i + 1).padStart(2, '0')}</td>
                <td class="project-name-cell">${escapeHtml(p.name || p.projectName || '—')}</td>
                <td>${escapeHtml(p.description || '—')}</td>
                <td>
                    <span class="leader-badge">
                        <span class="leader-avatar">${getInitials(p.projectLeader || '?')}</span>
                        ${escapeHtml(p.projectLeader || '—')}
                    </span>
                </td>
            </tr>
        `).join('');


        // Update badge count
        const badge = document.getElementById('project_count_badge');
        if (badge) badge.textContent = projects.length + ' project' + (projects.length !== 1 ? 's' : '');

    } catch (err) {
        console.error('Failed to load projects:', err);
        renderProjectListDemo(tbody, tableWrap, emptyState);
    }
}

async function deleteProject(id, name) {
    showConfirmModal(`Are you sure you want to delete project "${name}"?`, async () => {
        try {
            const res = await fetch(`https://localhost:7089/api/timesheet/projects/${id}`, {
                method: 'DELETE',
                headers: getAuthHeaders()
            });

            if (res.ok) {
                await loadProjectList();
                showMasterToast('Project deleted successfully.', 'success');
            } else {
                showMasterToast('Failed to delete project.', 'error');
            }
        } catch (err) {
            console.error('Error deleting project:', err);
            showMasterToast('An error occurred.', 'error');
        }
    });
}

function renderProjectListDemo(tbody, tableWrap, emptyState) {
    const demo = [
        { id: 1, name: 'AIA Mobile App', projectLeader: 'Brandon Oei' },
        { id: 2, name: 'HR System Integration', projectLeader: 'Brandon Oei' },
        { id: 3, name: 'Data Analytics Dashboard', projectLeader: 'Brandon Oei' },
    ];
    if (tableWrap) tableWrap.style.display = '';
    if (emptyState) emptyState.style.display = 'none';
    tbody.innerHTML = demo.map((p, i) => `
        <tr>
            <td class="row-num">${String(i + 1).padStart(2, '0')}</td>
            <td class="project-name-cell">${escapeHtml(p.name)}</td>
            <td>${escapeHtml(p.description || '—')}</td>
            <td>
                <span class="leader-badge">
                    <span class="leader-avatar">${getInitials(p.projectLeader)}</span>
                    ${escapeHtml(p.projectLeader)}
                </span>
            </td>
        </tr>
    `).join('');

}


// ── EDIT PROJECT PAGE ────────────────────────────────────────

let _originalProjects = [];
let _projectRows = [];
let _rowCounter = 0;

async function initEditProject() {
    await loadProjectsForEdit();
    document.getElementById('btn_add_project')?.addEventListener('click', addProjectRow);
    document.getElementById('btn_update_project')?.addEventListener('click', () => {
        if (!validateProjectRows()) return;
        showConfirmModal('Are you sure you want to update the projects?', submitProjectUpdate);
    });
    document.getElementById('btn_discard_changes')?.addEventListener('click', () => {
        showConfirmModal('Are you sure you want to discard your changes? Any unsaved changes will be lost.', () => {
            window.location.href = '/Timesheet/Supervisor/Projects';
        });
    });
    document.getElementById('confirm_modal_cancel')?.addEventListener('click', closeConfirmModal);
    document.getElementById('confirm_modal_confirm')?.addEventListener('click', onConfirmModalConfirm);
}

async function loadProjectsForEdit() {
    try {
        const res = await fetch('https://localhost:7089/api/timesheet/projects', { headers: getAuthHeaders() });
        if (res.status === 401) {
            console.error('401 Unauthorized fetching projects');
            // localStorage.removeItem('aia_jwt_token');
            // window.location.href = '/Account/Login';
            // return;
        }
        const data = await res.json();
        _originalProjects = data?.content || data?.Content || data?.data || data || [];
    } catch (err) {
        console.error('API unavailable, using demo data');
        _originalProjects = [
            { id: 1, name: 'AIA Mobile App', projectLeader: 'Brandon Oei' },
            { id: 2, name: 'HR System Integration', projectLeader: 'Brandon Oei' },
        ];
    }

    const container = document.getElementById('project_rows_container');
    if (!container) return;

    container.innerHTML = '';
    _projectRows = [];

    if (_originalProjects.length > 0) {
        _originalProjects.forEach(p => addProjectRow(null, {
            id: p.id,
            projectName: p.name || p.projectName || '',
            description: p.description || '',
            projectLeader: p.projectLeader || ''
        }));
    } else {
        // If no projects found, auto-add one empty row for convenience
        addProjectRow();
    }
}

function addProjectRow(e, data = null) {
    _rowCounter++;
    const id = `row_${_rowCounter}`;
    const container = document.getElementById('project_rows_container');
    if (!container) return;

    const isNew = !data;
    // checkEditEmptyState(true); // removed as requested

    const tr = document.createElement('tr');
    tr.className = 'edit-project-row';
    tr.dataset.rowId = id;
    tr.innerHTML = `
        <td class="row-num edit-row-num" style="vertical-align:middle;"></td>
        <td>
            <input
                type="text"
                class="edit-row-input"
                id="name_${id}"
                name="name_${id}"
                autocomplete="off"
                placeholder="Project Name"
                value="${data ? escapeHtml(data.projectName || '') : ''}"
            />
        </td>
        <td>
            <input
                type="text"
                class="edit-row-input"
                id="appused_${id}"
                name="appused_${id}"
                autocomplete="off"
                placeholder="App Used (e.g. Jira)"
                value="${data ? escapeHtml(data.description || '') : ''}"
            />
        </td>
        <td>
            <input
                type="text"
                class="edit-row-input"
                id="leader_${id}"
                name="leader_${id}"
                autocomplete="off"
                placeholder="Project Lead"
                value="${data ? escapeHtml(data.projectLeader || '') : ''}"
            />
        </td>
        <td style="text-align:center;">
            <div class="d-flex align-items-center justify-content-center gap-2">
                <button class="btn btn-icon btn-light-danger btn-sm rounded-circle h-35px w-35px" title="Remove row" onclick="removeProjectRow('${id}')">
                    <i class="bi bi-trash-fill"></i>
                </button>
            </div>
        </td>
    `;
    container.appendChild(tr);
    _projectRows.push({ id, dataId: data?.id || null });
    reindexRows();

    if (isNew) {
        setTimeout(() => document.getElementById(`name_${id}`)?.focus(), 50);
    }
}

function removeProjectRow(rowId) {

    const row = document.querySelector(`[data-row-id="${rowId}"]`);
    if (row) {
        row.style.opacity = '0';
        row.style.transform = 'translateX(12px)';
        row.style.transition = 'all 0.2s';
        setTimeout(() => {
            row.remove();
            _projectRows = _projectRows.filter(r => r.id !== rowId);
            reindexRows();
            checkEditEmptyState();
        }, 200);
    }
}

function reindexRows() {
    const rows = document.querySelectorAll('#project_rows_container .edit-project-row');
    rows.forEach((row, i) => {
        const numEl = row.querySelector('.edit-row-num');
        if (numEl) numEl.textContent = String(i + 1).padStart(2, '0');
    });
}

function checkEditEmptyState(isAdding = false) {
    // This function is now disabled in Edit mode as requested.
    // The user prefers an empty row instead of a descriptive empty state.
    return;
}

function validateProjectRows() {
    let isValid = true;
    const rows = document.querySelectorAll('#project_rows_container .edit-project-row');

    if (rows.length === 0) {
        showMasterToast('Please add at least one project.', 'error');
        return false;
    }

    rows.forEach(row => {
        const nameInput = row.querySelector('input[id^="name_"]');
        const leaderInput = row.querySelector('input[id^="leader_"]');
        [nameInput, leaderInput].forEach(input => {
            if (input && !input.value.trim()) {
                input.classList.add('is-invalid');
                input.addEventListener('input', () => input.classList.remove('is-invalid'), { once: true });
                isValid = false;
            }
        });
    });

    if (!isValid) showMasterToast('Please fill in all required fields.', 'error');
    return isValid;
}

async function submitProjectUpdate() {
    const rows = document.querySelectorAll('#project_rows_container .edit-project-row');
    const projects = [];

    rows.forEach((row, i) => {
        const rowId = row.dataset.rowId;
        const matchedMeta = _projectRows.find(r => r.id === rowId);
        const nameInput = document.getElementById(`name_${rowId}`);
        const appusedInput = document.getElementById(`appused_${rowId}`);
        const leaderInput = document.getElementById(`leader_${rowId}`);
        if (nameInput && leaderInput) {
            projects.push({
                id: matchedMeta?.dataId || null,
                projectName: nameInput.value.trim(),
                description: appusedInput ? appusedInput.value.trim() : '',
                projectLeader: leaderInput.value.trim(),
                sortOrder: i + 1
            });
        }
    });

    // Show loading state on button
    const btn = document.getElementById('btn_update_project');
    if (btn) { btn.disabled = true; btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Saving...'; }

    console.log("Submitting projects:", projects);
    try {
        const res = await fetch('https://localhost:7089/api/timesheet/projects/bulk', {
            method: 'PUT',
            headers: getAuthHeaders(),
            body: JSON.stringify({ projects })
        });

        if (res.status === 401) {
            console.error('401 Unauthorized fetching projects');
            // localStorage.removeItem('aia_jwt_token');
            // window.location.href = '/Account/Login';
            // return;
        }

        let json;
        const text = await res.text();
        try { json = text ? JSON.parse(text) : {}; } catch (e) { json = { message: text || res.statusText }; }

        if (!res.ok || json?.isError) {
            throw new Error(json?.message || json?.statusMessage || json?.title || 'Server error');
        }

        showMasterToast('Projects updated successfully!', 'success');
        setTimeout(() => window.location.href = '/Timesheet/Supervisor/Projects', 1200);
    } catch (err) {
        console.error('Submit failed:', err);
        showMasterToast(err.message || 'Failed to update. Please try again.', 'error');
        if (btn) { btn.disabled = false; btn.innerHTML = '<i class="bi bi-check-circle"></i> Update Project'; }
    }
}

// ── Helpers ──────────────────────────────────────────────────

function escapeHtml(str) {
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

function getInitials(name) {
    return String(name).split(' ').filter(Boolean).slice(0, 2).map(w => w[0].toUpperCase()).join('');
}
