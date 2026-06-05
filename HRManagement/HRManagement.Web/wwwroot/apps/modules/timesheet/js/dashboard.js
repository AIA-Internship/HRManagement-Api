/**
 * Supervisor Dashboard Module
 * AIA PROFESSIONAL CLEAN OVERHAUL (MATCHING MOCKUP)
 */

let allocationChart = null;
const AIA_COLORS = ['#C0214B', '#F1B25C', '#519C42', '#86A3E8', '#181C32', '#009EF7', '#50CD89'];

const englishMonths = ["JANUARY", "FEBRUARY", "MARCH", "APRIL", "MAY", "JUNE", "JULY", "AUGUST", "SEPTEMBER", "OCTOBER", "NOVEMBER", "DECEMBER"];

async function initSupervisorDashboard() {
    try {
        const data = await fetchAPI('timesheet/supervisor/dashboard');
        if (!data) return;

        renderBanner(data);
        renderTopPendingApprovals(data.pendingApprovals);
        renderPivotTable(data.internHoursBreakdown);
        renderAllocationDonut(data.projectAllocations);
        renderLiveActivity(data.recentActivity);
        setupDropdownFilter(data.internHoursBreakdown);

    } catch (err) {
        console.error("Dashboard Init Error:", err);
    }
}

function renderBanner(data) {
    const welcomeEl = document.getElementById('welcome_text');
    const activeLabel = document.getElementById('active_intern_count_label');
    
    if (welcomeEl && data.supervisorName) welcomeEl.innerText = `Good Morning, ${data.supervisorName}`;
    if (activeLabel) activeLabel.innerText = `${data.totalActiveInterns || 0} active members`;
}


function renderTopPendingApprovals(pendingApprovals) {
    const container = document.getElementById('top_pending_container');
    const badge = document.getElementById('pending_count_badge');
    if (!container) return;

    const total = pendingApprovals ? pendingApprovals.length : 0;
    if (badge) badge.textContent = total;

    if (!pendingApprovals || pendingApprovals.length === 0) {
        container.innerHTML = `
            <div class="text-center py-8" style="color: #C4CAD4; font-size: 0.85rem; font-weight: 600;">
                <i class="bi bi-check-circle-fill text-success fs-2 mb-3 d-block"></i>
                All caught up! No pending approvals.
            </div>`;
        return;
    }

    const top3 = pendingApprovals.slice(0, 3);
    const MONTHS = ["January","February","March","April","May","June","July","August","September","October","November","December"];

    const html = top3.map(item => {
        const initials = (item.employeeName || '?').split(' ').map(w => w[0]).join('').substring(0, 2).toUpperCase();
        const period = `${MONTHS[(item.month || 1) - 1]} ${item.year}`;
        const reviewUrl = item.submissionId > 0
            ? `/Timesheet/Supervisor/Review?id=${item.submissionId}`
            : `/Timesheet/Supervisor/Review?employeeId=${item.employeeId}&month=${item.month}&year=${item.year}`;
        return `
            <div class="missing-dash-row" style="display:flex; align-items:center; gap:14px; padding: 14px 0; border-bottom: 1px solid #F4F6FA;">
                <div style="width:40px; height:40px; border-radius:10px; background:#FFF5F8; color:#D31145; display:flex; align-items:center; justify-content:center; font-weight:900; font-size:0.9rem; flex-shrink:0; border:1px solid #FFD6E3;">
                    ${initials}
                </div>
                <div style="flex:1; min-width:0;">
                    <div style="font-weight:800; color:#181C32; font-size:0.95rem; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;">${item.employeeName}</div>
                    <div style="font-size:0.78rem; color:#9EA5B2; font-weight:600; margin-top:2px;">${period}</div>
                </div>
                <a href="${reviewUrl}" class="btn btn-sm btn-light-danger fw-boldest" style="border-radius:8px; white-space:nowrap; font-size:0.8rem;">
                    Review <i class="bi bi-arrow-right ms-1"></i>
                </a>
            </div>
        `;
    }).join('');

    container.innerHTML = `<div style="padding: 0 4px;">${html}</div>`;
}



