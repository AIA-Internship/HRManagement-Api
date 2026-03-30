/**
 * Supervisor Dashboard Module
 * Handles UI rendering for Supervisor Time Management
 * EXACT MATCH DESIGN SYSTEM
 */

let allocationChart = null;
const AIA_COLORS = ['#B62B43', '#F0BB78', '#538D22', '#90AFEA', '#FF809B', '#FFE2E5', '#3F4254'];

const indonesianMonths = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
];

async function initSupervisorDashboard() {
    const data = await fetchAPI('timesheet/supervisor/dashboard');
    if (!data) return;

    renderBanner(data);
    renderMissingSubmissions(data.missingSubmissions);
    renderApprovalSummary(data);
    renderPivotTable(data.internHoursBreakdown);
    renderAllocationDonut(data.projectAllocations);
    setupDropdownFilter(data.internHoursBreakdown);
}

function renderBanner(data) {
    const hour = new Date().getHours();
    let greet = "Good Morning";
    if (hour >= 12 && hour < 17) greet = "Good Afternoon";
    else if (hour >= 17 && hour < 21) greet = "Good Evening";
    else if (hour >= 21 || hour < 5) greet = "Good Night";

    const titleEl = document.getElementById('page_title_text');
    if (titleEl) {
        const userInfoRaw = localStorage.getItem('aia_user_info');
        if (userInfoRaw) {
            const u = JSON.parse(userInfoRaw);
            const rawName = u.fullname || u.fullName || u.name || data.supervisorName || "Supervisor";
            const fname = rawName.split(' ')[0];
            titleEl.innerText = `${greet}, ${fname}`;
        } else {
            titleEl.innerText = `${greet}, ${data.supervisorName || "Supervisor"}`;
        }
    }
    
    const activeEl = document.getElementById('active_intern_count');
    if (activeEl) activeEl.innerText = data.totalActiveInterns || 0;
}

function renderMissingSubmissions(missing) {
    const container = document.getElementById('missing_list_container');
    if (!container) return;
    container.innerHTML = '';

    if (!missing || missing.length === 0) {
        container.innerHTML = '<div class="text-center py-10 opacity-25 fw-bold">All interns up to date.</div>';
        return;
    }

    missing.forEach((m) => {
        const monthName = indonesianMonths[m.month - 1] || m.month;
        container.innerHTML += `
            <div class="d-flex justify-content-between align-items-center mb-8 pb-2 border-bottom border-dashed border-light">
                <span class="fw-boldest fs-6 text-brand">${m.employeeName}</span>
                <span class="fw-bold fs-8 text-black opacity-75 text-uppercase ls-1">- ${monthName} ${m.year}</span>
            </div>
        `;
    });
}

function renderApprovalSummary(data) {
    const reviewStatEl = document.getElementById('needs_review_count_stat');
    const container = document.getElementById('pending_approvals_list');

    if (reviewStatEl) {
        const reviewCount = data.pendingApprovals ? data.pendingApprovals.length : 0;
        const totalInterns = data.totalActiveInterns || 0;
        reviewStatEl.innerText = `${reviewCount} / ${totalInterns}`;
    }

    if (container) {
        if (!data.pendingApprovals || data.pendingApprovals.length === 0) {
            container.innerHTML = '<div class="text-center py-10 opacity-25 fw-bold">No submissions waiting for review.</div>';
            return;
        }

        container.innerHTML = '';
        data.pendingApprovals.forEach(app => {
            const mName = indonesianMonths[app.submissionMonth - 1] || app.submissionMonth;
            container.innerHTML += `
                <div class="d-flex justify-content-between align-items-center mb-6 pb-4 border-bottom border-dashed border-light px-8">
                    <div class="d-flex align-items-center gap-4">
                        <div class="avatar-circle" style="width: 35px; height: 35px; font-size: 0.8rem">${app.employeeName.charAt(0)}</div>
                        <div>
                            <div class="fw-boldest text-gray-800 fs-7">${app.employeeName}</div>
                            <div class="fs-9 text-gray-400 fw-bold">${mName} ${app.submissionYear}</div>
                        </div>
                    </div>
                    <button class="btn btn-sm btn-light-danger fw-boldest px-5 py-2" style="border-radius: 8px; font-size: 0.7rem;" 
                            onclick="window.location.href='/Timesheet/Supervisor/Review?id=${app.submissionId}'">
                        REVIEW
                    </button>
                </div>
            `;
        });
    }
}

/**
 * REVERSED PIVOT TABLE: 
 * Rows = Projects
 * Columns = Interns
 */
