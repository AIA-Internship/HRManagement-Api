(function ($, app) {
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    app.elearning.supervisor.moduleDetail = {};
    app.elearning.supervisor.moduleDetail._moduleData = null;

    app.elearning.supervisor.moduleDetail._api = async function (path, opts) {
        var token = window.aiaAuth && window.aiaAuth.getToken();
        if (!token) { window.aiaAuth && window.aiaAuth.signOut(); return null; }
        opts = opts || {};
        var isFormData = opts.body instanceof FormData;
        var headers = { 'Authorization': 'Bearer ' + token };
        if (!isFormData) headers['Content-Type'] = 'application/json';
        opts.headers = $.extend(headers, opts.headers || {});
        try {
            var res = await fetch('https://localhost:7089' + path, opts);
            if (res.status === 401) { window.aiaAuth.signOut(); return null; }
            if (res.status === 404) return null;
            if (!res.ok) return null;
            var json = await res.json().catch(function () { return {}; });
            if (json && json.isError) return null;
            return json;
        } catch (err) {
            console.error('API Error:', err);
            return null;
        }
    };

    app.elearning.supervisor.moduleDetail._getModuleId = function () {
        var params = new URLSearchParams(window.location.search);
        return parseInt(params.get('id') || '0') || null;
    };

    app.elearning.supervisor.moduleDetail._getIconClass = function (filename) {
        var ext = (filename || '').split('.').pop().toLowerCase();
        if (ext === 'pdf') return 'pdf';
        if (ext === 'ppt' || ext === 'pptx') return 'slides';
        if (ext === 'mp4' || ext === 'mov' || ext === 'avi') return 'video';
        return 'pdf';
    };

    app.elearning.supervisor.moduleDetail.init = function () {
        var self = this;
        var moduleId = self._getModuleId();
        if (!moduleId) return;

        app.loading && app.loading.show('Loading module...');
        self._api('/api/ELearning/modules/' + moduleId).then(function (json) {
            app.loading && app.loading.hide();
            if (!json) {
                Swal.fire({ icon: 'error', title: 'Failed to load module' });
                return;
            }
            var mod = json.content || json.data || json;
            self._moduleData = mod;
            self.renderContent(mod);
            self.renderQuiz(mod);
            self.bindEvents(mod);
        });
    };

    app.elearning.supervisor.moduleDetail.renderContent = function (mod) {
        mod = mod || this._moduleData || {};
        $('#el-sv-detail-title').text(mod.title || '');
        $('#el-sv-detail-desc').text(mod.description || '');
        var rawDate = mod.dueDate || mod.deadline || '';
        var displayDate = '-';
        if (rawDate) {
            var d = new Date(rawDate);
            var dateStr = d.toLocaleDateString('en-GB', { day: 'numeric', month: 'long', year: 'numeric' });
            var timeStr = d.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit', hour12: false });
            displayDate = dateStr + ' ' + timeStr;
        }
        $('#el-sv-detail-due').text('Due Date at: ' + displayDate);

        var contents = mod.contents || mod.materials || [];
        var html = '';
        var iconMap = {
            pdf: '<i class="ki-duotone ki-document fs-2"><span class="path1"></span><span class="path2"></span></i>',
            slides: '<i class="ki-duotone ki-some-files fs-2"><span class="path1"></span><span class="path2"></span></i>',
            video: '<i class="ki-duotone ki-screen fs-2"><span class="path1"></span><span class="path2"></span></i>'
        };

        if (!contents.length) {
            html = '<div class="text-gray-500 fs-7 py-4 text-center">No content yet. Click "Add Content" to upload.</div>';
        } else {
            contents.forEach(function (c, idx) {
                var iconClass = c.iconClass || app.elearning.supervisor.moduleDetail._getIconClass(c.fileName || c.name || '');
                var sizeText = c.size || c.fileSize || '';
                if (!sizeText && c.fileSizeBytes) {
                    var kb = c.fileSizeBytes / 1024;
                    sizeText = kb >= 1024 ? (kb / 1024).toFixed(1) + ' MB' : Math.round(kb) + ' KB';
                }
                html += '<div class="el-sv-content-item el-animate el-animate-delay-' + (idx + 1) + '">' +
                    '<div class="el-sv-content-info">' +
                    '<div class="el-material-icon ' + iconClass + ' me-3">' + (iconMap[iconClass] || iconMap.pdf) + '</div>' +
                    '<div>' +
                    '<div class="fw-bold fs-6 text-gray-800">' + (c.title || c.name || '') + '</div>' +
                    '<div class="fs-7 text-gray-500">' + sizeText + ' •</div>' +
                    '</div>' +
                    '</div>' +
                    '<button class="el-sv-content-delete" data-content-id="' + (c.contentId || c.id) + '" title="Delete">' +
                    '<i class="ki-duotone ki-trash fs-5"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>' +
                    '</button>' +
                    '</div>';
            });
        }
        $('#el-sv-content-list').html(html);
    };

    app.elearning.supervisor.moduleDetail.renderQuiz = function (mod) {
        mod = mod || this._moduleData || {};
        var quizzes = mod.quizzes || (mod.quiz ? [mod.quiz] : []);
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
                    '<div>' +
                    '<div class="fw-bold fs-6 text-gray-800">' + (q.name || q.title || 'Quiz') + '</div>' +
                    '<div class="fs-7 text-gray-500">' + questionCount + ' questions •</div>' +
                    '</div>' +
                    '</div>' +
                    '<div class="el-sv-quiz-actions">' +
                    '<button class="btn-el-view-result" data-quiz-id="' + qId + '">View Result</button>' +
                    '<button class="el-sv-quiz-delete" data-quiz-id="' + qId + '" title="Delete">' +
                    '<i class="ki-duotone ki-trash fs-5"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>' +
                    '</button>' +
                    '</div>' +
                    '</div>';
            });
        }
        $('#el-sv-quiz-list').html(html);
    };

    app.elearning.supervisor.moduleDetail.bindEvents = function (mod) {
        var self = this;
        var moduleId = mod ? (mod.moduleId || mod.id) : self._getModuleId();

        $(document).on('click', '#el-sv-add-quiz', function () {
            sessionStorage.setItem('sv_quiz_module_id', moduleId);
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

            if (!title) {
                $('#el-add-content-title-err').removeClass('d-none');
                $('#el-add-content-title').addClass('is-invalid');
                valid = false;
            } else {
                $('#el-add-content-title-err').addClass('d-none');
                $('#el-add-content-title').removeClass('is-invalid');
            }
            if (!file) {
                $('#el-add-content-file-err').removeClass('d-none');
                valid = false;
            } else {
                $('#el-add-content-file-err').addClass('d-none');
            }
            if (!valid) return;

            var formData = new FormData();
            formData.append('moduleId', moduleId);
            formData.append('title', title);
            formData.append('FilePayload', file);

            app.loading && app.loading.show('Uploading...');
            self._api('/api/ELearning/add-content', { method: 'POST', body: formData }).then(function (json) {
                app.loading && app.loading.hide();
                if (!json) { Swal.fire({ icon: 'error', title: 'Upload failed' }); return; }
                $('#el-modal-add-content').modal('hide');
                var newContent = json.content || json.data || json;
                if (self._moduleData) {
                    if (!self._moduleData.contents) self._moduleData.contents = [];
                    if (newContent && newContent.id) self._moduleData.contents.push(newContent);
                }
                self.renderContent();
                Swal.fire({ icon: 'success', title: 'Content Added', text: '"' + title + '" has been added successfully.', customClass: { confirmButton: 'btn btn-sm fw-bold btn-primary' }, buttonsStyling: false });
            });
        });

        $(document).on('click', '.el-sv-content-delete', function () {
            var id = parseInt($(this).data('content-id'));
            Swal.fire({
                title: 'Delete Content?',
                text: 'Are you sure you want to delete this content?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, Delete',
                cancelButtonText: 'Cancel',
                customClass: { confirmButton: 'btn btn-sm fw-bold btn-danger', cancelButton: 'btn btn-sm fw-bold btn-light' },
                buttonsStyling: false
            }).then(function (result) {
                if (!result.isConfirmed) return;
                app.loading && app.loading.show('Deleting...');
                self._api('/api/ELearning/delete-content', { method: 'DELETE', body: JSON.stringify({ contentId: id }) }).then(function (json) {
                    app.loading && app.loading.hide();
                    if (self._moduleData && self._moduleData.contents) {
                        self._moduleData.contents = self._moduleData.contents.filter(function (c) { return (c.contentId || c.id) !== id; });
                    }
                    self.renderContent();
                    Swal.fire({ icon: 'success', title: 'Content Deleted', timer: 1200, showConfirmButton: false });
                });
            });
        });

        $(document).on('click', '.el-sv-quiz-delete', function () {
            var id = parseInt($(this).data('quiz-id'));
            Swal.fire({
                title: 'Delete Quiz?',
                text: 'Are you sure you want to delete this quiz?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, Delete',
                cancelButtonText: 'Cancel',
                customClass: { confirmButton: 'btn btn-sm fw-bold btn-danger', cancelButton: 'btn btn-sm fw-bold btn-light' },
                buttonsStyling: false
            }).then(function (result) {
                if (!result.isConfirmed) return;
                app.loading && app.loading.show('Deleting...');
                self._api('/api/ELearning/delete-quiz', { method: 'DELETE', body: JSON.stringify({ quizId: id }) }).then(function () {
                    app.loading && app.loading.hide();
                    if (self._moduleData && self._moduleData.quizzes) {
                        self._moduleData.quizzes = self._moduleData.quizzes.filter(function (q) { return (q.quizId || q.id) !== id; });
                    } else if (self._moduleData && self._moduleData.quiz && (self._moduleData.quiz.quizId || self._moduleData.quiz.id) === id) {
                        self._moduleData.quiz = null;
                    }
                    self.renderQuiz();
                    Swal.fire({ icon: 'success', title: 'Quiz Deleted', timer: 1200, showConfirmButton: false });
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
    };

    app.elearning.supervisor.moduleDetail._setUploadFile = function (file) {
        $('#el-sv-upload-zone').addClass('el-sv-upload-zone--has-file');
        $('#el-sv-upload-filename').removeClass('d-none').text(file.name + ' (' + (file.size / (1024 * 1024)).toFixed(1) + ' MB)');
        $('#el-add-content-file-err').addClass('d-none');
    };

})(jQuery, window.app = window.app || {});


