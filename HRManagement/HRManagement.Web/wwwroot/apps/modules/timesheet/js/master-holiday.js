/**
 * Master Holiday Management JS
 * Handles listing, manual editing, and Excel import.
 */

// ── SHARED UTILITIES ─────────────────────────────────────────

function getAuthHeaders() {
    const token = localStorage.getItem('aia_jwt_token');
    const headers = { 'Content-Type': 'application/json' };
    if (token) headers['Authorization'] = `Bearer ${token}`;
    return headers;
}

function formatDate(dateStr) {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function showMasterToast(message, type = 'success') {
    let toast = document.getElementById('master_toast');
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'master_toast';
        toast.className = 'master-toast';
        document.body.appendChild(toast);
    }
    toast.className = `master-toast ${type}`;
    toast.innerHTML = `<i class="bi bi-${type === 'success' ? 'check-circle-fill' : 'exclamation-triangle-fill'}"></i> ${message}`;
    toast.classList.add('show');
    clearTimeout(toast._timer);
    toast._timer = setTimeout(() => toast.classList.remove('show'), 3000);
}


// ── LIST PAGE ────────────────────────────────────────────────

async function initHolidayList() {
    const tbody = document.getElementById('holiday_list_tbody');
    const loading = document.getElementById('holiday_loading');
    const emptyState = document.getElementById('holiday_empty_state');
    const tableWrap = document.getElementById('holiday_table_wrap');

    try {
        const year = new Date().getFullYear();
        const res = await fetch(`https://localhost:7089/api/timesheet/holidays?year=${year}&month=0`, { headers: getAuthHeaders() });
        const data = await res.json();
        const holidays = data?.content || data?.Content || data?.data || data || [];

        if (loading) loading.style.display = 'none';
        
        // Always show table wrap so headers are visible
        if (tableWrap) tableWrap.style.display = 'block';

        if (!Array.isArray(holidays) || holidays.length === 0) {
            tbody.innerHTML = '';
            if (emptyState) emptyState.style.display = 'block';
            return;
        }
        
        if (emptyState) emptyState.style.display = 'none';
        
        // Initialize confirmation modal for list page
        document.getElementById('confirm_modal_cancel')?.addEventListener('click', () => {
            document.getElementById('confirm_modal_backdrop').style.display = 'none';
        });

        tbody.innerHTML = holidays.map((h, i) => `
            <tr>
                <td class="row-num">${String(i + 1).padStart(2, '0')}</td>
                <td class="project-name-cell">${formatDate(h.holidayDate)}</td>
                <td class="fw-boldest" style="color: #101828;">${escapeHtml(h.holidayName)}</td>
            </tr>
        `).join('');


    } catch (err) {
        console.error('Failed to load holidays:', err);
        if (loading) loading.style.display = 'none';
        if (emptyState) emptyState.style.display = 'block';
    }
}

async function deleteHoliday(id, name) {
    showConfirmModal(`Are you sure you want to delete "${name}"?`, async () => {
        try {
            const res = await fetch(`https://localhost:7089/api/timesheet/holidays/${id}`, {
                method: 'DELETE',
                headers: getAuthHeaders()
            });

            if (res.ok) {
                await initHolidayList();
                showMasterToast('Holiday deleted successfully.', 'success');
            } else {
                showMasterToast('Failed to delete holiday.', 'error');
            }
        } catch (err) {
            console.error('Error deleting holiday:', err);
            showMasterToast('An error occurred.', 'error');
        }

    });
}



// ── EDIT PAGE ────────────────────────────────────────────────

let _originalHolidays = [];
let _holidayRows = [];
let _rowCounter = 0;

async function initHolidayEdit() {
    await loadHolidaysForEdit();

    document.getElementById('btn_add_holiday')?.addEventListener('click', () => addHolidayRow());
    document.getElementById('btn_save_holiday')?.addEventListener('click', () => {
        if (!validateHolidayRows()) return;
        showConfirmModal('Are you sure you want to save these holidays?', submitHolidayUpdate);
    });
    document.getElementById('btn_discard_holiday')?.addEventListener('click', () => {
        window.location.href = '/Timesheet/Supervisor/Holiday';
    });

}

