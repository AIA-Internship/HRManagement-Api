function redirectToDashboard(roleId) {
    console.log("roleId =", roleId);

    if (roleId === "1") {
        console.log("redirect supervisor");
        window.location.href = "/Leave/Supervisor/Dashboard";
    } else {
        console.log("redirect employee");
        window.location.href = "/Leave/Employee/Dashboard";
    }
}

function onLoad() {
    console.log("onLoad");

    const userInfoRaw = localStorage.getItem("aia_user_info");
    console.log(userInfoRaw);

    if (userInfoRaw) {
        try {
            const user = JSON.parse(userInfoRaw);
            console.log(user);

            if (user && user.role_id) {
                redirectToDashboard(user.role_id);
            } else if (user && user.role) {
                const roleId = user.role === "supervisor" ? "1" : "0";
                redirectToDashboard(roleId);
            } else {
                console.log("role tidak ada");
            }
        } catch (e) {
            console.error(e);
        }
    } else {
        console.log("localStorage kosong");
    }
}