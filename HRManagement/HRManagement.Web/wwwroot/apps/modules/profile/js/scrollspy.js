document.addEventListener("DOMContentLoaded", function () {

    const tabs = document.getElementById("profileTabs");
    const navHeight = tabs.offsetHeight;

    new bootstrap.ScrollSpy(document.body, {
        target: '#profileTabs',
        offset: navHeight + 20
    });

});

document.querySelectorAll('#profileTabs a').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();

        const target = document.querySelector(this.getAttribute('href'));
        const offset = 110;

        const top = target.getBoundingClientRect().top + window.scrollY - offset;

        window.scrollTo({
            top: top,
            behavior: 'smooth'
        });
    });
});
