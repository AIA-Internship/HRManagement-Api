const API_BASE = "https://localhost:7089";
const PLAN_ENDPOINT = "/api/performance-review/plans";
const VIEW_PLAN_ID = getPlanIdFromUrl();

let currentPlan = null;

const ASSESSMENT_META = {
    "self-assessment": { title: "Self Assessment", subtitle: "Individual reflection form", icon: "bi-person", iconClass: "bg-light-success", iconText: "text-success", sectionClass: "view-assessment-self" },
    "supervisor-assessment": { title: "Supervisor Assessment", subtitle: "Individual review form", icon: "bi-star", iconClass: "bg-light-warning", iconText: "text-warning", sectionClass: "view-assessment-supervisor" },
    "peer-review": { title: "Peer Review", subtitle: "Colleague evaluation module", icon: "bi-people", iconClass: "bg-light-danger", iconText: "text-danger", sectionClass: "view-assessment-peer" }
};

document.addEventListener("DOMContentLoaded", initializePage);

async function initializePage() {
    showLoading(true);
    if (!VIEW_PLAN_ID) { showLoading(false); showMessage("Invalid performance review plan.", "danger"); return; }
    try {
        const result = await apiRequest(`${PLAN_ENDPOINT}/${VIEW_PLAN_ID}`);
        currentPlan = result.content ?? result;
        if (!currentPlan || Number(currentPlan.id) !== Number(VIEW_PLAN_ID)) throw new Error("Performance review plan not found.");
        renderPlan(currentPlan);
    } catch (error) {
        console.error("Failed to load performance review plan:", error);
        showMessage(error.message || "Unable to load performance review plan.", "danger");
    } finally { showLoading(false); }
}

function getPlanIdFromUrl() {
    const root = document.getElementById("viewPlannerPage");
    const dataPlanId = root?.dataset.planId;
    if (dataPlanId) return Number(dataPlanId);
    return Number(new URLSearchParams(window.location.search).get("planId")) || null;
}

function getToken() { return window.aiaAuth ? window.aiaAuth.getToken() : null; }

async function apiRequest(url, options = {}) {
    const token = getToken();
    if (!token) { if (window.aiaAuth) window.aiaAuth.signOut(); throw new Error("Authentication token not found."); }
    const response = await fetch(`${API_BASE}${url}`, { ...options, headers: { Accept: "application/json", ...options.headers, Authorization: `Bearer ${token}` } });
    if (response.status === 401) { if (window.aiaAuth) window.aiaAuth.signOut(); throw new Error("Unauthorized."); }
    let result = {};
    try { result = await response.json(); } catch { if (!response.ok) throw new Error(`API request failed: ${response.status}`); }
    if (!response.ok || result.isError) throw new Error(result.statusMessage || `API request failed: ${response.status}`);
    return result;
}

function renderPlan(plan) {
    setText("planTitle", plan.name || "Performance Plan");
    renderStatus(plan.status);
    setText("periodType", formatPeriodType(plan.periodType));
    setText("startDate", formatDate(plan.startDate));
    setText("endDate", formatDate(plan.endDate));
    setText("duration", `${Number(plan.durationInMonth) || 0} months`);
    setText("minReviewDuration", `${Number(plan.minReviewDurationInDays) || 0} days`);
    renderAssessments(plan);
    renderScoreWeights(plan.scoreWeightConfigurations ?? []);
}

function renderStatus(status) {
    const element = document.getElementById("planStatus");
    if (!element) return;
    const value = String(status ?? "").trim();
    element.textContent = value ? formatStatus(value) : "";
    element.className = `view-plan-status ${getStatusClass(value)}`;
}

function getStatusClass(status) {
    const normalized = String(status ?? "").toLowerCase();
    if (normalized === "ongoing") return "view-status-ongoing";
    if (normalized === "done" || normalized === "completed") return "view-status-completed";
    if (normalized === "drafted" || normalized === "draft") return "view-status-drafted";
    return "view-status-default";
}

function renderAssessments(plan) {
    const container = document.getElementById("assessmentList");
    if (!container) return;

    const sections = [];
    (plan.selfAssessments ?? []).forEach(item => sections.push(renderAssessment("self-assessment", item, sections.length)));
    (plan.supervisorAssessments ?? []).forEach(item => sections.push(renderAssessment("supervisor-assessment", item, sections.length)));
    (plan.peerReviews ?? []).forEach(item => sections.push(renderAssessment("peer-review", item, sections.length)));

    container.innerHTML = sections.length ? sections.join("") : `<div class="view-empty">No assessment configuration found for this plan.</div>`;
}

