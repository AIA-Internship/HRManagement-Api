///**
// * SupervisorCalendar.js
// * Custom full-width calendar styled like the AIA Timesheet Dashboard.
// *
// * Leave bars now stretch from the leave's start date to its end date
// * (instead of only appearing on the start day), colored by status:
// *   - approved -> green
// *   - pending  -> yellow
// * When more leaves fall on a given day than can be shown (2 colored
// * bars), the extra leaves collapse into a single grey "+N more" pill
// * for that day, clickable to reveal the full list.
// */

//(function () {
//    "use strict";

//    let calendarInitialized = false;
//    let currentDate = new Date();

//    let leaveRecords = [];

//    const monthNames = [
//        "January", "February", "March", "April",
//        "May", "June", "July", "August",
//        "September", "October", "November", "December"
//    ];

//    const dayNames = [
//        "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN"
//    ];

//    // Max colored leave bars shown per day before collapsing into "+N more"
//    const MAX_VISIBLE_LANES = 2;

//const DAY_HEADER_HEIGHT = 34;


//    const LANE_HEIGHT = 34;
//    const LANE_GAP = 8;
//    const ROW_BOTTOM_PADDING = 16;
//    const MIN_ROW_HEIGHT = 140;

//    // ---------------------------------------------------------------
//    // 1. Data source
//    // ---------------------------------------------------------------

//    function fetchLeaveEvents() {
//        const sample = [
//            {
//                employeeName: "Jane Smith",
//                startDate: "2026-07-10",
//                endDate: "2026-07-11",
//                status: "approved"
//            },
//            {
//                employeeName: "Mike Ross",
//                startDate: "2026-07-10",
//                endDate: "2026-07-12",
//                status: "approved"
//            },
//            {
//                employeeName: "Alex Turner",
//                startDate: "2026-07-10",
//                endDate: "2026-07-11",
//                status: "approved"
//            },
//            {
//                employeeName: "Priya Nair",
//                startDate: "2026-07-10",
//                endDate: "2026-07-10",
//                status: "pending"
//            },
//            {
//                employeeName: "Sam Lee",
//                startDate: "2026-07-10",
//                endDate: "2026-07-10",
//                status: "approved"
//            }
//        ];

//        return Promise.resolve(sample);
//    }

//    // ---------------------------------------------------------------
//    // 2. Helpers
//    // ---------------------------------------------------------------

//    function normalizeStatus(status) {
//        const value = String(status || "").toLowerCase();

//        if (value === "approved" || value === "2") {
//            return "approved";
//        }

//        return "pending";
//    }

//    function formatDate(date) {
//        const year = date.getFullYear();
//        const month = String(date.getMonth() + 1).padStart(2, "0");
//        const day = String(date.getDate()).padStart(2, "0");

//        return `${year}-${month}-${day}`;
//    }

//    function parseDate(dateString) {
//        const [year, month, day] = dateString.split("-").map(Number);

//        return new Date(year, month - 1, day);
//    }

//    function isSameDate(date1, date2) {
//        return formatDate(date1) === formatDate(date2);
//    }

//    function addDays(date, days) {
//        const result = new Date(date);
//        result.setDate(result.getDate() + days);
//        return result;
//    }

//    function startOfWeekMonday(date) {
//        const result = new Date(date);
//        const dayIndex = (result.getDay() + 6) % 7; // Monday = 0
//        result.setDate(result.getDate() - dayIndex);
//        result.setHours(0, 0, 0, 0);
//        return result;
//    }

//    function getNormalizedRecords() {
//        return leaveRecords.map(record => ({
//            employeeName: record.employeeName,
//            status: normalizeStatus(record.status),
//            start: parseDate(record.startDate),
//            end: parseDate(record.endDate)
//        }));
//    }

//    // ---------------------------------------------------------------
//    // 3. Calendar rendering
//    // ---------------------------------------------------------------

//    function renderCalendar() {
//        const calendarContainer = document.getElementById("leaveCalendar");

//        if (!calendarContainer) {
//            console.error("Calendar container #leaveCalendar not found.");
//            return;
//        }

//        const year = currentDate.getFullYear();
//        const month = currentDate.getMonth();

//        calendarContainer.innerHTML = "";
//        calendarContainer.style.display = "flex";
//        calendarContainer.style.flexDirection = "column";

//        const titleElement = document.getElementById("calendarTitle");

//        if (titleElement) {
//            titleElement.textContent = `${monthNames[month]} ${year}`;
//        }

//        // Weekday header row
//        const headerRow = document.createElement("div");
//        headerRow.className = "leave-calendar-header-row";

//        dayNames.forEach(day => {
//            const header = document.createElement("div");
//            header.className = "leave-calendar-header";
//            header.textContent = day;
//            headerRow.appendChild(header);
//        });

//        calendarContainer.appendChild(headerRow);

//        const today = new Date();
//        today.setHours(0, 0, 0, 0);

//        const firstOfMonth = new Date(year, month, 1);
//        const gridStart = startOfWeekMonday(firstOfMonth);
//        const allEvents = getNormalizedRecords();

//        for (let week = 0; week < 6; week++) {
//            const weekStart = addDays(gridStart, week * 7);
//            const weekEnd = addDays(weekStart, 6);

//            const weekRow = buildWeekRow(weekStart, weekEnd, month, today, allEvents);
//            calendarContainer.appendChild(weekRow);
//        }
//    }

//    function buildWeekRow(weekStart, weekEnd, currentMonth, today, allEvents) {
//        const weekRow = document.createElement("div");
//        weekRow.className = "leave-week";
//        weekRow.style.position = "relative";
//        weekRow.style.display = "grid";
//        weekRow.style.gridTemplateColumns = "repeat(7, minmax(0, 1fr))";

//        // Events overlapping this week
//        const weekEvents = allEvents
//            .filter(evt => evt.start <= weekEnd && evt.end >= weekStart)
//            .sort((a, b) => {
//                if (a.start - b.start !== 0) {
//                    return a.start - b.start;
//                }
//                return (b.end - b.start) - (a.end - a.start);
//            });

//        // Greedy lane assignment so overlapping leaves never share a row
//        const lanes = [];
//        const placedEvents = [];

//        weekEvents.forEach(evt => {
//            const clippedStart = evt.start < weekStart ? weekStart : evt.start;
//            const clippedEnd = evt.end > weekEnd ? weekEnd : evt.end;
//            const colStart = Math.round((clippedStart - weekStart) / 86400000);
//            const colEnd = Math.round((clippedEnd - weekStart) / 86400000);

