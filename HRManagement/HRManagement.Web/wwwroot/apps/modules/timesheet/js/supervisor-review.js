/**
 * Supervisor Review Page JS
 * Per-day checkbox approval logic:
 * - Approved day      → checkbox checked + disabled
 * - Has remark        → checkbox unchecked + disabled (empty)
 * - Need Revision (never revised) → status tag, checkbox disabled (empty)
 * - Checked day       → remark disabled
 * - 2nd revision      → can edit remark
 */

'use strict';

const REVIEW_MONTHS = ["January","February","March","April","May","June","July","August","September","October","November","December"];

async function initReviewPage() {
    const urlParams = new URLSearchParams(window.location.search);
    const submissionId = urlParams.get('id');
    const employeeId   = urlParams.get('employeeId');
    const month        = urlParams.get('month');
    const year         = urlParams.get('year');

    let endpoint = '';
    if (submissionId && parseInt(submissionId) > 0) {
        endpoint = `timesheet/supervisor/review/${submissionId}`;
    } else if (employeeId && month && year) {
        endpoint = `timesheet/supervisor/review/anytime?employeeId=${employeeId}&month=${month}&year=${year}`;
    } else {
        window.location.href = '/Timesheet/Supervisor/Dashboard';
        return;
    }

    try {
        const data = await reviewFetchAPI(endpoint);
        if (!data) return;

        renderReviewHeader(data);
        renderReviewTable(data.days || []);
        renderFeedbackSection(data);
        window.currentReviewData = data;
    } catch (err) {
        console.error("Review Page Init Error:", err);
    }
}

// ── Header ───────────────────────────────────────────────────────────────────
function renderReviewHeader(data) {
    const nameEl   = document.getElementById('employee_name');
    const periodEl = document.getElementById('review_period_label');
    const statusEl = document.getElementById('review_status_badge');
    const avatarEl = document.getElementById('employee_avatar');
    const subEl    = document.getElementById('submitted_at');

    if (nameEl)   nameEl.innerText   = data.employeeName || '—';
    if (avatarEl) avatarEl.innerText = (data.employeeName || '?').charAt(0).toUpperCase();
    if (periodEl) periodEl.innerText = `${REVIEW_MONTHS[(data.month || 1) - 1]} ${data.year}`;
    if (subEl)    subEl.innerText    = data.submittedDate || '--';

    if (statusEl) {
        const s = (data.status || 'Needs Approval');
        const isApproved  = s === 'Approved';
        const isRevision  = s.toLowerCase().includes('revision');
        statusEl.innerText  = s.toUpperCase();
        statusEl.className  = `badge-pill-status ${isApproved ? 'badge-pill-approved' : isRevision ? 'badge-pill-rejected' : 'badge-pill-waiting'}`;
    }
}

