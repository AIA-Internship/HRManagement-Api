/**
 * intern/modules.js — Component-Based Architecture
 *
 * MaterialsPanel  — renders materials list independently
 * QuizListPanel   — renders quiz list independently
 *
 * Single fetch provides data for both panels. Each panel renders
 * as soon as the response arrives, independently.
 */
(function ($, app) {
    'use strict';
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    /* ── Shared API shorthand ── */
    var _api = function (path, opts) { return app.elearning.api(path, opts); };
    var _uw  = function (json, fb)   { return app.elearning.unwrap(json, fb); };

    var _moduleData = null;

    /* Icon helpers */
    var _iconMap = {
        pdf:    '<i class="ki-duotone ki-document fs-2"><span class="path1"></span><span class="path2"></span></i>',
        slides: '<i class="ki-duotone ki-some-files fs-2"><span class="path1"></span><span class="path2"></span></i>',
        video:  '<i class="ki-duotone ki-screen fs-2"><span class="path1"></span><span class="path2"></span></i>'
    };

    /* ════════════════════════════════════════════
       COMPONENT: MaterialsPanel
       Renders materials list from module data.
    ════════════════════════════════════════════ */
    var MaterialsPanel = {
        render: function (mod) {
            var materials = mod.materials || mod.contents || [];
            var html = '';
            materials.forEach(function (mat, idx) {
                var isCompleted = mat.completed || mat.isCompleted || false;
                var checkHtml = isCompleted ? '<div class="el-material-check"><i class="ki-duotone ki-check fs-8"></i></div>' : '';
                var ext = (mat.fileName || mat.name || '').split('.').pop().toLowerCase();
                var iconClass = 'pdf';
                if (ext === 'ppt' || ext === 'pptx') iconClass = 'slides';
                else if (ext === 'mp4' || ext === 'mov' || ext === 'avi' || ext === 'mkv') iconClass = 'video';
                var sizeText = mat.size || mat.fileSize || '';
                if (!sizeText && mat.fileSizeBytes) { var kb = mat.fileSizeBytes / 1024; sizeText = kb >= 1024 ? (kb / 1024).toFixed(1) + ' MB' : Math.round(kb) + ' KB'; }
                html += '<div class="el-material-item el-animate el-animate-delay-' + (idx + 1) + '" data-content-id="' + (mat.contentId || mat.id) + '">' +
                    checkHtml +
                    '<div class="el-material-icon ' + iconClass + '">' + (_iconMap[iconClass] || _iconMap.pdf) + '</div>' +
                    '<div><div class="fw-bold fs-6 text-gray-800">' + (mat.title || mat.name || '') + '</div>' +
                    '<div class="fs-7 text-gray-500">' + sizeText + '</div></div></div>';
            });
            $('#el-materials-list').html(html || '<div class="text-gray-500 fs-7 text-center py-3">No materials available.</div>');
        }
    };

    /* ════════════════════════════════════════════
       COMPONENT: QuizListPanel
       Renders quiz list from module data.
    ════════════════════════════════════════════ */
    var QuizListPanel = {
        render: function (mod) {
            var quizzes = mod.quizzes || (mod.quiz ? [mod.quiz] : []);
            var quizHtml = '';
            if (!quizzes.length) {
                quizHtml = '<div class="text-gray-500 fs-7 py-4 text-center">No quizzes available for this module.</div>';
            } else {
                quizzes.forEach(function (quiz, idx) {
                    var scoreText = (quiz.latestScore !== undefined && quiz.latestScore !== null)
                        ? '<span class="fw-bold fs-5 text-gray-800 me-4">Score: ' + quiz.latestScore.toFixed(0) + '</span>'
                        : '<span class="fw-bold fs-5 text-gray-800 me-4 d-none">Score: -</span>';
                    quizHtml += '<div class="el-quiz-card el-animate el-animate-delay-' + (idx + 3) + ' d-flex justify-content-between align-items-center mb-4">' +
                        '<div class="d-flex align-items-center">' +
                        '<div class="el-material-icon quiz me-3"><i class="ki-duotone ki-questionnaire-tablet fs-2"><span class="path1"></span><span class="path2"></span></i></div>' +
                        '<div><div class="fw-bold fs-5 text-gray-800">Quiz ' + (idx + 1) + '</div>' +
                        '<div class="fs-7 text-gray-500">' + (quiz.totalQuestions || quiz.questionCount || 0) + ' Questions</div></div></div>' +
                        '<div class="d-flex align-items-center">' + scoreText +
                        '<button class="btn-el-primary el-start-quiz" data-quiz-id="' + (quiz.quizId || quiz.id) + '">Start Quiz</button></div></div>';
                });
            }
            $('#el-quizzes-list').html(quizHtml);
        }
    };

    /* ════════════════════════════════════════════
       PAGE CONTROLLER
    ════════════════════════════════════════════ */
    app.elearning.modules = {

        init: function () {
            var self = this;
            var params = new URLSearchParams(window.location.search);
            var moduleId = parseInt(params.get('id') || '0') || null;
            if (!moduleId) {
                window.location.href = '/Modules/ELearning/Intern/Dashboard';
                return;
            }

            var user = window.aiaAuth && window.aiaAuth.getUserInfo();
            var userId = user ? (user.EmployeeId || user.employeeId || user.sub || user.id) : null;
            var qs = userId ? '?userId=' + userId : '';

            /* Show skeletons in each panel immediately */
            app.elearning.showSkeleton('#el-materials-list', 3);
            app.elearning.showSkeleton('#el-quizzes-list', 2);

            _api('/api/ELearning/modules/' + moduleId + qs).then(function (json) {
                if (!json) { Swal.fire({ icon: 'error', title: 'Failed to load module', customClass: { popup: 'el-swal', confirmButton: 'btn-el-primary' }, buttonsStyling: false }); return; }
                var mod = _uw(json, {});
                _moduleData = mod;

                /* Render page header */
                $('#el-module-title').text(mod.title || '');
                $('#el-module-desc').text(mod.description || '');
                var rawDate = mod.dueDate || mod.deadline || '';
                var displayDate = '-';
                if (rawDate) { var d = new Date(rawDate); displayDate = d.toLocaleDateString('en-GB', { day: 'numeric', month: 'long', year: 'numeric' }) + ' ' + d.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit', hour12: false }); }
                $('#el-module-due').text('Due Date at: ' + displayDate);

                /* Each panel renders independently from same data */
                MaterialsPanel.render(mod);
                QuizListPanel.render(mod);

                self.bindEvents();
            });
        },




        bindEvents: function () {
            var user = window.aiaAuth && window.aiaAuth.getUserInfo();
            var userId = user ? (user.EmployeeId || user.employeeId || user.sub || user.id) : null;

        $(document).on('click', '.el-material-item', function () {
            var $item = $(this);
            var contentId = $item.data('content-id');

            if (userId && contentId) {
                _api('/api/ELearning/mark-opened', {
                    method: 'POST',
                    body: JSON.stringify({ userId: parseInt(userId), contentId: parseInt(contentId) })
                }).then(function (res) {
                    if (res && !$item.find('.el-material-check').length) {
                        $item.prepend('<div class="el-material-check"><i class="ki-duotone ki-check fs-8"></i></div>');
                    }
                });
            }

            var token = window.aiaAuth && window.aiaAuth.getToken();
            var downloadUrl = 'https://localhost:7089/api/ELearning/content/' + contentId + '/download';
            var link = document.createElement('a');
            link.href = downloadUrl + '?access_token=' + encodeURIComponent(token);
            link.target = '_blank';
            link.rel = 'noopener';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        });

        $(document).on('click', '.el-start-quiz', function () {
            var quizId = $(this).data('quiz-id');
            var mod = _moduleData || {};
            var quizzes = mod.quizzes || (mod.quiz ? [mod.quiz] : []);
            var quiz = quizzes.find(function(q) { return (q.quizId || q.id) === quizId; }) || {};

            var user = window.aiaAuth && window.aiaAuth.getUserInfo();
            var userId = user ? (user.EmployeeId || user.employeeId || user.sub || user.id) : null;
            var params = new URLSearchParams(window.location.search);
            var urlModuleId = parseInt(params.get('id') || '0') || null;
            sessionStorage.setItem('intern_quiz_context', JSON.stringify({
                moduleId: mod.moduleId || mod.id || urlModuleId || null,
                moduleTitle: mod.title || mod.moduleTitle || 'Quiz',
                quizId: quiz.quizId || quiz.id || null,
                questions: quiz.questions || []
            }));
            window.location.href = '/Modules/ELearning/Intern/Quiz';
        });
        }
    };

})(jQuery, window.app = window.app || {});
