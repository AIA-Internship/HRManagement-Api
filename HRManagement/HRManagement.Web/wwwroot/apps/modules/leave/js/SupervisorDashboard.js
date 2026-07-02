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

    const empIds = [...new Set(requests.map(x => x.requesterId))];
    const token = window.aiaAuth.getToken();

    const responses = await Promise.all(
        empIds.map(async id => {

            const response = await fetch(
                `${API_URL}/api/employee/${id}`,
                {
                    headers: {
                        Authorization: `Bearer ${token}`,
                        "Content-Type": "application/json"
                    }
                });

            const result = await response.json();

            return result.content;
        })
    );

    employees = responses;

    mergeData();
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
                day: "numeric",
                month: "short",
                year: "numeric"
            }),

            endDate: new Date(req.endDate).toLocaleDateString("en-GB", {
                day: "numeric",
                month: "short",
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

    filteredRequests = [...requests];

    renderTable();
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

        if (!response.ok) {
            throw new Error("Failed to fetch");
        }

        responseData = await response.json();

        requests = responseData.content;

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
                <td>${req.type}</td>
                <td>${req.startDate}</td>
                <td>${req.endDate}</td>
                <td>${req.days}</td>
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
    // Map status parameter to status text
    const statusMap = {
        '1': 'Needs Approval',
        '2': 'Approved',
        '3': 'Rejected'
    };

    const statusText = statusMap[status] || status;

    // Toggle behavior: if same status clicked, clear filter
    if (activeStatus === statusText) {
        activeStatus = null;
        filteredRequests = [...requests];
        // remove active class from buttons
        document.querySelectorAll('.btn-outline-secondary').forEach(btn => btn.classList.remove('active'));
    } else {
        activeStatus = statusText;
        filteredRequests = requests.filter(r => r.status === statusText);
        // update active class
        document.querySelectorAll('.btn-outline-secondary').forEach(btn => btn.classList.remove('active'));
        if (button) button.classList.add('active');
    }

    currentPage = 1;
    renderTable();
}






function addlistener() {

    document.addEventListener("DOMContentLoaded", function () {
        loadUser();
        fetchRequests();
    });
}

function addSortLogic() {
    document
        .getElementById("sortSelect")
        .addEventListener("change", function () {

            if (this.value === "newest") {

                filteredRequests.sort((a, b) => b.startDateRaw - a.startDateRaw);

            } else {

                filteredRequests.sort((a, b) => a.startDateRaw - b.startDateRaw);
            }

            renderTable();
        });
}

function addSearchLogic() {
    document
        .getElementById("searchInput")
        .addEventListener("input", function () {

            const keyword = this.value.toLowerCase();

            filteredRequests = requests.filter(x =>
                x.name.toLowerCase().includes(keyword) ||
                x.type.toLowerCase().includes(keyword)
            );

            currentPage = 1;

            renderTable();
        });
}

function renderSupervisorInfo() {

    const user = window.aiaAuth.getUserInfo();

    if (!user) return;

    document.getElementById("userName").textContent =
        user.fullName || "Supervisor";
}

function main() {
    addlistener();
    addSearchLogic();
    addSortLogic();
    renderSupervisorInfo();
}
main();