document.addEventListener("DOMContentLoaded", () => {
    const total = sick + personal + emergency;

    function getPercent(value) {
        if (total === 0) return 0;
        return (value / total) * 100;
    }

    document.getElementById("sickBar").style.width = getPercent(sick) + "%";
    document.getElementById("personalBar").style.width = getPercent(personal) + "%";
    document.getElementById("emergencyBar").style.width = getPercent(emergency) + "%";
});

document.addEventListener("DOMContentLoaded", () => {
    const popover = document.getElementById("popover_monthly");
    if (popover) {
        popover.remove();
    }
});