document.addEventListener("DOMContentLoaded", function () {

    const form = document.querySelector('.leave-form');
    const submitBtn = document.getElementById('submitBtn');

    submitBtn.addEventListener('click', function (e) {

        e.preventDefault();

        const startDate = document.getElementById('startDate').value;
        const description = document.getElementById('description').value.trim();

        let isValid = true;

        if (!startDate) {
            document.getElementById('startDateError').textContent =
                'Start date is required';
            isValid = false;
        }

        if (!description) {
            document.getElementById('descriptionError').textContent =
                'Description is required';
            isValid = false;
        }

        if (!isValid) return;

        const modal = new bootstrap.Modal(
            document.getElementById('submitModal')
        );

        modal.show();
    });

    document
        .getElementById('confirmSubmitBtn')
        .addEventListener('click', function () {

            form.submit();
        });

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

        var allowedExtensions = ['zip', 'pdf', 'jpg', 'jpeg', 'png'];

        var invalidFile = newFiles.find(function (file) {

            var ext = file.name.split('.').pop().toLowerCase();

            return !allowedExtensions.includes(ext);
        });

        if (invalidFile) {

            showAttachmentError(
                "Only ZIP, PDF, JPG, JPEG, and PNG files are allowed."
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

        fileInput.disabled = isLimitReached;

        drop.classList.toggle('disabled', isLimitReached);

        content.innerHTML = `
            <div class="uploaded-list"></div>

            ${!isLimitReached
                ? `
                <div class="upload-top" style="margin-top:${selectedFiles.length > 0 ? '18px' : '0'}">
                    <div class="drop-icon">
                        <i class="bi bi-file-earmark-arrow-up-fill"></i>
                    </div>

                    <div class="drop-title">
                        Click or drag file to upload
                    </div>

                    <div class="drop-sub">
                        Medical certificate, travel docs, etc. (PDF, JPG, max 5MB)
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

        selectedFiles.forEach(function (file, index) {

            var iconClass = "bi-file-earmark-fill";
            var iconColor = "#6c757d";

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

            uploadedList.insertAdjacentHTML('beforeend', `
                <div class="uploaded-file clickable-file" data-index="${index}">
                    <div class="uploaded-left">
                        <div class="uploaded-icon">
                            <i class="bi ${iconClass}" style="color:${iconColor}"></i>
                        </div>

                        <div class="uploaded-info">
                            <div class="uploaded-name">${file.name}</div>
                            <div class="uploaded-size">${(file.size / 1024).toFixed(1)} KB</div>
                        </div>
                    </div>

                    <button type="button"
                            class="remove-file-btn"
                            data-index="${index}">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            `);

        });

        bindFileEvents();
    }

    function bindFileEvents() {

        document.querySelectorAll('.remove-file-btn').forEach(function (btn) {

            btn.onclick = function (e) {

                e.stopPropagation();

                var index = parseInt(btn.dataset.index);

                selectedFiles.splice(index, 1);

                renderFiles();
            };

        });

        document.querySelectorAll('.clickable-file').forEach(function (item) {

            item.onclick = function (e) {

                e.stopPropagation();

                var file = selectedFiles[parseInt(item.dataset.index)];

                if (!file) return;

                window.open(URL.createObjectURL(file), '_blank');
            };

        });

    }

    // =========================
    // SUBMIT
    // =========================
    var leaveForm = document.querySelector('.leave-form');

    startDateInput.addEventListener('change', function () {
        document.getElementById('startDateError').style.display = 'none';
    });

    document.getElementById('description').addEventListener('input', function () {
        document.getElementById('descriptionError').style.display = 'none';
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

        // Start Date validation
        if (!startDate.value.trim()) {

            startDateError.textContent =
                'Start Date is required.';

            startDateError.style.display = 'block';

            isValid = false;
        }

        // Description validation
        if (!description.value.trim()) {

            descriptionError.textContent =
                'Description is required.';

            descriptionError.style.display = 'block';

            isValid = false;
        }

        if (!isValid) {
            return;
        }

        const formData = new FormData(leaveForm);

        const leaveResponse = await fetch(
            leaveForm.action || window.location.href,
            {
                method: "POST",
                body: formData
            }
        );

        if (!leaveResponse.ok) {
            alert("Failed to create leave request");
            return;
        }

        const leaveResult = await leaveResponse.json();

        const leaveId = leaveResult.data.leaveId;

        // upload attachment
        if (selectedFiles.length > 0) {

            const attachmentForm = new FormData();

            attachmentForm.append(
                "DocumentType",
                "Supporting Document"
            );

            selectedFiles.forEach(file => {
                attachmentForm.append("Files", file);
            });

            await fetch(
                `/employees/${employeeId}/leaves/${leaveId}/attachments`,
                {
                    method: "POST",
                    body: attachmentForm
                }
            );
        }

        window.location.reload();

    });

});