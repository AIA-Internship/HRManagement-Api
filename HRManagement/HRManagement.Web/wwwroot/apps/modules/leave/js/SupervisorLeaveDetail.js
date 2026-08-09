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

// NOTE: adjust these two endpoints to match the actual controller routes for
// ApprovedLeaveRequest / RejectedLeaveRequest on the backend.
async function apiPost(endpoint, body) {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    if (!token) { window.aiaAuth && window.aiaAuth.signOut(); return null; }
    try {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            method: "POST",
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: body ? JSON.stringify(body) : undefined
        });
        if (res.status === 401) { window.aiaAuth.signOut(); return null; }
        if (!res.ok) throw new Error(`Request failed with status ${res.status}`);
        return await res.json();
    } catch (err) {
        console.error("API POST failed:", err);
        throw err;
    }
}

function escapeHtml(s) {
    if (s === undefined || s === null) return '';
    return String(s).replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" }[c]));
}

function getField(obj, ...names) {
    for (const n of names) {
        if (obj == null) continue;
        if (Object.prototype.hasOwnProperty.call(obj, n)) return obj[n];
        const lower = n.charAt(0).toLowerCase() + n.slice(1);
        if (Object.prototype.hasOwnProperty.call(obj, lower)) return obj[lower];
    }
    return null;
}

function formatDate(date) {
    return new Date(date).toLocaleDateString("en-GB", {
        day: "numeric",
        month: "long",
        year: "numeric"
    });
}

function formatDateTimeWib(input) {
    if (!input) return '-';

    let s = input;
    if (typeof s === 'string') {
        s = s.trim();
        if (s.indexOf('T') === -1) {
            s = s.replace(' ', 'T');
        }
        if (!s.endsWith('Z') && !/[+-]\d{2}:?\d{2}$/.test(s)) s = s + 'Z';
    }

    const d = new Date(s);
    if (isNaN(d)) return '-';

    const wib = new Date(d.getTime() + 7 * 60 * 60 * 1000);

    const day = wib.getUTCDate();
    const month = wib.getUTCMonth() + 1;
    const year = wib.getUTCFullYear();
    const hours = String(wib.getUTCHours()).padStart(2, '0');
    const minutes = String(wib.getUTCMinutes()).padStart(2, '0');

    return `${day}/${month}/${year}, ${hours}.${minutes} WIB`;
}

// Backend has been observed sending leaveStatus as either a numeric code
// ("1"/"2"/"3") or a string ("Pending"/"Approved"/"Rejected"). Everything
// downstream (badge, action buttons, timeline, accent border) must agree on
// the same normalized value, otherwise pieces of UI go out of sync — this is
// what previously hid the Approve/Reject buttons even when the request was
// still pending.
function normalizeStatus(raw) {
    const s = String(raw ?? '').trim().toLowerCase();
    if (s === '1' || s === 'pending' || s === 'needsapproval' || s === 'needs approval') return 'Pending';
    if (s === '2' || s === 'approved') return 'Approved';
    if (s === '3' || s === 'rejected') return 'Rejected';
    return 'Unknown';
}