//            let laneIndex = 0;
//            // eslint-disable-next-line no-constant-condition
//            while (true) {
//                const lane = lanes[laneIndex] || (lanes[laneIndex] = []);
//                const overlaps = lane.some(seg => !(colEnd < seg.colStart || colStart > seg.colEnd));

//                if (!overlaps) {
//                    lane.push({ colStart, colEnd });
//                    placedEvents.push({ ...evt, colStart, colEnd, laneIndex });
//                    break;
//                }

//                laneIndex++;
//            }
//        });

//        const overflowNeeded = lanes.length > MAX_VISIBLE_LANES;

//        // Per-day count of leaves pushed into the "+N more" pill
//        const overflowCountByDay = [0, 0, 0, 0, 0, 0, 0];

//        if (overflowNeeded) {
//            placedEvents
//                .filter(evt => evt.laneIndex >= MAX_VISIBLE_LANES)
//                .forEach(evt => {
//                    for (let col = evt.colStart; col <= evt.colEnd; col++) {
//                        overflowCountByDay[col]++;
//                    }
//                });
//        }

//        const visibleLaneCount = overflowNeeded
//            ? MAX_VISIBLE_LANES + 1
//            : Math.min(lanes.length, MAX_VISIBLE_LANES);

//        const rowHeight = DAY_HEADER_HEIGHT
//            + (visibleLaneCount * (LANE_HEIGHT + LANE_GAP))
//            + ROW_BOTTOM_PADDING;

//        weekRow.style.minHeight = `${Math.max(rowHeight, MIN_ROW_HEIGHT)}px`;

//        // Day cells (background layer, holds only the date number)
//        for (let i = 0; i < 7; i++) {
//            const cellDate = addDays(weekStart, i);
//            const cell = document.createElement("div");
//            cell.className = "leave-calendar-day";

//            if (cellDate.getMonth() !== currentMonth) {
//                cell.classList.add("other-month");
//            }

//            if (cellDate.getDay() === 0 || cellDate.getDay() === 6) {
//                cell.classList.add("weekend");
//            }

//            if (isSameDate(cellDate, today)) {
//                cell.classList.add("today");
//            }

//            const dateElement = document.createElement("div");
//            dateElement.className = "leave-calendar-date";
//            dateElement.textContent = cellDate.getDate();
//            cell.appendChild(dateElement);

//            weekRow.appendChild(cell);
//        }

//        // Leave bars (overlay layer), stretched from start col to end col
//        placedEvents.forEach(evt => {
//            if (overflowNeeded && evt.laneIndex >= MAX_VISIBLE_LANES) {
//                return; // represented by the overflow pill instead
//            }
//            const bar = document.createElement("div");

//            bar.className = `leave-calendar-event ${evt.status}`;

//            bar.innerHTML = `
//    <span class="leave-event-name">${evt.employeeName}</span>
//    <span class="leave-event-status">
//        ${evt.status === "approved" ? "Approved Leave" : "Pending Request"}
//    </span>
//`;

//            bar.title = `${evt.employeeName} (${evt.status})`;

//            bar.addEventListener("click", function (event) {
//                event.stopPropagation();

//                showLeaveDetailPopup(bar, evt);
//            });

//            positionOverlay(
//                bar,
//                evt.colStart,
//                evt.colEnd,
//                evt.laneIndex
//            );

//            weekRow.appendChild(bar);



//        });

//        // "+N more" pill, one per day that has overflow
//        if (overflowNeeded) {
//            overflowCountByDay.forEach((count, dayIndex) => {
//                if (count === 0) {
//                    return;
//                }

//                const pill = document.createElement("div");
//                pill.className = "leave-calendar-more";
//                pill.textContent = `+${count} more`;

//                positionOverlay(pill, dayIndex, dayIndex, MAX_VISIBLE_LANES);

//                pill.addEventListener("click", event => {
//                    event.stopPropagation();
//                    showDayOverflowPopover(pill, weekStart, dayIndex, allEvents);
//                });

//                weekRow.appendChild(pill);
//            });
//        }

//        return weekRow;
//    }

//    function positionOverlay(element, colStart, colEnd, laneIndex) {
//        const leftPct = (colStart / 7) * 100;
//        const widthPct = ((colEnd - colStart + 1) / 7) * 100;
//        const top = DAY_HEADER_HEIGHT + laneIndex * (LANE_HEIGHT + LANE_GAP);

//        element.style.position = "absolute";
//        element.style.left = `calc(${leftPct}% + 4px)`;
//        element.style.width = `calc(${widthPct}% - 8px)`;
//        element.style.top = `${top}px`;
//        element.style.height = `${LANE_HEIGHT}px`;
//    }

//    // ---------------------------------------------------------------
//    // 3b. Overflow popover ("+N more")
//    // ---------------------------------------------------------------

//    function showDayOverflowPopover(anchorEl, weekStart, dayIndex, allEvents) {
//        document.querySelectorAll(".leave-day-overflow-popover").forEach(el => el.remove());

//        const dayDate = addDays(weekStart, dayIndex);

//        const dayEvents = allEvents.filter(evt => dayDate >= evt.start && dayDate <= evt.end);

//        const popover = document.createElement("div");
//        popover.className = "leave-day-overflow-popover";

//        dayEvents.forEach(evt => {
//            const item = document.createElement("div");
//            item.className = `popover-item ${evt.status}`;
//            item.textContent = evt.employeeName;
//            popover.appendChild(item);
//        });

//        popover.style.left = `${anchorEl.offsetLeft}px`;
//        popover.style.top = `${anchorEl.offsetTop + anchorEl.offsetHeight + 4}px`;

//        anchorEl.parentElement.appendChild(popover);

//        setTimeout(() => {
//            document.addEventListener("click", function handler(e) {
//                if (!popover.contains(e.target) && e.target !== anchorEl) {
//                    popover.remove();
//                    document.removeEventListener("click", handler);
//                }
//            });
//        }, 0);
//    }

//    function formatDisplayDate(date) {
//        return date.toLocaleDateString("en-GB", {
//            day: "2-digit",
//            month: "short",
//            year: "numeric"
//        });
//    }

//    function showLeaveDetailPopup(anchorEl, evt) {
//        document
//            .querySelectorAll(".leave-detail-popup")
//            .forEach(el => el.remove());

//        const popup = document.createElement("div");

//        popup.className = "leave-detail-popup";

//        popup.innerHTML = `
//        <div class="leave-detail-popup-header">
//            <strong>${evt.employeeName}</strong>

