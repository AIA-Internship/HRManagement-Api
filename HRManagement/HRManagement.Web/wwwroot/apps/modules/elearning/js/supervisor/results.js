(function ($, app) {
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    app.elearning.supervisor.results = {};
    app.elearning.supervisor.results._table = null;
    app.elearning.supervisor.results._allData = [];
    app.elearning.supervisor.results._moduleId = null;

    app.elearning.supervisor.results._api = async function (path, opts) {
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

    app.elearning.supervisor.results._getQuizId = function () {
        var params = new URLSearchParams(window.location.search);
        return parseInt(params.get('quizId') || '0') || null;
    };

    app.elearning.supervisor.results.init = function () {
        var self = this;
        var quizId = self._getQuizId();
        if (!quizId) return;

        app.loading && app.loading.show('Loading results...');
        return self._api('/api/ELearning/quiz/' + quizId + '/submissions').then(function (json) {
            app.loading && app.loading.hide();
            var dataObj = (json && (json.content || json.data || json)) || {};
            var sub = Array.isArray(dataObj.submitted) ? dataObj.submitted : [];
            var notSub = Array.isArray(dataObj.notSubmitted) ? dataObj.notSubmitted : [];
            self._moduleId = dataObj.moduleId || parseInt(sessionStorage.getItem('sv_quiz_module_id') || '0') || null;
            self._allData = [];
            sub.forEach(function(item) {
                item.submissionStatus = 'Submitted';
                self._allData.push(item);
            });
            notSub.forEach(function(item) {
                item.submissionStatus = 'Not Submitted';
                self._allData.push(item);
            });

            self.renderBadge(dataObj, sub.length, notSub.length);
            self._buildTable();
            self._bindEvents();
        });
    };

    app.elearning.supervisor.results.renderBadge = function (data, subLen, notSubLen) {
        var totalSubmitted = (data && data.submittedCount) || subLen;
        var totalInterns = (data && data.totalEligible) || (subLen + notSubLen);
        $('#el-sv-results-count').text(totalSubmitted + ' / ' + totalInterns + ' Submitted');
    };

    app.elearning.supervisor.results._getColumns = function () {
        return [
            {
                title: 'Employee ID',
                data: null,
                className: 'text-start',
                render: function (data) {
                    return '<span class="fw-bold text-gray-800">' + (data.userId || data.employeeId || data.id || '') + '</span>';
                }
            },
            {
                title: 'Name',
                data: null,
                render: function (data) {
                    return '<span class="fw-bold text-gray-800">' + (data.name || data.fullName || '') + '</span>';
                }
            },
            {
                title: 'Status',
                data: null,
                className: 'text-center',
                render: function (data) {
                    if (data.submissionStatus === 'Submitted') {
                        return '<span class="badge badge-light-success fw-semibold">Submitted</span>';
                    }
                    return '<span class="badge badge-light-danger fw-semibold">Not Submitted</span>';
                }
            },
            {
                title: 'Score',
                data: null,
                className: 'text-center',
                render: function (data, type) {
                    if (data.submissionStatus !== 'Submitted') return '<span class="badge badge-light text-gray-500 fw-semibold">-</span>';
                    
                    var score = data.score !== undefined ? data.score : data.totalScore;
                    if (type === 'sort' || type === 'type') return score !== null && score !== undefined ? score : -1;
                    if (score !== null && score !== undefined) {
                        var color = score >= 70 ? '#1bc5bd' : score >= 50 ? '#ffc700' : '#f1416c';
                        return '<span class="fw-bold fs-6" style="color:' + color + ';">' + score + '</span>';
                    }
                    return '<span class="badge badge-light text-gray-500 fw-semibold">Not Graded</span>';
                }
            },
            {
                title: 'Action',
                data: null,
                orderable: false,
                className: 'text-end',
                render: function (data) {
                    if (data.submissionStatus === 'Submitted') {
                        return '<button class="btn-el-view-answer" data-submission-id="' + (data.submissionId || data.id || '') + '">View Answer</button>';
                    }
                    return ''; // No action for Not Submitted
                }
            }
        ];
    };

    app.elearning.supervisor.results._buildTable = function () {
        var self = this;
        var data = self._allData;
        var columns = self._getColumns();

        var theadHtml = '<tr class="fw-bold text-gray-500 fs-7 text-uppercase">';
        columns.forEach(function (col) {
            var cls = col.className ? ' class="' + col.className + '"' : '';
            theadHtml += '<th' + cls + '>' + col.title + '</th>';
        });
        theadHtml += '</tr>';
        $('#el-sv-results-table thead').html(theadHtml);

        if (self._table) {
            self._table.destroy();
            self._table = null;
        }

        self._table = $('#el-sv-results-table').DataTable({
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
            columns: columns
        });

        $('#el-sv-results-table_filter input').addClass('el-search-input');
        $('#el-sv-results-table_filter label').contents().filter(function () {
            return this.nodeType === 3;
        }).remove();
    };

    app.elearning.supervisor.results._bindEvents = function () {
        var self = this;

        $(document).on('click', '#el-sv-results-back', function () {
            var moduleId = self._moduleId || parseInt(sessionStorage.getItem('sv_quiz_module_id') || '0') || null;
            if (moduleId) {
                window.location.href = '/Modules/ELearning/Supervisor/ModuleDetail?id=' + moduleId;
            } else {
                window.location.href = '/Modules/ELearning/Supervisor/Modules';
            }
        });

        $(document).on('click', '.btn-el-view-answer', function () {
            var submissionId = $(this).data('submission-id');
            window.location.href = '/Modules/ELearning/Supervisor/QuizAnswers?submissionId=' + submissionId;
        });
    };

})(jQuery, window.app = window.app || {});
