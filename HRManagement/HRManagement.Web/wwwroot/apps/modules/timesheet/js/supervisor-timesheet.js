const API_BASE_URL = 'https://localhost:7089/api';
const token = localStorage.getItem('aia_jwt_token');

let currentDate = new Date();
let pickerDate = new Date();
let activeView = 'monthly';
let selectedInternId = null;
let internsList = [];

// ── GLOBAL EXPOSURE ─────────────────────────────────────────────────────────

window.switchView = switchView;
window.moveDate = moveDate;
window.toggleDatePopup = toggleDatePopup;
window.closeDatePopup = closeDatePopup;
window.movePickerTime = movePickerTime;
window.executePickerSelection = executePickerSelection;
window.selectMonth = selectMonth;
window.selectIntern = selectIntern;

const indonesianMonths = ["Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "November", "Desember"];
const indonesianDaysShort = ["Min", "Sen", "Sel", "Rab", "Kam", "Jum", "Sab"];

// ── CORE API ENGINE ──────────────────────────────────────────────────────────

async function fetchAPI(endpoint, options = {}) {
    try {
        const response = await fetch(`${API_BASE_URL}/${endpoint}`, {
            ...options,
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
                ...options.headers
            }
        });
        const result = await response.json();
        if (!response.ok) {
            throw new Error(result.message || 'Access Denied');
        }
        return result.data;
    } catch (error) {
        console.error('API Error:', error);
        return null;
    }
}

// ── INTERN DROPDOWN LOGIC ───────────────────────────────────────────────────

