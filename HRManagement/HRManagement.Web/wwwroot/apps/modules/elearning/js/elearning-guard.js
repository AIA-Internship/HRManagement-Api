(function () {
    var SUPERVISOR_ROLE_ID = 1;
    var INTERN_ROLE_ID = 2;

    var path = window.location.pathname.toLowerCase();
    var isSupervisorPage = path.indexOf('/modules/elearning/supervisor') !== -1;
    var isInternPage = path.indexOf('/modules/elearning/intern') !== -1;

    if (!isSupervisorPage && !isInternPage) return;

    var user = window.aiaAuth && window.aiaAuth.getUserInfo();
    if (!user) {
        window.aiaAuth && window.aiaAuth.signOut();
        return;
    }

    var roleId = parseInt(user.RoleId || user.roleid || user['RoleId'] || 0, 10);

    if (isSupervisorPage && roleId !== SUPERVISOR_ROLE_ID) {
        window.location.replace('/Modules/ELearning/Intern/Dashboard');
        return;
    }

    if (isInternPage && roleId !== INTERN_ROLE_ID) {
        window.location.replace('/Modules/ELearning/Supervisor/Modules');
        return;
    }
})();
