// FR-0002 Profile — wired to real API (/api/employee/me)
// Fields not yet in backend DTO (Profile Picture, NPWP) are stored client-side as a stop-gap.

const API_BASE = "https://localhost:7089";
const LOCAL_OVERRIDE_KEY = "userProfileLocal"; // for fields not in API DTO

const GENDER_MAP = { Male: 0, Female: 1, 0: "Male", 1: "Female" };
const MARITAL_MAP = { Single: 0, Married: 1, 0: "Single", 1: "Married" };

let currentProfile = null;          // last loaded DTO
let localOverrides = JSON.parse(localStorage.getItem(LOCAL_OVERRIDE_KEY) || "{}");

// ────────────────────── HTTP helpers ──────────────────────
async function apiGet(endpoint) {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    if (!token) { window.aiaAuth && window.aiaAuth.signOut(); return null; }
    try {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (res.status === 401) { window.aiaAuth.signOut(); return null; }
        const json = await res.json();
        if (json.isError) {
            console.error("API error:", json.statusMessage);
            return null;
        }
        return json.content || json.data;
    } catch (err) {
        console.error("API GET failed:", err);
        return null;
    }
}

async function apiSend(endpoint, method, body) {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    if (!token) { window.aiaAuth && window.aiaAuth.signOut(); return { ok: false, message: "Not authenticated" }; }
    try {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            method,
            headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        if (res.status === 401) { window.aiaAuth.signOut(); return { ok: false, message: "Session expired" }; }
        const json = await res.json();
        return { ok: res.ok && !json.isError, message: json.statusMessage || "", data: json.content };
    } catch (err) {
        console.error("API send failed:", err);
        return { ok: false, message: "Network error. Check API is running on 7089." };
    }
}

// ────────────────────── helpers ──────────────────────
function setText(id, val) {
    const el = document.getElementById(id);
    if (el) el.textContent = (val === undefined || val === null || val === "") ? "-" : val;
}
function setVal(id, val) {
    const el = document.getElementById(id);
    if (el) el.value = (val === undefined || val === null) ? "" : val;
}
function maskNik(nik) {
    if (!nik) return "-";
    return nik.length > 4 ? "*".repeat(nik.length - 4) + nik.slice(-4) : nik;
}
function escapeHtml(s) {
    return String(s).replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}
