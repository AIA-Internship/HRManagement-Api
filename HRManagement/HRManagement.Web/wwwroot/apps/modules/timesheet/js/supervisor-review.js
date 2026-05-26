const urlParams = new URLSearchParams(window.location.search);
const submissionId = urlParams.get('id');
let dayComments = {}; // Map of date -> comment content

async function initReviewPage() {
    if (!submissionId) { window.location.href = '/Timesheet/Supervisor/Dashboard'; return; }
    
    const data = await fetchAPI(`supervisor/review/${submissionId}`);
    if (!data) return;

    // Sync Header
    const internNameEl = document.getElementById('intern_name');
    if(internNameEl) internNameEl.innerText = data.employeeName;
    
    const internAvatarEl = document.getElementById('intern_avatar');
    if(internAvatarEl) internAvatarEl.innerText = data.employeeName.charAt(0);
    
    const reviewPeriodEl = document.getElementById('review_period');
    if(reviewPeriodEl && indonesianMonths) reviewPeriodEl.innerText = `${indonesianMonths[data.month-1]} ${data.year}`;
    
    const submissionStatusEl = document.getElementById('submission_status');
    if(submissionStatusEl) submissionStatusEl.innerText = data.status.toUpperCase();
    
    const submittedAtEl = document.getElementById('submitted_at');
    if(submittedAtEl) submittedAtEl.innerText = `Submitted ${data.reviewedDate || '--'}`;
    
    const overallRevisionNoteEl = document.getElementById('overall_revision_note');
    if(overallRevisionNoteEl) overallRevisionNoteEl.value = data.revisionNote || '';

    // Populate Table
    const tbody = document.getElementById('review_tbody');
    if(!tbody) return;
    tbody.innerHTML = '';

    // Map existing comments
    if (data.dayComments) {
        data.dayComments.forEach(c => { dayComments[c.date] = c.comment; });
    }

    data.days.forEach(day => {
        const tr = document.createElement('tr');
        const projectText = Object.keys(day.projectMinutes).map(p => `<strong>${p}</strong>`).join(', ');
        const hours = (day.totalMinutes / 60).toFixed(1);

        tr.innerHTML = `
            <td class="text-center fw-boldest text-gray-600">${day.date.split('-')[2]}</td>
            <td>
                <div class="fs-7 text-gray-800">${projectText || '<span class="text-muted">No specific project</span>'}</div>
            </td>
            <td class="text-center fw-boldest text-gray-900">${hours}h</td>
            <td>
                <input type="text" class="form-control form-control-sm border-0 fs-8 fw-bold p-2" 
                       style="background: #FBFBFB; border: 1px solid #ECEFF4 !important; border-radius: 6px;"
                       placeholder="Comment for this day..." 
                       value="${dayComments[day.date] || ''}"
                       onchange="updateDayComment('${day.date}', this.value)">
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function updateDayComment(date, val) {
    dayComments[date] = val;
}

async function approveTimesheet() {
    const confirm = await Swal.fire({
        title: 'Confirm Approval',
        text: "Are you sure you want to approve this timesheet?",
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#10b981',
        confirmButtonText: 'Yes, Approve It!'
    });

    if (!confirm.isConfirmed) return;

    const res = await fetchAPI('supervisor/approve', {
        method: 'POST',
        body: JSON.stringify({ submissionId: parseInt(submissionId) })
    });

    if (res) {
        Swal.fire('Approved!', 'Timesheet has been approved.', 'success').then(() => {
            window.location.href = '/Timesheet/Supervisor/Dashboard';
        });
    }
}

async function giveRevision() {
    const note = document.getElementById('overall_revision_note').value;
    if (!note) {
        Swal.fire('Note Required', 'Please provide a general feedback reason for revision.', 'warning');
        return;
    }

    const formattedDayComments = Object.keys(dayComments).map(dt => ({
        date: dt,
        comment: dayComments[dt]
    })).filter(c => c.comment.trim() !== '');

    const res = await fetchAPI('supervisor/revision', {
        method: 'POST',
        body: JSON.stringify({ 
            submissionId: parseInt(submissionId),
            overallNote: note,
            dayComments: formattedDayComments
        })
    });

    if (res) {
        Swal.fire('Revision Sent', 'Intern will be notified to revise their logs.', 'info').then(() => {
            window.location.href = '/Timesheet/Supervisor/Dashboard';
        });
    }
}

document.addEventListener('DOMContentLoaded', () => {
    if(document.getElementById('review_tbody')) {
        initReviewPage();
    }
});
