const API_BASE = "https://localhost:7089";

let plans = [];
let deletePlanId = null;
let activatePlanId = null;

function openActivatePlanModal(planId, planName) {
    activatePlanId = Number(planId);

    const modal = document.getElementById("activatePlanModal");
    const message = document.getElementById("activatePlanMessage");
    const error = document.getElementById("activatePlanError");

    if (message) {
        message.textContent =
            `"${planName}" will become the new ongoing performance review plan.`;
    }

    if (error) {
        error.textContent = "";
        error.classList.add("d-none");
    }

    bootstrap.Modal.getOrCreateInstance(modal).show();
}

async function activatePlan() {
    if (!activatePlanId) {
        return;
    }

    const button =
        document.getElementById("activatePlanButton");

    try {
        if (button) {
            button.disabled = true;
        }

        await apiPost(
            `/api/performance-review/plans/ongoing/${activatePlanId}`,
            {}
        );

        bootstrap.Modal.getInstance(
            document.getElementById("activatePlanModal")
        )?.hide();

        activatePlanId = null;

        await loadPlans();
    } catch (error) {
        console.error(
            "Failed to activate performance review plan:",
            error
        );

        const errorElement =
            document.getElementById("activatePlanError");

        if (errorElement) {
            errorElement.textContent =
                error.message ||
                "Performance review plan cannot be activated.";

            errorElement.classList.remove("d-none");
        }
    }
}

document.addEventListener("DOMContentLoaded", initializePage);