function renderAssessment(type, item, index) {
    const meta = ASSESSMENT_META[type] ?? ASSESSMENT_META["peer-review"];
    const questions = item.questions ?? [];
    const receivers = item.receivers ?? [];
    const groups = item.groups ?? [];
    const assessmentId = `view-assessment-${type}-${Number(item.assessmentId) || index}`;
    const questionHtml = questions.length ? questions.map((question, questionIndex) => `
        <div class="view-question-row">
            <div class="view-question-number">${String(questionIndex + 1).padStart(2, "0")}</div>
            <div class="flex-grow-1 min-w-0">
                <div class="view-question-text">${escapeHtml(question.questionText || "-")}</div>
                <div class="view-question-type">${formatQuestionType(question.questionType || item.answerType)}</div>
            </div>
        </div>
    `).join("") : cardEmpty("No questions configured.");

    const details = type === "peer-review" ? renderGroups(groups) : renderReceivers(receivers);

    return `
        <div class="view-assessment-card ${meta.sectionClass}">
            <button type="button" class="view-assessment-header" onclick="toggleViewAssessment('${assessmentId}')" aria-expanded="true">
                <span class="d-flex align-items-center gap-3 min-w-0">
                    <span class="view-assessment-icon ${meta.iconClass}"><i class="bi ${meta.icon} ${meta.iconText}"></i></span>
                    <span class="min-w-0">
                        <span class="d-block fw-bold text-gray-800 fs-6">${meta.title} ${index + 1}</span>
                        <span class="d-block text-muted fs-8 mt-1">${meta.subtitle}</span>
                    </span>
                </span>
                <span class="d-flex align-items-center gap-3 ms-3 flex-shrink-0">
                    <span class="view-assessment-count">${questions.length} question${questions.length === 1 ? "" : "s"}</span>
                    <i id="${assessmentId}-icon" class="bi bi-chevron-up text-muted fs-6"></i>
                </span>
            </button>

            <div id="${assessmentId}" class="view-assessment-body is-open">
                <div class="row g-4 mb-5">
                    <div class="col-md-4"><div class="view-detail-card"><div class="view-detail-label">Role</div><div class="view-detail-value">${escapeHtml(item.role || "-")}</div></div></div>
                    <div class="col-md-4"><div class="view-detail-card"><div class="view-detail-label">Answer Type</div><div class="view-detail-value">${formatQuestionType(item.answerType)}</div></div></div>
                    <div class="col-md-4"><div class="view-detail-card"><div class="view-detail-label">Participants</div><div class="view-detail-value">${type === "peer-review" ? `${groups.reduce((sum, group) => sum + (group.members?.length || 0), 0)} group members` : `${receivers.length} receiver${receivers.length === 1 ? "" : "s"}`}</div></div></div>
                </div>

                <div class="view-subsection mb-5">
                    <div class="view-subsection-header">Rating Description</div>
                    <div class="view-description">${escapeHtml(item.ratingDescription || "No rating description provided.")}</div>
                </div>

                <div class="view-subsection mb-5">
                    <div class="view-subsection-header">Questions</div>
                    <div class="view-question-list">${questionHtml}</div>
                </div>

                <div class="view-subsection">
                    <div class="view-subsection-header">${type === "peer-review" ? "Peer Review Groups" : "Receivers"}</div>
                    ${details}
                </div>
            </div>
        </div>
    `;
}

function renderReceivers(receivers) {
    if (!receivers.length) return cardEmpty("No receivers configured.");
    return `<div class="view-employee-grid">${receivers.map(employee => renderEmployee(employee)).join("")}</div>`;
}

function renderGroups(groups) {
    if (!groups.length) return cardEmpty("No peer review groups configured.");
    return groups.map((group, index) => `
        <div class="view-group-card">
            <div class="d-flex align-items-start justify-content-between gap-3 mb-4">
                <div>
                    <div class="view-group-name">${escapeHtml(group.name || `Group ${index + 1}`)}</div>
                    ${group.description ? `<div class="view-group-description">${escapeHtml(group.description)}</div>` : ""}
                </div>
                <span class="view-group-count">${group.members?.length || 0} members</span>
            </div>
            <div class="view-employee-grid">${(group.members ?? []).map(member => renderEmployee({ fullName: member.fullName, employeeDisplayId: member.displayId, position: "" })).join("") || cardEmpty("No members configured.")}</div>
        </div>
    `).join("");
}

function renderEmployee(employee) {
    const name = employee.fullName || "Unknown Employee";
    const displayId = employee.employeeDisplayId || employee.displayId || "-";
    const meta = [employee.position, employee.department].filter(Boolean).join(" | ");
    return `
        <div class="view-employee-card">
            <div class="view-employee-avatar">${escapeHtml(getInitials(name))}</div>
            <div class="min-w-0 flex-grow-1">
                <div class="view-employee-name text-truncate">${escapeHtml(name)}</div>
                <div class="view-employee-meta text-truncate">${escapeHtml(displayId)}${meta ? ` | ${escapeHtml(meta)}` : ""}</div>
            </div>
        </div>
    `;
}

