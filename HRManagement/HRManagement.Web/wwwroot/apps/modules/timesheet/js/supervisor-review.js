/**
 * Supervisor Review Page JS
 */
'use strict';

const REVIEW_MONTHS = ["January","February","March","April","May","June","July","August","September","October","November","December"];
let currentDayComments = {}; // map of date -> comment

async function initReviewPage() {
    const urlParams = new URLSearchParams(window.location.search);
    const submissionId = urlParams.get('id');
    const employeeId   = urlParams.get('employeeId');
    const month        = urlParams.get('month');
    const year         = urlParams.get('year');

    let endpoint = '';
    if (submissionId && parseInt(submissionId) > 0) {
        endpoint = `timesheet/supervisor/review/${submissionId}`;
    } else if (employeeId && month && year) {
        endpoint = `timesheet/supervisor/review/anytime?employeeId=${employeeId}&month=${month}&year=${year}`;
    } else {
        window.location.href = '/Timesheet/Supervisor/Dashboard';
        return;
    }

    try {
        const data = await reviewFetchAPI(endpoint);
        if (!data) return;

        window.currentReviewData = data;
        
        // Load existing day comments from DB if present
        if (data.dayComments) {
            data.dayComments.forEach(c => {
                currentDayComments[c.date] = c.comment;
            });
        }
        // Load remarks directly embedded in days
        if (data.days) {
            data.days.forEach(d => {
                if (d.remark && d.remark.trim() !== '') {
                    currentDayComments[d.date] = d.remark;
                }
            });
        }

        renderReviewHeader(data);
        renderReviewTable(data.days || []);
    } catch (err) {
        console.error("Review Page Init Error:", err);
    }
}

// ── Header ───────────────────────────────────────────────────────────────────
function renderReviewHeader(data) {
    const nameEl   = document.getElementById('employee_name');
    const periodEl = document.getElementById('review_period_label');

    if (nameEl)   nameEl.innerText   = data.employeeName || '—';

    const picker = $('#review_period_picker');
    if (picker.length) {
        // Set initial dates based on the loaded month/year
        const startDate = moment(new Date(data.year, (data.month || 1) - 1, 1));
        const endDate = moment(new Date(data.year, data.month || 1, 0));
        
        picker.daterangepicker({
            startDate: startDate,
            endDate: endDate,
            locale: {
                format: 'MMMM D, YYYY'
            },
            opens: 'center'
        }, function(start, end) {
            // Update the display text to match the requested format
            const formatStr = start.format('D MMM') + ' - ' + end.format('D MMM YYYY');
            $('#review_period_label').text(formatStr);
            
            // Filter the table
            window.reviewFilterStart = start.startOf('day');
            window.reviewFilterEnd = end.endOf('day');
            renderReviewTable(window.currentReviewData.days || []);
        });

        // Initialize value
        $('#review_period_label').text(startDate.format('D MMM') + ' - ' + endDate.format('D MMM YYYY'));
        window.reviewFilterStart = startDate;
        window.reviewFilterEnd = endDate;

        // Custom arrows
        $('#prev_period_btn').click(() => {
            const currentStart = picker.data('daterangepicker').startDate;
            const currentEnd = picker.data('daterangepicker').endDate;
            const diff = currentEnd.diff(currentStart, 'days');
            picker.data('daterangepicker').setStartDate(currentStart.subtract(diff + 1, 'days'));
            picker.data('daterangepicker').setEndDate(currentEnd.subtract(diff + 1, 'days'));
            picker.trigger('apply.daterangepicker', picker.data('daterangepicker'));
        });
        $('#next_period_btn').click(() => {
            const currentStart = picker.data('daterangepicker').startDate;
            const currentEnd = picker.data('daterangepicker').endDate;
            const diff = currentEnd.diff(currentStart, 'days');
            picker.data('daterangepicker').setStartDate(currentStart.add(diff + 1, 'days'));
            picker.data('daterangepicker').setEndDate(currentEnd.add(diff + 1, 'days'));
            picker.trigger('apply.daterangepicker', picker.data('daterangepicker'));
        });
    }
}

