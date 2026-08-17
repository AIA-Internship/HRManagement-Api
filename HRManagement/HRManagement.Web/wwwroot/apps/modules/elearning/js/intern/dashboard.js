(function ($, app) {
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    app.elearning.dashboard = {};
    app.elearning.dashboard.currentFilter = 'all';
    app.elearning.dashboard.visibleCount = 8;
    app.elearning.dashboard._data = null;
    app.elearning.dashboard._modules = [];
    app.elearning.dashboard._batches = [];

    app.elearning.dashboard._api = async function (path, opts) {
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

    app.elearning.dashboard.init = function () {
        var self = this;
        var user = window.aiaAuth && window.aiaAuth.getUserInfo();
        if (!user) { window.aiaAuth && window.aiaAuth.signOut(); return; }

        var displayName = user.fullName || user.name || user['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'User';
        var userId = user.EmployeeId || user.employeeId || user['EmployeeId'] || user.sub || user.id;
        self.renderGreeting(displayName);
        app.loading && app.loading.show('Loading dashboard...');
        self._loadData(userId).then(function () {
            app.loading && app.loading.hide();
            self.bindEvents();
            
            // Initialize tooltips
            var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        });
    };

    app.elearning.dashboard._loadData = function (userId) {
        var self = this;
        return Promise.all([
            self._api('/api/ELearning/dashboard?userId=' + userId),
            self._api('/api/ELearning/modules?userId=' + userId)
        ]).then(function (results) {
            var dashJson = results[0];
            var modsJson = results[1];

            if (dashJson) {
                var data = dashJson.content || dashJson.data || dashJson;
                self._data = data;
                var todoItems = data.toDoList || data.todoList || [];
                self.renderProgress(data);
                self.renderTodoList(todoItems);
                self.renderBatchInfo(data.batch || data.batches || null);
            }

            if (modsJson) {
                var mods = modsJson.content || modsJson.data || modsJson || [];
                self._modules = Array.isArray(mods) ? mods : [];
                self.renderModules();
            }
        });
    };

    app.elearning.dashboard.renderGreeting = function (name) {
        var hour = new Date().getHours();
        var greeting = hour < 12 ? 'Good Morning' : hour < 18 ? 'Good Afternoon' : 'Good Evening';
        $('#el-greeting').text(greeting + ', ' + name);
    };

    app.elearning.dashboard.renderProgress = function (data) {
        var completed = 0, total = 0;
        if (data && data.completedModules !== undefined) {
            completed = data.completedModules || 0;
            total = data.totalModules || 0;
        } else {
            var mods = this._modules;
            completed = mods.filter(function (m) { return (m.progressStatus ||  '').toLowerCase() === 'completed'; }).length;
            total = mods.length;
        }
        var percentage = total > 0 ? (completed / total) * 100 : 0;

        $('#el-progress-count').text(completed + '/' + total);
        $('#el-progress-desc').text(completed + ' out of ' + total + ' Modules completed');

        var circle = document.getElementById('el-progress-circle');
        if (circle) {
            var radius = circle.r.baseVal.value;
            var circumference = 2 * Math.PI * radius;
            circle.style.strokeDasharray = circumference;
            circle.style.strokeDashoffset = circumference - (percentage / 100) * circumference;
        }

        var mods = this._modules;
        var hasInProgress = mods.some(function (m) { return (m.progressStatus || '').toLowerCase() === 'in-progress'; });
        var allCompleted = total > 0 && completed === total;
        
        var batchStatus = data && data.batches && data.batches.length ? data.batches[0].status : null;
        var statusText = batchStatus || (allCompleted ? 'Completed' : hasInProgress ? 'In Progress' : 'Not Started');
        $('#el-progress-status').text(statusText);
    };

    app.elearning.dashboard.renderTodoList = function (todoList) {
        var html = '';
        if (!todoList || !todoList.length) {
            todoList = this._modules.filter(function (m) {
                return (m.progressStatus || m.status || '').toLowerCase() !== 'completed';
            }).map(function (m) {
                return { title: m.title, daysLeft: m.daysLeft || m.daysRemaining || '-' };
            });
        }
        if (!todoList.length) {
            html = '<div class="text-gray-500 fs-7 text-center py-3">All modules completed!</div>';
        } else {
            todoList.forEach(function (item) {
                html += '<div class="el-todo-item">' +
                    '<div class="d-flex align-items-start">' +
                    '<span class="bullet bullet-dot bg-dark me-3 mt-1"></span>' +
                    '<div>' +
                    '<div class="el-todo-title">' + (item.title || item.moduleName || '') + '</div>' +
                    '<div class="el-todo-days">' + (item.daysLeft !== undefined ? item.daysLeft : (item.daysRemaining || '-')) + ' days left</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>';
            });
        }
        $('#el-todo-list').html(html);
    };

    app.elearning.dashboard.renderBatchInfo = function (batchData) {
        if (!batchData) return;
        var batches = Array.isArray(batchData) ? batchData : [batchData];
        this._batches = batches;
        if (batches.length) {
            var b = batches[0];
            $('#el-batch-period').text(b.period || b.batchPeriod || '');
            $('#el-batch-ends').text((b.endsIn || b.daysRemaining || '-') + ' Days');
            $('#el-batch-status-text').text(b.status || b.batchStatus || '');
        }
        var selectHtml = '';
        batches.forEach(function (b) {
            selectHtml += '<option value="' + b.id + '">' + (b.name || b.batchName || 'Batch') + '</option>';
        });
        $('#el-batch-select').html(selectHtml);
    };

    app.elearning.dashboard.renderModules = function () {
        var self = this;
        var filter = self.currentFilter;
        var searchTerm = ($('#el-module-search').val() || '').toLowerCase();
        var filtered = self._modules.filter(function (m) {
            var status = (m.progressStatus || m.status || '').toLowerCase().replace(' ', '-');
            var matchFilter = filter === 'all' || status === filter;
            var matchSearch = (m.title || '').toLowerCase().indexOf(searchTerm) > -1 ||
                (m.description || '').toLowerCase().indexOf(searchTerm) > -1;
            return matchFilter && matchSearch;
        });
        var visible = filtered.slice(0, self.visibleCount);
        var html = '';
        visible.forEach(function (mod, idx) {
            var modId = mod.moduleId || mod.id;
            var status = (mod.status || mod.progressStatus || 'not-started').toLowerCase().replace(' ', '-');
            var statusLabel = mod.progressStatus || mod.statusLabel || (status === 'completed' ? 'Completed' : status === 'in-progress' ? 'In Progress' : 'Not Started');
            
            var dueDate = mod.dueDate || mod.deadline;
            var deadlineText = '-';
            if (dueDate) {
                var d = new Date(dueDate);
                var months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
                deadlineText = d.getDate() + ' ' + months[d.getMonth()];
            }

            html += '<div class="col-md-6 col-lg-3 mb-4">' +
                '<div class="el-module-card ' + status + ' el-animate el-animate-delay-' + ((idx % 4) + 1) + '" data-module-id="' + modId + '">' +
                '<div class="d-flex flex-column">' +
                '<div class="el-module-title mb-1">' + (mod.title || '') + '</div>' +
                '<div class="text-gray-500 fs-7 mb-3">' + deadlineText + '</div>' +
                '<div class="el-module-badge">' + statusLabel + '</div>' +
                '</div>' +
                '</div>' +
                '</div>';
        });
        $('#el-modules-grid').html(html);
        if (filtered.length > self.visibleCount) { $('#el-load-more').show(); } else { $('#el-load-more').hide(); }
    };

    app.elearning.dashboard.bindEvents = function () {
        var self = this;

        $(document).on('click', '.el-filter-pill', function () {
            $('.el-filter-pill').removeClass('active');
            $(this).addClass('active');
            self.currentFilter = $(this).data('filter');
            self.visibleCount = 8;
            self.renderModules();
        });

        $(document).on('input', '#el-module-search', function () {
            self.visibleCount = 8;
            self.renderModules();
        });

        $(document).on('click', '#el-load-more', function () {
            self.visibleCount += 4;
            self.renderModules();
        });

        $(document).on('change', '#el-batch-select', function () {
            var batchId = parseInt($(this).val());
            var batch = self._batches.find(function (b) { return b.id === batchId; });
            if (batch) {
                $('#el-batch-period').text(batch.period || batch.batchPeriod || '');
                $('#el-batch-ends').text((batch.endsIn || batch.daysRemaining || '-') + ' Days');
                $('#el-batch-status-text').text(batch.status || batch.batchStatus || '');
            }
        });

        $(document).on('click', '.el-module-card', function () {
            var moduleId = $(this).data('module-id');
            window.location.href = '/Modules/ELearning/Intern/Modules?id=' + moduleId;
        });
    };

})(jQuery, window.app = window.app || {});


