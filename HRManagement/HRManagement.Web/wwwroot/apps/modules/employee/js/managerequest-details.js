const API_BASE = "https://localhost:7089";
let currentRequestId = null;

const FIELD_LABELS = {
    newFullName: "Full Name",
    newGender: "Gender",
    newPersonalEmail: "Personal Email",
    newPlaceOfBirth: "Place of Birth",
    newNik: "NIK",
    newDateOfBirth: "Date of Birth",
    newMaritalStatus: "Marital Status",
    newCurrentStreetAddress: "Current Address — Street",
    newCurrentCity: "Current Address — City",
    newCurrentProvince: "Current Address — Province",
    newCurrentPostalCode: "Current Address — Postal Code",
    newResidentialStreetAddress: "Residential Address — Street",
    newResidentialCity: "Residential Address — City",
    newResidentialProvince: "Residential Address — Province",
    newResidentialPostalCode: "Residential Address — Postal Code",
    newPhoneNumber: "Phone Number",
    newEmergencyContactName: "Emergency Contact Name",
    newEmergencyContactPhone: "Emergency Contact Phone",
    newEmergencyContactRelationship: "Emergency Contact Relationship"
};

document.addEventListener("DOMContentLoaded", async () => {
    const idStr = new URLSearchParams(window.location.search).get("id");
    if (!idStr) { showError("Missing request ID."); return; }

    currentRequestId = parseInt(idStr, 10);
    bindActionButtons();

    const request = await fetchRequest(currentRequestId);
    if (!request) { showError("Request not found."); return; }

    // 1. Grab the ID (with robust fallbacks)
    const employeeId = request.employeeDisplayId || request.employeeId || request.requesterId;

    // 2. Fetch the profile
    const profile = employeeId ? await fetchEmployeeProfile(employeeId) : null;

    // 3. VISUAL DEBUGGER: If profile is null, show a big red warning on screen!
    if (!profile) {
        const debugDiv = document.createElement("div");
        debugDiv.className = "alert alert-danger mx-8 mt-6 mb-0 d-flex align-items-center p-5";
        debugDiv.innerHTML = `
            <i class="bi bi-exclamation-triangle fs-2 text-danger me-4"></i>
            <div class="d-flex flex-column">
                <h4 class="mb-1 text-danger">Developer Debug Warning</h4>
                <span>Failed to load profile for Employee ID: <strong>${employeeId || 'NULL'}</strong>. Previous values cannot be displayed. Press <strong>F12 -> Network</strong> to see if the API failed.</span>
            </div>
        `;
        document.getElementById("kt_app_content_container").prepend(debugDiv);
    }

    renderDetails(request, profile);
});

// Prefer the stored snapshot (survives approval); fall back to deriving from the live profile.
function getChanges(req, profile, status) {
    if (req.changesJson) {
        try {
            const snap = JSON.parse(req.changesJson);
            if (Array.isArray(snap) && snap.length) {
                return snap.map(c => {
                    const field = c.field ?? c.Field;
                    const previous = c.previous ?? c.Previous;
                    const updated = c.updated ?? c.Updated;
                    return {
                        label: FIELD_LABELS[field] || field,
                        previous: (previous === null || previous === undefined || previous === "") ? "—" : previous,
                        updated: updated
                    };
                });
            }
        } catch (e) {
            console.warn("Bad changesJson, falling back:", e);
        }
    }
    return collectChanges(req, profile, status);
}

