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
    const requesterId = getField(dto, 'requesterId');
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

    // Status
    const statusRaw = getField(dto, 'leaveStatus');
    const status = String(statusRaw ?? '');

    const statusText = status === '1' ? 'Needs Approval'
        : status === '2' ? 'Approved'
            : status === '3' ? 'Rejected'
                : '-';

    const statusClass = status === '1' ? 'status-needs-approval'
        : status === '2' ? 'status-approved'
            : status === '3' ? 'status-rejected'
                : '';

    const statusIconClass = status === '1' ? 'bi-hourglass-split'
        : status === '2' ? 'bi-check2-circle'
            : status === '3' ? 'bi-x-circle'
                : 'bi-question-circle';

    const statusBadge = document.getElementById('leaveStatus');
    const statusTextEl = document.getElementById('leaveStatusText');
    const statusIconEl = document.getElementById('leaveStatusIcon');

    if (statusTextEl) statusTextEl.textContent = statusText;
    if (statusIconEl) statusIconEl.className = `bi me-1 ${statusIconClass}`;
    if (statusBadge) statusBadge.classList.add(statusClass);

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

    (function renderTimeline() {
        const timeline = document.getElementById('timelineList');
        if (!timeline) return;

        const createdText = formatDateTimeWib(created);
        const idPart = leaveId ? `REQ${escapeHtml(leaveId)}` : 'REQ-';

        let html = `
        <li class="mb-4 d-flex">
            <div class="timeline-marker mt-2">
                <div class="timeline-icon">
                    <i class="bi bi-clock-history fs-2"></i>
                </div>
            </div>
            <div>
                <div class="fw-bold">${idPart} Requested</div>
                <div class="small text-muted">
                    Request submitted by ${escapeHtml(employee?.fullName || '-')} for ${escapeHtml(leaveTypeText)}.
                </div>
                <div class="small text-muted">${createdText}</div>
            </div>
        </li>
        `;

        if (status === '2' || status === '3') {
            const decisionText = formatDateTimeWib(decisionDate);
            const label = status === '2' ? 'Approved' : 'Rejected';
            const icon = status === '2' ? 'bi-check2-circle' : 'bi-x-circle';

            html += `
            <li class="mb-4 d-flex">
                <div class="timeline-marker mt-2">
                    <div class="timeline-icon">
                        <i class="bi ${icon} fs-2"></i>
                    </div>
                </div>
                <div>
                    <div class="fw-bold">${idPart} ${label}</div>
                    <div class="small text-muted">${decisionText}</div>
                </div>
            </li>
            `;
        }

        timeline.innerHTML = html;
    })();

    // Approve / Reject actions — only visible while the request is still pending
    const actionRow = document.getElementById('actionButtonsRow');
    if (actionRow) {
        actionRow.classList.toggle('d-none', status !== '1');
    }

    const approveBtn = document.getElementById('approveBtn');
    const rejectBtn = document.getElementById('rejectBtn');

    if (approveBtn) {
        approveBtn.addEventListener('click', async function () {
            if (!confirm('Approve this leave request?')) return;
            approveBtn.disabled = true;
            rejectBtn.disabled = true;
            try {
                await apiPost(`/api/leave/approve/${leaveId}`);
                window.location.href = '/Leave/Supervisor/Dashboard';
            } catch (err) {
                alert('Failed to approve the request. Please try again.');
                approveBtn.disabled = false;
                rejectBtn.disabled = false;
            }
        });
    }

    if (rejectBtn) {
        rejectBtn.addEventListener('click', async function () {
            if (!confirm('Reject this leave request?')) return;
            approveBtn.disabled = true;
            rejectBtn.disabled = true;
            try {
                await apiPost(`/api/leave/reject-leave-request/${leaveId}`);
                window.location.href = '/Leave/Supervisor/Dashboard';
            } catch (err) {
                alert('Failed to reject the request. Please try again.');
                approveBtn.disabled = false;
                rejectBtn.disabled = false;
            }
        });
    }
});
