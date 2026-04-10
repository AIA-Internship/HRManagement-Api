const API_BASE_URL = 'https://127.0.0.1:7089/api';

// GLOBAL APP OBJECT (FALLBACK IF SHARED IS MISSING)
window.app = window.app || {
    loading: {
        show: function(msg) {
            let overlay = document.getElementById('app_loading_overlay');
            if (!overlay) {
                overlay = document.createElement('div');
                overlay.id = 'app_loading_overlay';
                overlay.style = "position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(255,255,255,0.7);z-index:9999;display:flex;flex-direction:column;align-items:center;justify-content:center;font-family:Inter,sans-serif;";
                overlay.innerHTML = `
                    <div class="spinner-border text-brand" role="status" style="width: 3rem; height: 3rem; color:#D31145"></div>
                    <div class="mt-4 fw-boldest text-gray-800 fs-4">${msg || 'Processing...'}</div>
                `;
                document.body.appendChild(overlay);
            }
            overlay.style.display = 'flex';
        },
        hide: function() {
            const overlay = document.getElementById('app_loading_overlay');
            if (overlay) overlay.style.display = 'none';
        }
    }
};

let currentDate = new Date();
let pickerDate = new Date();
let activeView = 'monthly';

let employeeInfo = null;

const englishMonths = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
const englishDays = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
const englishDaysShort = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];



