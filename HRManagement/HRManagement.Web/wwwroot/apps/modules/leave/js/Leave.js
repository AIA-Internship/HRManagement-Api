function redirectToDashboard(roleId) {
    if (roleId === "1") {
        window.location.href = "/Timesheet/Supervisor/Dashboard";
    } else {
        window.location.href = "/Leave/Employee/Dashboard";
    }
}

function onLoad(){
    const userInfoRaw = localStorage.getItem("aia_user_info");
    if (userInfoRaw) {
        try {
            const user = JSON.parse(userInfoRaw);
            if (user && user.role_id) {
                redirectToDashboard(user.role_id);
            } else if (user && user.role) {
                // Fallback to role if role_id is not available
                const roleId = user.role === "supervisor" ? "1" : "0";
                redirectToDashboard(roleId);
            }
        } catch (e) {
            console.error("Error parsing user info:", e);
        }
    }
}

// Try to redirect immediately
if (document.readyState === 'loading') {
    document.addEventListener("DOMContentLoaded", onLoad);
} else {
    // DOM is already loaded
    onLoad();
}