// ── Table ─────────────────────────────────────────────────────────────────────
function renderReviewTable(days) {
    const tbody = document.getElementById('review_tbody');
    if (!tbody) return;
    tbody.innerHTML = '';

    if (!days || days.length === 0) {
        tbody.innerHTML = '<tr><td colspan="10" class="text-center py-20 text-muted fw-bold">No logs found for this period.</td></tr>';
        return;
    }

    const overallStatus = window.currentReviewData.status || 'Waiting for Approval';
    const isTimesheetApproved = overallStatus.toLowerCase() === 'approved';

    days.forEach((d, idx) => {
        const dateObj  = new Date(d.date);
        const dayMoment = moment(dateObj);

        // Filter by Date Range
        if (window.reviewFilterStart && window.reviewFilterEnd) {
            if (dayMoment.isBefore(window.reviewFilterStart) || dayMoment.isAfter(window.reviewFilterEnd)) {
                return; // skip rendering this row
            }
        }

        const dayName  = dateObj.toLocaleDateString('en-US', { weekday: 'long' });
        const dateNum  = dateObj.getDate();
        const monShort = dateObj.toLocaleDateString('en-US', { month: 'short' });
        const isWeekend = dateObj.getDay() === 0 || dateObj.getDay() === 6;

        let remarkValue = currentDayComments[d.date] || '';
        
        // Calculate Row Status
        let rowStatus = 'Needs Approval';
        if (isTimesheetApproved || remarkValue === '[APPROVED]') {
            rowStatus = 'Approved';
        } else if (remarkValue && remarkValue !== '[APPROVED]') {
            rowStatus = 'Need Revision';
        } else if (overallStatus === 'Need Revision') {
            // Overall is revision, but this day has no remark? It means it's implicitly approved/okay.
            rowStatus = 'Approved'; 
        }

        // Checkbox logic
        let cbChecked = rowStatus === 'Approved';
        let cbDisabled = isTimesheetApproved || (remarkValue && remarkValue !== '[APPROVED]');

        // Activities
        let projHtml = '-';
        let appHtml = '-';
        let taskHtml = '-';
        let locHtml = '-';

        if (d.entries && d.entries.length > 0) {
            projHtml = d.entries.map(e => `<div>${escapeHtml(e.projectName || '-')}</div>`).join('');
            appHtml = d.entries.map(e => `<div>${escapeHtml(e.applicationUsed || '-')}</div>`).join('');
            taskHtml = d.entries.map(e => `<div>${escapeHtml(e.taskDescription || '-')}</div>`).join('');
            let uniqueLocs = [...new Set(d.entries.map(e => escapeHtml(e.location || '-')))];
            locHtml = `<div style="font-weight: 600; color: #4B5563;">${uniqueLocs.join(', ')}</div>`;
        } else if (!isWeekend) {
            projHtml = 'OFF'; locHtml = 'OFF'; taskHtml = 'OFF';
        } else {
            projHtml = '-'; locHtml = '-'; taskHtml = '-';
        }

        const totalHrs = d.totalMinutes ? (d.totalMinutes / 60).toFixed(1) : '0.0';

        // Status badge UI
        let statusTag = '';
        if (rowStatus === 'Approved') statusTag = '<span class="badge-pill-status badge-pill-approved">Approved</span>';
        else if (rowStatus === 'Need Revision') statusTag = '<span class="badge-pill-status badge-pill-revision">Need Revision</span>';
        else statusTag = '<span class="badge-pill-status badge-pill-needs-approval">Needs Approval</span>';

        let rowClass = isWeekend ? 'row-weekend' : '';
        if (rowStatus === 'Need Revision') rowClass += ' row-need-revision';

        // Remark Icon styling
        let hasRealRemark = (remarkValue && remarkValue !== '[APPROVED]');
        let iconHtml = '';
        if (!isWeekend && totalHrs !== '0.0') {
            iconHtml = `<button class="remark-icon-btn ${hasRealRemark ? 'has-remark' : 'no-remark'}" onclick="promptRemark('${d.date}')" ${cbChecked ? 'disabled style="opacity:0.3; cursor:not-allowed;"' : ''}>
                <i class="bi ${hasRealRemark ? 'bi-chat-left-text-fill' : 'bi-chat-left-text'}" style="font-size:1.1rem;"></i>
            </button>`;
        }

        tbody.innerHTML += `
            <tr class="${rowClass}" data-date="${d.date}">
                <td class="text-center">
                    ${isWeekend ? '' : `
                    <div class="form-check form-check-custom form-check-solid justify-content-center">
                        <input type="checkbox" class="form-check-input h-20px w-20px day-cb"
                            data-date="${d.date}"
                            ${cbChecked  ? 'checked'  : ''}
                            ${cbDisabled ? 'disabled' : ''}
                            onchange="onDayCbChange(this)">
                    </div>`}
                </td>
                <td>${statusTag}</td>
                <td>${dayName}</td>
                <td>${dateNum}-${monShort}</td>
                <td class="text-dark fw-boldest">${totalHrs} h</td>
                <td class="multi-line-cell">${projHtml}</td>
                <td class="multi-line-cell">${appHtml}</td>
                <td class="multi-line-cell">${taskHtml}</td>
                <td class="multi-line-cell">${locHtml}</td>
                <td class="text-center">
                    ${iconHtml}
                </td>
            </tr>
        `;
    });
    
    syncSelectAll();
}

