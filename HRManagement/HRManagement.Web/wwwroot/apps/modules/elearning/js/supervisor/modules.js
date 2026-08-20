/**
 * supervisor/modules.js â€” Component-Based Architecture
 *
 * ProgramSelectorComponent  â€” loads & renders program dropdown independently
 * BatchSelectorComponent    â€” loads & renders batch dropdown (only reloads when program changes)
 * ModulesGridComponent      â€” loads & renders modules grid (only reloads when batch/search/filter changes)
 */
(function ($, app) {
    'use strict';
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    /* â”€â”€ Shared API shorthand (uses elearning-api.js shared util) â”€â”€ */
    var _api = function (path, opts) { return app.elearning.api(path, opts); };
    var _uw  = function (json, fb)   { return app.elearning.unwrap(json, fb); };

    /* â”€â”€ Page-level shared state â”€â”€ */
    var state = {
        selectedProgram : null,
        selectedBatch   : null,
        selectedRoles   : [],
        visibleCount    : 8,
        programs        : [],
        batches         : [],
        allRoles        : []
    };

    /* â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
       COMPONENT: ProgramSelector
       Fetches and renders the program dropdown.
       Re-runs only on page init or after add-program.
    â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• */
    var ProgramSelector = {
        load: function () {
            return _api('/api/ELearning/programs').then(function (json) {
                state.programs = _uw(json, []);
                if (!Array.isArray(state.programs)) state.programs = [];
                if (state.programs.length && !state.selectedProgram) {
                    state.selectedProgram = state.programs[0].programId;
                }
                ProgramSelector.render();
            });
        },
        render: function () {
            var menu = $('#el-sv-program-menu');
            menu.find('.el-sv-dropdown-item:not(.el-sv-dropdown-add)').remove();
            state.programs.forEach(function (p) {
                menu.append('<div class="el-sv-dropdown-item' + (p.programId === state.selectedProgram ? ' active' : '') + '" data-value="' + p.programId + '">' + (p.programName || '') + '</div>');
            });
            var sel = state.programs.find(function (p) { return p.programId === state.selectedProgram; });
            if (sel) { $('#el-sv-program-text').text(sel.programName || ''); }
        }
    };

    /* â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
       COMPONENT: BatchSelector
       Fetches and renders the batch dropdown.
       Re-runs only when selectedProgram changes.
    â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• */
    var BatchSelector = {
        load: function () {
            if (!state.selectedProgram) {
                state.batches = []; state.selectedBatch = null;
                BatchSelector.render(); return Promise.resolve();
            }
            return _api('/api/ELearning/programs/' + state.selectedProgram + '/batches').then(function (json) {
                state.batches = _uw(json, []);
                if (!Array.isArray(state.batches)) state.batches = [];
                if (state.batches.length && !state.selectedBatch) {
                    state.selectedBatch = state.batches[0].batchId;
                }
                BatchSelector.render();
            });
        },
        render: function () {
            var menu = $('#el-sv-batch-menu');
            menu.find('.el-sv-dropdown-item:not(.el-sv-dropdown-add)').remove();
            state.batches.forEach(function (b) {
                menu.append('<div class="el-sv-dropdown-item' + (b.batchId === state.selectedBatch ? ' active' : '') + '" data-value="' + b.batchId + '">' + (b.batchName || '') + '</div>');
            });
            var sel = state.batches.find(function (b) { return b.batchId === state.selectedBatch; });
            if (sel) {
                $('#el-sv-batch-text').text(sel.batchName || '');
                var period = '';
                if (sel.startDate && sel.endDate) {
                    var s = new Date(sel.startDate), e = new Date(sel.endDate);
                    period = s.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) + ' - ' + e.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
                }
                $('#el-sv-period').text(period);
            } else {
                $('#el-sv-batch-text').text('Batch 1'); $('#el-sv-period').text('');
            }
        }
    };

    /* â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
       COMPONENT: ModulesGrid
       Fetches and renders the module card grid.
       Re-runs only when batch, search, or role filter changes.
    â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• */
    var ModulesGrid = {
        _data: [],
        load: function () {
            if (!state.selectedBatch) {
                ModulesGrid._data = []; ModulesGrid.render(); return Promise.resolve();
            }
            var search = $('#el-sv-module-search').val() || '';
            var rolesParam = state.selectedRoles.map(function (r) { return 'roles=' + encodeURIComponent(r); }).join('&');
            var url = '/api/ELearning/batches/' + state.selectedBatch + '/modules?search=' + encodeURIComponent(search) + (rolesParam ? '&' + rolesParam : '');
            /* Show skeleton only inside grid â€” program/batch dropdowns are not touched */
            app.elearning.showSkeleton('#el-sv-modules-grid', 4);
            $('#el-sv-load-more').hide();
            return _api(url).then(function (json) {
                var modules = _uw(json, []);
                ModulesGrid._data = Array.isArray(modules) ? modules : [];
                ModulesGrid._data.forEach(function (m) {
                    var r = (m.role || '').toLowerCase();
                    if (r && r !== 'all' && state.allRoles.indexOf(m.role) === -1) { state.allRoles.push(m.role); }
                });
                ModulesGrid.render();
            });
        },
        render: function () {
            var searchTerm = ($('#el-sv-module-search').val() || '').toLowerCase();
            var activeRoles = state.selectedRoles;
            var filtered = ModulesGrid._data.filter(function (m) {
                var matchSearch = (m.title || '').toLowerCase().indexOf(searchTerm) > -1;
                var isUniversal = (m.role || '').toLowerCase() === 'all';
                var matchRole = isUniversal || activeRoles.length === 0 || activeRoles.indexOf((m.role || '').toLowerCase()) > -1;
                return matchSearch && matchRole;
            });
            var visible = filtered.slice(0, state.visibleCount);
            var html = '';
            visible.forEach(function (mod, idx) {
                var modId = mod.moduleId || mod.id;
                var displayDate = '-';
                if (mod.dueDate) { displayDate = new Date(mod.dueDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }); }
                html += '<div class="col-md-6 col-lg-3 mb-4">' +
                    '<div class="el-sv-module-card el-animate el-animate-delay-' + ((idx % 4) + 1) + '" data-module-id="' + modId + '">' +
                    '<div><div class="el-sv-card-role">' + (mod.role || '') + '</div>' +
                    '<div class="el-sv-card-title">' + (mod.title || '') + '</div>' +
                    '<div class="el-sv-card-due">Due Date: ' + displayDate + '</div></div>' +
                    '<div class="el-sv-action-icons">' +
                    '<button class="el-sv-action-btn copy" data-action="copy" data-id="' + modId + '" title="Duplicate"><i class="ki-duotone ki-copy fs-5"><span class="path1"></span><span class="path2"></span></i></button>' +
                    '<button class="el-sv-action-btn edit" data-action="edit" data-id="' + modId + '" title="Edit"><i class="ki-duotone ki-notepad-edit fs-5"><span class="path1"></span><span class="path2"></span></i></button>' +
                    '<button class="el-sv-action-btn delete" data-action="delete" data-id="' + modId + '" title="Delete"><i class="ki-duotone ki-trash fs-5"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i></button>' +
                    '</div></div></div>';
            });
            $('#el-sv-modules-grid').html(html);
            if (filtered.length > state.visibleCount) { $('#el-sv-load-more').show(); } else { $('#el-sv-load-more').hide(); }
        }
    };

    /* ═════════════════════════════════════════════════════════════════════════
       PAGE CONTROLLER
    â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• */
    app.elearning.supervisor.modules = {

        init: function () {
            var self = this;
            app.loading && app.loading.show('Loading...');
            /* Sequential init: programs â†’ batches â†’ modules (each depends on prior selection) */
            ProgramSelector.load()
                .then(function () { return BatchSelector.load(); })
                .then(function () { return ModulesGrid.load(); })
                .then(function () {
                    app.loading && app.loading.hide();
                    self._buildFilterModal();
                    self._bindEvents();
                });
        },

        _isBatchEditable: function(startDateStr) {
            if (!startDateStr) return false;
            var start = new Date(startDateStr);
            start.setHours(0,0,0,0);
            var now = new Date();
            now.setHours(0,0,0,0);
            var diffDays = (start.getTime() - now.getTime()) / (1000 * 3600 * 24);
            return diffDays >= 7;
        },

        _validateFields: function (fields) {
            var valid = true;
            fields.forEach(function (f) {
                var val = $('#' + f.inputId).val().trim();
                if (!val) { $('#' + f.errId).removeClass('d-none'); $('#' + f.inputId).addClass('is-invalid'); valid = false; }
                else { $('#' + f.errId).addClass('d-none'); $('#' + f.inputId).removeClass('is-invalid'); }
            });
            return valid;
        },

        _bindLiveClear: function (fields) {
            fields.forEach(function (f) {
                $(document).on('input change', '#' + f.inputId, function () {
                    if ($(this).val().trim()) { $(this).removeClass('is-invalid'); $('#' + f.errId).addClass('d-none'); }
                });
            });
        },

        _buildFilterModal: function () {
            var container = $('#el-filter-roles-container');
            container.empty();
            var displayRoles = state.allRoles.filter(function (r) { return r.toLowerCase() !== 'all'; });
            displayRoles.forEach(function (role) {
                var isSelected = state.selectedRoles.length === 0 || state.selectedRoles.indexOf(role.toLowerCase()) > -1;
                container.append('<div class="el-filter-role-box' + (isSelected ? ' selected' : '') + '" data-role="' + role + '"><span class="el-filter-role-check"><i class="ki-duotone ki-check fs-7"><span class="path1"></span><span class="path2"></span></i></span>' + role + '</div>');
            });
        },

        _populateRoleDropdowns: function (callback) {
            if (!state.selectedProgram) { if (callback) callback(); return; }
            _api('/api/ELearning/programs/' + state.selectedProgram + '/positions').then(function (json) {
                var positions = _uw(json, []);
                if (!Array.isArray(positions)) positions = [];
                var options = '<div class="el-sv-dropdown-item" data-value="">-- Select Role --</div><div class="el-sv-dropdown-item" data-value="All">All Roles</div>';
                positions.forEach(function (p) { options += '<div class="el-sv-dropdown-item" data-value="' + p + '">' + p + '</div>'; });
                $('#el-add-module-role-menu, #el-update-module-role-menu').html(options);
                if (callback) callback();
            });
        },

        _bindEvents: function () {
            var self = this;

            self._bindLiveClear([
                { inputId: 'el-add-module-title',    errId: 'el-add-module-title-err' },
                { inputId: 'el-add-module-desc',     errId: 'el-add-module-desc-err' },
                { inputId: 'el-add-module-due',      errId: 'el-add-module-due-err' },
                { inputId: 'el-update-module-title', errId: 'el-update-module-title-err' },
                { inputId: 'el-update-module-desc',  errId: 'el-update-module-desc-err' },
                { inputId: 'el-update-module-due',   errId: 'el-update-module-due-err' },
                { inputId: 'el-add-program-name',    errId: 'el-add-program-name-err' },
                { inputId: 'el-add-batch-start-date', errId: 'el-add-batch-start-err' },
                { inputId: 'el-add-batch-end-date',  errId: 'el-add-batch-end-err' }
            ]);
            $(document).on('change', '#el-add-module-role', function () {
                if ($(this).val()) { $('#el-add-module-role-dropdown').removeClass('is-invalid'); $('#el-add-module-role-err').addClass('d-none'); }
            });
            $(document).on('change', '#el-update-module-role', function () {
                if ($(this).val()) { $('#el-update-module-role-dropdown').removeClass('is-invalid'); $('#el-update-module-role-err').addClass('d-none'); }
            });

            /* Generic dropdown toggle */
            $(document).on('click', '.el-sv-dropdown-toggle', function (e) {
                e.stopPropagation();
                var menu = $(this).siblings('.el-sv-dropdown-menu');
                $('.el-sv-dropdown-menu').not(menu).removeClass('show');
                menu.toggleClass('show');
            });
            $(document).on('click', function () { $('.el-sv-dropdown-menu').removeClass('show'); });
            $(document).on('click', '.el-sv-dropdown-menu', function (e) { e.stopPropagation(); });

            $(document).on('click', '#el-add-module-role-menu .el-sv-dropdown-item', function () {
                var value = $(this).data('value');
                $('#el-add-module-role').val(value).trigger('change'); $('#el-add-module-role-text').text($(this).text());
                $('#el-add-module-role-menu .el-sv-dropdown-item').removeClass('active');
                $(this).addClass('active'); $('#el-add-module-role-menu').removeClass('show');
            });
            $(document).on('click', '#el-update-module-role-menu .el-sv-dropdown-item', function () {
                var value = $(this).data('value');
                $('#el-update-module-role').val(value).trigger('change'); $('#el-update-module-role-text').text($(this).text());
                $('#el-update-module-role-menu .el-sv-dropdown-item').removeClass('active');
                $(this).addClass('active'); $('#el-update-module-role-menu').removeClass('show');
            });

            /* â”€â”€ Program changed â†’ ONLY reload BatchSelector + ModulesGrid â”€â”€ */
            $(document).on('click', '#el-sv-program-menu .el-sv-dropdown-item:not(.el-sv-dropdown-add)', function () {
                state.selectedProgram = parseInt($(this).data('value'));
                state.selectedBatch = null;
                $('#el-sv-program-text').text($(this).text());
                $('#el-sv-program-menu .el-sv-dropdown-item').removeClass('active');
                $(this).addClass('active'); $('#el-sv-program-menu').removeClass('show');
                /* ProgramSelector is already current; only Batch + Grid re-fetch */
                BatchSelector.load().then(function () {
                    state.visibleCount = 8; return ModulesGrid.load();
                });
            });

            /* â”€â”€ Batch changed â†’ ONLY reload ModulesGrid â”€â”€ */
            $(document).on('click', '#el-sv-batch-menu .el-sv-dropdown-item:not(.el-sv-dropdown-add)', function () {
                state.selectedBatch = parseInt($(this).data('value'));
                $('#el-sv-batch-text').text($(this).text());
                var batch = state.batches.find(function (b) { return b.batchId === state.selectedBatch; });
                if (batch) {
                    var period = '';
                    if (batch.startDate && batch.endDate) {
                        var s = new Date(batch.startDate), e = new Date(batch.endDate);
                        period = s.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) + ' - ' + e.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
                    }
                    $('#el-sv-period').text(period);
                }
                $('#el-sv-batch-menu .el-sv-dropdown-item').removeClass('active');
                $(this).addClass('active'); $('#el-sv-batch-menu').removeClass('show');
                state.visibleCount = 8;
                /* Only ModulesGrid re-fetches; ProgramSelector and BatchSelector stay */
                ModulesGrid.load();
            });

            /* â”€â”€ Add Program / Batch dropdown trigger â”€â”€ */
            $(document).on('click', '.el-sv-dropdown-add', function () {
                var action = $(this).data('action');
                $('.el-sv-dropdown-menu').removeClass('show');
                if (action === 'add-program') {
                    $('#el-add-program-name').val('').removeClass('is-invalid');
                    $('#el-add-program-name-err, #el-add-program-groups-err').addClass('d-none');
                    $('#el-add-program-groups-container').html('<div class="text-muted fs-7">Loading groups...</div>');
                    _api('/api/ELearning/groups').then(function (json) {
                        var groups = _uw(json, []); if (!Array.isArray(groups)) groups = [];
                        var html = groups.length === 0 ? '<div class="text-muted fs-7">No groups available.</div>' :
                            groups.map(function (g) { return '<input type="text" class="el-form-control el-group-select-item" value="' + (g.groupName || '') + '" data-id="' + g.groupId + '" readonly style="cursor:pointer;background-color:#fff;" />'; }).join('');
                        $('#el-add-program-groups-container').html(html);
                    });
                    $('#el-modal-add-program').modal('show');
                } else if (action === 'add-batch') {
                    $('#el-add-batch-start-date, #el-add-batch-end-date').val('').removeClass('is-invalid');
                    $('#el-add-batch-start-err, #el-add-batch-end-err').addClass('d-none');
                    $('#el-modal-add-batch').modal('show');
                }
            });

            /* Group item toggle */
            $(document).on('click', '.el-group-select-item', function () {
                $(this).toggleClass('selected');
                if ($(this).hasClass('selected')) {
                    $(this).css({ 'border-color': '#d31145', 'color': '#d31145' });
                    $('#el-add-program-groups-err').addClass('d-none');
                } else {
                    $(this).css({ 'border-color': '#e1e3ea', 'color': '#4b5675' }).removeClass('is-invalid');
                }
            });

            /* â”€â”€ Search â†’ ONLY ModulesGrid re-fetches â”€â”€ */
            $(document).on('input', '#el-sv-module-search', function () {
                state.visibleCount = 8; ModulesGrid.load();
            });

            /* â”€â”€ Load More â†’ re-render only, no network â”€â”€ */
            $(document).on('click', '#el-sv-load-more', function () {
                state.visibleCount += 4; ModulesGrid.render();
            });

            /* â”€â”€ Filter modal â”€â”€ */
            self._buildFilterModal();
            $(document).on('show.bs.modal', '#el-modal-filter-list', function () { self._buildFilterModal(); });
            $(document).on('click', '.el-filter-role-box', function () { $(this).toggleClass('selected'); });
            $(document).on('click', '#el-filter-select-all', function () {
                var boxes = $('.el-filter-role-box');
                var allSel = boxes.filter('.selected').length === boxes.length;
                if (allSel) { boxes.removeClass('selected'); } else { boxes.addClass('selected'); }
            });
            $(document).on('click', '#el-filter-ok', function () {
                var selected = [];
                $('.el-filter-role-box.selected').each(function () { selected.push($(this).data('role').toLowerCase()); });
                state.selectedRoles = selected; state.visibleCount = 8;
                /* Re-render with current data â€” no network request needed */
                ModulesGrid.render();
            });

            /* â”€â”€ Add Module â”€â”€ */
            $(document).on('click', '#el-sv-add-module', function () {
                var batch = state.batches.find(function (b) { return b.batchId === state.selectedBatch; });
                if (!batch || !self._isBatchEditable(batch.startDate)) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Add Not Allowed',
                        text: 'You can only add a module in a batch up to 7 days before it starts.',
                        customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' },
                        buttonsStyling: false
                    });
                    return;
                }
                $('#el-add-module-title, #el-add-module-desc, #el-add-module-due').val('').removeClass('is-invalid');
                $('#el-add-module-title-err, #el-add-module-desc-err, #el-add-module-role-err').addClass('d-none');
                $('#el-add-module-due-err').text('Field must not be empty.').addClass('d-none');
                $('#el-add-module-role').val('').trigger('change');
                $('#el-add-module-role-text').text('-- Select Role --');
                $('#el-add-module-role-dropdown').removeClass('is-invalid');
                self._populateRoleDropdowns();
                $('#el-modal-add-module').modal('show');
            });

            $(document).on('click', '#el-add-module-submit', function () {
                $('#el-add-module-due-err').text('Field must not be empty.');
                var fields = [
                    { inputId: 'el-add-module-title', errId: 'el-add-module-title-err' },
                    { inputId: 'el-add-module-desc',  errId: 'el-add-module-desc-err' },
                    { inputId: 'el-add-module-role',  errId: 'el-add-module-role-err' },
                    { inputId: 'el-add-module-due',   errId: 'el-add-module-due-err' }
                ];
                if (!self._validateFields(fields)) return;

                var batch = state.batches.find(function (b) { return b.batchId === state.selectedBatch; });
                if (!batch || !self._isBatchEditable(batch.startDate)) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Add Not Allowed',
                        text: 'You can only add a module in a batch up to 7 days before it starts.',
                        customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' },
                        buttonsStyling: false
                    });
                    return;
                }

                var dueVal = $('#el-add-module-due').val().trim();
                if (batch.startDate && batch.endDate && dueVal) {
                    var due = new Date(dueVal);
                    var start = new Date(batch.startDate);
                    var end = new Date(batch.endDate);
                    due.setHours(0, 0, 0, 0);
                    start.setHours(0, 0, 0, 0);
                    end.setHours(0, 0, 0, 0);
                    if (due < start || due > end) {
                        var startLabel = start.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
                        var endLabel = end.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
                        $('#el-add-module-due').addClass('is-invalid');
                        $('#el-add-module-due-err').text('Due date must be within the batch period (' + startLabel + ' - ' + endLabel + ').').removeClass('d-none');
                        return;
                    }
                }

                var payload = { title: $('#el-add-module-title').val().trim(), description: $('#el-add-module-desc').val().trim(), role: $('#el-add-module-role').val().trim(), dueDate: dueVal, batchId: state.selectedBatch };
                app.loading && app.loading.show('Adding module...');
                _api('/api/ELearning/add-module', { method: 'POST', body: JSON.stringify(payload) }).then(function (json) {
                    app.loading && app.loading.hide();
                    if (!json) { Swal.fire({ icon: 'error', title: 'Failed to add module', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false }); return; }
                    $('#el-modal-add-module').modal('hide');
                    ModulesGrid.load(); /* Only grid re-fetches */
                    Swal.fire({ icon: 'success', title: 'Module Added', text: '"' + payload.title + '" has been added successfully.', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
                });
            });

            /* â”€â”€ Add Program submit â”€â”€ */
            $(document).on('click', '#el-add-program-submit', function () {
                var fields = [{ inputId: 'el-add-program-name', errId: 'el-add-program-name-err' }];
                var valid = self._validateFields(fields);
                var selectedGroups = [];
                $('.el-group-select-item.selected').each(function () { selectedGroups.push(parseInt($(this).data('id'))); });
                if (selectedGroups.length === 0) { $('#el-add-program-groups-err').removeClass('d-none'); valid = false; }
                if (!valid) return;
                var name = $('#el-add-program-name').val().trim();
                app.loading && app.loading.show('Creating program...');
                _api('/api/ELearning/create-program', { method: 'POST', body: JSON.stringify({ programName: name, groupIds: selectedGroups }) }).then(function (json) {
                    app.loading && app.loading.hide();
                    if (!json) { Swal.fire({ icon: 'error', title: 'Failed to create program', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false }); return; }
                    $('#el-modal-add-program').modal('hide');
                    Swal.fire({ icon: 'success', title: 'Program Added', text: '"' + name + '" has been created successfully.', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false })
                        .then(function () { window.location.reload(); });
                });
            });

            /* â”€â”€ Add Batch submit â”€â”€ */
            $(document).on('click', '#el-add-batch-submit', function () {
                var fields = [
                    { inputId: 'el-add-batch-start-date', errId: 'el-add-batch-start-err' },
                    { inputId: 'el-add-batch-end-date',   errId: 'el-add-batch-end-err' }
                ];
                if (!self._validateFields(fields)) return;
                var startVal = $('#el-add-batch-start-date').val().trim(), endVal = $('#el-add-batch-end-date').val().trim();
                if (startVal && endVal && new Date(startVal) >= new Date(endVal)) {
                    $('#el-add-batch-end-date').addClass('is-invalid');
                    $('#el-add-batch-end-err').text('End date must be after start date.').removeClass('d-none'); return;
                }
                app.loading && app.loading.show('Creating batch...');
                _api('/api/ELearning/create-batch', { method: 'POST', body: JSON.stringify({ programId: state.selectedProgram, startDate: startVal, endDate: endVal }) }).then(function (json) {
                    app.loading && app.loading.hide();
                    if (!json) { Swal.fire({ icon: 'error', title: 'Failed to create batch', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false }); return; }
                    var newBatch = _uw(json, {});
                    $('#el-modal-add-batch').modal('hide');
                    if (newBatch && newBatch.id) { state.selectedBatch = newBatch.id; }
                    /* BatchSelector + ModulesGrid reload; ProgramSelector untouched */
                    BatchSelector.load().then(function () { ModulesGrid.load(); });
                    Swal.fire({ icon: 'success', title: 'Batch Added', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
                });
            });

            /* â”€â”€ Module card title â†’ navigate to detail â”€â”€ */
            $(document).on('click', '.el-sv-module-card .el-sv-card-title', function () {
                var moduleId = $(this).closest('.el-sv-module-card').data('module-id');
                window.location.href = '/Modules/ELearning/Supervisor/ModuleDetail?id=' + moduleId;
            });

            /* â”€â”€ Action buttons (copy / edit / delete) â”€â”€ */
            $(document).on('click', '.el-sv-action-btn', function (e) {
                e.stopPropagation();
                var action = $(this).data('action'), id = parseInt($(this).data('id'));

                if (action === 'delete' || action === 'edit') {
                    var batch = state.batches.find(function (b) { return b.batchId === state.selectedBatch; });
                    if (!batch || !self._isBatchEditable(batch.startDate)) {
                        var title = action === 'delete' ? 'Delete Not Allowed' : 'Edit Not Allowed';
                        var text = action === 'delete' 
                            ? 'You can only delete a module in a batch up to 7 days before it starts.'
                            : 'You can only edit a module in a batch up to 7 days before it starts.';
                        Swal.fire({ icon: 'warning', title: title, text: text, customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
                        return;
                    }
                }

                if (action === 'delete') {
                    Swal.fire({ title: 'Delete Module?', text: 'Are you sure you want to delete this module?', icon: 'warning', showCancelButton: true, confirmButtonText: 'Yes, Delete', cancelButtonText: 'Cancel', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm', cancelButton: 'btn-el-swal-cancel' }, buttonsStyling: false })
                        .then(function (result) {
                            if (!result.isConfirmed) return;
                            app.loading && app.loading.show('Deleting...');
                            _api('/api/ELearning/delete-module', { method: 'DELETE', body: JSON.stringify({ moduleId: id }) }).then(function () {
                                app.loading && app.loading.hide();
                                ModulesGrid.load(); /* Only grid re-fetches */
                                Swal.fire({ icon: 'success', title: 'Module Deleted', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
                            });
                        });

                } else if (action === 'copy') {
                    $('#el-modal-copy-module').data('copy-id', id).modal('show');

                } else if (action === 'edit') {
                    var mod = ModulesGrid._data.find(function (m) { return (m.moduleId || m.id) === id; });
                    ['title', 'desc', 'due'].forEach(function (f) { $('#el-update-module-' + f + '-err').addClass('d-none'); $('#el-update-module-' + f).removeClass('is-invalid'); });
                    $('#el-update-module-role-err').addClass('d-none'); $('#el-update-module-role-dropdown').removeClass('is-invalid');
                    $('#el-update-module-id').val(id);
                    self._populateRoleDropdowns(function () {
                        if (mod) {
                            $('#el-update-module-title').val(mod.title || '');
                            $('#el-update-module-desc').val(mod.description || '');
                            var roleVal = mod.role || '';
                            $('#el-update-module-role').val(roleVal);
                            var activeItem = $('#el-update-module-role-menu .el-sv-dropdown-item[data-value="' + roleVal + '"]');
                            if (activeItem.length) { $('#el-update-module-role-text').text(activeItem.text()); $('#el-update-module-role-menu .el-sv-dropdown-item').removeClass('active'); activeItem.addClass('active'); }
                            else { $('#el-update-module-role-text').text(roleVal || '-- Select Role --'); }
                            var dueVal = mod.dueDateISO || mod.dueDate || '';
                            if (dueVal) { var dDate = new Date(dueVal); if (!isNaN(dDate.getTime())) { dueVal = dDate.getFullYear() + '-' + String(dDate.getMonth() + 1).padStart(2, '0') + '-' + String(dDate.getDate()).padStart(2, '0'); } }
                            $('#el-update-module-due').val(dueVal);
                        }
                        $('#el-modal-update-module').modal('show');
                    });
                }
            });

            /* â”€â”€ Update Module submit â”€â”€ */
            $(document).on('click', '#el-update-module-submit', function () {
                var fields = [
                    { inputId: 'el-update-module-title', errId: 'el-update-module-title-err' },
                    { inputId: 'el-update-module-desc',  errId: 'el-update-module-desc-err' },
                    { inputId: 'el-update-module-role',  errId: 'el-update-module-role-err' },
                    { inputId: 'el-update-module-due',   errId: 'el-update-module-due-err' }
                ];
                if (!self._validateFields(fields)) return;
                var payload = { moduleId: parseInt($('#el-update-module-id').val()), title: $('#el-update-module-title').val().trim(), description: $('#el-update-module-desc').val().trim(), role: $('#el-update-module-role').val().trim(), dueDate: $('#el-update-module-due').val().trim() };
                app.loading && app.loading.show('Updating module...');
                _api('/api/ELearning/update-module', { method: 'PUT', body: JSON.stringify(payload) }).then(function (json) {
                    app.loading && app.loading.hide();
                    if (!json) { Swal.fire({ icon: 'error', title: 'Failed to update module', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false }); return; }
                    $('#el-modal-update-module').modal('hide');
                    ModulesGrid.load(); /* Only grid re-fetches */
                    Swal.fire({ icon: 'success', title: 'Module Updated', text: '"' + payload.title + '" has been updated successfully.', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
                });
            });

            /* â”€â”€ Copy Module modal â”€â”€ */
            $(document).on('show.bs.modal', '#el-modal-copy-module', function () {
                var programMenu = $('#el-copy-program-menu'), batchMenu = $('#el-copy-batch-menu');
                programMenu.empty(); batchMenu.empty();
                $('#el-copy-program-text').text('Select Program'); $('#el-copy-batch-text').text('Select Batch');
                $('#el-copy-program-val, #el-copy-batch-val').val('');
                state.programs.forEach(function (p) { programMenu.append('<div class="el-sv-dropdown-item" data-value="' + p.programId + '">' + (p.programName || '') + '</div>'); });
            });
            $(document).on('click', '#el-copy-program-menu .el-sv-dropdown-item', function () {
                var value = $(this).data('value');
                $('#el-copy-program-val').val(value); $('#el-copy-program-text').text($(this).text());
                $('#el-copy-program-menu .el-sv-dropdown-item').removeClass('active'); $(this).addClass('active'); $('#el-copy-program-menu').removeClass('show');
                $('#el-copy-batch-text').text('Loading...'); $('#el-copy-batch-val').val('');
                _api('/api/ELearning/programs/' + value + '/batches').then(function (json) {
                    $('#el-copy-batch-text').text('Select Batch');
                    var batches = _uw(json, []); if (!Array.isArray(batches)) batches = [];
                    $('#el-modal-copy-module').data('target-batches', batches);
                    var batchMenu = $('#el-copy-batch-menu'); batchMenu.empty();
                    batches.forEach(function (b) { batchMenu.append('<div class="el-sv-dropdown-item" data-value="' + b.batchId + '">' + (b.batchName || '') + '</div>'); });
                });
            });
            $(document).on('click', '#el-copy-batch-menu .el-sv-dropdown-item', function () {
                var value = $(this).data('value');
                $('#el-copy-batch-val').val(value); $('#el-copy-batch-text').text($(this).text());
                $('#el-copy-batch-menu .el-sv-dropdown-item').removeClass('active'); $(this).addClass('active'); $('#el-copy-batch-menu').removeClass('show');
            });
            $(document).on('click', '#el-copy-module-submit', function () {
                var copyId = parseInt($('#el-modal-copy-module').data('copy-id'));
                var targetBatchId = parseInt($('#el-copy-batch-val').val());
                if (!targetBatchId || isNaN(targetBatchId)) { Swal.fire({ icon: 'warning', title: 'Validation Error', text: 'Please select a batch.', customClass: { popup: 'el-swal' } }); return; }
                
                var targetBatches = $('#el-modal-copy-module').data('target-batches') || [];
                var targetBatch = targetBatches.find(function(b) { return b.batchId === targetBatchId; });
                if (!targetBatch || !self._isBatchEditable(targetBatch.startDate)) {
                    Swal.fire({ 
                        icon: 'warning', 
                        title: 'Copy Not Allowed', 
                        text: 'You can only copy a module to a batch up to 7 days before the destination batch starts.', 
                        customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, 
                        buttonsStyling: false 
                    });
                    return;
                }

                app.loading && app.loading.show('Copying module...');
                _api('/api/ELearning/copy-module', { method: 'POST', body: JSON.stringify({ sourceModuleId: copyId, targetBatchId: targetBatchId }) }).then(function (json) {
                    app.loading && app.loading.hide();
                    if (!json) { Swal.fire({ icon: 'error', title: 'Failed to copy module', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false }); return; }
                    $('#el-modal-copy-module').modal('hide');
                    ModulesGrid.load(); /* Only grid re-fetches */
                    Swal.fire({ icon: 'success', title: 'Module Copied', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
                });
            });
        }
    };

})(jQuery, window.app = window.app || {});
