function openLeaveModal(dateStr, data) {
    document.getElementById("modalDate").innerText = formatDate(dateStr);

    const approved = document.getElementById("approvedList");
    const pending = document.getElementById("pendingList");

    console.log("dateStr:", dateStr);

    approved.innerHTML = "";
    pending.innerHTML = "";

    const filtered = data.filter(item => {
        const itemDate = new Date(item.leaveStartDate)
            .toISOString()
            .split("T")[0];

        return itemDate === dateStr;
    });
    console.log("filtered data:", filtered);

    filtered.forEach(item => {
        const el = document.createElement("div");
        el.className = "card p-2 mb-2";

        el.innerText = `${item.leaveType} - ${item.leaveStatus}`;

        if (item.leaveStatus === "Approved") {
            approved.appendChild(el);
        } else {
            pending.appendChild(el);
        }
    });

    const modal = new bootstrap.Modal(document.getElementById('leaveCalendarModal'));
    modal.show();
}
function closeLeaveModal() {
    document.getElementById("leaveCalendarModal").style.display = "none";
}

function toggleSection(el) {
    const body = el.nextElementSibling;
    body.style.display = body.style.display === "none" ? "block" : "none";
}

function formatDate(dateStr) {
    const date = new Date(dateStr);
    return date.toLocaleDateString("en-GB", {
        day: "numeric",
        month: "long",
        year: "numeric"
    });
}