// FR-0003 Request History — wired to /api/employee/me/requests

const API_BASE = "https://localhost:7089";

const state = {
    raw: [],
    sortKey: "createdAt",
    sortDir: "desc",
    statusFilter: "all",
    currentPage: 1,
    pageSize: 20
};

document.addEventListener("DOMContentLoaded", async () => {
    const tableBody = document.getElementById("requestTableBody");
    const banner = document.getElementById("pendingBanner");
    if (!tableBody) return;

    state.raw = await fetchMyRequests();

    bindSortHandlers();
    bindFilterHandlers();
    applySortIndicator();
    render();
    updateBanner(banner);
});

async function fetchMyRequests() {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    if (!token) { window.aiaAuth && window.aiaAuth.signOut(); return []; }
    try {
        const res = await fetch(`${API_BASE}/api/employee/requests`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (res.status === 401) { window.aiaAuth.signOut(); return []; }
        if (res.status === 404) return [];
        const json = await res.json();
        if (json.isError) return [];
        return json.content || json.data || [];
    } catch (err) {
        console.error("Failed to fetch requests:", err);
        return [];
    }
}

function render() {
    const tableBody = document.getElementById("requestTableBody");
    tableBody.innerHTML = "";

    const rows = state.raw.map(r => ({
        ...r,
        _status: normalizeStatus(r.status),
        _fieldsCount: countFields(r)
    }));

    const filtered = state.statusFilter === "all"
        ? rows
        : rows.filter(r => r._status === state.statusFilter);

    filtered.sort(compareRows);
    
    const totalItems = filtered.length;
    const totalPages = Math.ceil(totalItems / state.pageSize) || 1;

    if (state.currentPage > totalPages) state.currentPage = totalPages;
    if (state.currentPage < 1) state.currentPage = 1;

    const startIndex = (state.currentPage - 1) * state.pageSize;
    const pagedData = filtered.slice(startIndex, startIndex + state.pageSize);

    renderPagination(totalItems, startIndex, pagedData.length, totalPages);

    if (!filtered.length) {
        tableBody.innerHTML = `
            <tr><td colspan="4" class="text-center py-10 text-muted fs-6">No request found</td></tr>
        `;
        return;
    }

    filtered.forEach(req => {
        const tr = document.createElement("tr");
        tr.className = "row-clickable";
        tr.addEventListener("click", () => {
            window.location.href = `/EmployeeManagement/RequestDetails?id=${req.requestId}`;
        });
        tr.innerHTML = `
            <td class="ps-4"><span class="text-gray-900 fw-bold fs-6">#REQ${String(req.requestId).padStart(3, "0")}</span></td>
            <td><span class="text-gray-700 fw-semibold">${req.requesterName}</span></td>
            <td><span class="text-gray-700 fw-semibold">${formatDate(req.createdAt)}</span></td>
            <td><span class="text-gray-700 fw-semibold">${req._fieldsCount} Field${req._fieldsCount === 1 ? "" : "s"} Changed</span></td>
            <td class="pe-4">${renderStatus(req._status)}</td>
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

function compareRows(a, b) {
    const dir = state.sortDir === "asc" ? 1 : -1;
    let va, vb;
    switch (state.sortKey) {
        case "requestId":
            va = Number(a.requestId) || 0;
            vb = Number(b.requestId) || 0;
            break;
        case "createdAt":
            va = new Date(a.createdAt).getTime() || 0;
            vb = new Date(b.createdAt).getTime() || 0;
            break;
        case "fieldsCount":
            va = a._fieldsCount;
            vb = b._fieldsCount;
            break;
        default:
            va = vb = 0;
    }
    if (va < vb) return -1 * dir;
    if (va > vb) return 1 * dir;
    return 0;
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
            btn.classList.toggle("active", state.statusFilter !== "all");
            popup.classList.remove("open");
            render();
        });
    });
}

function updateBanner(banner) {
    if (!banner) return;
    const hasPending = state.raw.some(r => normalizeStatus(r.status) === "Pending");
    banner.classList.toggle("d-none", !hasPending);
}

function normalizeStatus(s) {
    const v = String(s || "").toLowerCase();
    if (v === "pending" || v === "0" || v === "needs approval") return "Pending";
    if (v === "approved" || v === "1") return "Approved";
    if (v === "rejected" || v === "2") return "Rejected";
    return s || "Unknown";
}

function countFields(req) {
    const fields = [
        "newFullName", "newGender", "newPersonalEmail", "newPlaceOfBirth","newNik", "newDateOfBirth", "newMaritalStatus",
        "newCurrentStreetAddress", "newCurrentCity", "newCurrentProvince", "newCurrentPostalCode",
        "newResidentialStreetAddress", "newResidentialCity", "newResidentialProvince", "newResidentialPostalCode",
        "newPhoneNumber", "newEmergencyContactName", "newEmergencyContactPhone", "newEmergencyContactRelationship",
    ];
    return fields.filter(f => req[f] !== undefined && req[f] !== null && req[f] !== "" && req[f] !== "Unknown").length;
}

function renderStatus(status) {
    if (status === "Pending") {
        return `<span class="badge-status badge-pending"><i class="bi bi-clock"></i>Needs Approval</span>`;
    }
    if (status === "Approved") {
        return `<span class="badge-status badge-approved"><i class="bi bi-check-circle"></i>Approved</span>`;
    }
    if (status === "Rejected") {
        return `<span class="badge-status badge-rejected"><i class="bi bi-x-circle"></i>Rejected</span>`;
    }
    return `<span class="badge-status">${status}</span>`;
}

function formatDate(dateStr) {
    if (!dateStr) return "-";
    const date = new Date(dateStr);
    if (isNaN(date)) return "-";
    return date.toLocaleString("en-GB", {
        day: "2-digit", month: "long", year: "numeric",
        hour: "2-digit", minute: "2-digit", hour12: false
    });
}
