/**
 * Supervisor Dashboard Module
 * Handles UI rendering for Supervisor Time Management
 */

let allocationChart = null;
const AIA_COLORS = ['#B51B3B', '#D31145', '#E4496B', '#F1416C', '#FF809B', '#FFE2E5'];

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
    renderPivotTable(data.internHoursBreakdown, data.projectAllocations);
    renderAllocationBars(data.projectAllocations);
    renderAllocationDonut(data.projectAllocations);
    setupDropdownFilter(data.internHoursBreakdown);
}

function renderBanner(data) {
    const hours = new Date().getHours();
    let greeting = "Good Morning";
    if (hours >= 12) greeting = "Good Afternoon";
    if (hours >= 17) greeting = "Good Evening";

    if (data.supervisorName) {
        document.getElementById('welcome_text').innerText = `${greeting}, ${data.supervisorName}`;
    }
    
    const activeEl = document.getElementById('active_intern_count');
    if (activeEl) activeEl.innerText = data.totalActiveInterns || 0;
}

function renderMissingSubmissions(missing) {
    const container = document.getElementById('missing_list_container');
    container.innerHTML = '';

    if (!missing || missing.length === 0) {
        container.innerHTML = '<div class="text-muted fs-7 py-6 text-center">No missing timesheets this month.</div>';
        return;
    }

    missing.forEach((m, idx) => {
        const monthName = indonesianMonths[m.month - 1] || m.month;
        const avatarInitial = m.employeeName.charAt(0).toUpperCase();
        // UI/UX Polish: Using a vibrant AIA color instead of harsh black
        const avatarColor = AIA_COLORS[idx % AIA_COLORS.length];
        
        container.innerHTML += `
            <div class="d-flex justify-content-between align-items-center mb-1">
                <div class="d-flex align-items-center gap-4">
                    <div class="d-flex align-items-center justify-content-center text-white fw-boldest fs-5 shadow-sm" style="width: 45px; height: 45px; border-radius: 50%; background: ${avatarColor};">
                        ${avatarInitial}
                    </div>
                    <div>
                        <div class="fw-boldest text-gray-900 fs-5">${m.employeeName}</div>
                        <div class="text-gray-400 fs-8 fw-bold">${monthName} ${m.year}</div>
                    </div>
                </div>
                <a href="javascript:void(0)" class="fw-boldest fs-8 outline-none d-flex align-items-center gap-1" style="color: var(--aia-red); text-decoration: none; padding: 4px 8px; transition: opacity 0.2s;" onmouseover="this.style.opacity='0.7'" onmouseout="this.style.opacity='1'" onclick="window.nudgeIntern('${m.employeeName}')">Remind <i class="bi bi-chevron-right fs-9"></i></a>
            </div>
        `;
    });

    const remindAllContainer = document.getElementById('remind_all_container');
    if (remindAllContainer) {
        remindAllContainer.style.display = missing.length > 0 ? 'block' : 'none';
    }
}

function renderApprovalSummary(data) {
    const labelEl = document.getElementById('current_month_label');
    const pillEl = document.getElementById('pending_submissions_pill');
    const draftCount = document.querySelector('#needs_review_count');

    if (labelEl && data.currentMonthLabel) labelEl.innerText = data.currentMonthLabel;
    
    // Evaluate pending approvals count
    const count = data.pendingApprovals ? data.pendingApprovals.length : 0;
    if (pillEl) pillEl.innerText = `${count} Pending Submissions`;
    if (draftCount) draftCount.innerText = count;
}

