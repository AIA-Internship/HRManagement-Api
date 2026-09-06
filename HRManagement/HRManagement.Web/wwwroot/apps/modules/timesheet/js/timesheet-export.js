/**
 * AIA Timesheet Excel Export (V2.0)
 * Template column mapping (confirmed by inspection):
 *  Row 2  Col D (idx 3)  = "Timesheet" title (existing)
 *  Row 2  Col G (idx 6)  = Employee Name
 *  Row 3  Col D (idx 3)  = Month-Year label (e.g. "Jul-26")
 *  Row 4               = Header labels (untouched)
 *  Row 5+ Col B (idx 1)  = Day abbreviation (Mon/Tue/...)
 *         Col C (idx 2)  = Date label (e.g. "1-Jul-26")
 *         Col D (idx 3)  = Total Hours (decimal number)
 *         Col E (idx 4)  = Work Description
 *         Col F (idx 5)  = Work Location
 *         Col G (idx 6)  = Project Name
 */

const TIMESHEET_TEMPLATE_URL = '/assets/templates/Timesheet_AIA_Template.xlsx';

async function exportTimesheet(year, month) {
    const token = localStorage.getItem('aia_jwt_token');
    if (!token) { alert('Session expired. Please log in again.'); return; }

    if (typeof app !== 'undefined' && app.loading) app.loading.show('Generating Excel...');

    try {
        // 1. Fetch report data
        const apiUrl = `https://localhost:7089/api/timesheet/report?year=${year}&month=${month}`;
        const res = await fetch(apiUrl, { headers: { 'Authorization': `Bearer ${token}` } });
        if (!res.ok) { alert('Failed to fetch timesheet data. Status: ' + res.status); return; }

        const json = await res.json();
        if (json.isError || !json.content) {
            alert('Error: ' + (json.message || 'Could not load timesheet data.'));
            return;
        }
        const data = json.content; // ReportTimesheetResponseDto

        // 2. Load template as ArrayBuffer
        const tplRes = await fetch(TIMESHEET_TEMPLATE_URL);
        if (!tplRes.ok) { alert('Could not load Excel template.'); return; }
        const tplBuffer = await tplRes.arrayBuffer();

        // 3. Parse with SheetJS (preserve styles)
        const wb = XLSX.read(tplBuffer, { type: 'array', cellStyles: true, cellNF: true });
        const ws = wb.Sheets[wb.SheetNames[0]];

        // Helper: write a value into a cell, preserving existing style
        function setCell(r, c, value, type) {
            const ref = XLSX.utils.encode_cell({ r, c });
            const existing = ws[ref] || {};
            ws[ref] = {
                ...existing,
                v: value,
                t: type !== undefined ? type : (typeof value === 'number' ? 'n' : 's'),
                w: String(value)
            };
        }

        // 4. Month constants
        const MONTHS_LONG  = ["January","February","March","April","May","June",
                               "July","August","September","October","November","December"];
        const MONTHS_SHORT = ["Jan","Feb","Mar","Apr","May","Jun",
                               "Jul","Aug","Sep","Oct","Nov","Dec"];
        const DAY_ABBREV   = { Sunday:'Sun', Monday:'Mon', Tuesday:'Tue', Wednesday:'Wed',
                                Thursday:'Thu', Friday:'Fri', Saturday:'Sat' };
        const DOW_NAMES    = ['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'];

        const shortMonth = MONTHS_SHORT[month - 1];           // "Jul"
        const shortYear  = String(year).slice(-2);             // "26"
        const monthLabel = `${shortMonth}-${shortYear}`;       // "Jul-26"

        // 5. Fill header (row indices are 0-based)
        // Row 2 (index 1): Col G (index 6) = employee name
        setCell(1, 6, data.employeeName || '');
        // Row 3 (index 2): Col D (index 3) = month-year label
        setCell(2, 3, monthLabel);

        // 6. Build a lookup: dateKey (yyyy-MM-dd) → day data
        const dayMap = {};
        (data.days || []).forEach(d => { dayMap[d.date] = d; });

        const daysInMonth = new Date(year, month, 0).getDate();
        const DATA_START_ROW = 4; // 0-based index for Row 5

        for (let day = 1; day <= daysInMonth; day++) {
            const rowIdx = DATA_START_ROW + day - 1;

            // Date key matching API format
            const mm      = String(month).padStart(2, '0');
            const dd      = String(day).padStart(2, '0');
            const dateKey = `${year}-${mm}-${dd}`;

            // Day abbreviation
            const jsDate   = new Date(year, month - 1, day);
            const dowFull  = DOW_NAMES[jsDate.getDay()];
            const dowShort = DAY_ABBREV[dowFull] || dowFull.slice(0, 3);

            // Date label in template style: "1-Jul-26"
            const dateLabel = `${day}-${shortMonth}-${shortYear}`;

            // Col B (idx 1): Day abbrev
            setCell(rowIdx, 1, dowShort);
            // Col C (idx 2): Date label (as string to keep format)
            setCell(rowIdx, 2, dateLabel);

            const dayData = dayMap[dateKey];

            if (dayData && dayData.totalMinutes > 0) {
                const hoursDecimal = parseFloat((dayData.totalMinutes / 60).toFixed(2));
                const taskStr      = (dayData.tasks     || []).join('\r\n');
                const locStr       = [...new Set(dayData.locations || [])].join(', ');
                const projStr      = [...new Set(dayData.projects  || [])].join(', ');

                setCell(rowIdx, 3, hoursDecimal, 'n'); // Col D: Hours
                setCell(rowIdx, 4, taskStr);            // Col E: Work Description
                setCell(rowIdx, 5, locStr);             // Col F: Work Location
                setCell(rowIdx, 6, projStr);            // Col G: Project Name

            } else if (dayData && dayData.remark && dayData.remark !== '') {
                // OFF / HOLIDAY / PERSONAL LEAVE
                setCell(rowIdx, 3, 0, 'n');
                setCell(rowIdx, 4, dayData.remark);
                setCell(rowIdx, 5, '');
                setCell(rowIdx, 6, '');
            } else {
                // Weekend with no data – leave hours blank
                setCell(rowIdx, 3, '', 's');
                setCell(rowIdx, 4, '');
                setCell(rowIdx, 5, '');
                setCell(rowIdx, 6, '');
            }
        }

        // 7. Footer rows – find "Signed by:" and "Approved by:" rows, write names below them
        //    Template places footer several rows after last data row.
        //    We search for a cell containing "Signed by:" text to locate the footer dynamically.
        let signedByRow = -1;
        const fullRange = XLSX.utils.decode_range(ws['!ref']);
        for (let r = DATA_START_ROW + daysInMonth; r <= fullRange.e.r; r++) {
            for (let c = 0; c <= fullRange.e.c; c++) {
                const cell = ws[XLSX.utils.encode_cell({ r, c })];
                if (cell && typeof cell.v === 'string' && cell.v.toLowerCase().includes('signed by')) {
                    signedByRow = r;
                    break;
                }
            }
            if (signedByRow >= 0) break;
        }

        if (signedByRow >= 0) {
            // Name goes 2 rows below "Signed by:"
            setCell(signedByRow + 2, 1, data.employeeName   || ''); // Col B
            setCell(signedByRow + 2, 6, data.supervisorName || ''); // Col G
        }

        // 8. Update sheet range
        const sheetRange = XLSX.utils.decode_range(ws['!ref'] || 'A1:G5');
        sheetRange.e.r = Math.max(sheetRange.e.r, DATA_START_ROW + daysInMonth);
        ws['!ref'] = XLSX.utils.encode_range(sheetRange);

        // 9. Download
        const empNameClean = (data.employeeName || '').replace(/\s+/g, '_');
        const fileName = `Timesheet_AIA_${empNameClean}_${MONTHS_LONG[month - 1]}_${year}.xlsx`;
        XLSX.writeFile(wb, fileName);

    } catch (err) {
        console.error('Export error:', err);
        alert('An error occurred during export. Please try again.');
    } finally {
        if (typeof app !== 'undefined' && app.loading) app.loading.hide();
    }
}
