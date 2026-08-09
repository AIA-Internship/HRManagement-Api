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

async function apiDelete(endpoint) {
    const token = window.aiaAuth && window.aiaAuth.getToken();

    if (!token) {
        window.aiaAuth?.signOut();
        return false;
    }

    try {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            method: "DELETE",
            headers: {
                Authorization: `Bearer ${token}`
            }
        });

        if (res.status === 401) {
            window.aiaAuth.signOut();
            return false;
        }

        return await res.json();
    }
    catch (err) {
        console.error("API DELETE failed:", err);
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

    const timelineResult = await apiGet(`/api/leave/${leaveId}/timeline`);
    const timelineItems = Array.isArray(timelineResult) ? timelineResult : [];

    console.table(timelineItems);

    timelineItems.forEach((x, i) => {
        console.log(i, x.status, x.modifiedUtcDate);
    });

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
                const href = url.startsWith('http')
                    ? url
                    : (API_BASE + (url.startsWith('/') ? '' : '/') + url);

                const attachmentId = it.attachmentId || it.AttachmentId;

                return `
                    <div class="d-flex align-items-center gap-3 p-3 border rounded mb-2">

                        <a class="d-flex align-items-center gap-3 flex-grow-1 text-decoration-none text-reset"
                           href="${href}"
                           target="_blank">

                            <div class="uploaded-icon">
                                <i class="bi ${iconClass}"
                                   style="color:${iconColor}; font-size:2rem"></i>
                            </div>

                            <div class="flex-grow-1">
                                <div class="fw-bold">${escapeHtml(displayName)}</div>
                                <div class="text-muted small">${escapeHtml(sizeText)}</div>
                            </div>

                        </a>

                        <button class="btn btn-sm btn-outline-primary download-btn"
                                data-attachment-id="${attachmentId}"
                                data-name="${escapeHtml(displayName)}">
                            Download
                        </button>

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

        document.querySelectorAll(".download-btn").forEach(btn => {

            btn.addEventListener("click", async function (e) {

                e.preventDefault();
                e.stopPropagation();

                const attachmentId = this.dataset.attachmentId;

                if (!attachmentId) {
                    alert("Attachment ID not found.");
                    return;
                }

                const token = window.aiaAuth && window.aiaAuth.getToken();

                if (!token) {
                    window.aiaAuth?.signOut();
                    return;
                }

                try {
                    const response = await fetch(
                        `${API_BASE}/api/leave/attachment/${attachmentId}/download`,
                        {
                            method: "GET",
                            headers: {
                                Authorization: `Bearer ${token}`
                            }
                        }
                    );

                    if (response.status === 401) {
                        window.aiaAuth?.signOut();
                        return;
                    }

                    if (!response.ok) {
                        console.error(
                            "Download failed:",
                            response.status,
                            response.statusText
                        );

                        alert("Failed to download attachment.");
                        return;
                    }

                    const blob = await response.blob();

                    const downloadUrl = URL.createObjectURL(blob);

                    const a = document.createElement("a");
                    a.href = downloadUrl;
                    a.download = this.dataset.name || "attachment";

                    document.body.appendChild(a);
                    a.click();
                    a.remove();

                    URL.revokeObjectURL(downloadUrl);

                } catch (err) {
                    console.error("Download error:", err);
                    alert("Failed to download attachment.");
                }

            });

        });
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

    const deleteBtn = document.getElementById('deleteBtn');
    const editBtn = document.getElementById("editBtn");


    if (editBtn) {
        editBtn.addEventListener("click", function () {
            window.location.href = `/Leave/Employee/EditLeave?id=${leaveId}`;
        });

        editBtn.classList.toggle("d-none", !(status === 'pending' || status === '1'));
    }

    if (deleteBtn) {
        deleteBtn.classList.toggle("d-none", status === 'rejected' || status === '3');
    }

    if (deleteBtn) {
        deleteBtn.addEventListener("click", async function () {

            if (!confirm("Delete this leave request?"))
                return;

            const result = await apiDelete(`/api/leave/${leaveId}`);

            if (result && !result.isError) {
                alert("Leave request deleted successfully.");
                window.location.href = "/Leave/Employee/Dashboard";
            } else {
                alert(result?.statusMessage ?? "Delete failed.");
            }
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

            if (!s.endsWith('Z') && !/[+-]\d{2}:?\d{2}$/.test(s))
                s = s + 'Z';
        }

        const d = new Date(s);

        // ===== DEBUG =====
        console.log("Original :", input);
        console.log("After Z  :", s);
        console.log("Parsed   :", d);
        // =================

        if (isNaN(d)) return '-';

        const wib = new Date(d.getTime() + 7 * 60 * 60 * 1000);

        // ===== DEBUG =====
        console.log("After +7 :", wib);
        // =================

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
        const requesterName = user && (user.fullName || user.name) ? (user.fullName || user.name) : 'Unknown';

        const created = getField(dto, 'createdUtcDate', 'createdDate', 'createdUtc');
        const createdText = formatDateTimeWib(created);

        const idPart = leaveId ? `REQ${escapeHtml(leaveId)}` : 'REQ-';

        // base created entry (no red line per requirement)
        let html = `
        <li class="ms-1 mb-4 d-flex">
            <div class="timeline-marker mt-3">
                <div class="timeline-icon created">
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

        console.log("timelineResult", timelineResult);
        console.log("timelineItems", timelineItems);

        // discover timeline-like arrays on DTO
        const events = timelineItems;

        // if none found, try to infer from simple fields (fallback: approval info)
        if (events.length === 0) {
            const approval = getField(dto, 'approvedBy', 'approvedByName', 'approvedByFullName');
            if (approval) {
                events.push({ status: 'approved', actor: approval, message: getField(dto, 'approvedNote', 'note'), createdUtcDate: getField(dto, 'approvedUtcDate', 'approvedDate') });
            }
            const rejected = getField(dto, 'rejectedBy');
            if (rejected) {
                events.push({ status: 'rejected', actor: rejected, message: getField(dto, 'rejectedNote', 'note'), createdUtcDate: getField(dto, 'rejectedUtcDate', 'rejectedDate') });
            }
        }

        // render each event with a red vertical line and colored icon depending on status
        // normalize incoming event fields (server may use PascalCase or camelCase)
        events.forEach(orig => {
            const ev = {
                status: getField(orig, 'status', 'Status'),
                modifiedUtcDate: getField(orig, 'modifiedUtcDate', 'ModifiedUtcDate', 'modifiedUtc', 'modifiedDate', 'ModifiedDate'),
                actor: getField(orig, 'actor', 'Actor', 'name', 'fullName'),
                message: getField(orig, 'message', 'Message', 'note')
            };

            if (!ev.status) return; // skip unknown items

            const status = String(ev.status).toLowerCase();

            // skip the "created" event because we already render a base created entry above
            if (status === 'created') return;

            const createdTextEv = formatDateTimeWib(ev.modifiedUtcDate || ev.createdUtcDate || ev.createdDate);

            let icon = "";
            let iconColor = "";
            let iconBg = "";
            let title = "";
            let message = ev.message || "";

            let iconClass = "";

            switch (status) {
                case "edited":
                case "edit":
                    icon = "bi-pencil";
                    iconClass = "edited";
                    title = "Request Edited";
                    break;

                case "approved":
                    icon = "bi-check2-circle";
                    iconClass = "approved";
                    title = "Request Approved";
                    break;

                case "rejected":
                case "declined":
                    icon = "bi-x-circle";
                    iconClass = "rejected";
                    title = "Request Rejected";
                    break;

                default:
                    icon = "bi-info-circle";
                    iconClass = "created";
            }
            html += `
<li class="ms-1 mb-4 d-flex">

    <div class="timeline-marker" style="margin-top:10px;">

        <div class="timeline-icon ${iconClass}">
    <i class="bi ${icon} fs-4"
   style="color:${iconColor};"></i>
</div>

    </div>

    <div style="margin-top:10px;">

        <div class="fw-bold">
            ${title}
        </div>

        <div class="small text-muted">
            ${message}
        </div>

        <div class="small text-muted">
            ${createdTextEv}
        </div>

    </div>

</li>
`;
        });

        timeline.innerHTML = html;
    })();

});