function renderScoreWeights(configurations) {
    const container = document.getElementById("scoreWeightList");
    if (!container) return;
    if (!configurations.length) { container.innerHTML = cardEmpty("No score weight configuration found."); return; }

    container.innerHTML = `
        <div class="view-weight-toolbar">
            <div>
                <label class="form-label fs-8 fw-bold text-muted text-uppercase">Intern Position</label>
                <select id="viewWeightRole" class="form-select form-select-solid fs-7" onchange="selectViewWeight(this.value)">
                    ${configurations.map((config, index) => `<option value="${index}">${escapeHtml(config.jobTitle || "Unknown Position")}</option>`).join("")}
                </select>
            </div>
        </div>
        <div id="viewWeightContent"></div>
    `;
    renderSelectedWeight(configurations, 0);
}

function selectViewWeight(index) { renderSelectedWeight(currentPlan?.scoreWeightConfigurations ?? [], Number(index)); }

function renderSelectedWeight(configurations, index) {
    const container = document.getElementById("viewWeightContent");
    if (!container) return;
    const config = configurations[index];
    if (!config) { container.innerHTML = cardEmpty("No score weight configuration found."); return; }

    const values = { technical: 0, "soft-skill": 0, "self-assessment": 0, "peer-review": 0 };
    (config.scores ?? []).forEach(item => { const key = normalizeScoreType(item.scoreType); if (key in values) values[key] = Number(item.weight) || 0; });
    const total = Object.values(values).reduce((sum, value) => sum + value, 0);

    container.innerHTML = `
        <div class="view-weight-card">
            <div class="view-weight-header">${escapeHtml(config.jobTitle || "Unknown Position")}</div>
            <div class="view-weight-grid">
                ${renderWeightItem("Technical", values.technical)}
                ${renderWeightItem("Soft Skill", values["soft-skill"])}
                ${renderWeightItem("Self Assessment", values["self-assessment"])}
                ${renderWeightItem("Peer Review", values["peer-review"])}
            </div>
            <div class="view-weight-total">
                <span class="view-weight-total-label">Total Weight</span>
                <span class="view-weight-total-value ${Math.abs(total - 100) < 0.01 ? "is-valid" : "is-invalid"}">${formatWeight(total)}%</span>
            </div>
        </div>
    `;
}

function renderWeightItem(label, value) { return `<div class="view-weight-item"><div class="view-weight-label">${label}</div><div class="view-weight-value">${formatWeight(value)}%</div></div>`; }
function normalizeScoreType(value) { return String(value ?? "").trim().toLowerCase().replace(/[ _-]/g, ""); }
function formatWeight(value) { return Number(value).toFixed(2).replace(/\.00$/, " ").trim(); }
function formatPeriodType(value) { const text = String(value ?? "-"); return text ? text.charAt(0).toUpperCase() + text.slice(1).toLowerCase() : "-"; }
function formatStatus(value) { const text = String(value ?? "").trim(); return text ? text.charAt(0).toUpperCase() + text.slice(1).toLowerCase() : ""; }
function formatQuestionType(value) { const normalized = normalizeScoreType(value); if (normalized.includes("freetext")) return "Free Text"; if (normalized.includes("rating")) return "Rating"; return value ? String(value) : "-"; }

function formatDate(value) {
    if (!value) return "-";
    const raw = String(value);
    const match = raw.match(/^(\d{4})-(\d{2})-(\d{2})/);
    if (match) return `${match[3]}/${match[2]}/${match[1]}`;
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "-";
    return new Intl.DateTimeFormat("en-GB", { day: "2-digit", month: "2-digit", year: "numeric" }).format(date);
}

function getInitials(name) { return String(name ?? "").trim().split(/\s+/).slice(0, 2).map(word => word.charAt(0).toUpperCase()).join("") || "?"; }
function setText(id, value) { const element = document.getElementById(id); if (element) element.textContent = value; }
function cardEmpty(message) { return `<div class="view-empty">${escapeHtml(message)}</div>`; }

function toggleViewAssessment(id) {
    const body = document.getElementById(id);
    const icon = document.getElementById(`${id}-icon`);
    const header = body?.previousElementSibling;
    if (!body) return;
    const open = body.classList.toggle("is-open");
    if (icon) { icon.classList.toggle("bi-chevron-up", open); icon.classList.toggle("bi-chevron-down", !open); }
    if (header) header.setAttribute("aria-expanded", String(open));
}

function showLoading(show) {
    const overlay = document.getElementById("viewPlannerLoading");
    if (!overlay) return;
    overlay.classList.toggle("d-none", !show);
    document.body.classList.toggle("view-planner-loading-active", show);
}

function showMessage(message, type) {
    const element = document.getElementById("messageContainer");
    if (!element) return;
    element.className = `alert alert-${type} mb-5`;
    element.innerHTML = message;
    element.classList.remove("d-none");
}

function escapeHtml(value) {
    if (value === null || value === undefined) return "";
    return String(value).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;");
}
