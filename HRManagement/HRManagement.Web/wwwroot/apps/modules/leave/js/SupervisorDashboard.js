const API_URL = "/api/leave/requests"; // placeholder
let requests = [];
let filteredRequests = [];
let currentPage = 1;
const pageSize = 8;

function redirectToDashboard(roleId) {
    if (roleId === "1") {
        window.location.href = "/Leave/Supervisor/Dashboard";
    } else {
        window.location.href = "/Leave/Employee/Dashboard";
    }
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
    try {
        // Placeholder backend call
        const response = await fetch(API_URL);

        if (response.ok) {
            requests = await response.json();
        } else {
            requests = mockData();
        }

        filteredRequests = requests;
        renderTable();
    } catch {
        requests = mockData();
        filteredRequests = requests;
        renderTable();
    }
}

function renderTable() {
    const tbody = document.getElementById("requestTableBody");
    tbody.innerHTML = "";

    const start = (currentPage - 1) * pageSize;
    const paginated = filteredRequests.slice(start, start + pageSize);

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
                    <td><span class="status-badge ${req.status}">${req.status}</span></td>
                </tr>
            `;
    });

    renderPagination();
}

function renderPagination() {
    const totalPages = Math.ceil(filteredRequests.length / pageSize);
    const container = document.getElementById("paginationButtons");

    container.innerHTML = "";

    for (let i = 1; i <= totalPages; i++) {
        container.innerHTML += `
                <button class="page-btn ${i === currentPage ? 'active' : ''}" onclick="goToPage(${i})">${i}</button>
            `;
    }

    document.getElementById("paginationText").textContent =
        `Showing ${(currentPage - 1) * pageSize + 1}-${Math.min(currentPage * pageSize, filteredRequests.length)} of ${filteredRequests.length} requests`;
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

function mockData() {
    return [
        {
            initials: "AF",
            name: "Arnold Frans",
            position: "Lead UI Designer",
            type: "Emergency Leave",
            startDate: "9 Mar 2026",
            endDate: "11 Mar 2026",
            days: 3,
            status: "pending"
        },
        {
            initials: "OP",
            name: "Owen Pangalila",
            position: "Project Manager",
            type: "Sick Leave",
            startDate: "10 Mar 2026",
            endDate: "10 Mar 2026",
            days: 1,
            status: "approved"
        }
    ];
}

document.addEventListener("DOMContentLoaded", function () {
    loadUser();
    fetchRequests();
});