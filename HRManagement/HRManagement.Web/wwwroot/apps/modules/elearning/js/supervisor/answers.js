(function ($, app) {
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    app.elearning.supervisor.answers = {};
    app.elearning.supervisor.answers.currentTab = 'not-submitted';
    app.elearning.supervisor.answers._table = null;
    app.elearning.supervisor.answers._submitted = [];
    app.elearning.supervisor.answers._notSubmitted = [];

    app.elearning.supervisor.answers._api = async function (path, opts) {
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

    app.elearning.supervisor.answers._getQuizId = function () {
        var params = new URLSearchParams(window.location.search);
        return parseInt(params.get('quizId') || '0') || null;
    };

    app.elearning.supervisor.answers.init = function () {
        var self = this;
        var quizId = self._getQuizId();
        if (!quizId) return;

        app.loading && app.loading.show('Loading submissions...');
        self._api('/api/ELearning/quiz/' + quizId + '/submissions').then(function (json) {
            app.loading && app.loading.hide();
            if (!json) return;
            var data = json.content || json.data || json;
            self._submitted = data.submitted || [];
            self._notSubmitted = data.notSubmitted || [];
            self._buildTable();
            self._bindEvents();
        });
    };

    app.elearning.supervisor.answers._buildTable = function () {
        var self = this;
        var data = self._getTabData();

        self._table = $('#el-sv-answers-table').DataTable({
            data: data,
            pageLength: 10,
            lengthChange: false,
            responsive: true,
            language: {
                search: '',
                searchPlaceholder: 'Search by ID or name...',
                paginate: { previous: 'Previous', next: 'Next' },
                info: 'Showing _START_ to _END_ of _TOTAL_ entries',
                infoEmpty: 'No entries found',
                zeroRecords: 'No entries match your search'
            },
            dom: '<"el-dt-top d-flex align-items-center gap-3 mb-4"f>rt<"el-dt-bottom d-flex justify-content-between align-items-center mt-4 flex-wrap gap-3"ip>',
            columns: [
                {
                    data: null,
                    render: function (data) { return '<span class="fw-bold text-gray-800">' + (data.employeeId || data.id || '') + '</span>'; }
                },
                {
                    data: null,
                    render: function (data) { return '<span class="fw-bold text-gray-800">' + (data.name || data.fullName || '') + '</span>'; }
                },
                {
                    data: null,
                    orderable: false,
                    className: 'text-end',
                    render: function () {
                        var tab = app.elearning.supervisor.answers.currentTab;
                        if (tab === 'submitted') {
                            return '<span class="badge badge-light-success fw-semibold fs-8 px-3 py-2">Submitted</span>';
                        }
                        return '<span class="badge badge-light-danger fw-semibold fs-8 px-3 py-2">Not Submitted</span>';
                    }
                }
            ]
        });

        $('#el-sv-answers-table_filter input').addClass('el-search-input');
        $('#el-sv-answers-table_filter label').contents().filter(function () {
            return this.nodeType === 3;
        }).remove();
    };

    app.elearning.supervisor.answers._getTabData = function () {
        return this.currentTab === 'submitted' ? this._submitted : this._notSubmitted;
    };

    app.elearning.supervisor.answers._reloadTable = function () {
        var self = this;
        if (!self._table) return;
        self._table.clear();
        self._table.rows.add(self._getTabData());
        self._table.draw();
    };

    app.elearning.supervisor.answers._bindEvents = function () {
        var self = this;

        $(document).on('click', '.el-sv-tab', function () {
            var tab = $(this).data('tab');
            if (!tab || !$('#el-sv-answers-table').length) return;
            self.currentTab = tab;
            $('.el-sv-tab').removeClass('active');
            $(this).addClass('active');
            self._reloadTable();
        });
    };

})(jQuery, window.app = window.app || {});


