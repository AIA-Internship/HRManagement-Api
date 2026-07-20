const API_BASE = "https://localhost:7089";

async function apiGet(endpoint) {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    if (!token) { window.aiaAuth && window.aiaAuth.signOut(); return null; }
    try {
        const res = await fetch(`${API_BASE}${endpoint}`, { headers: { 'Authorization': `Bearer ${token}` } });
        if (res.status === 401) { window.aiaAuth.signOut(); return null; }
        const json = await res.json();
        return json.content || json.data || json;
    } catch (err) {
        console.error("API GET failed:", err);
        return null;
    }
}

async function apiDelete(endpoint) {
    console.log("endpoint =", endpoint);
    console.log("API_BASE =", API_BASE);

    try {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            method: "DELETE",
            headers: {
                Authorization: `Bearer ${window.aiaAuth.getToken()}`
            }
        });

        console.log("status =", res.status);

        const text = await res.text();
        console.log(text);

        return text;
    }
    catch (err) {
        console.error(err.name);
        console.error(err.message);
        console.error(err);
        throw err;
    }
}

document.addEventListener("DOMContentLoaded", function () {

    const form = document.querySelector('.leave-form');
    const submitBtn = document.getElementById('submitBtn');

    submitBtn.addEventListener('click', function (e) {

        e.preventDefault();

        const startDate = document.getElementById('startDate').value;
        const description = document.getElementById('description').value.trim();

        let isValid = true;

        if (!startDate) {
            const sd = document.getElementById('startDate');
            const sdErr = document.getElementById('startDateError');

            sdErr.textContent = 'Start date is required';
            sdErr.style.display = 'block';
            sdErr.style.fontWeight = '600';
            sd.classList.add('is-invalid');
            isValid = false;
        }

        if (!description) {
            const desc = document.getElementById('description');
            const descErr = document.getElementById('descriptionError');

            descErr.textContent = 'Description is required';
            descErr.style.display = 'block';
            descErr.style.fontWeight = '600';
            desc.classList.add('is-invalid');
            isValid = false;
        }

        if (!isValid) {
            submitBtn.blur();
            return;
        }

        const modal = new bootstrap.Modal(
            document.getElementById('submitModal')
        );

        modal.show();
    });

    document
        .getElementById('confirmSubmitBtn')
        .addEventListener('click', function () {

            if (typeof form.requestSubmit === 'function') {
                form.requestSubmit();
            }
            else {

                var evt = new Event('submit', { bubbles: true, cancelable: true });
                var canceled = !form.dispatchEvent(evt);

                if (canceled === false) {
                    form.submit();
                }
            }
        });

    document.getElementById("cancelBtn").addEventListener("click", function () {
        const params = new URLSearchParams(window.location.search);
        const leaveId = params.get("id");

        if (leaveId) {
            window.location.href = `/Leave/Employee/LeaveDetail?id=${leaveId}`;
        } else {
            window.location.href = "/Leave/Employee/Dashboard";
        }
    });

    document
        .getElementById("backToDetailBtn")
        .addEventListener("click", function () {

            const params = new URLSearchParams(window.location.search);
            const leaveId = params.get("id");

            window.location.href =
                `/Leave/Employee/LeaveDetail?id=${leaveId}`;
        });

    async function getMyProfile() {
        const token = window.aiaAuth && window.aiaAuth.getToken();

        if (!token) {
            window.aiaAuth && window.aiaAuth.signOut();
            return null;
        }

        try {
            const res = await fetch(`${API_BASE}/api/employee/me`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (res.status === 401) {
                window.aiaAuth.signOut();
                return null;
            }

            const json = await res.json();

            return json?.content || json?.data || json;
        }
        catch (err) {
            console.error('getMyProfile failed:', err);
            return null;
        }
    }



    async function apiPost(endpoint, payload) {
        const token = window.aiaAuth && window.aiaAuth.getToken();

        const res = await fetch(`${API_BASE}${endpoint}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        const json = await res.json().catch(() => null);

        console.log("STATUS =", res.status);
        console.log("RESPONSE =", json);

        return json;
    }

    async function apiUpload(endpoint, formData) {
        const token = window.aiaAuth && window.aiaAuth.getToken();
        if (!token) { window.aiaAuth && window.aiaAuth.signOut(); return null; }
        try {
            const res = await fetch(`${API_BASE}${endpoint}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`
                },
                body: formData
            });

            if (res.status === 401) { window.aiaAuth.signOut(); return null; }

            return res;
        }
        catch (err) {
            console.error('apiUpload failed:', err);
            return null;
        }
    }

    // =========================
    // TOM SELECT
    // =========================
    var leaveWrapper = document.getElementById('leaveWrapper');

    var leaveTs = new TomSelect("#leaveType", {
        create: false,
        allowEmptyOption: false,

        onDropdownOpen: function () {
            if (leaveWrapper) {
                leaveWrapper.classList.add('open');
            }
        },

        onDropdownClose: function () {
            if (leaveWrapper) {
                leaveWrapper.classList.remove('open');
            }
        }
    });

    if (leaveWrapper) {
        leaveWrapper.addEventListener('click', function () {

            if (leaveTs.isOpen) {
                leaveTs.close();
            }
            else {
                leaveTs.open();
            }

        });
    }

    // =========================
    // STEPPER
    // =========================
    document.querySelectorAll('.stepper').forEach(function (stepper) {

        var input = stepper.querySelector('.stepper-input');

        stepper.addEventListener('click', function (e) {

            var btn = e.target.closest('[data-action]');

            if (!btn) return;

            var action = btn.getAttribute('data-action');

            var val = parseFloat(input.value) || 0.5;

            if (action === 'increment') {

                if (val === 0) {
                    val = 0.5;
                }
                else if (val === 0.5) {
                    val = 1;
                }
                else {
                    val += 1;
                }

            }

            if (action === 'decrement') {

                if (val === 1) {
                    val = 0.5;
                }
                else if (val === 0.5) {
                    val = 0.5;
                }
                else if (val > 1) {
                    val -= 1;
                }

            }

            input.value = val;

            document.getElementById('businessDaysText').textContent =
                `${daysInput.value} Business Days`;

        });

    });

    // =========================
    // DATE INPUT
    // =========================
    var startDateInput = document.getElementById('startDate');
    var endDateInput = document.getElementById('endDate');
    var daysInput = document.getElementById('daysInput');

    async function loadExistingLeave() {
        try {
            const params = new URLSearchParams(window.location.search);
            const leaveId = params.get('id');
            if (!leaveId) return;

            const result = await apiGet(`/api/leave/get-by-leave-id/${leaveId}`);
            const dto = Array.isArray(result) ? result[0] : result;
            if (!dto) return;

            const attachments = await apiGet(`/api/leave/${leaveId}/attachments`);

            existingAttachments = attachments || [];

            renderFiles();

            function getField(obj, ...names) {
                for (const n of names) {
                    if (!obj) continue;
                    if (Object.prototype.hasOwnProperty.call(obj, n)) return obj[n];
                    const lower = n.charAt(0).toLowerCase() + n.slice(1);
                    if (Object.prototype.hasOwnProperty.call(obj, lower)) return obj[lower];
                }
                return null;
            }

            const lt = getField(dto, 'leaveType', 'LeaveType');

            if (lt != null) {

                let value = "";

                switch (lt) {
                    case "PaidLeave":
                        value = "1";
                        break;

                    case "UnpaidLeave":
                        value = "2";
                        break;
                }

                leaveTs.setValue(value);
            }

            // Dates and days
            const startRaw = getField(dto, 'leaveStartDate', 'LeaveStartDate');
            const dayAmount = getField(dto, 'dayAmount', 'DayAmount', 'Days');

            if (startRaw) {
                const d = new Date(startRaw);
                const yyyy = d.getFullYear();
                const mm = String(d.getMonth() + 1).padStart(2, '0');
                const dd = String(d.getDate()).padStart(2, '0');
                startDateInput.value = `${yyyy}-${mm}-${dd}`;
            }

            if (dayAmount != null) {
                daysInput.value = String(dayAmount);
            }

            // Description
            const desc = getField(dto, 'leaveDescription', 'LeaveDescription', 'Description');
            if (desc != null) {
                const descEl = document.getElementById('description');
                if (descEl) descEl.value = desc;
            }

            // recompute end date and calendar
            updateEndDate();
            refreshCalendarRange();
            document.getElementById('businessDaysText').textContent = `${daysInput.value} Business Days`;

        } catch (err) {
            console.error('Failed to load existing leave:', err);
        }
    }

    // attempt to load when page is ready
    loadExistingLeave();

    function updateEndDate() {

        if (!startDateInput.value) {

            endDateInput.value = '';

            return;
        }

        var start = new Date(startDateInput.value);

        var days = parseFloat(daysInput.value) || 0;

        if (days <= 1) {

            endDateInput.value = startDateInput.value;

            return;
        }

        var end = new Date(start);

        end.setDate(end.getDate() + (Math.ceil(days) - 1));

        var yyyy = end.getFullYear();
        var mm = String(end.getMonth() + 1).padStart(2, '0');
        var dd = String(end.getDate()).padStart(2, '0');

        endDateInput.value = `${yyyy}-${mm}-${dd}`;
    }

    startDateInput.addEventListener('change', updateEndDate);

    document.querySelectorAll('.stepper-btn').forEach(function (btn) {

        btn.addEventListener('click', function () {

            setTimeout(function () {
                updateEndDate();
                refreshCalendarRange();
            }, 10);

        });

    });

    updateEndDate();

    // =========================
    // FLATPICKR CALENDAR
    // =========================
    var calendar = flatpickr("#leaveCalendar", {
        inline: true,
        mode: "range",
        monthSelectorType: "static",
        defaultDate: new Date(),
        clickOpens: false,
        prevArrow: '<i class="bi bi-chevron-left"></i>',
        nextArrow: '<i class="bi bi-chevron-right"></i>'
    });

    function refreshCalendarRange() {

        if (!startDateInput.value) return;

        calendar.setDate([
            startDateInput.value,
            endDateInput.value || startDateInput.value
        ], true);

    }

    startDateInput.addEventListener("change", function () {
        updateEndDate();
        refreshCalendarRange();
    });

    function normalizeDaysInput() {

        let val = parseFloat(daysInput.value);

        if (isNaN(val) || val < 0.5) {
            val = 0.5;
        }

        if (!Number.isInteger(val) && val !== 0.5) {
            val = 0.5;
        }

        daysInput.value = val;

        updateEndDate();
        refreshCalendarRange();
        document.getElementById('businessDaysText').textContent =
            `${daysInput.value} Business Days`;
    }

    daysInput.addEventListener('blur', normalizeDaysInput);

    daysInput.addEventListener('keydown', function (e) {

        if (e.key === 'Enter') {
            e.preventDefault();
            normalizeDaysInput();
        }

    });

    // =========================
    // DROPZONE
    // =========================
    var drop = document.getElementById('dropzone');
    var fileInput = document.getElementById('attachmentInput');
    var content = drop.querySelector('.dropzone-content');

    var selectedFiles = [];
    var existingAttachments = [];
    var deletedAttachmentIds = [];

    drop.addEventListener('click', function (e) {

        if (e.target.closest('.clickable-file')) return;

        if (e.target.closest('.remove-file-btn')) return;

        fileInput.click();

    });

    drop.addEventListener('dragover', function (e) {

        e.preventDefault();

        drop.classList.add('drag');

    });

    drop.addEventListener('dragleave', function () {

        drop.classList.remove('drag');

    });

    drop.addEventListener('drop', function (e) {

        e.preventDefault();

        drop.classList.remove('drag');

        addFiles(e.dataTransfer.files);

    });

    fileInput.addEventListener('change', function () {

        addFiles(fileInput.files);
        fileInput.value = '';

    });

    var attachmentError = document.getElementById('attachmentError');

    function showAttachmentError(message) {
        attachmentError.textContent = message;
        attachmentError.style.display = 'block';
    }

    function clearAttachmentError() {
        attachmentError.textContent = '';
        attachmentError.style.display = 'none';
    }

    function addFiles(files) {

        if (!files || files.length === 0) return;

        var newFiles = Array.from(files);

        var allowedExtensions = ['pdf', 'jpg', 'jpeg', 'png'];

        var invalidFile = newFiles.find(function (file) {

            var ext = file.name.split('.').pop().toLowerCase();

            return !allowedExtensions.includes(ext);
        });

        if (invalidFile) {

            showAttachmentError(
                "Only PDF, JPG, JPEG, and PNG files are allowed."
            );

            return;
        }

        var totalSize = selectedFiles.reduce(function (sum, file) {
            return sum + file.size;
        }, 0);

        newFiles.forEach(function (file) {
            totalSize += file.size;
        });

        if (totalSize > 5 * 1024 * 1024) {

            showAttachmentError(
                "Could not upload file. Total attachment size cannot exceed 5 MB."
            );

            return;
        }

        clearAttachmentError();

        newFiles.forEach(function (file) {
            selectedFiles.push(file);
        });

        renderFiles();
    }

    function renderFiles() {

        var totalSize = selectedFiles.reduce(function (sum, file) {
            return sum + file.size;
        }, 0);

        var isLimitReached = totalSize >= 5 * 1024 * 1024;

        const totalFiles =
            selectedFiles.length + existingAttachments.length;

        fileInput.disabled = isLimitReached;

        drop.classList.toggle('disabled', isLimitReached);

        content.innerHTML = `
            <div class="uploaded-list"></div>

            ${!isLimitReached
                ? `
            <div class="upload-top ${totalFiles > 0 ? 'has-files' : ''}">
                <div class="drop-icon">
                    <i class="bi bi-file-earmark-arrow-up-fill"></i>
                </div>

                <div class="drop-title">
                    Click or drag file to upload
                </div>

                <div class="drop-sub">
                    Medical certificate, travel docs, etc. (PDF, JPG, JPEG, PNG; max 5MB)
                </div>
            </div>
            `
                :
                `
            <div class="drop-sub text-danger" style="margin-top:1.7rem">
                Maximum total attachment size reached (5 MB)
            </div>
            `
            }
    `;

        var uploadedList = content.querySelector('.uploaded-list');

        existingAttachments.forEach((file, index) => {
            const displayName = file.fileName || file.name || '';
            const ext = (displayName.split('.').pop() || '').toLowerCase();
            let iconClass = 'bi-file-earmark-fill';
            let iconColor = '#6c757d';

            if (ext === 'pdf') {
                iconClass = 'bi-file-earmark-pdf-fill';
                iconColor = '#dc3545';
            } else if (ext === 'jpg' || ext === 'jpeg' || ext === 'png') {
                iconClass = 'bi-file-earmark-image-fill';
                iconColor = '#0d6efd';
            }

            uploadedList.insertAdjacentHTML("beforeend", `
            <div class="uploaded-file existing-file">

                <div class="uploaded-left">

                    <div class="uploaded-icon">
                        <i class="bi ${iconClass}" style="color:${iconColor}; font-size:2.7rem"></i>
                    </div>

                    <div class="uploaded-info">
                        <div class="uploaded-name">
                            ${displayName}
                        </div>

                        <div class="uploaded-size">
                            ${( (file.fileSize || file.size || 0) / 1024).toFixed(1)} KB
                        </div>

                    </div>

                </div>

                <button
                    type="button"
                    data-type="new"
                    class="remove-existing-btn"
                    data-index="${index}">
                    <i class="bi bi-trash"></i>
                </button>

            </div>
        `);
        });


        selectedFiles.forEach((file, index) => {

            let iconClass = "bi-file-earmark-fill";
            let iconColor = "#6c757d";

            if (file.name.toLowerCase().endsWith(".pdf")) {
                iconClass = "bi-file-earmark-pdf-fill";
                iconColor = "#dc3545";
            }
            else if (
                file.name.toLowerCase().endsWith(".jpg") ||
                file.name.toLowerCase().endsWith(".jpeg") ||
                file.name.toLowerCase().endsWith(".png")
            ) {
                iconClass = "bi-file-earmark-image-fill";
                iconColor = "#0d6efd";
            }

            uploadedList.insertAdjacentHTML("beforeend", `
            <div class="uploaded-file clickable-file"
                 data-index="${index}">

                <div class="uploaded-left">

                    <div class="uploaded-icon">
                        <i class="bi ${iconClass}"
                           style="color:${iconColor}"></i>
                    </div>

                    <div class="uploaded-info">

                        <div class="uploaded-name">
                            ${file.name}
                        </div>

                        <div class="uploaded-size">
                            ${(file.size / 1024).toFixed(1)} KB
                        </div>

                    </div>

                </div>

                <button
                    type="button"
                    class="remove-file-btn"
                    data-index="${index}">
                    <i class="bi bi-trash"></i>
                </button>

            </div>
        `);

        });


        bindFileEvents();
    }

    document.querySelectorAll(".remove-existing-btn")
        .forEach(btn => {

            btn.onclick = function (e) {

                e.stopPropagation();

                const index = parseInt(btn.dataset.index);

                const attachment = existingAttachments[index];

                deletedAttachmentIds.push(attachment.attachmentId);

                existingAttachments.splice(index, 1);

                renderFiles();

            };

        });

    function bindFileEvents() {

        document.querySelectorAll('.remove-file-btn').forEach(btn => {

            btn.onclick = function (e) {

                e.stopPropagation();

                const index = parseInt(this.dataset.index);

                selectedFiles.splice(index, 1);

                renderFiles();
            };

        });

        document.querySelectorAll('.remove-existing-btn').forEach(btn => {

            btn.onclick = function (e) {

                e.stopPropagation();

                const index = parseInt(this.dataset.index);

                const attachment = existingAttachments[index];

                deletedAttachmentIds.push(attachment.attachmentId);

                existingAttachments.splice(index, 1);

                renderFiles();
            };

        });

    }

    // =========================
    // SUBMIT
    // =========================
    var leaveForm = document.querySelector('.leave-form');

    startDateInput.addEventListener('change', function () {
        var sdErr = document.getElementById('startDateError');
        sdErr.style.display = 'none';
        sdErr.textContent = '';
        startDateInput.classList.remove('is-invalid');
    });

    document.getElementById('description').addEventListener('input', function () {
        var descErr = document.getElementById('descriptionError');
        descErr.style.display = 'none';
        descErr.textContent = '';
        this.classList.remove('is-invalid');
    });

    leaveForm.addEventListener('submit', async function (e) {

        e.preventDefault();

        var isValid = true;

        var startDate = document.getElementById('startDate');
        var description = document.getElementById('description');

        var startDateError = document.getElementById('startDateError');
        var descriptionError = document.getElementById('descriptionError');

        startDateError.style.display = 'none';
        descriptionError.style.display = 'none';

        // clear invalid styles
        startDate.classList.remove('is-invalid');
        description.classList.remove('is-invalid');

        // validation
        if (!startDate.value.trim()) {

            startDateError.textContent =
                'Start Date is required.';

            startDateError.style.display = 'block';
            startDate.classList.add('is-invalid');
            isValid = false;
        }

        if (!description.value.trim()) {

            descriptionError.textContent =
                'Description is required.';

            descriptionError.style.display = 'block';
            description.classList.add('is-invalid');
            isValid = false;
        }

        if (!isValid) {
            return;
        }

        try {
            const params = new URLSearchParams(window.location.search);

            const leavePayload = {
                leaveId: parseInt(params.get("id")),
                leaveDescription: description.value.trim(),
                leaveStartDate: startDate.value,
                dayAmount: parseFloat(daysInput.value),
                leaveType: leaveTs.getValue() === "1" ? 1 : 2
            };

            console.log("leavePayload =", leavePayload);
            console.log(JSON.stringify(leavePayload, null, 2));
            const leaveResult = await apiPost('/api/leave/edit', leavePayload);

            if (!leaveResult) {
                alert('Failed to Update leave request');
                return;
            }

            console.log('leaveResult:', leaveResult);

            const leaveId = parseInt(params.get("id"));

            for (const attachmentId of deletedAttachmentIds) {
                await apiDelete(`/api/leave/${attachmentId}/attachments`);
            }

            if (selectedFiles.length > 0) {

                for (const file of selectedFiles) {

                    const attachmentForm = new FormData();

                    attachmentForm.append(
                        'DocumentType',
                        'Supporting Document'
                    );

                    attachmentForm.append(
                        'Files',
                        file
                    );

                    const attachmentResponse = await apiUpload(`/api/leave/${leaveId}/attachments`, attachmentForm);

                    if (!attachmentResponse || !attachmentResponse.ok) {

                        console.error(
                            `Failed upload file: ${file.name}`
                        );
                    }
                }
            }

            const submitModalEl = document.getElementById("submitModal");
            const submitModal =
                bootstrap.Modal.getInstance(submitModalEl);

            if (submitModal) {
                submitModal.hide();
            }

            const successModal =
                new bootstrap.Modal(document.getElementById("successModal"));

            successModal.show();

        }
        catch (error) {

            console.error(error);

            alert('Something went wrong');
        }

    });

});