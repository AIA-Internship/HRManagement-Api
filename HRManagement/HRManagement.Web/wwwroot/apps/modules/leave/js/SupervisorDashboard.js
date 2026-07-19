const API_URL = "https://localhost:7089"; // placeholder
let requests = [];
let filteredRequests = [];
let employees = []
let currentPage = 1;
const pageSize = 8;
let activeStatus = null; // currently selected status filter (null = no filter)

window.app = window.app || {
    loading: {
        show: function (msg) {
            let overlay = document.getElementById('app_loading_overlay');
            if (!overlay) {
                overlay = document.createElement('div');
                overlay.id = 'app_loading_overlay';
                overlay.style = "position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(255,255,255,0.7);z-index:9999;display:flex;flex-direction:column;align-items:center;justify-content:center;font-family:Inter,sans-serif;";
                overlay.innerHTML = `
                    <div class="spinner-border text-brand" role="status" style="width: 3rem; height: 3rem; color:#D31145"></div>
                    <div class="mt-4 fw-boldest text-gray-800 fs-4">${msg || 'Processing...'}</div>
                `;
                document.body.appendChild(overlay);
            }
            overlay.style.display = 'flex';
        },
        hide: function () {
            const overlay = document.getElementById('app_loading_overlay');
            if (overlay) overlay.style.display = 'none';
        }
    }
};
async function getEmployees() {
    try {
        const empIds = [...new Set(requests.map(x => x.requesterDisplayId))];
        const token = window.aiaAuth ? window.aiaAuth.getToken() : null;

        const responses = await Promise.all(
            empIds.map(async id => {
                const response = await fetch(`${API_URL}/api/employee/${id}`, {
                    headers: {
                        Authorization: token ? `Bearer ${token}` : undefined,
                        "Content-Type": "application/json"
                    }
                });

                if (response.status === 401) {
                    redirectToLogin();
                    throw new Error('Unauthorized');
                }

                if (!response.ok) {
                    // return null for missing employee to avoid breaking merge
                    return null;
                }

                const result = await response.json();
                return result.content;
            })
        );

        employees = responses.filter(r => r != null);
        mergeData();
    } catch (err) {
        console.error('getEmployees error', err);
        // if unauthorized, redirectToLogin already called; otherwise continue with available data
        employees = [];
        mergeData();
    }
}

function redirectToLogin() {
    try {
        // clear local session storage keys used by auth
        localStorage.removeItem('aia_user_info');
    } catch (e) {}
    // redirect to login page
    window.location.href = '/Account/Login';
}

function isAuthenticated() {
    try {
        if (window.aiaAuth && typeof window.aiaAuth.getToken === 'function') {
            const t = window.aiaAuth.getToken();
            return !!t;
        }
    } catch (e) {}
    const u = localStorage.getItem('aia_user_info');
    return !!u;
}
function mergeData() {
    if (requests.length === 0) {
        renderTable()
        return
    }
    requests = requests.map(req => {

        const emp = employees.find(e => e.id === req.requesterId);

        return {
            ...req,

            initials: emp?.fullName
                ?.split(" ")
                .map(x => x[0])
                .join("")
                .substring(0, 2)
                .toUpperCase() || "--",

            name: emp?.fullName || "-",

            position: emp?.position || "-",

            type:
                req.leaveType === "1"
                    ? "Annual Leave"
                    : req.leaveType === "2"
                        ? "Sick Leave"
                        : "Other",

            startDateRaw: new Date(req.leaveStartDate),
            startDate: new Date(req.leaveStartDate).toLocaleDateString("en-GB", {
                weekday: 'long',
                day: "numeric",
                month: "long",
                year: "numeric"
            }),

            endDate: new Date(req.endDate).toLocaleDateString("en-GB", {
                weekday: 'long',
                day: "numeric",
                month: "long",
                year: "numeric"
            }),

            days: req.dayAmount,

            status:
                req.leaveStatus === "1"
                    ? "Needs Approval"
                    : req.leaveStatus === "2"
                        ? "Approved"
                        : "Rejected",

            statusClass:
                req.leaveStatus === "1"
                    ? "status-needs-approval"
                    : req.leaveStatus === "2"
                        ? "status-approved"
                        : "status-rejected",

            avatarColor: getAvatarColor(emp?.fullName || "--")
        };
    });

    // Default sort: Needs Approval first, then by start date (oldest first)
    const priority = {
        'Needs Approval': 0,
        'Approved': 1,
        'Rejected': 2
    };

    requests.sort((a, b) => {
        const pa = priority[a.status] ?? 9;
        const pb = priority[b.status] ?? 9;
        if (pa !== pb) return pa - pb;
        return a.startDateRaw - b.startDateRaw;
    });

    applyFilters();
}

