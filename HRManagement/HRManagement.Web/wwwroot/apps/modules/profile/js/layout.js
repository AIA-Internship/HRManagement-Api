document.addEventListener("DOMContentLoaded", () => {
    loadSidebar();

    const content = document.querySelector(".main-content");
    if (content) {
        setTimeout(() => {
            content.classList.add("loaded");
        }, 50);
    }
});

function loadSidebar() {
    fetch("../components/sidebar.html")
        .then(res => res.text())
        .then(data => {
            document.getElementById("sidebar-container").innerHTML = data;

            setActiveMenu();
            loadUserInfo();
        });
}

function setActiveMenu() {
    const currentPage = window.location.pathname.split("/").pop();

    const links = document.querySelectorAll(".sidebar-link");

    links.forEach(link => {
        const href = link.getAttribute("href");

        if (href === currentPage) {
            link.classList.add("active");
        }
    });
}

function loadUserInfo() {
    const user = {
        name: "Pulkeria Mayriel Rheimitz Godeliva",
        id: "E123456"
    };

    const nameEl = document.getElementById("sidebarUserName");
    const idEl = document.getElementById("sidebarUserId");

    if (nameEl) nameEl.textContent = user.name;
    if (idEl) idEl.textContent = user.id;
}
