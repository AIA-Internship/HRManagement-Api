const API_BASE = "https://localhost:7089";
const EMPLOYEE_ENDPOINT = "/api/employee/list-id";
const EMPLOYEE_ROLE_ENDPOINT = "/api/employee/employee-roles";
const PLAN_ENDPOINT = "/api/performance-review/plans";
const UPDATE_ENDPOINT = "/api/performance-review/plans/update";
const EDIT_PLAN_ID = getPlanIdFromUrl();

let employees = [];
let employeeRoles = [];
let roleWeights = [];
let selectedRoleId = null;
let activeAssessment = null;
let questionId = 0;
let groupId = 0;
let assessmentSectionId = 0;
const employeePickerContexts = {};

function createAssessmentSection(type) {
    return {
        id: ++assessmentSectionId,
        assessmentType: type,
        answerType: "rating",
        ratingDescription: "",
        questions: [],
        receiverIds: [],
        receiverDisplayIds: [],
        groups: []
    };
}
const assessments = {
    "self-assessment": [createAssessmentSection("self-assessment")],
    "supervisor-assessment": [createAssessmentSection("supervisor-assessment")],
    "peer-review": [createAssessmentSection("peer-review")]
};

document.addEventListener("DOMContentLoaded", initializePage);

async function initializePage() {
    showPageLoading();

    if (!EDIT_PLAN_ID) {
        hidePageLoading();
        showMessage("Invalid performance review plan.", "danger");
        return;
    }

    document.getElementById("periodType").addEventListener("change", handlePeriodChange);
    document.getElementById("startDate").addEventListener("change", handleDateChange);
    document.getElementById("durationInMonth").addEventListener("input", handleDateChange);
    document.getElementById("savePlanButton").addEventListener("click", updatePlan);

    try {
        const [, , plan] = await Promise.all([
            loadEmployees(),
            loadEmployeeRoles(),
            loadPlanRaw()
        ]);

        fillPlanDetails(plan);
        mapPlanAssessments(plan);
        mergePlanScoreWeights(
            plan.scoreWeightConfigurations ??
            []
        );

        renderAllAssessments();
        renderRoleWeights();
    } catch (error) {
        console.error("Failed to initialize edit planner:", error);
        showMessage(
            error.message ||
            "Unable to load performance review plan.",
            "danger"
        );
    } finally {
        hidePageLoading();
    }
}

async function loadPlanRaw() {
    const result = await apiRequest(
        `${PLAN_ENDPOINT}/${EDIT_PLAN_ID}`
    );

    const plan = result.content ?? result;

    if (!plan || Number(plan.id) !== Number(EDIT_PLAN_ID)) {
        throw new Error("Performance review plan not found.");
    }

    if (String(plan.status ?? "").toLowerCase() !== "drafted") {
        disableEditForm();
        throw new Error(
            "Only drafted performance review plans can be edited."
        );
    }

    return plan;
}


function showPageLoading() {
    let loader = document.getElementById("editPlannerLoading");
    if (loader) {
        loader.classList.remove("d-none");
        return;
    }

    loader = document.createElement("div");
    loader.id = "editPlannerLoading";
    loader.className = "edit-planner-loading";
    loader.innerHTML = `
        <div class="edit-planner-loading-card">
            <div class="spinner-border text-danger mb-4" role="status"></div>
            <div class="fw-bold text-gray-800 fs-5">
                Loading Performance Plan
            </div>
            <div class="text-muted fs-7 mt-2">
                Fetching the existing plan configuration...
            </div>
        </div>
    `;

    document.body.appendChild(loader);
}

function hidePageLoading() {
    const loader = document.getElementById("editPlannerLoading");
    if (loader) {
        loader.classList.add("d-none");
    }
}

async function loadEmployeeRoles() {
    try {
        const result = await apiRequest(EMPLOYEE_ROLE_ENDPOINT);
        employeeRoles = result.content ?? [];
        roleWeights = employeeRoles.filter(role => role.position?.trim()).map((role, index) => ({
            id: index + 1,
            subjectRoleId: null,
            subjectJobTitle: role.position.trim(),
            technical: 0,
            softSkill: 0,
            selfAssessment: 0,
            peerReview: 0
        }));
        if (roleWeights.length) selectedRoleId = roleWeights[0].id;
    } catch (error) {
        console.error("Failed to load employee roles:", error);
        showMessage("Unable to load employee positions.", "danger");
    }
}

function getPlanIdFromUrl() {
    const root = document.getElementById("editPlannerPage");
    const dataPlanId = root?.dataset.planId;
    if (dataPlanId) return Number(dataPlanId);
    const queryPlanId = new URLSearchParams(window.location.search).get("planId");
    return Number(queryPlanId) || null;
}

async function loadPlan() {
    const result = await apiRequest(`${PLAN_ENDPOINT}/${EDIT_PLAN_ID}`);
    const plan = result.content ?? result;

    console.log("FULL PLAN RESPONSE:", plan);
    console.log(
        "SCORE WEIGHT CONFIGURATIONS:",
        plan.scoreWeightConfigurations
    );

    if (!plan || Number(plan.id) !== Number(EDIT_PLAN_ID)) {
        throw new Error("Performance review plan not found.");
    }

    if (String(plan.status ?? "").toLowerCase() !== "drafted") {
        disableEditForm();
        throw new Error("Only drafted performance review plans can be edited.");
    }

    fillPlanDetails(plan);
    mapPlanAssessments(plan);

    console.log("FULL PLAN RESPONSE:", plan);

    console.log(
        "SCORE WEIGHT CONFIGURATIONS:",
        plan.scoreWeightConfigurations
    );

    mergePlanScoreWeights(
        plan.scoreWeightConfigurations ??
        plan.scoreWeights ??
        plan.performanceReviewPlanScoreWeights ??
        []
    );

    console.log(
        "ROLE WEIGHTS AFTER MERGE:",
        roleWeights
    );


}

function fillPlanDetails(plan) {
    document.getElementById("planName").value = plan.name ?? "";
    document.getElementById("periodType").value = normalizePeriodType(plan.periodType);
    document.getElementById("startDate").value = toDateInputValue(plan.startDate);
    document.getElementById("durationInMonth").value = Number(plan.durationInMonth) || "";
    window.editPlanMinReviewDurationInDays = Number(plan.minReviewDurationInDays) || 7;
    window.editPlanStatus = plan.status ?? "drafted";
    handlePeriodChange();
    handleDateChange();
}

