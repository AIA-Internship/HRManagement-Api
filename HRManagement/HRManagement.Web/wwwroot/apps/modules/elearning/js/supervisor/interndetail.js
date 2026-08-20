(function ($, app) {
    'use strict';

    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.supervisor.interndetail = app.elearning.supervisor.interndetail || {};

    app.elearning.supervisor.interndetail._api = function (url, method, data) {
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

    app.elearning.supervisor.interndetail.init = function () {
        var self = this;
        self._data = [];
        
        var urlParams = new URLSearchParams(window.location.search);
        self._employeeId = urlParams.get('employeeId');

        if (!self._employeeId) {
            Swal.fire({ icon: 'error', title: 'Error', text: 'No Employee ID specified.', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false }).then(function() {
                window.location.href = '/Modules/ELearning/Supervisor/Interns';
            });
            return;
        }

        self._bindEvents();
        
        app.loading && app.loading.show('Loading details...');
        
        Promise.resolve(self._fetchDetails()).catch(function(err) {
            console.error('Failed to fetch intern details:', err);
            Swal.fire({ icon: 'error', title: 'Error', text: 'Failed to load data.', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
        }).finally(function() {
            app.loading && app.loading.hide();
        });
    };

    app.elearning.supervisor.interndetail._fetchDetails = function () {
        var self = this;
        var url = '/api/ELearning/interns/' + self._employeeId + '/module-details';
        
        return self._api(url).then(function (json) {
            var content = json.content || json.data || json || {};
            self._data = Array.isArray(content) ? content : (content.modules || content.Modules || []);
            
            self._internName = content.internName || content.InternName || '';
            $('#el-sv-intern-name').text(self._internName);
            
            self._populateBatches();
            self._renderGrid();
        });
    };

    app.elearning.supervisor.interndetail._populateBatches = function () {
        var self = this;
        var menu = $('#el-sv-intern-batch-menu');
        menu.find('.el-sv-dropdown-item:not([data-value=""])').remove();
        
        var batches = [];
        var batchIds = [];
        self._data.forEach(function (d) {
            if (d.batchId && batchIds.indexOf(d.batchId) === -1) {
                batchIds.push(d.batchId);
                batches.push({ id: d.batchId, name: d.batchName || ('Batch ' + d.batchId) });
            }
        });
        
        batches.sort(function(a, b) { return a.name.localeCompare(b.name); });
        batches.forEach(function (b) {
            menu.append('<div class="el-sv-dropdown-item" data-value="' + b.id + '">' + b.name + '</div>');
        });
    };

    app.elearning.supervisor.interndetail._renderGrid = function () {
        var self = this;
        var batchFilter = $('#el-sv-intern-batch').val() || '';
        var statusFilter = ($('#el-sv-intern-status').val() || '').toLowerCase();
        
        var filtered = self._data.filter(function (m) {
            if (batchFilter && String(m.batchId) !== batchFilter) return false;
            if (statusFilter && (m.progressStatus || '').toLowerCase() !== statusFilter) return false;
            return true;
        });

        var html = '';
        if (filtered.length === 0) {
            html = '<div class="col-12 text-center text-gray-500 py-10">No modules match the current filter</div>';
        } else {
            filtered.forEach(function (mod, idx) {
                var score = mod.score != null ? mod.score : 'N/A';
                var rawDate = mod.dueDate || '';
                var displayDate = '-';
                if (rawDate) {
                    var d = new Date(rawDate);
                    displayDate = d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
                }
                
                var status = mod.progressStatus || '';
                var badgeClass = 'bg-secondary text-gray-800';
                if (status.toLowerCase() === 'completed') badgeClass = 'bg-success text-white';
                else if (status.toLowerCase() === 'failed') badgeClass = 'bg-danger text-white';
                else if (status.toLowerCase() === 'in progress') badgeClass = 'bg-primary text-white';

                html += '<div class="col-md-6 col-lg-3 mb-4">' +
                    '<div class="el-sv-module-card el-animate el-animate-delay-' + ((idx % 4) + 1) + '">' +
                    '<div>' +
                    '<div class="el-sv-card-role">' + (mod.batchName || '') + '</div>' + 
                    '<div class="el-sv-card-title">' + (mod.title || '') + '</div>' +
                    '<div class="mt-2"><span class="badge ' + badgeClass + '">' + status + '</span></div>' +
                    '<div class="el-sv-card-due mt-3">Score: <strong>' + score + '</strong></div>' +
                    '<div class="el-sv-card-due mt-1">Due Date: ' + displayDate + '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>';
            });
        }
        $('#el-sv-intern-detail-grid').html(html);
    };

    app.elearning.supervisor.interndetail._bindEvents = function () {
        var self = this;

        // Back button removed from JS since it's just an <a> tag now

        $(document).on('click', '.el-sv-dropdown-toggle', function (e) {
            e.stopPropagation();
            var menu = $(this).siblings('.el-sv-dropdown-menu');
            $('.el-sv-dropdown-menu').not(menu).removeClass('show');
            menu.toggleClass('show');
        });
        $(document).on('click', function () { $('.el-sv-dropdown-menu').removeClass('show'); });
        $(document).on('click', '.el-sv-dropdown-menu', function (e) { e.stopPropagation(); });

        $(document).on('click', '#el-sv-intern-batch-menu .el-sv-dropdown-item', function () {
            var value = $(this).data('value');
            $('#el-sv-intern-batch').val(value).trigger('change');
            $('#el-sv-intern-batch-text').text($(this).text());
            $('#el-sv-intern-batch-menu .el-sv-dropdown-item').removeClass('active');
            $(this).addClass('active');
            $('#el-sv-intern-batch-menu').removeClass('show');
        });
        
        $(document).on('click', '#el-sv-intern-status-menu .el-sv-dropdown-item', function () {
            var value = $(this).data('value');
            $('#el-sv-intern-status').val(value).trigger('change');
            $('#el-sv-intern-status-text').text($(this).text());
            $('#el-sv-intern-status-menu .el-sv-dropdown-item').removeClass('active');
            $(this).addClass('active');
            $('#el-sv-intern-status-menu').removeClass('show');
        });

        $('#el-sv-intern-batch, #el-sv-intern-status').on('change', function () {
            self._renderGrid();
        });
    };

})(jQuery, window.app = window.app || {});