function collectChanges(req, profile, status) {
    const fmtDate = v => {
        if (!v) return "";
        const d = new Date(v);
        if (isNaN(d)) return v;
        return `${String(d.getDate()).padStart(2, "0")}/${String(d.getMonth() + 1).padStart(2, "0")}/${d.getFullYear()}`;
    };

    // I have REMOVED the "Pending Only" check. 
    // It will now ALWAYS try to show previous values if the profile exists!
    const previousMap = profile ? {
        newFullName: profile.fullName,
        newGender: profile.gender,
        newPersonalEmail: profile.personalEmail,
        newPlaceOfBirth: profile.placeOfBirth,
        newNik: profile.nik,
        newDateOfBirth: fmtDate(profile.dateOfBirth),
        newMaritalStatus: profile.maritalStatusName,
        newCurrentStreetAddress: profile.currentStreetAddress,
        newCurrentCity: profile.currentCity,
        newCurrentProvince: profile.currentProvince,
        newCurrentPostalCode: profile.currentPostalCode,
        newResidentialStreetAddress: profile.residentialStreetAddress,
        newResidentialCity: profile.residentialCity,
        newResidentialProvince: profile.residentialProvince,
        newResidentialPostalCode: profile.residentialPostalCode,
        newPhoneNumber: profile.phoneNumber,
        newEmergencyContactName: profile.emergencyContact ? profile.emergencyContact.name : "",
        newEmergencyContactPhone: profile.emergencyContact ? profile.emergencyContact.phoneNumber : "",
        newEmergencyContactRelationship: profile.emergencyContact ? profile.emergencyContact.relationship : ""
    } : {};

    const changes = [];
    Object.keys(FIELD_LABELS).forEach(key => {
        let newVal = req[key];
        if (newVal === undefined || newVal === null || newVal === "" || newVal === "Unknown") return;
        if (key === "newDateOfBirth") newVal = fmtDate(newVal);

        const prevVal = previousMap[key] || "—";
        changes.push({ label: FIELD_LABELS[key], previous: prevVal, updated: newVal });
    });
    return changes;
}

async function fetchRequest(id) {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    try {
        const res = await fetch(`${API_BASE}/api/employee/requests`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!res.ok) return null;
        const json = await res.json();
        const list = json.content || json.data || [];
        return list.find(r => r.requestId === id) || null;
    } catch (err) {
        console.error("API error:", err);
        return null;
    }
}