function renderPivotTable(breakdown) {
    const headerRow = document.getElementById('breakdown_header_row');
    const tbody = document.getElementById('breakdown_body');
    if (!headerRow || !tbody) return;

    if (!breakdown || breakdown.length === 0) {
        headerRow.innerHTML = `<th>ONGOING PROJECTS</th><th>No Interns Found</th>`;
        tbody.innerHTML = '<tr><td colspan="2" class="text-center py-10 text-muted">No data available</td></tr>';
        return;
    }

    // 1. EXTRACT UNIQUE PROJECTS (ROWS) & INTERNS (COLS)
    const projectSet = new Set();
    breakdown.forEach(b => {
        if(b.projectMinutes) {
            Object.keys(b.projectMinutes).forEach(p => projectSet.add(p));
        }
    });
    const projectsArr = Array.from(projectSet);
    if(projectsArr.length === 0) projectsArr.push("General Work");

    const internNames = breakdown.map(b => b.employeeName);

    // 2. RENDER HEADERS (INTERN NAMES)
    let headerHTML = `<th style="background: transparent;">ONGOING PROJECTS</th>`;
    breakdown.forEach(intern => {
        headerHTML += `<th>${intern.employeeName}</th>`;
    });
    headerRow.innerHTML = headerHTML;

    // 3. RENDER ROWS (PROJECTS)
    let bodyHTML = '';
    projectsArr.forEach(project => {
        let rowHTML = `<tr><td>${project}</td>`;
        breakdown.forEach(intern => {
            const mins = (intern.projectMinutes && intern.projectMinutes[project]) ? intern.projectMinutes[project] : 0;
            const hrs = (mins / 60).toFixed(1);
            const content = mins === 0 ? "0" : hrs;
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
    
    const legendContainerDrt = document.getElementById('donut_legends');
    if(legendContainerDrt) legendContainerDrt.innerHTML = '';
    if (allocationChart) allocationChart.destroy();

    if (!allocations || allocations.length === 0) {
        if(legendContainerDrt) legendContainerDrt.innerHTML = '<div class="text-muted text-center py-10">No data found.</div>';
        return;
    }

    const labels = allocations.map(a => a.projectName);
    const dataValues = allocations.map(a => a.allocationPercentage);
    const backgroundColors = AIA_COLORS.slice(0, allocations.length);
    
    if(legendContainerDrt) {
        allocations.forEach((item, index) => {
            const color = AIA_COLORS[index % AIA_COLORS.length];
            legendContainerDrt.innerHTML += `
                <div class="legend-box">
                    <div class="color-box" style="background: ${color};"></div>
                    <span class="app-name">${item.projectName}</span>
                    <span class="app-percent">${item.allocationPercentage}%</span>
                </div>
            `;
        });
    }

    allocationChart = new Chart(canvas.getContext('2d'), {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: dataValues,
                backgroundColor: backgroundColors,
                borderWidth: 0,
                hoverOffset: 15
            }]
        },
        options: {
            cutout: '75%',
            plugins: { legend: { display: false }, tooltip: { enabled: true } },
            maintainAspectRatio: false,
            layout: { padding: 5 }
        }
    });
}

function setupDropdownFilter(breakdown) {
    const dropdown = document.getElementById('intern_filter_dropdown');
    dropdown.innerHTML = '<option value="">All Intern</option>';
    
    breakdown.forEach(intern => {
        dropdown.innerHTML += `<option value="${intern.employeeId}">${intern.employeeName}</option>`;
    });

    dropdown.addEventListener('change', async (e) => {
        const empId = e.target.value;
        const url = empId ? `timesheet/supervisor/dashboard?FilterEmployeeId=${empId}` : 'timesheet/supervisor/dashboard';
        const newData = await fetchAPI(url);
        if (newData) {
            renderPivotTable(newData.internHoursBreakdown);
        }
    });
}

async function fetchAPI(endpoint) {
    const token = localStorage.getItem('aia_jwt_token');
    if (!token) {
        window.location.href = '/Account/Login';
        return null;
    }

    try {
        const response = await fetch(`https://localhost:7089/api/${endpoint}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (response.status === 401) {
            localStorage.removeItem('aia_jwt_token');
            window.location.href = '/Account/Login';
            return null;
        }

        const json = await response.json();
        if (json.isError) {
             console.warn('API Error Content:', json.statusMessage);
             return null;
        }
        return json.content;
    } catch (error) {
        console.error('API Error:', error);
        return null;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    if (window.location.pathname.toLowerCase().includes('/supervisor/dashboard')) {
        initSupervisorDashboard();
    }
});
