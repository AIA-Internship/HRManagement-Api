(function ($, app) {
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    app.elearning.supervisor.modules = {};
    app.elearning.supervisor.modules.visibleCount = 8;
    app.elearning.supervisor.modules.selectedProgram = null;
    app.elearning.supervisor.modules.selectedBatch = null;
    app.elearning.supervisor.modules.selectedRoles = [];
    app.elearning.supervisor.modules._programs = [];
    app.elearning.supervisor.modules._batches = [];
    app.elearning.supervisor.modules._modules = [];
    app.elearning.supervisor.modules._allRoles = [];

    app.elearning.supervisor.modules._api = async function (path, opts) {
        var token = window.aiaAuth && window.aiaAuth.getToken();
        if (!token) { window.aiaAuth && window.aiaAuth.signOut(); return null; }
        opts = opts || {};
        opts.headers = $.extend({ 'Authorization': 'Bearer ' + token, 'Content-Type': 'application/json' }, opts.headers || {});
        try {
            var res = await fetch('https://localhost:7089' + path, opts);
            if (res.status === 401) { window.aiaAuth.signOut(); return null; }
            if (res.status === 404) return null;
            if (!res.ok) return null;
            var json = await res.json();
            if (json && json.isError) return null;
            return json;
        } catch (err) {
            console.error('API Error:', err);
            return null;
        }
    };

    app.elearning.supervisor.modules.init = function () {
        var self = this;
        app.loading && app.loading.show('Loading...');
        self._api('/api/ELearning/programs').then(function (json) {
            var programs = (json && (json.content || json.data || json)) || [];
            self._programs = Array.isArray(programs) ? programs : [];
            if (self._programs.length) {
                self.selectedProgram = self._programs[0].programId;
            }
            self.renderProgramDropdown();
            return self._loadBatches();
        }).then(function () {
            return self._loadModules();
        }).then(function () {
            app.loading && app.loading.hide();
            self.bindEvents();
        });
    };

    app.elearning.supervisor.modules._loadBatches = function () {
        var self = this;
        if (!self.selectedProgram) { self._batches = []; self.renderBatchDropdown(); return Promise.resolve(); }
        return self._api('/api/ELearning/programs/' + self.selectedProgram + '/batches').then(function (json) {
            var batches = (json && (json.content || json.data || json)) || [];
            self._batches = Array.isArray(batches) ? batches : [];
            if (self._batches.length && !self.selectedBatch) {
                self.selectedBatch = self._batches[0].batchId;
            }
            self.renderBatchDropdown();
        });
    };

    app.elearning.supervisor.modules._loadModules = function () {
        var self = this;
        if (!self.selectedBatch) { self._modules = []; self.renderModules(); return Promise.resolve(); }
        var search = $('#el-sv-module-search').val() || '';
        var rolesParam = self.selectedRoles.map(function (r) { return 'roles=' + encodeURIComponent(r); }).join('&');
        var url = '/api/ELearning/batches/' + self.selectedBatch + '/modules?search=' + encodeURIComponent(search) + (rolesParam ? '&' + rolesParam : '');
        return self._api(url).then(function (json) {
            var modules = (json && (json.content || json.data || json)) || [];
            self._modules = Array.isArray(modules) ? modules : [];
            // Update master role list: only track real roles (not 'all')
            self._modules.forEach(function (m) {
                var r = (m.role || '').toLowerCase();
                if (r && r !== 'all' && self._allRoles.indexOf(m.role) === -1) {
                    self._allRoles.push(m.role);
                }
            });
            self.renderModules();
        });
    };

    app.elearning.supervisor.modules.renderProgramDropdown = function () {
        var self = this;
        var menu = $('#el-sv-program-menu');
        menu.find('.el-sv-dropdown-item:not(.el-sv-dropdown-add)').remove();
        self._programs.forEach(function (p) {
            menu.append('<div class="el-sv-dropdown-item' + (p.programId === self.selectedProgram ? ' active' : '') + '" data-value="' + p.programId + '">' + (p.programName || '') + '</div>');
        });
        var selected = self._programs.find(function (p) { return p.programId === self.selectedProgram; });
        if (selected) $('#el-sv-program-text').text(selected.programName || '');
    };

    app.elearning.supervisor.modules.renderBatchDropdown = function () {
        var self = this;
        var menu = $('#el-sv-batch-menu');
        menu.find('.el-sv-dropdown-item:not(.el-sv-dropdown-add)').remove();
        self._batches.forEach(function (b) {
            menu.append('<div class="el-sv-dropdown-item' + (b.batchId === self.selectedBatch ? ' active' : '') + '" data-value="' + b.batchId + '">' + (b.batchName || '') + '</div>');
        });
        var selected = self._batches.find(function (b) { return b.batchId === self.selectedBatch; });
        if (selected) {
            $('#el-sv-batch-text').text(selected.batchName || '');
            var period = '';
            if (selected.startDate && selected.endDate) {
                var s = new Date(selected.startDate);
                var e = new Date(selected.endDate);
                period = s.toLocaleDateString('en-GB', {day:'2-digit',month:'short',year:'numeric'}) + ' - ' + e.toLocaleDateString('en-GB', {day:'2-digit',month:'short',year:'numeric'});
            }
            $('#el-sv-period').text(period);
        }
    };

    app.elearning.supervisor.modules.renderModules = function () {
        var self = this;
        var searchTerm = ($('#el-sv-module-search').val() || '').toLowerCase();
        var activeRoles = self.selectedRoles;

        var filtered = self._modules.filter(function (m) {
            var matchSearch = (m.title || '').toLowerCase().indexOf(searchTerm) > -1;
            // Modules with role "All" always pass the role filter
            var isUniversalRole = (m.role || '').toLowerCase() === 'all';
            var matchRole = isUniversalRole || activeRoles.length === 0 || activeRoles.indexOf((m.role || '').toLowerCase()) > -1;
            return matchSearch && matchRole;
        });

        var visible = filtered.slice(0, self.visibleCount);
        var html = '';
        visible.forEach(function (mod, idx) {
            var modId = mod.moduleId || mod.id;
            html += '<div class="col-md-6 col-lg-3 mb-4">' +
                '<div class="el-sv-module-card el-animate el-animate-delay-' + ((idx % 4) + 1) + '" data-module-id="' + modId + '">' +
                '<div>' +
                '<div class="el-sv-card-role">' + (mod.role || '') + '</div>' +
                '<div class="el-sv-card-title">' + (mod.title || '') + '</div>' +
                '<div class="el-sv-card-due">Due Date: ' + (mod.dueDate || '') + '</div>' +
                '</div>' +
                '<div class="el-sv-action-icons">' +
                '<button class="el-sv-action-btn copy" data-action="copy" data-id="' + modId + '" title="Duplicate"><i class="ki-duotone ki-copy fs-5"><span class="path1"></span><span class="path2"></span></i></button>' +
                '<button class="el-sv-action-btn edit" data-action="edit" data-id="' + modId + '" title="Edit"><i class="ki-duotone ki-notepad-edit fs-5"><span class="path1"></span><span class="path2"></span></i></button>' +
                '<button class="el-sv-action-btn delete" data-action="delete" data-id="' + modId + '" title="Delete"><i class="ki-duotone ki-trash fs-5"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i></button>' +
                '</div>' +
                '</div>' +
                '</div>';
        });
        $('#el-sv-modules-grid').html(html);
        if (filtered.length > self.visibleCount) { $('#el-sv-load-more').show(); } else { $('#el-sv-load-more').hide(); }
    };

    app.elearning.supervisor.modules._validateFields = function (fields) {
        var valid = true;
        fields.forEach(function (f) {
            var val = $('#' + f.inputId).val().trim();
            if (!val) {
                $('#' + f.errId).removeClass('d-none');
                $('#' + f.inputId).addClass('is-invalid');
                valid = false;
            } else {
                $('#' + f.errId).addClass('d-none');
                $('#' + f.inputId).removeClass('is-invalid');
            }
        });
        return valid;
    };

    app.elearning.supervisor.modules._bindLiveClear = function (fields) {
        fields.forEach(function (f) {
            $(document).on('input change', '#' + f.inputId, function () {
                if ($(this).val().trim()) {
                    $(this).removeClass('is-invalid');
                    $('#' + f.errId).addClass('d-none');
                }
            });
        });
    };

    app.elearning.supervisor.modules.bindEvents = function () {
        var self = this;

        self._bindLiveClear([
            { inputId: 'el-add-module-title', errId: 'el-add-module-title-err' },
            { inputId: 'el-add-module-desc', errId: 'el-add-module-desc-err' },
            { inputId: 'el-add-module-due', errId: 'el-add-module-due-err' },
            { inputId: 'el-update-module-title', errId: 'el-update-module-title-err' },
            { inputId: 'el-update-module-desc', errId: 'el-update-module-desc-err' },
            { inputId: 'el-update-module-due', errId: 'el-update-module-due-err' },
            { inputId: 'el-add-program-name', errId: 'el-add-program-name-err' },
            { inputId: 'el-add-batch-start-date', errId: 'el-add-batch-start-err' },
            { inputId: 'el-add-batch-end-date', errId: 'el-add-batch-end-err' }
        ]);
        // For custom dropdowns, clear error on change/select
        $(document).on('change', '#el-add-module-role', function () {
            if ($(this).val()) { $('#el-add-module-role-dropdown').removeClass('is-invalid'); $('#el-add-module-role-err').addClass('d-none'); }
        });
        $(document).on('change', '#el-update-module-role', function () {
            if ($(this).val()) { $('#el-update-module-role-dropdown').removeClass('is-invalid'); $('#el-update-module-role-err').addClass('d-none'); }
        });

        $(document).on('click', '.el-sv-dropdown-toggle', function (e) {
            e.stopPropagation();
            var menu = $(this).siblings('.el-sv-dropdown-menu');
            $('.el-sv-dropdown-menu').not(menu).removeClass('show');
            menu.toggleClass('show');
        });
        $(document).on('click', function () { $('.el-sv-dropdown-menu').removeClass('show'); });
        $(document).on('click', '.el-sv-dropdown-menu', function (e) { e.stopPropagation(); });
        
        // Handle custom dropdown item selection for Roles
        $(document).on('click', '#el-add-module-role-menu .el-sv-dropdown-item', function () {
            var value = $(this).data('value');
            $('#el-add-module-role').val(value).trigger('change');
            $('#el-add-module-role-text').text($(this).text());
            $('#el-add-module-role-menu .el-sv-dropdown-item').removeClass('active');
            $(this).addClass('active');
            $('#el-add-module-role-menu').removeClass('show');
        });

        $(document).on('click', '#el-update-module-role-menu .el-sv-dropdown-item', function () {
            var value = $(this).data('value');
            $('#el-update-module-role').val(value).trigger('change');
            $('#el-update-module-role-text').text($(this).text());
            $('#el-update-module-role-menu .el-sv-dropdown-item').removeClass('active');
            $(this).addClass('active');
            $('#el-update-module-role-menu').removeClass('show');
        });

        $(document).on('click', '#el-sv-program-menu .el-sv-dropdown-item:not(.el-sv-dropdown-add)', function () {
            var value = parseInt($(this).data('value'));
            self.selectedProgram = value;
            self.selectedBatch = null;
            $('#el-sv-program-text').text($(this).text());
            $('#el-sv-program-menu .el-sv-dropdown-item').removeClass('active');
            $(this).addClass('active');
            $('#el-sv-program-menu').removeClass('show');
            app.loading && app.loading.show('Loading batches...');
            self._loadBatches().then(function () {
                return self._loadModules();
            }).then(function () {
                app.loading && app.loading.hide();
                self.visibleCount = 8;
            });
        });

        $(document).on('click', '#el-sv-batch-menu .el-sv-dropdown-item:not(.el-sv-dropdown-add)', function () {
            var value = parseInt($(this).data('value'));
            self.selectedBatch = value;
            $('#el-sv-batch-text').text($(this).text());
            var batch = self._batches.find(function (b) { return b.batchId === value; });
            if (batch) {
                var period = '';
                if (batch.startDate && batch.endDate) {
                    var s = new Date(batch.startDate);
                    var e = new Date(batch.endDate);
                    period = s.toLocaleDateString('en-GB', {day:'2-digit',month:'short',year:'numeric'}) + ' - ' + e.toLocaleDateString('en-GB', {day:'2-digit',month:'short',year:'numeric'});
                }
                $('#el-sv-period').text(period);
            }
            $('#el-sv-batch-menu .el-sv-dropdown-item').removeClass('active');
            $(this).addClass('active');
            $('#el-sv-batch-menu').removeClass('show');
            self.visibleCount = 8;
            app.loading && app.loading.show('Loading modules...');
            self._loadModules().then(function () { app.loading && app.loading.hide(); });
        });

        $(document).on('click', '.el-sv-dropdown-add', function () {
            var action = $(this).data('action');
            $('.el-sv-dropdown-menu').removeClass('show');
            if (action === 'add-program') {
                $('#el-add-program-name').val('').removeClass('is-invalid');
                $('#el-add-program-name-err').addClass('d-none');
                $('#el-modal-add-program').modal('show');
            } else if (action === 'add-batch') {
                $('#el-add-batch-start-date, #el-add-batch-end-date').val('').removeClass('is-invalid');
                $('#el-add-batch-start-err, #el-add-batch-end-err').addClass('d-none');
                $('#el-modal-add-batch').modal('show');
            }
        });

        $(document).on('input', '#el-sv-module-search', function () {
            self.visibleCount = 8;
            self._loadModules();
        });

        self._buildFilterModal();
        $(document).on('show.bs.modal', '#el-modal-filter-list', function () { self._buildFilterModal(); });
        $(document).on('click', '.el-filter-role-box', function () { $(this).toggleClass('selected'); });
        $(document).on('click', '#el-filter-select-all', function () {
            var boxes = $('.el-filter-role-box');
            var allSelected = boxes.filter('.selected').length === boxes.length;
            if (allSelected) { boxes.removeClass('selected'); } else { boxes.addClass('selected'); }
        });
        $(document).on('click', '#el-filter-ok', function () {
            var selected = [];
            $('.el-filter-role-box.selected').each(function () { selected.push($(this).data('role').toLowerCase()); });
            self.selectedRoles = selected;
            self.visibleCount = 8;
            self._loadModules();
        });

        $(document).on('click', '#el-sv-load-more', function () {
            self.visibleCount += 4;
            self.renderModules();
        });

        $(document).on('click', '#el-sv-add-module', function () {
            $('#el-add-module-title, #el-add-module-desc, #el-add-module-due').val('').removeClass('is-invalid');
            $('#el-add-module-title-err, #el-add-module-desc-err, #el-add-module-role-err, #el-add-module-due-err').addClass('d-none');
            $('#el-add-module-role').val('').trigger('change');
            $('#el-add-module-role-text').text('-- Select Role --');
            $('#el-add-module-role-dropdown').removeClass('is-invalid');
            self._populateRoleDropdowns();
            $('#el-modal-add-module').modal('show');
        });

        $(document).on('click', '#el-add-module-submit', function () {
            var fields = [
                { inputId: 'el-add-module-title', errId: 'el-add-module-title-err' },
                { inputId: 'el-add-module-desc', errId: 'el-add-module-desc-err' },
                { inputId: 'el-add-module-role', errId: 'el-add-module-role-err' },
                { inputId: 'el-add-module-due', errId: 'el-add-module-due-err' }
            ];
            if (!self._validateFields(fields)) return;

            var payload = {
                title: $('#el-add-module-title').val().trim(),
                description: $('#el-add-module-desc').val().trim(),
                role: $('#el-add-module-role').val().trim(),
                dueDate: $('#el-add-module-due').val().trim(),
                batchId: self.selectedBatch
            };

            app.loading && app.loading.show('Adding module...');
            self._api('/api/ELearning/add-module', { method: 'POST', body: JSON.stringify(payload) }).then(function (json) {
                app.loading && app.loading.hide();
                if (!json) { Swal.fire({ icon: 'error', title: 'Failed to add module' }); return; }
                $('#el-modal-add-module').modal('hide');
                self._loadModules();
                Swal.fire({ icon: 'success', title: 'Module Added', text: '"' + payload.title + '" has been added successfully.', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
            });
        });

        $(document).on('click', '#el-add-program-submit', function () {
            var fields = [{ inputId: 'el-add-program-name', errId: 'el-add-program-name-err' }];
            if (!self._validateFields(fields)) return;

            var name = $('#el-add-program-name').val().trim();
            app.loading && app.loading.show('Creating program...');
            self._api('/api/ELearning/create-program', { method: 'POST', body: JSON.stringify({ name: name }) }).then(function (json) {
                app.loading && app.loading.hide();
                if (!json) { Swal.fire({ icon: 'error', title: 'Failed to create program' }); return; }
                var newProgram = json.content || json.data || json;
                if (newProgram && newProgram.id) {
                    self._programs.push(newProgram);
                    self.selectedProgram = newProgram.id;
                }
                $('#el-modal-add-program').modal('hide');
                self.renderProgramDropdown();
                self._loadBatches();
                Swal.fire({ icon: 'success', title: 'Program Added', text: '"' + name + '" has been created successfully.', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
            });
        });

        $(document).on('click', '#el-add-batch-submit', function () {
            var fields = [
                { inputId: 'el-add-batch-start-date', errId: 'el-add-batch-start-err' },
                { inputId: 'el-add-batch-end-date', errId: 'el-add-batch-end-err' }
            ];
            if (!self._validateFields(fields)) return;

            var startVal = $('#el-add-batch-start-date').val().trim();
            var endVal = $('#el-add-batch-end-date').val().trim();
            if (startVal && endVal && new Date(startVal) >= new Date(endVal)) {
                $('#el-add-batch-end-date').addClass('is-invalid');
                $('#el-add-batch-end-err').text('End date must be after start date.').removeClass('d-none');
                return;
            }

            var payload = { programId: self.selectedProgram, startDate: startVal, endDate: endVal };
            app.loading && app.loading.show('Creating batch...');
            self._api('/api/ELearning/create-batch', { method: 'POST', body: JSON.stringify(payload) }).then(function (json) {
                app.loading && app.loading.hide();
                if (!json) { Swal.fire({ icon: 'error', title: 'Failed to create batch' }); return; }
                var newBatch = json.content || json.data || json;
                $('#el-modal-add-batch').modal('hide');
                if (newBatch && newBatch.id) { self.selectedBatch = newBatch.id; }
                self._loadBatches().then(function () { self._loadModules(); });
                Swal.fire({ icon: 'success', title: 'Batch Added', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
            });
        });

        $(document).on('click', '.el-sv-module-card .el-sv-card-title', function () {
            var moduleId = $(this).closest('.el-sv-module-card').data('module-id');
            window.location.href = '/Modules/ELearning/Supervisor/ModuleDetail?id=' + moduleId;
        });

        $(document).on('click', '.el-sv-action-btn', function (e) {
            e.stopPropagation();
            var action = $(this).data('action');
            var id = parseInt($(this).data('id'));

            if (action === 'delete') {
                Swal.fire({
                    title: 'Delete Module?',
                    text: 'Are you sure you want to delete this module?',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Yes, Delete',
                    cancelButtonText: 'Cancel',
                    customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm', cancelButton: 'btn-el-swal-cancel' },
                    buttonsStyling: false
                }).then(function (result) {
                    if (!result.isConfirmed) return;
                    app.loading && app.loading.show('Deleting...');
                    self._api('/api/ELearning/delete-module', { method: 'DELETE', body: JSON.stringify({ moduleId: id }) }).then(function (json) {
                        app.loading && app.loading.hide();
                        if (!json && json !== '') { Swal.fire({ icon: 'error', title: 'Failed to delete module' }); return; }
                        self._loadModules();
                        Swal.fire({ icon: 'success', title: 'Module Deleted', timer: 1200, showConfirmButton: false, customClass: { popup: 'el-swal' } });
                    });
                });

            } else if (action === 'copy') {
                $('#el-modal-copy-module').data('copy-id', id).modal('show');

            } else if (action === 'edit') {
                // Bug #1 fix: search by moduleId (not id)
                var mod = self._modules.find(function (m) { return (m.moduleId || m.id) === id; });
                ['title', 'desc', 'due'].forEach(function (f) {
                    $('#el-update-module-' + f + '-err').addClass('d-none');
                    $('#el-update-module-' + f).removeClass('is-invalid');
                });
                $('#el-update-module-role-err').addClass('d-none');
                $('#el-update-module-role-dropdown').removeClass('is-invalid');
                $('#el-update-module-id').val(id);
                self._populateRoleDropdowns(function () {
                    if (mod) {
                        $('#el-update-module-title').val(mod.title || '');
                        $('#el-update-module-desc').val(mod.description || '');
                        var roleVal = mod.role || '';
                        $('#el-update-module-role').val(roleVal);
                        var activeItem = $('#el-update-module-role-menu .el-sv-dropdown-item[data-value="' + roleVal + '"]');
                        if (activeItem.length) {
                            $('#el-update-module-role-text').text(activeItem.text());
                            $('#el-update-module-role-menu .el-sv-dropdown-item').removeClass('active');
                            activeItem.addClass('active');
                        } else {
                            $('#el-update-module-role-text').text('-- Select Role --');
                        }
                        $('#el-update-module-due').val(mod.dueDateISO || mod.dueDate || '');
                    }
                    $('#el-modal-update-module').modal('show');
                });
            }
        });

        $(document).on('click', '#el-update-module-submit', function () {
            var fields = [
                { inputId: 'el-update-module-title', errId: 'el-update-module-title-err' },
                { inputId: 'el-update-module-desc', errId: 'el-update-module-desc-err' },
                { inputId: 'el-update-module-role', errId: 'el-update-module-role-err' },
                { inputId: 'el-update-module-due', errId: 'el-update-module-due-err' }
            ];
            if (!self._validateFields(fields)) return;

            var payload = {
                moduleId: parseInt($('#el-update-module-id').val()),
                title: $('#el-update-module-title').val().trim(),
                description: $('#el-update-module-desc').val().trim(),
                role: $('#el-update-module-role').val().trim(),
                dueDate: $('#el-update-module-due').val().trim()
            };

            app.loading && app.loading.show('Updating module...');
            self._api('/api/ELearning/update-module', { method: 'PUT', body: JSON.stringify(payload) }).then(function (json) {
                app.loading && app.loading.hide();
                if (!json) { Swal.fire({ icon: 'error', title: 'Failed to update module' }); return; }
                $('#el-modal-update-module').modal('hide');
                self._loadModules();
                Swal.fire({ icon: 'success', title: 'Module Updated', text: '"' + payload.title + '" has been updated successfully.', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
            });
        });

        $(document).on('click', '#el-copy-module-submit', function () {
            var copyId = parseInt($('#el-modal-copy-module').data('copy-id'));
            var targetBatchId = parseInt($('#el-copy-batch-val').val());

            if (!targetBatchId || isNaN(targetBatchId)) {
                Swal.fire({ icon: 'warning', title: 'Validation Error', text: 'Please select a batch.', customClass: { popup: 'el-swal' } });
                return;
            }

            app.loading && app.loading.show('Copying module...');
            self._api('/api/ELearning/copy-module', { method: 'POST', body: JSON.stringify({ sourceModuleId: copyId, targetBatchId: targetBatchId }) }).then(function (json) {
                app.loading && app.loading.hide();
                if (!json) { Swal.fire({ icon: 'error', title: 'Failed to copy module' }); return; }
                $('#el-modal-copy-module').modal('hide');
                self._loadModules();
                Swal.fire({ icon: 'success', title: 'Module Copied', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
            });
        });

        $(document).on('show.bs.modal', '#el-modal-copy-module', function () {
            var programMenu = $('#el-copy-program-menu');
            var batchMenu = $('#el-copy-batch-menu');
            programMenu.empty();
            batchMenu.empty();
            $('#el-copy-program-text').text('Select Program');
            $('#el-copy-batch-text').text('Select Batch');
            $('#el-copy-program-val, #el-copy-batch-val').val('');
            self._programs.forEach(function (p) {
                programMenu.append('<div class="el-sv-dropdown-item" data-value="' + p.programId + '">' + (p.programName || '') + '</div>');
            });
        });

        $(document).on('click', '#el-copy-program-menu .el-sv-dropdown-item', function () {
            var value = $(this).data('value');
            $('#el-copy-program-val').val(value);
            $('#el-copy-program-text').text($(this).text());
            $('#el-copy-program-menu .el-sv-dropdown-item').removeClass('active');
            $(this).addClass('active');
            $('#el-copy-program-menu').removeClass('show');

            $('#el-copy-batch-text').text('Loading...');
            $('#el-copy-batch-val').val('');
            self._api('/api/ELearning/programs/' + value + '/batches').then(function (json) {
                $('#el-copy-batch-text').text('Select Batch');
                var batches = (json && (json.content || json.data || json)) || [];
                if (!Array.isArray(batches)) batches = [];
                var batchMenu = $('#el-copy-batch-menu');
                batchMenu.empty();
                batches.forEach(function (b) {
                    batchMenu.append('<div class="el-sv-dropdown-item" data-value="' + b.batchId + '">' + (b.batchName || '') + '</div>');
                });
            });
        });

        $(document).on('click', '#el-copy-batch-menu .el-sv-dropdown-item', function () {
            var value = $(this).data('value');
            $('#el-copy-batch-val').val(value);
            $('#el-copy-batch-text').text($(this).text());
            $('#el-copy-batch-menu .el-sv-dropdown-item').removeClass('active');
            $(this).addClass('active');
            $('#el-copy-batch-menu').removeClass('show');
        });
    };

    app.elearning.supervisor.modules._buildFilterModal = function () {
        var self = this;
        var container = $('#el-filter-roles-container');
        container.empty();
        // Only show real roles (exclude 'all' — modules with role 'all' always appear in every filter)
        var displayRoles = self._allRoles.filter(function (r) { return r.toLowerCase() !== 'all'; });
        displayRoles.forEach(function (role) {
            var isSelected = self.selectedRoles.length === 0 || self.selectedRoles.indexOf(role.toLowerCase()) > -1;
            container.append(
                '<div class="el-filter-role-box' + (isSelected ? ' selected' : '') + '" data-role="' + role + '">' +
                '<span class="el-filter-role-check"><i class="ki-duotone ki-check fs-7"><span class="path1"></span><span class="path2"></span></i></span>' +
                role +
                '</div>'
            );
        });
    };

    app.elearning.supervisor.modules._populateRoleDropdowns = function (callback) {
        var self = this;
        if (!self.selectedProgram) { if (callback) callback(); return; }
        self._api('/api/ELearning/programs/' + self.selectedProgram + '/positions').then(function (json) {
            var positions = (json && (json.content || json.data || json)) || [];
            if (!Array.isArray(positions)) positions = [];
            var options = '<div class="el-sv-dropdown-item" data-value="">-- Select Role --</div><div class="el-sv-dropdown-item" data-value="All">All Roles</div>';
            positions.forEach(function (p) {
                options += '<div class="el-sv-dropdown-item" data-value="' + p + '">' + p + '</div>';
            });
            $('#el-add-module-role-menu, #el-update-module-role-menu').html(options);
            if (callback) callback();
        });
    };

})(jQuery, window.app = window.app || {});