function showToast(message, type = 'error') {
    if (window.Swal) {
        Swal.fire({
            title: type === 'error' ? 'Validation Error' : 'Success',
            text: message,
            icon: type,
            confirmButtonColor: '#D31145',
            timer: type === 'success' ? 3000 : undefined
        });
        return;
    }

    let container = document.getElementById('standard_toast_container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'standard_toast_container';
        container.className = 'standard-toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `standard-toast ${type}`;
    toast.innerHTML = `
        <i class="bi bi-exclamation-circle-fill fs-4 text-brand"></i>
        <div class="toast-content">
            <span class="toast-title">${type === 'error' ? 'System Error' : 'Success'}</span>
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
                mainBtn.innerHTML = '<i class="bi bi-check-circle-fill me-2"></i> Review Submission';
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
                mainBtn.innerHTML = '<i class="bi bi-pencil-square"></i> Edit Timesheet';
                mainBtn.removeAttribute('data-bs-toggle');
                mainBtn.removeAttribute('data-bs-target');
                mainBtn.onclick = () => window.location.href = '/Timesheet/Employee/Entry';
            } else {
                // BACK TO MONTHLY (SUBMIT)
                mainBtn.classList.remove('d-none');
                mainBtn.innerHTML = '<i class="bi bi-send-fill"></i> Submit Approval';
                mainBtn.setAttribute('data-bs-toggle', 'modal');
                mainBtn.setAttribute('data-bs-target', '#modal_review_submit');
                mainBtn.onclick = null;
            }
        }
    }

    renderCurrentState();
}

function jumpToDaily(dateStr) {
    currentDate = new Date(dateStr);
    const dailyBtn = Array.from(document.querySelectorAll('.timesheet-tabs-nav button')).find(b => b.innerText.toLowerCase().includes('daily'));
    if (dailyBtn) {
        switchView('daily', dailyBtn);
    } else {
        // Fallback if triggered from a page without tabs (like Entry)
        window.location.href = `/Timesheet/Employee/Management?view=daily&date=${dateStr}`;
    }
}

function renderCurrentState() {
    updateDateLabel();
    if (activeView === 'monthly') renderMonthlyGrid();
    if (activeView === 'weekly') renderWeeklyGrid();
    if (activeView === 'daily') renderDailyGrid();
}

function updateDateLabel(direction = 'right') {
    const label = document.getElementById('current_view_label');
    if (!label) return;

    // Apply animation
    label.classList.remove('slide-in-right', 'slide-in-left');
    void label.offsetWidth; // Trigger reflow
    label.classList.add(direction === 'right' ? 'slide-in-right' : 'slide-in-left');

    if (activeView === 'monthly') {
        label.innerText = `${englishMonths[currentDate.getMonth()]} ${currentDate.getFullYear()}`;
    } else if (activeView === 'weekly') {
        const start = new Date(currentDate);
        const end = new Date(currentDate);
        end.setDate(start.getDate() + 6);
        label.innerText = `${start.getDate()} ${englishMonths[start.getMonth()].substr(0, 3)} - ${end.getDate()} ${englishMonths[end.getMonth()].substr(0, 3)} ${end.getFullYear()}`;
    } else {
        label.innerText = `${englishMonths[currentDate.getMonth()]} ${currentDate.getDate()}, ${currentDate.getFullYear()}`;
    }
}

function moveDate(offset, direction = 'right') {
    if (activeView === 'monthly') currentDate.setMonth(currentDate.getMonth() + offset);
    else if (activeView === 'weekly') currentDate.setDate(currentDate.getDate() + (offset * 7));
    else currentDate.setDate(currentDate.getDate() + offset);
    
    updateDateLabel(direction);
    
    if (activeView === 'monthly') renderMonthlyGrid();
    if (activeView === 'weekly') renderWeeklyGrid();
    if (activeView === 'daily') renderDailyGrid();
}

async function renderMonthlyGrid() {
    const container = document.getElementById('monthly_grid_container');
    if (!container) return;
    container.innerHTML = `<div class="w-100 text-center p-10">Fetching ${englishMonths[currentDate.getMonth()]} data...</div>`;

    const y = currentDate.getFullYear(), m = currentDate.getMonth();
    const targetParam = window.selectedInternId ? `&targetEmployeeId=${window.selectedInternId}` : '';
    const data = await fetchAPI(`timesheet/monthly?year=${y}&month=${m + 1}${targetParam}`);

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
        const dayData = data?.days?.find(day => {
            const dDate = new Date(day.date);
            return dDate.getFullYear() === y && dDate.getMonth() === m && dDate.getDate() === d;
        });
        const hasWork = dayData && dayData.totalMinutes > 0;
        
        let workSummary = '';
        if (hasWork && dayData.projectMinutes) {
            const projectEntries = Object.entries(dayData.projectMinutes);
            workSummary = `<div class="cell-work-summary">
                ${projectEntries.slice(0, 2).map(([pName, mins]) => `
                    <div class="work-item-small">
                        <span class="work-pname text-truncate">${pName || 'Unknown'}</span>
                        <span class="work-pdur ms-auto">${Math.floor(mins/60)}h ${mins%60}m</span>
                    </div>
                `).join('')}
                ${projectEntries.length > 2 ? `<div class="work-more-label">+ ${projectEntries.length - 2} more</div>` : ''}
            </div>`;
        }

        container.innerHTML += `
            <div class="grid-cell ${isToday ? 'is-today' : ''} ${hasWork ? 'has-data' : ''}" 
                 onclick="${hasWork ? `jumpToDaily('${dateParam}')` : `window.location.href='/Timesheet/Employee/Entry?date=${dateParam}'`}">
                <div class="d-flex justify-content-between">
                    <span class="cell-date-num">${d}</span>
                </div>
                ${workSummary}
            </div>`;
    }
}

async function renderWeeklyGrid() {
    const headerRow = document.getElementById('weekly_header_row');
    const tableBody = document.querySelector('.weekly-master-table tbody');
    if (!headerRow || !tableBody) return;

    const dateStr = currentDate.toISOString().split('T')[0];
    const targetParam = window.selectedInternId ? `&targetEmployeeId=${window.selectedInternId}` : '';
    const data = await fetchAPI(`timesheet/weekly?weekStartDate=${dateStr}${targetParam}`);

    const middleHeaders = Array.from(headerRow.children).slice(1, -1);
    middleHeaders.forEach((col, idx) => {
        const d = new Date(currentDate);
        d.setDate(currentDate.getDate() + idx);
        const dayLabel = englishDaysShort[d.getDay()];
        const dateString = `${englishMonths[d.getMonth()].substr(0, 3)} ${d.getDate()}`;
        const isToday = (d.toDateString() === new Date().toDateString());
        const isWeekend = (d.getDay() === 0 || d.getDay() === 6);
        
        let headerCls = "";
        if (isToday) {
            headerCls = isWeekend ? "today-col-highlight-red" : "today-col-highlight-gray";
        } else if (isWeekend) {
            headerCls = "weekend-th";
        }
        
        const dateStrParam = d.toISOString().split('T')[0];
        
        col.className = headerCls;
        col.style.cursor = "pointer";
        col.onclick = () => jumpToDaily(dateStrParam);
        col.innerHTML = `<div class="day-title-wrap"><span class="day-short-label ${isWeekend ? 'text-brand' : ''}">${dayLabel}</span><span class="day-full-date ${isWeekend ? 'text-brand' : ''}">${dateString}</span></div>`;
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
        
        let cls = "";
        if (isToday) {
            cls = isWeekend ? 'today-cell-highlight-red' : 'today-cell-highlight-gray';
        } else if (isWeekend) {
            cls = 'weekend-td text-danger opacity-50';
        }

        return `<td class="p-4 text-center ${cls}" style="cursor:pointer" onclick="jumpToDaily('${dKey}')">
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
    const data = await fetchAPI(`timesheet/daily?date=${dateStr}${targetParam}`);

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
    const data = await fetchAPI('timesheet/dashboard');
    if (data) {
        employeeInfo = data;
        
        const internLabel = document.getElementById('intern_name_label');
        const supervisorLabel = document.getElementById('supervisor_name_label');
        
        if (internLabel) internLabel.innerText = data.employeeName || data.EmployeeName;
        if (supervisorLabel) supervisorLabel.innerText = data.supervisorName || data.SupervisorName;
    }
}

function toggleDatePopup(event) {
    event.stopPropagation();
    closeDatePopup();
    pickerDate = new Date(currentDate);

    const trigger = event.currentTarget || event.target;
    const popover = document.getElementById('popover_' + activeView);
    const backdrop = document.getElementById('datepicker_focal_backdrop');

    if (popover && backdrop) {
        renderPickerLayout(popover);
        
        // ANCHOR BELOW THE TRIGGER (Centered for 290px width)
        const rect = trigger.getBoundingClientRect();
        popover.style.position = 'fixed';
        popover.style.top = (rect.bottom + 10) + 'px';
        popover.style.left = (rect.left + (rect.width / 2) - 145) + 'px';

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

        header.innerText = `${englishMonths[pickerDate.getMonth()]} ${pickerDate.getFullYear()}`;
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
    const hiddenIdInput = wrap.querySelector('.aia-select-id');
    const val = option.getAttribute('data-value');
    const id = option.getAttribute('data-id');
    const lead = option.getAttribute('data-lead');
    const leadId = option.getAttribute('data-lead-id');

    trigger.querySelector('.trigger-text').innerText = val;
    hiddenInput.value = val;
    if (hiddenIdInput) hiddenIdInput.value = id;

    const row = option.closest('tr');
    if (lead && row) {
        const leadInput = row.querySelector('input[placeholder="Project Lead"]');
        if (leadInput) leadInput.value = lead;

        const leadIdInput = row.querySelector('.aia-select-lead-id');
        if (leadIdInput) leadIdInput.value = leadId || "1";
    }

    wrap.querySelectorAll('.aia-select-option').forEach(o => o.classList.remove('selected'));
    option.classList.add('selected');

    wrap.querySelector('.aia-select-popover').classList.remove('active');
    trigger.classList.remove('active');
}

function addNewLogEntry(existingData = null) {
    const tbody = document.getElementById('log_entry_tbody');
    if (!tbody) return;

    const projectHtml = (window.projectsList || []).map(p => 
        `<div class="aia-select-option ${existingData && existingData.projectId == p.id ? 'selected' : ''}" 
              data-id="${p.id}" data-value="${p.name}" data-lead="${p.projectLeadName}" data-lead-id="${p.projectLeadId}" 
              onclick="selectAiaOption(this)">${p.name}</div>`
    ).join('');

    const row = document.createElement('tr');
    row.className = "align-middle";
    row.innerHTML = `
        <td class="p-4 position-relative">
            <div class="d-flex align-items-center justify-content-center gap-2">
                <input type="text" class="entry-input text-center duration-trigger" value="${existingData ? existingData.durationFormatted : '00h 00m'}" style="width:100px; cursor:pointer" readonly onclick="toggleDurationPicker(this, event)">
            </div>
            <div class="duration-picker-popover shadow-lg">
                <div class="duration-columns">
                    <div class="duration-col"><span class="duration-col-label">HH</span><div class="duration-items-scroll">${Array.from({ length: 24 }, (_, i) => `<div class="duration-item" onclick="selectHH(this, ${i})">${i.toString().padStart(2, '0')}</div>`).join('')}</div></div>
                    <div class="duration-col"><span class="duration-col-label">MM</span><div class="duration-items-scroll">${Array.from({ length: 12 }, (_, i) => `<div class="duration-item" onclick="selectMM(this, ${i * 5})">${(i * 5).toString().padStart(2, '0')}</div>`).join('')}</div></div>
                </div>
            </div>
        </td>
        <td class="p-4">
            <div class="aia-custom-select-wrap">
                <div class="aia-select-trigger" onclick="toggleAiaSelect(this, event)"><span class="trigger-text">${existingData ? existingData.projectName : 'Select Project'}</span><i class="bi bi-chevron-down"></i></div>
                <div class="aia-select-popover">
                    ${projectHtml || '<div class="p-4 text-muted small">No projects found</div>'}
                </div>
                <input type="hidden" class="aia-select-value" value="${existingData ? existingData.projectName : ''}">
                <input type="hidden" class="aia-select-id" value="${existingData ? existingData.projectId : ''}">
                <input type="hidden" class="aia-select-lead-id" value="${existingData ? existingData.projectLeadId : ''}">
            </div>
        </td>
        <td class="p-4"><input type="text" class="entry-input" placeholder="App Used" value="${existingData ? existingData.applicationUsed : ''}"></td>
        <td class="p-4"><textarea class="entry-input" style="height:48px; min-height:48px" placeholder="Describe what you do....">${existingData ? existingData.taskDescription : ''}</textarea></td>
        <td class="p-4"><input type="text" class="entry-input bg-light-gray" placeholder="Project Lead" value="${existingData ? existingData.projectLeadName : ''}" readonly></td>
        <td class="p-4">
            <div class="aia-custom-select-wrap">
                <div class="aia-select-trigger" onclick="toggleAiaSelect(this, event)"><span class="trigger-text">${existingData ? (existingData.location === 1 ? 'WFH' : 'AIA Central') : 'Location'}</span><i class="bi bi-chevron-down"></i></div>
                <div class="aia-select-popover">
                    <div class="aia-select-option ${existingData && existingData.location === 0 ? 'selected' : ''}" data-id="0" data-value="AIA Central" onclick="selectAiaOption(this)">AIA Central (Office)</div>
                    <div class="aia-select-option ${existingData && existingData.location === 1 ? 'selected' : ''}" data-id="1" data-value="WFH" onclick="selectAiaOption(this)">WFH</div>
                </div>
                <input type="hidden" class="aia-select-value" value="${existingData ? (existingData.location === 1 ? 'WFH' : 'AIA Central') : ''}">
                <input type="hidden" class="aia-select-id" value="${existingData ? existingData.location : ''}">
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

async function initEntryPage() {
    const label = document.getElementById('entry_date_label');
    if (!label) return;

    const urlParams = new URLSearchParams(window.location.search);
    let dateStr = urlParams.get('date');
    if (!dateStr) dateStr = new Date().toISOString().split('T')[0];

    const targetDate = new Date(dateStr);
    const englishDays = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    const englishMonths = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

    label.innerText = `${englishDays[targetDate.getDay()]}, ${targetDate.getDate()} ${englishMonths[targetDate.getMonth()]} ${targetDate.getFullYear()}`;

    // 1. Fetch Projects & Existing Entries
    app.loading.show('Preparing Entry Table...');
    const [projectData, dailyData] = await Promise.all([
        fetchAPI('timesheet/projects'),
        fetchAPI(`timesheet/daily?date=${dateStr}`)
    ]);
    
    if (projectData) window.projectsList = projectData;

    const tbody = document.getElementById('log_entry_tbody');
    if (tbody) {
        tbody.innerHTML = '';
        if (dailyData && dailyData.entries && dailyData.entries.length > 0) {
            dailyData.entries.forEach(entry => addNewLogEntry(entry));
        } else {
            // Default row if none exists
            addNewLogEntry();
        }
    }

    app.loading.hide();
    syncProfileInfo();
    calculateTotalLogHours();
}

// --- DATA PERSISTENCE & BASIC VALIDATION ---

/**
 * Basic Validation for Daily Timesheet Entry
 * Moves "basic" checks to frontend to prevent "lemot" performance.
 * Keeps "complex" checks for the backend.
 */
function toggleDayType(radio) {
    const tableWrap = document.querySelector('.entry-table-wrap');
    const addBtn = document.getElementById('add_entry_btn');
    const type = radio.value;

    // Only Public Holiday blocks the entire table
    if (type === 'holiday') {
        tableWrap.classList.add('not-working');
        if (addBtn) addBtn.style.display = 'none';
    } else {
        tableWrap.classList.remove('not-working');
        if (addBtn) addBtn.style.display = 'inline-flex';
        
        // If leave, we can add a visual hint but keep it editable
        if (type === 'leave') {
            tableWrap.classList.add('on-leave');
        } else {
            tableWrap.classList.remove('on-leave');
        }
    }
}

async function fetchAPI(endpoint, options = {}) {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 30000); // Increased to 30s timeout

    try {
        const token = localStorage.getItem('aia_jwt_token');
        const response = await fetch(`${API_BASE_URL}/${endpoint}`, {
            ...options,
            signal: controller.signal,
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': token ? `Bearer ${token}` : '',
                ...options.headers
            }
        });
        clearTimeout(timeoutId);

        if (response.status === 401) {
            localStorage.removeItem('aia_jwt_token');
            window.location.href = '/Account/Login';
            return null;
        }

        const result = await response.json();
        
        if (!response.ok || (result && result.isError)) {
            const errorMsg = result.statusMessage || result.message || 'Access Denied: Please contact your administrator.';
            showToast(errorMsg, 'error');
            return null;
        }

        return result.content || result.data || result;
    } catch (error) {
        clearTimeout(timeoutId);
        console.error('API Error:', error);
        if (error.name === 'AbortError') {
            showToast('Request timed out after 30 seconds. Please check your connection or server status.', 'error');
        } else {
            showToast('Network error or server is unreachable.', 'error');
        }
        return null;
    }
}

async function saveDailyTimesheet() {
    app.loading.show('Saving Daily Entry...');
    
    const urlParams = new URLSearchParams(window.location.search);
    let dateStr = urlParams.get('date');
    if (!dateStr) {
        dateStr = new Date().toISOString().split('T')[0];
    }

    const rows = document.querySelectorAll('#log_entry_tbody tr');
    let entries = [];
    let totalMinutes = 0;

    for (const row of rows) {
        const durationStr = row.querySelector('.duration-trigger')?.value || "00h 00m";
        const projectIdRaw = row.querySelector('.aia-select-id')?.value;
        const appUsed = row.querySelector('input[placeholder="App Used"]')?.value || "";
        const taskDescription = row.querySelector('textarea')?.value || "";
        const projectLeadIdRaw = row.querySelector('.aia-select-lead-id')?.value || "1";
        
        // Find the second custom select (Location)
        const selects = row.querySelectorAll('.aia-custom-select-wrap');
        const locationIdRaw = selects.length > 1 ? selects[1].querySelector('.aia-select-id')?.value || "0" : "0";

        // Basic Validation
        if (durationStr === "00h 00m") {
            app.loading.hide();
            showToast("Work duration cannot be 00h 00m.", 'error');
            return;
        }
        if (!projectIdRaw || projectIdRaw === "0") {
            app.loading.hide();
            showToast("Please select a project for all entries.", 'error');
            return;
        }
        if (!taskDescription.trim()) {
            app.loading.hide();
            showToast("Task description cannot be empty.", 'error');
            return;
        }

        const match = durationStr.match(/(\d+)h\s+(\d+)m/);
        const mins = match ? (parseInt(match[1]) * 60 + parseInt(match[2])) : 0;
        totalMinutes += mins;

        entries.push({
            durationMinutes: mins,
            projectId: parseInt(projectIdRaw),
            applicationUsed: appUsed,
            taskDescription: taskDescription,
            projectLeadId: parseInt(projectLeadIdRaw),
            location: parseInt(locationIdRaw)
        });
    }

    if (totalMinutes > 1440) {
        app.loading.hide();
        showToast("Total daily duration cannot exceed 24 hours.", 'error');
        return;
    }

    if (entries.length === 0) {
        app.loading.hide();
        showToast("At least one entry is required to save.", 'error');
        return;
    }

    const result = await fetchAPI('timesheet/entry', {
        method: 'POST',
        body: JSON.stringify({
            date: dateStr,
            entries: entries
        })
    });

    app.loading.hide();
    if (result) {
        Swal.fire({
            title: 'Success!',
            text: 'Daily timesheet has been successfully saved.',
            icon: 'success',
            confirmButtonColor: '#D31145'
        }).then(() => {
            window.location.href = '/Timesheet/Employee/Management?view=daily';
        });
    }
}

/**
 * Basic Validation for Monthly Submission
 */
async function submitTimesheetApproval() {
    const isCertified = document.getElementById('certify_check')?.checked;
    if (!isCertified) {
        showToast("Please certify the accuracy of your timesheet before submitting.", 'error');
        return;
    }

    const y = currentDate.getFullYear();
    const m = currentDate.getMonth() + 1;

    // 1. Basic Validation: Future Month
    const today = new Date();
    today.setHours(today.getHours() + 7);
    if (y > today.getFullYear() || (y === today.getFullYear() && m > (today.getMonth() + 1))) {
        showToast(`You cannot submit timesheet for a future period.`, 'error');
        return;
    }

    // 2. API Call (Complex validations like missing days are handled in Backend)
    app.loading.show('Submitting Timesheet...');
    const result = await fetchAPI('timesheet/submit', {
        method: 'POST',
        body: JSON.stringify({
            year: y,
            month: m
        })
    });
    app.loading.hide();

    if (result) {
        // Close modal first
        const modalEl = document.getElementById('modal_review_submit');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();

        Swal.fire({
            title: 'Submitted!',
            text: 'Your timesheet has been sent to your supervisor for verification.',
            icon: 'success',
            confirmButtonColor: '#D31145'
        }).then(() => {
            renderCurrentState(); // Refresh grid state
        });
    }
}

// --- DASHBOARD DATA RENDERING ---

async function initDashboard() {
    try {
        const data = await fetchAPI('timesheet/dashboard');
        if (!data) return;

        const h = new Date().getHours();
        let g = "Good Morning";
        if (h >= 12 && h < 17) g = "Good Afternoon";
        else if (h >= 17) g = "Good Evening";
        
        const actualData = data.content || data.Content || data.data || data;
        window.dashboardData = actualData; // Store for global access

        // 1. Greeting
        const welcomeEl = document.getElementById('welcome_text');
        if (welcomeEl) {
            let eName = actualData.employeeName || actualData.EmployeeName || "User";
            // Clean up any potential role suffixes just in case
            eName = eName.replace(/\s(Intern|Admin|Supervisor)$/i, "");
            welcomeEl.innerText = `${g}, ${eName}!`;
        }

        // 2. Status Card
        const statusCard = document.getElementById('active_status_label');
        if (statusCard) {
            const cms = actualData.currentMonthSubmission || actualData.CurrentMonthSubmission;
            if (cms) {
                document.getElementById('active_period').innerText = `${cms.monthName} ${cms.year}`;
                
                const activeDeadline = document.getElementById('active_deadline');
                if (activeDeadline) activeDeadline.innerText = `${cms.daysRemaining} Days Remaining`;

                // Map API status string to design badge
                const s = (cms.status || 'Not Submitted').toLowerCase();
                let cls = 'badge-pill-draft';
                let icon = 'bi-file-earmark-text';
                let label = cms.status || 'Not Submitted';

                if (s.includes('waiting') || s.includes('approval')) {
                    cls = 'badge-pill-needs-approval';
                    icon = 'bi-clock';
                    label = 'Needs Approval';
                } else if (s.includes('approved')) {
                    cls = 'badge-pill-approved';
                    icon = 'bi-check-circle-fill';
                    label = 'Approved';
                } else if (s.includes('revision') || s.includes('rejected')) {
                    cls = 'badge-pill-rejected';
                    icon = 'bi-x-circle-fill';
                    label = 'Rejected';
                }

                statusCard.className = `badge-pill-status ${cls}`;
                statusCard.innerHTML = `<i class="bi ${icon}"></i> ${label}`;
            }
        }

        // 3. Project Allocations
        renderProjectAllocations(actualData.projectAllocations || actualData.ProjectAllocations);

        // 4. To Do List Rendering
        renderToDoList(actualData.todoTasks || actualData.TodoTasks);

    } catch (err) {
        console.error("Dashboard Init Error:", err);
    }
}

function renderProjectAllocations(allocations) {
    const container = document.getElementById('allocation_container');
    const totalEl = document.getElementById('total_hours_display');
    if (!container || !allocations) return;

    if (allocations.length === 0) return;

    let totalMins = 0;
    const colors = ['#D31145', '#181C32', '#009EF7', '#50CD89', '#F1416C', '#7239EA'];
    let htmlBuffer = '';

    allocations.forEach((p, idx) => {
        const pMins = p.totalMinutes || p.TotalMinutes || 0;
        totalMins += pMins;
        const hrs = (pMins / 60).toFixed(1); // One decimal for clarity
        const perc = p.allocationPercentage || p.AllocationPercentage || 10;
        const c = colors[idx % colors.length];
        const pName = p.projectName || p.ProjectName || "Project";

        htmlBuffer += `
           <div class="proj-card">
               <div class="proj-label" title="Project Name">${pName}</div>
               <div class="proj-hours">
                   <span class="num">${hrs}</span>
                   <span class="unit">h</span>
               </div>
               <div class="mt-auto">
                   <div class="fs-9 text-muted fw-bold mb-1">Total Logged Hours</div>
                   <div class="proj-bar-container">
                       <div class="proj-bar-inner" style="width: ${perc}%; background: ${c};"></div>
                   </div>
               </div>
           </div>
        `;
    });
    container.innerHTML = htmlBuffer;
    if (totalEl) totalEl.innerText = (totalMins / 60).toFixed(0) + ' h';
}

function renderToDoList(tasks) {
    const todoContent = document.querySelector('.todo-content');
    const progressText = document.querySelector('.todo-footer .text-brand-red');
    const progressBar = document.getElementById('todo_progress_bar');
    
    if (!todoContent) return;

    if (!tasks || tasks.length === 0) {
        todoContent.innerHTML = `
            <div class="my-auto">
                <img src="/assets/media/illustrations/sigma-1/17.png" class="todo-illustration" />
                <span class="fs-6 mb-1 d-block">No tasks yet. Add one!</span>
                <p class="text-muted fs-8 fw-bold">Keep track of your daily priorities.</p>
            </div>
        `;
        if (progressText) progressText.innerText = "0 of 0 tasks completed";
        if (progressBar) progressBar.style.width = "0%";
        return;
    }

    const completedCount = tasks.filter(t => t.isCompleted).length;
    const totalCount = tasks.length;
    const progress = Math.round((completedCount / totalCount) * 100);

    if (progressText) progressText.innerText = `${completedCount} of ${totalCount} tasks completed`;
    if (progressBar) progressBar.style.width = `${progress}%`;

    todoContent.innerHTML = `
        <div class="w-100 mt-2">
            ${tasks.map(t => `
                <div class="d-flex align-items-center mb-4 p-4 rounded-4 ${t.isCompleted ? 'bg-light-success bg-opacity-10' : 'bg-white border shadow-sm'}" style="transition: all 0.2s">
                    <div class="form-check form-check-custom form-check-solid me-4">
                        <input class="form-check-input h-20px w-20px" type="checkbox" ${t.isCompleted ? 'checked' : ''} onchange="toggleTask(${t.id})" />
                    </div>
                    <div class="flex-grow-1 text-start">
                        <span class="fw-boldest fs-6 ${t.isCompleted ? 'text-decoration-line-through text-gray-500' : 'text-gray-800'}">${t.taskName}</span>
                        <div class="mt-1">
                            ${t.dueDate ? `<span class="text-muted fs-9 fw-bold"><i class="bi bi-calendar-event fs-9 me-1"></i>${t.dueDate}</span>` : ''}
                        </div>
                    </div>
                    <div class="d-flex align-items-center gap-3">
                        <span class="badge badge-light-${t.priority === 'High' ? 'danger' : (t.priority === 'Medium' ? 'warning' : 'success')} fs-9 px-2 py-1">${t.priority}</span>
                        <button class="btn btn-icon btn-active-light-danger btn-sm w-30px h-30px" onclick="deleteTask(${t.id})">
                            <i class="bi bi-trash fs-6"></i>
                        </button>
                    </div>
                </div>
            `).join('')}
        </div>
    `;
}

async function toggleTask(taskId) {
    const result = await fetchAPI(`todos/${taskId}/toggle`, { method: 'PATCH' });
    if (result) initDashboard();
}

async function deleteTask(taskId) {
    const result = await fetchAPI(`todos/${taskId}`, { method: 'DELETE' });
    if (result) initDashboard();
}

/* --- NEW TASK MODAL LOGIC --- */
let taskPickerDate = new Date();
let selectedTaskDate = new Date();
let selectedPriority = 2; // Default HIGH

function initTaskModal() {
    renderTaskDaySelector();
}

function jumpToToday() {
    taskPickerDate = new Date();
    selectedTaskDate = new Date();
    renderTaskDaySelector();
}

function jumpToNextWeek() {
    const nextWeek = new Date(taskPickerDate);
    nextWeek.setDate(taskPickerDate.getDate() + 7);
    taskPickerDate = nextWeek;
    renderTaskDaySelector();
}

function renderTaskDaySelector() {
    const selector = document.getElementById('task_day_selector');
    const monthLabel = document.getElementById('task_month_label');
    if (!selector || !monthLabel) return;

    const mName = englishMonths[taskPickerDate.getMonth()];
    monthLabel.innerText = `${mName} ${taskPickerDate.getFullYear()}`;
    
    // HYBRID PICKER: Clicking Month Label triggers full monthly picker
    monthLabel.style.cursor = "pointer"; // Ensure it shows pointer
    monthLabel.onclick = (e) => {
        pickerDate = new Date(taskPickerDate);
        activeView = 'daily'; // Set to daily so picking a day updates the current view
        executePickerSelection = (day) => {
            selectedTaskDate = new Date(pickerDate.getFullYear(), pickerDate.getMonth(), day);
            taskPickerDate = new Date(selectedTaskDate);
            renderTaskDaySelector();
            closeDatePopup();
        };
        toggleDatePopup(e);
    };
    
    selector.innerHTML = "";
    
    // Find Monday of the current taskPickerDate week
    const start = new Date(taskPickerDate);
    const day = start.getDay();
    const diff = start.getDate() - day + (day === 0 ? -6 : 1);
    start.setDate(diff);

    for (let i = 0; i < 7; i++) {
        const d = new Date(start);
        d.setDate(start.getDate() + i);
        
        const isSelected = d.toDateString() === selectedTaskDate.toDateString();
        const dateKey = d.toISOString().split('T')[0];
        
        // Use real tasks to show status indicators if available
        // dashboardData is likely available globally from initDashboard
        const hasTask = window.dashboardData?.todoTasks?.some(t => t.dueDate === dateKey) || false;
        
        const dayName = englishDaysShort[d.getDay()].toUpperCase();
        
        const item = document.createElement('div');
        item.className = `day-item ${isSelected ? 'selected' : ''} ${hasTask ? 'has-task' : ''}`;
        item.onclick = () => {
            selectedTaskDate = new Date(d);
            renderTaskDaySelector();
        };
        item.innerHTML = `
            <span class="day-name">${dayName}</span>
            <span class="day-num">${d.getDate()}</span>
            <div class="day-dot"></div>
        `;
        selector.appendChild(item);
    }
}

function moveTaskPickerDate(offset) {
    taskPickerDate.setDate(taskPickerDate.getDate() + (offset * 7));
    renderTaskDaySelector();
}

function selectTaskPriority(el) {
    document.querySelectorAll('.priority-item').forEach(p => p.classList.remove('selected'));
    el.classList.add('selected');
    selectedPriority = parseInt(el.getAttribute('data-priority'));
}

async function confirmNewTask() {
    const taskNameInput = document.getElementById('task_name_input');
    const taskName = taskNameInput.value.trim();
    if (!taskName) {
        showToast("Please enter a task objective.", 'error');
        return;
    }

    const payload = {
        taskName: taskName,
        dueDate: selectedTaskDate.toISOString().split('T')[0],
        priority: selectedPriority
    };

    const confirmBtn = document.querySelector('.btn-confirm');
    if (confirmBtn) confirmBtn.disabled = true;

    const result = await fetchAPI('todos', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
    });

    if (result) {
        const modalEl = document.getElementById('modal_new_task');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
        
        taskNameInput.value = ""; // Reset input
        initDashboard(); // Refresh tasks without page reload
    }
    if (confirmBtn) confirmBtn.disabled = false;
}

document.addEventListener('DOMContentLoaded', () => {
    // Dispatch inits
    setTimeout(() => {
        if (document.getElementById('monthly_grid_container')) renderCurrentState();
        if (document.getElementById('intern_name_label')) syncProfileInfo();
        if (document.getElementById('entry_date_label')) initEntryPage();
        if (document.getElementById('welcome_text')) initDashboard();
    }, 50);
});

function showReviewModal() {
    const periodLabel = document.getElementById('review_period_label');
    if (periodLabel) {
        const m = englishMonths[currentDate.getMonth()];
        periodLabel.innerText = `${m} ${currentDate.getFullYear()}`;
    }
    const modalEl = document.getElementById('modal_review_submit');
    if (modalEl) {
        let modal = bootstrap.Modal.getInstance(modalEl);
        if (!modal) modal = new bootstrap.Modal(modalEl);
        modal.show();
    }
}

async function submitTimesheetApproval() {
    const check = document.getElementById('certify_check');
    if (!check) return;
    
    if (!check.checked) {
        Swal.fire({
            title: 'Certification Required',
            text: 'Please check the box to certify that your logs are accurate.',
            icon: 'warning',
            confirmButtonColor: '#D31145'
        });
        return;
    }

    const payload = {
        year: currentDate.getFullYear(),
        month: currentDate.getMonth() + 1
    };

    const res = await fetchAPI('timesheet/submit', {
        method: 'POST',
        body: JSON.stringify(payload)
    });

    if (res) {
        const modalEl = document.getElementById('modal_review_submit');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();

        Swal.fire({
            title: 'Successfully Submitted!',
            text: 'Your work logs have been sent to your supervisor for review.',
            icon: 'success',
            confirmButtonColor: '#D31145'
        }).then(() => {
            window.location.reload();
        });
    }
}
