(function ($, app) {
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    app.elearning.modules = {};
    app.elearning.modules._moduleData = null;

    app.elearning.modules._api = async function (path, opts) {
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

    app.elearning.modules._getModuleId = function () {
        var params = new URLSearchParams(window.location.search);
        return parseInt(params.get('id') || '0') || null;
    };

    app.elearning.modules.init = function () {
        var self = this;
        var moduleId = self._getModuleId();
        if (!moduleId) return;

        var user = window.aiaAuth && window.aiaAuth.getUserInfo();
        var userId = user ? (user.EmployeeId || user.employeeId || user.sub || user.id) : null;
        var qs = userId ? '?userId=' + userId : '';

        app.loading && app.loading.show('Loading module...');
        self._api('/api/ELearning/modules/' + moduleId + qs).then(function (json) {
            app.loading && app.loading.hide();
            if (!json) {
                Swal.fire({ icon: 'error', title: 'Failed to load module' });
                return;
            }
            var mod = json.content || json.data || json;
            self._moduleData = mod;
            self.renderModuleDetail(mod);
            self.bindEvents(mod);
        });
    };

    app.elearning.modules.renderModuleDetail = function (mod) {
        $('#el-module-title').text(mod.title || '');
        $('#el-module-due').text('Due Date at: ' + (mod.dueDate || mod.deadline || '-'));

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
                    '<div class="el-material-icon quiz me-3">' +
                    '<i class="ki-duotone ki-questionnaire-tablet fs-2"><span class="path1"></span><span class="path2"></span></i>' +
                    '</div>' +
                    '<div>' +
                    '<div class="fw-bold fs-5 text-gray-800">Quiz ' + (idx + 1) + '</div>' +
                    '<div class="fs-7 text-gray-500">' + (quiz.totalQuestions || quiz.questionCount || 0) + ' Questions</div>' +
                    '</div>' +
                    '</div>' +
                    '<div class="d-flex align-items-center">' +
                    scoreText +
                    '<button class="btn-el-primary el-start-quiz" data-quiz-id="' + (quiz.quizId || quiz.id) + '">Start Quiz</button>' +
                    '</div>' +
                    '</div>';
            });
        }
        $('#el-quizzes-list').html(quizHtml);

        var materials = mod.materials || mod.contents || [];
        var html = '';

        materials.forEach(function (mat, idx) {
            var isCompleted = mat.completed || mat.isCompleted || false;
            var checkHtml = isCompleted ? '<div class="el-material-check"><i class="ki-duotone ki-check fs-8"></i></div>' : '';

            var ext = (mat.fileName || mat.name || '').split('.').pop().toLowerCase();
            var iconClass = 'pdf';
            if (ext === 'ppt' || ext === 'pptx') iconClass = 'slides';
            else if (ext === 'mp4' || ext === 'mov' || ext === 'avi' || ext === 'mkv') iconClass = 'video';

            var iconMap = {
                pdf: '<i class="ki-duotone ki-document fs-2"><span class="path1"></span><span class="path2"></span></i>',
                slides: '<i class="ki-duotone ki-some-files fs-2"><span class="path1"></span><span class="path2"></span></i>',
                video: '<i class="ki-duotone ki-screen fs-2"><span class="path1"></span><span class="path2"></span></i>'
            };

            var sizeText = mat.size || mat.fileSize || '';
            if (!sizeText && mat.fileSizeBytes) {
                var kb = mat.fileSizeBytes / 1024;
                sizeText = kb >= 1024 ? (kb / 1024).toFixed(1) + ' MB' : Math.round(kb) + ' KB';
            }

            html += '<div class="el-material-item el-animate el-animate-delay-' + (idx + 1) + '" data-content-id="' + (mat.contentId || mat.id) + '">' +
                checkHtml +
                '<div class="el-material-icon ' + iconClass + '">' + (iconMap[iconClass] || iconMap.pdf) + '</div>' +
                '<div>' +
                '<div class="fw-bold fs-6 text-gray-800">' + (mat.title || mat.name || '') + '</div>' +
                '<div class="fs-7 text-gray-500">' + sizeText + '</div>' +
                '</div>' +
                '</div>';
        });

        $('#el-materials-list').html(html || '<div class="text-gray-500 fs-7 text-center py-3">No materials available.</div>');
    };

    app.elearning.modules.bindEvents = function () {
        var self = this;
        var user = window.aiaAuth && window.aiaAuth.getUserInfo();
        var userId = user ? (user.EmployeeId || user.employeeId || user.sub || user.id) : null;

        $(document).on('click', '.el-material-item', function () {
            var $item = $(this);
            var contentId = $item.data('content-id');

            if (userId && contentId) {
                self._api('/api/ELearning/mark-opened', {
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
            var mod = self._moduleData || {};
            var quizzes = mod.quizzes || (mod.quiz ? [mod.quiz] : []);
            var quiz = quizzes.find(function(q) { return (q.quizId || q.id) === quizId; }) || {};

            var user = window.aiaAuth && window.aiaAuth.getUserInfo();
            var userId = user ? (user.EmployeeId || user.employeeId || user.sub || user.id) : null;
            sessionStorage.setItem('intern_quiz_context', JSON.stringify({
                moduleId: mod.moduleId || mod.id || null,
                moduleTitle: mod.title || mod.moduleTitle || 'Quiz',
                quizId: quiz.quizId || quiz.id || null,
                questions: quiz.questions || []
            }));
            window.location.href = '/Modules/ELearning/Intern/Quiz';
        });
    };

})(jQuery, window.app = window.app || {});


