document.addEventListener("DOMContentLoaded", () => {
    
    // 1. Get ID from URL
    const urlParams = new URLSearchParams(window.location.search);
    const requestId = urlParams.get('id');

    if (!requestId) {
        window.location.href = "/Profile/History";
        return;
    }

    // 2. Load Data from LocalStorage
    const requests = JSON.parse(localStorage.getItem("requests")) || [];
    const request = requests.find(r => r.id === requestId);

    if (!request) {
        alert("Request not found!");
        window.location.href = "/Profile/History";
        return;
    }

    // 3. Render Header Info
    document.getElementById("requestIdText").innerText = request.id;
    document.getElementById("submittedAtText").innerText = formatDate(request.submittedAt);
    document.getElementById("totalChangesText").innerText = request.fieldsChanged.length;

    // Status Badge
    const statusBadge = document.getElementById("statusBadge");
    statusBadge.innerText = request.status;
    
    const lowerStatus = request.status.toLowerCase();
    if (lowerStatus === "pending") statusBadge.classList.add("badge-light-pending");
    else if (lowerStatus === "approved") statusBadge.classList.add("badge-light-approved");
    else if (lowerStatus === "rejected") statusBadge.classList.add("badge-light-rejected");

    // 4. Render Changes Table
    const tableBody = document.getElementById("changesTableBody");
    tableBody.innerHTML = "";

    request.fieldsChanged.forEach(change => {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td class="ps-9 fw-bold text-gray-800">${change.field}</td>
            <td class="old-value">${change.old || '-'}</td>
            <td class="pe-9 text-aia fw-bold">${change.new || '-'}</td>
        `;
        tableBody.appendChild(tr);
    });

    // Helper Date Format
    function formatDate(dateStr) {
        const date = new Date(dateStr);
        return date.toLocaleString("en-GB", {
            day: "2-digit",
            month: "long",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit",
            hour12: true
        });
    }

});
