/**
 * Supervisor Dashboard Module
 * AIA PROFESSIONAL CLEAN OVERHAUL
 */

let allocationChart = null;
const AIA_COLORS = ['#D31145', '#181C32', '#009EF7', '#50CD89', '#F1416C', '#7239EA', '#3F4254'];

const indonesianMonths = [
    "Jan", "Feb", "Mar", "Apr", "May", "Jun",
    "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
];

async function initSupervisorDashboard() {
    try {
        const data = await fetchAPI('timesheet/supervisor/dashboard');
        if (!data) return;

        renderBanner(data);
        renderMissingSubmissions(data.missingSubmissions);
        renderApprovalSummary(data);
        renderPivotTable(data.internHoursBreakdown);
        renderAllocationDonut(data.projectAllocations);
        setupDropdownFilter(data.internHoursBreakdown);
    } catch (err) {
        console.error("Dashboard Init Error:", err);
    }
}

function renderBanner(data) {
    const activeEl = document.getElementById('active_intern_count');
    if (activeEl) activeEl.innerText = data.totalActiveInterns || 0;
}

function renderMissingSubmissions(missing) {
    const container = document.getElementById('missing_list_container');
    if (!container) return;
    container.innerHTML = '';

    if (!missing || missing.length === 0) {
        container.innerHTML = '<div class="text-center py-20 opacity-25 fw-bold">All interns up to date.</div>';
        return;
    }

    missing.forEach((m) => {
        const monthName = indonesianMonths[m.month - 1] || m.month;
        container.innerHTML += `
            <div class="list-item-dashed">
                <div class="d-flex align-items-center gap-3">
                    <div class="symbol-circle">${m.employeeName.charAt(0)}</div>
                    <span class="fw-bold fs-6 text-gray-800">${m.employeeName}</span>
                </div>
                <span class="badge bg-light text-muted fw-bold fs-9 px-3 py-2 border">${monthName} ${m.year}</span>
            </div>
        `;
    });
}

function renderApprovalSummary(data) {
    const reviewStatEl = document.getElementById('needs_review_count_stat');
    const container = document.getElementById('pending_approvals_list');

    if (reviewStatEl) {
        const reviewCount = data.pendingApprovals ? data.pendingApprovals.length : 0;
        reviewStatEl.innerText = `${reviewCount} / ${data.totalActiveInterns || 0}`;
    }

    if (container) {
        if (!data.pendingApprovals || data.pendingApprovals.length === 0) {
            container.innerHTML = '<div class="text-center py-20 opacity-25 fw-bold">No submissions waiting review.</div>';
            return;
        }

        container.innerHTML = '';
        data.pendingApprovals.forEach(app => {
            const mName = indonesianMonths[app.submissionMonth - 1] || app.submissionMonth;
            container.innerHTML += `
                <div class="list-item-dashed">
                    <div class="d-flex align-items-center gap-4">
                        <div class="symbol-circle bg-light-brand text-brand">${app.employeeName.charAt(0)}</div>
                        <div>
                            <div class="fw-bold text-gray-900 fs-6">${app.employeeName}</div>
                            <div class="fs-tiny text-muted fw-bold">${mName} ${app.submissionYear}</div>
                        </div>
                    </div>
                    <button class="btn btn-sm btn-light-grey px-5 py-2 fs-tiny" onclick="window.location.href='/Timesheet/Supervisor/Review?id=${app.submissionId}'">
                        REVIEW
                    </button>
                </div>
            `;
        });
    }
}

