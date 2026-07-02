const API_BASE = "https://localhost:7089";

async function apiGet(endpoint) {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    if (!token) { window.aiaAuth && window.aiaAuth.signOut(); return null; }
    try {
        const res = await fetch(`${API_BASE}${endpoint}`, { headers: { 'Authorization': `Bearer ${token}` } });
        if (res.status === 401) { window.aiaAuth.signOut(); return null; }
        const json = await res.json();
        return json.content || json.data || json;
    } catch (err) {
        console.error("API GET failed:", err);
        return null;
    }
}

function setText(id, val) {
    const el = document.getElementById(id);
    if (el) el.textContent = (val === undefined || val === null || val === "") ? "-" : val;
}

function setProgress(paid, unpaid) {
    const total = (paid || 0) + (unpaid || 0);
    const pct = v => (total === 0 ? 0 : Math.round((v / total) * 100));
    const paidBar = document.getElementById('paidBar');
    const unpaidBar = document.getElementById('unpaidBar');
    if (paidBar) paidBar.style.width = pct(paid) + '%';
    if (unpaidBar) unpaidBar.style.width = pct(unpaid) + '%';
}

function renderRows(items) {
    const tableBody = document.getElementById('leaveTableBody');
    if (!tableBody) return;
    if (!items || items.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No leave requests found.</td></tr>';
        return;
    }

    tableBody.innerHTML = items.map(i => {
        const status = i.status;
        const badge = (function (status) {
            if (status === undefined || status === null) {
                return `<div class="d-flex justify-content-center"><span class="d-inline-flex align-items-center justify-content-center gap-1 text-center" style="border-radius:2rem; height:2.1rem; width:10.5rem; color:#B77938; background-color:#FEF3C7"><i class="bi bi-clock" style="color:#B77938"></i><strong>Needs Approval</strong></span></div>`;
            }
            const n = Number(status);
            if (!Number.isNaN(n)) {
                switch (n) {
                    case 2:
                        return `<div class="d-flex justify-content-center"><span class="d-inline-flex align-items-center justify-content-center gap-1 text-center" style="border-radius:2rem; height:2.1rem; width:8rem; color:#065F46; background-color:#D1FAE5"><i class="bi bi-check-circle" style="color:#2B8B15"></i><strong>Approved</strong></span></div>`;
                    case 3:
                        return `<div class="d-flex justify-content-center"><span class="d-inline-flex align-items-center justify-content-center gap-1 text-center" style="border-radius:2rem; height:2.1rem; width:8rem; color:#D31145; background-color:#FFE4E6"><i class="bi bi-x-circle" style="color:#D31145"></i><strong>Rejected</strong></span></div>`;
                    case 1:
                    default:
                        return `<div class="d-flex justify-content-center"><span class="d-inline-flex align-items-center justify-content-center gap-1 text-center" style="border-radius:2rem; height:2.1rem; width:10.5rem; color:#B77938; background-color:#FEF3C7"><i class="bi bi-clock" style="color:#B77938"></i><strong>Needs Approval</strong></span></div>`;
                }
            }
            const s = String(status).toLowerCase();
            if (s.includes('approve')) return `<div class="d-flex justify-content-center"><span class="d-inline-flex align-items-center justify-content-center gap-1 text-center" style="border-radius:2rem; height:2.1rem; width:8rem; color:#065F46; background-color:#D1FAE5"><i class="bi bi-check-circle" style="color:#2B8B15"></i><strong>Approved</strong></span></div>`;
            if (s.includes('reject')) return `<div class="d-flex justify-content-center"><span class="d-inline-flex align-items-center justify-content-center gap-1 text-center" style="border-radius:2rem; height:2.1rem; width:8rem; color:#D31145; background-color:#FFE4E6"><i class="bi bi-x-circle" style="color:#D31145"></i><strong>Rejected</strong></span></div>`;
            if (s.includes('need') || s.includes('approval')) return `<div class="d-flex justify-content-center"><span class="d-inline-flex align-items-center justify-content-center gap-1 text-center" style="border-radius:2rem; height:2.1rem; width:10.5rem; color:#B77938; background-color:#FEF3C7"><i class="bi bi-clock" style="color:#B77938"></i><strong>Needs Approval</strong></span></div>`;
            return `<div class="d-flex justify-content-center"><span class="d-inline-flex align-items-center justify-content-center gap-1 text-center" style="border-radius:2rem; height:2.1rem; width:auto; color:#fff; background-color:#6c757d"><strong>${status}</strong></span></div>`;
        })(status);

        const leaveTypeText = (function (type) {
            const t = (type || '').toString();
            if (t === '1') return 'Paid';
            if (t === '2') return 'Unpaid';
            return type || '';
        })(i.leaveType);

        return `
            <tr>
                <td>${leaveTypeText}</td>
                <td>${i.startDate}</td>
                <td>${i.endDate}</td>
                <td>${i.duration}</td>
                <td>${badge}</td>
            </tr>`;
    }).join('');
}