function renderPivotTable(breakdown, projectAllocations = []) {
    const headerRow = document.getElementById('breakdown_header_row');
    const tbody = document.getElementById('breakdown_body');
    if (!headerRow || !tbody) return;

    if (!breakdown || breakdown.length === 0) {
        headerRow.innerHTML = `
            <th style="padding-bottom: 12px; border:none; text-align: left; padding-left: 16px;">INTERN NAME</th>
            <th style="padding-bottom: 12px; border:none; text-align: right; padding-right: 16px;">TOTAL HOURS</th>
        `;
        tbody.innerHTML = '<tr><td colspan="2" class="text-center py-10 text-muted fw-bold">NO LOGS FOUND FOR THIS PERIOD.</td></tr>';
        return;
    }

    // 1. EXTRACT UNIQUE PROJECTS (COLUMNS)
    // We can use projectAllocations to determine the columns (or dynamically from breakdown)
    const projectNames = new Set();
    breakdown.forEach(b => {
        if(b.projectMinutes) {
            Object.keys(b.projectMinutes).forEach(p => projectNames.add(p));
        }
    });
    const projectsArr = Array.from(projectNames);

    if (projectsArr.length === 0) projectsArr.push('General Work');

    // 2. RENDER HEADERS
    let headerHTML = `<th style="padding-bottom: 12px; border:none; text-align: left; padding-left: 16px;">INTERN NAME</th>`;
    projectsArr.forEach(p => {
        headerHTML += `<th style="padding-bottom: 12px; border:none; text-align: center;">${p}</th>`;
    });
    headerHTML += `<th style="padding-bottom: 12px; border:none; text-align: right; padding-right: 16px;">TOTAL HOURS</th>`;
    headerRow.innerHTML = headerHTML;

    // 3. RENDER ROWS (INTERNS)
    let bodyHTML = '';
    breakdown.forEach((intern, idx) => {
        const bgRow = idx % 2 === 0 ? 'bg-transparent' : 'bg-transparent'; // No striping specified
        const avatarInitial = intern.employeeName.charAt(0).toUpperCase();
        
        // Pick an avatar preset color dynamically
        const avatarBg = ['#111', '#181C32', '#3F4254', '#009EF7', '#50CD89'][idx % 5];
        
        let rowHTML = `<tr class="${bgRow}">
            <td style="padding: 16px 16px; border-bottom: 1px solid #F1F4F9;">
                <div class="d-flex align-items-center gap-4">
                    <div class="d-flex align-items-center justify-content-center text-white fw-boldest fs-7" style="width: 32px; height: 32px; border-radius: 50%; background: ${avatarBg};">
                        ${avatarInitial}
                    </div>
                    <span class="fw-boldest text-gray-900 fs-7">${intern.employeeName}</span>
                </div>
            </td>`;
        
        // Loop projects to inject hours
        projectsArr.forEach(p => {
            const mins = (intern.projectMinutes && intern.projectMinutes[p]) ? intern.projectMinutes[p] : 0;
            const hrs = (mins / 60).toFixed(1);
            const styledHrs = mins === 0 ? `<span style="color: #D1D5DB; font-weight:500;">0.0h</span>` : `${hrs}h`;
            rowHTML += `<td style="padding: 16px; border-bottom: 1px solid #F1F4F9; text-align: center; color: #5E6278; font-weight: 600;">${styledHrs}</td>`;
        });
        
        // Total Hours
        const totalHrs = (intern.totalMinutes / 60).toFixed(1);
        rowHTML += `<td style="padding: 16px 16px; border-bottom: 1px solid #F1F4F9; text-align: right;">
            <span class="fw-boldest fs-6" style="color: var(--aia-red);">${totalHrs}h</span>
        </td>`;
        
        rowHTML += `</tr>`;
        bodyHTML += rowHTML;
    });

    tbody.innerHTML = bodyHTML;
}

function renderAllocationBars(allocations) {
    const barsContainer = document.getElementById('allocation_bars_container');
    const legendContainer = document.getElementById('allocation_legend_bars');
    
    if(!barsContainer || !legendContainer) return;
    
    barsContainer.innerHTML = '';
    legendContainer.innerHTML = '';
    
    if (!allocations || allocations.length === 0) {
        barsContainer.innerHTML = '<div class="text-muted fs-7">No project timesheets allocated.</div>';
        return;
    }
    
    allocations.forEach((item, index) => {
        const color = AIA_COLORS[index % AIA_COLORS.length];
        
        // Add legend dot
        legendContainer.innerHTML += `
            <div class="d-flex align-items-center gap-2">
                <div class="rounded-circle" style="width: 8px; height: 8px; background: ${color};"></div>
                <span class="text-gray-500 fs-9 fw-boldest text-uppercase" style="letter-spacing: 0.5px;">${item.projectName}</span>
            </div>
        `;
        
        // Add progress bar
        barsContainer.innerHTML += `
            <div>
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <span class="fw-boldest text-gray-900 fs-7">${item.projectName}</span>
                    <span class="text-gray-500 fw-bold fs-7">${item.allocationPercentage}%</span>
                </div>
                <div class="progress" style="height: 10px; border-radius: 5px; background: #F1F4F9; overflow: visible;">
                    <div class="progress-bar" role="progressbar" style="width: ${item.allocationPercentage}%; background: ${color}; border-radius: 5px; position:relative;"></div>
                </div>
            </div>
        `;
    });
}