//            <button type="button" class="leave-popup-close">
//                ×
//            </button>
//        </div>

//        <div class="leave-detail-popup-body">

//            <div class="leave-detail-row">
//                <span class="label">Status</span>
//                <span class="value status-${evt.status}">
//                    ${evt.status === "approved"
//                ? "Approved Leave"
//                : "Pending Request"}
//                </span>
//            </div>

//            <div class="leave-detail-row">
//                <span class="label">Start Date</span>
//                <span class="value">
//                    ${formatDisplayDate(evt.start)}
//                </span>
//            </div>

//            <div class="leave-detail-row">
//                <span class="label">End Date</span>
//                <span class="value">
//                    ${formatDisplayDate(evt.end)}
//                </span>
//            </div>

//        </div>
//    `;

//        popup.style.left = `${anchorEl.offsetLeft}px`;
//        popup.style.top =
//            `${anchorEl.offsetTop + anchorEl.offsetHeight + 8}px`;

//        anchorEl.parentElement.appendChild(popup);

//        popup
//            .querySelector(".leave-popup-close")
//            .addEventListener("click", function (event) {
//                event.stopPropagation();
//                popup.remove();
//            });

//        setTimeout(() => {
//            document.addEventListener("click", function handler(event) {
//                if (
//                    !popup.contains(event.target) &&
//                    event.target !== anchorEl
//                ) {
//                    popup.remove();
//                    document.removeEventListener("click", handler);
//                }
//            });
//        }, 0);
//    }

//    // ---------------------------------------------------------------
//    // 4. Calendar controls
//    // ---------------------------------------------------------------

//    function wireHeaderControls() {
//        const prevButton = document.getElementById("calPrevBtn");
//        const nextButton = document.getElementById("calNextBtn");
//        const todayButton = document.getElementById("calTodayBtn");

//        if (prevButton) {
//            prevButton.addEventListener("click", function () {
//                currentDate.setMonth(currentDate.getMonth() - 1);
//                renderCalendar();
//            });
//        }

//        if (nextButton) {
//            nextButton.addEventListener("click", function () {
//                currentDate.setMonth(currentDate.getMonth() + 1);
//                renderCalendar();
//            });
//        }

//        if (todayButton) {
//            todayButton.addEventListener("click", function () {
//                currentDate = new Date();
//                renderCalendar();
//            });
//        }
//    }

//    // ---------------------------------------------------------------
//    // 5. Calendar initialization
//    // ---------------------------------------------------------------

//    function initCalendar() {
//        if (calendarInitialized) {
//            return;
//        }

//        fetchLeaveEvents()
//            .then(function (records) {
//                leaveRecords = records || [];

//                renderCalendar();
//                wireHeaderControls();

//                calendarInitialized = true;
//            })
//            .catch(function (error) {
//                console.error("Failed to load leave calendar:", error);
//            });
//    }

//    // ---------------------------------------------------------------
//    // 6. Tab switching
//    // ---------------------------------------------------------------

//    function showTab(target) {
//        const tabRequest = document.getElementById("tabRequest");
//        const tabCalendar = document.getElementById("tabCalendar");
//        const requestCard = document.getElementById("requestCard");
//        const calendarCard = document.getElementById("calendarCard");

//        const showingCalendar = target === "calendar";

//        if (!tabRequest || !tabCalendar) {
//            return;
//        }

//        tabRequest.classList.toggle("active", !showingCalendar);
//        tabCalendar.classList.toggle("active", showingCalendar);

//        if (requestCard) {
//            requestCard.style.display = showingCalendar ? "none" : "";
//        }

//        if (calendarCard) {
//            calendarCard.style.display = showingCalendar ? "" : "none";
//        }

//        if (showingCalendar) {
//            initCalendar();
//        }
//    }

//    function initTabs() {
//        const tabRequest = document.getElementById("tabRequest");
//        const tabCalendar = document.getElementById("tabCalendar");

//        if (!tabRequest || !tabCalendar) {
//            return;
//        }

//        tabRequest.addEventListener("click", function () {
//            showTab("request");
//        });

//        tabCalendar.addEventListener("click", function () {
//            showTab("calendar");
//        });

//        if (tabCalendar.classList.contains("active")) {
//            showTab("calendar");
//        }
//    }

//    // ---------------------------------------------------------------
//    // 7. Month picker + injected styles
//    // ---------------------------------------------------------------

//    function injectStyles() {
//        const style = document.createElement("style");
//        style.textContent = `
//            .leave-calendar-header-row {
//                display: grid;
//                grid-template-columns: repeat(7, minmax(0, 1fr));
//            }
//            .leave-week {
//                border-bottom: 1px solid var(--bs-gray-200, #E4E6EF);
//            }
//            .leave-week:last-child {
//                border-bottom: none;
//            }
//            .leave-calendar-event {
//                box-sizing: border-box;
//                display: flex;
//                align-items: center;
//                padding: 0 10px;
//                border-radius: 6px;
//                font-size: 11px;
//                font-weight: 600;
//                overflow: hidden;
//                white-space: nowrap;
//                text-overflow: ellipsis;
//                cursor: default;
//            }
//            .leave-calendar-event.approved {
//                background: var(--bs-light-success, #DFF7E6);
//                color: var(--bs-success, #027A48);
//            }
//            .leave-calendar-event.pending {
//                background: var(--bs-light-warning, #FEF6DC);
//                color: var(--bs-warning, #93700B);
//            }
//            .leave-calendar-more {
//                box-sizing: border-box;
//                display: flex;
//                align-items: center;
//                padding: 0 10px;
//                border-radius: 6px;
//                font-size: 11px;
//                font-weight: 600;
//                background: #F2F4F7;
//                color: #475467;
//                cursor: pointer;
//            }
//            .leave-calendar-more:hover {
//                background: #E4E7EC;
//                color: #344054;
//            }
//            .leave-day-overflow-popover {
//                position: absolute;
//                z-index: 50;
//                background: #fff;
//                border: 1px solid #EFF2F5;
//                border-radius: 10px;
//                box-shadow: 0 8px 24px rgba(16, 24, 40, 0.12);
//                padding: 8px;
//                min-width: 180px;
//            }
//            .leave-day-overflow-popover .popover-item {
//                padding: 6px 8px;
//                border-radius: 6px;
//                font-size: 12px;
//                font-weight: 600;
//                margin-bottom: 4px;
//            }
//            .leave-day-overflow-popover .popover-item:last-child {
//                margin-bottom: 0;
//            }
//            .leave-day-overflow-popover .popover-item.approved {
//                background: #DFF7E6;
//                color: #027A48;
//            }
//            .leave-day-overflow-popover .popover-item.pending {
//                background: #FEF6DC;
//                color: #93700B;
//            }
//            .leave-detail-popup {
//            position: absolute;

