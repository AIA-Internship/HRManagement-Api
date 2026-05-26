// FR-0003 Request Details — wired to /api/employee/me/requests
const API_BASE = "https://localhost:7089";

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
    const id = parseInt(idStr, 10);

    const [request, profile] = await Promise.all([fetchRequest(id), fetchProfile()]);
    if (!request) { showError("Request not found."); return; }
    renderDetails(request, profile);
});

async function fetchRequest(id) {
    let isSupervisor = false;
    try {
        const u = window.aiaAuth && window.aiaAuth.getUserInfo();
        if (u) {
            const role = u.role || u["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || "";
            const roleId = u.role_id || "";
            isSupervisor = (
                String(role).toLowerCase() === "supervisor" ||
                String(role) === "0" ||
                String(roleId) === "0"
            );
        }
    } catch (e) {
        console.warn("Could not retrieve user info for role check:", e);
    }
    
    const endpoint = isSupervisor ? '/api/employee/requests' : '/api/employee/my-requests';
    const list = await apiGet(endpoint);

    if (!list) return null;
    return list.find(r => r.requestId === id) || null;
}

async function fetchProfile() {
    return await apiGet('/api/employee/me');
}

async function apiGet(endpoint) {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    if (!token) { window.aiaAuth && window.aiaAuth.signOut(); return null; }
    try {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (res.status === 401) { window.aiaAuth.signOut(); return null; }
        if (res.status === 404) return null;
        const json = await res.json();
        if (json.isError) return null;
        return json.content || json.data || null;
    } catch (err) {
        console.error("API error:", err);
        return null;
    }
}

function renderDetails(req, profile) {
    const status = normalizeStatus(req.status);

    document.getElementById("metaId").textContent = "REQ" + String(req.requestId).padStart(3, "0");
    document.getElementById("metaStatus").innerHTML = renderStatusBadge(status);
    document.getElementById("metaSubmitted").textContent = formatDate(req.createdAt);

    const changes = collectChanges(req, profile, status);
    document.getElementById("metaCount").textContent = changes.length;

    if (status === "Approved" || status === "Rejected") {
        const row = document.getElementById("approvalMetaRow");
        row.style.display = "flex";
        if (status === "Rejected") {
            document.getElementById("approvalByLabel").textContent = "Rejected By";
            document.getElementById("approvalDateLabel").textContent = "Rejected Date";
            document.getElementById("commentsCol").style.display = "block";
            document.getElementById("metaComments").textContent = req.hrReason || "-";
        } else {
            document.getElementById("approvalByLabel").textContent = "Approved By";
            document.getElementById("approvalDateLabel").textContent = "Approved Date";
        }
        document.getElementById("metaActionBy").textContent = "Supervisor";
        document.getElementById("metaActionDate").textContent = "-";
    }

    renderChanges(changes);
}

function collectChanges(req, profile, status) {
    const fmtDate = v => {
        if (!v) return "";
        const d = new Date(v);
        if (isNaN(d)) return v;
        return `${String(d.getDate()).padStart(2, "0")}/${String(d.getMonth() + 1).padStart(2, "0")}/${d.getFullYear()}`;
    };
    // For pending requests, profile reflects the "before" state.
    // For approved requests, profile already reflects the new state, so previous can't be derived. Show "—".
    const showPrevious = status === "Pending";
    const previousMap = (showPrevious && profile) ? {
        newFullName: profile.fullName,
        newGender: profile.gender,
        newPersonalEmail: profile.personalEmail,
        newPlaceOfBirth: profile.placeOfBirth,
        newNik: profile.Nik,
        newDateOfBirth: fmtDate(profile.dateOfBirth),
        newMaritalStatus: profile.maritalStatus,
        newCurrentStreetAddress: profile.currentStreetAddress,
        newCurrentCity: profile.currentCity,
        newCurrentProvince: profile.currentProvince,
        newCurrentPostalCode: profile.currentPostalCode,
        newResidentialStreetAddress: profile.residentialStreetAddress,
        newResidentialCity: profile.residentialCity,
        newResidentialProvince: profile.residentialProvince,
        newResidentialPostalCode: profile.residentialPostalCode,
        newPhoneNumber: profile.phoneNumber,
        newEmergencyContactName: profile.emergencyContactName,
        newEmergencyContactPhone: profile.emergencyContactPhone,
        newEmergencyContactRelationship: profile.relationship
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

function renderChanges(changes) {
    const wrap = document.getElementById("changesContainer");
    if (!changes.length) {
        wrap.innerHTML = '<div class="text-muted">No field changes recorded.</div>';
        return;
    }
    wrap.innerHTML = changes.map(c => `
        <div class="change-row">
            <div class="meta-label mb-1">${escapeHtml(c.label)}</div>
            <div>
                <span class="change-prev">${escapeHtml(String(c.previous))}</span>
                <i class="bi bi-arrow-right mx-2 text-muted"></i>
                <span class="change-new">${escapeHtml(String(c.updated))}</span>
            </div>
        </div>
    `).join("");
}

function showError(msg) {
    document.getElementById("changesContainer").innerHTML = `<div class="text-danger">${msg}</div>`;
}

function normalizeStatus(s) {
    const v = String(s || "").toLowerCase();
    if (v === "pending" || v === "0" || v === "needs approval") return "Pending";
    if (v === "approved" || v === "1") return "Approved";
    if (v === "rejected" || v === "2") return "Rejected";
    return s || "Unknown";
}

function renderStatusBadge(status) {
    const map = {
        Pending: "badge-light-pending",
        Approved: "badge-light-approved",
        Rejected: "badge-light-rejected"
    };
    return `<span class="badge ${map[status] || "badge-light-primary"} px-4 py-2 fs-7 fw-bold">${status}</span>`;
}

function formatDate(dateStr) {
    if (!dateStr) return "-";
    const d = new Date(dateStr);
    if (isNaN(d)) return "-";
    return d.toLocaleString("en-GB", {
        day: "2-digit", month: "short", year: "numeric",
        hour: "2-digit", minute: "2-digit", hour12: true
    });
}

function escapeHtml(s) {
    return String(s).replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}
