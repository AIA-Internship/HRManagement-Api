/**
 * intern/dashboard.js â€” Component-Based Architecture
 *
 * StatsComponent      â€” fetches & renders progress ring, todo list, batch info
 * ModulesGridComponent â€” fetches & renders module cards
 *
 * Both components start loading simultaneously (Promise.all).
 * Each one renders as soon as its own data arrives, without waiting for the other.
 */
(function ($, app) {
    'use strict';
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    /* â”€â”€ Shared API shorthand â”€â”€ */
    var _api = function (path, opts) { return app.elearning.api(path, opts); };
    var _uw  = function (json, fb)   { return app.elearning.unwrap(json, fb); };


    /* â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
       COMPONENT: StatsComponent
       Fetches dashboard stats and renders progress ring, todo, batch info.
       Completely independent from ModulesGrid.
    â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• */
    var StatsComponent = {
        _data: null,
        _batches: [],
        _modules: [], /* shared reference set by ModulesGrid after its load */
        load: function (userId) {
            return _api('/api/ELearning/dashboard?userId=' + userId).then(function (json) {
                if (!json) return;
                var data = _uw(json, {});
                StatsComponent._data = data;
                var todoItems = data.toDoList || data.todoList || [];
                StatsComponent.renderProgress(data);
                StatsComponent.renderTodoList(todoItems);
                StatsComponent.renderBatchInfo(data.batch || data.batches || null);
            });
        },
        renderProgress: function (data) {
            var completed = data.completedModules || 0;
            var total = data.totalModules || 0;
            /* Supplement from modules if available */
            if (!total && StatsComponent._modules.length) {
                var mods = StatsComponent._modules;
                completed = mods.filter(function (m) { return (m.progressStatus || '').toLowerCase() === 'completed'; }).length;
                total = mods.length;
            }
            var percentage = total > 0 ? (completed / total) * 100 : 0;
            $('#el-progress-count').text(completed + '/' + total);
            $('#el-progress-desc').text(completed + ' out of ' + total + ' Modules completed');
            var circle = document.getElementById('el-progress-circle');
            if (circle) { var r = circle.r.baseVal.value; var c = 2 * Math.PI * r; circle.style.strokeDasharray = c; circle.style.strokeDashoffset = c - (percentage / 100) * c; }
            var mods = StatsComponent._modules || [];
            var allCompleted = total > 0 && completed === total;
            var missedCount = 0; var now = new Date(); now.setHours(0,0,0,0);
            mods.forEach(function (m) {
                var status = (m.progressStatus || '').toLowerCase(); var dueDate = m.dueDate || m.deadline;
                if (status !== 'completed' && dueDate) { var d = new Date(dueDate); d.setHours(0,0,0,0); if (d < now) missedCount++; }
            });
            var statusText = total > 0 ? (allCompleted ? 'Completed' : (missedCount > (total / 2) ? 'Out of Track' : 'On track')) : 'On track';
            $('#el-progress-status').text(statusText);
            var pClass = statusText.toLowerCase() === 'completed' ? 'status-completed-box' : (statusText.toLowerCase() === 'out of track' ? 'status-failed-box' : 'status-on-track-box');
            $('#el-progress-status-container').removeClass('status-completed-box status-failed-box status-on-track-box').addClass(pClass);
        },
        renderTodoList: function (todoList) {
            var html = '';
            if (!todoList || !todoList.length) {
                todoList = StatsComponent._modules.filter(function (m) { return (m.progressStatus || m.status || '').toLowerCase() !== 'completed'; }).map(function (m) { 
                    var dLeft = m.daysLeft !== undefined ? m.daysLeft : m.daysRemaining;
                    if (dLeft === undefined && (m.dueDate || m.deadline)) {
                        var dueDate = new Date(m.dueDate || m.deadline);
                        var now = new Date();
                        now.setHours(0,0,0,0);
                        var diffTime = dueDate - now;
                        var diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                        dLeft = diffDays >= 0 ? diffDays : 0;
                    }
                    return { title: m.title || m.moduleTitle, daysLeft: dLeft !== undefined ? dLeft : '-' }; 
                });
            }
            if (!todoList.length) { html = '<div class="text-gray-500 fs-7 text-center py-3">All modules completed!</div>'; }
            else { todoList.forEach(function (item) { html += '<div class="el-todo-item"><div class="d-flex align-items-start"><span class="bullet bullet-dot bg-dark me-3 mt-1"></span><div><div class="el-todo-title">' + (item.title || item.moduleName || '') + '</div><div class="el-todo-days">' + (item.daysLeft !== undefined ? item.daysLeft : (item.daysRemaining || '-')) + ' days left</div></div></div></div>'; }); }
            $('#el-todo-list').html(html);
        },
        renderBatchInfo: function (batchData) {
            if (!batchData) return;
            var batches = Array.isArray(batchData) ? batchData : [batchData];
            StatsComponent._batches = batches;
            if (batches.length) {
                var b = batches[0];
                $('#el-batch-period').text(b.period || b.batchPeriod || '');
                $('#el-batch-ends').text((b.endsIn || b.daysRemaining || '-') + ' Days');
                var batchStat = b.status || b.batchStatus || '';
                $('#el-batch-status-text').text(batchStat);
                var tClass = batchStat.toLowerCase() === 'completed' ? 'status-completed-text' : (batchStat.toLowerCase() === 'failed' ? 'status-failed-text' : 'status-on-track-text');
                $('#el-batch-status-text').closest('.el-status-badge').removeClass('status-completed-box status-failed-box status-on-track-box').addClass('el-batch-no-box ' + tClass);
                $('#el-batch-status-text').closest('.el-status-badge').find('.el-status-dot').hide();
                $('#el-batch-status-icon').removeClass('text-primary status-completed-text status-failed-text status-on-track-text').addClass(tClass);
            }
            var selectHtml = '';
            batches.forEach(function (b) { selectHtml += '<option value="' + b.id + '">' + (b.name || b.batchName || 'Batch') + '</option>'; });
            $('#el-batch-select').html(selectHtml);
        }
    };

    /* â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
       COMPONENT: ModulesGrid
       Fetches modules and renders the card grid.
       Completely independent from StatsComponent.
    â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• */
    var ModulesGrid = {
        _data: [],
        _filter: 'all',
        _visibleCount: 8,
        load: function (userId, batchId) {
            ModulesGrid._currentBatchId = batchId || null;
            app.elearning.showSkeleton('#el-modules-grid', 4);
            var qs = '?userId=' + userId;
            return _api('/api/ELearning/modules' + qs).then(function (json) {
                var mods = _uw(json, []);
                var allMods = Array.isArray(mods) ? mods : [];
                if (batchId) {
                    allMods = allMods.filter(function(m) { return m.batchId === batchId; });
                }
                ModulesGrid._data = allMods;
                /* Share reference so StatsComponent can use it if needed */
                StatsComponent._modules = ModulesGrid._data;
                ModulesGrid.render();
            });
        },
        render: function () {
            var filter = ModulesGrid._filter;
            var searchTerm = ($('#el-module-search').val() || '').toLowerCase();
            var filtered = ModulesGrid._data.filter(function (m) {
                var status = (m.progressStatus || m.status || '').toLowerCase().replace(' ', '-');
                var matchFilter = filter === 'all' || status === filter;
                var matchSearch = (m.title || '').toLowerCase().indexOf(searchTerm) > -1 || (m.description || '').toLowerCase().indexOf(searchTerm) > -1;
                return matchFilter && matchSearch;
            });
            var visible = filtered.slice(0, ModulesGrid._visibleCount);
            var html = '';
            visible.forEach(function (mod, idx) {
                var modId = mod.moduleId || mod.id;
                var status = (mod.status || mod.progressStatus || 'not-started').toLowerCase().replace(' ', '-');
                var statusLabel = mod.progressStatus || mod.statusLabel || (status === 'completed' ? 'Completed' : status === 'in-progress' ? 'In Progress' : 'Not Started');
                var dueDate = mod.dueDate || mod.deadline; var deadlineText = '-';
                if (dueDate) { var d = new Date(dueDate); deadlineText = d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }); }
                html += '<div class="col-md-6 col-lg-3 mb-4"><div class="el-module-card ' + status + ' el-animate el-animate-delay-' + ((idx % 4) + 1) + '" data-module-id="' + modId + '"><div class="d-flex flex-column"><div class="el-module-title mb-1">' + (mod.title || '') + '</div><div class="text-gray-500 fs-7 mb-3">' + deadlineText + '</div><div class="el-module-badge">' + statusLabel + '</div></div></div></div>';
            });
            $('#el-modules-grid').html(html);
            if (filtered.length > ModulesGrid._visibleCount) { $('#el-load-more').show(); } else { $('#el-load-more').hide(); }
        }
    };


    /* â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
       PAGE CONTROLLER
    â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• */
    app.elearning.dashboard = {

        init: function () {
            var user = window.aiaAuth && window.aiaAuth.getUserInfo();
            if (!user) { window.aiaAuth && window.aiaAuth.signOut(); return; }
            var displayName = user.fullName || user.name || user['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'User';
            var userId = user.EmployeeId || user.employeeId || user['EmployeeId'] || user.sub || user.id;

            /* Render greeting immediately (no fetch needed) */
            var hour = new Date().getHours();
            var greeting = hour < 12 ? 'Good Morning' : hour < 18 ? 'Good Afternoon' : 'Good Evening';
            $('#el-greeting').text(greeting + ', ' + displayName);

            /* Show skeleton in modules grid area immediately */
            app.elearning.showSkeleton('#el-modules-grid', 4);

            /* Load StatsComponent first to determine the active batch */
            StatsComponent.load(userId).then(function () {
                var initialBatchId = parseInt($('#el-batch-select').val()) || null;
                return ModulesGrid.load(userId, initialBatchId).then(function() {
                    /* Force Stats to recount based on filtered modules */
                    var data = StatsComponent._data || {};
                    StatsComponent.renderProgress(data);
                    StatsComponent.renderTodoList(data.toDoList || data.todoList || []);
                });
            }).then(function () {
                this.bindEvents();
                var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
                tooltipTriggerList.forEach(function (el) { new bootstrap.Tooltip(el); });
            }.bind(this));
        },

        bindEvents: function () {
            /* Filter pills â€” only ModulesGrid re-renders (no fetch) */
            $(document).on('click', '.el-filter-pill', function () {
                $('.el-filter-pill').removeClass('active');
                $(this).addClass('active');
                ModulesGrid._filter = $(this).data('filter');
                ModulesGrid._visibleCount = 8;
                ModulesGrid.render();
            });

            /* Search â€” only ModulesGrid re-renders (no fetch) */
            $(document).on('input', '#el-module-search', function () {
                ModulesGrid._visibleCount = 8; ModulesGrid.render();
            });

            /* Load More â€” re-render only */
            $(document).on('click', '#el-load-more', function () {
                ModulesGrid._visibleCount += 4; ModulesGrid.render();
            });

            /* Batch select â€” only batch info updates */
            $(document).on('change', '#el-batch-select', function () {
                var batchId = parseInt($(this).val());
                var batch = StatsComponent._batches.find(function (b) { return b.id === batchId; });
                if (batch) {
                    $('#el-batch-period').text(batch.period || batch.batchPeriod || '');
                    $('#el-batch-ends').text((batch.endsIn || batch.daysRemaining || '-') + ' Days');
                    var batchStat = batch.status || batch.batchStatus || '';
                    $('#el-batch-status-text').text(batchStat);
                    var tClass = batchStat.toLowerCase() === 'completed' ? 'status-completed-text' : (batchStat.toLowerCase() === 'failed' ? 'status-failed-text' : 'status-on-track-text');
                    $('#el-batch-status-text').closest('.el-status-badge').removeClass('status-completed-box status-failed-box status-on-track-box').addClass('el-batch-no-box ' + tClass);
                    $('#el-batch-status-text').closest('.el-status-badge').find('.el-status-dot').hide();
                    $('#el-batch-status-icon').removeClass('text-primary status-completed-text status-failed-text status-on-track-text').addClass(tClass);
                }
                
                /* Fetch modules for selected batch and update Stats */
                var user = window.aiaAuth && window.aiaAuth.getUserInfo();
                var userId = user ? (user.EmployeeId || user.employeeId || user.sub || user.id) : null;
                if (userId) {
                    ModulesGrid.load(userId, batchId).then(function() {
                        /* Force Stats to recount based on new modules */
                        var data = StatsComponent._data || {};
                        StatsComponent.renderProgress(data);
                        StatsComponent.renderTodoList(data.toDoList || data.todoList || []);
                    });
                }
            });

            /* Module card click â€” navigate */
            $(document).on('click', '.el-module-card', function () {
                var moduleId = $(this).data('module-id');
                window.location.href = '/Modules/ELearning/Intern/Modules?id=' + moduleId;
            });
        }
    };

})(jQuery, window.app = window.app || {});