function normalizePeriodType(value) {
    const normalized = String(value ?? "").toLowerCase();
    if (normalized === "monthly") return "Monthly";
    if (normalized === "quarterly") return "Quarterly";
    return value ?? "";
}

function toDateInputValue(value) {
    if (!value) return "";
    const raw = String(value);
    const match = raw.match(/^(\d{4}-\d{2}-\d{2})/);
    if (match) return match[1];
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? "" : formatApiDate(date);
}

function mapPlanAssessments(plan) {
    const selfPlans = plan.selfAssessments ?? plan.selfAssessment ?? [];
    const supervisorPlans = plan.supervisorAssessments ?? plan.supervisorAssessment ?? [];
    const peerPlans = plan.peerReviews ?? plan.peerReview ?? [];

    assessments["self-assessment"] = selfPlans.map(item => mapAssessment(item, "self-assessment"));
    assessments["supervisor-assessment"] = supervisorPlans.map(item => mapAssessment(item, "supervisor-assessment"));
    assessments["peer-review"] = peerPlans.map(item => mapAssessment(item, "peer-review"));

    assessmentSectionId = Math.max(0, ...Object.values(assessments).flatMap(items => items.map(item => Number(item.id) || 0)));
    questionId = Math.max(0, ...Object.values(assessments).flatMap(items => items.flatMap(item => item.questions.map(question => Number(question.id) || 0))));
    groupId = Math.max(0, ...assessments["peer-review"].flatMap(item => item.groups.map(group => Number(group.id) || 0)));
}

function mapAssessment(item, type) {

    const receiverData =
        type === "peer-review"
            ? {
                receiverIds: [],
                receiverDisplayIds: []
            }
            : mapReceiverData(item.receivers ?? []);

    return {
        id: ++assessmentSectionId,

        sourceId: Number(
            item.assessmentId ?? item.id
        ) || null,

        assessmentType: type,

        answerType:
            item.answerType ?? "rating",

        ratingDescription: String(
            item.ratingDescription ?? ""
        ).trim(),

        questions: (item.questions ?? []).map(question => ({
            id: ++questionId,

            sourceId: Number(
                question.id ?? question.questionId
            ) || null,

            questionText:
                question.questionText ?? "",

            questionOrder:
                Number(question.questionOrder) || 1,

            questionType:
                question.questionType ??
                item.answerType ??
                "rating"
        })),

        receiverIds:
            receiverData.receiverIds,

        receiverDisplayIds:
            receiverData.receiverDisplayIds,

        groups: type === "peer-review"
            ? (item.groups ?? []).map(group => ({
                id: ++groupId,

                sourceId: Number(
                    group.id ?? group.groupId
                ) || null,

                name:
                    group.name ?? "",

                description:
                    group.description ?? "",

                memberIds:
                    (group.members ?? [])
                        .map(member =>
                            Number(
                                member.employeeId ??
                                member.id
                            )
                        )
                        .filter(Boolean)
            }))
            : []
    };
}

function mapReceiverIds(receivers) {
    return receivers
        .map(receiver => {
            const displayId = String(
                receiver.employeeDisplayId ??
                receiver.displayId ??
                ""
            ).trim().toLowerCase();

            const fullName = String(
                receiver.fullName ?? ""
            ).trim().toLowerCase();

            const employee = employees.find(employee => {
                const employeeDisplayId = String(
                    employee.employeeDisplayId ??
                    employee.displayId ??
                    ""
                ).trim().toLowerCase();

                const employeeName = String(
                    employee.fullName ?? ""
                ).trim().toLowerCase();

                return (
                    (displayId && employeeDisplayId === displayId) ||
                    (fullName && employeeName === fullName)
                );
            });

            return getEmployeeId(employee);
        })
        .filter(Boolean);
}

function mapReceiverData(receivers) {
    const receiverDisplayIds = (receivers ?? [])
        .map(receiver =>
            String(
                receiver.employeeDisplayId ??
                receiver.displayId ??
                ""
            ).trim()
        )
        .filter(Boolean);

    const receiverIds = mapReceiverIds(receivers);

    return {
        receiverIds,
        receiverDisplayIds
    };
}

function normalizeKey(value) {
    return String(value ?? "")
        .trim()
        .toLowerCase()
        .replace(/[\s_-]/g, "");
}

function getWeightByType(items, type) {
    const expected = normalizeKey(type);

    const item = (items ?? []).find(
        weight =>
            normalizeKey(
                weight.scoreType ??
                weight.ScoreType
            ) === expected
    );

    return Number(
        item?.weight ??
        item?.Weight ??
        0
    );
}
function normalizeRoleTitle(value) {
    return String(value ?? "")
        .trim()
        .toLowerCase()
        .replace(/\s+/g, " ");
}

function mergePlanScoreWeights(weightConfigs) {
    if (!Array.isArray(weightConfigs)) {
        console.warn(
            "No score weight configurations found:",
            weightConfigs
        );
        return;
    }

    roleWeights = roleWeights.map(role => {
        const config = weightConfigs.find(
            config =>
                normalizeRoleTitle(config.jobTitle) ===
                normalizeRoleTitle(role.subjectJobTitle)
        );

        if (!config) {
            console.warn(
                "No score weight found for:",
                role.subjectJobTitle
            );

            return role;
        }

        const scores = config.scores ?? [];

        return {
            ...role,
            technical: getWeightByType(scores, "technical"),
            softSkill: getWeightByType(scores, "soft-skill"),
            selfAssessment: getWeightByType(
                scores,
                "self-assessment"
            ),
            peerReview: getWeightByType(
                scores,
                "peer-review"
            )
        };
    });

    if (roleWeights.length) {
        const selected = roleWeights.find(
            role => role.id === selectedRoleId
        );

        selectedRoleId = selected
            ? selected.id
            : roleWeights[0].id;
    }
}

function disableEditForm() {
    document.querySelectorAll(
        "#editPlannerPage input,#editPlannerPage select,#editPlannerPage textarea,#editPlannerPage button"
    ).forEach(element => element.disabled = true);
}

function getToken() {
    return window.aiaAuth ? window.aiaAuth.getToken() : null;
}

