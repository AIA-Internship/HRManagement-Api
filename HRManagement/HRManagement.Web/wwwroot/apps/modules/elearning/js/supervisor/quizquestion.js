(function ($, app) {
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    app.elearning.supervisor.quizQuestion = {};
    app.elearning.supervisor.quizQuestion.currentIndex = 0;
    app.elearning.supervisor.quizQuestion.questions = [];

    app.elearning.supervisor.quizQuestion._api = async function (path, opts) {
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

    app.elearning.supervisor.quizQuestion.init = function () {
        var self = this;
        try {
            var cfg = JSON.parse(sessionStorage.getItem('sv_quiz_config') || '{}');
            self.questions = cfg.questions || [];
        } catch (e) {
            self.questions = [];
        }

        if (!self.questions.length) {
            for (var i = 1; i <= 3; i++) {
                self.questions.push({ type: 'mc', num: i, text: 'Sample MC Question ' + i });
            }
        }

        self.currentIndex = 0;
        self.renderQuestion();
        self.bindEvents();
    };

    app.elearning.supervisor.quizQuestion.renderQuestion = function () {
        var q = this.questions[this.currentIndex];
        var total = this.questions.length;
        var isMC = (q.type || '').toLowerCase() === 'mc';

        var mcTotal = this.questions.filter(function (x) { return x.type === 'mc'; }).length;
        var essayTotal = this.questions.filter(function (x) { return x.type === 'essay'; }).length;

        var stepLabel = '';
        if (isMC) {
            stepLabel = '<span class="badge me-2" style="background:#f1416c;color:#fff;font-size:11px;border-radius:20px;padding:4px 12px;">Multiple Choice</span>' +
                '<span class="fs-7 text-gray-500">' + q.num + ' / ' + mcTotal + '</span>';
        } else {
            stepLabel = '<span class="badge me-2" style="background:#1bc5bd;color:#fff;font-size:11px;border-radius:20px;padding:4px 12px;">Essay</span>' +
                '<span class="fs-7 text-gray-500">' + q.num + ' / ' + essayTotal + '</span>';
        }
        $('#el-sv-qq-step-label').html(stepLabel);

        $('#el-sv-qq-num').text(q.num);
        $('#el-sv-qq-type-label').text((isMC ? 'MC' : 'Essay') + ' ' + q.num);
        $('#el-sv-qq-text').val(q.text || '');

        if (isMC) {
            $('#el-sv-qq-options-section').show();
            $('#el-sv-qq-essay-note').addClass('d-none');
            $('#el-sv-qq-opt-a, #el-sv-qq-opt-b, #el-sv-qq-opt-c, #el-sv-qq-opt-d').val('');
            $('.el-sv-qq-checkbox').prop('checked', false);
        } else {
            $('#el-sv-qq-options-section').hide();
            $('#el-sv-qq-essay-note').removeClass('d-none');
        }

        var isLast = this.currentIndex >= total - 1;
        $('#el-sv-qq-next').html(
            isLast
                ? 'Finish <i class="ki-duotone ki-check fs-5"><span class="path1"></span><span class="path2"></span></i>'
                : 'Next <i class="ki-duotone ki-arrow-right fs-5"><span class="path1"></span><span class="path2"></span></i>'
        );
    };

    app.elearning.supervisor.quizQuestion.saveCurrentAnswer = function () {
        var q = this.questions[this.currentIndex];
        if ((q.type || '').toLowerCase() === 'mc') {
            q.savedText = $('#el-sv-qq-text').val();
            q.optionA = $('#el-sv-qq-opt-a').val();
            q.optionB = $('#el-sv-qq-opt-b').val();
            q.optionC = $('#el-sv-qq-opt-c').val();
            q.optionD = $('#el-sv-qq-opt-d').val();
            q.correctOption = $('input.el-sv-qq-checkbox:checked').data('option') || '';
        } else {
            q.savedText = $('#el-sv-qq-text').val();
        }
    };

    app.elearning.supervisor.quizQuestion.bindEvents = function () {
        var self = this;

        $(document).on('change', '.el-sv-qq-checkbox', function () {
            if ($(this).is(':checked')) {
                $('.el-sv-qq-checkbox').not(this).prop('checked', false);
            }
        });

        $(document).on('click', '#el-sv-qq-next', function () {
            self.saveCurrentAnswer();

            if (self.currentIndex < self.questions.length - 1) {
                self.currentIndex++;
                self.renderQuestion();
                window.scrollTo({ top: 0, behavior: 'smooth' });
            } else {
                var saved = JSON.parse(sessionStorage.getItem('sv_quiz_config') || '{}');
                saved.questions = self.questions;
                sessionStorage.setItem('sv_quiz_config', JSON.stringify(saved));

                var moduleId = parseInt(sessionStorage.getItem('sv_quiz_module_id') || '0') || null;
                var payload = {
                    moduleId: moduleId,
                    mcCount: saved.mcCount || 0,
                    essayCount: saved.essayCount || 0,
                    mcWeight: saved.mcWeight !== undefined ? saved.mcWeight : 100,
                    essayWeight: saved.essayWeight !== undefined ? saved.essayWeight : 0,
                    minimumPassingScore: saved.passingScore || 80,
                    currentUserId: 0,
                    questions: self.questions.map(function (q, idx) {
                        var isMC = (q.type || '').toLowerCase() === 'mc';
                        var questionDto = {
                            questionText: q.savedText || q.text || '',
                            questionType: isMC ? 'MC' : 'Essay',
                            sortOrder: idx + 1,
                            options: []
                        };
                        if (isMC) {
                            var correct = (q.correctOption || '').toUpperCase();
                            questionDto.options = [
                                { optionLetter: 'A', optionText: q.optionA || '', isCorrect: correct === 'A' },
                                { optionLetter: 'B', optionText: q.optionB || '', isCorrect: correct === 'B' },
                                { optionLetter: 'C', optionText: q.optionC || '', isCorrect: correct === 'C' },
                                { optionLetter: 'D', optionText: q.optionD || '', isCorrect: correct === 'D' }
                            ];
                        }
                        return questionDto;
                    })
                };

                app.loading && app.loading.show('Creating quiz...');
                self._api('/api/ELearning/create-quiz', { method: 'POST', body: JSON.stringify(payload) }).then(function (json) {
                    app.loading && app.loading.hide();
                    if (!json) {
                        Swal.fire({ icon: 'error', title: 'Failed to create quiz', customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' }, buttonsStyling: false });
                        return;
                    }
                    sessionStorage.removeItem('sv_quiz_config');
                    Swal.fire({
                        icon: 'success',
                        title: 'Quiz Created!',
                        text: 'All ' + self.questions.length + ' question(s) have been saved.',
                        customClass: { popup: 'el-swal', confirmButton: 'btn-el-swal-confirm' },
                        buttonsStyling: false
                    }).then(function () {
                        window.location.href = '/Modules/ELearning/Supervisor/ModuleDetail?id=' + (moduleId || '');
                    });
                });
            }
        });
    };

})(jQuery, window.app = window.app || {});