function renderPagination(meta) {
    const paginationList = document.getElementById('paginationList');
    if (!paginationList) return;
    const { currentPage, totalPages } = meta;
    const prevDisabled = currentPage <= 1;
    const nextDisabled = currentPage >= totalPages;

    const maxButtons = 3;
    let startPage = Math.max(1, currentPage - Math.floor(maxButtons / 2));
    let endPage = startPage + maxButtons - 1;
    if (endPage > totalPages) {
        endPage = totalPages;
        startPage = Math.max(1, endPage - maxButtons + 1);
    }

    let html = '';
    html += `<li class="${prevDisabled ? 'page-item disabled' : 'page-item'}"><a class="page-link" data-page="${prevDisabled ? currentPage : currentPage-1}" href="#" aria-label="Previous">&laquo;</a></li>`;

    for (let i = startPage; i <= endPage; i++) {
        html += `<li class="${i === currentPage ? 'page-item active' : 'page-item'}"><a class="page-link" data-page="${i}" href="#">${i}</a></li>`;
    }

    html += `<li class="${nextDisabled ? 'page-item disabled' : 'page-item'}"><a class="page-link" data-page="${nextDisabled ? currentPage : currentPage+1}" href="#" aria-label="Next">&raquo;</a></li>`;

    paginationList.innerHTML = html;
}

