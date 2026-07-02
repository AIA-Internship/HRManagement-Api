const API_URL = "https://localhost:7089"; // placeholder
let requests = [];
let filteredRequests = [];
let employees = []
let currentPage = 1;
const pageSize = 8;

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
                `${API_URL}/api/employee/employment-info/${id}`,
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

    requests = requests.map(req => {

        const emp = employees.find(e => e.employeeId === req.requesterId);

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
                    ? "Pending"
                    : req.leaveStatus === "2"
                        ? "Approved"
                        : "Rejected"
        };
    });

    filteredRequests = [...requests];

    renderTable();
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

        requests = await response.json();

        filteredRequests = requests;

        getEmployees();
        mergeData();

        renderTable();

    } catch (err) {
        console.error(err);
        app.loading.hide();

        requests = mockData();
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
                        <i class="ki-outline ki-information-5 fs-1 text-gray-400 mb-3"></i>
                        <div class="fw-semibold text-gray-600 fs-5">
                            No data available in table
                        </div>
                    </div>
                </td>
            </tr>
        `;

        renderPagination();
        return;
    }

    paginated.forEach(req => {
        tbody.innerHTML += `
            <tr>
                <td>
                    <div class="employee-cell">
                        <div class="avatar">${req.initials}</div>
                        <div>
                            <strong>${req.name}</strong>
                            <div class="subtext">${req.position}</div>
                        </div>
                    </div>
                </td>
                <td>${req.type}</td>
                <td>${req.startDate}</td>
                <td>${req.endDate}</td>
                <td>${req.days}</td>
                <td class="text-center">
                    <span class="status-badge ${req.statusClass}">
                        ${req.status}
                    </span>
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

    document.getElementById("showingText").textContent =
        `Showing ${filteredRequests.length === 0 ? 0 : ((currentPage - 1) * pageSize + 1)}
        -${Math.min(currentPage * pageSize, filteredRequests.length)}
        of ${filteredRequests.length} requests`;
}

function goToPage(page) {
    currentPage = page;
    renderTable();
}

function filterStatus(status) {
    filteredRequests = requests.filter(r => r.status === status);
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

                filteredRequests.sort((a, b) =>
                    new Date(b.startDate) - new Date(a.startDate));

            } else {

                filteredRequests.sort((a, b) =>
                    new Date(a.startDate) - new Date(b.startDate));
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
    renderSupervisorInfo();
}
main();