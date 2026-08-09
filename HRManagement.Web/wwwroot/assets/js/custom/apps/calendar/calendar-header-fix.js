/**
 * Calendar Header Visibility Fix
 * This script ensures the calendar header is hidden when there's no data available
 */

(function() {
    'use strict';

    // Wait for DOM to be ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initCalendarFix);
    } else {
        initCalendarFix();
    }

    function initCalendarFix() {
        const calendarEl = document.getElementById('kt_calendar_app');

        if (!calendarEl) {
            return;
        }

        // Function to check if calendar has events and update visibility
        function updateCalendarVisibility() {
            const events = calendarEl.querySelectorAll('.fc-event');
            const toolbar = calendarEl.querySelector('.fc-toolbar');
            const content = calendarEl.querySelector('.fc-daygrid-body, .fc-timegrid-body, .fc-col-time-frame');

            // If there are no events and calendar content is essentially empty, hide the toolbar
            if (events.length === 0 && toolbar) {
                // Add a class to indicate no data state
                calendarEl.classList.add('calendar-no-data');

                // Optional: Hide toolbar for cleaner UI
                // toolbar.style.display = 'none';
            } else if (toolbar) {
                calendarEl.classList.remove('calendar-no-data');
            }
        }

        // Initial check
        updateCalendarVisibility();

        // Watch for changes in the calendar
        const observer = new MutationObserver(function() {
            updateCalendarVisibility();
        });

        // Observe the calendar container for any changes
        observer.observe(calendarEl, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ['class', 'style']
        });
    }
})();
