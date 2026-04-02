async function initCalendar (e, callback, defaultdate) {

    console.log("function initCalendar loaded")
        var calendarEl = document.getElementById('calendar');

    console.log("calendarEvent:", e);
        var calendar = new FullCalendar.Calendar(calendarEl, {
            plugins: ['interaction', 'dayGrid', 'timeGrid', 'list'],
            height: 'parent',
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
            },
            defaultView: 'dayGridMonth',
            defaultDate: defaultdate,
            navLinks: true, // can click day/week names to navigate views
            editable: true,
            eventLimit: true, // allow "more" link when too many events
            events: e,
            dateClick: callback,
            datesSet: async function (info) {
                if (isFirstLoad) {
                    isFirstLoad = false;
                    return;
                }

                const month = info.start.getMonth() + 1;
                const year = info.start.getFullYear();

                const res = await fetch(`/api/leave/get-by-month?month=${month}&year=${year}`);
                const data = await res.json();

                const mappedEvents = data.map(item => ({
                    id: item.leaveId,
                    title: item.leaveType,
                    start: item.leaveStartDate,
                    end: item.leaveStartDate && item.dayAmount
                        ? new Date(new Date(item.leaveStartDate).setDate(
                            new Date(item.leaveStartDate).getDate() + item.dayAmount
                        ))
                        : null,
                    allDay: true
                }));

                calendar.removeAllEvents();
                calendar.addEventSource(mappedEvents);
            }
        });

        calendar.render();

}