async function loadInterns() {
    const dropdown = document.getElementById('intern_list_dropdown');
    const supervisorLabel = document.getElementById('supervisor_name_label');
    if (!dropdown || !supervisorLabel) return;
    
    // Attempt loading intern list from dashboard
    const data = await fetchAPI('timesheet/supervisor/dashboard');
    if (data && data.internHoursBreakdown && Array.isArray(data.internHoursBreakdown)) {
        internsList = data.internHoursBreakdown;
        supervisorLabel.innerText = data.supervisorName || '---';
        
        if (internsList.length > 0) {
            dropdown.innerHTML = internsList.map(intern => {
                const escapedName = intern.employeeName.replace(/'/g, "\\'");
                return `
                    <li><a class="dropdown-item ${selectedInternId == intern.employeeId ? 'active' : ''}" 
                       onclick="selectIntern(${intern.employeeId}, '${escapedName}')">
                       ${intern.employeeName}
                    </a></li>
                `;
            }).join('');
            
            // Auto-select first intern if none already selected
            if (selectedInternId === null) {
                selectIntern(internsList[0].employeeId, internsList[0].employeeName);
            }
        } else {
            dropdown.innerHTML = '<li><div class="px-4 py-2 text-muted fw-bold small">No interns assigned</div></li>';
        }
    } else {
        dropdown.innerHTML = '<li><div class="px-4 py-2 text-muted fw-bold small text-danger">Fetch failed: check connectivity</div></li>';
    }
}

function selectIntern(id, name) {
    if (!id || !name) return;
    
    selectedInternId = id;
    const internInput = document.getElementById('selected_intern_id');
    const internLabel = document.getElementById('intern_dropdown_label');
    
    if (internInput) internInput.value = id;
    if (internLabel) internLabel.innerText = name;
    
    // Update active highlight in dropdown
    document.querySelectorAll('#intern_list_dropdown .dropdown-item').forEach(item => {
        if (item.innerText.trim() === name) item.classList.add('active');
        else item.classList.remove('active');
    });

    renderCurrentState();
}

// ── NAVIGATION & VIEW SWITCHING ─────────────────────────────────────────────

function switchView(viewName, btn) {
    // Stop event propagation to prevent intercept by Metronic if applicable
    if (window.event) {
        window.event.preventDefault();
        window.event.stopPropagation();
    }
    
    console.log("Switching view to:", viewName);
    activeView = viewName;
    
    // Update Tab UI Class (using a more specific ts-nav-link to avoid conflicts)
    const tabs = document.querySelectorAll('.timesheet-tabs-nav .ts-nav-link');
    tabs.forEach(b => b.classList.remove('active'));
    if (btn) {
        btn.classList.add('active');
    } else {
        // Fallback: find it by data-view attribute or text
        tabs.forEach(b => {
             if (b.innerText.toLowerCase() === viewName.toLowerCase()) b.classList.add('active');
        });
    }

    // Toggle Port visibility
    document.querySelectorAll('.timesheet-view').forEach(v => v.classList.add('d-none'));
    const target = document.getElementById('view_' + viewName);
    if (target) {
        target.classList.remove('d-none');
    }

    renderCurrentState();
}

function moveDate(offset) {
    if (activeView === 'monthly') currentDate.setMonth(currentDate.getMonth() + offset);
    else if (activeView === 'weekly') currentDate.setDate(currentDate.getDate() + (offset * 7));
    else currentDate.setDate(currentDate.getDate() + offset);
    renderCurrentState();
}

function renderCurrentState() {
    updateDateLabel();
    if (selectedInternId === null) {
        console.warn("No intern selected yet for view update");
        return;
    }

    if (activeView === 'monthly') renderMonthlyGrid();
    else if (activeView === 'weekly') renderWeeklyGrid();
    else if (activeView === 'daily') renderDailyGrid();
}

function updateDateLabel() {
    const label = document.getElementById('current_view_label');
    if (!label) return;

    if (activeView === 'monthly') {
        label.innerText = `${indonesianMonths[currentDate.getMonth()]} ${currentDate.getFullYear()}`;
    } else if (activeView === 'weekly') {
        const start = new Date(currentDate);
        const end = new Date(currentDate);
        end.setDate(start.getDate() + 6);
        label.innerText = `${start.getDate()} ${indonesianMonths[start.getMonth()].substr(0, 3)} - ${end.getDate()} ${indonesianMonths[end.getMonth()].substr(0, 3)} ${end.getFullYear()}`;
    } else {
        label.innerText = `${indonesianMonths[currentDate.getMonth()]} ${currentDate.getDate()}, ${currentDate.getFullYear()}`;
    }
}

// ── GRID RENDERING (SAME AS EMPLOYEE BUT WITH targetEmployeeId) ──────────────

async function renderMonthlyGrid() {
    const container = document.getElementById('monthly_grid_container');
    if (!container) return;
    container.innerHTML = `<div class="w-100 text-center p-10"><span class="spinner-border spinner-border-sm me-2 text-aia"></span>Reading logs...</div>`;

    const y = currentDate.getFullYear(), m = currentDate.getMonth();
    const data = await fetchAPI(`timesheet/monthly?year=${y}&month=${m + 1}&targetEmployeeId=${selectedInternId}`);

    container.innerHTML = "";
    const firstDay = new Date(y, m, 1).getDay();
    const totalDays = new Date(y, m + 1, 0).getDate();
    const offset = (firstDay === 0) ? 6 : firstDay - 1;

    for (let i = 0; i < offset; i++) container.innerHTML += '<div class="grid-cell outside"><span class="cell-date-num">-</span></div>';

    for (let d = 1; d <= totalDays; d++) {
        const dObj = new Date(y, m, d);
        const isToday = (dObj.toDateString() === new Date().toDateString());
        const isWeekend = (dObj.getDay() === 0 || dObj.getDay() === 6);
        const dayData = data?.days?.find(day => new Date(day.date).getDate() === d);
        const hasWork = dayData && dayData.totalMinutes > 0;

        container.innerHTML += `
            <div class="grid-cell ${isToday ? 'is-today' : ''} ${hasWork ? 'has-data' : ''}">
                <span class="cell-date-num ${isWeekend ? 'text-danger' : ''}">${d}</span>
                ${hasWork ? '<div class="data-indicator"></div>' : ''}
            </div>`;
    }
}

async function renderWeeklyGrid() {
    const headerRow = document.getElementById('weekly_header_row');
    const tableBody = document.getElementById('weekly_log_tbody');
    if (!headerRow || !tableBody) return;

    const dateStr = currentDate.toISOString().split('T')[0];
    const data = await fetchAPI(`timesheet/weekly?weekStartDate=${dateStr}&targetEmployeeId=${selectedInternId}`);

    const middleHeaders = Array.from(headerRow.children).slice(1, -1);
    middleHeaders.forEach((col, idx) => {
        const d = new Date(currentDate);
        d.setDate(currentDate.getDate() + idx);
        const dayLabel = indonesianDaysShort[d.getDay()].toUpperCase();
        const dateString = `${indonesianMonths[d.getMonth()].substr(0, 3)} ${d.getDate()}`;
        const isToday = (d.toDateString() === new Date().toDateString());
        const isWeekend = (d.getDay() === 0 || d.getDay() === 6);
        col.className = isToday ? "today-col-highlight" : (isWeekend ? "weekend-th" : "");
        col.innerHTML = `<div class="day-title-wrap"><span class="day-short-label ${isWeekend ? 'text-aia' : ''}">${dayLabel}</span><span class="day-full-date ${isWeekend ? 'text-aia' : ''}">${dateString}</span></div>`;
    });

    if (!data || !data.projects || data.projects.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="9" class="text-center p-10 text-muted">No logs recorded for this week.</td></tr>';
        return;
    }

    tableBody.innerHTML = data.projects.map(proj => `
        <tr>
            <td class="p-4"><span class="project-title-large text-start d-block">${proj.projectName}</span></td>
            ${Array.from({ length: 7 }).map((_, idx) => {
                const d = new Date(currentDate);
                d.setDate(currentDate.getDate() + idx);
                const mins = proj.dailyMinutes[d.toISOString().split('T')[0]] || 0;
                return `<td class="p-4 text-center"><span class="duration-text-large">${mins > 0 ? (Math.floor(mins / 60) + 'h ' + (mins % 60) + 'm') : '-'}</span></td>`;
            }).join('')}
            <td class="p-4 text-center weekly-total-cell">${proj.weeklyTotalFormatted}</td>
        </tr>
    `).join('');
}

async function renderDailyGrid() {
    const tableBody = document.getElementById('daily_log_tbody');
    if (!tableBody) return;

    tableBody.innerHTML = '<tr><td colspan="6" class="text-center p-10">Searching activities...</td></tr>';

    const dateStr = currentDate.toISOString().split('T')[0];
    const data = await fetchAPI(`timesheet/daily?date=${dateStr}&targetEmployeeId=${selectedInternId}`);

    if (!data || !data.entries || data.entries.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="6" class="text-center p-10 text-muted">No entries found for this specific date.</td></tr>';
        return;
    }

    tableBody.innerHTML = data.entries.map(entry => `
        <tr class="align-middle">
            <td class="p-4 text-center"><span class="duration-text-large">${entry.durationFormatted}</span></td>
            <td class="p-4 text-start"><span class="project-title-large">${entry.projectName}</span></td>
            <td class="p-4 text-start"><span class="text-gray-600">${entry.applicationUsed || '-'}</span></td>
            <td class="p-4 text-start"><div style="max-width:300px" class="text-gray-600">${entry.taskDescription}</div></td>
            <td class="p-4 text-start"><span class="text-gray-600">${entry.projectLeadName}</span></td>
            <td class="p-4 text-start"><span class="text-gray-600">${entry.location}</span></td>
        </tr>
    `).join('');
}

// ── DATE PICKER DIALOG ENGINE ─────────────────────────────────────────────

function toggleDatePopup(event) {
    if (event) {
        event.stopPropagation();
        event.preventDefault();
    }
    closeDatePopup();
    pickerDate = new Date(currentDate);
    const popover = document.getElementById('popover_' + activeView);
    const backdrop = document.getElementById('datepicker_focal_backdrop');
    if (popover && backdrop) {
        renderPickerLayout(popover);
        popover.classList.add('active');
        backdrop.classList.add('active');
    }
}

function renderPickerLayout(popover) {
    if (activeView === 'monthly') {
        const label = popover.querySelector('#picker_year_label');
        if (label) label.innerText = pickerDate.getFullYear();
    } else {
        const grid = popover.querySelector('.datepicker-grid-7');
        const headerText = popover.querySelector('.datepicker-header-title');
        if (!grid || !headerText) return;

        headerText.innerText = `${indonesianMonths[pickerDate.getMonth()]} ${pickerDate.getFullYear()}`;
        const y = pickerDate.getFullYear(), m = pickerDate.getMonth();
        const firstDay = new Date(y, m, 1).getDay();
        const totalDays = new Date(y, m + 1, 0).getDate();

        grid.innerHTML = '<div class="datepicker-day-head">MO</div><div class="datepicker-day-head">TU</div><div class="datepicker-day-head">WE</div><div class="datepicker-day-head">TH</div><div class="datepicker-day-head">FR</div><div class="datepicker-day-head">SA</div><div class="datepicker-day-head">SU</div>';
        const offset = (firstDay === 0) ? 6 : firstDay - 1;
        for (let i = 0; i < offset; i++) grid.innerHTML += '<div class="datepicker-day-cell text-muted opacity-25">-</div>';

        for (let i = 1; i <= totalDays; i++) {
            const thisDate = new Date(y, m, i);
            let stateClass = "";

            if (activeView === 'weekly') {
                const rangeEnd = new Date(currentDate);
                rangeEnd.setDate(currentDate.getDate() + 6);
                if (thisDate.toDateString() === currentDate.toDateString()) stateClass = "range-start";
                else if (thisDate.toDateString() === rangeEnd.toDateString()) stateClass = "range-end";
                else if (thisDate > currentDate && thisDate < rangeEnd) stateClass = "range-mid";
            } else if (activeView === 'daily' && thisDate.toDateString() === currentDate.toDateString()) {
                stateClass = "selected";
            }

            grid.innerHTML += `<div class="datepicker-day-cell ${stateClass}" onclick="event.stopPropagation(); executePickerSelection(${i})">${i}</div>`;
        }
    }
}

function movePickerTime(offset) {
    (activeView === 'monthly') ? pickerDate.setFullYear(pickerDate.getFullYear() + offset) : pickerDate.setMonth(pickerDate.getMonth() + offset);
    renderPickerLayout(document.getElementById('popover_' + activeView));
}

function executePickerSelection(day) {
    currentDate = new Date(pickerDate.getFullYear(), pickerDate.getMonth(), day);
    renderCurrentState();
    closeDatePopup();
}

function selectMonth(monthShort) {
    const monthsShort = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    currentDate = new Date(pickerDate.getFullYear(), monthsShort.indexOf(monthShort), 1);
    renderCurrentState();
    closeDatePopup();
}

function closeDatePopup() {
    document.querySelectorAll('.datepicker-popover').forEach(p => p.classList.remove('active'));
    const backdrop = document.getElementById('datepicker_focal_backdrop');
    if (backdrop) backdrop.classList.remove('active');
}

// ── BOOTSTRAP INITIALIZATION ────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', () => {
    // Priority 1: Intern loading
    loadInterns().then(() => {
        renderCurrentState();
    }).catch(err => {
        console.error("Critical Init Error:", err);
    });

    // Cleanup background clicks
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.datepicker-popover') && !e.target.closest('.current-date-title')) {
            closeDatePopup();
        }
    });
});
