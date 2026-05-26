

function redirectToDashboard(roleId) {
    if (roleId === "1") {
        window.location.href = "/Timesheet/Supervisor/Dashboard";
    } else {
        window.location.href = "/Leave/Employee/Dashboard";
    }
}

function onLoad(){
    const user = JSON.parse(localStorage.getItem("aia_user_info"));
    if (user && user.role_id) {
        redirectToDashboard(user.role_id);
    }
}


document.addEventListener("DOMContentLoaded", function () {
    onLoad()
})