async function promptRemark(date) {
    const existing = currentDayComments[date] && currentDayComments[date] !== '[APPROVED]' ? currentDayComments[date] : '';
    
    // Format date string to match "Monday, 2nd February 2026"
    const dateObj = new Date(date);
    const dayName = dateObj.toLocaleDateString('en-US', { weekday: 'long' });
    const day = dateObj.getDate();
    const nth = function(d) {
        if (d > 3 && d < 21) return 'th';
        switch (d % 10) {
            case 1:  return "st";
            case 2:  return "nd";
            case 3:  return "rd";
            default: return "th";
        }
    };
    const monthName = dateObj.toLocaleDateString('en-US', { month: 'long' });
    const year = dateObj.getFullYear();
    const formattedDate = `${dayName}, ${day}${nth(day)} ${monthName} ${year}`;

    const { value: text, isConfirmed } = await Swal.fire({
        title: '<div style="text-align: left; font-size: 1.4rem; font-weight: 800; color: #111;">Remark</div>',
        html: `<div style="text-align: left; font-size: 1.05rem; font-weight: 700; color: #9EA5B2; letter-spacing: 1.5px; margin-bottom: 12px;">${formattedDate}</div>`,
        input: 'textarea',
        inputValue: existing,
        inputPlaceholder: 'Add Remark..',
        showCloseButton: true,
        showCancelButton: false,
        showDenyButton: false,
        confirmButtonText: 'Save',
        customClass: {
            container: 'custom-swal-container',
            popup: 'custom-swal-popup-v2',
            input: 'custom-swal-input-v2',
            confirmButton: 'custom-swal-btn'
        },
        buttonsStyling: false,
    });

    if (isConfirmed) {
        if (!text || text.trim() === '') {
            // Remove remark
            delete currentDayComments[date];
            const cb = document.querySelector(`.day-cb[data-date="${date}"]`);
            if (cb) cb.disabled = false;
        } else {
            currentDayComments[date] = text.trim();
            // Uncheck the checkbox since it has a remark
            const cb = document.querySelector(`.day-cb[data-date="${date}"]`);
            if (cb) { cb.checked = false; cb.disabled = true; }
        }
        renderReviewTable(window.currentReviewData.days);
    }
}

// ── Checkbox interaction ──────────────────────────────────────────────────────
function onDayCbChange(cb) {
    const date = cb.dataset.date;
    if (cb.checked) {
        // If checked, remove any revision remarks and set to approved
        currentDayComments[date] = '[APPROVED]';
    } else {
        delete currentDayComments[date];
    }
    syncSelectAll();
    // Render to update icon disabled state
    renderReviewTable(window.currentReviewData.days);
}

