const requests = [
  {
    id: "REQ001",
    status: "Pending",
    submittedAt: "2026-02-20T10:30:00",
    fieldsChanged: [
      {
        field: "Phone Number",
        oldValue: "+62 812-3456-7890",
        newValue: "+62-811-2092-1234"
      },
      {
        field: "Marital Status",
        oldValue: "Single",
        newValue: "Married"
      }
    ]
  },
  {
    id: "REQ002",
    status: "Approved",
    submittedAt: "2026-02-10T10:30:00",
    fieldsChanged: [
      {
        field: "Address",
        oldValue: "Jakarta",
        newValue: "Bandung"
      }
    ]
  },
  {
    id: "REQ003",
    status: "Rejected",
    submittedAt: "2026-01-10T09:30:00",
    fieldsChanged: [
      {
        field: "Email",
        oldValue: "old@email.com",
        newValue: "new@email.com"
      },
      {
        field: "Phone",
        oldValue: "0812",
        newValue: "0813"
      },
      {
        field: "Department",
        oldValue: "HR",
        newValue: "Finance"
      }
    ]
  }
];

const hasPending = requests.some(r => 
  r.status.toLowerCase() === "pending"
);

if (hasPending) {
  document.getElementById("pendingAlert").classList.remove("d-none");
}

function formatDate(dateString) {
  const date = new Date(dateString);

  const day = date.getDate();
  const month = date.toLocaleString("en-GB", { month: "long" }).toLowerCase();
  const year = date.getFullYear();

  let hours = date.getHours();
  const minutes = date.getMinutes().toString().padStart(2, "0");
  const ampm = hours >= 12 ? "pm" : "am";

  hours = hours % 12;
  hours = hours ? hours : 12;

  return `${day} ${month} ${year}, ${hours}.${minutes} ${ampm}`;
}

const tableBody = document.getElementById("requestTable");

requests.forEach(req => {
  const tr = document.createElement("tr");

  tr.innerHTML = `
    <td>#${req.id}</td>
    <td>${renderStatus(req.status)}</td>
    <td>${formatDate(req.submittedAt)}</td>
    <td>${req.fieldsChanged.length} Fields Changed</td>
  `;

  tr.style.cursor = "pointer";

  tr.addEventListener("click", () => {
    localStorage.setItem("selectedRequest", JSON.stringify(req));
    window.location.href = "/Profile/RequestDetails";
  });

  tableBody.appendChild(tr);
});

function renderStatus(status) {
  const lower = status.toLowerCase();

  if (lower === "pending")
    return `<span class="badge bg-warning text-dark">Pending</span>`;

  if (lower === "approved")
    return `<span class="badge bg-success">Approved</span>`;

  return `<span class="badge bg-danger">Rejected</span>`;
}