async function fetchEmployeeProfile(employeeId) {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    try {
        const res = await fetch(`${API_BASE}/api/employee/${employeeId}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!res.ok) return null;
        const json = await res.json();
        return json.content || json.data || null;
    } catch (err) {
        return null;
    }
}

function renderDetails(req, profile) {
    const status = normalizeStatus(req.requestStatus);
    document.getElementById("metaId").textContent = "#REQ" + String(req.requestId).padStart(3, "0");
    document.getElementById("metaSubmittedBy").textContent = req.requesterName || "Unknown Employee";
    document.getElementById("metaSubmittedDate").textContent = formatDate(req.createdAt);

    const statusEl = document.getElementById("metaStatus");
    if (statusEl) statusEl.innerHTML = renderStatusBadge(status);

    const changes = getChanges(req, profile, status);
    document.getElementById("metaCount").textContent = `${changes.length} Fields`;
    renderChanges(changes);

    // Approve/Reject only for pending requests AND supervisor/admin role.
    if (status === "Pending" && isReviewer()) {
        document.getElementById("actionButtonsContainer").style.setProperty("display", "flex", "important");
    }

    // Reviewer info + comment for reviewed (approved/rejected) requests.
    if (status === "Approved" || status === "Rejected") {
        const byLabel = status === "Rejected" ? "Rejected By" : "Approved By";
        const dateLabel = status === "Rejected" ? "Rejected Date" : "Approved Date";

        document.getElementById("reviewByLabel").textContent = byLabel;
        document.getElementById("metaReviewBy").textContent = req.reviewedBy || "-";
        document.getElementById("reviewByCol").style.display = "block";

        document.getElementById("reviewDateLabel").textContent = dateLabel;
        document.getElementById("metaReviewDate").textContent = req.reviewedAt ? formatDate(req.reviewedAt) : "-";
        document.getElementById("reviewDateCol").style.display = "block";
    }

    // Show supervisor comment on rejected requests.
    if (status === "Rejected" && req.hrReason) {
        const card = document.getElementById("supervisorCommentsCard");
        document.getElementById("supervisorCommentText").textContent = req.hrReason;
        if (card) card.style.display = "flex";
    }
}

// True only for supervisor/admin roles (employee/intern cannot review).
function isReviewer() {
    try {
        const u = window.aiaAuth && window.aiaAuth.getUserInfo();
        if (!u) return false;
        const role = String(u.role
            || u["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
            || "").toLowerCase();
        const roleId = String(u.role_id || "");
        return role === "supervisor" || role === "admin" || roleId === "0";
    } catch (e) {
        return false;
    }
}

function renderStatusBadge(status) {
    const map = {
        Pending: { cls: "badge-light-pending", icon: "bi-clock", label: "Needs Approval" },
        Approved: { cls: "badge-light-approved", icon: "bi-check-circle", label: "Approved" },
        Rejected: { cls: "badge-light-rejected", icon: "bi-x-circle", label: "Rejected" }
    };
    const m = map[status];
    if (!m) return `<span class="badge badge-light-primary px-4 py-2 fs-7 fw-bold">${status}</span>`;
    return `<span class="badge ${m.cls} px-4 py-2 fs-7 fw-bold"><i class="bi ${m.icon} me-1"></i>${m.label}</span>`;
}

function renderChanges(changes) {
    const wrap = document.getElementById("changesContainer");
    if (!changes.length) {
        wrap.innerHTML = '<div class="text-muted">No field changes recorded.</div>';
        return;
    }
    
    wrap.innerHTML = changes.map(c => `
        <div class="mb-6">
            <div class="meta-label mb-2 text-uppercase" style="font-size: 11px; letter-spacing: 0.5px;">${escapeHtml(c.label)}</div>
            <div class="d-flex align-items-center">
                <span class="text-muted text-decoration-line-through me-3" style="font-size: 14px;">${escapeHtml(String(c.previous))}</span>
                <i class="bi bi-arrow-right text-muted me-3"></i>
                <span class="text-gray-900 fw-bold" style="font-size: 14px;">${escapeHtml(String(c.updated))}</span>
            </div>
        </div>
    `).join("");
}

// --- ACTION BUTTON LOGIC ---

function bindActionButtons() {
    const btnApprove = document.getElementById("btnApprove");
    const btnReject = document.getElementById("btnReject");
    const btnConfirmReject = document.getElementById("btnConfirmReject");

    const btnConfirmApprove = document.getElementById("btnConfirmApprove");

    if (btnApprove) {
        btnApprove.addEventListener("click", () => {
            const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById("approveModal"));
            modal.show();
        });
    }

    if (btnConfirmApprove) {
        btnConfirmApprove.addEventListener("click", () => {
            bootstrap.Modal.getOrCreateInstance(document.getElementById("approveModal")).hide();
            submitReview(true, null);
        });
    }

    if (btnReject) {
        btnReject.addEventListener("click", () => {
            const err = document.getElementById("rejectReasonError");
            if (err) err.style.display = "none";
            bootstrap.Modal.getOrCreateInstance(document.getElementById("rejectModal")).show();
        });
    }

    if (btnConfirmReject) {
        btnConfirmReject.addEventListener("click", () => {
            const reason = document.getElementById("rejectReasonInput").value.trim();
            const err = document.getElementById("rejectReasonError");
            if (!reason) {
                // Comment mandatory before rejecting.
                if (err) err.style.display = "block";
                return;
            }
            bootstrap.Modal.getOrCreateInstance(document.getElementById("rejectModal")).hide();
            submitReview(false, reason);
        });
    }
}

async function submitReview(isApproved, reason) {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    const payload = {
        requestId: currentRequestId,
        isApproved: isApproved,
        reason: reason
    };

    try {
        const response = await fetch(`${API_BASE}/api/employee/review-update`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            alert(`Request successfully ${isApproved ? 'approved' : 'rejected'}.`);
            window.location.href = "/EmployeeManagement/ManageRequestList";
        } else {
            const bodyText = await response.text();
            console.error(`review-update failed: ${response.status}`, bodyText);
            alert(`Failed to submit review (HTTP ${response.status}).\n${bodyText || ""}`);
        }
    } catch (error) {
        console.error("API Error during submission:", error);
        alert("A network error occurred.");
    }
}

// --- UTILS ---

function showError(msg) {
    document.getElementById("changesContainer").innerHTML = `<div class="text-danger fw-bold">${msg}</div>`;
}

function normalizeStatus(s) {
    if (s === null || s === undefined || s === "") return "Unknown";
    const v = String(s).toLowerCase();
    if (v === "pending" || v === "0" || v === "needs approval") return "Pending";
    if (v === "approved" || v === "1") return "Approved";
    if (v === "rejected" || v === "2") return "Rejected";
    return String(s);
}

function formatDate(dateStr) {
    if (!dateStr) return "-";
    const d = new Date(dateStr);
    if (isNaN(d)) return "-";
    return d.toLocaleString("en-GB", {
        day: "2-digit", month: "long", year: "numeric",
        hour: "2-digit", minute: "2-digit", hour12: true
    }).replace("am", "AM").replace("pm", "PM"); // Enforces uppercase AM/PM
}

function escapeHtml(s) {
    return String(s).replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}