async function loadHolidaysForEdit() {
    try {
        const year = new Date().getFullYear();
        const res = await fetch(`https://localhost:7089/api/timesheet/holidays?year=${year}&month=0`, { headers: getAuthHeaders() });
        const data = await res.json();
        _originalHolidays = data?.content || data?.Content || data?.data || data || [];
    } catch (err) {
        console.error('API unavailable');
        _originalHolidays = [];
    }

    const container = document.getElementById('holiday_rows_container');
    if (!container) return;
    container.innerHTML = '';
    _holidayRows = [];

    if (_originalHolidays.length > 0) {
        _originalHolidays.forEach(h => addHolidayRow({
            id: h.id,
            holidayDate: h.holidayDate ? h.holidayDate.split('T')[0] : '',
            holidayName: h.holidayName
        }));
    } else {
        addHolidayRow(); // Add one empty row by default
    }
}

function addHolidayRow(data = null) {
    _rowCounter++;
    const id = `row_${_rowCounter}`;
    const container = document.getElementById('holiday_rows_container');
    if (!container) return;

    const tr = document.createElement('tr');
    tr.className = 'edit-project-row'; // Reuse project row styles
    tr.id = id;

    tr.innerHTML = `
        <td class="row-num">${String(container.children.length + 1).padStart(2, '0')}</td>
        <td style="position:relative;">
            <input type="text" class="form-control holiday-date" value="${data ? data.holidayDate : ''}" readonly onclick="toggleRowDatepicker(this, event)" placeholder="YYYY-MM-DD" style="cursor: pointer; background: #fff;" />
        </td>

        <td>
            <input type="text" class="form-control holiday-name" placeholder="E.g. Independence Day" value="${data ? escapeHtml(data.holidayName) : ''}" />
        </td>
        <td class="text-center">
            <button class="btn btn-icon btn-light-danger btn-sm rounded-circle h-35px w-35px" onclick="removeHolidayRow('${id}')" title="Delete">
                <i class="bi bi-trash-fill"></i>
            </button>
        </td>

    `;

    container.appendChild(tr);
    _holidayRows.push({ rowId: id, dbId: data?.id || null });
    updateRowNumbers();

    // Auto-scroll to the new row smoothly (only when adding manually, i.e., without data)
    if (!data) {
        setTimeout(() => {
            tr.scrollIntoView({ behavior: 'smooth', block: 'center' });
            tr.querySelector('.holiday-date')?.focus();
        }, 50);
    }
}

function removeHolidayRow(rowId) {
    showConfirmModal('Are you sure you want to delete this row?', () => {
        const row = document.getElementById(rowId);
        if (row) row.remove();
        _holidayRows = _holidayRows.filter(r => r.rowId !== rowId);
        updateRowNumbers();
    });
}

function updateRowNumbers() {
    const container = document.getElementById('holiday_rows_container');
    const rows = container.querySelectorAll('tr');
    rows.forEach((row, i) => {
        row.querySelector('.row-num').textContent = String(i + 1).padStart(2, '0');
    });
}

function validateHolidayRows() {
    const container = document.getElementById('holiday_rows_container');
    const dates = container.querySelectorAll('.holiday-date');
    const names = container.querySelectorAll('.holiday-name');
    
    let isValid = true;
    dates.forEach((input, i) => {
        if (!input.value) { input.classList.add('is-invalid'); isValid = false; }
        else { input.classList.remove('is-invalid'); }
        
        if (!names[i].value) { names[i].classList.add('is-invalid'); isValid = false; }
        else { names[i].classList.remove('is-invalid'); }
    });

    if (!isValid) showMasterToast('Please fill in all required fields (Date and Name).', 'error');
    return isValid;
}


