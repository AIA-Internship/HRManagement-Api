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

function escapeHtml(s) {
    if (s === undefined || s === null) return '';
    return String(s).replace(/[&<>"']/g, c => ({"&":"&amp;","<":"&lt;",">":"&gt;","\"":"&quot;","'":"&#39;"}[c]));
}


document.addEventListener("DOMContentLoaded", async function () {

    const params = new URLSearchParams(window.location.search);

    const leaveId = params.get("id");

    const result = await apiGet(`/api/leave/get-by-leave-id/${leaveId}`);

    const dto = Array.isArray(result) ? result[0] : result;

    console.log(dto);

    if (!dto) {
        console.error('Failed to load leave details for id', leaveId);
        return;
    }

    function formatDate(date) {

        return new Date(date).toLocaleDateString("en-GB", {
            day: "numeric",
            month: "long",
            year: "numeric"
        });

    }

    // Helper to safely get a property in multiple naming conventions
    function getField(obj, ...names) {
        for (const n of names) {
            if (obj == null) continue;
            if (Object.prototype.hasOwnProperty.call(obj, n)) return obj[n];
            const lower = n.charAt(0).toLowerCase() + n.slice(1);
            if (Object.prototype.hasOwnProperty.call(obj, lower)) return obj[lower];
        }
        return null;
    }

    // Map leave type
    const leaveTypeRaw = getField(dto, 'leaveType');
    const leaveTypeText = (function (v) {
        if (v == null) return '-';
        if (String(v) === '1' || v === 1) return 'Paid Leave';
        if (String(v) === '2' || v === 2) return 'Sick Leave';
        return String(v);
    })(leaveTypeRaw);

    const leaveTypeEl = document.getElementById('leaveType');
    if (leaveTypeEl) leaveTypeEl.textContent = leaveTypeText;

    const statusRaw = getField(dto, 'leaveStatus');
    const status = String(statusRaw ?? '').toLowerCase();
    const statusText = (statusRaw == null)
        ? '-'
        : (status === '1' ? 'pending'
            : (status === '2' ? 'approved'
                : (status === '3' ? 'rejected'
                    : String(statusRaw))));

    const statusIcon = document.getElementById('leaveStatusIcon');
    const statusTextEl = document.getElementById('leaveStatusText');
    const statusBadge = document.getElementById('leaveStatus');

    if (statusTextEl) statusTextEl.textContent = statusText;
    if (statusIcon) {
        statusIcon.className = 'bi me-1';
        if (status === 'pending' || status === '1') {
            statusIcon.classList.add('bi-clock');
        } else if (status === 'approved' || status === '2') {
            statusIcon.classList.add('bi-check2-circle');
        } else if (status === 'rejected' || status === '3') {
            statusIcon.classList.add('bi-x-circle');
        }
    }

    if (statusBadge) {
        statusBadge.className = 'badge';
        if (status === 'pending' || status === '1') statusBadge.classList.add("pending_status");
        else if (status === 'approved' || status === '2') statusBadge.classList.add("approved_status");
        else if (status === 'rejected' || status === '3') statusBadge.classList.add("rejected_status");
        else statusBadge.classList.add('bg-secondary');
    }

    // Apply colored left border to request-summary and color the status icon background
    const requestSummary = document.querySelector('.request-summary');
    if (requestSummary) {
        requestSummary.classList.remove('status-pending', 'status-approved', 'status-rejected');
        if (status === 'pending' || status === '1') requestSummary.classList.add('status-pending');
        else if (status === 'approved' || status === '2') requestSummary.classList.add('status-approved');
        else if (status === 'rejected' || status === '3') requestSummary.classList.add('status-rejected');
    }

    if (statusIcon) {
        // give icon a colored background and white icon color for visibility
        statusIcon.style.color = '#fff';
        statusIcon.style.display = 'inline-flex';
        statusIcon.style.alignItems = 'center';
        statusIcon.style.justifyContent = 'center';
        statusIcon.style.width = '1.4rem';
        statusIcon.style.height = '1.4rem';
        statusIcon.style.borderRadius = '0.35rem';
        statusIcon.style.marginRight = '0.5rem';

        if (status === 'pending' || status === '1') {
            statusIcon.style.color = '#F59E0B'; 
        } else if (status === 'approved' || status === '2') {
            statusIcon.style.color = '#10B981'; 
        } else if (status === 'rejected' || status === '3') {
            statusIcon.style.color = '#DC2626'; 
        } else {
            statusIcon.style.color = '#6c757d';
        }
    }

    // Dates
    const startRaw = getField(dto, 'leaveStartDate');
    const endRaw = getField(dto, 'leaveEndDate');
    const dayAmount = getField(dto, 'dayAmount');

    const startEl = document.getElementById('startDate');
    const endEl = document.getElementById('endDate');

    if (startEl) startEl.textContent = startRaw ? formatDate(startRaw) : '-';

    // compute end date if not provided
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

    function renderAttachments(items) {
        if (!attachmentWrap) return;
        if (!items || items.length === 0) {
            attachmentWrap.innerHTML = '<div class="text-muted fw-semibold fs-6">No attachments available.</div>';
            return;
        }

        const html = items.map(it => {
            // try several possible fields
            const name = it.name || it.fileName || it.file_name || it.displayName || it.title || '';
            const url = it.url || it.path || it.filePath || it.fileUrl || it.downloadUrl || it.file || '';
            const displayName = name || (url ? url.split('/').pop() : 'Attachment');

            if (url) {
                // ensure absolute URL if backend returned relative path
                const href = url.startsWith('http') ? url : (API_BASE + (url.startsWith('/') ? '' : '/') + url);
                return `
                    <div class="d-flex align-items-center gap-3 p-3 border rounded mb-2">
                        <i class="bi bi-paperclip fs-3 text-brand"></i>
                        <div class="flex-grow-1">
                            <div class="fw-bold">${displayName}</div>
                            <div class="text-muted small">${escapeHtml(url)}</div>
                        </div>
                        <a class="btn btn-sm btn-outline-primary" href="${href}" target="_blank">Open</a>
                    </div>
                `;
            }

            return `
                <div class="d-flex align-items-center gap-3 p-3 border rounded mb-2">
                    <i class="bi bi-paperclip fs-3 text-brand"></i>
                    <div class="flex-grow-1">
                        <div class="fw-bold">${displayName}</div>
                    </div>
                </div>
            `;
        }).join('');

        attachmentWrap.innerHTML = html;
    }

    // find attachments in several possible fields
    const attachments = getField(dto, 'attachments', 'Attachments', 'attachmentPaths', 'attachmentPath', 'files') || [];
    renderAttachments(attachments);

    // Cancel button
    const cancelBtn = document.getElementById('cancelBtn');
    if (cancelBtn) cancelBtn.addEventListener('click', function () { window.location.href = '/Leave/Employee/Dashboard'; });

    // Format CreatedUtcDate (assumed UTC) into WIB (UTC+7) with format d/M/yyyy, HH.mm WIB
    function formatDateTimeWib(input) {
        if (!input) return '-';

        // Normalize string like '2026-07-05 16:35:41.2479079' to ISO UTC
        let s = input;
        if (typeof s === 'string') {
            s = s.trim();
            if (s.indexOf('T') === -1) {
                // replace first space between date and time with T
                s = s.replace(' ', 'T');
            }
            // if no timezone info, treat as UTC
            if (!s.endsWith('Z') && !/[+-]\d{2}:?\d{2}$/.test(s)) s = s + 'Z';
        }

        const d = new Date(s);
        if (isNaN(d)) return '-';

        // shift to WIB (UTC+7)
        const wib = new Date(d.getTime() + 7 * 60 * 60 * 1000);

        const day = wib.getUTCDate();
        const month = wib.getUTCMonth() + 1;
        const year = wib.getUTCFullYear();
        const hours = String(wib.getUTCHours()).padStart(2, '0');
        const minutes = String(wib.getUTCMinutes()).padStart(2, '0');

        return `${day}/${month}/${year}, ${hours}.${minutes} WIB`;
    }

    // populate last updated
    (function () {
        const created = getField(dto, 'createdUtcDate', 'CreatedUtcDate', 'createdDate', 'createdUtc');
        const lastEl = document.getElementById('lastUpdated');
        if (lastEl) lastEl.textContent = formatDateTimeWib(created);
    })();

    // Render timeline: REQ{id} and requester/leave type info
    (function () {
        const timeline = document.getElementById('timelineList') || document.querySelector('.timeline');
        if (!timeline) return;

        const user = window.aiaAuth.getUserInfo();
        console.log(user);
        const requesterName = user.fullName;

        const created = getField(dto, 'createdUtcDate');
        const createdText = formatDateTimeWib(created);

        const idPart = leaveId ? `REQ${escapeHtml(leaveId)}` : 'REQ-';

        const html = `
        <li class="mb-4 d-flex">
            <div class="timeline-marker mt-2">
                <div class="timeline-icon">
                    <i class="bi bi-clock-history fs-2"></i>
                </div>
            </div>
            <div>
                <div class="fw-bold">${idPart} Requested</div>
                <div class="small text-muted">
                    Request submitted by ${escapeHtml(requesterName)} for ${escapeHtml(leaveTypeText)}.
                </div>
                <div class="small text-muted">${escapeHtml(createdText)}</div>
            </div>
        </li>
    `;

        timeline.innerHTML = html;
    })();

});