async function loadAllAndRender({ page = 1, sort = 'newest', statusOrder = '1,2,3', filterStatus = null } = {}) {
    const tableBody = document.getElementById('leaveTableBody');
    const showingText = document.getElementById('showingText');
    const sortSelect = document.getElementById('sortSelect');
    if (sortSelect) sortSelect.value = sort;

    try {
        const lb = await apiGet('/api/leave/get-leave-balance');
        if (lb && lb.leaveBalance !== undefined) setText('leaveBalance', lb.leaveBalance);

        const lt = await apiGet('/api/leave/get-all-amount-type');
        const paid = lt?.paidLeave ?? lt?.PaidLeave ?? 0;
        const unpaid = lt?.unpaidLeave ?? lt?.UnpaidLeave ?? 0;
        setText('paidCount', paid);
        setText('unpaidCount', unpaid);
        setProgress(paid, unpaid);

        const list = await apiGet('/api/leave/get-by-requester-id?max=1000');
        const items = Array.isArray(list) ? list : (list?.items || list?.content || []);

        // apply status filter (filterStatus: '1'|'2'|'3' or null)
        const filteredItems = (items || []).filter(x => {
            if (!filterStatus) return true;
            const s = (x.leaveStatus || x.status || x.LeaveStatus || '').toString().toLowerCase().trim();
            if (!s) return false;
            if (filterStatus === '1') return s === '1' || s.includes('need') || s.includes('pending');
            if (filterStatus === '2') return s === '2' || s.includes('approve');
            if (filterStatus === '3') return s === '3' || s.includes('reject');
            return false;
        });

        // client-side sorting and filtering
        const statusPriority = (statusOrder || '1,2,3').split(',').map(s => s.trim());
        const getPriority = (lr) => {
            const s = (lr.leaveStatus || lr.status || '').toString();
            const normalized = (s || '').toLowerCase().trim();
            if (normalized === '1' || normalized.includes('need') || normalized.includes('pending')) return 0;
            if (normalized === '2' || normalized.includes('approve')) return 1;
            if (normalized === '3' || normalized.includes('reject')) return 2;
            return 99;
        };

        const sorted = (filteredItems || []).slice().sort((a, b) => {
            const pa = getPriority(a);
            const pb = getPriority(b);
            if (pa !== pb) return pa - pb;
            const getCreatedTime = (x) => {
                const d = x && (x.createdUtcDate || x.CreatedUtcDate || x.created_at || x.createdAt || x.CreatedAt || x.leaveStartDate || x.startDate);
                if (!d) return 0;
                const t = Date.parse(d);
                return Number.isFinite(t) ? t : 0;
            };
            const da = getCreatedTime(a);
            const db = getCreatedTime(b);
            return (sort === 'oldest') ? da - db : db - da;
        });

        const totalItems = sorted.length;
        const pageSize = 5;
        const totalPages = Math.max(1, Math.ceil(totalItems / pageSize));
        const currentPage = Math.min(Math.max(1, page), totalPages);
        const startIdx = (currentPage - 1) * pageSize;
        const paged = sorted.slice(startIdx, startIdx + pageSize).map(x => ({
            leaveType: x.leaveType || x.leaveType,
            startDate: x.startDate || (x.leaveStartDate ? new Date(x.leaveStartDate).toLocaleDateString(undefined, { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }) : ''),
            endDate: x.endDate || (x.endDate ? new Date(x.endDate).toLocaleDateString(undefined, { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }) : (x.endDate || '')),
            duration: x.duration || x.dayAmount || x.DayAmount || '',
            status: x.status || x.leaveStatus || x.LeaveStatus
        }));

        renderRows(paged);
        renderPagination({ currentPage, totalPages });
        if (showingText) showingText.textContent = `Showing ${totalItems === 0 ? 0 : startIdx + 1}-${startIdx + paged.length} of ${totalItems} requests`;

    } catch (err) {
        console.error('Failed to load dashboard data', err);
        if (tableBody) tableBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Failed to load data.</td></tr>';
    }
}

document.addEventListener('DOMContentLoaded', function () {
    // remove old popover element if present
    const popover = document.getElementById('popover_monthly');
    if (popover) popover.remove();

    // wire up filter buttons and pagination delegations
    let selectedStatus = null; // default: show all statuses
    const filterButtons = Array.from(document.querySelectorAll('.btn-group[aria-label="filters"] button'));
    if (filterButtons.length) {
        const map = { 0: '1', 1: '2', 2: '3' };
        // clear any server-side "active" so default is "all"
        filterButtons.forEach(b => b.classList.remove('active'));

        filterButtons.forEach((btn, idx) => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                const wasActive = btn.classList.contains('active');
                // clear all
                filterButtons.forEach(b => b.classList.remove('active'));
                if (wasActive) {
                    // toggle off -> show all
                    selectedStatus = null;
                } else {
                    // activate clicked -> show only that status
                    btn.classList.add('active');
                    selectedStatus = map[idx] || null;
                }
                loadAllAndRender({ page: 1, sort: document.getElementById('sortSelect')?.value || 'newest', filterStatus: selectedStatus });
            });
        });
    }

    document.getElementById('paginationList')?.addEventListener('click', function (e) {
        const a = e.target.closest ? e.target.closest('a.page-link') : null;
        if (!a) return;
        e.preventDefault();
        const page = parseInt(a.getAttribute('data-page'), 10) || 1;
        loadAllAndRender({ page, sort: document.getElementById('sortSelect')?.value || 'newest', filterStatus: selectedStatus });
    });

    document.getElementById('sortSelect')?.addEventListener('change', function () { loadAllAndRender({ page: 1, sort: this.value, filterStatus: selectedStatus }); });

    // initial load (show all by default)
    loadAllAndRender({ page: 1, filterStatus: selectedStatus });
});
