const API_BASE = "https://localhost:7089";

let supervisorList = [];

document.addEventListener("DOMContentLoaded", async () => {
    const urlParams = new URLSearchParams(window.location.search);
    const displayId = urlParams.get("id");

    await fetchSupervisorList();
    bindSupervisorAutocomplete();

    if (displayId) {
        document.getElementById("hiddenDisplayId").value = displayId;
        await loadEmploymentData(displayId);
    }
});

async function fetchSupervisorList() {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    try {
        const res = await fetch(`${API_BASE}/api/employee/supervisors-lookup`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (res.ok) {
            const json = await res.json();
            supervisorList = json.content || json.data || json || [];
            if (!Array.isArray(supervisorList)) supervisorList = [];
        }
    } catch (err) {
        console.error("Failed to fetch supervisors:", err);
    }
}

function bindSupervisorAutocomplete() {
    const input = document.getElementById("editSupervisorName");
    const hiddenId = document.getElementById("editSupervisorId");
    const dropdown = document.getElementById("supervisorDropdown");

    if (!input || !dropdown) return;

    input.addEventListener("input", (e) => {
        const query = e.target.value.toLowerCase().trim();
        dropdown.innerHTML = "";

        if (!query) {
            hiddenId.value = "";
            dropdown.classList.remove("open");
            return;
        }

        const matches = supervisorList.filter(s =>
            (s.fullName && s.fullName.toLowerCase().includes(query)) ||
            (s.displayId && s.displayId.toLowerCase().includes(query))
        );

        if (matches.length === 0) {
            dropdown.innerHTML = `<div class="autocomplete-empty">No supervisors found</div>`;
        } else {
            matches.forEach(sup => {
                const item = document.createElement("div");
                item.className = "autocomplete-item";
                item.innerHTML = `<div class="fw-bold">${sup.fullName}</div><div class="fs-8 text-muted">${sup.displayId}</div>`;

                item.addEventListener("click", () => {
                    input.value = sup.fullName;
                    hiddenId.value = sup.displayId;
                    dropdown.classList.remove("open");
                });

                dropdown.appendChild(item);
            });
        }
        dropdown.classList.add("open");
    });

    document.addEventListener("click", (e) => {
        if (!input.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.classList.remove("open");

            if (input.value && !hiddenId.value) {
                const exactMatch = supervisorList.find(s => s.fullName && s.fullName.toLowerCase() === input.value.toLowerCase());
                if (exactMatch) {
                    input.value = exactMatch.fullName;
                    hiddenId.value = exactMatch.displayId;
                } else {
                    input.value = "";
                }
            }
        }
    });
}

async function loadEmploymentData(displayId) {
    const token = window.aiaAuth && window.aiaAuth.getToken();
    try {
        const response = await fetch(`${API_BASE}/api/employee/${displayId}`, { 
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.status === 401) { window.aiaAuth.signOut(); return; }
        if (response.status === 404) { alert("Employee not found."); return; }

        const json = await response.json();
        if (json.isError) return;

        const emp = json.content || json.data || json;
        
        // Employment Information
        if (document.getElementById("editEmployeeId")) document.getElementById("editEmployeeId").value = emp.employeeDisplayId || "";
        const employmentDropdown = document.getElementById("editEmploymentType");
        if (employmentDropdown && emp.employmentType !== undefined && emp.employmentType !== null) {
            const apiValue = String(emp.employmentType).toLowerCase().replace("-", "");
            let matchFound = false;
            Array.from(employmentDropdown.options).forEach(option => {
                const optionValue = option.value.toLowerCase();
                const optionText = option.text.toLowerCase().replace("-", "");
                if (apiValue === optionValue || apiValue === optionText) {
                    employmentDropdown.value = option.value;
                    matchFound = true;
                }
            });
            if (!matchFound) {
                console.warn("Dropdown Match Failed. The API sent:", emp.employmentType);
            }
        }
        if (document.getElementById("editDepartment")) document.getElementById("editDepartment").value = emp.department || "";
        if (document.getElementById("editPosition")) document.getElementById("editPosition").value = emp.position || "";
        if (document.getElementById("editSupervisorName")) document.getElementById("editSupervisorName").value = emp.supervisorName || "";
        if (document.getElementById("editSupervisorId")) document.getElementById("editSupervisorId").value = emp.displayId || "";

        // Personal Information
        if (document.getElementById("viewFullName")) document.getElementById("viewFullName").value = emp.fullName || emp.name || "";
        if (document.getElementById("viewEmail")) document.getElementById("viewEmail").value = emp.personalEmail || emp.email || "";
        if (document.getElementById("viewCompanyEmail")) document.getElementById("viewCompanyEmail").value = emp.employeeEmail || "";
        if (document.getElementById("viewGender")) document.getElementById("viewGender").value = emp.gender || "";
        if (document.getElementById("viewPhone")) document.getElementById("viewPhone").value = emp.phoneNumber || "";
        if (document.getElementById("viewNik")) document.getElementById("viewNik").value = emp.nik || "";
        if (document.getElementById("viewMarital")) document.getElementById("viewMarital").value = emp.maritalStatus || "";
        if (document.getElementById("viewCurrentStreet")) document.getElementById("viewCurrentStreet").value = emp.currentStreetAddress || "-";
        if (document.getElementById("viewCurrentCity")) document.getElementById("viewCurrentCity").value = emp.currentCity || "-";
        if (document.getElementById("viewCurrentProvince")) document.getElementById("viewCurrentProvince").value = emp.currentProvince || "-";
        if (document.getElementById("viewCurrentPostalCode")) document.getElementById("viewCurrentPostalCode").value = emp.currentPostalCode || "-";
        if (document.getElementById("viewResidentialStreet")) document.getElementById("viewResidentialStreet").value = emp.residentialStreetAddress || "-";
        if (document.getElementById("viewResidentialCity")) document.getElementById("viewResidentialCity").value = emp.residentialCity || "-";
        if (document.getElementById("viewResidentialProvince")) document.getElementById("viewResidentialProvince").value = emp.residentialProvince || "-";
        if (document.getElementById("viewResidentialPostalCode")) document.getElementById("viewResidentialPostalCode").value = emp.residentialPostalCode || "-";
        if (document.getElementById("viewBirthPlace")) document.getElementById("viewBirthPlace").value = emp.placeOfBirth || "-";
        if (document.getElementById("viewDateOfBirth")) document.getElementById("viewDateOfBirth").value = emp.dateOfBirth || "-";

        // Emergency Contacts
        if (document.getElementById("viewEmergencyName")) document.getElementById("viewEmergencyName").value = emp.emergencyContactName || "";
        if (document.getElementById("viewEmergencyRel")) document.getElementById("viewEmergencyRel").value = emp.relationship || "";
        if (document.getElementById("viewEmergencyPhone")) document.getElementById("viewEmergencyPhone").value = emp.emergencyContactPhone || "";

        // Profile Picture
        if (emp.profilePictureUrl && document.getElementById("profilePicPreview")) {
            document.getElementById("profilePicPreview").innerHTML = `<img src="${emp.profilePictureUrl}" />`;
        }

        if (emp.startDate && document.getElementById("editStartDate")) {
            const dateObj = new Date(emp.startDate);
            if (!isNaN(dateObj)) {
                document.getElementById("editStartDate").value = dateObj.toISOString().split('T')[0];
            }
        }
        
        if (emp.status || emp.employeeStatus) {
            let rawStatus = emp.status || emp.employeeStatus;
            let normalizedValue = (String(rawStatus).toLowerCase() === "active" || rawStatus === "1") ? "Active" : "Inactive";
            const statusRadio = document.querySelector(`input[name="employeeStatus"][value="${normalizedValue}"]`);
            if (statusRadio) statusRadio.checked = true;
        }

        if (emp.role) {
            let formattedRole = String(emp.role).charAt(0).toUpperCase() + String(emp.role).slice(1).toLowerCase();
            const roleRadio = document.querySelector(`input[name="employeeRole"][value="${formattedRole}"]`);
            if (roleRadio) roleRadio.checked = true;
        }

    } catch (error) {
        console.error("API error while loading data:", error);
    }
}

async function updateEmploymentDetails() {
    const displayId = document.getElementById("hiddenDisplayId").value;
    const rawEmploymentType = document.getElementById("editEmploymentType").value;
    const token = window.aiaAuth && window.aiaAuth.getToken();
    const payload = {
        status: document.querySelector('input[name="employeeStatus"]:checked')?.value,
        startDate: document.getElementById("editStartDate").value,
        employmentType: rawEmploymentType ? parseInt(rawEmploymentType, 10) : null,
        department: document.getElementById("editDepartment").value,
        position: document.getElementById("editPosition").value,
        role: document.querySelector('input[name="employeeRole"]:checked')?.value,
        supervisorId: document.getElementById("editSupervisorId").value,
        supervisorName: document.getElementById("editSupervisorName").value
    };

    try {
        const response = await fetch(`${API_BASE}/api/employee/employment-info/${displayId}`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            alert("Employment details updated successfully.");
            window.location.href = "/EmployeeList";
        } else {
            alert("Failed to update employment details.");
        }
    } catch (error) {
        console.error("API error:", error);
    }
}