async function initializePage() {
    try {
        await loadPlans();
    } catch (error) {
        console.error("Failed to initialize Planner page:", error);
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
    const data = result.content ?? result ?? [];

    if (!Array.isArray(data) || data.length === 0) {
        plans = [];
        renderEmpty("No performance review plans have been created yet.");
        return;
    }

    plans = [...data].sort((a, b) => Number(b.id) - Number(a.id));
    renderPlans(plans);
}

function renderPlans(plans) {
    const groups = [
        { status: "drafted", title: "Drafted Plans" },
        { status: "ongoing", title: "Ongoing Plans" },
        { status: "done", title: "Completed Plans" }
    ];

    const html = groups
        .map(group => {
            const items = plans.filter(
                plan => String(plan.status ?? "").toLowerCase() === group.status
            );

            if (!items.length) return "";

            return `
                <div class="mb-8">
                    <div class="d-flex align-items-center gap-3 mb-4">
                        <h3 class="fw-bold text-gray-800 fs-6 mb-0">
                            ${group.title}
                        </h3>

                        <span class="badge badge-light-secondary fw-bold fs-8">
                            ${items.length}
                        </span>
                    </div>

                    <div class="d-flex flex-column gap-2">
                        ${items.map(createPlanCard).join("")}
                    </div>
                </div>
            `;
        })
        .join("");

    document.getElementById("plannerList").innerHTML = html;
}

function createPlanCard(plan) {
    const status = String(plan.status ?? "").toLowerCase();
    const isDrafted = status === "drafted";

    return `
                <div class="card card-flush shadow-sm">
                    <div class="card-body d-flex flex-column flex-lg-row align-items-start align-items-lg-center justify-content-between gap-4 px-5 px-lg-6 py-5">
                        <div class="min-w-0">
                            <div class="fw-bold text-gray-900 fs-5 mb-2">${escapeHtml(plan.name ?? `Plan ${plan.id}`)}</div>
                            <div class="text-muted fs-7">${formatPeriodType(plan.periodType)} <span class="mx-1">|</span> ${formatDateRange(plan.startDate, plan.endDate)} <span class="mx-1">|</span> ${formatDuration(plan.periodType, plan.durationInMonth)}</div>
                        </div>

                        <div class="d-flex align-items-center gap-4 flex-shrink-0">
                            ${createStatusBadge(status)}
                            ${isDrafted ? createDraftActions(plan.id, plan.name ?? `Plan ${plan.id}`) : createViewAction(plan.id)}
                        </div>
                    </div>
                </div>
            `;
}

function createStatusBadge(status) {
    const config = {
        drafted: { text: "DRAFTED", className: "badge-light-secondary" },
        ongoing: { text: "ONGOING", className: "badge-light-warning" },
        done: { text: "DONE", className: "badge-light-success" }
    };

    const current = config[status] ?? { text: status.toUpperCase(), className: "badge-light-secondary" };

    return `<span class="badge ${current.className} fw-bold fs-8 px-3 py-2">${escapeHtml(current.text)}</span>`;
}

function createDraftActions(planId, planName) {
    return `
        <a href="/PerformanceReview/Supervisor/Planner/Edit?planId=${planId}"
           class="btn btn-icon btn-sm btn-light"
           title="Edit">
            <i class="bi bi-pencil-square fs-6 text-muted"></i>
        </a>

        <button type="button"
                class="btn btn-icon btn-sm btn-light"
                title="Activate"
                onclick="openActivatePlanModal(${planId}, '${escapeJs(planName)}')">
            <i class="bi bi-play-circle fs-6 text-success"></i>
        </button>

        <button type="button"
                class="btn btn-icon btn-sm btn-light"
                title="Delete"
                onclick="openDeletePlanModal(${planId}, '${escapeJs(planName)}')">
            <i class="bi bi-trash fs-6 text-muted"></i>
        </button>
    `;
}

function escapeJs(value) {
    return String(value ?? "")
        .replace(/\\/g, "\\\\")
        .replace(/'/g, "\\'")
        .replace(/"/g, '\\"')
        .replace(/\r/g, "\\r")
        .replace(/\n/g, "\\n");
}

function createViewAction(planId) {
    return `
                <a href="/PerformanceReview/Supervisor/Planner/View?planId=${planId}" class="btn btn-icon btn-sm btn-light" title="View">
                    <i class="bi bi-eye fs-6 text-muted"></i>
                </a>
            `;
}

function formatPeriodType(periodType) {
    if (!periodType) return "-";

    const value = String(periodType).toLowerCase();

    if (value === "quarterly") return "Quarter";
    if (value === "monthly") return "Monthly";

    return periodType;
}

function formatDateRange(startDate, endDate) {
    return `${formatDate(startDate)} - ${formatDate(endDate)}`;
}

function formatDate(value) {
    if (!value) return "-";

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) return "-";

    return date.toLocaleDateString("en-US", {
        month: "long",
        year: "numeric"
    });
}

function formatDuration(periodType, durationInMonth) {
    if (!durationInMonth) return "-";

    const value = String(periodType ?? "").toLowerCase();

    if (value === "quarterly") {
        const quarters = durationInMonth / 3;
        return `${quarters} ${quarters === 1 ? "quarter" : "quarters"}`;
    }

    return `${durationInMonth} ${durationInMonth === 1 ? "month" : "months"}`;
}

function renderEmpty(message) {
    document.getElementById("plannerList").innerHTML = `
                <div class="card card-flush shadow-sm">
                    <div class="card-body text-center py-15">
                        <i class="bi bi-calendar-x fs-2x text-gray-400 d-block mb-3"></i>
                        <div class="fw-bold text-gray-700 fs-6 mb-1">No Performance Review Plans</div>
                        <div class="text-muted fs-7">${escapeHtml(message)}</div>
                    </div>
                </div>
            `;
}

function renderError(message) {
    document.getElementById("plannerList").innerHTML = `
                <div class="card card-flush shadow-sm">
                    <div class="card-body text-center py-15">
                        <i class="bi bi-exclamation-circle fs-2x text-danger d-block mb-3"></i>
                        <div class="fw-bold text-gray-700 fs-6 mb-1">Something went wrong</div>
                        <div class="text-muted fs-7">${escapeHtml(message)}</div>
                    </div>
                </div>
            `;
}

function escapeHtml(value) {
    if (value === null || value === undefined) return "";
    return String(value).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;");
}

function goToCreatePlan() {
    window.location.href = "/PerformanceReview/Supervisor/Planner/Create";
}

function openCopyPlanModal() {
    const sourceSelect = document.getElementById("copySourcePlan");

    sourceSelect.innerHTML = `
        <option value="">Select plan to copy</option>
        ${plans.map(plan => `
            <option value="${plan.id}">
                ${escapeHtml(plan.name ?? `Plan ${plan.id}`)}
            </option>
        `).join("")}
    `;

    resetCopyPlanForm();

    bootstrap.Modal.getOrCreateInstance(
        document.getElementById("copyPlanModal")
    ).show();
}


function handleCopyPeriodChange() {
    clearCopyErrors();

    const periodType = document.getElementById("copyPeriodType").value;
    const help = document.getElementById("copyDurationHelp");

    if (periodType === "Monthly") {
        help.textContent =
            "Monthly plans can use any positive whole number of months.";
    } else if (periodType === "Quarterly") {
        help.textContent =
            "Quarterly plans must use 3, 6, 9, 12 months, or another duration divisible by 3.";
    } else {
        help.textContent =
            "Select a period type to see the duration rules.";
    }

    handleCopyDateChange();
}

function handleCopyDateChange() {
    clearCopyFieldErrors();

    const periodType = document.getElementById("copyPeriodType").value;
    const startValue = document.getElementById("copyStartDate").value;
    const duration = Number(
        document.getElementById("copyDuration").value
    );

    const preview = document.getElementById("copyEndDatePreview");
    const previewText =
        document.getElementById("copyEndDatePreviewText");

    preview.classList.add("d-none");
    previewText.textContent = "";

    if (!periodType || !startValue || !duration || duration < 1) {
        return;
    }

    const startDate = new Date(`${startValue}T00:00:00`);

    if (startDate.getDate() !== 1) {
        showCopyFieldError(
            "copyStartDateError",
            "Start date must be the first day of a month."
        );
        return;
    }

    if (periodType === "Quarterly" && duration % 3 !== 0) {
        showCopyFieldError(
            "copyDurationError",
            "Quarterly duration must be divisible by 3 months."
        );
        return;
    }

    const endDate = new Date(startDate);

    endDate.setMonth(endDate.getMonth() + duration);
    endDate.setDate(endDate.getDate() - 1);

    previewText.textContent =
        `This review plan will end on ${formatCopyDate(endDate)}.`;

    preview.classList.remove("d-none");
}
function formatCopyDate(date) {
    const day = String(date.getDate()).padStart(2, "0");
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const year = date.getFullYear();

    return `${day}/${month}/${year}`;
}

function clearCopyFieldErrors() {
    [
        "copyPlanNameError",
        "copyPeriodTypeError",
        "copyStartDateError",
        "copyDurationError"
    ].forEach(id => {
        const element = document.getElementById(id);

        if (!element) return;

        element.textContent = "";
        element.classList.add("d-none");
    });
}

async function copyPlan() {
    clearCopyErrors();

    const sourcePlanId = Number(
        document.getElementById("copySourcePlan").value
    );

    const name =
        document.getElementById("copyPlanName").value.trim();

    const periodType =
        document.getElementById("copyPeriodType").value;

    const startValue =
        document.getElementById("copyStartDate").value;

    const duration =
        Number(document.getElementById("copyDuration").value);

    let isValid = true;

    if (!sourcePlanId) {
        showCopyFieldError(
            "copySourcePlanError",
            "Please select a source plan."
        );
        isValid = false;
    }

    if (!name) {
        showCopyFieldError(
            "copyPlanNameError",
            "Plan name is required."
        );
        isValid = false;
    }

    if (!periodType) {
        showCopyFieldError(
            "copyPeriodTypeError",
            "Period type is required."
        );
        isValid = false;
    }

    if (!startValue) {
        showCopyFieldError(
            "copyStartDateError",
            "Start date is required."
        );
        isValid = false;
    }

    if (!Number.isInteger(duration) || duration < 1) {
        showCopyFieldError(
            "copyDurationError",
            "Duration must be a positive whole number."
        );
        isValid = false;
    }

    const startDate = startValue
        ? new Date(`${startValue}T00:00:00`)
        : null;

    if (startDate && startDate.getDate() !== 1) {
        showCopyFieldError(
            "copyStartDateError",
            "Start date must be the first day of a month."
        );
        isValid = false;
    }

    if (
        periodType === "Quarterly" &&
        Number.isInteger(duration) &&
        duration % 3 !== 0
    ) {
        showCopyFieldError(
            "copyDurationError",
            "Quarterly duration must be divisible by 3 months."
        );
        isValid = false;
    }

    if (!isValid) {
        return;
    }

    const endDate = new Date(startDate);

    endDate.setMonth(endDate.getMonth() + duration);
    endDate.setDate(endDate.getDate() - 1);

    const payload = {
        name,
        periodType,
        startDate: formatApiDate(startDate),
        endDate: formatApiDate(endDate),
        durationInMonth: duration
    };

    const button = document.getElementById("copyPlanButton");

    try {
        setButtonLoading(button, true);

        await apiPost(
            `/api/performance-review/plans/copy/${sourcePlanId}`,
            payload
        );

        bootstrap.Modal.getInstance(
            document.getElementById("copyPlanModal")
        )?.hide();

        await loadPlans();
    } catch (error) {
        console.error("Failed to copy performance review plan:", error);

        showCopyError(
            error.message ||
            "Failed to copy performance review plan."
        );
    } finally {
        setButtonLoading(button, false);
    }
}



async function apiPost(url, body) {
    const token = getToken();

    if (!token) {
        if (window.aiaAuth) window.aiaAuth.signOut();
        throw new Error("Authentication token not found.");
    }

    const response = await fetch(`${API_BASE}${url}`, {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify(body)
    });

    if (response.status === 401) {
        if (window.aiaAuth) window.aiaAuth.signOut();
        throw new Error("Unauthorized.");
    }

    let result;

    try {
        result = await response.json();
    } catch {
        throw new Error(`API request failed: ${response.status}`);
    }

    if (!response.ok || result.isError) {
        throw new Error(
            result.statusMessage ||
            `API request failed: ${response.status}`
        );
    }

    return result;
}

function resetCopyPlanForm() {
    document.getElementById("copySourcePlan").value = "";
    document.getElementById("copyPlanName").value = "";
    document.getElementById("copyPeriodType").value = "";
    document.getElementById("copyStartDate").value = "";
    document.getElementById("copyDuration").value = "";

    document.getElementById("copyEndDatePreview").classList.add("d-none");
    document.getElementById("copyEndDatePreviewText").textContent = "";

    clearCopyErrors();
}

function formatApiDate(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");

    return `${year}-${month}-${day}`;
}


function setButtonLoading(button, loading) {
    if (!button) return;

    const label = button.querySelector(".indicator-label");
    const progress = button.querySelector(".indicator-progress");

    button.disabled = loading;

    if (label) {
        label.classList.toggle("d-none", loading);
    }

    if (progress) {
        progress.classList.toggle("d-none", !loading);
    }
}

function showPlannerMessage(message, type) {
    let element = document.getElementById("messageContainer");

    if (!element) {
        element = document.createElement("div");
        element.id = "messageContainer";
        element.className = "alert mb-5";

        const plannerList = document.getElementById("plannerList");

        if (plannerList) {
            plannerList.parentNode.insertBefore(element, plannerList);
        }
    }

    element.className = `alert alert-${type} mb-5`;
    element.textContent = message;
    element.classList.remove("d-none");

    window.scrollTo({
        top: 0,
        behavior: "smooth"
    });
}


async function apiDelete(url) {
    const token = getToken();

    if (!token) {
        if (window.aiaAuth) window.aiaAuth.signOut();
        throw new Error("Authentication token not found.");
    }

    const response = await fetch(`${API_BASE}${url}`, {
        method: "DELETE",
        headers: {
            "Accept": "application/json",
            "Authorization": `Bearer ${token}`
        }
    });

    if (response.status === 401) {
        if (window.aiaAuth) window.aiaAuth.signOut();
        throw new Error("Unauthorized.");
    }

    let result = null;

    try {
        result = await response.json();
    } catch {
        if (!response.ok) {
            throw new Error(`API request failed: ${response.status}`);
        }
    }

    if (!response.ok || result?.isError) {
        throw new Error(
            result?.statusMessage ||
            `API request failed: ${response.status}`
        );
    }

    return result;
}


function openDeletePlanModal(planId, planName) {
    deletePlanId = Number(planId);

    const modal = document.getElementById("deletePlanModal");
    const message = document.getElementById("deletePlanMessage");
    const error = document.getElementById("deletePlanError");

    if (message) {
        message.textContent =
            `"${planName}" will be deleted permanently.`;
    }

    if (error) {
        error.textContent = "";
        error.classList.add("d-none");
    }

    const button = document.getElementById("deletePlanButton");
    setButtonLoading(button, false);

    bootstrap.Modal.getOrCreateInstance(modal).show();
}


async function deletePlan() {
    if (!deletePlanId) {
        return;
    }

    const button = document.getElementById("deletePlanButton");

    try {
        if (button) {
            button.disabled = true;
        }

        await apiDelete(
            `/api/performance-review/plans/${deletePlanId}`
        );

        bootstrap.Modal.getInstance(
            document.getElementById("deletePlanModal")
        )?.hide();

        showPlannerMessage(
            "Performance review plan deleted successfully.",
            "success"
        );

        deletePlanId = null;

        await loadPlans();
    } catch (error) {
        console.error("Failed to delete performance review plan:", error);

        const errorElement =
            document.getElementById("deletePlanError");

        if (errorElement) {
            errorElement.textContent =
                error.message ||
                "Failed to delete performance review plan.";

            errorElement.classList.remove("d-none");
        }
    } finally {
        if (button) {
            button.disabled = false;
        }
    }

}

function showCopyError(message) {
    const element = document.getElementById("copyPlanError");

    if (!element) return;

    element.textContent = message;
    element.classList.remove("d-none");
}

function clearCopyErrors() {
    [
        "copyPlanError",
        "copySourcePlanError",
        "copyPlanNameError",
        "copyPeriodTypeError",
        "copyStartDateError",
        "copyDurationError"
    ].forEach(id => {
        const element = document.getElementById(id);

        if (!element) return;

        element.textContent = "";
        element.classList.add("d-none");
    });
}

function showCopyFieldError(id, message) {
    const element = document.getElementById(id);

    if (!element) return;

    element.textContent = message;
    element.classList.remove("d-none");
}


