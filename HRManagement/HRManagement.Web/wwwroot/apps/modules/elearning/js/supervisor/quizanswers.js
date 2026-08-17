(function ($, app) {
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    app.elearning.supervisor.quizAnswers = {};
    app.elearning.supervisor.quizAnswers._reviewData = [];
    app.elearning.supervisor.quizAnswers._submissionId = null;

    app.elearning.supervisor.quizAnswers._api = async function (path, opts) {
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

    app.elearning.supervisor.quizAnswers._getSubmissionId = function () {
        var params = new URLSearchParams(window.location.search);
        return parseInt(params.get('submissionId') || '0') || null;
    };

    app.elearning.supervisor.quizAnswers.init = function () {
        var self = this;
        self._submissionId = self._getSubmissionId();
        if (!self._submissionId) return;

        app.loading && app.loading.show('Loading submission...');
        self._api('/api/ELearning/submissions/' + self._submissionId).then(function (json) {
            app.loading && app.loading.hide();
            if (!json) {
                Swal.fire({ icon: 'error', title: 'Failed to load submission' });
                return;
            }
            var data = json.content || json.data || json;
            self._reviewData = data.answers || data.questions || [];
            self.renderHeader(data);
            self.renderList();
            self.bindEvents();
        });
    };

    app.elearning.supervisor.quizAnswers.renderHeader = function (data) {
        if (!data) return;
        var internName = data.internName || '';
        var score = data.totalScore !== null && data.totalScore !== undefined ? data.totalScore : '-';
        var passed = data.isPassed === true ? 'Passed' : data.isPassed === false ? 'Failed' : '-';
        if ($('#el-sv-qa-intern-name').length) $('#el-sv-qa-intern-name').text(internName);
        if ($('#el-sv-qa-total-score').length) $('#el-sv-qa-total-score').text(score);
        if ($('#el-sv-qa-status').length) $('#el-sv-qa-status').text(passed);
    };

    app.elearning.supervisor.quizAnswers.renderList = function () {
        var data = this._reviewData;
        var html = '';

        data.forEach(function (q, idx) {
            var qType = (q.questionType || q.type || '').toLowerCase();
            var isMC = qType === 'mc' || qType === 'multiple-choice' || qType === 'multiplechoice';
            var assignedScore = q.assignedScore !== undefined ? q.assignedScore : (q.score || 0);
            var maxScore = q.maxScore || 100;
            var scoreArea = '';

            if (isMC) {
                scoreArea = '<div class="el-sv-qa-score-box">' + assignedScore + '</div>';
            } else {
                scoreArea = '<div class="d-flex align-items-center gap-1">' +
                    '<input type="number" class="el-sv-qa-score-input" value="' + assignedScore + '" min="0" max="' + maxScore + '" data-idx="' + idx + '">' +
                    '<span class="el-sv-qa-score-max">/' + maxScore + '</span>' +
                    '</div>';
            }

            var answerArea = '';
            if (isMC) {
                var options = q.options || [];
                var internAnswer = q.selectedOption || q.internAnswer || q.answer || '';
                var correctAnswer = '';
                if (options.length) {
                    var correctOpt = options.find(function (o) { return o.isCorrect; });
                    correctAnswer = correctOpt ? correctOpt.optionLetter : '';
                }
                answerArea = '<div class="el-sv-qa-options-list">';
                options.forEach(function (opt) {
                    var letter = opt.optionLetter || '';
                    var text = opt.optionText || '';
                    var pillClass = 'el-sv-qa-opt-pill';
                    if (letter === internAnswer && letter === correctAnswer) {
                        pillClass += ' correct';
                    } else if (letter === internAnswer && letter !== correctAnswer) {
                        pillClass += ' wrong';
                    } else if (letter === correctAnswer && letter !== internAnswer) {
                        pillClass += ' expected';
                    }
                    answerArea += '<div class="' + pillClass + '">' + letter + '. ' + text + '</div>';
                });
                answerArea += '</div>';
            } else {
                answerArea = '<div class="el-sv-qa-essay-box">' + (q.essayAnswerText || q.essayAnswer || q.answer || '') + '</div>';
            }

            html += '<div class="el-sv-qa-question-card el-animate el-animate-delay-' + ((idx % 4) + 1) + '">' +
                '<div class="el-sv-qa-header">' +
                '<div class="el-sv-qa-question-text">' + (idx + 1) + '. ' + (q.questionText || q.text || q.question || '') + '</div>' +
                scoreArea +
                '</div>' +
                answerArea +
                '</div>';
        });

        $('#el-sv-quiz-answers-list').html(html);
    };

    app.elearning.supervisor.quizAnswers.bindEvents = function () {
        var self = this;

        $(document).on('click', '#el-sv-qa-submit', function () {
            Swal.fire({
                title: 'Submit Result?',
                text: 'Are you sure you want to submit this grading result?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, Submit',
                cancelButtonText: 'Cancel',
                customClass: { confirmButton: 'btn btn-sm fw-bold btn-danger', cancelButton: 'btn btn-sm fw-bold btn-light' },
                buttonsStyling: false
            }).then(function (result) {
                if (!result.isConfirmed) return;

                var scores = [];
                $('.el-sv-qa-score-input').each(function () {
                    var idx = parseInt($(this).data('idx'));
                    var q = self._reviewData[idx];
                    scores.push({ questionId: q.questionId || q.id, score: parseInt($(this).val()) || 0 });
                });

                var payload = { submissionId: self._submissionId, scores: scores };
                app.loading && app.loading.show('Submitting result...');
                self._api('/api/ELearning/grade-submission', { method: 'PUT', body: JSON.stringify(payload) }).then(function (json) {
                    app.loading && app.loading.hide();
                    if (!json) {
                        Swal.fire({ icon: 'error', title: 'Failed to submit grading' });
                        return;
                    }
                    Swal.fire({
                        icon: 'success',
                        title: 'Result Submitted!',
                        text: 'The grading result has been recorded.',
                        customClass: { confirmButton: 'btn btn-sm fw-bold btn-primary' },
                        buttonsStyling: false
                    }).then(function () {
                        window.history.back();
                    });
                });
            });
        });
    };

})(jQuery, window.app = window.app || {});