function getAvatarColor(name) {
    const colors = [
        '#FF69B4', // Pink
        '#87CEEB', // Sky Blue
        '#90EE90', // Light Green
        '#FFD700', // Gold
        '#FF6347', // Tomato
        '#4169E1', // Royal Blue
        '#FFA500'  // Orange
    ];

    let hash = 0;
    for (let i = 0; i < name.length; i++) {
        hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }

    const index = Math.abs(hash) % colors.length;
    return colors[index];
}

function loadUser() {
    const user = JSON.parse(localStorage.getItem("aia_user_info"));

    if (user) {
        document.getElementById("userName").textContent = user.full_name || "Supervisor";

        if (user.role_id !== "1") {
            redirectToDashboard(user.role_id);
        }
    }
    return;
}


async function fetchRequests() {

    app.loading.show();

    try {

        const token = window.aiaAuth.getToken();

        const response = await fetch(API_URL + "/api/leave/get-by-supervisor-id", {
            method: "GET",
            headers: {
                Authorization: `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        });

        if (response.status === 401) {
            // session expired / unauthorized
            redirectToLogin();
            return;
        }

        if (!response.ok) {
            throw new Error("Failed to fetch");
        }

        responseData = await response.json();

        requests = responseData.content || [];

        // fetch employee details then merge and render inside getEmployees/mergeData
        getEmployees();

    } catch (err) {
        console.error(err);
        app.loading.hide();

        filteredRequests = requests;
        renderTable();
    }
    app.loading.hide();
}

function renderTable() {
    const tbody = document.getElementById("requestTableBody");
    tbody.innerHTML = "";

    const start = (currentPage - 1) * pageSize;
    const paginated = filteredRequests.slice(start, start + pageSize);

    if (paginated.length === 0) {
        tbody.innerHTML = `
            <tr class="empty-row">
                <td colspan="6" class="text-center">
                    <div class="empty-state">
                        <div style="height:180px; display:flex; align-items:center; justify-content:center;">
                            <div class="text-gray-500">No data available in table</div>
                        </div>
                    </div>
                </td>
            </tr>
        `;

        renderPagination();
        return;
    }

    paginated.forEach(req => {
        const statusIcon = req.statusClass === 'status-needs-approval' 
            ? '⊙' 
            : req.statusClass === 'status-approved'
                ? '✓'
                : '✕';

        tbody.innerHTML += `
            <tr>
                <td>
                    <div class="employee-cell">
                        <div class="employee-avatar" style="background-color: ${req.avatarColor};">${req.initials}</div>
                        <div class="employee-details">
                            <p class="employee-name">${req.name}</p>
                            <p class="employee-role">${req.position}</p>
                        </div>
                    </div>
                </td>
                <td class="leave-type">${req.type}</td>
                <td class="date-text">${req.startDate}</td>
                <td class="date-text">${req.endDate}</td>
                <td class="days-cell">${req.days}</td>
                <td class="text-center">
                    <button class="btn leave-status-btn ${req.statusClass}" tabindex="-1">
                        ${req.status}
                    </button>
                </td>
            </tr>
        `;
    });

    renderPagination();
}

function renderPagination() {

    const totalPages = Math.ceil(filteredRequests.length / pageSize);

    const container = document.getElementById("paginationList");

    container.innerHTML = "";

    if (totalPages <= 1) return;

    container.innerHTML += `
        <li class="page-item ${currentPage === 1 ? "disabled" : ""}">
            <a class="page-link" href="#" onclick="goToPage(${currentPage - 1});return false;">
                <i class="ki-outline ki-left fs-5"></i>
            </a>
        </li>
    `;

    for (let i = 1; i <= totalPages; i++) {

        container.innerHTML += `
            <li class="page-item ${i === currentPage ? "active" : ""}">
                <a class="page-link" href="#" onclick="goToPage(${i});return false;">
                    ${i}
                </a>
            </li>
        `;
    }

    container.innerHTML += `
        <li class="page-item ${currentPage === totalPages ? "disabled" : ""}">
            <a class="page-link" href="#" onclick="goToPage(${currentPage + 1});return false;">
                <i class="ki-outline ki-right fs-5"></i>
            </a>
        </li>
    `;

    const total = filteredRequests.length;
    const from = total === 0 ? 0 : ((currentPage - 1) * pageSize + 1);
    const to = total === 0 ? 0 : Math.min(currentPage * pageSize, total);
    document.getElementById("showingText").textContent = `Showing ${from}-${to} of ${total} requests`;
}

function goToPage(page) {
    currentPage = page;
    renderTable();
}

function filterStatus(status, button) {

    const statusMap = {
        "1": "Needs Approval",
        "2": "Approved",
        "3": "Rejected"
    };

    const selected = statusMap[status];

    document
        .querySelectorAll(".status-filter-btn")
        .forEach(btn => btn.classList.remove("active"));

    if (activeStatus === selected) {
        activeStatus = null;
    }
    else {
        activeStatus = selected;
        button.classList.add("active");
    }

    applyFilters();
}





function addlistener() {

    document.addEventListener("DOMContentLoaded", function () {
        // if not authenticated, redirect to login
        if (!isAuthenticated()) {
            redirectToLogin();
            return;
        }

        // load user if needed
        try { loadUser(); } catch (e) {}
        fetchRequests();

        // initial layout adjustment (no @media usage)
        adjustToolbarLayout();
    });
}

// Toggle stacked toolbar class based on window width (no @media)
function adjustToolbarLayout() {
    const card = document.querySelector('.card.mx-10.mb-15');
    if (!card) return;

    // threshold matches previous media breakpoint (992px)
    if (window.innerWidth < 992) {
        card.classList.add('stacked-toolbar');
    } else {
        card.classList.remove('stacked-toolbar');
    }
}

// debounce helper for resize
function debounce(fn, wait) {
    let t;
    return function () {
        clearTimeout(t);
        t = setTimeout(() => fn.apply(this, arguments), wait);
    };
}

// listen to resize to adjust layout dynamically
window.addEventListener('resize', debounce(adjustToolbarLayout, 120));

function addSortLogic() {

    document
        .getElementById("sortSelect")
        .addEventListener("change", function () {

            applyFilters();

        });
}

function addSearchLogic() {

    document
        .getElementById("searchInput")
        .addEventListener("input", function () {

            applyFilters();

        });
}

function renderSupervisorInfo() {

    const user = window.aiaAuth.getUserInfo();

    if (!user) return;

    document.getElementById("userName").textContent =
        user.fullName || "Supervisor";
}
function addTabLogic() {
    document.addEventListener('DOMContentLoaded', function () {
        var tabRequest = document.getElementById('tabRequest');
        var tabCalendar = document.getElementById('tabCalendar');
        var requestCard = document.getElementById('requestCard');
        var calendarCard = document.getElementById('calendarCard');
        var calendarInitialized = false;
        function activate(tab, card, otherTab, otherCard) {
            tab.classList.add('active');
            otherTab.classList.remove('active');

            // Fade out the currently visible card
            otherCard.style.opacity = '0';

            setTimeout(function () {
                otherCard.style.display = 'none';

                // Prep the incoming card to fade in
                card.style.display = '';
                card.style.opacity = '0';

                // Force reflow so the browser registers opacity:0 before transitioning
                void card.offsetWidth;

                card.style.opacity = '1';
            }, 180); // matches the CSS transition duration
        }

        tabRequest.addEventListener('click', function () {
            activate(tabRequest, requestCard, tabCalendar, calendarCard);
        });

        tabCalendar.addEventListener('click', function () {
            activate(tabCalendar, calendarCard, tabRequest, requestCard);

            // Initialize the calendar plugin lazily, only once, the first
            // time the Calendar tab is opened (avoids sizing issues that
            // happen when a calendar is initialized inside a hidden element).
            if (!calendarInitialized) {
                if (typeof KTAppCalendar !== 'undefined' && typeof KTAppCalendar.init === 'function') {
                    KTAppCalendar.init();
                }
                calendarInitialized = true;
            }
        });
    });
} 

function applyFilters() {

    let result = [...requests];

    // Status filter
    if (activeStatus) {
        result = result.filter(x => x.status === activeStatus);
    }

    // Search filter
    const keyword = document
        .getElementById("searchInput")
        .value
        .trim()
        .toLowerCase();

    if (keyword) {
        result = result.filter(x =>
            x.name.toLowerCase().includes(keyword) ||
            x.type.toLowerCase().includes(keyword)
        );
    }

    // Sort
    const sort = document.getElementById("sortSelect").value;

    result.sort((a, b) => {
        return sort === "newest"
            ? b.startDateRaw - a.startDateRaw
            : a.startDateRaw - b.startDateRaw;
    });

    filteredRequests = result;
    currentPage = 1;
    renderTable();
}

function main() {
    addlistener();
    addSearchLogic();
    addSortLogic();
    renderSupervisorInfo();
    addTabLogic();
}
main();