function formatDate(d) {
    if (!d) return "";
    const dt = new Date(d);
    if (isNaN(dt)) return "";
    const dd = String(dt.getDate()).padStart(2, "0");
    const mm = String(dt.getMonth() + 1).padStart(2, "0");
    return `${dd}/${mm}/${dt.getFullYear()}`;
}
function parseDate(str) {
    if (!str) return null;
    const m = str.match(/^(\d{2})\/(\d{2})\/(\d{4})$/);
    if (!m) return new Date(str);
    return new Date(`${m[3]}-${m[2]}-${m[1]}T00:00:00Z`);
}
function getRoleFromJwt() {
    const u = window.aiaAuth && window.aiaAuth.getUserInfo();
    if (!u) return null;
    const role = u.role || u["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || "";
    if (String(role).toLowerCase() === "supervisor" || String(role) === "0") return "Supervisor";
    return "Intern";
}

// ────────────────────── VIEW PAGE ──────────────────────
async function renderProfileView() {
    if (!document.getElementById("fullName")) return;

    const dto = await apiGet('/api/employee/me');
    if (!dto) {
        setText("fullName", "Failed to load profile");
        return;
    }
    currentProfile = dto;

    // Profile Picture (local override)
    if (localOverrides.profilePicture) {
        const img = document.getElementById("profilePicView");
        const ph = document.getElementById("profilePicPlaceholder");
        if (img) { img.src = localOverrides.profilePicture; img.style.display = "block"; }
        if (ph) ph.style.display = "none";
    }

    setText("fullName", dto.fullName);
    setText("gender", dto.gender);
    setText("personalEmail", dto.personalEmail);
    setText("companyEmail", dto.employeeEmail);
    setText("phoneNumber", dto.phoneNumber);
    setText("nik", maskNik(dto.nik));
    setText("npwp", localOverrides.npwp || "-");
    setText("placeOfBirth", dto.placeOfBirth);
    setText("dateOfBirth", formatDate(dto.dateOfBirth));
    setText("maritalStatus", dto.maritalStatusName || dto.maritalStatus);

    setText("currentStreet", dto.currentStreetAddress);
    setText("currentCity", dto.currentCity);
    setText("currentProvince", dto.currentProvince);
    setText("currentPostal", dto.currentPostalCode);

    setText("residentialStreet", dto.residentialStreetAddress);
    setText("residentialCity", dto.residentialCity);
    setText("residentialProvince", dto.residentialProvince);
    setText("residentialPostal", dto.residentialPostalCode);

    const emp = dto.employmentInformation || {};
    const ec = dto.emergencyContact || {};

    setText("employeeId", emp.displayId);
    setText("employeeStatus", dto.isActive ? "Active" : "Inactive");
    setText("startDate", formatDate(emp.startDate));
    setText("employmentType", emp.typeName);
    setText("department", emp.departmentName);
    setText("position", emp.positionName);

    const role = getRoleFromJwt();
    setText("employeeRole", role);
    setText("supervisorName", emp.supervisorName);

    const supWrap = document.getElementById("supervisorNameWrapperView");
    if (supWrap && role === "Supervisor") supWrap.style.display = "none";

    setText("emergencyName", ec.name);
    setText("emergencyRelationship", ec.relationship);
    setText("emergencyPhone", ec.phoneNumber);

    renderAttachmentsView();
}

function renderAttachmentsView() {
    const wrap = document.getElementById("attachmentList");
    if (!wrap) return;
    const items = localOverrides.attachments || [];
    if (items.length === 0) {
        wrap.innerHTML = '<div class="text-muted fw-semibold fs-6">No attachments available yet.</div>';
        return;
    }
    wrap.innerHTML = items.map(a => `
        <div class="d-flex align-items-center gap-3 p-3 border rounded mb-2">
            <i class="bi bi-paperclip fs-3 text-brand"></i>
            <div class="flex-grow-1">
                <div class="fw-bold">${escapeHtml(a.name)}</div>
                <div class="text-muted small">${escapeHtml(a.fileName || "")}</div>
            </div>
        </div>
    `).join("");
}

// ────────────────────── EDIT PAGE ──────────────────────
async function loadEditForm() {
    if (!document.getElementById("editFullName")) return;

    const dto = await apiGet('/api/employee/me');
    if (!dto) {
        alert("Failed to load profile from API. Check API is running on 7089.");
        return;
    }
    currentProfile = dto;

    setVal("editFullName", dto.fullName);
    setVal("editPersonalEmail", dto.personalEmail);
    setVal("editCompanyEmail", dto.employeeEmail);
    setVal("editPhoneNumber", dto.phoneNumber);
    setVal("editNik", dto.nik);
    setVal("editNpwp", localOverrides.npwp || "");
    setVal("editPlaceOfBirth", dto.placeOfBirth);
    setVal("editDateOfBirth", formatDate(dto.dateOfBirth));
    setVal("editMaritalStatus", MARITAL_MAP[dto.maritalStatus] ?? "");

    document.querySelectorAll('input[name="gender"]').forEach(r => r.checked = (r.value === dto.gender));

    setVal("editResidentialStreet", dto.residentialStreetAddress);
    setVal("editResidentialCity", dto.residentialCity);
    setVal("editResidentialProvince", dto.residentialProvince);
    setVal("editResidentialPostal", dto.residentialPostalCode);

    setVal("editCurrentStreet", dto.currentStreetAddress);
    setVal("editCurrentCity", dto.currentCity);
    setVal("editCurrentProvince", dto.currentProvince);
    setVal("editCurrentPostal", dto.currentPostalCode);

    const emp = dto.employmentInformation || {};
    const ec = dto.emergencyContact || {};

    setVal("editEmployeeId", emp.displayId);
    setVal("editStartDate", formatDate(emp.startDate));
    setVal("editEmploymentType", emp.typeName);
    setVal("editDepartment", emp.departmentName);
    setVal("editPosition", emp.positionName);
    setVal("editSupervisorName", emp.supervisorName);

    const empStatus = dto.isActive ? "Active" : "Inactive";
    document.querySelectorAll('input[name="employeeStatus"]').forEach(r => r.checked = (r.value === empStatus));
    const role = getRoleFromJwt();
    document.querySelectorAll('input[name="employeeRole"]').forEach(r => r.checked = (r.value === role));

    if (role === "Supervisor") {
        const wrap = document.getElementById("supervisorNameWrapper");
        if (wrap) wrap.style.display = "none";
    }

    setVal("editEmergencyName", ec.name);
    setVal("editEmergencyRelationship", ec.relationship);
    setVal("editEmergencyPhone", ec.phoneNumber);

    if (localOverrides.profilePicture) {
        const wrap = document.getElementById("profilePicPreview");
        if (wrap) wrap.innerHTML = `<img src="${localOverrides.profilePicture}" alt="">`;
    }

    renderAttachmentsEdit();
}

function renderAttachmentsEdit() {
    const wrap = document.getElementById("attachmentList");
    if (!wrap) return;
    const items = localOverrides.attachments || [];
    if (items.length === 0) {
        wrap.innerHTML = '<div class="text-muted fw-semibold fs-6">No attachments yet.</div>';
        return;
    }
    wrap.innerHTML = items.map((a, i) => `
        <div class="attachment-item">
            <i class="bi bi-paperclip fs-3 text-brand"></i>
            <div class="flex-grow-1">
                <div class="fw-bold">${escapeHtml(a.name)}</div>
                <div class="text-muted small">${escapeHtml(a.fileName || "")}</div>
            </div>
            <i class="bi bi-trash attachment-remove" onclick="removeAttachment(${i})"></i>
        </div>
    `).join("");
}

function addAttachment() {
    const nameEl = document.getElementById("editAttachmentName");
    const fileEl = document.getElementById("editAttachmentFile");
    const name = nameEl.value.trim();
    const file = fileEl.files[0];
    if (!name || !file) {
        alert("Please provide both attachment name and file.");
        return;
    }
    if (!localOverrides.attachments) localOverrides.attachments = [];
    localOverrides.attachments.push({ name, fileName: file.name });
    localStorage.setItem(LOCAL_OVERRIDE_KEY, JSON.stringify(localOverrides));
    nameEl.value = "";
    fileEl.value = "";
    renderAttachmentsEdit();
}

function removeAttachment(idx) {
    localOverrides.attachments.splice(idx, 1);
    localStorage.setItem(LOCAL_OVERRIDE_KEY, JSON.stringify(localOverrides));
    renderAttachmentsEdit();
}

document.addEventListener("DOMContentLoaded", function () {
    if (typeof flatpickr !== "undefined" && document.getElementById("editDateOfBirth")) {
        flatpickr("#editDateOfBirth", { dateFormat: "d/m/Y", allowInput: true });
    }

    const sameChk = document.getElementById("sameAsResidential");
    if (sameChk) {
        sameChk.addEventListener("change", () => {
            if (sameChk.checked) {
                setVal("editCurrentStreet", document.getElementById("editResidentialStreet").value);
                setVal("editCurrentCity", document.getElementById("editResidentialCity").value);
                setVal("editCurrentProvince", document.getElementById("editResidentialProvince").value);
                setVal("editCurrentPostal", document.getElementById("editResidentialPostal").value);
            }
        });
    }

    const picInput = document.getElementById("editProfilePicture");
    if (picInput) {
        picInput.addEventListener("change", () => {
            const f = picInput.files[0];
            if (!f) return;
            const reader = new FileReader();
            reader.onload = e => {
                const wrap = document.getElementById("profilePicPreview");
                if (wrap) wrap.innerHTML = `<img src="${e.target.result}" alt="">`;
                localOverrides._pendingPic = e.target.result;
            };
            reader.readAsDataURL(f);
        });
    }
});

// ────────────────────── SUBMIT FOR APPROVAL ──────────────────────
function submitForApproval() {
    if (!validateEditForm()) return;
    const modal = new bootstrap.Modal(document.getElementById("confirmSubmitModal"));
    modal.show();
}

function validateEditForm() {
    let valid = true;
    document.querySelectorAll("[required]").forEach(f => {
        if (!f.value || f.value.trim() === "") {
            f.classList.add("is-invalid");
            valid = false;
        } else {
            f.classList.remove("is-invalid");
        }
    });
    if (!document.querySelector('input[name="gender"]:checked')) valid = false;
    if (!valid) alert("Please complete all required fields.");
    return valid;
}

async function confirmSubmission() {
    const dto = buildUpdateDto();
    const result = await apiSend('/api/employee/me', 'PUT', dto);

    // Persist local-only fields (NPWP, profile picture)
    const npwp = document.getElementById("editNpwp").value;
    if (npwp) localOverrides.npwp = npwp;
    if (localOverrides._pendingPic) {
        localOverrides.profilePicture = localOverrides._pendingPic;
        delete localOverrides._pendingPic;
    }
    localStorage.setItem(LOCAL_OVERRIDE_KEY, JSON.stringify(localOverrides));

    const modalEl = document.getElementById("confirmSubmitModal");
    const inst = bootstrap.Modal.getInstance(modalEl);
    if (inst) inst.hide();

    if (result.ok) {
        alert("Submitted. " + (result.message || "Profile updated."));
        window.location.href = "/Profile";
    } else {
        alert("Submit failed: " + (result.message || "Unknown error"));
    }
}

function buildUpdateDto() {
    const genderStr = document.querySelector('input[name="gender"]:checked')?.value || "Male";
    const maritalStr = document.getElementById("editMaritalStatus").value;
    const dobStr = document.getElementById("editDateOfBirth").value;
    const dob = parseDate(dobStr);

    return {
        fullName: document.getElementById("editFullName").value,
        gender: genderStr,
        personalEmail: document.getElementById("editPersonalEmail").value,
        placeOfBirth: document.getElementById("editPlaceOfBirth").value,
        nik: document.getElementById("editNik").value,
        dateOfBirth: dob ? dob.toISOString() : null,
        maritalStatus: maritalStr ? MARITAL_MAP[maritalStr] : null,

        currentStreetAddress: document.getElementById("editCurrentStreet").value,
        currentCity: document.getElementById("editCurrentCity").value,
        currentProvince: document.getElementById("editCurrentProvince").value,
        currentPostalCode: document.getElementById("editCurrentPostal").value,

        residentialStreetAddress: document.getElementById("editResidentialStreet").value,
        residentialCity: document.getElementById("editResidentialCity").value,
        residentialProvince: document.getElementById("editResidentialProvince").value,
        residentialPostalCode: document.getElementById("editResidentialPostal").value,

        phoneNumber: document.getElementById("editPhoneNumber").value,
        emergencyContactName: document.getElementById("editEmergencyName").value,
        emergencyContactPhone: document.getElementById("editEmergencyPhone").value,
        emergencyContactRelationship: document.getElementById("editEmergencyRelationship").value
    };
}

document.addEventListener("DOMContentLoaded", function () {
    renderProfileView();
    loadEditForm();
});
