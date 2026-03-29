let userProfile = JSON.parse(localStorage.getItem("userProfile")) || {
    fullName: "Pulkeria Mayriel Rheimitz Godeliva",
    gender: "Female",
    personalEmail: "mayrielgodeliva@gmail.com",
    companyEmail: "mayriel.godeliva@aia.com",
    phoneNumber: "+62 819-3211-5107",
    nik: "3273012345678901",
    npwp: "3273012345678901",
    placeOfBirth: "Jakarta",
    dateOfBirth: "19/05/2005",
    maritalStatus: "Single",

    currentAddress: {
        street: "Jl. Sudirman No. 123, District 8",
        city: "Jakarta Selatan",
        province: "DKI Jakarta",
        postal: "12190"
    }
};


//
// VIEW PAGE RENDER
//
function renderProfile() {

    if (!document.getElementById("fullName")) return;

    document.getElementById("fullName").textContent = userProfile.fullName;
    document.getElementById("gender").textContent = userProfile.gender;
    document.getElementById("personalEmail").textContent = userProfile.personalEmail;
    document.getElementById("companyEmail").textContent = userProfile.companyEmail;
    document.getElementById("phoneNumber").textContent = userProfile.phoneNumber;
    document.getElementById("nik").textContent = userProfile.nik;
    document.getElementById("npwp").textContent = userProfile.npwp;
    document.getElementById("placeOfBirth").textContent = userProfile.placeOfBirth;
    document.getElementById("dateOfBirth").textContent = userProfile.dateOfBirth;
    document.getElementById("maritalStatus").textContent = userProfile.maritalStatus;

    if (userProfile.currentAddress) {
        document.getElementById("currentStreet").textContent = userProfile.currentAddress.street;
        document.getElementById("currentCity").textContent = userProfile.currentAddress.city;
        document.getElementById("currentProvince").textContent = userProfile.currentAddress.province;
        document.getElementById("currentPostal").textContent = userProfile.currentAddress.postal;
    }
}

//
// EDIT PAGE LOAD
//
function loadEditForm() {

    if (!document.getElementById("editFullName")) return;

    document.getElementById("editFullName").value = userProfile.fullName;
    document.getElementById("editPersonalEmail").value = userProfile.personalEmail;
    document.getElementById("editCompanyEmail").value = userProfile.companyEmail;
    document.getElementById("editPhoneNumber").value = userProfile.phoneNumber;
    document.getElementById("editNik").value = userProfile.nik;
    document.getElementById("editNpwp").value = userProfile.npwp;
    document.getElementById("editPlaceOfBirth").value = userProfile.placeOfBirth;
    document.getElementById("editDateOfBirth").value = userProfile.dateOfBirth;
    document.getElementById("editMaritalStatus").value = userProfile.maritalStatus;

    // RADIO BUTTON GENDER
    const genderRadios = document.querySelectorAll('input[name="gender"]');
    genderRadios.forEach(radio => {
        radio.checked = (radio.value === userProfile.gender);
    });

    if (userProfile.currentAddress) {
        document.getElementById("editCurrentStreet").value = userProfile.currentAddress.street;
        document.getElementById("editCurrentCity").value = userProfile.currentAddress.city;
        document.getElementById("editCurrentProvince").value = userProfile.currentAddress.province;
        document.getElementById("editCurrentPostal").value = userProfile.currentAddress.postal;
    }
}

document.addEventListener("DOMContentLoaded", function () {

    flatpickr("#editDateOfBirth", {
        dateFormat: "d/m/Y",
        allowInput: true
    });

});

//
// UPDATE PROFILE
//
function updateProfile() {

    let isValid = true;

    // ambil semua field required
    const requiredFields = document.querySelectorAll("[required]");

    requiredFields.forEach(field => {
        if (!field.value || field.value.trim() === "") {
            field.classList.add("is-invalid");
            isValid = false;
        } else {
            field.classList.remove("is-invalid");
        }
    });

    // cek radio gender manual (karena radio beda)
    const selectedGender = document.querySelector('input[name="gender"]:checked');
    if (!selectedGender) {
        isValid = false;
        alert("Please select gender.");
    }

    if (!isValid) {
        alert("Please complete all required fields.");
        return;
    }

    // ===== KODE LAMA KAMU (TETAP DIPAKAI) =====

    userProfile.fullName = document.getElementById("editFullName").value;

    userProfile.gender = selectedGender.value;

    userProfile.personalEmail = document.getElementById("editPersonalEmail").value;
    userProfile.companyEmail = document.getElementById("editCompanyEmail").value;
    userProfile.phoneNumber = document.getElementById("editPhoneNumber").value;
    userProfile.nik = document.getElementById("editNik").value;
    userProfile.npwp = document.getElementById("editNpwp").value;
    userProfile.placeOfBirth = document.getElementById("editPlaceOfBirth").value;
    userProfile.dateOfBirth = document.getElementById("editDateOfBirth").value;
    userProfile.maritalStatus = document.getElementById("editMaritalStatus").value;

    userProfile.currentAddress = {
        street: document.getElementById("editCurrentStreet").value,
        city: document.getElementById("editCurrentCity").value,
        province: document.getElementById("editCurrentProvince").value,
        postal: document.getElementById("editCurrentPostal").value
    };

    localStorage.setItem("userProfile", JSON.stringify(userProfile));

    window.location.href = "/Profile";
}

//
// AUTO INIT
//
document.addEventListener("DOMContentLoaded", function () {
    renderProfile();
    loadEditForm();
});