//            z-index: 100;

//            width: 280px;

//            background: #ffffff;

//            border: 2px solid #D0D5DD;

//            border-radius: 12px;

//            box-shadow:
//                0 12px 30px rgba(16, 24, 40, 0.18);

//            padding: 16px;
//        }

//        .leave-detail-popup-header {
//            display: flex;

//            align-items: center;
//            justify-content: space-between;

//            padding-bottom: 12px;

//            border-bottom: 1px solid #EAECF0;

//            font-size: 15px;
//        }

//        .leave-popup-close {
//            border: none;

//            background: transparent;

//            font-size: 22px;

//            line-height: 1;

//            cursor: pointer;

//            color: #667085;
//        }

//        .leave-popup-close:hover {
//            color: #101828;
//        }

//        .leave-detail-popup-body {
//            display: flex;

//            flex-direction: column;

//            gap: 12px;

//            padding-top: 14px;
//        }

//        .leave-detail-row {
//            display: flex;

//            flex-direction: column;

//            gap: 3px;
//        }

//        .leave-detail-row .label {
//            font-size: 11px;

//            font-weight: 600;

//            color: #667085;

//            text-transform: uppercase;
//        }

//        .leave-detail-row .value {
//            font-size: 13px;

//            font-weight: 700;

//            color: #101828;
//        }

//        .leave-detail-row .status-approved {
//            color: #027A48;
//        }

//        .leave-detail-row .status-pending {
//            color: #B54708;
//        }
//        `;
//        document.head.appendChild(style);
//    }

//    const calendarMonthTrigger = document.getElementById("calendarMonthTrigger");
//    const monthPickerDropdown = document.getElementById("monthPickerDropdown");
//    const monthPickerYear = document.getElementById("monthPickerYear");
//    const monthGrid = document.getElementById("monthGrid");
//    const prevYearBtn = document.getElementById("prevYearBtn");
//    const nextYearBtn = document.getElementById("nextYearBtn");
//    const calTodayBtn = document.getElementById("calTodayBtn");

//    let monthPickerCurrentYear = currentDate.getFullYear();

//    const monthShortNames = [
//        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
//        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
//    ];

//    function renderMonthPicker() {
//        monthPickerYear.textContent = monthPickerCurrentYear;
//        monthGrid.innerHTML = "";

//        monthShortNames.forEach((month, index) => {
//            const button = document.createElement("button");

//            button.type = "button";
//            button.className = "month-option";
//            button.textContent = month;

//            if (
//                currentDate.getFullYear() === monthPickerCurrentYear &&
//                currentDate.getMonth() === index
//            ) {
//                button.classList.add("active");
//            }

//            button.addEventListener("click", () => {
//                currentDate = new Date(monthPickerCurrentYear, index, 1);
//                renderCalendar();
//                closeMonthPicker();
//            });

//            monthGrid.appendChild(button);
//        });
//    }

//    function openMonthPicker() {
//        monthPickerCurrentYear = currentDate.getFullYear();
//        renderMonthPicker();

//        monthPickerDropdown.classList.add("show");
//        calendarMonthTrigger.classList.add("active");
//    }

//    function closeMonthPicker() {
//        monthPickerDropdown.classList.remove("show");
//        calendarMonthTrigger.classList.remove("active");
//    }

//    if (calendarMonthTrigger) {
//        calendarMonthTrigger.addEventListener("click", (event) => {
//            event.stopPropagation();

//            if (monthPickerDropdown.classList.contains("show")) {
//                closeMonthPicker();
//            } else {
//                openMonthPicker();
//            }
//        });
//    }

//    if (prevYearBtn) {
//        prevYearBtn.addEventListener("click", () => {
//            monthPickerCurrentYear--;
//            renderMonthPicker();
//        });
//    }

//    if (nextYearBtn) {
//        nextYearBtn.addEventListener("click", () => {
//            monthPickerCurrentYear++;
//            renderMonthPicker();
//        });
//    }

//    document.addEventListener("click", (event) => {
//        if (!event.target.closest(".month-picker-wrapper")) {
//            closeMonthPicker();
//        }
//    });

//    if (calTodayBtn) {
//        calTodayBtn.addEventListener("click", () => {
//            currentDate = new Date();
//            renderCalendar();

//            calTodayBtn.classList.add("active");
//            setTimeout(() => {
//                calTodayBtn.classList.remove("active");
//            }, 300);
//        });
//    }

//    // ---------------------------------------------------------------
//    // 8. Init
//    // ---------------------------------------------------------------

//    document.addEventListener("DOMContentLoaded", function () {
//        injectStyles();
//        initTabs();
//    });

//})();]




/**
 * SupervisorCalendar.js
 * Custom full-width calendar styled like the AIA Timesheet Dashboard.
 */

