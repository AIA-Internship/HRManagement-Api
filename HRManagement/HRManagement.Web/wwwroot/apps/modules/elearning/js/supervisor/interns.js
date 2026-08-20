/**
 * supervisor/interns.js â€” Component-Based Architecture
 *
 * ProgramFilterComponent â€” loads & renders program dropdown independently
 * InternsTableComponent  â€” loads & renders interns table; re-fetches only
 *                          when program or role filter changes
 */
(function ($, app) {
    'use strict';
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.supervisor.interns = app.elearning.supervisor.interns || {};

    /* â”€â”€ Shared API shorthand â”€â”€ */
    var _api = function (path, opts) { return app.elearning.api(path, opts); };
    var _uw  = function (json, fb)   { return app.elearning.unwrap(json, fb); };

    /* â”€â”€ Page state â”€â”€ */
    var state = {
        selectedProgram : 0,
        selectedRole    : ''
    };

    /* â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
       COMPONENT: ProgramFilter
       Fetches and renders the program dropdown.
       Re-runs only on page init.
    â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• */
    var ProgramFilter = {
        load: function () {
            return _api('/api/ELearning/programs').then(function (json) {
                var programs = _uw(json, []);
                programs = Array.isArray(programs) ? programs : [];
                var menu = $('#el-sv-intern-program-menu');
                menu.find('.el-sv-dropdown-item:not([data-value=""])').remove();
                programs.forEach(function (p) {
                    var name = p.programName || p.name || ('Program ' + p.programId);
                    menu.append('<div class="el-sv-dropdown-item" data-value="' + p.programId + '">' + name + '</div>');
                });
            });
        }
    };

    /* â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
       COMPONENT: InternsTable
       Fetches interns based on current filters.
       Re-runs only when program or role filter changes.
    â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• */
    var InternsTable = {
        _dt: null,
        _data: [],

        load: function (programId, search, role) {
            programId = programId !== undefined ? programId : state.selectedProgram;
            search    = search    !== undefined ? search    : '';
            role      = role      !== undefined ? role      : state.selectedRole;
            var url = '/api/ELearning/programs/' + (programId || 0) + '/interns?pageNumber=1&pageSize=100';
            if (search) url += '&search=' + encodeURIComponent(search);
            return _api(url).then(function (json) {
                var content = _uw(json, []);
                if (content && content.interns) content = content.interns;
                InternsTable._data = Array.isArray(content) ? content : [];
                if (InternsTable._dt) {
                    InternsTable._dt.clear();
                    InternsTable._dt.rows.add(InternsTable._data);
                    InternsTable._dt.draw();
                }
                InternsTable._populateRoles();
            });
        },

        _populateRoles: function () {
            var menu = $('#el-sv-intern-role-menu');
            menu.find('.el-sv-dropdown-item:not([data-value=""])').remove();
            var roles = [];
            InternsTable._data.forEach(function (d) {
                var r = d.Role || d.role || d.internRole || '';
                if (r && roles.indexOf(r) === -1) roles.push(r);
            });
            roles.sort();
            roles.forEach(function (r) { menu.append('<div class="el-sv-dropdown-item" data-value="' + r + '">' + r + '</div>'); });
        },

        build: function () {
            var self = InternsTable;
            self._dt = $('#el-sv-interns-table').DataTable({
                data: [],
                pageLength: 10,
                lengthChange: false,
                responsive: true,
                order: [[4, 'desc']],
                language: {
                    search: '', searchPlaceholder: 'Search by ID or name...',
                    paginate: { previous: 'Previous', next: 'Next' },
                    info: 'Showing _START_ to _END_ of _TOTAL_ interns',
                    infoEmpty: 'No interns found',
                    zeroRecords: 'No interns match the current filter'
                },
                dom: '<"el-dt-top d-flex align-items-center gap-3 mb-4"f>rt<"el-dt-bottom d-flex justify-content-between align-items-center mt-4 flex-wrap gap-3"ip>',
                columns: [
                    { data: null, className: 'text-start el-truncate-cell', width: '15%', render: function (data, type) { var empId = data.EmployeeId || data.employeeId || data.id || ''; if (type === 'filter' || type === 'sort') return String(empId); var s = $('<div>').text(empId).html(); return '<span class="fw-bold text-gray-800" title="' + s + '">' + s + '</span>'; } },
                    { data: null, className: 'el-truncate-cell', width: '25%', render: function (data, type) { var name = data.Name || data.name || data.fullName || ''; if (type === 'filter' || type === 'sort') return String(name); var s = $('<div>').text(name).html(); return '<span class="fw-bold text-gray-800" title="' + s + '">' + s + '</span>'; } },
                    { data: null, className: 'el-truncate-cell', width: '20%', render: function (data, type) { var role = data.Role || data.role || data.internRole || ''; if (type === 'filter' || type === 'sort') return String(role); var s = $('<div>').text(role).html(); return '<span class="fw-bold text-gray-800" title="' + s + '">' + s + '</span>'; } },
                    { data: null, orderable: true, className: 'el-truncate-cell', width: '20%', render: function (data, type) { var text = data.TotalModulesCompletedText || data.totalModulesCompletedText || data.modulesCompleted || '0 / 0'; if (type === 'filter' || type === 'sort' || type === 'type') return String(text); var s = $('<div>').text(text).html(); return '<div class="el-sv-modules-completed" title="' + s + '"><i class="ki-duotone ki-document fs-5"><span class="path1"></span><span class="path2"></span></i> ' + s + '</div>'; } },
                    { data: null, className: 'text-end el-truncate-cell', width: '20%', render: function (data, type) { var scoreText = data.AccumulativeScoreDisplay || data.accumulativeScoreDisplay || data.score || '0'; if (type === 'filter' || type === 'sort' || type === 'type') return String(scoreText); var s = $('<div>').text(scoreText).html(); return '<span class="fw-bold" title="' + s + '">' + s + '</span>'; } }
                ],
                drawCallback: function () { $('#el-sv-interns-table tbody tr').addClass('el-dt-row-animate row-clickable'); }
            });

            $('#el-sv-interns-table_filter input').addClass('el-search-input');
            $('#el-sv-interns-table_filter label').contents().filter(function () { return this.nodeType === 3; }).remove();

            /* Custom search filter for role column */
            $.fn.dataTable.ext.search.push(function (settings, data) {
                if (settings.nTable.id !== 'el-sv-interns-table') return true;
                var roleFilter = ($('#el-sv-intern-role').val() || '').toLowerCase();
                var roleCell = $('<div>').html(data[2]).text().toLowerCase();
                if (roleFilter && roleCell.indexOf(roleFilter) === -1) return false;
                return true;
            });
        }
    };

    /* â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
       PAGE CONTROLLER
    â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• */
    app.elearning.supervisor.interns = {

        init: function () {
            /* Build the DataTable shell first (empty) */
            InternsTable.build();

            app.loading && app.loading.show('Loading interns...');

            /* Programs and interns load independently in parallel */
            Promise.all([
                ProgramFilter.load(),
                InternsTable.load(0, '', '')
            ]).then(function () {
                app.loading && app.loading.hide();
                this._bindEvents();
            }.bind(this)).catch(function (err) {
                app.loading && app.loading.hide();
                console.error('Failed to load interns page:', err);
            });
        },

        _bindEvents: function () {
            $(document).on('click', '.el-sv-dropdown-toggle', function (e) {
                e.stopPropagation();
                var menu = $(this).siblings('.el-sv-dropdown-menu');
                $('.el-sv-dropdown-menu').not(menu).removeClass('show');
                menu.toggleClass('show');
            });
            $(document).on('click', function () { $('.el-sv-dropdown-menu').removeClass('show'); });
            $(document).on('click', '.el-sv-dropdown-menu', function (e) { e.stopPropagation(); });

            /* Role filter â†’ only InternsTable re-draws (no re-fetch, uses DataTable custom search) */
            $(document).on('click', '#el-sv-intern-role-menu .el-sv-dropdown-item', function () {
                var value = $(this).data('value');
                state.selectedRole = value;
                $('#el-sv-intern-role').val(value).trigger('change');
                $('#el-sv-intern-role-text').text($(this).text());
                $('#el-sv-intern-role-menu .el-sv-dropdown-item').removeClass('active');
                $(this).addClass('active');
                $('#el-sv-intern-role-menu').removeClass('show');
            });

            /* Role hidden input change â†’ DataTable redraw only */
            $(document).on('change', '#el-sv-intern-role', function () {
                if (InternsTable._dt) InternsTable._dt.draw();
            });

            /* Program filter â†’ only InternsTable re-fetches */
            $(document).on('click', '#el-sv-intern-program-menu .el-sv-dropdown-item', function () {
                var value = parseInt($(this).data('value') || '0') || 0;
                state.selectedProgram = value;
                $('#el-sv-intern-program').val(value).trigger('change');
                $('#el-sv-intern-program-text').text($(this).text());
                $('#el-sv-intern-program-menu .el-sv-dropdown-item').removeClass('active');
                $(this).addClass('active');
                $('#el-sv-intern-program-menu').removeClass('show');
            });

            $(document).on('change', '#el-sv-intern-program', function () {
                var programId = parseInt($(this).val() || '0') || 0;
                state.selectedProgram = programId;
                app.loading && app.loading.show('Loading interns...');
                /* Only InternsTable re-fetches â€” ProgramFilter is already rendered */
                InternsTable.load(programId, '', state.selectedRole).then(function () {
                    app.loading && app.loading.hide();
                });
            });

            /* Row click â†’ navigate to intern detail */
            $(document).on('click', '#el-sv-interns-table tbody tr', function () {
                if (!InternsTable._dt) return;
                var data = InternsTable._dt.row(this).data();
                if (data) {
                    var empId = data.EmployeeId || data.employeeId || data.id;
                    if (empId) { window.location.href = '/Modules/ELearning/Supervisor/InternDetail?employeeId=' + empId; }
                }
            });
        }
    };

})(jQuery, window.app = window.app || {});