function renderPivotTable(breakdown) {
    const headerRow = document.getElementById('breakdown_header_row');
    const tbody = document.getElementById('breakdown_body');
    if (!headerRow || !tbody) return;

    if (!breakdown || breakdown.length === 0) {
        headerRow.innerHTML = `<th>PROJECT NAME</th><th>DATA STATUS</th>`;
        tbody.innerHTML = '<tr><td colspan="2" class="text-center py-10 opacity-25">No data found</td></tr>';
        return;
    }

    const projectSet = new Set();
    breakdown.forEach(b => {
        if(b.projectMinutes) {
            Object.keys(b.projectMinutes).forEach(p => projectSet.add(p));
        }
    });
    const projectsArr = Array.from(projectSet);
    if(projectsArr.length === 0) projectsArr.push("General Work");

    let headerHTML = `<th>PROJECT NAME</th>`;
    breakdown.forEach(intern => {
        headerHTML += `<th>${intern.employeeName.split(' ')[0]}</th>`;
    });
    headerRow.innerHTML = headerHTML;

    let bodyHTML = '';
    projectsArr.forEach(project => {
        let rowHTML = `<tr><td>${project}</td>`;
        breakdown.forEach(intern => {
            const mins = (intern.projectMinutes && intern.projectMinutes[project]) ? intern.projectMinutes[project] : 0;
            const hrs = (mins / 60).toFixed(1);
            const content = mins === 0 ? '<span class="opacity-10">-</span>' : `<span class="badge bg-light text-dark fw-bold fs-8">${hrs}h</span>`;
            rowHTML += `<td>${content}</td>`;
        });
        rowHTML += `</tr>`;
        bodyHTML += rowHTML;
    });

    tbody.innerHTML = bodyHTML;
}

function renderAllocationDonut(allocations) {
    const canvas = document.getElementById('allocation_donut_chart');
    if(!canvas) return;
    
    const legendContainer = document.getElementById('donut_legends');
    if(legendContainer) legendContainer.innerHTML = '';
    if (allocationChart) allocationChart.destroy();

    if (!allocations || allocations.length === 0) {
        if(legendContainer) legendContainer.innerHTML = '<div class="text-muted text-center py-10">No usage data found.</div>';
        return;
    }

    if(legendContainer) {
        allocations.forEach((item, idx) => {
            const color = AIA_COLORS[idx % AIA_COLORS.length];
            legendContainer.innerHTML += `
                <div class="chart-legend-item">
                    <div class="dot" style="background: ${color};"></div>
                    <span class="legend-label">${item.projectName}</span>
                    <span class="legend-value text-brand">${item.allocationPercentage}%</span>
                </div>
            `;
        });
    }

    allocationChart = new Chart(canvas.getContext('2d'), {
        type: 'doughnut',
        data: {
            labels: allocations.map(a => a.projectName),
            datasets: [{
                data: allocations.map(a => a.allocationPercentage),
                backgroundColor: AIA_COLORS.slice(0, allocations.length),
                borderWidth: 0,
                hoverOffset: 12
            }]
        },
        options: {
            cutout: '75%',
            plugins: { legend: { display: false } },
            maintainAspectRatio: false
        }
    });
}

function setupDropdownFilter(breakdown) {
    const dropdown = document.getElementById('intern_filter_dropdown');
    if(!dropdown) return;
    dropdown.innerHTML = '<option value="">All Interns</option>';
    breakdown.forEach(i => {
        dropdown.innerHTML += `<option value="${i.employeeId}">${i.employeeName}</option>`;
    });

    dropdown.addEventListener('change', async (e) => {
        const empId = e.target.value;
        const url = empId ? `timesheet/supervisor/dashboard?FilterEmployeeId=${empId}` : 'timesheet/supervisor/dashboard';
        const newData = await fetchAPI(url);
        if (newData) renderPivotTable(newData.internHoursBreakdown);
    });
}

async function fetchAPI(endpoint) {
    const token = window.aiaAuth ? window.aiaAuth.getToken() : null;
    const response = await fetch(`https://127.0.0.1:7089/api/${endpoint}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    if (response.status === 401) { window.location.href = '/Account/Login'; return null; }
    const json = await response.json();
    return json.content || json.Content || json.data || json;
}

document.addEventListener('DOMContentLoaded', () => {
    if (window.location.pathname.toLowerCase().includes('/supervisor/dashboard')) {
        initSupervisorDashboard();
    }
});
