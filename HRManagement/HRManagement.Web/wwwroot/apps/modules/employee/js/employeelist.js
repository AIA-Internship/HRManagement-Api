const API_BASE = "https://localhost:7089";

const state = {
    raw: [],
    searchQuery: "",
    sortKey: "name",
    sortDir: "asc",
    statusFilter: "all",
    currentPage: 1,
    pageSize: 20 
};

document.addEventListener("DOMContentLoaded", async () => {
    const tableBody = document.getElementById("employeeTableBody");
    if (!tableBody) return;

    state.raw = await fetchEmployeeList();

    bindSearchHandler();
    bindSortHandlers();
    bindFilterHandlers();
    applySortIndicator();
    render();
    
    const btnAdd = document.getElementById("btnAddEmployee");
    if (btnAdd) {
        btnAdd.addEventListener("click", () => {
            window.location.href = "/Employee/Create"; 
        });
    }
});

async function fetchEmployeeList() {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    if (!token) { window.aiaAuth && window.aiaAuth.signOut(); return []; }

    try {
        const res = await fetch(`${API_BASE}/api/employee/list`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (res.status === 401) { window.aiaAuth.signOut(); return []; }
        if (res.status === 404) return [];
        const json = await res.json();
        if (json.isError) return [];
        return json.content || json.data || [];
    } catch (err) {
        console.error("Failed to fetch employee list:", err);
        return [];
    }
}

function render() {
    const tableBody = document.getElementById("employeeTableBody");
    tableBody.innerHTML = "";
    
    let filtered = state.statusFilter === "all" ? state.raw : state.raw.filter(emp => {
        const s = String(emp.employeeStatus || "").toLowerCase();
        if (state.statusFilter === "active") return s === "active" || s === "1";
        if (state.statusFilter === "inactive") return s === "inactive" || s === "resigned" || s === "0" || s === "on leave" || s === "leave";
        return true;
    });
    
    if (state.searchQuery) {
        const q = state.searchQuery.toLowerCase();
        filtered = filtered.filter(emp => {
            const idMatch = String(emp.employeeDisplayId || "").toLowerCase().includes(q);
            const nameMatch = String(emp.name || emp.fullName || "").toLowerCase().includes(q);
            const deptMatch = String(emp.department || "").toLowerCase().includes(q);
            const posMatch = String(emp.position || "").toLowerCase().includes(q);
            return idMatch || nameMatch || deptMatch || posMatch;
        });
    }
    
    filtered.sort(compareRows);
    
    const totalItems = filtered.length;
    const totalPages = Math.ceil(totalItems / state.pageSize) || 1;
    
    if (state.currentPage > totalPages) state.currentPage = totalPages;
    if (state.currentPage < 1) state.currentPage = 1;

    const startIndex = (state.currentPage - 1) * state.pageSize;
    const pagedData = filtered.slice(startIndex, startIndex + state.pageSize);
    
    renderPagination(totalItems, startIndex, pagedData.length, totalPages);
    
    if (!pagedData.length) {
        tableBody.innerHTML = `
            <tr><td colspan="5" class="text-center py-10 text-muted fs-6">No employees found</td></tr>
        `;
        return;
    }

    pagedData.forEach(emp => {
        const empId = emp.employeeDisplayId || emp.nik || "N/A";
        const name = emp.name || emp.fullName || "Unknown";
        const dept = emp.department || "-";
        const pos = emp.position || "-";
        const status = emp.employeeStatus || "Unknown";

        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td class="ps-4"><span class="text-gray-900 fw-bold fs-6">${empId}</span></td>
            <td><span class="text-gray-800 fw-semibold">${name}</span></td>
            <td><span class="text-gray-700 fw-semibold">${dept}</span></td>
            <td><span class="text-gray-700 fw-semibold">${pos}</span></td>
            <td class="pe-4">${renderStatus(status, empId)}</td>
        `;
        tableBody.appendChild(tr);
    });
}

function renderPagination(totalItems, startIndex, currentCount, totalPages) {
    const infoEl = document.getElementById("paginationInfo");
    const controlsEl = document.getElementById("paginationControls");

    if (totalItems === 0) {
        infoEl.textContent = "Showing 0 to 0 of 0 entries";
        controlsEl.innerHTML = "";
        return;
    }
    
    const endItem = startIndex + currentCount;
    infoEl.textContent = `Showing ${startIndex + 1} to ${endItem} of ${totalItems} entries`;
    
    const prevDisabled = state.currentPage === 1 ? "disabled" : "";
    const nextDisabled = state.currentPage === totalPages ? "disabled" : "";

    controlsEl.innerHTML = `
        <li class="page-item previous ${prevDisabled}">
            <a href="javascript:void(0)" class="page-link page-prev" style="border-radius: 6px; margin-right: 4px;">
                <i class="bi bi-chevron-left"></i> Previous
            </a>
        </li>
        <li class="page-item next ${nextDisabled}">
            <a href="javascript:void(0)" class="page-link page-next" style="border-radius: 6px;">
                Next <i class="bi bi-chevron-right"></i>
            </a>
        </li>
    `;

    if (!prevDisabled) {
        controlsEl.querySelector(".page-prev").addEventListener("click", () => {
            state.currentPage--;
            render();
        });
    }

    if (!nextDisabled) {
        controlsEl.querySelector(".page-next").addEventListener("click", () => {
            state.currentPage++;
            render();
        });
    }
}

function bindSearchHandler() {
    const searchInput = document.getElementById("searchInput");
    if (!searchInput) return;

    searchInput.addEventListener("input", (e) => {
        state.searchQuery = e.target.value.trim();
        state.currentPage = 1; 
        render();
    });
}

function bindSortHandlers() {
    document.querySelectorAll(".th-sortable").forEach(th => {
        th.addEventListener("click", () => {
            const key = th.dataset.sort;
            if (state.sortKey === key) {
                state.sortDir = state.sortDir === "asc" ? "desc" : "asc";
            } else {
                state.sortKey = key;
                state.sortDir = "asc";
            }
            state.currentPage = 1; 
            applySortIndicator();
            render();
        });
    });
}

function applySortIndicator() {
    document.querySelectorAll(".th-sortable").forEach(th => {
        th.classList.remove("asc", "desc");
        const icon = th.querySelector(".sort-ic i");
        if (th.dataset.sort === state.sortKey) {
            th.classList.add(state.sortDir);
            icon.className = state.sortDir === "asc" ? "bi bi-arrow-up" : "bi bi-arrow-down";
            icon.style.color = "#181C32";
        } else {
            icon.className = "bi bi-arrow-down-up";
            icon.style.color = "";
        }
    });
}

function bindFilterHandlers() {
    const btn = document.getElementById("statusFilterBtn");
    const popup = document.getElementById("statusFilterPopup");
    if (!btn || !popup) return;

    btn.addEventListener("click", (e) => {
        e.stopPropagation();
        popup.classList.toggle("open");
    });

    document.addEventListener("click", (e) => {
        if (!popup.contains(e.target) && e.target !== btn) {
            popup.classList.remove("open");
        }
    });

    popup.querySelectorAll(".filter-option").forEach(opt => {
        opt.addEventListener("click", () => {
            popup.querySelectorAll(".filter-option").forEach(o => o.classList.remove("selected"));
            opt.classList.add("selected");
            state.statusFilter = opt.dataset.value;
            state.currentPage = 1; // Reset to page 1 on filter change
            btn.classList.toggle("active", state.statusFilter !== "all");
            popup.classList.remove("open");
            render();
        });
    });
}

function compareRows(a, b) {
    const dir = state.sortDir === "asc" ? 1 : -1;
    let va = (a[state.sortKey] || a.fullName || "").toString().toLowerCase();
    let vb = (b[state.sortKey] || b.fullName || "").toString().toLowerCase();

    if (va < vb) return -1 * dir;
    if (va > vb) return 1 * dir;
    return 0;
}

function renderStatus(status, employeeId) {
    const s = String(status).toLowerCase();
    let badgeClass = "badge-active";
    let dotColor = "#1BC5BD"; 
    let text = "Active";

    if (s === "inactive" || s === "resigned" || s === "0") {
        badgeClass = "badge-inactive";
        dotColor = "#F1416C"; 
        text = "Inactive";
    } else if (s === "on leave" || s === "leave") {
        badgeClass = "badge-leave";
        dotColor = "#FFA800"; 
        text = "On Leave";
    } else if (s !== "active" && s !== "1") {
        text = status; 
    }
    
    return `
        <div class="d-flex align-items-center">
            <span class="badge-status ${badgeClass} me-3" style="width: 100px; justify-content: flex-start;">
                <span style="width: 8px; height: 8px; border-radius: 50%; background-color: ${dotColor}; display: inline-block;"></span>
                ${text}
            </span>
            <a href="/EmployeeManagement/EditProfile?id=${employeeId}" class="btn btn-icon btn-sm btn-light btn-active-light-primary w-25px h-25px ms-3" title="Edit Employment Details">
                <i class="bi bi-pencil fs-7"></i>
            </a>
        </div>
    `;
}