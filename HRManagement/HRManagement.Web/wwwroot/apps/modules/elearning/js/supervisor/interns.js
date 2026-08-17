(function ($, app) {
    'use strict';

    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.supervisor.interns = app.elearning.supervisor.interns || {};

    app.elearning.supervisor.interns._api = function (url, method, data) {
        method = method || 'GET';
        var fullUrl = url.indexOf('http') === 0 ? url : 'https://localhost:7089' + (url.indexOf('/') === 0 ? '' : '/') + url;
        var token = window.aiaAuth ? window.aiaAuth.getToken() : '';
        return $.ajax({
            url: fullUrl,
            method: method,
            contentType: 'application/json',
            data: data ? JSON.stringify(data) : undefined,
            headers: { 'Authorization': 'Bearer ' + token }
        });
    };

    app.elearning.supervisor.interns.init = function () {
        var self = this;
        self._data = [];
        self._currentProgram = 0;
        self._buildTable();
        self._registerCustomSearch();
        self._bindEvents();
        
        var initProgram = parseInt($('#el-sv-intern-program').val() || '0') || 0;
        var initRole = $('#el-sv-intern-role').val() || '';
        
        app.loading && app.loading.show('Loading interns...');
        
        Promise.resolve(self._fetchPrograms()).catch(function(err) {
            console.error('Failed to fetch programs:', err);
        }).finally(function() {
            Promise.resolve(self._fetchInterns(initProgram, '', initRole)).catch(function(err) {
                console.error('Failed to fetch interns:', err);
            }).finally(function () {
                app.loading && app.loading.hide();
            });
        });
    };

    app.elearning.supervisor.interns._fetchPrograms = function () {
        var self = this;
        return self._api('/api/ELearning/programs').then(function (json) {
            var programs = (json && (json.content || json.data || json)) || [];
            programs = Array.isArray(programs) ? programs : [];
            var menu = $('#el-sv-intern-program-menu');
            menu.find('.el-sv-dropdown-item:not([data-value=""])').remove();
            programs.forEach(function (p) {
                var name = p.programName || p.name || ('Program ' + p.programId);
                menu.append('<div class="el-sv-dropdown-item" data-value="' + p.programId + '">' + name + '</div>');
            });
        });
    };

    app.elearning.supervisor.interns._populateRoles = function () {
        var self = this;
        var menu = $('#el-sv-intern-role-menu');
        menu.find('.el-sv-dropdown-item:not([data-value=""])').remove();
        
        var roles = [];
        self._data.forEach(function (d) {
            var r = d.Role || d.role || d.internRole || '';
            if (r && roles.indexOf(r) === -1) {
                roles.push(r);
            }
        });
        
        roles.sort();
        roles.forEach(function (r) {
            menu.append('<div class="el-sv-dropdown-item" data-value="' + r + '">' + r + '</div>');
        });
    };

    app.elearning.supervisor.interns._fetchInterns = function (programId, search, role) {
        var self = this;
        var url = '/api/ELearning/programs/' + (programId || 0) + '/interns?pageNumber=1&pageSize=100';
        if (search) url += '&search=' + encodeURIComponent(search);
        
        return self._api(url).then(function (json) {
            var content = json.content || json.data || json.interns || json || [];
            self._data = Array.isArray(content) ? content : [];
            if (self._table) {
                self._table.clear();
                self._table.rows.add(self._data);
                self._table.draw();
            }
            self._populateRoles();
        });
    };

    app.elearning.supervisor.interns._buildTable = function () {
        var self = this;

        self._table = $('#el-sv-interns-table').DataTable({
            data: self._data,
            pageLength: 10,
            lengthChange: false,
            responsive: true,
            order: [[4, 'desc']],
            language: {
                search: '',
                searchPlaceholder: 'Search by ID or name...',
                paginate: { previous: 'Previous', next: 'Next' },
                info: 'Showing _START_ to _END_ of _TOTAL_ interns',
                infoEmpty: 'No interns found',
                zeroRecords: 'No interns match the current filter'
            },
            dom: '<"el-dt-top d-flex align-items-center gap-3 mb-4"f>rt<"el-dt-bottom d-flex justify-content-between align-items-center mt-4 flex-wrap gap-3"ip>',
            columns: [
                {
                    data: null,
                    className: 'text-start el-truncate-cell',
                    width: '15%',
                    render: function (data, type) {
                        var empId = data.EmployeeId || data.employeeId || data.id || '';
                        if (type === 'filter' || type === 'sort') return String(empId);
                        var safeText = $('<div>').text(empId).html();
                        return '<span class="fw-bold text-gray-800" title="' + safeText + '">' + safeText + '</span>';
                    }
                },
                {
                    data: null,
                    className: 'el-truncate-cell',
                    width: '25%',
                    render: function (data, type) {
                        var name = data.Name || data.name || data.fullName || '';
                        if (type === 'filter' || type === 'sort') return String(name);
                        var safeText = $('<div>').text(name).html();
                        return '<span class="fw-bold text-gray-800" title="' + safeText + '">' + safeText + '</span>';
                    }
                },
                {
                    data: null,
                    className: 'el-truncate-cell',
                    width: '20%',
                    render: function (data, type) {
                        var role = data.Role || data.role || data.internRole || '';
                        if (type === 'filter' || type === 'sort') return String(role);
                        var safeText = $('<div>').text(role).html();
                        return '<span class="fw-bold text-gray-800" title="' + safeText + '">' + safeText + '</span>';
                    }
                },
                {
                    data: null,
                    orderable: true,
                    className: 'el-truncate-cell',
                    width: '20%',
                    render: function (data, type) {
                        var text = data.TotalModulesCompletedText || data.totalModulesCompletedText || data.modulesCompleted || '0 / 0';
                        if (type === 'filter' || type === 'sort' || type === 'type') return String(text);
                        var safeText = $('<div>').text(text).html();
                        return '<div class="el-sv-modules-completed" title="' + safeText + '">' +
                            '<i class="ki-duotone ki-document fs-5"><span class="path1"></span><span class="path2"></span></i> ' +
                            safeText +
                            '</div>';
                    }
                },
                {
                    data: null,
                    className: 'text-end el-truncate-cell',
                    width: '20%',
                    render: function (data, type) {
                        var scoreText = data.AccumulativeScoreDisplay || data.accumulativeScoreDisplay || data.score || '0';
                        if (type === 'filter' || type === 'sort' || type === 'type') return String(scoreText);
                        var safeText = $('<div>').text(scoreText).html();
                        return '<span class="fw-bold" title="' + safeText + '">' + safeText + '</span>';
                    }
                }
            ],
            drawCallback: function () {
                $('#el-sv-interns-table tbody tr').addClass('el-dt-row-animate');
            }
        });

        $('#el-sv-interns-table_filter input').addClass('el-search-input');
        $('#el-sv-interns-table_filter label').contents().filter(function () {
            return this.nodeType === 3;
        }).remove();
    };

    app.elearning.supervisor.interns._registerCustomSearch = function () {
        $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
            if (settings.nTable.id !== 'el-sv-interns-table') return true;
            var roleFilter = ($('#el-sv-intern-role').val() || '').toLowerCase();
            var roleCell = $('<div>').html(data[2]).text().toLowerCase();
            if (roleFilter && roleCell.indexOf(roleFilter) === -1) return false;
            return true;
        });
    };

    app.elearning.supervisor.interns._bindEvents = function () {
        var self = this;

        $(document).on('click', '.el-sv-dropdown-toggle', function (e) {
            e.stopPropagation();
            var menu = $(this).siblings('.el-sv-dropdown-menu');
            $('.el-sv-dropdown-menu').not(menu).removeClass('show');
            menu.toggleClass('show');
        });
        $(document).on('click', function () { $('.el-sv-dropdown-menu').removeClass('show'); });
        $(document).on('click', '.el-sv-dropdown-menu', function (e) { e.stopPropagation(); });

        $(document).on('click', '#el-sv-intern-role-menu .el-sv-dropdown-item', function () {
            var value = $(this).data('value');
            $('#el-sv-intern-role').val(value).trigger('change');
            $('#el-sv-intern-role-text').text($(this).text());
            $('#el-sv-intern-role-menu .el-sv-dropdown-item').removeClass('active');
            $(this).addClass('active');
            $('#el-sv-intern-role-menu').removeClass('show');
        });

        $(document).on('click', '#el-sv-intern-program-menu .el-sv-dropdown-item', function () {
            var value = $(this).data('value');
            $('#el-sv-intern-program').val(value).trigger('change');
            $('#el-sv-intern-program-text').text($(this).text());
            $('#el-sv-intern-program-menu .el-sv-dropdown-item').removeClass('active');
            $(this).addClass('active');
            $('#el-sv-intern-program-menu').removeClass('show');
        });

        $(document).on('change', '#el-sv-intern-role', function () {
            if (self._table) self._table.draw();
        });

        $(document).on('change', '#el-sv-intern-program', function () {
            var programId = parseInt($(this).val() || '0') || 0;
            self._currentProgram = programId;
            var role = $('#el-sv-intern-role').val() || '';
            app.loading && app.loading.show('Loading interns...');
            self._fetchInterns(programId, '', role).then(function () {
                app.loading && app.loading.hide();
            });
        });
    };

})(jQuery, window.app = window.app || {});
