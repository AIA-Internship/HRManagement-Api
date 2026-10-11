const API_BASE = "https://localhost:7089";
let selectedPlanId = null;
let selectedRole = null;

document.addEventListener("DOMContentLoaded", initializePage);

async function initializePage() {
    try {
        await loadPlans();
    } catch (error) {
        console.error("Failed to initialize Intern Performance page:", error);
        renderError("Failed to load performance review plans.");
    }
}

function getToken() {
    return window.aiaAuth ? window.aiaAuth.getToken() : null;
}

async function apiGet(url) {
    const token = getToken();

    if (!token) {
        if (window.aiaAuth) window.aiaAuth.signOut();
        throw new Error("Authentication token not found.");
    }

    const response = await fetch(`${API_BASE}${url}`, {
        method: "GET",
        headers: { "Accept": "application/json", "Authorization": `Bearer ${token}` }
    });

    if (response.status === 401) {
        if (window.aiaAuth) window.aiaAuth.signOut();
        throw new Error("Unauthorized.");
    }

    if (!response.ok) throw new Error(`API request failed: ${response.status}`);
    return await response.json();
}

async function loadPlans() {
    const result = await apiGet("/api/performance-review/plans/all");
    const plans = result.content ?? result ?? [];

    renderPlanDropdown(plans);

    if (!plans.length) {
        renderEmpty("No performance review plans found.");
        return;
    }

    const sortedPlans = [...plans].sort((a, b) => Number(b.id) - Number(a.id));
    const ongoingPlan = sortedPlans.find(plan => String(plan.status).toLowerCase() === "ongoing");
    const defaultPlan = ongoingPlan ?? sortedPlans[0];

    selectedPlanId = defaultPlan.id;
    document.getElementById("planFilter").value = String(selectedPlanId);

    await loadRoles(selectedPlanId);
}

function renderPlanDropdown(plans) {
    const select = document.getElementById("planFilter");
    select.innerHTML = "";

    plans.forEach(plan => {
        const option = document.createElement("option");
        option.value = plan.id;
        option.textContent = plan.name ?? `Plan ${plan.id}`;
        select.appendChild(option);
    });
}

async function loadRoles(planId) {
    const roleSelect = document.getElementById("roleFilter");

    roleSelect.disabled = true;
    roleSelect.innerHTML = `<option value="">Loading roles...</option>`;

    const result = await apiGet(`/api/performance-review/plans/${planId}/roles`);
    const roles = result.content ?? result ?? [];

    renderRoleDropdown(roles);

    if (!roles.length) {
        selectedRole = null;
        renderEmpty("No intern roles are available for this plan.");
        return;
    }

    selectedRole = roles[0];
    roleSelect.value = selectedRole;

    await loadRanking();
}

function renderRoleDropdown(roles) {
    const select = document.getElementById("roleFilter");
    select.innerHTML = "";

    roles.forEach(role => {
        const option = document.createElement("option");
        option.value = role;
        option.textContent = role;
        select.appendChild(option);
    });

    select.disabled = roles.length === 0;
}

async function loadRanking() {
    if (selectedPlanId === null || !selectedRole) return;

    showLoading();

    try {
        const result = await apiGet(`/api/intern-performance/${selectedPlanId}/ranking?role=${encodeURIComponent(selectedRole)}`);
        const ranking = result.content ?? result ?? [];
        renderRanking(ranking);
    } catch (error) {
        console.error("Failed to load intern ranking:", error);
        renderError("Failed to load intern performance ranking.");
    }
}

function renderRanking(ranking) {
    const tbody = document.getElementById("rankingTableBody");

    if (!Array.isArray(ranking) || ranking.length === 0) {
        renderEmpty("No intern performance data is available.");
        return;
    }

    tbody.innerHTML = ranking.map(intern => createRankingRow(intern)).join("");
}

function createRankingRow(intern) {
    const rank = intern.rank;
    const topRank = rank <= 3;
    const initials = getInitials(intern.fullName);

    return `
                <tr>
                    <td class="ps-5 ps-lg-8">
                        <span class="fw-bold fs-5 ${topRank ? "text-danger" : "text-gray-500"}">${String(rank).padStart(2, "0")}</span>
                    </td>

                    <td>
                        <div class="d-flex align-items-center gap-3">
                            <div class="intern-avatar rounded-circle bg-light d-flex align-items-center justify-content-center flex-shrink-0">
                                <span class="fw-bold text-gray-600 fs-7">${escapeHtml(initials)}</span>
                            </div>

                            <div>
                                <a href="/PerformanceReview/Supervisor/InternPerformance/Detail?planId=${selectedPlanId}&internId=${intern.internId}" class="text-gray-800 text-hover-primary fw-bold fs-6">
                                    ${escapeHtml(intern.fullName)}
                                </a>
                                <div class="text-muted text-uppercase fs-8 fw-semibold mt-1">${escapeHtml(intern.internRole)}</div>
                            </div>
                        </div>
                    </td>

                    <td class="text-center fs-6 text-gray-600">${formatScore(intern.techScore)}</td>
                    <td class="text-center fs-6 text-gray-600">${formatScore(intern.softSkillScore)}</td>
                    <td class="text-center fs-6 text-gray-600">${formatScore(intern.selfAssessmentScore)}</td>
                    <td class="text-center fs-6 text-gray-600">${formatScore(intern.peerReviewScore)}</td>

                    <td class="text-center pe-5 pe-lg-8">
                        <span class="fw-bold fs-6 text-gray-700">${formatScoreValue(intern.averageOverallScore)}</span>
                    </td>
                </tr>`;
}

function showLoading() {
    document.getElementById("rankingTableBody").innerHTML = `
                <tr>
                    <td colspan="7" class="text-center py-15 text-muted fs-7">
                        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
                        Loading intern performance...
                    </td>
                </tr>`;
}

function renderEmpty(message) {
    document.getElementById("rankingTableBody").innerHTML = `
                <tr>
                    <td colspan="7" class="text-center py-15">
                        <i class="bi bi-bar-chart-line fs-2x text-gray-400 d-block mb-3"></i>
                        <div class="fw-bold text-gray-700 fs-6 mb-1">No Performance Data</div>
                        <div class="text-muted fs-7">${escapeHtml(message)}</div>
                    </td>
                </tr>`;
}

function renderError(message) {
    document.getElementById("rankingTableBody").innerHTML = `
                <tr>
                    <td colspan="7" class="text-center py-15">
                        <i class="bi bi-exclamation-circle fs-2x text-danger d-block mb-3"></i>
                        <div class="fw-bold text-gray-700 fs-6 mb-1">Something went wrong</div>
                        <div class="text-muted fs-7">${escapeHtml(message)}</div>
                    </td>
                </tr>`;
}

document.getElementById("planFilter").addEventListener("change", async function () {
    selectedPlanId = Number(this.value);
    selectedRole = null;
    await loadRoles(selectedPlanId);
});

document.getElementById("roleFilter").addEventListener("change", async function () {
    selectedRole = this.value;
    await loadRanking();
});

function formatScore(value) {
    return value === null || value === undefined || value === "" ? "-" : formatScoreValue(value);
}

function formatScoreValue(value) {
    return value === null || value === undefined || value === "" ? "-" : Number(value).toFixed(2);
}

function getInitials(name) {
    if (!name) return "?";
    return name.trim().split(/\s+/).slice(0, 2).map(word => word.charAt(0).toUpperCase()).join("");
}

function escapeHtml(value) {
    if (value === null || value === undefined) return "";
    return String(value).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;");
}