// ── Table ─────────────────────────────────────────────────────────────────────
function renderReviewTable(days) {
    const tbody = document.getElementById('review_tbody');
    if (!tbody) return;
    tbody.innerHTML = '';

    if (!days || days.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center py-20 text-muted fw-bold">No logs found for this period.</td></tr>';
        return;
    }

    days.forEach((d, idx) => {
        const dateObj  = new Date(d.date);
        const dayName  = dateObj.toLocaleDateString('en-US', { weekday: 'short' }).toUpperCase();
        const dateNum  = dateObj.getDate();
        const isWeekend = dateObj.getDay() === 0 || dateObj.getDay() === 6;

        // ── Determine state ──
        const isApproved      = d.dayStatus === 'Approved';
        const hasRemark       = !!(d.remark && d.remark.trim());
        const needsRevision   = d.dayStatus === 'NeedsRevision';
        const canEditRemark   = d.revisionCount > 1; // 2nd+ revision: allow editing comment

        // Checkbox rules
        let cbChecked   = isApproved;
        let cbDisabled  = isApproved || (hasRemark && !isApproved) || (needsRevision && !canEditRemark);

        // Remark rules
        let remarkDisabled = cbChecked;
        let remarkValue    = d.remark || '';
        let remarkReadonly = cbChecked || (needsRevision && !canEditRemark && !hasRemark);

        // Row CSS
        let rowClass = isWeekend ? 'row-weekend' : isApproved ? 'row-approved' : needsRevision ? 'row-revision' : '';

        // Activities
        let activitiesHTML = '--';
        if (d.projectMinutes && Object.keys(d.projectMinutes).length > 0) {
            activitiesHTML = Object.entries(d.projectMinutes).map(([name, mins]) => {
                const hrs = (mins / 60).toFixed(1);
                return `<span class="review-proj-tag fw-boldest">${name}: <span class="text-dark">${hrs}h</span></span>`;
            }).join('');
        }

        const totalHrs = d.totalMinutes ? (d.totalMinutes / 60).toFixed(1) : '0.0';

        // Status overlay badge
        let statusTag = '';
        if (isApproved)    statusTag = '<span class="badge badge-light-success ms-2 fw-bold fs-9">Approved</span>';
        if (needsRevision) statusTag = '<span class="badge badge-light-danger ms-2 fw-bold fs-9">Needs Revision</span>';

        tbody.innerHTML += `
            <tr class="${rowClass}" data-day-idx="${idx}" data-date="${d.date}">
                <td class="col-check text-center">
                    ${isWeekend ? '<span class="text-gray-300 fs-8">—</span>' : `
                    <input type="checkbox" class="day-check day-cb"
                        data-idx="${idx}"
                        ${cbChecked  ? 'checked'  : ''}
                        ${cbDisabled ? 'disabled' : ''}
                        onchange="onDayCbChange(this)">`}
                </td>
                <td class="col-day text-center">
                    <div class="review-date-box">
                        <span class="review-day-name">${dayName}</span>
                        <span class="review-date-val">${dateNum}</span>
                    </div>
                </td>
                <td>
                    <div class="d-flex flex-wrap gap-2 align-items-center">
                        ${activitiesHTML}
                        ${statusTag}
                    </div>
                </td>
                <td class="col-dur text-center fw-boldest text-dark">${totalHrs}h</td>
                <td class="col-remark">
                    ${isWeekend ? '' : `
                    <input type="text" class="remark-input day-remark"
                        data-idx="${idx}"
                        value="${remarkValue.replace(/"/g, '&quot;')}"
                        placeholder="${needsRevision && !canEditRemark ? 'Revision requested' : 'Add remark...'}"
                        ${remarkDisabled ? 'disabled' : ''}
                        ${remarkReadonly ? 'readonly' : ''}>`}
                </td>
            </tr>
        `;
    });

    updateSelectedCount();
}

function renderFeedbackSection(data) {
    const noteArea = document.getElementById('overall_revision_note');
    if (noteArea && data.revisionNote) noteArea.value = data.revisionNote;
}

// ── Checkbox interaction ──────────────────────────────────────────────────────
function onDayCbChange(cb) {
    const idx         = cb.dataset.idx;
    const remarkInput = document.querySelector(`.day-remark[data-idx="${idx}"]`);

    if (remarkInput) {
        remarkInput.disabled = cb.checked;
        if (cb.checked) remarkInput.value = '';
    }

    updateSelectedCount();
    syncSelectAll();
}

function toggleSelectAll(masterCb) {
    document.querySelectorAll('.day-cb:not(:disabled)').forEach(cb => {
        cb.checked = masterCb.checked;
        onDayCbChange(cb);
    });
    updateSelectedCount();
}

function syncSelectAll() {
    const all       = document.querySelectorAll('.day-cb:not(:disabled)');
    const allChecked = [...all].every(cb => cb.checked);
    const masterCb  = document.getElementById('select_all_cb');
    if (masterCb) masterCb.checked = allChecked;
}

function updateSelectedCount() {
    const count = document.querySelectorAll('.day-cb:checked:not(:disabled)').length;
    const label = document.getElementById('selected_count_label');
    if (label) label.textContent = `${count} selected`;
}

// ── Bulk Actions ─────────────────────────────────────────────────────────────
async function bulkApproveSelected() {
    const selectedDates = getSelectedDayDates();
    if (selectedDates.length === 0) {
        showToast('Please select at least one day to approve.', 'warning');
        return;
    }
    const data = window.currentReviewData;
    if (!data) return;

    const { isConfirmed } = await Swal.fire({
        title: `Approve ${selectedDates.length} day(s)?`,
        text: `These selected days will be marked as approved.`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Yes, Approve',
        confirmButtonColor: '#059669'
    });

    if (!isConfirmed) return;

    const payload = {
        submissionId: data.submissionId,
        employeeId:   data.employeeId,
        month:        data.month,
        year:         data.year,
        approvedDates: selectedDates
    };

    const res = await reviewFetchAPI('timesheet/supervisor/approve-days', 'POST', payload);
    if (res) {
        showToast(`${selectedDates.length} day(s) approved!`, 'success');
        setTimeout(() => window.location.reload(), 1200);
    }
}

