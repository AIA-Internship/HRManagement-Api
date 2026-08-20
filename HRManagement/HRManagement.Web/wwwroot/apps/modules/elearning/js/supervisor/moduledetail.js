/**
 * supervisor/moduledetail.js — Component-Based Architecture
 *
 * ContentPanel — loads and renders content list independently
 * QuizPanel    — loads and renders quiz list independently
 *
 * Both panels start fetching in parallel. Each shows its own skeleton
 * and renders as soon as its data arrives, without waiting for the other.
 */
(function ($, app) {
    'use strict';
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    /* ── Shared API shorthand ── */
    var _api = function (path, opts) { return app.elearning.api(path, opts); };
    var _uw  = function (json, fb)   { return app.elearning.unwrap(json, fb); };

    var _moduleId   = null;
    var _moduleData = null;

    /* ── Icon helpers ── */
    var _iconMap = {
        pdf:    '<i class="ki-duotone ki-document fs-2"><span class="path1"></span><span class="path2"></span></i>',
        slides: '<i class="ki-duotone ki-some-files fs-2"><span class="path1"></span><span class="path2"></span></i>',
        video:  '<i class="ki-duotone ki-screen fs-2"><span class="path1"></span><span class="path2"></span></i>'
    };
    function _getIconClass(filename) {
        var ext = (filename || '').split('.').pop().toLowerCase();
        if (ext === 'pdf') return 'pdf';
        if (ext === 'ppt' || ext === 'pptx') return 'slides';
        if (ext === 'mp4' || ext === 'mov' || ext === 'avi') return 'video';
        return 'pdf';
    }

    /* ════════════════════════════════════════════
       COMPONENT: ContentPanel
       Fetches and renders the content list independently.
       Call .refresh() after upload or delete — Quiz is NOT touched.
    ════════════════════════════════════════════ */
    var ContentPanel = {
        _data: [],
        load: function (mod) {
            /* Data already included in the module response — no extra fetch needed */
            ContentPanel._data = mod.contents || mod.materials || [];
            ContentPanel.render();
        },
        refresh: function () {
            /* Re-fetch only the module to get updated content list */
            app.elearning.showSkeleton('#el-sv-content-list', 2);
            return _api('/api/ELearning/modules/' + _moduleId).then(function (json) {
                var mod = _uw(json, {});
                _moduleData = mod;
                ContentPanel._data = mod.contents || mod.materials || [];
                ContentPanel.render();
            });
        },
        render: function () {
            var contents = ContentPanel._data;
            var html = '';
            if (!contents.length) {
                html = '<div class="text-gray-500 fs-7 py-4 text-center">No content yet. Click "Add Content" to upload.</div>';
            } else {
                contents.forEach(function (c, idx) {
                    var iconClass = c.iconClass || _getIconClass(c.fileName || c.name || '');
                    var sizeText = c.size || c.fileSize || '';
                    if (!sizeText && c.fileSizeBytes) { var kb = c.fileSizeBytes / 1024; sizeText = kb >= 1024 ? (kb / 1024).toFixed(1) + ' MB' : Math.round(kb) + ' KB'; }
                    html += '<div class="el-sv-content-item el-animate el-animate-delay-' + (idx + 1) + '">' +
                        '<div class="el-sv-content-info">' +
                        '<div class="el-material-icon ' + iconClass + ' me-3">' + (_iconMap[iconClass] || _iconMap.pdf) + '</div>' +
                        '<div><div class="fw-bold fs-6 text-gray-800">' + (c.title || c.name || '') + '</div>' +
                        '<div class="fs-7 text-gray-500">' + sizeText + ' •</div></div></div>' +
                        '<button class="el-sv-content-delete" data-content-id="' + (c.contentId || c.id) + '" title="Delete">' +
                        '<i class="ki-duotone ki-trash fs-5"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>' +
                        '</button></div>';
                });
            }
            $('#el-sv-content-list').html(html);
        }
    };

    /* ════════════════════════════════════════════
       COMPONENT: QuizPanel
       Fetches and renders the quiz list independently.
       Call .refresh() after delete — Content is NOT touched.
    ════════════════════════════════════════════ */
    var QuizPanel = {
        _data: [],
        load: function (mod) {
            QuizPanel._data = mod.quizzes || (mod.quiz ? [mod.quiz] : []);
            QuizPanel.render();
        },
        refresh: function () {
            app.elearning.showSkeleton('#el-sv-quiz-list', 2);
            return _api('/api/ELearning/modules/' + _moduleId).then(function (json) {
                var mod = _uw(json, {});
                _moduleData = mod;
                QuizPanel._data = mod.quizzes || (mod.quiz ? [mod.quiz] : []);
                QuizPanel.render();
            });
        },
        render: function () {
            var quizzes = QuizPanel._data;
            var html = '';
            if (!quizzes.length) {
                html = '<div class="text-gray-500 fs-7 py-4 text-center">No quiz yet. Click "Add Quiz" to create one.</div>';
            } else {
                quizzes.forEach(function (q, idx) {
                    var questionCount = q.questionCount || q.totalQuestions || q.size || 0;
                    var qId = q.quizId || q.id;
                    html += '<div class="el-sv-quiz-item el-animate el-animate-delay-' + (idx + 2) + '">' +
                        '<div class="el-sv-quiz-info">' +
                        '<div class="el-material-icon quiz me-3"><i class="ki-duotone ki-questionnaire-tablet fs-2"><span class="path1"></span><span class="path2"></span></i></div>' +
                        '<div><div class="fw-bold fs-6 text-gray-800">' + (q.name || q.title || 'Quiz') + '</div>' +
                        '<div class="fs-7 text-gray-500">' + questionCount + ' questions •</div></div></div>' +
                        '<div class="el-sv-quiz-actions">' +
                        '<button class="btn-el-view-result" data-quiz-id="' + qId + '">View Result</button>' +
                        '<button class="el-sv-quiz-delete" data-quiz-id="' + qId + '" title="Delete">' +
                        '<i class="ki-duotone ki-trash fs-5"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>' +
                        '</button></div></div>';
                });
            }
            $('#el-sv-quiz-list').html(html);
        }
    };

    /* ════════════════════════════════════════════
       PAGE CONTROLLER
    ════════════════════════════════════════════ */
    app.elearning.supervisor.moduleDetail = {

        init: function () {
            var self = this;
            _moduleId = (function () {
                var params = new URLSearchParams(window.location.search);
                return parseInt(params.get('id') || '0') || null;
            }());
            if (!_moduleId) return;

            /* Show skeletons immediately in each panel area */
            app.elearning.showSkeleton('#el-sv-content-list', 2);
            app.elearning.showSkeleton('#el-sv-quiz-list', 2);

            /* Single fetch — then each panel renders from same response independently */
            _api('/api/ELearning/modules/' + _moduleId).then(function (json) {
                if (!json) { Swal.fire({ icon: 'error', title: 'Failed to load module', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false }); return; }
                var mod = _uw(json, {});
                _moduleData = mod;

                /* Render header info */
                $('#el-sv-detail-title').text(mod.title || '');
                $('#el-sv-detail-desc').text(mod.description || '');
                var rawDate = mod.dueDate || mod.deadline || '';
                var displayDate = '-';
                if (rawDate) {
                    var d = new Date(rawDate);
                    displayDate = d.toLocaleDateString('en-GB', { day: 'numeric', month: 'long', year: 'numeric' }) + ' ' + d.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit', hour12: false });
                }
                $('#el-sv-detail-due').text('Due Date at: ' + displayDate);

                /* Each panel renders independently — no waiting for the other */
                ContentPanel.load(mod);
                QuizPanel.load(mod);

                self.bindEvents();
            });
        },

        /* Keep public surface for backward compat */
        renderContent: function () { ContentPanel.render(); },
        renderQuiz:    function () { QuizPanel.render(); },

        bindEvents: function () {
            var self = this;

        $(document).on('click', '#el-sv-add-quiz', function () {
            sessionStorage.setItem('sv_quiz_module_id', _moduleId);
            window.location.href = '/Modules/ELearning/Supervisor/QuizArchitecture';
        });

        $(document).on('click', '#el-sv-add-content', function () {
            $('#el-add-content-title').val('').removeClass('is-invalid');
            $('#el-add-content-title-err').addClass('d-none');
            $('#el-add-content-file').val('');
            $('#el-add-content-file-err').addClass('d-none');
            $('#el-sv-upload-zone').removeClass('el-sv-upload-zone--active el-sv-upload-zone--has-file');
            $('#el-sv-upload-filename').addClass('d-none').text('');
            $('#el-modal-add-content').modal('show');
        });

        $(document).on('input', '#el-add-content-title', function () {
            if ($(this).val().trim()) {
                $(this).removeClass('is-invalid');
                $('#el-add-content-title-err').addClass('d-none');
            }
        });

        $(document).on('click', '#el-add-content-submit', function () {
            var title = $('#el-add-content-title').val().trim();
            var fileInput = $('#el-add-content-file')[0];
            var file = fileInput && fileInput.files[0];
            var valid = true;
            if (!title) { $('#el-add-content-title-err').removeClass('d-none'); $('#el-add-content-title').addClass('is-invalid'); valid = false; }
            else { $('#el-add-content-title-err').addClass('d-none'); $('#el-add-content-title').removeClass('is-invalid'); }
            if (!file) { $('#el-add-content-file-err').removeClass('d-none'); valid = false; }
            else { $('#el-add-content-file-err').addClass('d-none'); }
            if (!valid) return;

            var formData = new FormData();
            formData.append('moduleId', _moduleId);
            formData.append('title', title);
            formData.append('FilePayload', file);

            app.loading && app.loading.show('Uploading...');
            _api('/api/ELearning/add-content', { method: 'POST', body: formData }).then(function (json) {
                app.loading && app.loading.hide();
                if (!json) { Swal.fire({ icon: 'error', title: 'Upload failed', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false }); return; }
                $('#el-modal-add-content').modal('hide');
                /* Only ContentPanel refreshes — QuizPanel is untouched */
                ContentPanel.refresh();
                Swal.fire({ icon: 'success', title: 'Content Added', text: '"' + title + '" has been added successfully.', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
            });
        });

        $(document).on('click', '.el-sv-content-delete', function () {
            var id = parseInt($(this).data('content-id'));
            Swal.fire({ title: 'Delete Content?', text: 'Are you sure you want to delete this content?', icon: 'warning', showCancelButton: true, confirmButtonText: 'Yes, Delete', cancelButtonText: 'Cancel', customClass: { confirmButton: 'btn btn-sm fw-bold btn-danger', cancelButton: 'btn btn-sm fw-bold btn-light' }, buttonsStyling: false })
                .then(function (result) {
                    if (!result.isConfirmed) return;
                    app.loading && app.loading.show('Deleting...');
                    _api('/api/ELearning/delete-content', { method: 'DELETE', body: JSON.stringify({ contentId: id }) }).then(function () {
                        app.loading && app.loading.hide();
                        /* Only ContentPanel refreshes — QuizPanel is untouched */
                        ContentPanel.refresh();
                        Swal.fire({ icon: 'success', title: 'Content Deleted', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
                    });
                });
        });

        $(document).on('click', '.el-sv-quiz-delete', function () {
            var id = parseInt($(this).data('quiz-id'));
            Swal.fire({ title: 'Delete Quiz?', text: 'Are you sure you want to delete this quiz?', icon: 'warning', showCancelButton: true, confirmButtonText: 'Yes, Delete', cancelButtonText: 'Cancel', customClass: { confirmButton: 'btn btn-sm fw-bold btn-danger', cancelButton: 'btn btn-sm fw-bold btn-light' }, buttonsStyling: false })
                .then(function (result) {
                    if (!result.isConfirmed) return;
                    app.loading && app.loading.show('Deleting...');
                    _api('/api/ELearning/delete-quiz', { method: 'DELETE', body: JSON.stringify({ quizId: id }) }).then(function () {
                        app.loading && app.loading.hide();
                        /* Only QuizPanel refreshes — ContentPanel is untouched */
                        QuizPanel.refresh();
                        Swal.fire({ icon: 'success', title: 'Quiz Deleted', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
                    });
                });
        });

        $(document).on('click', '#el-sv-upload-zone', function (e) {
            if (!$(e.target).is('input')) { $('#el-add-content-file').trigger('click'); }
        });

        $(document).on('change', '#el-add-content-file', function () {
            var file = this.files[0];
            if (file) { app.elearning.supervisor.moduleDetail._setUploadFile(file); }
        });

        $(document).on('dragover dragenter', '#el-sv-upload-zone', function (e) {
            e.preventDefault(); e.stopPropagation();
            $(this).addClass('el-sv-upload-zone--active');
        });

        $(document).on('dragleave drop', '#el-sv-upload-zone', function (e) {
            e.preventDefault(); e.stopPropagation();
            $(this).removeClass('el-sv-upload-zone--active');
            if (e.type === 'drop') {
                var file = e.originalEvent.dataTransfer.files[0];
                if (file) { app.elearning.supervisor.moduleDetail._setUploadFile(file); }
            }
        });

        $(document).on('click', '.btn-el-view-result', function () {
            var quizId = $(this).data('quiz-id');
            window.location.href = '/Modules/ELearning/Supervisor/Results?quizId=' + quizId;
        });

        $(document).on('click', '#el-sv-add-quiz', function () {
            sessionStorage.setItem('sv_quiz_module_id', _moduleId);
            window.location.href = '/Modules/ELearning/Supervisor/QuizArchitecture';
        });
        }
    };

    app.elearning.supervisor.moduleDetail._setUploadFile = function (file) {
        $('#el-sv-upload-zone').addClass('el-sv-upload-zone--has-file');
        $('#el-sv-upload-filename').removeClass('d-none').text(file.name + ' (' + (file.size / (1024 * 1024)).toFixed(1) + ' MB)');
        $('#el-add-content-file-err').addClass('d-none');
    };

})(jQuery, window.app = window.app || {});