(function () {
    "use strict";

    let calendarInitialized = false;
    let currentDate = new Date();

    let leaveRecords = [];

    const monthNames = [
        "January", "February", "March", "April",
        "May", "June", "July", "August",
        "September", "October", "November", "December"
    ];

    const dayNames = [
        "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN"
    ];

    // ---------------------------------------------------------------
    // Calendar layout constants
    // ---------------------------------------------------------------

    const MAX_VISIBLE_LANES = 2;

    const DAY_HEADER_HEIGHT = 34;

    const LANE_HEIGHT = 34;
    const LANE_GAP = 8;

    const ROW_BOTTOM_PADDING = 16;
    const MIN_ROW_HEIGHT = 140;

    // ---------------------------------------------------------------
    // 1. Data source
    // ---------------------------------------------------------------

    function fetchLeaveEvents() {
        const sample = [
            {
                employeeName: "Jane Smith",
                startDate: "2026-07-10",
                endDate: "2026-07-11",
                status: "approved",
                description : "asdasd"
            },
            {
                employeeName: "Mike Ross",
                startDate: "2026-07-10",
                endDate: "2026-07-12",
                status: "approved",
                description: "asdasd"
            },
            {
                employeeName: "Alex Turner",
                startDate: "2026-07-10",
                endDate: "2026-07-11",
                status: "approved",
                description: "asdasd"
            },
            {
                employeeName: "Priya Nair",
                startDate: "2026-07-10",
                endDate: "2026-07-10",
                status: "pending",
                description: "asdasd"
            },
            {
                employeeName: "Sam Lee",
                startDate: "2026-07-10",
                endDate: "2026-07-10",
                status: "approved",
                description: "asdasd"
            }
        ];

        return Promise.resolve(sample);
    }

    // ---------------------------------------------------------------
    // 2. Helpers
    // ---------------------------------------------------------------

    function normalizeStatus(status) {
        const value = String(status || "").toLowerCase();

        if (value === "approved" || value === "2") {
            return "approved";
        }

        return "pending";
    }

    function formatDate(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const day = String(date.getDate()).padStart(2, "0");

        return `${year}-${month}-${day}`;
    }

    function parseDate(dateString) {
        const [year, month, day] = dateString.split("-").map(Number);

        return new Date(year, month - 1, day);
    }

    function isSameDate(date1, date2) {
        return formatDate(date1) === formatDate(date2);
    }

    function addDays(date, days) {
        const result = new Date(date);

        result.setDate(result.getDate() + days);

        return result;
    }

    function startOfWeekMonday(date) {
        const result = new Date(date);

        const dayIndex = (result.getDay() + 6) % 7;

        result.setDate(result.getDate() - dayIndex);

        result.setHours(0, 0, 0, 0);

        return result;
    }

    function getNormalizedRecords() {
        return leaveRecords.map(record => ({
            employeeName: record.employeeName,
            status: normalizeStatus(record.status),
            start: parseDate(record.startDate),
            end: parseDate(record.endDate)
        }));
    }

    function formatDisplayDate(date) {
        return date.toLocaleDateString("en-GB", {
            day: "numeric",
            month: "long",
            year: "numeric"
        });
    }

    function getLeaveType(status) {
        return status === "approved"
            ? "Paid Leave"
            : "Unpaid Leave";
    }

    // ---------------------------------------------------------------
    // 3. Calendar rendering
    // ---------------------------------------------------------------

    function renderCalendar() {
        const calendarContainer =
            document.getElementById("leaveCalendar");

        if (!calendarContainer) {
            console.error(
                "Calendar container #leaveCalendar not found."
            );

            return;
        }

        const year = currentDate.getFullYear();
        const month = currentDate.getMonth();

        calendarContainer.innerHTML = "";

        calendarContainer.style.display = "flex";
        calendarContainer.style.flexDirection = "column";

        const titleElement =
            document.getElementById("calendarTitle");

        if (titleElement) {
            titleElement.textContent =
                `${monthNames[month]} ${year}`;
        }

        // -----------------------------------------------------------
        // Weekday header
        // -----------------------------------------------------------

        const headerRow = document.createElement("div");

        headerRow.className =
            "leave-calendar-header-row";

        dayNames.forEach(day => {
            const header = document.createElement("div");

            header.className =
                "leave-calendar-header";

            header.textContent = day;

            headerRow.appendChild(header);
        });

        calendarContainer.appendChild(headerRow);

        // -----------------------------------------------------------
        // Calendar dates
        // -----------------------------------------------------------

        const today = new Date();

        today.setHours(0, 0, 0, 0);

        const firstOfMonth =
            new Date(year, month, 1);

        const gridStart =
            startOfWeekMonday(firstOfMonth);

        const allEvents =
            getNormalizedRecords();

        for (let week = 0; week < 6; week++) {
            const weekStart =
                addDays(gridStart, week * 7);

            const weekEnd =
                addDays(weekStart, 6);

            const weekRow =
                buildWeekRow(
                    weekStart,
                    weekEnd,
                    month,
                    today,
                    allEvents
                );

            calendarContainer.appendChild(weekRow);
        }
    }

    function buildWeekRow(
        weekStart,
        weekEnd,
        currentMonth,
        today,
        allEvents
    ) {
        const weekRow =
            document.createElement("div");

        weekRow.className =
            "leave-week";

        weekRow.style.position = "relative";
        weekRow.style.display = "grid";
        weekRow.style.gridTemplateColumns =
            "repeat(7, minmax(0, 1fr))";

        // -----------------------------------------------------------
        // Events overlapping this week
        // -----------------------------------------------------------

        const weekEvents = allEvents
            .filter(evt =>
                evt.start <= weekEnd &&
                evt.end >= weekStart
            )
            .sort((a, b) => {
                if (a.start - b.start !== 0) {
                    return a.start - b.start;
                }

                return (
                    (b.end - b.start) -
                    (a.end - a.start)
                );
            });

        // -----------------------------------------------------------
        // Lane assignment
        // -----------------------------------------------------------

        const lanes = [];
        const placedEvents = [];

        weekEvents.forEach(evt => {
            const clippedStart =
                evt.start < weekStart
                    ? weekStart
                    : evt.start;

            const clippedEnd =
                evt.end > weekEnd
                    ? weekEnd
                    : evt.end;

            const colStart =
                Math.round(
                    (clippedStart - weekStart) /
                    86400000
                );

            const colEnd =
                Math.round(
                    (clippedEnd - weekStart) /
                    86400000
                );

            let laneIndex = 0;

            while (true) {
                const lane =
                    lanes[laneIndex] ||
                    (lanes[laneIndex] = []);

                const overlaps =
                    lane.some(seg =>
                        !(
                            colEnd < seg.colStart ||
                            colStart > seg.colEnd
                        )
                    );

                if (!overlaps) {
                    lane.push({
                        colStart,
                        colEnd
                    });

                    placedEvents.push({
                        ...evt,
                        colStart,
                        colEnd,
                        laneIndex
                    });

                    break;
                }

                laneIndex++;
            }
        });

        const overflowNeeded =
            lanes.length > MAX_VISIBLE_LANES;

        const overflowCountByDay =
            [0, 0, 0, 0, 0, 0, 0];

        if (overflowNeeded) {
            placedEvents
                .filter(evt =>
                    evt.laneIndex >= MAX_VISIBLE_LANES
                )
                .forEach(evt => {
                    for (
                        let col = evt.colStart;
                        col <= evt.colEnd;
                        col++
                    ) {
                        overflowCountByDay[col]++;
                    }
                });
        }

        const visibleLaneCount =
            overflowNeeded
                ? MAX_VISIBLE_LANES + 1
                : Math.min(
                    lanes.length,
                    MAX_VISIBLE_LANES
                );

        const rowHeight =
            DAY_HEADER_HEIGHT +
            (
                visibleLaneCount *
                (LANE_HEIGHT + LANE_GAP)
            ) +
            ROW_BOTTOM_PADDING;

        weekRow.style.minHeight =
            `${Math.max(
                rowHeight,
                MIN_ROW_HEIGHT
            )}px`;

        // -----------------------------------------------------------
        // Day cells
        // -----------------------------------------------------------

        for (let i = 0; i < 7; i++) {
            const cellDate =
                addDays(weekStart, i);

            const cell =
                document.createElement("div");

            cell.className =
                "leave-calendar-day";

            if (
                cellDate.getMonth() !==
                currentMonth
            ) {
                cell.classList.add(
                    "other-month"
                );
            }

            if (
                cellDate.getDay() === 0 ||
                cellDate.getDay() === 6
            ) {
                cell.classList.add(
                    "weekend"
                );
            }

            if (
                isSameDate(
                    cellDate,
                    today
                )
            ) {
                cell.classList.add(
                    "today"
                );
            }

            const dateElement =
                document.createElement("div");

            dateElement.className =
                "leave-calendar-date";

            dateElement.textContent =
                cellDate.getDate();

            cell.appendChild(dateElement);

            // Click day -> open day modal
            cell.addEventListener(
                "click",
                function () {
                    showDayLeaveModal(
                        cellDate,
                        allEvents
                    );
                }
            );

            weekRow.appendChild(cell);
        }

        // -----------------------------------------------------------
        // Leave bars
        // -----------------------------------------------------------

        placedEvents.forEach(evt => {
            if (
                overflowNeeded &&
                evt.laneIndex >=
                MAX_VISIBLE_LANES
            ) {
                return;
            }

            const bar =
                document.createElement("div");

            bar.className =
                `leave-calendar-event ${evt.status}`;

            bar.innerHTML = `
                <span class="leave-event-name">
                    ${evt.employeeName}
                </span>

                <span class="leave-event-status">
                    ${evt.status === "approved"
                    ? "Approved Leave"
                    : "Pending Request"
                }
                </span>
            `;

            bar.title =
                `${evt.employeeName} (${evt.status})`;

            // Click leave bar -> open the day modal
            bar.addEventListener(
                "click",
                function (event) {
                    event.stopPropagation();

                    showDayLeaveModal(
                        evt.start,
                        allEvents
                    );
                }
            );

            positionOverlay(
                bar,
                evt.colStart,
                evt.colEnd,
                evt.laneIndex
            );

            weekRow.appendChild(bar);
        });

        // -----------------------------------------------------------
        // "+N more"
        // -----------------------------------------------------------

        if (overflowNeeded) {
            overflowCountByDay.forEach(
                (count, dayIndex) => {
                    if (count === 0) {
                        return;
                    }

                    const pill =
                        document.createElement("div");

                    pill.className =
                        "leave-calendar-more";

                    pill.textContent =
                        `+${count} more`;

                    positionOverlay(
                        pill,
                        dayIndex,
                        dayIndex,
                        MAX_VISIBLE_LANES
                    );

                    pill.addEventListener(
                        "click",
                        event => {
                            event.stopPropagation();

                            const dayDate =
                                addDays(
                                    weekStart,
                                    dayIndex
                                );

                            showDayLeaveModal(
                                dayDate,
                                allEvents
                            );
                        }
                    );

                    weekRow.appendChild(pill);
                }
            );
        }

        return weekRow;
    }

    function positionOverlay(
        element,
        colStart,
        colEnd,
        laneIndex
    ) {
        const leftPct =
            (colStart / 7) * 100;

        const widthPct =
            (
                (colEnd - colStart + 1) /
                7
            ) * 100;

        const top =
            DAY_HEADER_HEIGHT +
            laneIndex *
            (
                LANE_HEIGHT +
                LANE_GAP
            );

        element.style.position =
            "absolute";

        element.style.left =
            `calc(${leftPct}% + 4px)`;

        element.style.width =
            `calc(${widthPct}% - 8px)`;

        element.style.top =
            `${top}px`;

        element.style.height =
            `${LANE_HEIGHT}px`;
    }

    // ---------------------------------------------------------------
    // 4. Day modal
    // ---------------------------------------------------------------

    function showDayLeaveModal(
        dayDate,
        allEvents
    ) {
        document
            .querySelectorAll(
                ".leave-day-modal-overlay"
            )
            .forEach(el => el.remove());

        const dayEvents =
            allEvents.filter(evt =>
                dayDate >= evt.start &&
                dayDate <= evt.end
            );

        if (dayEvents.length === 0) {
            return;
        }

        const overlay =
            document.createElement("div");

        overlay.className =
            "leave-day-modal-overlay";

        const modal =
            document.createElement("div");

        modal.className =
            "leave-day-modal";

        const formattedDate =
            formatDisplayDate(dayDate);

        const approvedEvents =
            dayEvents.filter(
                evt =>
                    evt.status === "approved"
            );

        const pendingEvents =
            dayEvents.filter(
                evt =>
                    evt.status === "pending"
            );

        modal.innerHTML = `
            <div class="leave-day-modal-header">

                <h2>
                    ${formattedDate}
                </h2>

                <button
                    type="button"
                    class="leave-day-modal-close">

                    ×

                </button>

            </div>

            ${createLeaveSection(
            "Approved leave",
            approvedEvents
        )}

            ${createLeaveSection(
            "Pending request",
            pendingEvents
        )}
        `;

        overlay.appendChild(modal);

        document.body.appendChild(overlay);

        const closeButton =
            modal.querySelector(
                ".leave-day-modal-close"
            );

        closeButton.addEventListener(
            "click",
            function () {
                overlay.remove();
            }
        );

        overlay.addEventListener(
            "click",
            function (event) {
                if (
                    event.target === overlay
                ) {
                    overlay.remove();
                }
            }
        );
    }

    function createLeaveSection(
        title,
        events
    ) {
        if (events.length === 0) {
            return "";
        }

        const sectionId =
            `leave-section-${Math.random()
                .toString(36)
                .substring(2, 9)}`;

        return `
            <div
                class="leave-modal-section"
                data-section="${sectionId}">

                <div
                    class="leave-modal-section-header">

                    <span>
                        ${title}
                    </span>

                    <span
                        class="leave-section-arrow">

                        ⌃

                    </span>

                </div>

                <div
                    class="leave-modal-section-content">

                    ${events.map(evt => `
                        <div
                            class="leave-modal-item">

                            <div
                                class="leave-modal-employee">

                                ${evt.employeeName}

                            </div>

                            <div
                                class="leave-modal-type">

                                ${getLeaveType(
            evt.status
        )}

                            </div>

                            <div
                                class="
                                    leave-modal-description
                                ">

                                ${evt.description}

                            </div>

                        </div>
                    `).join("")}

                </div>

            </div>
        `;
    }

    // ---------------------------------------------------------------
    // 5. Calendar controls
    // ---------------------------------------------------------------

    function wireHeaderControls() {
        const prevButton =
            document.getElementById(
                "calPrevBtn"
            );

        const nextButton =
            document.getElementById(
                "calNextBtn"
            );

        const todayButton =
            document.getElementById(
                "calTodayBtn"
            );

        if (prevButton) {
            prevButton.addEventListener(
                "click",
                function () {
                    currentDate.setMonth(
                        currentDate.getMonth() - 1
                    );

                    renderCalendar();
                }
            );
        }

        if (nextButton) {
            nextButton.addEventListener(
                "click",
                function () {
                    currentDate.setMonth(
                        currentDate.getMonth() + 1
                    );

                    renderCalendar();
                }
            );
        }

        if (todayButton) {
            todayButton.addEventListener(
                "click",
                function () {
                    currentDate =
                        new Date();

                    renderCalendar();
                }
            );
        }
    }

    // ---------------------------------------------------------------
    // 6. Calendar initialization
    // ---------------------------------------------------------------

    function initCalendar() {
        if (calendarInitialized) {
            return;
        }

        fetchLeaveEvents()
            .then(function (records) {
                leaveRecords =
                    records || [];

                renderCalendar();

                wireHeaderControls();

                calendarInitialized =
                    true;
            })
            .catch(function (error) {
                console.error(
                    "Failed to load leave calendar:",
                    error
                );
            });
    }

    // ---------------------------------------------------------------
    // 7. Tab switching
    // ---------------------------------------------------------------

    function showTab(target) {
        const tabRequest =
            document.getElementById(
                "tabRequest"
            );

        const tabCalendar =
            document.getElementById(
                "tabCalendar"
            );

        const requestCard =
            document.getElementById(
                "requestCard"
            );

        const calendarCard =
            document.getElementById(
                "calendarCard"
            );

        const showingCalendar =
            target === "calendar";

        if (
            !tabRequest ||
            !tabCalendar
        ) {
            return;
        }

        tabRequest.classList.toggle(
            "active",
            !showingCalendar
        );

        tabCalendar.classList.toggle(
            "active",
            showingCalendar
        );

        if (requestCard) {
            requestCard.style.display =
                showingCalendar
                    ? "none"
                    : "";
        }

        if (calendarCard) {
            calendarCard.style.display =
                showingCalendar
                    ? ""
                    : "none";
        }

        if (showingCalendar) {
            initCalendar();
        }
    }

    function initTabs() {
        const tabRequest =
            document.getElementById(
                "tabRequest"
            );

        const tabCalendar =
            document.getElementById(
                "tabCalendar"
            );

        if (
            !tabRequest ||
            !tabCalendar
        ) {
            return;
        }

        tabRequest.addEventListener(
            "click",
            function () {
                showTab("request");
            }
        );

        tabCalendar.addEventListener(
            "click",
            function () {
                showTab("calendar");
            }
        );

        if (
            tabCalendar.classList.contains(
                "active"
            )
        ) {
            showTab("calendar");
        }
    }

    // ---------------------------------------------------------------
    // 8. Inject styles
    // ---------------------------------------------------------------

    function injectStyles() {
        const style =
            document.createElement("style");

        style.textContent = `

            .leave-calendar-header-row {
                display: grid;

                grid-template-columns:
                    repeat(
                        7,
                        minmax(0, 1fr)
                    );
            }

            .leave-week {
                border-bottom:
                    1px solid
                    var(
                        --bs-gray-200,
                        #E4E6EF
                    );
            }

            .leave-week:last-child {
                border-bottom: none;
            }

            /* -------------------------------------------------- */
            /* Leave bar */
            /* -------------------------------------------------- */

            .leave-calendar-event {
                box-sizing: border-box;

                display: flex;

                flex-direction: column;

                justify-content: center;

                padding: 4px 10px;

                border-radius: 8px;

                border: 2px solid;

                font-size: 11px;

                font-weight: 700;

                overflow: hidden;

                white-space: nowrap;

                text-overflow: ellipsis;

                cursor: pointer;

                transition:
                    transform 0.15s ease,
                    box-shadow 0.15s ease;
            }

            .leave-calendar-event:hover {
                transform:
                    translateY(-1px);

                box-shadow:
                    0 4px 10px
                    rgba(
                        0,
                        0,
                        0,
                        0.12
                    );
            }

            .leave-calendar-event.approved {
                background:
                    var(
                        --bs-light-success,
                        #DFF7E6
                    );

                color:
                    var(
                        --bs-success,
                        #027A48
                    );

                border-color:
                    var(
                        --bs-success,
                        #027A48
                    );
            }

            .leave-calendar-event.pending {
                background:
                    var(
                        --bs-light-warning,
                        #FEF6DC
                    );

                color:
                    var(
                        --bs-warning,
                        #93700B
                    );

                border-color:
                    var(
                        --bs-warning,
                        #93700B
                    );
            }

            .leave-event-name {
                font-weight: 800;

                overflow: hidden;

                text-overflow: ellipsis;
            }

            .leave-event-status {
                font-size: 9px;

                font-weight: 600;

                opacity: 0.8;
            }

            /* -------------------------------------------------- */
            /* More pill */
            /* -------------------------------------------------- */

            .leave-calendar-more {
                box-sizing: border-box;

                display: flex;

                align-items: center;

                padding: 0 10px;

                border-radius: 6px;

                font-size: 11px;

                font-weight: 600;

                background: #F2F4F7;

                color: #475467;

                cursor: pointer;
            }

            .leave-calendar-more:hover {
                background: #E4E7EC;

                color: #344054;
            }

            /* -------------------------------------------------- */
            /* Day modal */
            /* -------------------------------------------------- */

            .leave-day-modal-overlay {
                position: fixed;

                inset: 0;

                z-index: 9999;

                display: flex;

                align-items: center;

                justify-content: center;

                background:
                    rgba(
                        0,
                        0,
                        0,
                        0.35
                    );
            }

            .leave-day-modal {
                width: min(
                    900px,
                    90vw
                );

                max-height: 80vh;

                overflow-y: auto;

                background: #ffffff;

                border-radius: 14px;

                padding: 28px;

                box-shadow:
                    0 20px 40px
                    rgba(
                        0,
                        0,
                        0,
                        0.18
                    );
            }

            .leave-day-modal-header {
                display: flex;

                align-items: center;

                justify-content: space-between;

                margin-bottom: 24px;
            }

            .leave-day-modal-header h2 {
                margin: 0;

                color: #D41446;

                font-size: 28px;

                font-weight: 800;
            }

            .leave-day-modal-close {
                border: none;

                background: transparent;

                color: #101828;

                font-size: 28px;

                line-height: 1;

                cursor: pointer;
            }

            /* -------------------------------------------------- */
            /* Modal section */
            /* -------------------------------------------------- */

            .leave-modal-section {
                margin-bottom: 20px;
            }

            .leave-modal-section-header {
                display: flex;

                align-items: center;

                justify-content: space-between;

                padding: 16px 20px;

                border: 1px solid #D9E2EC;

                border-radius: 8px;

                background: #F8FAFC;

                color: #344054;

                font-size: 16px;

                font-weight: 700;

                cursor: pointer;
            }

            .leave-modal-section-header:hover {
                background: #F2F4F7;
            }

            .leave-modal-section-content {
                padding: 12px 0;
            }

            .leave-modal-item {
                display: grid;

                grid-template-columns:
                    110px
                    120px
                    1fr;

                align-items: center;

                gap: 24px;

                padding: 14px 0;

                border-bottom:
                    1px solid
                    #F2F4F7;
            }

            .leave-modal-item:last-child {
                border-bottom: none;
            }

            .leave-modal-employee {
                display: flex;

                align-items: center;

                justify-content: center;

                min-height: 40px;

                padding: 0 12px;

                border: 1px solid #6CCB6C;

                border-radius: 10px;

                background: #E8F8E8;

                color: #159447;

                font-size: 13px;

                font-weight: 700;

                text-align: center;
            }

            .leave-modal-type {
                font-size: 14px;

                font-weight: 800;

                color: #101828;
            }

            .leave-modal-description {
                font-size: 12px;

                line-height: 1.6;

                color: #344054;
            }

            @media (max-width: 768px) {

                .leave-day-modal {
                    width: 95vw;

                    padding: 18px;
                }

                .leave-modal-item {
                    grid-template-columns:
                        1fr;

                    gap: 8px;
                }

            }

        `;

        document.head.appendChild(style);
    }

    // ---------------------------------------------------------------
    // 9. Month picker
    // ---------------------------------------------------------------

    const calendarMonthTrigger =
        document.getElementById(
            "calendarMonthTrigger"
        );

    const monthPickerDropdown =
        document.getElementById(
            "monthPickerDropdown"
        );

    const monthPickerYear =
        document.getElementById(
            "monthPickerYear"
        );

    const monthGrid =
        document.getElementById(
            "monthGrid"
        );

    const prevYearBtn =
        document.getElementById(
            "prevYearBtn"
        );

    const nextYearBtn =
        document.getElementById(
            "nextYearBtn"
        );

    const calTodayBtn =
        document.getElementById(
            "calTodayBtn"
        );

    let monthPickerCurrentYear =
        currentDate.getFullYear();

    const monthShortNames = [
        "Jan", "Feb", "Mar", "Apr",
        "May", "Jun", "Jul", "Aug",
        "Sep", "Oct", "Nov", "Dec"
    ];

    function renderMonthPicker() {
        if (
            !monthPickerYear ||
            !monthGrid
        ) {
            return;
        }

        monthPickerYear.textContent =
            monthPickerCurrentYear;

        monthGrid.innerHTML = "";

        monthShortNames.forEach(
            (month, index) => {
                const button =
                    document.createElement(
                        "button"
                    );

                button.type =
                    "button";

                button.className =
                    "month-option";

                button.textContent =
                    month;

                if (
                    currentDate.getFullYear() ===
                    monthPickerCurrentYear &&
                    currentDate.getMonth() ===
                    index
                ) {
                    button.classList.add(
                        "active"
                    );
                }

                button.addEventListener(
                    "click",
                    () => {
                        currentDate =
                            new Date(
                                monthPickerCurrentYear,
                                index,
                                1
                            );

                        renderCalendar();

                        closeMonthPicker();
                    }
                );

                monthGrid.appendChild(
                    button
                );
            }
        );
    }

    function openMonthPicker() {
        if (
            !monthPickerDropdown ||
            !calendarMonthTrigger
        ) {
            return;
        }

        monthPickerCurrentYear =
            currentDate.getFullYear();

        renderMonthPicker();

        monthPickerDropdown.classList.add(
            "show"
        );

        calendarMonthTrigger.classList.add(
            "active"
        );
    }

    function closeMonthPicker() {
        if (
            !monthPickerDropdown ||
            !calendarMonthTrigger
        ) {
            return;
        }

        monthPickerDropdown.classList.remove(
            "show"
        );

        calendarMonthTrigger.classList.remove(
            "active"
        );
    }

    if (calendarMonthTrigger) {
        calendarMonthTrigger.addEventListener(
            "click",
            event => {
                event.stopPropagation();

                if (
                    monthPickerDropdown.classList.contains(
                        "show"
                    )
                ) {
                    closeMonthPicker();
                } else {
                    openMonthPicker();
                }
            }
        );
    }

    if (prevYearBtn) {
        prevYearBtn.addEventListener(
            "click",
            () => {
                monthPickerCurrentYear--;

                renderMonthPicker();
            }
        );
    }

    if (nextYearBtn) {
        nextYearBtn.addEventListener(
            "click",
            () => {
                monthPickerCurrentYear++;

                renderMonthPicker();
            }
        );
    }

    document.addEventListener(
        "click",
        event => {
            if (
                !event.target.closest(
                    ".month-picker-wrapper"
                )
            ) {
                closeMonthPicker();
            }
        }
    );

    if (calTodayBtn) {
        calTodayBtn.addEventListener(
            "click",
            () => {
                currentDate =
                    new Date();

                renderCalendar();

                calTodayBtn.classList.add(
                    "active"
                );

                setTimeout(
                    () => {
                        calTodayBtn.classList.remove(
                            "active"
                        );
                    },
                    300
                );
            }
        );
    }

    // ---------------------------------------------------------------
    // 10. Init
    // ---------------------------------------------------------------

    document.addEventListener(
        "DOMContentLoaded",
        function () {
            injectStyles();

            initTabs();
        }
    );

})();