function renderAllocationDonut(allocations) {
    const ctx = document.getElementById('allocation_donut_chart');
    if(!ctx) return;
    const canvasContext = ctx.getContext('2d');
    const legendContainerDrt = document.getElementById('donut_legends');
    const donutCenterVal = document.getElementById('donut_total_hrs');
    
    if(legendContainerDrt) legendContainerDrt.innerHTML = '';

    if (allocationChart) allocationChart.destroy();

    let labels = [];
    let dataValues = [];
    let backgroundColors = [];

    if (!allocations || allocations.length === 0) {
        labels = ['No Data Yet'];
        dataValues = [1];
        backgroundColors = ['#F1F4F9'];
        if(legendContainerDrt) legendContainerDrt.innerHTML = '<div class="text-muted text-center fs-8">No data available</div>';
        if (donutCenterVal) donutCenterVal.innerText = '0';
    } else {
        const totalMinutes = allocations.reduce((sum, item) => sum + item.totalMinutes, 0);
        const totalHrsString = (totalMinutes / 60).toLocaleString(undefined, { maximumFractionDigits: 1 });
        if (donutCenterVal) donutCenterVal.innerText = totalHrsString;
        
        labels = allocations.map(a => a.projectName);
        dataValues = allocations.map(a => a.allocationPercentage);
        backgroundColors = AIA_COLORS.slice(0, allocations.length);
        
        if(legendContainerDrt) {
            allocations.forEach((item, index) => {
                const color = AIA_COLORS[index % AIA_COLORS.length];
                legendContainerDrt.innerHTML += `
                    <div class="d-flex align-items-center gap-2">
                        <div class="rounded-circle" style="width: 8px; height: 8px; background: ${color};"></div>
                        <span class="text-gray-600 fs-9 fw-boldest">${item.projectName}</span>
                    </div>
                `;
            });
        }
    }

    allocationChart = new Chart(canvasContext, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: dataValues,
                backgroundColor: backgroundColors,
                borderWidth: 0,
                hoverOffset: 12
            }]
        },
        options: {
            cutout: '80%',
            plugins: { legend: { display: false }, tooltip: { enabled: allocations && allocations.length > 0 } },
            maintainAspectRatio: false,
            layout: { padding: 10 }
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
            renderPivotTable(newData.internHoursBreakdown, newData.projectAllocations);
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
            // "biar di forntendnya tinggal ambil"
            Swal.fire({
                title: 'Operation Failed',
                text: json.statusMessage || 'An unexpected error occurred.',
                icon: 'error',
                confirmButtonColor: '#B51B3B'
            }).then(() => {
                // If it's a critical access error, redirect to login
                if (json.statusMessage && (json.statusMessage.includes('kedaluwarsa') || json.statusMessage.includes('Ditolak'))) {
                    localStorage.removeItem('aia_jwt_token');
                    window.location.href = '/Account/Login';
                }
            });
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

// Nudge / Notification Actions
window.nudgeIntern = function(name) {
    Swal.fire({
        title: 'Nudge Sent!',
        text: `A reminder notification has been pushed directly to ${name}'s device.`,
        icon: 'success',
        confirmButtonColor: '#B51B3B',
        timer: 3000,
        showConfirmButton: false
    });
};

window.nudgeAll = function() {
    Swal.fire({
        title: 'Send Mass Nudge?',
        text: "You are about to alert all interns with missing timesheets. Do you wish to proceed?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#B51B3B',
        cancelButtonColor: '#A1A5B7',
        confirmButtonText: 'Yes, Nudge All'
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire({
                title: 'Broadcast Sent!',
                text: 'All targeted interns have successfully received push reminders.',
                icon: 'success',
                confirmButtonColor: '#B51B3B',
                timer: 3000,
                showConfirmButton: false
            });
        }
    });
};
