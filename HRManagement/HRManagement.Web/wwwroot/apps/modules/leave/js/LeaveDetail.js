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

    const requestSummary = document.querySelector('.request-summary');
    if (requestSummary) {
        requestSummary.classList.remove('status-pending', 'status-approved', 'status-rejected');
        if (status === 'pending' || status === '1') requestSummary.classList.add('status-pending');
        else if (status === 'approved' || status === '2') requestSummary.classList.add('status-approved');
        else if (status === 'rejected' || status === '3') requestSummary.classList.add('status-rejected');
    }

    if (statusIcon) {
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

    // compute end date
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
    }
    catch (err) {
        console.warn('Failed to fetch attachments from API:', err);
    }

    if (!attachments || attachments.length === 0) {
        attachments = getField(dto, 'attachments', 'Attachments', 'attachmentPaths', 'attachmentPath', 'files') || [];
    }

    renderAttachments(attachments);

    const cancelBtn = document.getElementById('cancelBtn');
    if (cancelBtn) cancelBtn.addEventListener('click', function () { window.location.href = '/Leave/Employee/Dashboard'; });

    const editBtn = document.getElementById("editBtn");
    const deleteBtn = document.getElementById("cancelBtn");

    if (editBtn) {
        editBtn.addEventListener("click", function () {
            window.location.href = `/Leave/Employee/EditLeave?id=${leaveId}`;
        });

        editBtn.classList.toggle("d-none", !(status === 'pending' || status === '1'));
    }

    if (deleteBtn) {
        deleteBtn.classList.toggle("d-none", status === 'rejected' || status === '3');
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

    (function () {
        const created = getField(dto, 'createdUtcDate', 'CreatedUtcDate', 'createdDate', 'createdUtc');
        const lastEl = document.getElementById('lastUpdated');
        if (lastEl) lastEl.textContent = formatDateTimeWib(created);
    })();

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