/**
 * Approval Management JS
 * Handles fetching, filtering and displaying submission reports for supervisors.
 * UPDATED: Supervisor can review/approve anytime.
 */

'use strict';

const _indonesianMonths = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

async function loadApprovalData() {
    const tbody = document.getElementById('approval_tbody');
    const emptyState = document.getElementById('approval_empty_state');
    const filterMonth = document.getElementById('filter_month').value;
    const filterYear = document.getElementById('filter_year').value;
    const filterStatus = document.getElementById('filter_status').value;

    if (!tbody) return;
    
    tbody.innerHTML = `<tr><td colspan="7" class="text-center py-20">
        <div class="spinner-border text-primary" role="status"></div>
        <div class="mt-4 fw-boldest text-gray-400">Loading data...</div>
    </td></tr>`;
    if (emptyState) emptyState.style.display = 'none';

    try {
        const data = await fetchAPI('timesheet/supervisor/report');
        if (!data) throw new Error('No data received');

        // Unified list logic
        let unifiedList = [];
        
        // 1. Process History / Submissions
        if (data.approvalHistory) {
            unifiedList = unifiedList.concat(data.approvalHistory.map(h => ({ 
                ...h, 
                unifiedStatus: h.status === 'Approved' ? 'Approved' : 'Needs Approval',
                originalStatus: h.status,
                isMissing: false
            })));
        }

        // 2. Process Missing (Interns who haven't submitted yet)
        if (data.missingSubmissions) {
            data.missingSubmissions.forEach(m => {
                // Check if already in history (to avoid duplicates if filter is broad)
                const exists = unifiedList.find(x => x.employeeId === m.employeeId && x.month === m.month && x.year === m.year);
                if (!exists) {
                    unifiedList.push({ 
                        employeeId: m.employeeId, 
                        employeeName: m.employeeName, 
                        month: m.month, 
                        year: m.year, 
                        unifiedStatus: 'Needs Approval', 
                        originalStatus: 'Not Submitted',
                        submissionId: 0, 
                        reviewedDate: '--', 
                        revisionNote: '',
                        isMissing: true
                    });
                }
            });
        }

        // Apply Local Filters
        let filtered = unifiedList.filter(h => {
            const matchMonth = h.month == filterMonth;
            const matchYear = h.year == filterYear;
            const matchStatus = !filterStatus || h.unifiedStatus === filterStatus;
            return matchMonth && matchYear && matchStatus;
        });

        // Sort: Needs Approval first, then by name
        filtered.sort((a, b) => {
            if (a.unifiedStatus === 'Needs Approval' && b.unifiedStatus !== 'Needs Approval') return -1;
            if (a.unifiedStatus !== 'Needs Approval' && b.unifiedStatus === 'Needs Approval') return 1;
            return a.employeeName.localeCompare(b.employeeName);
        });

        // Update Stats
        document.getElementById('stat_pending').textContent = filtered.filter(x => x.unifiedStatus === 'Needs Approval').length;
        document.getElementById('stat_approved').textContent = filtered.filter(x => x.unifiedStatus === 'Approved').length;
        document.getElementById('stat_revision').textContent = filtered.filter(x => x.originalStatus.includes('Revision')).length;

        if (filtered.length === 0) {
            tbody.innerHTML = '';
            if (emptyState) emptyState.style.display = 'block';
            return;
        }

        tbody.innerHTML = filtered.map(h => {
            const isApproved = h.unifiedStatus === 'Approved';
            const hasRevision = h.originalStatus.includes('Revision') || h.originalStatus.includes('Need');
            
            const statusClass = isApproved ? 'badge-pill-approved' : 'badge-pill-waiting';
            const initials = h.employeeName.split(' ').map(x => x[0]).join('').substring(0, 2).toUpperCase();
            
            // Link logic: if no submissionId, we pass employeeId/month/year
            const reviewUrl = h.submissionId > 0 
                ? `/Timesheet/Supervisor/Review?id=${h.submissionId}` 
                : `/Timesheet/Supervisor/Review?employeeId=${h.employeeId}&month=${h.month}&year=${h.year}`;

            return `
                <tr onclick="window.location.href='${reviewUrl}'">
                    <td>
                        <div class="employee-info">
                            <div class="employee-avatar">${initials}</div>
                            <div>
                                <div class="employee-name">${h.employeeName}</div>
                                <div class="employee-id">ID: ${h.employeeId}</div>
                            </div>
                        </div>
                    </td>
                    <td>
                        <div class="period-text">${_indonesianMonths[h.month-1]} ${h.year}</div>
                    </td>
                    <td>
                        <span class="badge-pill-status ${statusClass}">${h.unifiedStatus.toUpperCase()}</span>
                    </td>
                    <td class="text-center">
                        ${isApproved ? '<i class="bi bi-check-circle-fill text-success fs-4"></i>' : '<i class="bi bi-dash-circle text-gray-300 fs-4"></i>'}
                    </td>
                    <td class="text-center">
                        ${hasRevision ? '<i class="bi bi-arrow-repeat text-warning fs-4" title="Revision Requested"></i>' : '<i class="bi bi-dash-circle text-gray-300 fs-4"></i>'}
                    </td>
                    <td>
                        <div class="decision-date">${h.reviewedDate || '--'}</div>
                    </td>
                    <td class="text-end">
                        <a href="${reviewUrl}" class="btn-standard-info btn-standard-sm fw-boldest">
                            Review
                        </a>
                    </td>

                </tr>
            `;
        }).join('');

    } catch (err) {
        console.error('Failed to load approval data:', err);
        tbody.innerHTML = `<tr><td colspan="7" class="text-center py-20 text-danger fw-bold">Error loading data.</td></tr>`;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const now = new Date();
    const m = document.getElementById('filter_month');
    const y = document.getElementById('filter_year');
    if (m) m.value = now.getMonth() + 1;
    if (y) y.value = now.getFullYear();

    loadApprovalData();

    ['filter_month', 'filter_year', 'filter_status'].forEach(id => {
        document.getElementById(id)?.addEventListener('change', loadApprovalData);
    });
});