async function submitHolidayUpdate() {
    const container = document.getElementById('holiday_rows_container');
    const holidayData = _holidayRows.map(r => {
        const row = document.getElementById(r.rowId);
        return {
            id: r.dbId,
            holidayDate: row.querySelector('.holiday-date').value,
            holidayName: row.querySelector('.holiday-name').value,
            description: ""
        };
    });

    const btn = document.getElementById('btn_save_holiday');
    if (btn) { btn.disabled = true; btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Saving...'; }

    try {
        const res = await fetch('https://localhost:7089/api/timesheet/holidays/bulk', {
            method: 'PUT',
            headers: getAuthHeaders(),
            body: JSON.stringify({ holidays: holidayData })
        });

        if (res.ok) {
            window.location.href = '/Timesheet/Supervisor/Holiday';
        } else {
            showMasterToast('Failed to save holidays. Please try again.', 'error');
        }
    } catch (err) {
        console.error('Error saving holidays:', err);
        showMasterToast('An error occurred. Check your connection.', 'error');
    } finally {

        if (btn) { btn.disabled = false; btn.innerHTML = '<i class="bi bi-check2-circle"></i> Save Holidays'; }
    }
}

// ── EXCEL IMPORT LOGIC (REMOVED) ─────────────────────────────
// Excel logic has been replaced entirely by API Import.

// ── API IMPORT LOGIC ─────────────────────────────────────────

async function handleApiImport() {
    const btn = document.getElementById('btn_import_api');
    if (btn) { btn.disabled = true; btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Fetching...'; }

    try {
        const year = new Date().getFullYear();
        // Fetching directly from public Nager.Date API so we don't depend on backend restart
        const res = await fetch(`https://date.nager.at/api/v3/PublicHolidays/${year}/ID`);
        
        if (!res.ok) throw new Error(`API returned an error (Status: ${res.status})`);
        
        const data = await res.json();
        
        // Ensure data is valid
        const holidays = data?.data || data || [];
        if (!Array.isArray(holidays) || holidays.length === 0) {
            showMasterToast('No holidays found from API.', 'error');
            return;
        }

        // Add fetched holidays to the bottom of the table
        holidays.forEach(h => {
            // Check common Indonesian and English property names
            const hDate = h.tanggal || h.date || h.holiday_date || h.holidayDate || h.waktu;
            const hName = h.localName || h.keterangan || h.name || h.holiday_name || h.holidayName || h.deskripsi || h.title;
            if (hDate && hName) {
                addHolidayRow({
                    id: null,
                    holidayDate: hDate,
                    holidayName: hName,
                    description: 'Imported from API'
                });
            }
        });
        
        showMasterToast(`Successfully imported ${holidays.length} holidays from API.`, 'success');
        
    } catch (err) {
        console.error('Failed to import from API:', err);
        showMasterToast('Failed to connect to the API or Invalid API Key.', 'error');
    } finally {
        if (btn) { btn.disabled = false; btn.innerHTML = '<i class="bi bi-cloud-arrow-down me-2"></i> Import from API'; }
    }
}

// ── DATEPICKER LOGIC ─────────────────────────────────────────

let _activeDateInput = null;
let _pickerDate = new Date();
const _months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

function toggleRowDatepicker(input, event) {
    event.stopPropagation();
    _activeDateInput = input;
    
    // Parse current value
    const currentVal = input.value;
    _pickerDate = currentVal ? new Date(currentVal) : new Date();
    if (isNaN(_pickerDate)) _pickerDate = new Date();

    const popover = document.getElementById('popover_daily');
    const backdrop = document.getElementById('datepicker_focal_backdrop');
    
    if (popover && backdrop) {
        renderRowDatePicker();
        
        // Position smartly: if it goes off-screen at the bottom, place it ABOVE the input
        popover.classList.add('active');
        backdrop.classList.add('active');
        
        const rect = input.getBoundingClientRect();
        const popoverHeight = popover.offsetHeight || 320; // Fallback height if offsetHeight fails
        
        if (rect.bottom + popoverHeight + 10 > window.innerHeight) {
            // Place above
            popover.style.top = (rect.top - popoverHeight - 5) + 'px';
        } else {
            // Place below
            popover.style.top = (rect.bottom + 5) + 'px';
        }
        
        // Also ensure it doesn't go off-screen to the right
        const popoverWidth = popover.offsetWidth || 290;
        if (rect.left + popoverWidth > window.innerWidth) {
            popover.style.left = (window.innerWidth - popoverWidth - 10) + 'px';
        } else {
            popover.style.left = rect.left + 'px';
        }
    }
}

function renderRowDatePicker() {
    const popover = document.getElementById('popover_daily');
    const title = document.getElementById('picker_daily_label');
    const grid = popover.querySelector('.datepicker-grid-7');
    if (!popover || !title || !grid) return;

    title.innerText = `${_months[_pickerDate.getMonth()]} ${_pickerDate.getFullYear()}`;
    
    const y = _pickerDate.getFullYear(), m = _pickerDate.getMonth();
    const firstDay = new Date(y, m, 1).getDay();
    const totalDays = new Date(y, m + 1, 0).getDate();

    grid.innerHTML = `
        <div class="datepicker-day-head">MO</div><div class="datepicker-day-head">TU</div>
        <div class="datepicker-day-head">WE</div><div class="datepicker-day-head">TH</div>
        <div class="datepicker-day-head">FR</div><div class="datepicker-day-head">SA</div>
        <div class="datepicker-day-head">SU</div>
    `;

    const offset = (firstDay === 0) ? 6 : firstDay - 1;
    for (let i = 0; i < offset; i++) grid.innerHTML += '<div class="datepicker-day-cell text-muted">-</div>';

    const selectedDateStr = _activeDateInput?.value;

    for (let i = 1; i <= totalDays; i++) {
        const dObj = new Date(y, m, i);
        const dStr = dObj.toISOString().split('T')[0];
        const isSelected = (selectedDateStr === dStr);
        
        grid.innerHTML += `
            <div class="datepicker-day-cell ${isSelected ? 'selected' : ''}" 
                 onclick="event.stopPropagation(); executeRowDateSelection(${i})">${i}</div>
        `;
    }
}

function movePickerTime(offset) {
    _pickerDate.setMonth(_pickerDate.getMonth() + offset);
    renderRowDatePicker();
}

function executeRowDateSelection(day) {
    const d = new Date(_pickerDate.getFullYear(), _pickerDate.getMonth(), day);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const dayStr = String(d.getDate()).padStart(2, '0');
    const dateStr = `${year}-${month}-${dayStr}`;
    
    if (_activeDateInput) {
        _activeDateInput.value = dateStr;
        // Trigger change event if needed
        _activeDateInput.dispatchEvent(new Event('change'));
    }
    closeDatePopup();
}

function closeDatePopup() {
    document.querySelectorAll('.datepicker-popover').forEach(p => p.classList.remove('active'));
    document.getElementById('datepicker_focal_backdrop')?.classList.remove('active');
}

// Attach global close and move functions if they don't exist
if (!window.movePickerTime) window.movePickerTime = movePickerTime;
if (!window.closeDatePopup) window.closeDatePopup = closeDatePopup;
if (!window.selectMonth) window.selectMonth = (m) => { /* Monthly not used here */ };

// ── MODAL UTILITIES ──────────────────────────────────────────

let _confirmCallback = null;

function showConfirmModal(message, callback) {
    const modal = document.getElementById('confirm_modal_backdrop');
    const desc = document.getElementById('confirm_modal_desc');
    if (!modal || !desc) { callback(); return; }

    desc.textContent = message;
    _confirmCallback = callback;
    modal.style.display = 'flex';
}

document.getElementById('confirm_modal_confirm')?.addEventListener('click', () => {
    document.getElementById('confirm_modal_backdrop').style.display = 'none';
    if (_confirmCallback) _confirmCallback();
});

document.getElementById('confirm_modal_cancel')?.addEventListener('click', () => {
    document.getElementById('confirm_modal_backdrop').style.display = 'none';
});

