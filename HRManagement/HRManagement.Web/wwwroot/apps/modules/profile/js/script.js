document.addEventListener("DOMContentLoaded", () => {

  const form = document.querySelector(".needs-validation");
  if (!form) return;

  const modal = document.getElementById("errorModal");
  const emailInput = form.querySelector('input[type="email"]');
  const passwordInput = form.querySelector('input[type="password"]');

  form.addEventListener("submit", e => {
    e.preventDefault();
    form.classList.add("was-validated");

    if (!form.checkValidity()) return;

    const isValidUser =
      emailInput.value.trim() === "admin@aia.com" &&
      passwordInput.value.trim() === "12345678";

    if (!isValidUser) {
      modal && (modal.style.display = "flex");
      return;
    }

    localStorage.setItem("isLoggedIn", "true");
    window.location.href = "/Profile";
  });

  // Toggle password
window.togglePassword = function () {
    const password = document.getElementById("password");
    const icon = document.getElementById("toggleIcon");

    if (password.type === "password") {
        password.type = "text";
        icon.classList.remove("bi-eye");
        icon.classList.add("bi-eye-slash");
    } else {
        password.type = "password";
        icon.classList.remove("bi-eye-slash");
        icon.classList.add("bi-eye");
    }
};

  // Close modal
  window.closeModal = () => {
    modal && (modal.style.display = "none");
  };

});

async function loadProfile() {

    // nanti ganti dengan endpoint asli kamu
    const response = await fetch(" ");

    const data = await response.json();

    document.getElementById("fullName").textContent = data.fullName;
    document.getElementById("gender").textContent = data.gender;
    document.getElementById("personalEmail").textContent = data.personalEmail;
    document.getElementById("companyEmail").textContent = data.companyEmail;
    document.getElementById("phone").textContent = data.phone;
    document.getElementById("nik").textContent = data.nik;
    document.getElementById("npwp").textContent = data.npwp;
    document.getElementById("placeOfBirth").textContent = data.placeOfBirth;
    document.getElementById("dateOfBirth").textContent = data.dateOfBirth;
    document.getElementById("maritalStatus").textContent = data.maritalStatus;
}

loadProfile();
