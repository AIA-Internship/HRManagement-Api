    (function ($, app) {
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    app.elearning.quiz = {};
    app.elearning.quiz.currentIndex = 0;
    app.elearning.quiz.answers = {};
    app.elearning.quiz._questions = [];
    app.elearning.quiz._context = null;

    app.elearning.quiz._api = async function (path, opts) {
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

    app.elearning.quiz.init = function () {
        var self = this;
        self.currentIndex = 0;
        self.answers = {};

        try {
            var ctx = JSON.parse(sessionStorage.getItem('intern_quiz_context') || '{}');
            self._context = ctx;
            self._questions = ctx.questions || [];
            
            var title = ctx.moduleTitle || 'Quiz';
            $('.fs-2.fw-bold.text-gray-900').text(title);
            
            var moduleId = ctx.moduleId || '';
            if (moduleId) {
                $('.el-back-btn').attr('href', '/Modules/ELearning/Intern/Modules?id=' + moduleId);
            }
        } catch (e) {
            self._questions = [];
        }

        if (!self._questions.length) {
            $('#el-quiz-question').html('<div class="text-gray-500 text-center py-4">No questions available.</div>');
            return;
        }

        self.renderQuestion();
        self.bindEvents();
    };

    app.elearning.quiz.renderQuestion = function () {
        var questions = this._questions;
        var q = questions[this.currentIndex];
        var total = questions.length;
        var num = this.currentIndex + 1;

        $('#el-quiz-badge').text(num);

        var qHtml = '<div class="el-quiz-question-card el-animate">' +
            '<div class="fs-5 fw-semibold text-gray-800">' +
            '<span class="fw-bold">' + num + '.</span> ' + (q.text || q.question || '') +
            '</div>' +
            '</div>';
        $('#el-quiz-question').html(qHtml);

        var ansHtml = '';
        var qType = (q.type || '').toLowerCase();
        if (qType === 'multiple-choice' || qType === 'mc' || qType === 'multiplechoice') {
            var savedAnswer = this.answers[q.id] || null;
            var options = q.options || [q.optionA, q.optionB, q.optionC, q.optionD].filter(Boolean);
            ansHtml = '<div class="row g-3">';
            options.forEach(function (opt, idx) {
                var selectedClass = savedAnswer === opt ? ' selected' : '';
                ansHtml += '<div class="col-md-6">' +
                    '<div class="el-quiz-option el-animate el-animate-delay-' + (idx + 1) + selectedClass + '" data-option="' + opt + '">' +
                    opt +
                    '</div>' +
                    '</div>';
            });
            ansHtml += '</div>';
        } else if (qType === 'essay') {
            var savedText = this.answers[q.id] || '';
            ansHtml = '<textarea class="el-quiz-textarea el-animate el-animate-delay-1" id="el-essay-answer" placeholder="Write your answer here">' + savedText + '</textarea>';
        }
        $('#el-quiz-answer').html(ansHtml);

        var navHtml = '<div class="d-flex justify-content-between align-items-center mt-4">';
        if (this.currentIndex > 0) {
            navHtml += '<button class="btn-el-modal-cancel" id="el-quiz-prev"><i class="ki-duotone ki-arrow-left fs-5"><span class="path1"></span><span class="path2"></span></i> Prev</button>';
        } else {
            navHtml += '<div></div>';
        }
        if (this.currentIndex < total - 1) {
            navHtml += '<button class="btn-el-primary" id="el-quiz-next">Next <i class="ki-duotone ki-arrow-right fs-5"><span class="path1"></span><span class="path2"></span></i></button>';
        } else {
            navHtml += '<button class="btn-el-primary" id="el-quiz-finish">Finish</button>';
        }
        navHtml += '</div>';
        $('#el-quiz-nav').html(navHtml);
    };

    app.elearning.quiz.saveCurrentAnswer = function () {
        var q = this._questions[this.currentIndex];
        var qType = (q.type || '').toLowerCase();
        if (qType === 'essay') {
            this.answers[q.id] = $('#el-essay-answer').val();
        }
    };

    app.elearning.quiz.bindEvents = function () {
        var self = this;

        $(document).on('click', '.el-quiz-option', function () {
            var q = self._questions[self.currentIndex];
            var opt = $(this).data('option');
            self.answers[q.id] = opt;
            $('.el-quiz-option').removeClass('selected');
            $(this).addClass('selected');
        });

        $(document).on('click', '#el-quiz-next', function () {
            self.saveCurrentAnswer();
            if (self.currentIndex < self._questions.length - 1) {
                self.currentIndex++;
                self.renderQuestion();
            }
        });

        $(document).on('click', '#el-quiz-prev', function () {
            self.saveCurrentAnswer();
            if (self.currentIndex > 0) {
                self.currentIndex--;
                self.renderQuestion();
            }
        });

        $(document).on('click', '#el-quiz-finish', function () {
            self.saveCurrentAnswer();

            Swal.fire({
                title: 'Submit Quiz?',
                text: 'Are you sure you want to submit your answers?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, Submit!',
                cancelButtonText: 'Cancel',
                customClass: {
                    popup: 'el-swal',
                    confirmButton: 'btn btn-sm fw-bold btn-danger mx-2 w-125px',
                    cancelButton: 'btn btn-sm fw-bold btn-light mx-2 w-125px'
                },
                buttonsStyling: false
            }).then(function (result) {
                if (!result.isConfirmed) return;

                var user = window.aiaAuth && window.aiaAuth.getUserInfo();
                var userIdRaw = user ? (user.EmployeeId || user.employeeId || user.sub || user.id) : null;
                var ctx = self._context || {};
                var answersPayload = Object.keys(self.answers).map(function (qId) {
                    var q = self._questions.find(function(x) { return x.id == qId; });
                    var qType = (q && q.type ? q.type.toLowerCase() : '');
                    var item = { questionId: parseInt(qId) };
                    if (qType === 'essay') {
                        item.essayAnswerText = self.answers[qId];
                    } else {
                        item.selectedOption = self.answers[qId];
                    }
                    return item;
                });

                var payload = {
                    userId: userIdRaw ? parseInt(userIdRaw) : null,
                    moduleId: ctx.moduleId || null,
                    quizId: ctx.quizId || null,
                    answers: answersPayload
                };

                app.loading && app.loading.show('Submitting answers...');
                self._api('/api/ELearning/submit-quiz', {
                    method: 'POST',
                    body: JSON.stringify(payload)
                }).then(function (json) {
                    app.loading && app.loading.hide();
                    if (!json) {
                        Swal.fire({ icon: 'error', title: 'Submission failed', text: 'Please try again.', customClass: { popup: 'el-swal', confirmButton: 'btn-el-primary' }, buttonsStyling: false });
                        return;
                    }
                    Swal.fire({
                        icon: 'success',
                        title: 'Quiz Submitted!',
                        text: 'Your answers have been recorded successfully.',
                        customClass: { popup: 'el-swal', confirmButton: 'btn-el-primary' },
                        buttonsStyling: false
                    }).then(function () {
                        sessionStorage.removeItem('intern_quiz_context');
                        window.location.href = '/Modules/ELearning/Intern/Modules?id=' + (ctx.moduleId || '');
                    });
                });
            });
        });
    };

})(jQuery, window.app = window.app || {});


