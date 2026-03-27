document.addEventListener("DOMContentLoaded", () => {

    const tableBody = document.getElementById("requestTableBody");
    const banner = document.getElementById("pendingBanner");

    if (!tableBody) return;

    initializeDummyData();
    const requests = getRequests();
    renderTable(requests);

    // INITIALIZE DUMMY DATA (ONLY ONCE)
    function initializeDummyData() {
        const existing = localStorage.getItem("requests");

        if (!existing) {
            const dummy = [
                {
                    id: "REQ001",
                    status: "Pending",
                    submittedAt: "2026-02-20T10:30:00",
                    fieldsChanged: [
                        { field: "Phone Number", old: "+62 812-3456-7890", new: "+62-811-2092-1234" },
                        { field: "Marital Status", old: "Single", new: "Married" }
                    ]
                },
                {
                    id: "REQ002",
                    status: "Approved",
                    submittedAt: "2026-02-10T10:30:00",
                    fieldsChanged: [
                        { field: "Current Address", old: "Jl. Sudirman No 1", new: "Jl. Gatot Subroto No. 42" }
                    ]
                },
                {
                    id: "REQ003",
                    status: "Rejected",
                    submittedAt: "2026-01-10T09:30:00",
                    fieldsChanged: [
                        { field: "Personal Email", old: "old@email.com", new: "new@email.com" },
                        { field: "Emergency Name", old: "John Doe", new: "Johnathan" },
                        { field: "Emergency Relationship", old: "Father", new: "Uncle" }
                    ]
                }
            ];

            localStorage.setItem("requests", JSON.stringify(dummy));
        }
    }

    // GET DATA FROM LOCALSTORAGE
    function getRequests() {
        return JSON.parse(localStorage.getItem("requests")) || [];
    }

    // RENDER TABLE
    function renderTable(data) {

        tableBody.innerHTML = "";

        if (!data.length) {
            tableBody.innerHTML = `
                <tr>
                    <td colspan="5" class="text-center py-10 text-muted fs-6">
                        No request history found
                    </td>
                </tr>
            `;
            banner.classList.add("d-none");
            return;
        }

        // Show banner ONLY if pending exists
        const hasPending = data.some(r => r.status === "Pending");
        banner.classList.toggle("d-none", !hasPending);
        if (hasPending) {
             banner.classList.remove("d-none");
             banner.classList.add("d-flex");
        }

        data.forEach(req => {

            const row = document.createElement("tr");
            row.className = "row-hover";

            row.innerHTML = `
                <td class="ps-4">
                    <span class="text-gray-800 fw-bold fs-6">#${req.id}</span>
                </td>
                <td class="text-center">
                    ${renderStatus(req.status)}
                </td>
                <td class="text-center">
                    <span class="text-gray-700 fw-semibold">${formatDate(req.submittedAt)}</span>
                </td>
                <td class="text-center">
                    <span class="badge badge-light-primary fw-bold px-4 py-3">${req.fieldsChanged.length} Field(s)</span>
                </td>
                <td class="text-end pe-4">
                    <a href="/Profile/RequestDetails?id=${req.id}" 
                       class="btn btn-icon btn-bg-light btn-active-color-primary btn-sm me-1">
                        <i class="ki-duotone ki-arrow-right fs-2">
                            <span class="path1"></span>
                            <span class="path2"></span>
                        </i>
                    </a>
                </td>
            `;

            tableBody.appendChild(row);
        });
    }

    // STATUS PILL RENDER
    function renderStatus(status) {

        const map = {
            Pending: "badge badge-light-pending px-4 py-3",
            Approved: "badge badge-light-approved px-4 py-3",
            Rejected: "badge badge-light-rejected px-4 py-3"
        };

        return `<span class="${map[status]} fs-7 fw-bold">${status}</span>`;
    }

    // DATE FORMAT
    function formatDate(dateStr) {

        const date = new Date(dateStr);

        return date.toLocaleString("en-GB", {
            day: "2-digit",
            month: "short",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit",
            hour12: true
        });
    }

});
