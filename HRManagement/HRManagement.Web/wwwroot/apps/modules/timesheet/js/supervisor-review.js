/**
 * Supervisor Review Page JS
 * Handles loading data for a specific employee's month and performing Approve/Revision actions.
 * UPDATED: Support for anytime review (without submissionId).
 */

'use strict';

async function initReviewPage() {
    const urlParams = new URLSearchParams(window.location.search);
    const submissionId = urlParams.get('id');
    const employeeId = urlParams.get('employeeId');
    const month = urlParams.get('month');
    const year = urlParams.get('year');

    let endpoint = '';
    if (submissionId && submissionId > 0) {
        endpoint = `timesheet/supervisor/review/${submissionId}`;
    } else if (employeeId && month && year) {
        endpoint = `timesheet/supervisor/review/anytime?employeeId=${employeeId}&month=${month}&year=${year}`;
    } else {
        window.location.href = '/Timesheet/Supervisor/Dashboard';
        return;
    }

    try {
        const data = await fetchAPI(endpoint);
        if (!data) return;

        renderReviewHeader(data);
        renderReviewTable(data.days);
        renderFeedbackSection(data);
        
        // Store globally for actions
        window.currentReviewData = data;
    } catch (err) {
        console.error("Review Page Init Error:", err);
    }
}

function renderReviewHeader(data) {
    const nameEl = document.getElementById('employee_name');
    const periodEl = document.getElementById('review_period_label');
    const statusEl = document.getElementById('review_status_badge');
    const avatarEl = document.getElementById('employee_avatar');
    
    if (nameEl) nameEl.innerText = data.employeeName;
    if (avatarEl) avatarEl.innerText = data.employeeName.charAt(0).toUpperCase();

    if (periodEl) {
        const months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        periodEl.innerText = `${months[data.month - 1]} ${data.year}`;
    }
    if (statusEl) {
        statusEl.innerText = (data.status || 'Needs Approval').toUpperCase();
        const isApproved = data.status === 'Approved';
        statusEl.className = `badge-pill-status ${isApproved ? 'badge-pill-approved' : 'badge-pill-waiting'}`;
    }
}

function renderReviewTable(days) {
    const tbody = document.getElementById('review_table_body');
    if (!tbody) return;
    tbody.innerHTML = '';

    if (!days || days.length === 0) {
        tbody.innerHTML = '<tr><td colspan="4" class="text-center py-20 text-muted fw-bold">No logs found for this period.</td></tr>';
        return;
    }

    days.forEach((d, idx) => {
        const dateObj = new Date(d.date);
        const dayName = dateObj.toLocaleDateString('en-US', { weekday: 'short' }).toUpperCase();
        const dateNum = dateObj.getDate();
        const isWeekend = dateObj.getDay() === 0 || dateObj.getDay() === 6;

        let projectsHTML = '';
        if (d.projectMinutes) {
            projectsHTML = Object.entries(d.projectMinutes).map(([name, mins]) => {
                const hrs = (mins / 60).toFixed(1);
                return `<div class="review-proj-tag"><strong>${name}</strong>: ${hrs}h</div>`;
            }).join('');
        }

        const totalHrs = (d.totalMinutes / 60).toFixed(1);

        tbody.innerHTML += `
            <tr class="${isWeekend ? 'row-weekend' : ''}">
                <td class="text-center fw-boldest text-gray-400">${idx + 1}</td>
                <td>
                    <div class="review-date-box">
                        <span class="review-day-name">${dayName}</span>
                        <span class="review-date-val">${dateNum}</span>
                    </div>
                </td>
                <td>
                    <div class="d-flex flex-wrap gap-2">${projectsHTML || '--'}</div>
                </td>
                <td class="text-center fw-boldest text-dark fs-5">${totalHrs}h</td>
            </tr>
        `;
    });
}

function renderFeedbackSection(data) {
    const noteArea = document.getElementById('overall_revision_note');
    if (noteArea && data.revisionNote) {
        noteArea.value = data.revisionNote;
    }
}


async function approveTimesheet() {
    const data = window.currentReviewData;
    if (!data) return;

    const { isConfirmed } = await Swal.fire({
        title: 'Approve Timesheet?',
        text: `Are you sure you want to approve ${data.employeeName}'s timesheet for ${document.getElementById('review_period_label').innerText}?`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Confirm',
        cancelButtonText: 'Cancel',
        confirmButtonColor: '#D31145',
        customClass: { confirmButton: 'btn btn-primary', cancelButton: 'btn btn-light' }
    });

    if (isConfirmed) {
        const payload = {
            submissionId: data.submissionId,
            employeeId: data.employeeId,
            month: data.month,
            year: data.year
        };

        const res = await fetchAPI('timesheet/supervisor/approve', 'POST', payload);
        if (res) {
            await Swal.fire('Success', 'Timesheet has been approved.', 'success');
            window.location.href = '/Timesheet/Supervisor/Report';
        }
    }
}

async function giveRevision() {
    const data = window.currentReviewData;
    if (!data) return;

    const feedback = document.getElementById('overall_revision_note').value;

    if (!feedback || feedback.trim().length < 5) {
        Swal.fire('Note Required', 'Please provide a clear revision note (min 5 characters) so the employee knows what to fix.', 'warning');
        return;
    }

    const { isConfirmed } = await Swal.fire({
        title: 'Request Revision?',
        text: 'This will send the timesheet back to the employee for corrections.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Confirm',
        cancelButtonText: 'Cancel',
        confirmButtonColor: '#D31145'
    });

    if (isConfirmed) {
        const payload = {
            submissionId: data.submissionId,
            employeeId: data.employeeId,
            month: data.month,
            year: data.year,
            revisionNote: feedback
        };

        const res = await fetchAPI('timesheet/supervisor/revision', 'POST', payload);
        if (res) {
            await Swal.fire('Revision Requested', 'The employee has been notified to revise their logs.', 'info');
            window.location.href = '/Timesheet/Supervisor/Report';
        }
    }
}

async function fetchAPI(endpoint, method = 'GET', body = null) {
    const token = localStorage.getItem('aia_jwt_token');
    const options = {
        method,
        headers: { 
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    };
    if (body) options.body = JSON.stringify(body);

    const r = await fetch(`https://localhost:7089/api/${endpoint}`, options);
    if (!r.ok) {
        const err = await r.json();
        Swal.fire('Error', err.message || 'Operation failed', 'error');
        return null;
    }
    const json = await r.json();
    return json.data || json;
}

document.addEventListener('DOMContentLoaded', () => {
    initReviewPage();
});