async function bulkRevisionSelected() {
    const selectedDates = getSelectedDayDates();
    if (selectedDates.length === 0) {
        showToast('Please select at least one day to request revision.', 'warning');
        return;
    }
    const data = window.currentReviewData;
    if (!data) return;

    // Collect remarks for selected days
    const dayRemarks = [];
    selectedDates.forEach(date => {
        const row    = document.querySelector(`tr[data-date="${date}"]`);
        const remark = row ? (row.querySelector('.day-remark')?.value || '') : '';
        dayRemarks.push({ date, remark });
    });

    const { isConfirmed } = await Swal.fire({
        title: `Request Revision for ${selectedDates.length} day(s)?`,
        text: 'Selected days will be sent back for revision.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Request',
        confirmButtonColor: '#D31145'
    });

    if (!isConfirmed) return;

    const payload = {
        submissionId: data.submissionId,
        employeeId:   data.employeeId,
        month:        data.month,
        year:         data.year,
        dayRemarks:   dayRemarks
    };

    const res = await reviewFetchAPI('timesheet/supervisor/revision-days', 'POST', payload);
    if (res) {
        showToast('Revision requested for selected days.', 'info');
        setTimeout(() => window.location.reload(), 1200);
    }
}

function getSelectedDayDates() {
    return [...document.querySelectorAll('.day-cb:checked:not(:disabled)')]
        .map(cb => {
            const row = cb.closest('tr');
            return row ? row.dataset.date : null;
        })
        .filter(Boolean);
}

// ── Full Approve / Revision ───────────────────────────────────────────────────
async function approveTimesheet() {
    const data = window.currentReviewData;
    if (!data) return;

    const { isConfirmed } = await Swal.fire({
        title: 'Approve Full Timesheet?',
        text: `Approve ${data.employeeName}'s entire timesheet for ${document.getElementById('review_period_label')?.innerText}?`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Confirm',
        confirmButtonColor: '#059669'
    });

    if (!isConfirmed) return;

    const payload = {
        submissionId: data.submissionId,
        employeeId:   data.employeeId,
        month:        data.month,
        year:         data.year
    };

    const res = await reviewFetchAPI('timesheet/supervisor/approve', 'POST', payload);
    if (res) {
        await Swal.fire('Approved!', 'Timesheet has been approved.', 'success');
        window.location.href = '/Timesheet/Supervisor/Report';
    }
}

async function giveRevision() {
    const data     = window.currentReviewData;
    const feedback = document.getElementById('overall_revision_note')?.value || '';

    if (!feedback.trim() || feedback.trim().length < 5) {
        Swal.fire('Note Required', 'Please provide a clear revision note (min 5 characters).', 'warning');
        return;
    }

    const { isConfirmed } = await Swal.fire({
        title: 'Request Revision?',
        text: 'This will send the timesheet back to the employee for corrections.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Confirm',
        confirmButtonColor: '#D31145'
    });

    if (!isConfirmed) return;

    const payload = {
        submissionId: data.submissionId,
        employeeId:   data.employeeId,
        month:        data.month,
        year:         data.year,
        revisionNote: feedback
    };

    const res = await reviewFetchAPI('timesheet/supervisor/revision', 'POST', payload);
    if (res) {
        await Swal.fire('Revision Requested', 'The employee has been notified.', 'info');
        window.location.href = '/Timesheet/Supervisor/Report';
    }
}

// ── API Helper ────────────────────────────────────────────────────────────────
async function reviewFetchAPI(endpoint, method = 'GET', body = null) {
    const token = localStorage.getItem('aia_jwt_token');
    const opts = {
        method,
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    };
    if (body) opts.body = JSON.stringify(body);

    const r = await fetch(`https://localhost:7089/api/${endpoint}`, opts);
    if (!r.ok) {
        const err = await r.json().catch(() => ({}));
        if (typeof Swal !== 'undefined') Swal.fire('Error', err.message || 'Operation failed', 'error');
        return null;
    }
    const json = await r.json();
    return json.content || json.data || json;
}

document.addEventListener('DOMContentLoaded', () => {
    initReviewPage();
});
