const API_BASE_URL = 'https://localhost:7089/api/timesheet';

let currentDate = new Date();
let pickerDate = new Date();
let activeView = 'monthly';
let employeeInfo = null;

const indonesianMonths = ["Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "November", "Desember"];
const indonesianDays = ["Minggu", "Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu"];
const indonesianDaysShort = ["Min", "Sen", "Sel", "Rab", "Kam", "Jum", "Sab"];

async function fetchAPI(endpoint, options = {}) {
    try {
        const token = localStorage.getItem('aia_jwt_token');
        const response = await fetch(`${API_BASE_URL}/${endpoint}`, {
            ...options,
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': token ? `Bearer ${token}` : '',
                ...options.headers
            }
        });
        const result = await response.json();
        if (!response.ok) {
            throw new Error(result.message || result.statusMessage || 'Access Denied: Please contact your administrator.');
        }
        // Handle various response wrappers used across AIA systems
        return result.data || result.content || result.Content;
    } catch (error) {
        console.error('API Error:', error);
        showToast(error.message, 'error');
        return null;
    }
}

function showToast(message, type = 'error') {
    let container = document.getElementById('aia_toast_container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'aia_toast_container';
        container.className = 'aia-toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `aia-toast ${type}`;
    toast.innerHTML = `
        <i class="bi bi-exclamation-circle-fill fs-4 text-aia"></i>
        <div class="toast-content">
            <span class="toast-title">System Error</span>
            <span class="toast-message">${message}</span>
        </div>
    `;

    container.appendChild(toast);
    setTimeout(() => {
        toast.style.opacity = '0';
        setTimeout(() => toast.remove(), 500);
    }, 4500);
}

function switchView(viewName, btn) {
    activeView = viewName;
    document.querySelectorAll('.timesheet-tabs-nav .nav-link').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');

    document.querySelectorAll('.timesheet-view').forEach(v => v.classList.add('d-none'));
    const target = document.getElementById('view_' + viewName);
    if (target) target.classList.remove('d-none');

    // UPDATE MAIN ACTION BUTTON CONTEXTUALLY
    const mainBtn = document.getElementById('main_action_btn');
    if (mainBtn) {
        // If a supervisor is viewing (has selectedInternId), we handle the button differently
        if (window.selectedInternId) {
            if (viewName === 'monthly') {
                mainBtn.classList.remove('d-none');
                mainBtn.innerHTML = '<i class="bi bi-check-circle-fill me-2"></i> REVIEW SUBMISSION';
                mainBtn.onclick = () => window.location.href = '/Timesheet/Supervisor/Review';
                mainBtn.removeAttribute('data-bs-toggle');
                mainBtn.removeAttribute('data-bs-target');
            } else {
                // Hide for weekly/daily in supervisor view
                mainBtn.classList.add('d-none');
            }
        } else {
            // EMPLOYEE VIEW LOGIC
            if (viewName === 'weekly') {
                mainBtn.classList.add('d-none');
            } else if (viewName === 'daily') {
                mainBtn.classList.remove('d-none');
                mainBtn.innerHTML = '<i class="bi bi-pencil-square"></i> EDIT TIMESHEET';
                mainBtn.removeAttribute('data-bs-toggle');
                mainBtn.removeAttribute('data-bs-target');
                mainBtn.onclick = () => window.location.href = '/Timesheet/Employee/Entry';
            } else {
                // BACK TO MONTHLY (SUBMIT)
                mainBtn.classList.remove('d-none');
                mainBtn.innerHTML = '<i class="bi bi-send-fill"></i> SUBMIT APPROVAL';
                mainBtn.setAttribute('data-bs-toggle', 'modal');
                mainBtn.setAttribute('data-bs-target', '#modal_review_submit');
                mainBtn.onclick = null;
            }
        }
    }

    renderCurrentState();
}

function renderCurrentState() {
    updateDateLabel();
    if (activeView === 'monthly') renderMonthlyGrid();
    if (activeView === 'weekly') renderWeeklyGrid();
    if (activeView === 'daily') renderDailyGrid();
}

function updateDateLabel() {
    const label = document.getElementById('current_view_label');
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

function moveDate(offset) {
    if (activeView === 'monthly') currentDate.setMonth(currentDate.getMonth() + offset);
    else if (activeView === 'weekly') currentDate.setDate(currentDate.getDate() + (offset * 7));
    else currentDate.setDate(currentDate.getDate() + offset);
    renderCurrentState();
}

async function renderMonthlyGrid() {
    const container = document.getElementById('monthly_grid_container');
    if (!container) return;
    container.innerHTML = `<div class="w-100 text-center p-10">Fetching ${indonesianMonths[currentDate.getMonth()]} data...</div>`;

    const y = currentDate.getFullYear(), m = currentDate.getMonth();
    const targetParam = window.selectedInternId ? `&targetEmployeeId=${window.selectedInternId}` : '';
    const data = await fetchAPI(`monthly?year=${y}&month=${m + 1}${targetParam}`);

    container.innerHTML = "";
    const firstDay = new Date(y, m, 1).getDay();
    const totalDays = new Date(y, m + 1, 0).getDate();
    const offset = (firstDay === 0) ? 6 : firstDay - 1;

    for (let i = 0; i < offset; i++) container.innerHTML += '<div class="grid-cell outside"><span class="cell-date-num">-</span></div>';

    for (let d = 1; d <= totalDays; d++) {
        const dObj = new Date(y, m, d);
        const isToday = (dObj.toDateString() === new Date().toDateString());
        const dateParam = `${y}-${(m + 1).toString().padStart(2, '0')}-${d.toString().padStart(2, '0')}`;

        // Find data for this specific day from API result
        const dayData = data?.days?.find(day => new Date(day.date).getDate() === d);
        const hasWork = dayData && dayData.totalMinutes > 0;

        container.innerHTML += `
            <div class="grid-cell ${isToday ? 'is-today' : ''} ${hasWork ? 'has-data' : ''}" 
                 onclick="${window.selectedInternId ? '' : `window.location.href='/Timesheet/Employee/Entry?date=${dateParam}'`}">
                <span class="cell-date-num">${d}</span>
                ${hasWork ? '<div class="data-indicator"></div>' : ''}
            </div>`;
    }
}

async function renderWeeklyGrid() {
    const headerRow = document.getElementById('weekly_header_row');
    const tableBody = document.querySelector('.weekly-master-table tbody');
    if (!headerRow || !tableBody) return;

    const dateStr = currentDate.toISOString().split('T')[0];
    const targetParam = window.selectedInternId ? `&targetEmployeeId=${window.selectedInternId}` : '';
    const data = await fetchAPI(`weekly?weekStartDate=${dateStr}${targetParam}`);

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
        tableBody.innerHTML = '<tr><td colspan="9" class="text-center p-10 text-muted">No projects found for this week.</td></tr>';
        return;
    }

    tableBody.innerHTML = data.projects.map(proj => `
        <tr>
            <td class="p-4"><span class="project-title-large">${proj.projectName}</span></td>
            ${Array.from({ length: 7 }).map((_, idx) => {
        const d = new Date(currentDate);
        d.setDate(currentDate.getDate() + idx);
        const dKey = d.toISOString().split('T')[0];
        const mins = proj.dailyMinutes[dKey] || 0;
        const isToday = (d.toDateString() === new Date().toDateString());
        const isWeekend = (d.getDay() === 0 || d.getDay() === 6);
        let cls = isToday ? 'today-cell-highlight' : (isWeekend ? 'weekend-td text-danger opacity-50' : '');
        return `<td class="p-4 text-center ${cls}" style="cursor:pointer" onclick="${window.selectedInternId ? '' : `window.location.href='/Timesheet/Employee/Entry?date=${dKey}'`}">
                            <span class="duration-text-large">${mins > 0 ? (Math.floor(mins / 60) + 'h ' + (mins % 60) + 'm') : '-'}</span>
                        </td>`;
    }).join('')}
            <td class="p-4 text-center weekly-total-cell">${proj.weeklyTotalFormatted}</td>
        </tr>
    `).join('');
}

async function renderDailyGrid() {
    const tableBody = document.getElementById('view_daily_tbody');
    if (!tableBody) return;

    // UI Loading state (without changing design)
    tableBody.innerHTML = '<tr><td colspan="6" class="text-center p-10">Loading entries...</td></tr>';

    const dateStr = currentDate.toISOString().split('T')[0];
    const targetParam = window.selectedInternId ? `&targetEmployeeId=${window.selectedInternId}` : '';
    const data = await fetchAPI(`daily?date=${dateStr}${targetParam}`);

    if (!data || !data.entries || data.entries.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="6" class="text-center p-10 text-muted">No entries found for this date.</td></tr>';
        return;
    }

    tableBody.innerHTML = data.entries.map(entry => `
        <tr class="align-middle">
            <td class="p-4 text-center"><span class="duration-text-large">${entry.durationFormatted}</span></td>
            <td class="p-4"><span class="project-title-large">${entry.projectName}</span></td>
            <td class="p-4"><span class="text-gray-600">${entry.applicationUsed || '-'}</span></td>
            <td class="p-4"><div style="max-width:300px" class="text-gray-600">${entry.taskDescription}</div></td>
            <td class="p-4"><span class="text-gray-600">${entry.projectLeadName}</span></td>
            <td class="p-4"><span class="text-gray-600">${entry.location}</span></td>
        </tr>
    `).join('');
}

async function syncProfileInfo() {
    const nameLabels = document.querySelectorAll('.info-person-name');
    if (nameLabels.length === 0) return;

    const data = await fetchAPI('dashboard');
    if (data) {
        employeeInfo = data;
        // The first label is usually the Intern/Employee name in our info cards
        if (nameLabels[0]) nameLabels[0].innerText = data.employeeName;
    }
}

function toggleDatePopup(event) {
    event.stopPropagation();
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
        const header = popover.querySelector('.datepicker-header-title');
        if (!grid || !header) return;

        header.innerText = `${indonesianMonths[pickerDate.getMonth()]} ${pickerDate.getFullYear()}`;
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
    document.getElementById('datepicker_focal_backdrop').classList.remove('active');
}

// DURATION SPINNER LOGIC (AIA PREMIUM)
let selectedHH = "00";
let selectedMM = "00";

function toggleDurationPicker(input, event) {
    event.stopPropagation();
    const popover = input.closest('td').querySelector('.duration-picker-popover');
    const isActive = popover.classList.contains('active');

    // CLOSE ALL TYPES OF POPOVERS
    document.querySelectorAll('.duration-picker-popover, .aia-select-popover').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.aia-select-trigger').forEach(t => t.classList.remove('active'));

    if (!isActive) popover.classList.add('active');

    // Parse current value
    const match = input.value.match(/(\d+)h\s+(\d+)m/);
    if (match) {
        selectedHH = match[1];
        selectedMM = match[2];
    }
}

function selectHH(el, h) {
    selectedHH = h.toString().padStart(2, '0');
    el.closest('.duration-items-scroll').querySelectorAll('.duration-item').forEach(i => i.classList.remove('selected'));
    el.classList.add('selected');

    // INSTANT UPDATE
    const input = el.closest('td').querySelector('.duration-trigger');
    if (input) {
        input.value = `${selectedHH}h ${selectedMM}m`;
        calculateTotalLogHours();
    }
}

function selectMM(el, m) {
    selectedMM = m.toString().padStart(2, '0');
    el.closest('.duration-items-scroll').querySelectorAll('.duration-item').forEach(i => i.classList.remove('selected'));
    el.classList.add('selected');

    // INSTANT UPDATE
    const input = el.closest('td').querySelector('.duration-trigger');
    if (input) {
        input.value = `${selectedHH}h ${selectedMM}m`;
        calculateTotalLogHours();
    }

    // AUTO CLOSE (FINAL STEP)
    el.closest('.duration-picker-popover').classList.remove('active');
}

function confirmDuration(btn) {
    const input = btn.closest('td').querySelector('.duration-trigger');
    if (input) {
        input.value = `${selectedHH}h ${selectedMM}m`;
    }
    btn.closest('.duration-picker-popover').classList.remove('active');
}

function toggleAiaSelect(trigger, event) {
    event.stopPropagation();
    const wrap = trigger.closest('.aia-custom-select-wrap');
    const popover = wrap.querySelector('.aia-select-popover');
    const isActive = popover.classList.contains('active');

    // CLOSE EVERYTHING ELSE FIRST
    document.querySelectorAll('.aia-select-popover, .duration-picker-popover').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.aia-select-trigger').forEach(t => t.classList.remove('active'));

    if (!isActive) {
        popover.classList.add('active');
        trigger.classList.add('active');
    }
}

function selectAiaOption(option) {
    const wrap = option.closest('.aia-custom-select-wrap');
    const trigger = wrap.querySelector('.aia-select-trigger');
    const hiddenInput = wrap.querySelector('.aia-select-value');
    const val = option.getAttribute('data-value');
    const lead = option.getAttribute('data-lead');

    trigger.querySelector('.trigger-text').innerText = val;
    hiddenInput.value = val;

    if (lead) {
        const leadInput = option.closest('tr').querySelector('.bg-light-gray');
        if (leadInput) leadInput.value = lead;
    }

    wrap.querySelectorAll('.aia-select-option').forEach(o => o.classList.remove('selected'));
    option.classList.add('selected');

    wrap.querySelector('.aia-select-popover').classList.remove('active');
    trigger.classList.remove('active');
}

function addNewLogEntry() {
    const tbody = document.getElementById('log_entry_tbody');
    if (!tbody) return;
    const row = document.createElement('tr');
    row.className = "align-middle";
    row.innerHTML = `
        <td class="p-4 position-relative">
            <div class="d-flex align-items-center justify-content-center gap-2">
                <input type="text" class="entry-input text-center duration-trigger" value="00h 00m" style="width:100px; cursor:pointer" readonly onclick="toggleDurationPicker(this, event)">
            </div>
            <div class="duration-picker-popover shadow-lg">
                <div class="duration-columns">
                    <div class="duration-col"><span class="duration-col-label">HH</span><div class="duration-items-scroll">${Array.from({ length: 24 }, (_, i) => `<div class="duration-item ${i === 0 ? 'selected' : ''}" onclick="selectHH(this, ${i})">${i.toString().padStart(2, '0')}</div>`).join('')}</div></div>
                    <div class="duration-col"><span class="duration-col-label">MM</span><div class="duration-items-scroll">${Array.from({ length: 12 }, (_, i) => `<div class="duration-item ${i === 0 ? 'selected' : ''}" onclick="selectMM(this, ${i * 5})">${(i * 5).toString().padStart(2, '0')}</div>`).join('')}</div></div>
                </div>
            </div>
        </td>
        <td class="p-4">
            <div class="aia-custom-select-wrap">
                <div class="aia-select-trigger" onclick="toggleAiaSelect(this, event)"><span class="trigger-text">Select Project</span><i class="bi bi-chevron-down"></i></div>
                <div class="aia-select-popover">
                    <div class="aia-select-option" data-value="Insurable Interest" data-lead="Novia" onclick="selectAiaOption(this)">Insurable Interest</div>
                    <div class="aia-select-option" data-value="Click Revamp" data-lead="Hansen" onclick="selectAiaOption(this)">Click Revamp</div>
                    <div class="aia-select-option" data-value="iRecruit" data-lead="Hansen" onclick="selectAiaOption(this)">iRecruit 3.0</div>
                </div>
                <input type="hidden" class="aia-select-value" value="">
            </div>
        </td>
        <td class="p-4"><input type="text" class="entry-input" placeholder="App Used"></td>
        <td class="p-4"><textarea class="entry-input" style="height:48px; min-height:48px" placeholder="Describe what you do...."></textarea></td>
        <td class="p-4"><input type="text" class="entry-input bg-light-gray" placeholder="Project Lead" readonly></td>
        <td class="p-4">
            <div class="aia-custom-select-wrap">
                <div class="aia-select-trigger" onclick="toggleAiaSelect(this, event)"><span class="trigger-text">Location</span><i class="bi bi-chevron-down"></i></div>
                <div class="aia-select-popover">
                    <div class="aia-select-option" data-value="AIA Central" onclick="selectAiaOption(this)">AIA Central</div>
                    <div class="aia-select-option" data-value="WFH" onclick="selectAiaOption(this)">WFH</div>
                </div>
                <input type="hidden" class="aia-select-value" value="">
            </div>
        </td>
        <td class="p-4 text-center">
            <button class="btn btn-icon btn-light-danger btn-sm rounded-circle h-35px w-35px" onclick="this.closest('tr').remove(); calculateTotalLogHours();"><i class="bi bi-trash-fill"></i></button>
        </td>
    `;
    tbody.appendChild(row);
    calculateTotalLogHours();
}

function calculateTotalLogHours() {
    const totalLabel = document.getElementById('total_log_hours');
    if (!totalLabel) return;

    let totalMinutes = 0;
    const inputs = document.querySelectorAll('.duration-trigger');
    
    inputs.forEach(input => {
        const val = input.value || "00h 00m";
        const match = val.match(/(\d+)h\s+(\d+)m/);
        if (match) {
            totalMinutes += parseInt(match[1]) * 60 + parseInt(match[2]);
        }
    });

    const hh = Math.floor(totalMinutes / 60);
    const mm = totalMinutes % 60;
    totalLabel.innerText = `${hh.toString().padStart(2, '0')}h ${mm.toString().padStart(2, '0')}m`;
}

document.addEventListener('click', (e) => {
    // Universal cleanup for any click outside popovers
    if (!e.target.closest('.duration-picker-popover') && !e.target.closest('.duration-trigger') && !e.target.closest('.aia-custom-select-wrap')) {
        document.querySelectorAll('.duration-picker-popover, .aia-select-popover').forEach(p => p.classList.remove('active'));
        document.querySelectorAll('.aia-select-trigger').forEach(t => t.classList.remove('active'));
    }
});

function initEntryPage() {
    const label = document.getElementById('entry_date_label');
    if (!label) return;

    const urlParams = new URLSearchParams(window.location.search);
    const dateStr = urlParams.get('date');
    let targetDate = new Date();

    // Check if valid date passed in URL (YYYY-MM-DD or similar)
    if (dateStr && dateStr.length >= 8) {
        const d = new Date(dateStr);
        if (!isNaN(d.getTime())) {
            targetDate = d;
        }
    }

    // FORMAL CORPORATE LABELS
    const englishDays = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    const englishMonths = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

    label.innerText = `${englishDays[targetDate.getDay()]}, ${targetDate.getDate()} ${englishMonths[targetDate.getMonth()]} ${targetDate.getFullYear()}`;

    // SYNC NAMES DYNAMICALLY
    syncProfileInfo();
    calculateTotalLogHours();
}

document.addEventListener('DOMContentLoaded', () => {
    // Dispatch inits
    setTimeout(() => {
        if (document.getElementById('monthly_grid_container')) renderCurrentState();
        if (document.getElementById('entry_date_label')) initEntryPage();
    }, 50);
});