function toggleSelectAll(masterCb) {
    document.querySelectorAll('.day-cb:not(:disabled)').forEach(cb => {
        cb.checked = masterCb.checked;
        const date = cb.dataset.date;
        if (cb.checked) {
            currentDayComments[date] = '[APPROVED]';
        } else {
            delete currentDayComments[date];
        }
    });
    renderReviewTable(window.currentReviewData.days);
}

function syncSelectAll() {
    const all = document.querySelectorAll('.day-cb:not(:disabled)');
    const allChecked = all.length > 0 && [...all].every(cb => cb.checked);
    const masterCb  = document.getElementById('select_all_cb');
    if (masterCb) masterCb.checked = allChecked;
}

// ── Submission ────────────────────────────────────────────────────────────────
async function submitReview() {
    const data = window.currentReviewData;
    if (!data) return;

    // Validate
    const days = data.days || [];
    let hasUnreviewed = false;
    const reviewedDays = [];

    days.forEach(d => {
        const dateObj  = new Date(d.date);
        const isWeekend = dateObj.getDay() === 0 || dateObj.getDay() === 6;
        const totalHrs = d.totalMinutes ? (d.totalMinutes / 60).toFixed(1) : '0.0';
        
        if (isWeekend || totalHrs === '0.0') return; // Skip weekends/empty

        const remark = currentDayComments[d.date];
        const cb = document.querySelector(`.day-cb[data-date="${d.date}"]`);
        
        if (remark && remark !== '[APPROVED]') {
            // Revision
            reviewedDays.push({ date: d.date, comment: remark });
        } else if (cb && cb.checked) {
            // Approved
            reviewedDays.push({ date: d.date, comment: '[APPROVED]' });
        } else {
            // Missing decision
            hasUnreviewed = true;
        }
    });

    if (hasUnreviewed) {
        Swal.fire('Incomplete Review', 'You must review all working days. Please either check the box to approve, or add a remark for revision.', 'warning');
        return;
    }

    if (reviewedDays.length === 0) {
        Swal.fire('Error', 'No data to submit.', 'error');
        return;
    }

    const { isConfirmed } = await Swal.fire({
        title: 'Submit Review?',
        text: `You are about to submit the evaluation for ${data.employeeName}.`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Yes, Submit',
        confirmButtonColor: 'var(--brand-red)'
    });

    if (!isConfirmed) return;

    const payload = {
        submissionId: data.submissionId || 0,
        employeeId:   data.employeeId,
        month:        data.month,
        year:         data.year,
        reviewedDays: reviewedDays
    };

    const res = await reviewFetchAPI('timesheet/supervisor/review/submit', 'POST', payload);
    if (res) {
        await Swal.fire('Success', 'Review submitted successfully.', 'success');
        window.location.href = '/Timesheet/Supervisor/Report';
    }
}

// ── API Helper ────────────────────────────────────────────────────────────────
async function reviewFetchAPI(endpoint, method = 'GET', body = null) {
    const token = localStorage.getItem('aia_jwt_token');
    const opts = {
        method,
        cache: 'no-store',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    };
    if (body) opts.body = JSON.stringify(body);

    const r = await fetch(`https://localhost:7089/api/${endpoint}`, opts);
    if (!r.ok) {
        const err = await r.json().catch(() => ({}));
        if (typeof Swal !== 'undefined') Swal.fire('Error', err.message || err.Message || 'Operation failed', 'error');
        return null;
    }
    const json = await r.json();
    if (json && (json.success === false || json.Success === false)) {
        if (typeof Swal !== 'undefined') Swal.fire('Error', json.message || json.Message || 'Operation failed', 'error');
        return null;
    }
    return json.content || json.data || json;
}

function escapeHtml(unsafe) {
    if (!unsafe) return '';
    return unsafe
         .replace(/&/g, "&amp;")
         .replace(/</g, "&lt;")
         .replace(/>/g, "&gt;")
         .replace(/"/g, "&quot;")
         .replace(/'/g, "&#039;");
}

document.addEventListener('DOMContentLoaded', () => {
    initReviewPage();
});
