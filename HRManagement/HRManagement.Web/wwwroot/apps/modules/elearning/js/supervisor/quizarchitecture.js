(function ($, app) {
    app.elearning = app.elearning || {};
    app.elearning.supervisor = app.elearning.supervisor || {};
    app.elearning.intern = app.elearning.intern || {};

    app.elearning.supervisor.quizArchitecture = {};
    app.elearning.supervisor.quizArchitecture.state = {
        mcSelected: true,
        essaySelected: false,
        mcCount: 10,
        essayCount: 0,
        mcWeight: 100,
        essayWeight: 0,
        passingScore: 80
    };

    app.elearning.supervisor.quizArchitecture.init = function () {
        this.renderUI();
        this.bindEvents();
    };

    app.elearning.supervisor.quizArchitecture.renderUI = function () {
        var s = this.state;
        var self = this;

        self._applyOptionVisual('#el-sv-arch-mc', '#el-sv-arch-mc-check', s.mcSelected);
        self._applyOptionVisual('#el-sv-arch-essay', '#el-sv-arch-essay-check', s.essaySelected);

        $('#el-sv-arch-mc-plus, #el-sv-arch-mc-minus').prop('disabled', !s.mcSelected);
        $('#el-sv-arch-essay-plus, #el-sv-arch-essay-minus').prop('disabled', !s.essaySelected);
        $('.el-sv-arch-qty-row').eq(0).find('.el-sv-arch-stepper').toggleClass('el-sv-arch-stepper-disabled', !s.mcSelected);
        $('.el-sv-arch-qty-row').eq(1).find('.el-sv-arch-stepper').toggleClass('el-sv-arch-stepper-disabled', !s.essaySelected);

        if (!s.mcSelected) { s.mcCount = 0; $('#el-sv-arch-mc-count').text(0); }
        if (!s.essaySelected) { s.essayCount = 0; $('#el-sv-arch-essay-count').text(0); }

        var bothOn = s.mcSelected && s.essaySelected;
        if (!s.mcSelected) { s.mcWeight = 0; }
        if (!s.essaySelected) { s.essayWeight = 0; }
        if (!bothOn && s.mcSelected) { s.mcWeight = 100; s.essayWeight = 0; }
        if (!bothOn && s.essaySelected) { s.essayWeight = 100; s.mcWeight = 0; }

        $('#el-sv-arch-mc-weight').prop('disabled', !s.mcSelected || !bothOn).val(s.mcWeight);
        $('#el-sv-arch-essay-weight').prop('disabled', !s.essaySelected || !bothOn).val(s.essayWeight);
        $('#el-sv-arch-mc-pct').text(s.mcWeight);
        $('#el-sv-arch-essay-pct').text(s.essayWeight);

        self.updateSliderTrack('#el-sv-arch-mc-weight', s.mcWeight);
        self.updateSliderTrack('#el-sv-arch-essay-weight', s.essayWeight);
        self.updateBalance();
    };

    app.elearning.supervisor.quizArchitecture._applyOptionVisual = function (optSel, checkSel, active) {
        if (active) { $(optSel).addClass('selected'); $(checkSel).addClass('checked'); }
        else { $(optSel).removeClass('selected'); $(checkSel).removeClass('checked'); }
    };

    app.elearning.supervisor.quizArchitecture.updateSliderTrack = function (sel, val) {
        var el = document.querySelector(sel);
        if (el) {
            el.style.background = 'linear-gradient(to right, #f1416c 0%, #f1416c ' + val + '%, #f1f1f4 ' + val + '%, #f1f1f4 100%)';
        }
    };

    app.elearning.supervisor.quizArchitecture.updateBalance = function () {
        var s = this.state;
        var total = s.mcWeight + s.essayWeight;
        var badge = $('#el-sv-arch-balance-badge');
        if (total === 100) { badge.text('● Balanced 100%').css('color', '#f1416c'); }
        else if (total === 0) { badge.text('● No selection').css('color', '#99a1b7'); }
        else { badge.text('● Total: ' + total + '%').css('color', '#ffc700'); }
    };

    app.elearning.supervisor.quizArchitecture.bindEvents = function () {
        var self = this;

        $(document).on('click', '#el-sv-arch-mc', function (e) {
            if ($(e.target).closest('.el-sv-arch-stepper').length) return;
            var s = self.state;
            if (s.mcSelected && !s.essaySelected) {
                Swal.fire({ icon: 'warning', title: 'At least one type required', timer: 1500, showConfirmButton: false });
                return;
            }
            s.mcSelected = !s.mcSelected;
            if (s.mcSelected && s.essaySelected) { s.mcWeight = 50; s.essayWeight = 50; }
            self.renderUI();
        });

        $(document).on('click', '#el-sv-arch-essay', function (e) {
            if ($(e.target).closest('.el-sv-arch-stepper').length) return;
            var s = self.state;
            if (s.essaySelected && !s.mcSelected) {
                Swal.fire({ icon: 'warning', title: 'At least one type required', timer: 1500, showConfirmButton: false });
                return;
            }
            s.essaySelected = !s.essaySelected;
            if (s.mcSelected && s.essaySelected) {
                s.mcWeight = 50; s.essayWeight = 50;
                if (s.essayCount === 0) s.essayCount = 5;
                $('#el-sv-arch-essay-count').text(s.essayCount);
            }
            self.renderUI();
        });

        $(document).on('click', '#el-sv-arch-mc-plus', function (e) {
            e.stopPropagation();
            if (self.state.mcCount < 50) { self.state.mcCount++; $('#el-sv-arch-mc-count').text(self.state.mcCount); }
        });
        $(document).on('click', '#el-sv-arch-mc-minus', function (e) {
            e.stopPropagation();
            if (self.state.mcCount > 1) { self.state.mcCount--; $('#el-sv-arch-mc-count').text(self.state.mcCount); }
        });
        $(document).on('click', '#el-sv-arch-essay-plus', function (e) {
            e.stopPropagation();
            if (self.state.essayCount < 20) { self.state.essayCount++; $('#el-sv-arch-essay-count').text(self.state.essayCount); }
        });
        $(document).on('click', '#el-sv-arch-essay-minus', function (e) {
            e.stopPropagation();
            if (self.state.essayCount > 1) { self.state.essayCount--; $('#el-sv-arch-essay-count').text(self.state.essayCount); }
        });

        $(document).on('input', '#el-sv-arch-mc-weight', function () {
            var val = parseInt($(this).val());
            self.state.mcWeight = val;
            $('#el-sv-arch-mc-pct').text(val);
            self.updateSliderTrack('#el-sv-arch-mc-weight', val);
            if (self.state.mcSelected && self.state.essaySelected) {
                var linked = 100 - val;
                self.state.essayWeight = linked;
                $('#el-sv-arch-essay-weight').val(linked);
                $('#el-sv-arch-essay-pct').text(linked);
                self.updateSliderTrack('#el-sv-arch-essay-weight', linked);
            }
            self.updateBalance();
        });

        $(document).on('input', '#el-sv-arch-essay-weight', function () {
            var val = parseInt($(this).val());
            self.state.essayWeight = val;
            $('#el-sv-arch-essay-pct').text(val);
            self.updateSliderTrack('#el-sv-arch-essay-weight', val);
            if (self.state.mcSelected && self.state.essaySelected) {
                var linked = 100 - val;
                self.state.mcWeight = linked;
                $('#el-sv-arch-mc-weight').val(linked);
                $('#el-sv-arch-mc-pct').text(linked);
                self.updateSliderTrack('#el-sv-arch-mc-weight', linked);
            }
            self.updateBalance();
        });

        $(document).on('input', '#el-sv-arch-pass-score', function () {
            self.state.passingScore = parseInt($(this).val()) || 0;
        });

        $(document).on('click', '#el-sv-arch-next', function () {
            var s = self.state;
            if (!s.mcSelected && !s.essaySelected) {
                Swal.fire({ icon: 'warning', title: 'Select at least one question type', timer: 1800, showConfirmButton: false });
                return;
            }
            if (s.mcSelected && s.mcCount < 1) {
                Swal.fire({ icon: 'warning', title: 'Multiple Choice count must be at least 1', timer: 1800, showConfirmButton: false });
                return;
            }
            if (s.essaySelected && s.essayCount < 1) {
                Swal.fire({ icon: 'warning', title: 'Essay count must be at least 1', timer: 1800, showConfirmButton: false });
                return;
            }

            var questions = [];
            if (s.mcSelected) {
                for (var mi = 1; mi <= s.mcCount; mi++) { questions.push({ type: 'mc', num: mi, text: '' }); }
            }
            if (s.essaySelected) {
                for (var ei = 1; ei <= s.essayCount; ei++) { questions.push({ type: 'essay', num: ei, text: '' }); }
            }

            var config = {
                mcSelected: s.mcSelected,
                essaySelected: s.essaySelected,
                mcCount: s.mcSelected ? s.mcCount : 0,
                essayCount: s.essaySelected ? s.essayCount : 0,
                mcWeight: s.mcWeight,
                essayWeight: s.essayWeight,
                passingScore: s.passingScore,
                questions: questions
            };
            sessionStorage.setItem('sv_quiz_config', JSON.stringify(config));
            window.location.href = '/Modules/ELearning/Supervisor/QuizQuestion';
        });
    };

})(jQuery, window.app = window.app || {});


