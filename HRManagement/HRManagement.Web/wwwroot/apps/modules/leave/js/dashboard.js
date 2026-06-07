document.addEventListener("DOMContentLoaded", () => {
    const total = paid + unpaid;

    function getPercent(value) {
        if (total === 0) return 0;
        return (value / total) * 100;
    }

    document.getElementById("paidBar").style.width = getPercent(paid) + "%";
    document.getElementById("unpaidBar").style.width = getPercent(unpaid) + "%";
});

document.addEventListener("DOMContentLoaded", () => {
    const popover = document.getElementById("popover_monthly");
    if (popover) {
        popover.remove();
    }
});