document.addEventListener("DOMContentLoaded", async function () {

    const params = new URLSearchParams(window.location.search);
    const leaveId = params.get("id");

    if (!leaveId) {
        console.error('Missing leave id in query string');
        return;
    }

    const result = await apiGet(`/api/leave/get-by-leave-id/${leaveId}`);
    const dto = Array.isArray(result) ? result[0] : result;

    if (!dto) {
        console.error('Failed to load leave details for id', leaveId);
        return;
    }


   


    // Fetch requester (employee) info for the "Submitted By" field
    const requesterId = getField(dto, 'requesterDisplayId');
    let employee = null;
    if (requesterId) {
        employee = await apiGet(`/api/employee/${requesterId}`);
    }

    const submittedByEl = document.getElementById('submittedBy');
    if (submittedByEl) submittedByEl.textContent = employee?.fullName || '-';

    // Leave type
    const leaveTypeRaw = getField(dto, 'leaveType');
    const leaveTypeText = (function (v) {
        if (v == null) return '-';
        if (String(v) === '1') return 'Annual Leave';
        if (String(v) === '2') return 'Sick Leave';
        return 'Other';
    })(leaveTypeRaw);

    const leaveTypeEl = document.getElementById('leaveType');
    if (leaveTypeEl) leaveTypeEl.textContent = leaveTypeText;

    // Status — single source of truth used everywhere below
    const statusRaw = getField(dto, 'leaveStatus');
    const statusKey = normalizeStatus(statusRaw);

    const statusText =
        statusKey === 'Pending' ? 'Needs Approval'
            : statusKey === 'Approved' ? 'Approved'
                : statusKey === 'Rejected' ? 'Rejected'
                    : '-';

    const statusClass =
        statusKey === 'Pending' ? 'status-needs-approval'
            : statusKey === 'Approved' ? 'status-approved'
                : statusKey === 'Rejected' ? 'status-rejected'
                    : null;

    const statusIconClass =
        statusKey === 'Pending' ? 'bi-hourglass-split'
            : statusKey === 'Approved' ? 'bi-check2-circle'
                : statusKey === 'Rejected' ? 'bi-x-circle'
                    : 'bi-question-circle';

    const statusBadge = document.getElementById('leaveStatus');
    const statusTextEl = document.getElementById('leaveStatusText');
    const statusIconEl = document.getElementById('leaveStatusIcon');

    if (statusTextEl) statusTextEl.textContent = statusText;
    if (statusIconEl) statusIconEl.className = `bi me-1 ${statusIconClass}`;
    if (statusBadge && statusClass) statusBadge.classList.add(statusClass);

    // Accent border on the summary card, matched to status
    const summaryCard = document.getElementById('summaryCard');
    if (summaryCard) {
        const accentClass =
            statusKey === 'Pending' ? 'accent-pending'
                : statusKey === 'Approved' ? 'accent-approved'
                    : statusKey === 'Rejected' ? 'accent-rejected'
                        : null;
        if (accentClass) summaryCard.classList.add(accentClass);
    }

    // Dates
    const startRaw = getField(dto, 'leaveStartDate');
    const endRaw = getField(dto, 'leaveEndDate', 'endDate');
    const dayAmount = getField(dto, 'dayAmount');

    const startEl = document.getElementById('startDate');
    const endEl = document.getElementById('endDate');
    const dayAmountEl = document.getElementById('dayAmount');

    if (startEl) startEl.textContent = startRaw ? formatDate(startRaw) : '-';
    if (dayAmountEl) dayAmountEl.textContent = dayAmount ?? '-';

    if (endEl) {
        if (endRaw) {
            endEl.textContent = formatDate(endRaw);
        } else if (startRaw && dayAmount) {
            const sd = new Date(startRaw);
            const end = new Date(sd);
            end.setDate(end.getDate() + (Math.ceil(Number(dayAmount)) - 1));
            endEl.textContent = formatDate(end.toISOString());
        } else {
            endEl.textContent = '-';
        }
    }

    // Description
    const desc = getField(dto, 'leaveDescription');
    const descEl = document.getElementById('leaveDescription');
    if (descEl) descEl.textContent = desc || '-';

    // Attachments
    const attachmentWrap = document.getElementById('attachmentContainer');

    async function downloadFile(url, fileName) {
        const response = await fetch(url);
        const blob = await response.blob();
        const objectUrl = URL.createObjectURL(blob);

        const a = document.createElement("a");
        a.href = objectUrl;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        a.remove();

        URL.revokeObjectURL(objectUrl);
    }
    window.downloadFile = downloadFile;

    function renderAttachments(items) {
        if (!attachmentWrap) return;
        if (!items || items.length === 0) {
            attachmentWrap.innerHTML = '<div class="text-muted fw-semibold fs-6">No attachments available.</div>';
            return;
        }
        const html = items.map(it => {
            const name = it.name || it.fileName || it.file_name || it.displayName || it.title || '';
            const url = it.url || it.path || it.filePath || it.fileUrl || it.downloadUrl || it.file || '';
            const sizeBytes = it.size || it.fileSize || it.length || it.sizeInBytes || it.contentLength || null;
            const displayName = name || (url ? url.split('/').pop() : 'Attachment');
            const sizeText = (sizeBytes || sizeBytes === 0) ? `${(Number(sizeBytes) / 1024).toFixed(1)} KB` : '';

            const ext = (displayName.split('.').pop() || '').toLowerCase();
            let iconClass = 'bi-file-earmark-fill';
            let iconColor = '#6c757d';

            if (ext === 'pdf') {
                iconClass = 'bi-file-earmark-pdf-fill';
                iconColor = '#dc3545';
            } else if (ext === 'jpg' || ext === 'jpeg' || ext === 'png') {
                iconClass = 'bi-file-earmark-image-fill';
                iconColor = '#0d6efd';
            }

            if (url) {
                const href = url.startsWith('http') ? url : (API_BASE + (url.startsWith('/') ? '' : '/') + url);
                return `
                    <div class="d-flex align-items-center gap-3 p-3 border rounded mb-2">
                        <a class="d-flex align-items-center gap-3 flex-grow-1 text-decoration-none text-reset" href="${href}" target="_blank" rel="noopener noreferrer">
                            <div class="uploaded-icon">
                                <i class="bi ${iconClass}" style="color:${iconColor}; font-size:2rem"></i>
                            </div>
                            <div class="flex-grow-1">
                                <div class="fw-bold">${escapeHtml(displayName)}</div>
                                <div class="text-muted small">${escapeHtml(sizeText)}</div>
                            </div>
                        </a>
                        <a class="btn btn-sm btn-outline-primary" href="${href}" target="_blank" download="${escapeHtml(displayName)}" onclick="event.stopPropagation();">Download</a>
                    </div>
                `;
            }

            return `
                <div class="d-flex align-items-center gap-3 p-3 border rounded mb-2">
                    <div class="uploaded-icon">
                        <i class="bi ${iconClass}" style="color:${iconColor}; font-size:1.6rem"></i>
                    </div>
                    <div class="flex-grow-1">
                        <div class="fw-bold">${escapeHtml(displayName)}</div>
                        <div class="text-muted small">${escapeHtml(sizeText)}</div>
                    </div>
                </div>
            `;
        }).join('');

        attachmentWrap.innerHTML = html;
    }

    let attachments = [];
    try {
        const apiAttachments = await apiGet(`/api/leave/${leaveId}/attachments`);
        if (Array.isArray(apiAttachments) && apiAttachments.length > 0) {
            attachments = apiAttachments;
        }
    } catch (err) {
        console.warn('Failed to fetch attachments from API:', err);
    }

    if (!attachments || attachments.length === 0) {
        attachments = getField(dto, 'attachments', 'Attachments', 'attachmentPaths', 'attachmentPath', 'files') || [];
    }

    renderAttachments(attachments);

    // Last updated + timeline
    const created = getField(dto, 'createdUtcDate', 'CreatedUtcDate', 'createdDate', 'createdUtc');
    const decisionDate = getField(dto, 'approvedUtcDate', 'rejectedUtcDate', 'decisionUtcDate', 'updatedUtcDate');

    const lastEl = document.getElementById('lastUpdated');
    if (lastEl) lastEl.textContent = formatDateTimeWib(decisionDate || created);

    //timeline
    // Fetch history rows for the timeline (separate from the leave request itself)
    let historyRows = [];
    try {
        const historyResult = await apiGet(`/api/leave/${leaveId}/timeline`);
        if (Array.isArray(historyResult)) historyRows = historyResult;
    } catch (err) {
        console.warn('Failed to fetch leave request history:', err);
    }

    // "Changed by" always comes from the leave request itself, not from the
    // history row — per spec.
    const lastModifiedBy = getField(dto, 'lastModifiedBy', 'lastModifiedByName', 'changedBy') || '-';

    (function renderTimeline() {
        const timeline = document.getElementById('timelineList');
        if (!timeline) return;

        const createdText = formatDateTimeWib(created);
        const idPart = leaveId ? `REQ${escapeHtml(leaveId)}` : 'REQ-';

        // First entry: always the original submission
        let html = `
    <li class="timeline-item">
        <div class="timeline-marker">
            <div class="timeline-icon timeline-icon-default">
                <i class="bi bi-clock-history"></i>
            </div>
            <div class="timeline-line"></div>
        </div>
        <div class="timeline-content">
            <div class="timeline-title">${idPart} Requested</div>
            <div class="timeline-desc">
                Request submitted by ${escapeHtml(employee?.fullName || '-')} for '${escapeHtml(leaveTypeText)}'.
            </div>
            <div class="timeline-date">${createdText}</div>
        </div>
    </li>
    `;

        // History rows: reason column literally drives the label + icon
        historyRows.forEach((row, i) => {
            const reasonRaw = String(getField(row, 'reason') || '').trim();
            const noteText = getField(row, 'description', 'comment', 'note', 'remarks') || reasonRaw;
            const rowDate = formatDateTimeWib(getField(row, 'createdUtcDate', 'date', 'changedDate'));

            let label, iconClass, iconVariant;
            if (reasonRaw.toLowerCase() === 'rejected') {
                label = `Rejected by ${escapeHtml(lastModifiedBy)}`;
                iconClass = 'bi-x-circle';
                iconVariant = 'timeline-icon-rejected';
            } else if (reasonRaw.toLowerCase() === 'approved') {
                label = `Approved by ${escapeHtml(lastModifiedBy)}`;
                iconClass = 'bi-check-lg';
                iconVariant = 'timeline-icon-approved';
            } else {
                // default style — anything that isn't explicitly Approved/Rejected
                label = escapeHtml(reasonRaw || 'Updated');
                iconClass = 'bi-arrow-repeat';
                iconVariant = 'timeline-icon-default';
            }

            const isLast = i === historyRows.length - 1;

            html += `
        <li class="timeline-item">
            <div class="timeline-marker">
                <div class="timeline-icon ${iconVariant}">
                    <i class="bi ${iconClass}"></i>
                </div>
                ${isLast ? '' : '<div class="timeline-line"></div>'}
            </div>
            <div class="timeline-content">
                <div class="timeline-title">${label}</div>
                ${noteText ? `<div class="timeline-desc">"${escapeHtml(noteText)}"</div>` : ''}
                <div class="timeline-date">${rowDate}</div>
            </div>
        </li>
        `;
        });

        timeline.innerHTML = html;
    })();

    // Approve / Reject actions — only visible while the request is still pending
    const actionRow = document.getElementById('actionButtonsRow');
    if (actionRow) {
        actionRow.classList.toggle('d-none', statusKey !== 'Pending');
    }

    const approveBtn = document.getElementById('approveBtn');
    const rejectBtn = document.getElementById('rejectBtn');


    console.log('Wiring approve/reject buttons', { approveBtn, rejectBtn, actionRow });
    // --- Modal helpers ---
    function openModal(id) {
        document.getElementById(id)?.classList.remove('d-none');
    }
    function closeModal(id) {
        document.getElementById(id)?.classList.add('d-none');
    }
    document.querySelectorAll('[data-close]').forEach(btn => {
        btn.addEventListener('click', () => closeModal(btn.dataset.close));
    });

    if (approveBtn) {
        approveBtn.addEventListener('click', () => openModal('approveConfirmModal'));
    }

    const approveConfirmBtn = document.getElementById('approveConfirmBtn');
    if (approveConfirmBtn) {
        approveConfirmBtn.addEventListener('click', async function () {
            approveConfirmBtn.disabled = true;
            try {
                await apiPost(`/api/leave/approve-request/${leaveId}`);
                closeModal('approveConfirmModal');
                openModal('approveSuccessModal');
            } catch (err) {
                alert('Failed to approve the request. Please try again.');
            } finally {
                approveConfirmBtn.disabled = false;
            }
        });
    }

    document.getElementById('approveSuccessOkBtn')?.addEventListener('click', function () {
        window.location.href = '/Leave/Supervisor/Dashboard';
    });

    if (rejectBtn) {
        rejectBtn.addEventListener('click', () => openModal('rejectConfirmModal'));
    }

    const rejectConfirmBtn = document.getElementById('rejectConfirmBtn');
    if (rejectConfirmBtn) {
        rejectConfirmBtn.addEventListener('click', async function () {
            const reasonInput = document.getElementById('rejectReasonInput');
            const errorEl = document.getElementById('rejectReasonError');
            const reason = reasonInput.value.trim();

            if (!reason) {
                errorEl.classList.remove('d-none');
                reasonInput.focus();
                return;
            }
            errorEl.classList.add('d-none');

            rejectConfirmBtn.disabled = true;
            try {
                await apiPost(`/api/leave/rejected-request/${leaveId}`, { reason });
                closeModal('rejectConfirmModal');
                openModal('rejectSuccessModal');
            } catch (err) {
                alert('Failed to reject the request. Please try again.');
            } finally {
                rejectConfirmBtn.disabled = false;
            }
        });
    }

    document.getElementById('rejectSuccessOkBtn')?.addEventListener('click', function () {
        window.location.href = '/Leave/Supervisor/Dashboard';
    });
});