function renderPivotTable(breakdown) {
    const headerRow = document.getElementById('breakdown_header_row');
    const tbody = document.getElementById('breakdown_body');
    if (!headerRow || !tbody) return;

    if (!breakdown || breakdown.length === 0) {
        headerRow.innerHTML = `<th>ONGOING PROJECTS</th><th>STATUS</th>`;
        tbody.innerHTML = '<tr><td colspan="2" class="text-center py-10 opacity-25">No activity found</td></tr>';
        return;
    }

    // PERFORMANCE: Use Set for O(1) project collection
    const projectsSet = new Set();
    breakdown.forEach(b => {
        if(b.projectMinutes) {
            Object.keys(b.projectMinutes).forEach(p => projectsSet.add(p));
        }
    });

    const projectsArr = Array.from(projectsSet).sort();

    // PERFORMANCE: Pre-calculate Header HTML
    let headerHTML = '<th>ONGOING PROJECTS</th>' + 
                     breakdown.map(i => `<th>${i.employeeName}</th>`).join('');
    headerRow.innerHTML = headerHTML;

    // PERFORMANCE: Pre-calculate Body HTML using joined strings
    const bodyHTML = projectsArr.map(project => {
        const cells = breakdown.map(intern => {
            const mins = (intern.projectMinutes && intern.projectMinutes[project]) ? intern.projectMinutes[project] : 0;
            const hrs = (mins / 60).toFixed(1);
            return (mins === 0) ? '<td><span class="val-zero">0</span></td>' : `<td><span class="fw-boldest text-dark">${hrs}</span></td>`;
        }).join('');
        return `<tr><td>${project}</td>${cells}</tr>`;
    }).join('');

    tbody.innerHTML = bodyHTML;
}

function renderAllocationDonut(allocations) {
    const canvas = document.getElementById('allocation_donut_chart');
    if(!canvas) return;
    
    const legendContainer = document.getElementById('donut_legends');
    if(legendContainer) legendContainer.innerHTML = '';
    if (allocationChart) allocationChart.destroy();

    if (!allocations || allocations.length === 0) {
        if(legendContainer) legendContainer.innerHTML = '<div class="text-muted text-center py-20 fs-7 fw-boldest">NO USAGE DATA FOUND.</div>';
        const valueEl = document.getElementById('donut_total_value');
        if(valueEl) valueEl.innerText = "0%";
        
        // Render Empty Gray Ring Placeholder
        allocationChart = new Chart(canvas.getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: ['No Data'],
                datasets: [{
                    data: [100],
                    backgroundColor: ['#F1F4F9'],
                    borderWidth: 0
                }]
            },
            options: { cutout: '80%', plugins: { legend: { display: false }, tooltip: { enabled: false } }, maintainAspectRatio: false }
        });
        return;
    }

    // Calculate Actual Total and Update Center Text
    const total = allocations.reduce((sum, item) => sum + (item.allocationPercentage || 0), 0);
    const valueEl = document.getElementById('donut_total_value');
    if(valueEl) valueEl.innerText = `${total}%`;

    if(legendContainer) {
        allocations.forEach((item, idx) => {
            const color = AIA_COLORS[idx % AIA_COLORS.length];
            legendContainer.innerHTML += `
                <div class="legend-pill">
                    <div class="legend-pill-left">
                        <div class="legend-dot" style="background: ${color};"></div>
                        ${item.projectName}
                    </div>
                    <div class="legend-val">${item.allocationPercentage}%</div>
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
                hoverOffset: 15
            }]
        },
        options: {
            cutout: '80%',
            plugins: { legend: { display: false } },
            maintainAspectRatio: false
        }
    });
}

function setupDropdownFilter(breakdown) {
    const dropdown = document.getElementById('intern_filter_dropdown');
    if(!dropdown) return;
    dropdown.innerHTML = '<option value="">All Intern</option>';
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
    const token = localStorage.getItem('aia_jwt_token');
    const response = await fetch(`https://localhost:7089/api/${endpoint}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    if (response.status === 401) { 
        console.error('401 Unauthorized in dashboard');
        // window.location.href = '/Account/Login'; 
        return null; 
    }
    const json = await response.json();
    return json.content || json.Content || json.data || json;
}

function renderLiveActivity(activities) {
    const tbody = document.getElementById('live_activity_tbody');
    if (!tbody) return;

    if (!activities || activities.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center py-10 text-muted">No recent activity detected.</td></tr>';
        return;
    }

    tbody.innerHTML = activities.map(a => `
        <tr>
            <td>
                <div class="d-flex align-items-center">
                    <div class="symbol symbol-35px symbol-circle me-3">
                        <span class="symbol-label bg-light-danger text-danger fw-boldest">${a.employeeName.charAt(0)}</span>
                    </div>
                    <div class="d-flex flex-column">
                        <span class="text-dark fw-boldest fs-7">${a.employeeName}</span>
                        <span class="text-muted fs-9">Just now</span>
                    </div>
                </div>
            </td>
            <td><span class="badge badge-light-primary fw-boldest fs-9">${a.projectName}</span></td>
            <td><div class="text-gray-600 fs-7 text-truncate" style="max-width: 250px;" title="${a.taskDescription}">${a.taskDescription}</div></td>
            <td class="text-center"><span class="text-dark fw-boldest fs-7">${a.durationFormatted}</span></td>
            <td class="text-end text-muted fs-8 fw-bold">${a.entryDate}</td>
        </tr>
    `).join('');
}


document.addEventListener('DOMContentLoaded', () => {
    initSupervisorDashboard();
});