async function apiRequest(url, options = {}) {
    const start = performance.now();
    const token = getToken();

    if (!token) {
        if (window.aiaAuth) window.aiaAuth.signOut();
        throw new Error("Authentication token not found.");
    }

    console.log(`[API START] ${url}`);

    const response = await fetch(`${API_BASE}${url}`, {
        ...options,
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`,
            ...(options.headers || {})
        }
    });

    const elapsed = ((performance.now() - start) / 1000).toFixed(2);

    console.log(`[API DONE] ${url} - ${elapsed}s - ${response.status}`);

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
        console.error("===== API ERROR =====");
        console.error("HTTP Status:", response.status);
        console.error("Response Body:", result);
        console.error("====================");

        throw new Error(
            result.statusMessage ||
            `API request failed: ${response.status}`
        );
    }

    return result;
}

async function loadEmployees() {
    try {
        const result = await apiRequest(EMPLOYEE_ENDPOINT);
        employees = result.content ?? [];
    } catch (error) {
        console.error("Failed to load employees:", error);
        showMessage("Unable to load employee list.", "danger");
    }
}

function handlePeriodChange() {
    const periodType = document.getElementById("periodType").value;
    const help = document.getElementById("durationHelp");

    clearFieldError("periodTypeError");
    clearFieldError("durationError");

    if (periodType === "Monthly") {
        help.textContent = "Enter how many months the plan should run.";
    } else if (periodType === "Quarterly") {
        help.textContent = "Enter a duration divisible by 3, such as 3, 6, 9, or 12 months.";
    } else {
        help.textContent = "Select a period type to see the duration rules.";
    }

    handleDateChange();
}

function handleDateChange() {
    clearFieldError("startDateError");
    clearFieldError("durationError");

    const periodType = document.getElementById("periodType").value;
    const startValue = document.getElementById("startDate").value;
    const duration = Number(document.getElementById("durationInMonth").value);
    const preview = document.getElementById("endDatePreview");
    const previewText = document.getElementById("endDatePreviewText");

    preview.classList.add("d-none");
    previewText.textContent = "";

    if (!periodType || !startValue || !duration || duration < 1) return;

    const startDate = parseDate(startValue);
    if (!startDate) return;

    if (startDate.getDate() !== 1) {
        showFieldError("startDateError", "Start date must be the first day of a month.");
        return;
    }

    if (periodType === "Quarterly" && duration % 3 !== 0) {
        showFieldError("durationError", "Quarterly duration must be divisible by 3 months.");
        return;
    }

    const endDate = calculateEndDate(startDate, duration);
    previewText.textContent = `This review plan will end on ${formatDate(endDate)}.`;
    preview.classList.remove("d-none");
}

function calculateEndDate(startDate, duration) {
    const endDate = new Date(startDate);
    endDate.setMonth(endDate.getMonth() + duration);
    endDate.setDate(endDate.getDate() - 1);
    return endDate;
}

function groupEmployeesByPosition() {
    const groups = new Map();

    employees.forEach(employee => {
        const position = employee.position?.trim() || "No Position";
        if (!groups.has(position)) groups.set(position, []);
        groups.get(position).push(employee);
    });

    return Array.from(groups.entries())
        .sort(([a], [b]) => a.localeCompare(b))
        .map(([position, roleEmployees]) => [
            position,
            roleEmployees.sort((a, b) => String(a.fullName).localeCompare(String(b.fullName)))
        ]);
}

function toggleEmployeePicker(pickerId) {
    const menu = document.getElementById(`${pickerId}-menu`);
    if (!menu) return;
    menu.classList.toggle("d-none");
}

function toggleAllEmployees(pickerId, checked) {
    const menu = document.getElementById(`${pickerId}-menu`);
    if (!menu) return;

    menu.querySelectorAll(".employee-checkbox").forEach(checkbox => {
        checkbox.checked = checked;
    });

    handleEmployeePickerChange(pickerId);
}

function toggleRoleEmployees(pickerId, roleIndex, checked) {
    const menu = document.getElementById(`${pickerId}-menu`);
    if (!menu) return;

    menu.querySelectorAll(`.employee-checkbox[data-role-index="${roleIndex}"]`).forEach(checkbox => {
        checkbox.checked = checked;
    });

    handleEmployeePickerChange(pickerId);
}

function toggleAssessment(type) {
    const config = document.getElementById(getAssessmentConfigId(type));
    const icon = document.getElementById(getAssessmentIconId(type));
    const isOpen = config.classList.contains("is-open");

    if (isOpen) {
        config.classList.remove("is-open");
        icon.classList.remove("bi-chevron-up");
        icon.classList.add("bi-chevron-down");
        return;
    }

    activeAssessment = type;
    renderAssessmentConfig(type);
    config.classList.add("is-open");
    icon.classList.remove("bi-chevron-down");
    icon.classList.add("bi-chevron-up");
}

function handleEmployeePickerChange(pickerId) {
    const menu = document.getElementById(`${pickerId}-menu`);
    const context = employeePickerContexts[pickerId];

    if (!menu || !context) return;

    const selectedIds = Array.from(
        menu.querySelectorAll(".employee-checkbox:checked")
    )
        .map(checkbox => Number(checkbox.value))
        .filter(Boolean);

    // =========================
    // RECEIVER
    // =========================
    if (context.type === "receiver") {
        const assessment = assessments[context.assessmentType].find(
            item => item.id === Number(context.sectionId)
        );

        if (!assessment) return;

        const selectedEmployees = selectedIds
            .map(id => findEmployee(id))
            .filter(Boolean);

        // Internal employee IDs
        assessment.receiverIds = selectedEmployees
            .map(employee => getEmployeeId(employee))
            .filter(Boolean);

        // Display IDs for UI / mapping
        assessment.receiverDisplayIds = selectedEmployees
            .map(employee =>
                String(employee.employeeDisplayId ?? "").trim()
            )
            .filter(Boolean);

        updateEmployeePickerLabel(
            pickerId,
            assessment.receiverIds
        );

        const selectedContainer = document.getElementById(
            `${pickerId}-selected`
        );

        if (selectedContainer) {
            selectedContainer.innerHTML =
                renderReceiverCards(
                    context.assessmentType,
                    context.sectionId
                );
        }
    }

    // =========================
    // PEER GROUP MEMBER
    // =========================
    if (context.type === "group-member") {
        const assessment = assessments["peer-review"].find(
            item => item.id === Number(context.sectionId)
        );

        if (!assessment) return;

        const group = assessment.groups.find(
            item => item.id === Number(context.groupId)
        );

        if (!group) return;

        group.memberIds = selectedIds;

        updateEmployeePickerLabel(
            pickerId,
            group.memberIds
        );

        const selectedContainer = document.getElementById(
            `${pickerId}-selected`
        );

        if (selectedContainer) {
            selectedContainer.innerHTML =
                renderGroupMemberCards(
                    context.sectionId,
                    context.groupId
                );
        }
    }

    syncEmployeePickerSelectAll(pickerId);
}

function updateEmployeePickerLabel(pickerId, selectedIds) {
    const label = document.getElementById(`${pickerId}-label`);
    if (!label) return;
    label.textContent = selectedIds.length ? `${selectedIds.length} employees selected` : "Select employees";
}

function syncEmployeePickerSelectAll(pickerId) {
    const menu = document.getElementById(`${pickerId}-menu`);
    if (!menu) return;

    const employeeCheckboxes = Array.from(
        menu.querySelectorAll(".employee-checkbox")
    );

    const selectedCount = employeeCheckboxes.filter(
        checkbox => checkbox.checked
    ).length;

    const selectAll = menu.querySelector(".employee-select-all");

    if (selectAll) {
        selectAll.checked =
            employeeCheckboxes.length > 0 &&
            selectedCount === employeeCheckboxes.length;
    }

    menu.querySelectorAll(".employee-picker-group").forEach(group => {
        const roleCheckboxes = Array.from(
            group.querySelectorAll(".employee-checkbox")
        );

        const roleSelectAll = group.querySelector(
            ".employee-role-select-all"
        );

        if (roleSelectAll) {
            roleSelectAll.checked =
                roleCheckboxes.length > 0 &&
                roleCheckboxes.every(checkbox => checkbox.checked);
        }
    });
}

function getAssessmentConfigId(type) {
    if (type === "self-assessment") return "selfAssessmentConfig";
    if (type === "supervisor-assessment") return "supervisorAssessmentConfig";
    return "peerReviewConfig";
}

function getAssessmentIconId(type) {
    if (type === "self-assessment") return "selfAssessmentIcon";
    if (type === "supervisor-assessment") return "supervisorAssessmentIcon";
    return "peerReviewIcon";
}

function renderAllAssessments() {
    renderAssessmentConfig("self-assessment");
    renderAssessmentConfig("supervisor-assessment");
    renderAssessmentConfig("peer-review");

    document.getElementById("selfAssessmentConfig").classList.remove("is-open");
    document.getElementById("supervisorAssessmentConfig").classList.remove("is-open");
    document.getElementById("peerReviewConfig").classList.remove("is-open");
}

function renderAssessmentConfig(type) {
    const container = document.getElementById(getAssessmentConfigId(type));
    const sections = assessments[type];

    container.innerHTML = `
        ${sections.map((assessment, index) =>
        renderAssessmentSection(type, assessment, index)
    ).join("")}
        <div class="mt-4">
            <button type="button"
                    class="btn add-assessment-button w-100 fw-semibold"
                    onclick="addAssessmentSection('${type}')">
                <i class="bi bi-plus-lg me-2"></i>
                Add ${formatAssessmentName(type)}
            </button>
        </div>
    `;
}

document.addEventListener("click", closeEmployeePickersOnOutside);

function closeEmployeePickersOnOutside(event) {
    document.querySelectorAll(".employee-picker-menu").forEach(menu => {
        const picker = menu.closest(".employee-picker");
        if (picker && !picker.contains(event.target)) {
            menu.classList.add("d-none");
        }
    });
}

function renderReceiverCards(type, sectionId) {
    const assessment = assessments[type].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return "";

    return assessment.receiverDisplayIds.map(displayId => {
        const employee = findEmployeeByDisplayId(displayId);

        return `
            <div class="col">
                ${renderEmployeeCard(
            employee,
            `<button type="button"
                             class="employee-remove"
                             onclick="removeReceiver('${type}',${sectionId},'${escapeHtml(displayId)}')">
                        <i class="bi bi-x"></i>
                    </button>`
        )}
            </div>
        `;
    }).join("");
}


function findEmployeeByDisplayId(displayId) {
    const normalized = String(displayId ?? "").trim().toLowerCase();

    return employees.find(
        employee => String(
            employee.employeeDisplayId ??
            employee.displayId ??
            ""
        ).trim().toLowerCase() === normalized
    );
}

function renderAssessmentSection(type, assessment, index) {
    return `
        <div class="assessment-section ${getAssessmentSectionClass(type)} p-5 mb-4">
            <div class="assessment-section-header d-flex align-items-center justify-content-between mb-5">
                <div>
                    <div class="fw-bold text-gray-800 fs-6">
                        ${formatAssessmentName(type)} ${index + 1}
                    </div>
                    <div class="text-muted fs-8 mt-1">
                        Configure questions and participants for this assessment.
                    </div>
                </div>

                ${assessments[type].length > 1 ? `
                    <button type="button"
                            class="btn btn-icon btn-sm btn-light-danger"
                            onclick="removeAssessmentSection('${type}',${assessment.id})"
                            title="Remove">
                        <i class="bi bi-trash"></i>
                    </button>
                `: ""}
            </div>

            <div class="row g-4 mb-5">
                <div class="col-md-4">
                    <label class="form-label fs-8 fw-bold text-muted text-uppercase">
                        Answer Type
                    </label>

                    <select class="form-select form-select-solid fs-7"
                            onchange="updateAssessment('${type}',${assessment.id},'answerType',this.value)">
                        <option value="rating" ${assessment.answerType === "rating" ? "selected" : ""}>
                            Rating
                        </option>
                        <option value="free-text" ${assessment.answerType === "free-text" ? "selected" : ""}>
                            Free Text
                        </option>
                    </select>
                </div>

                <div class="col-md-8">
                    <label class="form-label fs-8 fw-bold text-muted text-uppercase">
                        Rating Description
                    </label>

                    <input type="text"
                           class="form-control form-control-solid fs-7"
                           value="${escapeHtml(assessment.ratingDescription)}"
                           placeholder="e.g. 1 = Needs Improvement, 5 = Excellent"
                           onchange="updateAssessment('${type}',${assessment.id},'ratingDescription',this.value)">
                </div>
            </div>

            <div class="assessment-subsection">
                <div class="d-flex align-items-center justify-content-between mb-4">
                    <div>
                        <div class="fw-bold text-gray-800 fs-6">
                            Questions
                        </div>
                        <div class="text-muted fs-8">
                            Questions that will be answered by the assigned receiver.
                        </div>
                    </div>

                    <button type="button"
                            class="btn btn-sm btn-light fw-bold"
                            onclick="addQuestion('${type}',${assessment.id})">
                        <i class="bi bi-plus-lg me-1"></i>
                        Add Question
                    </button>
                </div>

                <div id="${type}-${assessment.id}-questions">
                    ${renderQuestions(type, assessment.id)}
                </div>
            </div>

            ${type === "peer-review"
            ? renderPeerGroups(assessment.id)
            : renderReceivers(type, assessment.id)}
        </div>
    `;
}

function addAssessmentSection(type) {
    assessments[type].push(createAssessmentSection(type));
    renderAssessmentConfig(type);
}

function removeAssessmentSection(type, sectionId) {
    assessments[type] = assessments[type].filter(
        section => section.id !== Number(sectionId)
    );
    renderAssessmentConfig(type);
}

function getAssessmentSectionClass(type) {
    if (type === "self-assessment") return "assessment-section-self";
    if (type === "supervisor-assessment") return "assessment-section-supervisor";
    return "assessment-section-peer";
}

function renderQuestions(type, sectionId) {
    const assessment = assessments[type].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return "";

    const questions = assessment.questions;

    if (!questions.length) {
        return `<div class="text-muted fs-8 py-4">No questions added yet.</div>`;
    }

    return questions.map((question, index) => `
        <div class="question-row bg-white p-4 mb-3">
            <div class="row g-3 align-items-end">
                <div class="col-md-1">
                    <label class="form-label fs-9 fw-bold text-muted">#</label>
                    <div class="form-control form-control-solid fs-7 bg-light">
                        ${index + 1}
                    </div>
                </div>

                <div class="col-md-10">
                    <label class="form-label fs-9 fw-bold text-muted">
                        Question
                    </label>

                    <input type="text"
                           class="form-control form-control-solid fs-7"
                           value="${escapeHtml(question.questionText)}"
                           placeholder="Enter assessment question"
                           onchange="updateQuestion('${type}',${sectionId},${question.id},'questionText',this.value)">
                </div>

                <div class="col-md-1">
                    <button type="button"
                            class="btn btn-icon btn-sm btn-light-danger"
                            onclick="removeQuestion('${type}',${sectionId},${question.id})">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </div>
        </div>
    `).join("");
}

function renderEmployeeCard(employee, removeButton) {
    if (!employee) return "";

    const name = employee.fullName || "Unknown Employee";
    const displayId = employee.employeeDisplayId || "-";
    const position = employee.position || "-";
    const initials = getInitials(name);

    return `
        <div class="employee-card d-flex align-items-center gap-3">
            <div class="employee-avatar">
                ${escapeHtml(initials)}
            </div>

            <div class="min-w-0 flex-grow-1">
                <div class="fw-bold text-gray-800 text-truncate">
                    ${escapeHtml(name)}
                </div>

                <div class="text-muted mt-1 text-truncate">
                    ${escapeHtml(displayId)} | ${escapeHtml(position)}
                </div>
            </div>

            ${removeButton}
        </div>
    `;
}

function getInitials(name) {
    if (!name) return "?";

    return name.trim()
        .split(/\s+/)
        .slice(0, 2)
        .map(word => word.charAt(0).toUpperCase())
        .join("");
}

function renderReceivers(type, sectionId) {
    const assessment = assessments[type].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return "";

    const selected = assessment.receiverIds;
    const pickerId = `receiver-picker-${type}-${sectionId}`;

    return `
        <div class="separator my-5"></div>

        <div class="d-flex align-items-center justify-content-between mb-4">
            <div>
                <div class="fw-bold text-gray-800 fs-6">
                    Receivers
                </div>

                <div class="text-muted fs-8">
                    ${type === "self-assessment"
            ? "Employees who will complete their own assessment."
            : "Employees whose supervisor assessment will be generated."
        }
                </div>
            </div>
        </div>

        ${renderEmployeePicker(
            pickerId,
            selected,
            {
                type: "receiver",
                assessmentType: type,
                sectionId: sectionId
            }
        )}

        <div id="${pickerId}-selected"
             class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-3 receiver-list">
            ${renderReceiverCards(type, sectionId)}
        </div>
    `;
}

function renderEmployeePicker(pickerId, selectedIds, context) {
    employeePickerContexts[pickerId] = context;

    const groups = groupEmployeesByPosition();

    return `
        <div class="employee-picker mb-4">
            <button type="button"
                    class="form-select form-select-solid fs-7 d-flex align-items-center justify-content-between"
                    onclick="toggleEmployeePicker('${pickerId}')">
                <span id="${pickerId}-label">
                    ${selectedIds.length
            ? `${selectedIds.length} employees selected`
            : "Select employees"
        }
                </span>
            </button>

            <div id="${pickerId}-menu"
                 class="employee-picker-menu d-none"
                 onclick="event.stopPropagation()">

                <div class="employee-picker-toolbar">
                    <label class="employee-picker-option p-0">
                        <input type="checkbox"
                               class="employee-select-all"
                               onchange="toggleAllEmployees('${pickerId}', this.checked)"
                               ${selectedIds.length === employees.length
            ? "checked"
            : ""
        }>

                        <span class="fw-semibold text-gray-700 fs-8">
                            Select all employees
                        </span>
                    </label>
                </div>

                <div class="employee-picker-list">
                    ${groups.map(([position, roleEmployees], roleIndex) => {

            const roleIds = roleEmployees
                .map(employee => getEmployeeId(employee))
                .filter(Boolean);

            const allSelected =
                roleIds.length > 0 &&
                roleIds.every(id => selectedIds.includes(id));

            return `
                            <div class="employee-picker-group">

                                <div class="employee-picker-group-header">
                                    <label class="employee-picker-option p-0 flex-grow-1">

                                        <input type="checkbox"
                                               class="employee-role-select-all"
                                               data-role-index="${roleIndex}"
                                               onchange="toggleRoleEmployees('${pickerId}', ${roleIndex}, this.checked)"
                                               ${allSelected ? "checked" : ""}>

                                        <span class="text-gray-700 fs-8">
                                            ${escapeHtml(position)}
                                        </span>

                                    </label>

                                    <span class="employee-picker-count">
                                        ${roleEmployees.length}
                                    </span>
                                </div>

                                ${roleEmployees.map(employee => {

                const employeeId =
                    getEmployeeId(employee);

                const displayId =
                    String(
                        employee.employeeDisplayId ?? ""
                    ).trim();

                const employeePosition =
                    employee.position || "-";

                return `
                                        <label class="employee-picker-option">

                                            <input type="checkbox"
                                                   class="employee-checkbox"
                                                   data-role-index="${roleIndex}"
                                                   value="${employeeId}"
                                                   onchange="handleEmployeePickerChange('${pickerId}')"
                                                   ${selectedIds.includes(employeeId)
                        ? "checked"
                        : ""
                    }>

                                            <div class="employee-picker-avatar">
                                                ${escapeHtml(
                        getInitials(
                            employee.fullName
                        )
                    )}
                                            </div>

                                            <div class="employee-picker-info">

                                                <div class="employee-picker-name">
                                                    ${escapeHtml(
                        employee.fullName
                    )}
                                                </div>

                                                <div class="employee-picker-meta">
                                                    ${escapeHtml(displayId)}
                                                    |
                                                    ${escapeHtml(employeePosition)}
                                                </div>

                                            </div>

                                        </label>
                                    `;
            }).join("")}

                            </div>
                        `;
        }).join("")}
                </div>

            </div>
        </div>
    `;
}

function renderPeerGroups(sectionId) {
    const assessment = assessments["peer-review"].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return "";

    const groups = assessment.groups;

    return `
        <div class="separator my-5"></div>

        <div class="d-flex align-items-center justify-content-between mb-4">
            <div>
                <div class="fw-bold text-gray-800 fs-6">
                    Peer Review Groups
                </div>

                <div class="text-muted fs-8">
                    Members inside the same group will review each other.
                </div>
            </div>

            <button type="button"
                    class="btn btn-sm btn-light fw-bold"
                    onclick="addGroup(${sectionId})">
                <i class="bi bi-plus-lg me-1"></i>
                Add Group
            </button>
        </div>

        ${groups.length
            ? groups.map((group, index) => `
                <div class="group-row bg-white p-4 mb-3">
                    <div class="d-flex align-items-center justify-content-between mb-4">
                        <span class="fw-bold text-gray-800 fs-7">
                            Group ${index + 1}
                        </span>

                        <button type="button"
                                class="btn btn-icon btn-sm btn-light-danger"
                                onclick="removeGroup(${sectionId},${group.id})">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>

                    <div class="row g-4">
                        <div class="col-md-5">
                            <label class="form-label fs-9 fw-bold text-muted">
                                Group Name
                            </label>

                            <input type="text"
                                   class="form-control form-control-solid fs-7"
                                   value="${escapeHtml(group.name)}"
                                   placeholder="e.g. Development Team"
                                   onchange="updateGroup(${sectionId},${group.id},'name',this.value)">
                        </div>

                        <div class="col-md-7">
                            <label class="form-label fs-9 fw-bold text-muted">
                                Description
                            </label>

                            <input type="text"
                                   class="form-control form-control-solid fs-7"
                                   value="${escapeHtml(group.description)}"
                                   placeholder="Optional description"
                                   onchange="updateGroup(${sectionId},${group.id},'description',this.value)">
                        </div>

                        <div class="col-12">
                            <label class="form-label fs-9 fw-bold text-muted">
                                Members
                            </label>

                            ${renderEmployeePicker(
                `peer-group-${sectionId}-${group.id}`,
                group.memberIds,
                { type: "group-member", sectionId: sectionId, groupId: group.id }
                )}

                            <div id="peer-group-${sectionId}-${group.id}-selected"
                                 class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-3 group-members-list">
                                ${renderGroupMemberCards(sectionId, group.id)}
                            </div>
                        </div>
                    </div>
                </div>
            `).join("")
            : `<div class="text-muted fs-8 py-4">No groups added yet.</div>`}
    `;
}

function renderGroupMemberCards(sectionId, groupId) {
    const assessment = assessments["peer-review"].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return "";

    const group = assessment.groups.find(
        item => item.id === Number(groupId)
    );

    if (!group) return "";

    return group.memberIds.map(id => {
        const employee = findEmployee(id);

        return `
            <div class="col">
                ${renderEmployeeCard(
            employee,
            `<button type="button"
                             class="employee-remove"
                             onclick="removeGroupMember(${sectionId},${groupId},${id})">
                        <i class="bi bi-x"></i>
                    </button>`
        )}
            </div>
        `;
    }).join("");
}

function getEmployeeId(employee) {
    const id = employee.employeeId ?? employee.id;
    return id ? Number(id) : null;
}

function findEmployee(id) {
    return employees.find(
        employee => getEmployeeId(employee) === Number(id)
    );
}

function updateAssessment(type, sectionId, field, value) {
    const assessment = assessments[type].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return;

    assessment[field] = value;

    if (field === "answerType") {
        assessment.questions.forEach(
            question => question.questionType = value
        );
    }
}

function addQuestion(type, sectionId) {
    const assessment = assessments[type].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return;

    assessment.questions.push({
        id: ++questionId,
        questionText: "",
        questionOrder: assessment.questions.length + 1,
        questionType: assessment.answerType
    });

    renderAssessmentConfig(type);
}

function updateQuestion(type, sectionId, id, field, value) {
    const assessment = assessments[type].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return;

    const question = assessment.questions.find(
        item => item.id === id
    );

    if (question) question[field] = value;
}

function removeQuestion(type, sectionId, id) {
    const assessment = assessments[type].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return;

    assessment.questions = assessment.questions.filter(
        item => item.id !== id
    );

    assessment.questions.forEach(
        (item, index) => item.questionOrder = index + 1
    );

    renderAssessmentConfig(type);
}

function removeReceiver(type, sectionId, displayId) {
    const assessment = assessments[type].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return;

    const normalizedDisplayId =
        String(displayId).trim().toLowerCase();

    // Find employee from display ID
    const employee = findEmployeeByDisplayId(displayId);
    const employeeId = getEmployeeId(employee);

    // Remove display ID
    assessment.receiverDisplayIds =
        assessment.receiverDisplayIds.filter(
            id =>
                String(id).trim().toLowerCase() !==
                normalizedDisplayId
        );

    // Remove internal employee ID
    if (employeeId) {
        assessment.receiverIds =
            assessment.receiverIds.filter(
                id => Number(id) !== Number(employeeId)
            );
    }

    const pickerId =
        `receiver-picker-${type}-${sectionId}`;

    const menu =
        document.getElementById(`${pickerId}-menu`);

    // Uncheck the employee checkbox
    if (menu && employeeId) {
        const checkbox = Array.from(
            menu.querySelectorAll(".employee-checkbox")
        ).find(
            checkbox =>
                Number(checkbox.value) === Number(employeeId)
        );

        if (checkbox) {
            checkbox.checked = false;
        }
    }

    // Update selected count
    updateEmployeePickerLabel(
        pickerId,
        assessment.receiverIds
    );

    // Re-render receiver cards
    const selectedContainer =
        document.getElementById(
            `${pickerId}-selected`
        );

    if (selectedContainer) {
        selectedContainer.innerHTML =
            renderReceiverCards(
                type,
                sectionId
            );
    }

    syncEmployeePickerSelectAll(pickerId);
}

function addGroup(sectionId) {
    const assessment = assessments["peer-review"].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return;

    assessment.groups.push({
        id: ++groupId,
        name: "",
        description: "",
        memberIds: []
    });

    renderAssessmentConfig("peer-review");
}

function updateGroup(sectionId, groupId, field, value) {
    const assessment = assessments["peer-review"].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return;

    const group = assessment.groups.find(
        item => item.id === Number(groupId)
    );

    if (group) group[field] = value;
}

function removeGroup(sectionId, groupId) {
    const assessment = assessments["peer-review"].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return;

    assessment.groups = assessment.groups.filter(
        item => item.id !== Number(groupId)
    );

    renderAssessmentConfig("peer-review");
}

function applyGroupMemberSelection(sectionId, groupId, pickerId) {
    const picker = document.getElementById(pickerId);
    const assessment = assessments["peer-review"].find(
        item => item.id === Number(sectionId)
    );

    if (!picker || !assessment) return;

    const group = assessment.groups.find(
        item => item.id === Number(groupId)
    );

    if (!group) return;

    group.memberIds = Array.from(
        picker.querySelectorAll("input[type='checkbox']:checked")
    ).map(input => Number(input.value));

    renderAssessmentConfig("peer-review");
}

function removeGroupMember(sectionId, groupId, employeeId) {
    const assessment = assessments["peer-review"].find(
        item => item.id === Number(sectionId)
    );

    if (!assessment) return;

    const group = assessment.groups.find(
        item => item.id === Number(groupId)
    );

    if (!group) return;

    group.memberIds = group.memberIds.filter(
        id => id !== Number(employeeId)
    );

    renderAssessmentConfig("peer-review");
}

function renderRoleWeights() {
    const container = document.getElementById("scoreWeightList");

    if (!roleWeights.length) {
        container.innerHTML = `
            <div class="text-muted fs-8 py-4">
                No intern positions available.
            </div>
        `;
        return;
    }

    const selectedRole = roleWeights.find(
        role => role.id === selectedRoleId
    ) || roleWeights[0];

    selectedRoleId = selectedRole.id;

    container.innerHTML = `
        <div class="d-flex flex-column gap-5">
            <div>
                <label class="form-label fs-8 fw-bold text-muted text-uppercase">
                    Intern Position
                </label>

                <select class="form-select form-select-solid fs-7"
                        style="max-width:320px;"
                        onchange="selectRoleWeight(this.value)">
                    ${roleWeights.map(role => `
                        <option value="${role.id}"
                                ${role.id === selectedRole.id ? "selected" : ""}>
                            ${escapeHtml(role.subjectJobTitle)}
                        </option>
                    `).join("")}
                </select>
            </div>

            <div class="weight-role p-5">
                <div class="assessment-section-header d-flex align-items-center justify-content-between mb-5">
                    <div class="fw-bold text-gray-800 fs-6">
                        ${escapeHtml(selectedRole.subjectJobTitle)}
                    </div>
                </div>

                <div class="row g-3">
                    ${weightInput(selectedRole, "technical", "Technical")}
                    ${weightInput(selectedRole, "softSkill", "Soft Skill")}
                    ${weightInput(selectedRole, "selfAssessment", "Self Assessment")}
                    ${weightInput(selectedRole, "peerReview", "Peer Review")}
                </div>

                <div class="d-flex align-items-center justify-content-between bg-light rounded p-3 mt-4">
                    <span class="fw-semibold text-gray-700 fs-7">
                        Total Weight
                    </span>

                    <span class="fw-bold ${getWeightTotal(selectedRole) === 100
            ? "text-success"
            : "text-danger"
        } fs-7">
                        ${getWeightTotal(selectedRole)}%
                    </span>
                </div>
            </div>
        </div>
    `;
}

function selectRoleWeight(value) {
    selectedRoleId = Number(value);
    renderRoleWeights();
}

function weightInput(role, field, label) {
    return `
        <div class="col-6">
            <label class="form-label fs-9 fw-bold text-muted text-uppercase">
                ${label}
            </label>

            <div class="input-group">
                <input type="number"
                       min="0"
                       max="100"
                       step="0.01"
                       class="form-control form-control-solid fs-7"
                       value="${role[field]}"
                       onchange="updateRoleWeight(${role.id},'${field}',this.value)">

                <span class="input-group-text bg-light border-0 fs-7">
                    %
                </span>
            </div>
        </div>
    `;
}

function updateRoleWeight(id, field, value) {
    const role = roleWeights.find(
        item => item.id === id
    );

    if (!role) return;

    role[field] = Number(value) || 0;
    selectedRoleId = id;
    renderRoleWeights();
}

function getWeightTotal(role) {
    const total =
        Number(role.technical || 0) +
        Number(role.softSkill || 0) +
        Number(role.selfAssessment || 0) +
        Number(role.peerReview || 0);

    return Number(total.toFixed(2));
}

function validatePlan() {
    clearAllErrors();

    const name = document.getElementById("planName").value.trim();
    const periodType = document.getElementById("periodType").value;
    const startValue = document.getElementById("startDate").value;
    const duration = Number(
        document.getElementById("durationInMonth").value
    );

    let valid = true;

    if (!name) {
        showFieldError("planNameError", "Plan title is required.");
        valid = false;
    }

    if (!periodType) {
        showFieldError("periodTypeError", "Period type is required.");
        valid = false;
    }

    if (!startValue) {
        showFieldError("startDateError", "Start date is required.");
        valid = false;
    }

    const startDate = parseDate(startValue);

    if (startDate && startDate.getDate() !== 1) {
        showFieldError(
            "startDateError",
            "Start date must be the first day of a month."
        );
        valid = false;
    }

    if (!Number.isInteger(duration) || duration < 1) {
        showFieldError(
            "durationError",
            "Duration must be a positive whole number."
        );
        valid = false;
    }

    if (
        periodType === "Quarterly" &&
        Number.isInteger(duration) &&
        duration % 3 !== 0
    ) {
        showFieldError(
            "durationError",
            "Quarterly duration must be divisible by 3 months."
        );
        valid = false;
    }

    return valid;
}

function validateAssessments() {
    const errors = [];

    Object.values(assessments).forEach(typeAssessments => {
        typeAssessments.forEach(assessment => {
            if (!assessment.questions.length) {
                errors.push(
                    `${formatAssessmentName(assessment.assessmentType)} ${getAssessmentNumber(assessment)} must have at least one question.`
                );
            }

            assessment.questions.forEach((question, index) => {
                if (!question.questionText.trim()) {
                    errors.push(
                        `${formatAssessmentName(assessment.assessmentType)} ${getAssessmentNumber(assessment)} Question ${index + 1} cannot be empty.`
                    );
                }
            });

            if (assessment.assessmentType === "peer-review") {
                if (!assessment.groups.length) {
                    errors.push(
                        `Peer Review ${getAssessmentNumber(assessment)} must have at least one group.`
                    );
                }

                assessment.groups.forEach((group, index) => {
                    if (!group.name.trim()) {
                        errors.push(
                            `Peer Review ${getAssessmentNumber(assessment)} Group ${index + 1} must have a name.`
                        );
                    }

                    if (group.memberIds.length < 2) {
                        errors.push(
                            `Peer Review ${getAssessmentNumber(assessment)} Group ${index + 1} must have at least 2 members.`
                        );
                    }
                });
            } else if (!assessment.receiverIds.length) {
                errors.push(
                    `${formatAssessmentName(assessment.assessmentType)} ${getAssessmentNumber(assessment)} must have at least one receiver.`
                );
            }
        });
    });

    return errors;
}

function getAssessmentNumber(assessment) {
    const typeAssessments = assessments[assessment.assessmentType];

    const index = typeAssessments.findIndex(
        item => item.id === assessment.id
    );

    return index + 1;
}

function validateWeights() {
    const errors = [];

    roleWeights.forEach(role => {
        if (!role.subjectJobTitle.trim()) {
            errors.push(
                "Every intern position must have a job title."
            );
        }

        const total = getWeightTotal(role);

        if (Math.abs(total - 100) > 0.01) {
            errors.push(
                `${role.subjectJobTitle} weight must total 100%.`
            );
        }
    });

    return errors;
}

async function updatePlan() {
    const planValid = validatePlan();
    const assessmentErrors = validateAssessments();
    const weightErrors = validateWeights();
    const errors = [...assessmentErrors, ...weightErrors];

    if (!planValid || errors.length) {
        showMessage(
            errors.length
                ? errors.join("<br>")
                : "Please complete the required plan information.",
            "danger"
        );

        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });

        return;
    }

    const button = document.getElementById("savePlanButton");
    setButtonLoading(button, true);

    const startDate = parseDate(
        document.getElementById("startDate").value
    );

    const duration = Number(
        document.getElementById("durationInMonth").value
    );

    const endDate = calculateEndDate(startDate, duration);

    const payload = {
        name: document.getElementById("planName").value.trim(),
        periodType: document.getElementById("periodType").value,
        startDate: formatApiDate(startDate),
        endDate: formatApiDate(endDate),
        durationInMonth: duration,
        minReviewDurationInDays: window.editPlanMinReviewDurationInDays || 7,
        status: window.editPlanStatus || "drafted",

        assessments: Object.values(assessments).flatMap(
            typeAssessments => typeAssessments.map(assessment => ({
                assessmentType: assessment.assessmentType,
                answerType: assessment.answerType,
                ratingDescription: assessment.ratingDescription || null,

                fillerRoleId: null,

                fillerJobTitle:
                    assessment.assessmentType === "supervisor-assessment"
                        ? "Supervisor"
                        : assessment.assessmentType === "peer-review"
                            ? "Peer"
                            : null,

                subjectRoleId: null,
                subjectJobTitle: null,
                receiverIds:
                    assessment.assessmentType === "peer-review"
                        ? []
                        : assessment.receiverIds,

                questions: assessment.questions.map(question => ({
                    questionText: question.questionText.trim(),
                    questionOrder: question.questionOrder,
                    questionType:
                        question.questionType ||
                        assessment.answerType
                })),

                groups: assessment.groups.map(group => ({
                    name: group.name.trim(),
                    description: group.description.trim(),
                    memberIds: group.memberIds
                }))
            }))
        ),

        scoreWeights: roleWeights.flatMap(role => [
            {
                subjectRoleId: role.subjectRoleId,
                subjectJobTitle: role.subjectJobTitle,
                scoreType: "technical",
                weight: Number(role.technical)
            },
            {
                subjectRoleId: role.subjectRoleId,
                subjectJobTitle: role.subjectJobTitle,
                scoreType: "soft-skill",
                weight: Number(role.softSkill)
            },
            {
                subjectRoleId: role.subjectRoleId,
                subjectJobTitle: role.subjectJobTitle,
                scoreType: "self-assessment",
                weight: Number(role.selfAssessment)
            },
            {
                subjectRoleId: role.subjectRoleId,
                subjectJobTitle: role.subjectJobTitle,
                scoreType: "peer-review",
                weight: Number(role.peerReview)
            }
        ])
    };

    console.log("Edit Planner Payload:", payload);

    try {
        await apiRequest(
            `${UPDATE_ENDPOINT}/${EDIT_PLAN_ID}`,
            {
                method: "PUT",
                body: JSON.stringify(payload)
            }
        );

        window.location.href = "/PerformanceReview/Supervisor/Planner";
    } catch (error) {
        console.error("Failed to update plan:", error);
        showMessage(
            error.message || "Failed to update performance review plan.",
            "danger"
        );
    } finally {
        setButtonLoading(button, false);
    }
}

function formatAssessmentName(type) {
    if (type === "self-assessment") return "Self Assessment";
    if (type === "supervisor-assessment") return "Supervisor Assessment";
    return "Peer Review";
}

function parseDate(value) {
    if (!value) return "";

    const parts = value.split("-");
    if (parts.length !== 3) return null;

    const date = new Date(
        Number(parts[0]),
        Number(parts[1]) - 1,
        Number(parts[2])
    );

    return Number.isNaN(date.getTime()) ? null : date;
}

function formatApiDate(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");

    return `${year}-${month}-${day}`;
}

function formatDate(date) {
    if (!date) return "-";

    const day = String(date.getDate()).padStart(2, "0");
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const year = date.getFullYear();

    return `${day}/${month}/${year}`;
}

function clearAllErrors() {
    [
        "planNameError",
        "periodTypeError",
        "startDateError",
        "durationError"
    ].forEach(clearFieldError);
}

function clearFieldError(id) {
    const element = document.getElementById(id);
    if (!element) return;

    element.textContent = "";
    element.classList.add("d-none");
}

function showFieldError(id, message) {
    const element = document.getElementById(id);
    if (!element) return;

    element.textContent = message;
    element.classList.remove("d-none");
}

function setButtonLoading(button, loading) {
    if (!button) return;

    const label = button.querySelector(".indicator-label");
    const progress = button.querySelector(".indicator-progress");

    button.disabled = loading;

    if (label) label.classList.toggle("d-none", loading);
    if (progress) progress.classList.toggle("d-none", !loading);
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

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}