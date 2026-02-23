var app = {};
var session = {}

const monthNames = ["Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "November", "Desember"];
const monthNamesAbbr = ["Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des"];

// if (window.location.pathname !== '/account/signin') {
//     var token = localStorage.getItem('jwt_token');
//     var is_valid_login = localStorage.getItem('is_valid_login');
//     if (!token && !is_valid_login) {
//         window.location.href = '/account/signin';
//     } 
// }

// 2. Setup Global AJAX
$.ajaxSetup({
    // Sebelum kirim request, tempel token
    beforeSend: function(xhr) {
        var token = localStorage.getItem('jwt_token');
        if (token) {
            xhr.setRequestHeader('Authorization', 'Bearer ' + token);
        }
    },
    // Global Error Handler
    error: function(xhr, status, error) {
        // Jika Backend C# membalas 401 Unauthorized
        if (xhr.status === 401) {
            console.warn('Token Expired atau Invalid via Interceptor');
            
            // Hapus token yang sudah basi
            localStorage.removeItem('jwt_token');
            window.location.href = '/account/signin';

            // Opsional: Tampilkan SweetAlert sebelum redirect
            Swal.fire({
                text: "Sesi Anda telah berakhir. Silakan login kembali.",
                icon: "warning",
                confirmButtonText: "Login",
                allowOutsideClick: false
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = '/account/signin';
                }
            });
        }
    }
});

window.handleImageError = function(img) {
    // Cek supaya tidak loop infinite
    if (!img.getAttribute('data-error-handled')) {
        img.setAttribute('data-error-handled', 'true');

        // Ganti source ke gambar default
        img.src = '/apps/media/avatar/blank.png'; 

        // Opsional: Tambah class biar visualnya beda
        //img.classList.add('opacity-50');
    }
};

$.fn.thousandSeparatorAdvanced = function() {
    return this.on('input', function() {
        let input = $(this);
        let value = input.val();
        let cursorStart = this.selectionStart;

        // 1. Hitung jumlah digit (angka) sebelum posisi kursor
        let digitsBeforeCursor = (value.substring(0, cursorStart).match(/\d/g) || []).length;

        // 2. Bersihkan nilai: hapus semua selain angka
        let numericValue = value.replace(/[^0-9]/g, '');

        // 3. Format nilai
        let formattedValue;
        if (numericValue === '') {
            formattedValue = '';
        } else {
            // Gunakan 'id-ID' untuk pemisah titik (misal: 1.000.000)
            // Ganti ke 'en-US' jika ingin pemisah koma (misal: 1,000,000)
            formattedValue = new Intl.NumberFormat('id-ID').format(numericValue);
        }

        // 4. Set nilai baru ke input
        input.val(formattedValue);

        // 5. Hitung dan set posisi kursor baru
        let newCursorPos = 0;
        let digitCount = 0;

        if (formattedValue === '') {
            newCursorPos = 0;
        } else {
            // Loop string yang baru diformat
            for (let i = 0; i < formattedValue.length; i++) {
                // Tambah hitungan jika karakter adalah digit
                if (/\d/.test(formattedValue[i])) {
                    digitCount++;
                }
                
                // Jika jumlah digit yang dihitung sama dengan jumlah digit 
                // sebelum kursor di nilai lama, kita temukan posisi kursor baru
                if (digitCount === digitsBeforeCursor) {
                    newCursorPos = i + 1; // Posisikan kursor *setelah* digit tersebut
                    break;
                }
            }
        }
        
        // Jika kursor tadinya ada di awal (0 digit), posisinya tetap di 0
        // Jika kursor ada di akhir, loop akan selesai dan kita perlu set manual
        if (digitsBeforeCursor > 0 && newCursorPos === 0) {
            newCursorPos = formattedValue.length;
        }

        // Set posisi kursor
        this.setSelectionRange(newCursorPos, newCursorPos);
    });
};

(function ($, app) {
    // Global variables.

    const d = new Date();
    var inProgressGettingData;

    app.baseUrl = '';

    app.profile = () => {
        if (window.location.pathname !== '/account/signin' && window.location.pathname !== '/account/verification') {
            $.ajax({
                url: app.baseUrl + '/api/user/me',
                type: 'GET',
                contentType: "application/json",
                success: function (data) {
                    console.log('profile : ' + JSON.stringify(data.content));

                    $('.gmia-data-username').text(data.content.userName);
                    $('.gmia-data-rolename').text(data.content.roleName);
                    $('.gmia-data-mobile').text(data.content.mobilePhone);
                    app.avatar('.gmia-data-photo', data.content.photoUrl);

                    return data.content;
                }
            });
        }
        return null;
    };
    

    app.menu = {};
    app.menu.active = (menu, name) => {
        $(`.gmia-menu-${menu}`).addClass('here show');
        $(`.gmia-sub-menu-${menu}`).addClass('menu-active-bg');
        $(`.gmia-menu-link-${name}-active`).addClass('active');
    };

    app.cookie = {
        create: function (name, value, days) {
            var expires = "";

            if (days) {
                var date = new Date();
                date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                expires = "; expires=" + date.toGMTString();
            }

            document.cookie = name + "=" + value + expires + "; path=/";
        },

        read: function (name) {
            var nameEQ = name + "=";
            var ca = document.cookie.split(";");

            for (var i = 0; i < ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) == " ") c = c.substring(1, c.length);
                if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
            }

            return null;
        },
        erase: function (name) {
            app.cookie.create(name, "", -1);
        }
    }



    app.avatar = (element, url) => {
        var $img = $(element);
        $img.on('error', function() {
            $(this).attr('src', '/apps/media/avatar/blank.png').show();
        });

        $img.attr('src', url);
    };

    app.photo = (url) => {
        const safeUrl = url && url.trim() !== ""
            ? url
            : "/apps/media/avatar/blank.png";

        return safeUrl;
    };
    
    app.chartbgcolor = (idx) => {
        var obj = [
            'primary',
            'success',
            'info',
            'warning',
            'danger',
            'secondary',
            'primary'
        ]
        return obj[idx];
    };

    app.bgcolor = () => {
        var obj = [
            'primary',
            'success',
            'info',
            'warning',
            'danger',
            'dark'
        ]
        
        const randomIndex = Math.floor(Math.random() * obj.length);
        return obj[randomIndex];
    };

    app.prioritycolor = (id) => {
        switch(id) {
            case 1:
                return 'danger';
            case 2:
                return 'warning';
            case 3:
                return 'primary';
            case 4:
                return 'success';
            case 5:
                return 'success';
        }
    };

    app.statuscolor = (id) => {
        switch(id) {
            case 1:
                return 'warning';
            case 2:
                return 'info';
            case 3:
                return 'primary';
            case 4:
                return 'success';
            case 5:
                return 'danger';
        }
    };

    app.paidcolor = (id) => {
        switch(id) {
            case 1:
                return 'info';
            case 2:
                return 'primary';
            case 3:
                return 'success';
        }
    };

    app.messages = {};
    app.messages = {
        success: "Permintaan perubahan data sudah berhasil. Silahkan menunggu proses selanjutnya.",
        update: "Permintaan perubahan data sudah diperbaharui. Silahkan menunggu proses selanjutnya.",
        acknowledged: "Permintaan karyawan ini sudah berhasil anda acknowledged.",
        rejected: "Permintaan karyawan ini sudah berhasil anda tolak.",
        cancelled: "Permintaan anda sudah berhasil dibatalkan.",
        approved: "Permintaan karyawan sudah berhasil Anda setujui dan otomatis data karyawan sudah diperbaharui.",
    };

    app.select2 = {};
    app.select2 = {
        employees: function (elementId, options) {
            $(elementId).select2({
                placeholder: options.placeholder,
                width: '100%',
                allowClear: true,
                ajax: {
                    url: app.baseUrl + '/api/v1/employees',
                    dataType: 'json',
                    delay: 250,
                    //headers: {
                    //    "Authorization": "Bearer " + app.baseUrl.applicationUser.accessToken
                    //},
                    data: function (params) {
                        return {
                            filterText: params.term,
                            page: params.page
                        };
                    },
                    processResults: function (data, params) {
                        params.page = params.page || 1;
                        return {
                            results: $.map(data, function (e) { return { id: e.employeeId, text: e.employeeName, photo: e.photo } }),
                            pagination: {
                                more: (params.page * 30) < data.totalCount
                            }
                        };
                    },
                    cache: true
                },
                escapeMarkup: function (markup) { return markup; },
                minimumInputLength: 2,
                templateResult: formatRepo,
                templateSelection: formatRepoSelection

            }).on('change', function (e) {
                if (typeof (options.change) !== 'undefined') {
                    options.change();
                }
            });

            function formatRepoSelection(repo) {
                //window[options.result] = repo;
                return repo.name || repo.text;
            }

            function formatRepo(repo) {
                if (repo.loading) { return repo.text; }

                var html =
                    '<div class="alert fade show g-brd-none rounded-0 g-mb-0 g-pa-0 g-py-5"> ' +
                    '   <div class="media"> ' +
                    '       <div class="d-flex g-mr-10"> ' +
                    '           <img class="g-width-40 g-height-40 g-rounded-50x" src="' + repo.photo + '" onerror="app.utility.onImageError(this);"> ' +
                    '       </div> ' +
                    '       <div class="media-body"> ' +
                    '           <div class="d-flex justify-content-between"> ' +
                    '               <p class="m-0"><strong>' + repo.text + '</strong></p> ' +
                    '           </div> ' +
                    '           <p class="m-0 g-font-size-14">' + repo.id + '</p> ' +
                    '       </div> ' +
                    '   </div> ' +
                    '</div> ';
                return html;
            };
        },
        locations: function (elementId, options) {
            $(elementId).select2({
                placeholder: options.placeholder,
                width: '100%',
                allowClear: true,
                ajax: {
                    url: app.baseUrl + '/api/v1/locations',
                    dataType: 'json',
                    delay: 250,
                    headers: {
                        "Authorization": "Bearer " + app.configuration.applicationUser.accessToken
                    },
                    data: function (params) {
                        return {
                            filterText: params.term,
                            page: params.page
                        };
                    },
                    processResults: function (data, params) {
                        params.page = params.page || 1;
                        return {
                            results: $.map(data, function (e) { return { id: e.locationCode, text: e.locationName } }),
                            pagination: {
                                more: (params.page * 30) < data.totalCount
                            }
                        };
                    },
                    cache: true
                },
                escapeMarkup: function (markup) { return markup; },
                minimumInputLength: 2,
                templateResult: formatRepo,
                templateSelection: formatRepoSelection

            }).on('change', function (e) {
                if (typeof (options.change) !== 'undefined') {
                    options.change();
                }
            });

            function formatRepoSelection(repo) {
                //window[options.result] = repo;
                return repo.locationName || repo.text;
            }

            function formatRepo(repo) {
                if (repo.loading) { return repo.locationName; }
                return repo.locationName || repo.text;
            };
        }
    }

    // check object
    app.checkObj = {};

    app.checkObj.isEmptyNullOrUndefined = function (param) {
        if (param === undefined) { return true };
        if (param === null) { return true };
        if (this.isString(param)) { return param.replace(/\s+/g, '') === '' };
        return false;
    };

    app.checkObj.isArray = function (param) {
        return Object.prototype.toString.call(param) === '[object Array]';
    };

    app.checkObj.isFunction = function (param) {
        return Object.prototype.toString.call(param) === '[object Function]';
    };

    app.checkObj.isString = function (param) {
        return Object.prototype.toString.call(param) === '[object String]';
    };

    app.loading = {};
    app.loading.show = function (title) {
        Swal.fire({
            title: title || 'Loading Data...',
            allowOutsideClick: false,
            didOpen: () => Swal.showLoading()
        });
    };

    app.loading.hide = function () {
        Swal.close();
    };

    // utility
    app.utility = {};

    app.utility.ajaxCall = function (url, type, param, options, isLogin) {
        jQuery.support.cors = true;

        var dType = "json";
        var dParam = param;
        switch (type) {
            case "GET":
                dType = 'json';
                dParam = param;
                break;
            case "POST":
                dType = 'json';
                dParam = JSON.stringify(param);
                break;
            case "PATCH":
                dType = 'text';
                dParam = JSON.stringify(param);
                break;
            case "DELETE":
                dType = 'text';
                dParam = param;
                break;
            default:
                dType = "json";
                dParam = param;
                break;
        }

        $.ajax(
            {
                url: app.baseUrl + url,
                type: type,
                data: dParam,
                dataType: dType,
                contentType: "application/json",
                crossDomain: true,
                xhrFields: {
                    withCredentials: false
                },
                cache: true,
                async: true,
                beforeSend: function (xhr) {
                    //console.log('beforeSend : ' + xhr);
                    //if (url.indexOf('login') === -1) {
                    //    //xhr.setRequestHeader("Authorization", "Bearer " + app.authorization.token);
                    //}

                    if (isLogin !== true) {
                        xhr.setRequestHeader("Authorization", "Bearer " + localStorage.getItem('access_token'));
                    }

                    if ($.isFunction(options.beforeSend)) {
                        return options.beforeSend(xhr);
                    }
                }
            })
            .done(function (data, textStatus, jqXHR) {
                //console.log('done : ' + JSON.stringify(data));
                //console.log('done : ' + JSON.stringify(textStatus));
                //console.log('done : ' + JSON.stringify(jqXHR));
                //$('#modalProcess').on('show.bs.modal', function () {
                //    $('#modalProcess').modal('hide');
                //});

                if ($.isFunction(options.done)) {
                    //setTimeout(function () {
                    //    $('#modalProcess').modal('hide');
                    //}, 1000);

                    return options.done(data, textStatus, jqXHR);

                    //if (typeof (data.messageCode) === 'undefined') {

                    //}
                    //else {
                    //    if (data.messageCode !== 1) {
                    //        app.MessageBox.alert(data.messageName);
                    //    }
                    //    else {
                    //        return options.done(data, textStatus, jqXHR);
                    //    }
                    //}
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                //console.log('fail : ' + jqXHR);
                //console.log('fail : ' + textStatus);
                //console.log('fail : ' + errorThrown);
                //setTimeout(function () {
                //    $('#modalProcess').modal('hide');
                //}, 500);

                if (errorThrown === "Unauthorized") {
                    localStorage.removeItem('user_name');
                    localStorage.removeItem('access_token');
                    app.utility.logoff();
                }
                else {
                    app.MessageBox.invalid(jqXHR.responseText);
                    if ($.isFunction(options.fail)) {
                        return options.fail(jqXHR, textStatus, errorThrown);
                    }
                }
                //console.log(jqXHR);
                //console.log(textStatus);
                //console.log(errorThrown);

            });

    };

    app.utility.webAjaxWithToken = function (url, type, param, options) {
        // 1. Setup URL dan Method
        const fullUrl = app.baseUrl + url;
        const method = type.toLowerCase(); // Axios lebih umum menggunakan lowercase (get, post)

        // 2. Setup Headers (Content-Type & Authorization)
        const headers = {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + localStorage.getItem('jwt_token')
        };

        // 3. Konfigurasi Config Axios
        const axiosConfig = {
            url: fullUrl,
            method: method,
            headers: headers,
            // withCredentials: false // Default axios memang false, aktifkan jika butuh cookie cross-domain
        };

        // 4. Handle Data (Body) vs Params (Query String)
        // jQuery: data dikirim sebagai body. Axios: membedakan 'data' (body) dan 'params' (url query)
        if (param !== null) {
            if (method === 'get') {
                axiosConfig.params = param;
            } else {
                // Axios otomatis melakukan JSON.stringify jika content-type application/json
                axiosConfig.data = param; 
            }
        }

        // 5. Handle 'beforeSend'
        // Di Axios tidak ada callback beforeSend di dalam config, kita jalankan manual sebelum request.
        if (options && typeof options.beforeSend === 'function') {
            // Kita passing null karena axios belum menghasilkan xhr object di tahap ini
            options.beforeSend(null); 
        }

        // 6. Eksekusi Request
        axios(axiosConfig)
            .then(function (response) {
                // --- SUKSES (.done) ---

                if (options && typeof options.done === 'function') {
                    // Mapping parameter agar mirip jQuery .done(data, textStatus, jqXHR)
                    // response.data       -> data
                    // response.statusText -> textStatus
                    // response            -> jqXHR (object response lengkap)
                    return options.done(response.data, response.statusText, response);
                }
            })
            .catch(function (error) {
                // --- ERROR (.fail) ---

                // Deteksi Unauthorized (401)
                // Di jQuery Anda cek string "Unauthorized", di Axios kita cek status code
                const isUnauthorized = error.response && error.response.status === 401;

                if (isUnauthorized) {
                    app.utility.logoff();
                } else {
                    // Ambil pesan error dari response server
                    let responseText = "";
                    if (error.response && error.response.data) {
                        // Jika data berupa object, ubah jadi string agar alert bisa membacanya
                        responseText = typeof error.response.data === 'object' 
                            ? JSON.stringify(error.response.data) 
                            : error.response.data;
                    } else {
                        responseText = error.message; // Error jaringan/client side
                    }

                    app.MessageBox.invalid(responseText);

                    if (options && typeof options.fail === 'function') {
                        // Mapping parameter agar mirip jQuery .fail(jqXHR, textStatus, errorThrown)
                        // error.response -> jqXHR
                        // "error"        -> textStatus
                        // error.message  -> errorThrown
                        return options.fail(error.response, "error", error.message);
                    }
                }
            });
    };

    app.utility.webAjax = function (url, type, param, options) {
        // 1. Setup URL dan Method
        const fullUrl = app.baseUrl + url;
        const method = type.toLowerCase(); // Axios lebih umum menggunakan lowercase (get, post)

        // 2. Setup Headers (Content-Type & Authorization)
        const headers = {
            'Content-Type': 'application/json'
        };

        // 3. Konfigurasi Config Axios
        const axiosConfig = {
            url: fullUrl,
            method: method,
            headers: headers,
            // withCredentials: false // Default axios memang false, aktifkan jika butuh cookie cross-domain
        };

        // 4. Handle Data (Body) vs Params (Query String)
        // jQuery: data dikirim sebagai body. Axios: membedakan 'data' (body) dan 'params' (url query)
        if (param !== null) {
            if (method === 'get') {
                axiosConfig.params = param;
            } else {
                // Axios otomatis melakukan JSON.stringify jika content-type application/json
                axiosConfig.data = param; 
            }
        }

        // 5. Handle 'beforeSend'
        // Di Axios tidak ada callback beforeSend di dalam config, kita jalankan manual sebelum request.
        if (options && typeof options.beforeSend === 'function') {
            // Kita passing null karena axios belum menghasilkan xhr object di tahap ini
            options.beforeSend(null); 
        }

        // 6. Eksekusi Request
        axios(axiosConfig)
            .then(function (response) {
                // --- SUKSES (.done) ---

                if (options && typeof options.done === 'function') {
                    // Mapping parameter agar mirip jQuery .done(data, textStatus, jqXHR)
                    // response.data       -> data
                    // response.statusText -> textStatus
                    // response            -> jqXHR (object response lengkap)
                    return options.done(response.data, response.statusText, response);
                }
            })
            .catch(function (error) {
                // --- ERROR (.fail) ---

                // Deteksi Unauthorized (401)
                // Di jQuery Anda cek string "Unauthorized", di Axios kita cek status code
                const isUnauthorized = error.response && error.response.status === 401;

                if (isUnauthorized) {
                    app.utility.logoff();
                } else {
                    // Ambil pesan error dari response server
                    let responseText = "";
                    if (error.response && error.response.data) {
                        // Jika data berupa object, ubah jadi string agar alert bisa membacanya
                        responseText = typeof error.response.data === 'object' 
                            ? JSON.stringify(error.response.data) 
                            : error.response.data;
                    } else {
                        responseText = error.message; // Error jaringan/client side
                    }

                    app.MessageBox.invalid(responseText);

                    if (options && typeof options.fail === 'function') {
                        // Mapping parameter agar mirip jQuery .fail(jqXHR, textStatus, errorThrown)
                        // error.response -> jqXHR
                        // "error"        -> textStatus
                        // error.message  -> errorThrown
                        return options.fail(error.response, "error", error.message);
                    }
                }
            });
    };

    app.utility.loadProfile = function (id, photo, name, mobile) {
        let imgHtml = '';
        let color = app.bgcolor();

        if(photo) {
            imgHtml = `
                <a class="d-block overlay w-40px h-40px" data-type="image" data-fslightbox="member-gallery" href="${photo}">
                    <div class="overlay-wrapper bgi-no-repeat bgi-position-center bgi-size-cover card-rounded h-40px"
                        style="background-image:url('${photo}')">
                    </div>
                    
                    <div class="overlay-layer card-rounded bg-dark bg-opacity-25 shadow">
                        <i class="bi bi-eye-fill text-white fs-3"></i>
                    </div>
                </a>
                `;
        } else {
            imgHtml = `<span class="symbol-label bg-light-${color} text-${color} fs-6 fw-bold">${app.utility.getInitial(name)}</span>`;
        }

        // Template Item (Sesuai style HTML Anda: text-gray-900, text-hover-primary)
        var html = `
            <div class="d-flex align-items-center">
                <div class="symbol symbol-40px me-3">
                    ${imgHtml}
                </div>
                <div class="d-flex flex-column justify-content-start">
                    <a href="/member/${id}/detail" target="_blank" class="text-gray-800 fw-bold text-hover-primary fs-6">${name || '-'}</a>
                    <span class="fs-7 fw-semibold text-gray-700">${app.utility.mobile(mobile) || '-'}</span>
                </div>
            </div>
        `;

        return html;
    };

    app.utility.formatAvatar = function (id, photo, name, age) {
        let imgHtml = '';
        let color = app.bgcolor();

        if(photo) {
            imgHtml = `
                <a class="d-block overlay w-40px h-40px" data-fslightbox="member-gallery" href="${photo}">
                    <div class="overlay-wrapper bgi-no-repeat bgi-position-center bgi-size-cover card-rounded h-40px"
                        style="background-image:url('${photo}')">
                    </div>
                    
                    <div class="overlay-layer card-rounded bg-dark bg-opacity-25 shadow">
                        <i class="bi bi-eye-fill text-white fs-3"></i>
                    </div>
                </a>
                `;
        } else {
            imgHtml = `<span class="symbol-label bg-light-${color} text-${color} fs-6 fw-bold">${app.utility.getInitial(name)}</span>`;
        }

        // Template Item (Sesuai style HTML Anda: text-gray-900, text-hover-primary)
        var html = `
            <div class="d-flex align-items-center mb-7">
                <div class="symbol symbol-40px me-3">
                    ${imgHtml}
                </div>
                <div class="d-flex flex-column justify-content-start">
                    <a href="/member/${id}/detail" target="_blank" class="text-gray-800 fw-bold text-hover-primary fs-6">${name}</a>
                    <span class="fs-7 fw-semibold text-gray-700">${age} Tahun</span>
                </div>
            </div>
        `;

        return html;
    };

    app.utility.formatAvatarSmall = function (id, photo, name, age) {
        let imgHtml = '';
        let color = app.bgcolor();

        if(photo) {
            imgHtml = `<img alt="Pic" src="${app.photo(photo)}" style="object-fit: cover;" onerror="handleImageError(this)" />`;
        } else {
            imgHtml = `<span class="symbol-label bg-${color} text-inverse-${color} fw-bold">${app.utility.getInitial(name)}</span>`;
        }

        // Template Item (Sesuai style HTML Anda: text-gray-900, text-hover-primary)
        var html = `
            <a href="/member/${id}/detail" target="_blank" class="symbol symbol-35px symbol-circle" data-bs-toggle="tooltip" title="${name}">
                ${imgHtml}
            </a>
        `;

        return html;
    };

    app.utility.formatAvatarPhoto = function (photo, name) {
        let imgHtml = '';
        let color = app.bgcolor();

        if(photo) {
            imgHtml = `
                <a class="d-block overlay w-40px h-40px" data-fslightbox="member-gallery" href="${photo}">
                    <div class="overlay-wrapper bgi-no-repeat bgi-position-center bgi-size-cover card-rounded h-40px"
                        style="background-image:url('${photo}')">
                    </div>
                    
                    <div class="overlay-layer card-rounded bg-dark bg-opacity-25 shadow">
                        <i class="bi bi-eye-fill text-white fs-3"></i>
                    </div>
                </a>
                `;
        } else {
            imgHtml = `<span class="symbol-label bg-light-${color} text-${color} fs-6 fw-bold">${app.utility.getInitial(name)}</span>`;
        }

        // Template Item (Sesuai style HTML Anda: text-gray-900, text-hover-primary)
        var html = `
            <div class="symbol symbol-40px me-3">
                ${imgHtml}
            </div>
        `;

        return html;
    };

    app.utility.formatAvatarFamily = function (id, photo, name, mobile, age) {
        let imgHtml = '';
        let color = app.bgcolor();

        if(photo) {
            imgHtml = `
                <a class="d-block overlay w-40px h-40px" data-type="image" data-fslightbox="member-gallery" href="${photo}">
                    <div class="overlay-wrapper bgi-no-repeat bgi-position-center bgi-size-cover card-rounded h-40px"
                        style="background-image:url('${photo}')">
                    </div>
                    
                    <div class="overlay-layer card-rounded bg-dark bg-opacity-25 shadow">
                        <i class="bi bi-eye-fill text-white fs-3"></i>
                    </div>
                </a>
                `;
        } else {
            imgHtml = `<span class="symbol-label bg-light-${color} text-${color} fs-6 fw-bold">${app.utility.getInitial(name)}</span>`;
        }

        // Template Item (Sesuai style HTML Anda: text-gray-900, text-hover-primary)
        var html = `
            <div class="d-flex flex-stack">
                <div class="symbol symbol-40px me-5">
                    ${imgHtml}
                </div>
                <div class="d-flex align-items-center flex-row-fluid flex-wrap">
                    <div class="flex-grow-1 me-2">
                        <a href="/member/${id}/detail" target="_blank" class="text-gray-800 text-hover-primary fs-6 fw-bold d-block">${name}</a>
                        <span class="fs-7 fw-semibold text-gray-700">${app.utility.mobile(mobile) || '-'}</span>
                    </div>
                    <span class="badge badge-sm badge-success fs-8 fw-bold">${age} Tahun</span>
                </div>
            </div>
        `;

        return html;
    };

    app.utility.mobile = function (num) {
        if (num === null) {
            return '-';
        }
        return num.match(/.{1,4}/g)?.join(' ');
    };

    app.utility.currencyFormat = function (num, dec) {
        if (dec === null) {
            return Number.parseFloat(num).toFixed(2).replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1.');
        }
        else {
            return Number.parseFloat(num).toFixed(dec).replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1.');
        }
    };

    app.utility.getAmount = function (value) {
        if (value === null) {
            return 0;
        }
        else {
            return value.replace(/[^0-9]/g, '');
        }
    };

    app.utility.isNullOrEmpty = function (data) {
        if (data === '' || data === null) {
            return '-'
        }
        return data;
    };
    app.utility.currencyUKFormat = function (num) {
        return Number.parseFloat(num).toFixed(2).replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1.');
    };

    // Generic tools. 
    app.utility.formatDateMinHour = function (date) {
        if (date === null || moment(date).format('DD MMM YYYY') === '01 Jan 1900' || moment(date).format('DD MMM YYYY') === '01 Jan 0001') {
            return '';
        }
        var dates = new Date(date)
        var offset = dates.getTimezoneOffset(), o = Math.abs(offset);
        var no = (offset > 0 ? "+" : "-") + ("00" + Math.floor(o / 60)).slice(-2);
        //console.log(no);

        return moment(date).add(no, 'hours').format('DD MMM YYYY HH:mm:ss');
    };

    app.utility.formatDate1 = function (date) {
        if (moment(date).format('DD MMM YYYY') === '01 Jan 1900' || moment(date).format('DD MMM YYYY') === '01 Jan 0001') {
            return '';
        }
        return moment(date).format('DD-MMM');
    };

    app.utility.formatDate = function (date) {
        if (date === null || moment(date).format('DD MMM YYYY') === '01 Jan 1900' || moment(date).format('DD MMM YYYY') === '01 Jan 0001') {
            return '-';
        }
        return moment(date).format('DD MMM YYYY');
    };

    app.utility.formatMtd = function (date) {
        if (moment(date).format('DD MMM YYYY') === '01 Jan 1900' || moment(date).format('DD MMM YYYY') === '01 Jan 0001') {
            return '';
        }
        return moment(date).format('MMM YYYY');
    };

    app.utility.formatTime = function (date) {
        if (date=== null || moment(date).format('DD MMM YYYY') === '01 Jan 1900' || moment(date).format('DD MMM YYYY') === '01 Jan 0001') {
            return '';
        }
        return moment(date).format('HH:mm:ss');
    };

    app.utility.formatDateTime = function (date) {
        if (date=== null || moment(date).format('DD MMM YYYY') === '01 Jan 1900' || moment(date).format('DD MMM YYYY') === '01 Jan 0001') {
            return '';
        }
        return moment(date).format('DD MMM YYYY HH:mm:ss');
    };

    app.utility.formatDateString = function (date) {
        if (date === '' || moment(date).format('DD MMM YYYY') === '01 Jan 1900' || moment(date).format('DD MMM YYYY') === '01 Jan 0001') {
            return '1900-01-01';
        }
        return moment(date).format('YYYY-MM-DD');
    };

    app.utility.formatDateString2 = function (date) {
        if (date === '' || moment(date).format('DD MMM YYYY') === '01 Jan 1900' || moment(date).format('DD MMM YYYY') === '01 Jan 0001') {
            return '1900-01-01';
        }
        return moment(date).format('DD-MMM-YYYY');
    };

    app.utility.flatpicker = function (id) {
        flatpickr(id, {
            dateFormat: "d-M-Y",
            allowInput: false,
            defaultDate: "today"
        });
    };

    app.utility.flatpicker.setDate = function (id, value) {
        if (value === null || moment(value).format('DD MMM YYYY') === '01 Jan 1900' || moment(value).format('DD MMM YYYY') === '01 Jan 0001') { 
            flatpickr(id, 
            {
                dateFormat: "d-M-Y",
                allowInput: false
            }).clear();
        }
        else {
            flatpickr(id, 
            {
                dateFormat: "d-M-Y",
                allowInput: false
            }).setDate(new Date(value));
        }
    };

    app.utility.formatUTCDate = function (date) {
        if (moment.utc(date).utcOffset(moment().utcOffset()).format('DD MMM YYYY') == '01 Jan 1900' || moment.utc(date).utcOffset(moment().utcOffset()).format('DD MMM YYYY') == '01 Jan 0001') {
            return '';
        }
        return moment.utc(date).utcOffset(moment().utcOffset()).format('DD MMMM YYYY HH:mm');
    };


    app.utility.datepicker = function (elementId) {
        $(elementId).datepicker({
            format: 'dd M yyyy',
            todayHighlight: true
        });
    };

    app.utility.ckeditor = function (elementId) {
        var editorElement = CKEDITOR.document.getById(elementId);

        if (isWysiwygareaAvailable) {
            CKEDITOR.replace(elementId);
        } else {
            editorElement.setAttribute('contenteditable', 'true');
            CKEDITOR.inline(elementId);
        }

        function isWysiwygareaAvailable() {
            if (CKEDITOR.revision == ('%RE' + 'V%')) {
                return true;
            }

            return !!CKEDITOR.plugins.get('wysiwygarea');
        }
    };

    app.utility.ckeditor.get = function (elementId) {
        if ($('#' + elementId + '').length > 0) {
            if (CKEDITOR.instances["" + elementId].getData() == null) {
                return null;
            } else {
                return CKEDITOR.instances["" + elementId].getData();
            }
        }
        else {
            return null;
        }
    };

    app.utility.ckeditor.set = function (elementId, value) {
        return CKEDITOR.instances["" + elementId].setData(value);
    };

    app.utility.datepicker.get = function (elementId) {
        if ($(elementId).length > 0) {
            if ($(elementId).datepicker('getDate') == null) {
                return null; //moment(new Date).format('LLLL');
            } else {
                return moment($(elementId).datepicker('getDate')).format('YYYY-MM-DD');
            }
        }
        else {
            return null;
        }
    };

    app.utility.datepicker.set = function (elementId, value) {
        return $(elementId).datepicker('setDate', value);
    };

    app.utility.datepicker.disable = function (elementId) {
        $(elementId).prop('disabled', true);
    };

    app.utility.datepicker.enable = function (elementId) {
        $(elementId).prop('disabled', false);
    };

    app.utility.select2 = {};

    app.utility.select2.get = function (elementId) {
        if ($(elementId).select2('data').length > 0)
            return $(elementId).select2('data')[0];
        else
            return { id: null, text: null };
    };

    app.utility.select2.set = function (elementId, dataSource) {
        if (typeof dataSource !== "undefined") {
            return $(elementId).select2({
                data: dataSource
            });
        }
    };

    app.utility.select2.disable = function (elementId) {
        $(elementId).prop('disabled', true);
        $('#select2-' + elementId.substr(1) + '-container').css("background-color", "#f5f6fa");
    };

    app.utility.select2.enable = function (elementId) {
        $(elementId).prop('disabled', false);
        $('#select2-' + elementId.substr(1) + '-container').css("background-color", "#fff");
    };

    app.utility.rangedatepicker = function (elementId) {
        $('.input-daterange').datepicker({
            format: 'dd MM yyyy',
            todayHighlight: true
        });
    };

    app.utility.numericOnly = function (elementId) {
        $(elementId).keydown(function (event) {
            // Allow: backspace, delete, tab, escape, and enter
            if (event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 || event.keyCode == 13 ||
                // Allow: Ctrl+A
                (event.keyCode == 65 && event.ctrlKey === true) ||
                // Allow: home, end, left, rightasdasd123123
                (event.keyCode >= 35 && event.keyCode <= 39)) {
                // let it happen, don't do anything
                return;
            }
            else {
                // Ensure that it is a number and stop the keypress
                if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
                    event.preventDefault();
                }
            }
        });
    };

    // Month navigator. 
    app.utility.displayPreviousMonth = function () {
        d.setMonth(d.getMonth() - 1);
        $('#spanPeriod').val(monthNamesAbbr[d.getMonth()] + " " + d.getFullYear());
        $('#spanPeriod').attr('data-period', d.getFullYear() + '' + (d.getMonth().toString().length == 1 ? '0' + (d.getMonth() + 1) : (d.getMonth() + 1)));
        $('#divLoading').toggleClass('pm-visibility-hidden');
        //inProgressGettingData = false;
    };

    app.utility.displayNextMonth = function () {
        d.setMonth(d.getMonth() + 1);
        $('#spanPeriod').val(monthNamesAbbr[d.getMonth()] + " " + d.getFullYear());
        $('#spanPeriod').attr('data-period', d.getFullYear() + '' + (d.getMonth().toString().length == 1 ? '0' + (d.getMonth() + 1) : (d.getMonth() + 1)));
        $('#divLoading').toggleClass('pm-visibility-hidden');
        //inProgressGettingData = false;

    };

    app.utility.displayCurrentMonth = function () {
        $('#spanPeriod').text(monthNames[d.getMonth()] + " " + d.getFullYear());
        $('#spanPeriod').attr('data-period', d.getFullYear() + '' + (d.getMonth().toString().length == 1 ? '0' + (d.getMonth() + 1) : (d.getMonth() + 1)));
        $('#divLoading').toggleClass('pm-visibility-hidden');
        //inProgressGettingData = false;

    };

    app.utility.displayMonthNavigator = function () {
        $('#spanPeriod').val(monthNamesAbbr[d.getMonth()] + " " + d.getFullYear());
    };

    app.utility.displayUserCompetencySubordinate = function () {
        $('.pm-show-competency').prop('href', 'PKGolongan14');
    };

    app.utility.displayUserCompetencyTopLowPerformer = function () {
        $('.pm-show-competency').click(function () {
            window.open('PKGolongan14');
        });
    };

    app.utility.getBackgoundColor = function (status) {
        var oColor = ''

        switch (status) {
            case '1': //Belum Dimulai
                oColor = 'u-label g-bg-bluegray g-rounded-20 g-px-10';
                break;
            case '2': //Menunggu Penilaian Personal
                oColor = 'u-label g-bg-yellow g-rounded-20 g-px-10';
                break;
            case '3': //Menunggu Penilaian Supervisor
                oColor = 'u-label g-bg-blue g-rounded-20 g-px-10';
                break;
            case '4': //Menunggu Persetujuan Kepala Departemen
                oColor = 'u-label g-bg-purple g-rounded-20 g-px-10';
                break;
            case '5': //Menunggu Penilaian Rekan Kerja
                oColor = 'u-label g-bg-orange g-rounded-20 g-px-10';
                break;
            case '6': //Penilaian Karya Selesai 
                oColor = 'u-label g-bg-primary g-rounded-20 g-px-10';
                break;
            default:
                oColor = 'u-label g-bg-pink g-rounded-20 g-px-10';
                break;
        }

        return oColor;
    };

    app.utility.getRandomColor = function () {
        var idx = Math.floor(Math.random() * 13);

        var color = [
            //'abstract',
            //'lighter',
            'primary',
            'secondary',
            'success',
            'info',
            'warning',
            'danger',
            'dark',
            //'gray',
            //'light',
            'blue',
            'azure',
            'indigo',
            'purple',
            'pink',
            'orange',
            'teal',
            //'blue-dim',
            //'azure-dim',
            //'indigo-dim',
            //'purple-dim',
            //'pink-dim',
            //'orange-dim',
            //'teal-dim',
            //'primary-dim',
            //'secondary-dim',
            //'success-dim',
            //'info-dim',
            //'warning-dim',
            //'danger-dim',
            //'dark-dim',
            //'gray-dim',
        ]

        return color[idx];
    };

    app.utility.compare = function (currentElementId, requestElemetId, commentElementId) {
        if ($.trim($(currentElementId).text()) != $.trim($(requestElemetId).val())) {
            $(commentElementId).show();
            $(commentElementId).addClass('d-block');
            if ($(requestElemetId).hasClass('attachmentMandatory'))
                $(requestElemetId).addClass('attachment-mandatory');
        }
        else {
            $(commentElementId).removeClass('d-block');
            $(commentElementId).hide();
            if ($(requestElemetId).hasClass('attachmentMandatory'))
                $(requestElemetId).removeClass('attachment-mandatory');
        }
    }

    app.utility.compareCheckbox = function (currentElementId, requestElemetId, commentElementId) {
        if (($.trim($(currentElementId).text()) == 'true') != $(requestElemetId).is(":checked")) {
            $(commentElementId).show();
            $(commentElementId).addClass('d-block');
        }
        else {
            $(commentElementId).removeClass('d-block');
            $(commentElementId).hide();
        }
    }

    app.MessageBox = {};
    app.MessageBox.alert = function (message, callback, title, callback, options) {
        //var labelOk = "Tutup";

        //if (options) {
        //    labelOk = options.labelOK || labelOk;
        //}

        //return app.utility.Dialog({
        //    title: title || 'Peringatan'
        //    , message: message
        //    , show: true
        //    , onEscape: true
        //    , closeButton: true
        //    , animate: true
        //    , backdrop: 'static'
        //    , buttons: [{ className: 'btn u-btn-bluegray g-width-80', label: labelOk, callback: callback || true }]
        //});
    };

    app.MessageBox.success = function (message) {
        Swal.fire({
            text: message,
            icon: "success",
            buttonsStyling: false,
            confirmButtonText: "OK!",
            customClass: {
                confirmButton: "btn btn-primary"
            }
        }).then((result) => {
            if (result.isConfirmed) {
                return true;
            }
        });
    };

    app.MessageBox.invalid = function (message) {
        Swal.fire({
            icon: "warning",
            title: "Something went wrong!",
            text: message,
            footer: "Please contact your IT developer or teams."
        });
        //var labelOk = "Tutup";

        //if (options) {
        //    labelOk = options.labelOK || labelOk;
        //}

        //return app.utility.invalidDialog({
        //    title: title
        //    , message: message
        //    , show: true
        //    , onEscape: true
        //    , closeButton: true
        //    , animate: true
        //    , backdrop: 'static'
        //    , buttons: [{ className: 'btn u-btn-bluegray g-width-80', label: labelOk, callback: callback || true }]
        //});


    };

    app.MessageBox.invalidCallback = function (message, callback) {
        Swal.fire({
            icon: "warning",
            title: "Something went wrong!",
            text: message,
            footer: "Please contact your IT developer or teams."
        }).then(function (e) {
            if ($.isFunction(callback)) {
                return callback(e);
            }

        });
    };

    app.MessageBox.validate = function (message) {
        Swal.fire({
            icon: "warning",
            title: "",
            text: message,
            footer: ""
        });
    };

    app.MessageBox.confirm = function (message, callback) {
        Swal.fire({
            title: "Are you sure?",
            text: message,
            icon: "warning",
            showCancelButton: !0,
            confirmButtonText: "Yes"
        }).then(function (e) {
            if ($.isFunction(callback)) {
                return callback(e);
            }

        });

        //var labelOk = 'Ya';
        //var labelCancel = 'Tidak';

        //if (options) {
        //    labelOk = options.labelOK || labelOk;
        //    labelCancel = options.labelCancel || labelCancel;
        //}

        //var cancelCallback = function () {
        //    if (typeof callback === 'function') {
        //        return callback(false);
        //    }
        //};

        //var confirmCallback = function () {
        //    if (typeof callback === 'function') {
        //        return callback(true);
        //    }
        //};

        //return app.utility.confirmDialog({
        //    title: title
        //    , message: message
        //    , show: true
        //    , onEscape: true
        //    , closeButton: true
        //    , animate: true
        //    , backdrop: 'static'
        //    , buttons: [
        //        { className: 'btn u-btn-blue g-width-80', label: labelOk, callback: confirmCallback },
        //        { className: 'btn u-btn-bluegray g-width-80', label: labelCancel, callback: cancelCallback }
        //    ]
        //});
    };

    app.utility.FillForm = function (formID, resultData) {
        var form = formID;
        if (formID != null && typeof formID === "string")
            form = $("#" + formID);

        $.each($(form).find('input.datepicker, input.hasDatepicker'), function (i, element) {
            if (!$(element).hasClass('month-picker')) {
                var dataFrom = $(this).attr('data-from');
                var dataTo = $(this).attr('data-to');
                if (typeof dataFrom !== typeof undefined && dataFrom !== false)
                    $(element).datepicker("option", "minDate", null);
                if (typeof dataTo !== typeof undefined && dataTo !== false)
                    $(element).datepicker("option", "maxDate", null);
            }
        });

        var sendValueToElement = function (element, result, fieldName) {

            if (fieldName != null && fieldName != "" && result[fieldName] != null) {
                var displayValue = result[fieldName];
                if (!$(element).is('input:checkbox') && !$(element).is('input:radio')) {
                    $(element).val(displayValue);
                }
                if ($(element).is('span'))
                    $(element).html(displayValue);


                if ($(element).is('strong'))
                    $(element).html(displayValue);

                if ($(element).is('label'))
                    $(element).html(displayValue);


                $(element).attr("data-duplicate", displayValue);

                if ($(element).is('div'))
                    $(element).html(displayValue);
                $(element).attr("text", displayValue);

                if ($(element).is('input:radio')) {
                    $(element).prop('checked', $(element).val() == result[fieldName]);
                    $(element).trigger("change");
                }
                if ($(element).is('select'))
                    $(element).trigger("change");
                if ($(element).is('input:checkbox'))
                    $(element).prop('checked', result[fieldName]);
                if ($(element).hasClass('lookup-desc')) {
                    $(element).show();
                }
                // Untuk jxq widget
                if ($(element).hasClass('jqx-datetimeinput')) {
                    $(element).jqxDateTimeInput("setDate", FACE.Utils.SqlDateConverter(result[fieldName]));
                }
            }
        }
        $.each($(form).find("[data-lookup-field]"), function () {
            var fieldName = $(this).attr("data-lookup-field");

            sendValueToElement(this, resultData, fieldName);
        });
        $.each($(form).find("[data-lookup-rel]"), function () {
            var fieldName = $(this).attr("data-lookup-rel");
            sendValueToElement(this, resultData, fieldName);
        });
    }


    app.utility.Dialog = function (options) {
        var templates = {
            dialog:
                '<div class="modal fade" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static" data-keyboard="false">' +
                '   <div class="modal-dialog modal-dialog-centered" role="document">' +
                '       <div class="modal-content">' +
                '           <div class="modal-body">' +
                '<div class="alert alert-danger alert-dismissible fade show g-mb-0" role="alert">' +
                '<h4 class="h5">' +
                '   <i class="fa fa-minus-circle"></i> Something wrong!' +
                '</h4>' +
                '<p>Please contact your administrator with Log ID below for further assistance.</p>' +
                '   <ul class="u-alert-list g-mt-10 g-mb-0">' +
                '<li>' +
                '   Log ID : ' + options.message +
                '</li>' +
                '<li>' +
                '   User ID : ' + app.configuration.applicationUser.id + ' - ' + app.configuration.applicationUser.userName +
                '</li>' +
                '</ul>' +
                '</div>' +
                '           </div>' +
                '       </div>' +
                '   </div>' +
                '</div>',
            header:
                '',
            footer:
                '<div class="modal-footer g-pt-10 g-pb-10"></div>',
            closeButton:
                ''
        };

        var dialog = $(templates.dialog);
        var body = dialog.find(".modal-body");
        var buttons = options.buttons || [];
        var buttonbuilder = [];
        var callbacks = {
            onEscape: options.onEscape
        };

        // build buttons
        $.each(buttons, function (index, button) {
            var icon = '';
            if (button.icon) {
                icon = '<i class="' + button.icon + '"></i> ';
            }
            if (button.className) {
                buttonbuilder.push("<button data-km-handler='" + index + "' type='button' class='btn " + button.className + "'>" + icon + button.label + "</button>");
            }
            else {
                buttonbuilder.push("<button data-km-handler='" + index + "' type='button' class='btn btn-md u-btn-darkgray'>" + icon + button.label + "</button>");
            }

            callbacks[index] = button.callback;
        });

        body.before(templates.header);


        var titleMessage = '<h4 class="modal-title padding-bottom-5"></h4>';
        //if (options.title) {
        //    titleMessage = '<div class="u-heading-v1-1 g-bg-main g-brd-gray-light-v2 g-mb-40">'+
        //                    '<h2 class="h5 u-heading-v1__title" > ' + options.title+'</h2 >'+
        //                     '</div>';
        //}
        // '<h4 class="modal-title padding-bottom-5"><b>' + options.title + '</b></h4>';
        // set message
        //body.html(titleMessage + '<p>' + options.message + '</p>');

        // set animation, mostly this would be set to true
        if (options.animate === true) {
            dialog.addClass("fade");
        }

        // set additional classname to the dialog
        if (options.className) {
            dialog.addClass(options.className);
        }

        // note: ptfi modal always has a cloase button
        if (options.closeButton) {
            var closeButton = $(templates.closeButton);

            if (options.title) {
                dialog.find(".modal-header").prepend(closeButton);
            } else {
                closeButton.css("margin-top", "-10px").prependTo(body);
            }
        }

        // append the generated buttons that we build earlier
        if (buttonbuilder.length) {
            body.after(templates.footer);
            dialog.find(".modal-footer").html(buttonbuilder.join(''));
        }

        // Bootstrap event listeners. Use new namespaces event in bs 3.0
        dialog.on("hidden.bs.modal", function (e) {
            if (e.target === this) {
                dialog.remove();
            }
        });

        dialog.on("shown.bs.modal", function () {
            //dialog.find(".btn-primary:first").focus();
            dialog.find(".btn-info:first").focus();
        });

        //jQuery event listeners
        function processCallback(e, dialog, callback) {
            e.preventDefault();

            var preserveDialog = $.isFunction(callback) && callback(e) === false;
            if (!preserveDialog) {
                dialog.modal("hide");
            }
        }

        dialog.on("click", ".modal-footer button", function (e) {
            var callbackKey = $(this).data("km-handler");

            processCallback(e, dialog, callbacks[callbackKey]);
        });

        dialog.on("click", ".km-messagebox-close-button", function (e) {
            processCallback(e, dialog, callbacks.onEscape);
        });

        dialog.on("keyup", function (e) {
            if (e.which === 27) {
                //dialog.trigger("escape.close.bb");
                processCallback(e, dialog, callbacks.onEscape);
            }
        });

        // add dialog to the body
        $("body").append(dialog);

        var dialogoption = {
            backdrop: options.backdrop,
            keyboard: false,
            show: false
        };
        if (options.modaloptions) {
            dialogoption = $.extend({}, dialogoption, options.modaloptions);
        }

        dialog.modal(dialogoption);

        if (options.show) {
            dialog.modal("show");
        }

        return dialog;
    };

    app.utility.successDialog = function (options) {
        var templates = {
            dialog:
                '<div class="modal fade" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static" data-keyboard="false">' +
                '   <div class="modal-dialog modal-dialog-centered" role="document">' +
                '       <div class="modal-content g-px-20 g-py-10 g-bg-gray-light-v5">' +
                '<div class="modal-header g-py-10 g-pl-0"> ' +
                '<h5 class="modal-title">Informasi</h5>' +
                '</div> ' +
                '           <div class="modal-body g-py-10 g-pl-5">' +
                '<div class="form-group fade show g-mb-0"> ' +
                '<div class="media"> ' +

                '<div class="media-body"> ' +
                '<p class="g-mb-0"> ' + options.message + '</p>' +
                '</div> ' +
                '</div> ' +
                '</div> ' +
                '           </div>' +
                '       </div>' +
                '   </div>' +
                '</div>',
            header:
                '',
            footer:
                '<div class="modal-footer g-py-10 g-pr-0"></div>',
            closeButton:
                ''
        };

        var dialog = $(templates.dialog);
        var body = dialog.find(".modal-body");
        var buttons = options.buttons || [];
        var buttonbuilder = [];
        var callbacks = {
            onEscape: options.onEscape
        };

        // build buttons
        $.each(buttons, function (index, button) {
            var icon = '';
            if (button.icon) {
                icon = '<i class="' + button.icon + '"></i> ';
            }
            if (button.className) {
                buttonbuilder.push("<button data-km-handler='" + index + "' type='button' class='btn " + button.className + "'>" + icon + button.label + "</button>");
            }
            else {
                buttonbuilder.push("<button data-km-handler='" + index + "' type='button' class='btn u-btn-darkgray'>" + icon + button.label + "</button>");
            }

            callbacks[index] = button.callback;
        });

        body.before(templates.header);


        var titleMessage = '<h4 class="modal-title padding-bottom-5"></h4>';
        //if (options.title) {
        //    titleMessage = '<div class="u-heading-v1-1 g-bg-main g-brd-gray-light-v2 g-mb-40">'+
        //                    '<h2 class="h5 u-heading-v1__title" > ' + options.title+'</h2 >'+
        //                     '</div>';
        //}
        // '<h4 class="modal-title padding-bottom-5"><b>' + options.title + '</b></h4>';
        // set message
        //body.html(titleMessage + '<p>' + options.message + '</p>');

        // set animation, mostly this would be set to true
        if (options.animate === true) {
            dialog.addClass("fade");
        }

        // set additional classname to the dialog
        if (options.className) {
            dialog.addClass(options.className);
        }

        // note: ptfi modal always has a cloase button
        if (options.closeButton) {
            var closeButton = $(templates.closeButton);

            if (options.title) {
                dialog.find(".modal-header").prepend(closeButton);
            } else {
                closeButton.css("margin-top", "-10px").prependTo(body);
            }
        }

        // append the generated buttons that we build earlier
        if (buttonbuilder.length) {
            body.after(templates.footer);
            dialog.find(".modal-footer").html(buttonbuilder.join(''));
        }

        // Bootstrap event listeners. Use new namespaces event in bs 3.0
        dialog.on("hidden.bs.modal", function (e) {
            if (e.target === this) {
                dialog.remove();
            }
        });

        dialog.on("shown.bs.modal", function () {
            //dialog.find(".btn-primary:first").focus();
            dialog.find(".btn-info:first").focus();
        });

        //jQuery event listeners
        function processCallback(e, dialog, callback) {
            e.preventDefault();

            var preserveDialog = $.isFunction(callback) && callback(e) === false;
            if (!preserveDialog) {
                dialog.modal("hide");
            }
        }

        dialog.on("click", ".modal-footer button", function (e) {
            var callbackKey = $(this).data("km-handler");

            processCallback(e, dialog, callbacks[callbackKey]);
        });

        dialog.on("click", ".km-messagebox-close-button", function (e) {
            processCallback(e, dialog, callbacks.onEscape);
        });

        dialog.on("keyup", function (e) {
            if (e.which === 27) {
                //dialog.trigger("escape.close.bb");
                processCallback(e, dialog, callbacks.onEscape);
            }
        });

        // add dialog to the body
        $("body").append(dialog);

        var dialogoption = {
            backdrop: options.backdrop,
            keyboard: false,
            show: false
        };
        if (options.modaloptions) {
            dialogoption = $.extend({}, dialogoption, options.modaloptions);
        }

        dialog.modal(dialogoption);

        if (options.show) {
            dialog.modal("show");
        }

        return dialog;
    };

    app.utility.invalidDialog = function (options) {
        var templates = {
            dialog:
                '<div class="modal fade" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static" data-keyboard="false">' +
                '   <div class="modal-dialog modal-dialog-centered" role="document">' +
                '       <div class="modal-content g-px-20 g-py-10 g-bg-gray-light-v5">' +
                '<div class="modal-header g-py-10 g-pl-0"> ' +
                '<h5 class="modal-title">Perhatian</h5>' +
                '</div> ' +
                '           <div class="modal-body g-py-10 g-pl-5">' +
                '<div class="form-group fade show g-mb-0"> ' +
                '<div class="media"> ' +

                '<div class="media-body"> ' +
                '<p class="g-mb-0"> ' + options.message + '</p>' +
                '</div> ' +
                '</div> ' +
                '</div> ' +
                '           </div>' +
                '       </div>' +
                '   </div>' +
                '</div>',
            header:
                '',
            footer:
                '<div class="modal-footer g-py-10 g-pr-0"></div>',
            closeButton:
                ''
        };

        var dialog = $(templates.dialog);
        var body = dialog.find(".modal-body");
        var buttons = options.buttons || [];
        var buttonbuilder = [];
        var callbacks = {
            onEscape: options.onEscape
        };

        // build buttons
        $.each(buttons, function (index, button) {
            var icon = '';
            if (button.icon) {
                icon = '<i class="' + button.icon + '"></i> ';
            }
            if (button.className) {
                buttonbuilder.push("<button data-km-handler='" + index + "' type='button' class='btn " + button.className + "'>" + icon + button.label + "</button>");
            }
            else {
                buttonbuilder.push("<button data-km-handler='" + index + "' type='button' class='btn btn-md u-btn-darkgray'>" + icon + button.label + "</button>");
            }

            callbacks[index] = button.callback;
        });

        body.before(templates.header);


        var titleMessage = '<h4 class="modal-title padding-bottom-5"></h4>';
        //if (options.title) {
        //    titleMessage = '<div class="u-heading-v1-1 g-bg-main g-brd-gray-light-v2 g-mb-40">'+
        //                    '<h2 class="h5 u-heading-v1__title" > ' + options.title+'</h2 >'+
        //                     '</div>';
        //}
        // '<h4 class="modal-title padding-bottom-5"><b>' + options.title + '</b></h4>';
        // set message
        //body.html(titleMessage + '<p>' + options.message + '</p>');

        // set animation, mostly this would be set to true
        if (options.animate === true) {
            dialog.addClass("fade");
        }

        // set additional classname to the dialog
        if (options.className) {
            dialog.addClass(options.className);
        }

        // note: ptfi modal always has a cloase button
        if (options.closeButton) {
            var closeButton = $(templates.closeButton);

            if (options.title) {
                dialog.find(".modal-header").prepend(closeButton);
            } else {
                closeButton.css("margin-top", "-10px").prependTo(body);
            }
        }

        // append the generated buttons that we build earlier
        if (buttonbuilder.length) {
            body.after(templates.footer);
            dialog.find(".modal-footer").html(buttonbuilder.join(''));
        }

        // Bootstrap event listeners. Use new namespaces event in bs 3.0
        dialog.on("hidden.bs.modal", function (e) {
            if (e.target === this) {
                dialog.remove();
            }
        });

        dialog.on("shown.bs.modal", function () {
            //dialog.find(".btn-primary:first").focus();
            dialog.find(".btn-info:first").focus();
        });

        //jQuery event listeners
        function processCallback(e, dialog, callback) {
            e.preventDefault();

            var preserveDialog = $.isFunction(callback) && callback(e) === false;
            if (!preserveDialog) {
                dialog.modal("hide");
            }
        }

        dialog.on("click", ".modal-footer button", function (e) {
            var callbackKey = $(this).data("km-handler");

            processCallback(e, dialog, callbacks[callbackKey]);
        });

        dialog.on("click", ".km-messagebox-close-button", function (e) {
            processCallback(e, dialog, callbacks.onEscape);
        });

        dialog.on("keyup", function (e) {
            if (e.which === 27) {
                //dialog.trigger("escape.close.bb");
                processCallback(e, dialog, callbacks.onEscape);
            }
        });

        // add dialog to the body
        $("body").append(dialog);

        var dialogoption = {
            backdrop: options.backdrop,
            keyboard: false,
            show: false
        };
        if (options.modaloptions) {
            dialogoption = $.extend({}, dialogoption, options.modaloptions);
        }

        dialog.modal(dialogoption);

        if (options.show) {
            dialog.modal("show");
        }

        return dialog;
    };

    app.utility.confirmDialog = function (options) {
        var templates = {
            dialog:
                '<div class="modal fade" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static" data-keyboard="false">' +
                '   <div class="modal-dialog modal-dialog-centered" role="document">' +
                '       <div class="modal-content g-px-20 g-py-10 g-bg-gray-light-v5">' +
                '<div class="modal-header g-py-10 g-pl-0"> ' +
                '<h5 class="modal-title">Konfirmasi</h5>' +
                '</div> ' +
                '           <div class="modal-body g-py-10 g-pl-5">' +
                '<div class="form-group fade show g-mb-0"> ' +
                '<div class="media"> ' +

                '<div class="media-body"> ' +
                '<p class="g-mb-0"> ' + options.message + '</p>' +
                '</div> ' +
                '</div> ' +
                '</div> ' +
                '           </div>' +
                '       </div>' +
                '   </div>' +
                '</div>',
            header:
                '',
            footer:
                '<div class="modal-footer g-py-10 g-pr-0"></div>',
            closeButton:
                ''
        };

        var dialog = $(templates.dialog);
        var body = dialog.find(".modal-body");
        var buttons = options.buttons || [];
        var buttonbuilder = [];
        var callbacks = {
            onEscape: options.onEscape
        };

        // build buttons
        $.each(buttons, function (index, button) {
            var icon = '';
            if (button.icon) {
                icon = '<i class="' + button.icon + '"></i> ';
            }
            if (button.className) {
                buttonbuilder.push("<button data-km-handler='" + index + "' type='button' class='btn " + button.className + "'>" + icon + button.label + "</button>");
            }
            else {
                buttonbuilder.push("<button data-km-handler='" + index + "' type='button' class='btn btn-md u-btn-bluegray'>" + icon + button.label + "</button>");
            }

            callbacks[index] = button.callback;
        });

        body.before(templates.header);


        var titleMessage = '<h4 class="modal-title padding-bottom-5"></h4>';
        //if (options.title) {
        //    titleMessage = '<div class="u-heading-v1-1 g-bg-main g-brd-gray-light-v2 g-mb-40">'+
        //                    '<h2 class="h5 u-heading-v1__title" > ' + options.title+'</h2 >'+
        //                     '</div>';
        //}
        // '<h4 class="modal-title padding-bottom-5"><b>' + options.title + '</b></h4>';
        // set message
        //body.html(titleMessage + '<p>' + options.message + '</p>');

        // set animation, mostly this would be set to true
        if (options.animate === true) {
            dialog.addClass("fade");
        }

        // set additional classname to the dialog
        if (options.className) {
            dialog.addClass(options.className);
        }

        // note: ptfi modal always has a cloase button
        if (options.closeButton) {
            var closeButton = $(templates.closeButton);

            if (options.title) {
                dialog.find(".modal-header").prepend(closeButton);
            } else {
                closeButton.css("margin-top", "-10px").prependTo(body);
            }
        }

        // append the generated buttons that we build earlier
        if (buttonbuilder.length) {
            body.after(templates.footer);
            dialog.find(".modal-footer").html(buttonbuilder.join(''));
        }

        // Bootstrap event listeners. Use new namespaces event in bs 3.0
        dialog.on("hidden.bs.modal", function (e) {
            if (e.target === this) {
                dialog.remove();
            }
        });

        dialog.on("shown.bs.modal", function () {
            //dialog.find(".btn-primary:first").focus();
            dialog.find(".btn-info:first").focus();
        });

        //jQuery event listeners
        function processCallback(e, dialog, callback) {
            e.preventDefault();

            var preserveDialog = $.isFunction(callback) && callback(e) === false;
            if (!preserveDialog) {
                dialog.modal("hide");
            }
        }

        dialog.on("click", ".modal-footer button", function (e) {
            var callbackKey = $(this).data("km-handler");

            processCallback(e, dialog, callbacks[callbackKey]);
        });

        dialog.on("click", ".km-messagebox-close-button", function (e) {
            processCallback(e, dialog, callbacks.onEscape);
        });

        dialog.on("keyup", function (e) {
            if (e.which === 27) {
                //dialog.trigger("escape.close.bb");
                processCallback(e, dialog, callbacks.onEscape);
            }
        });

        // add dialog to the body
        $("body").append(dialog);

        var dialogoption = {
            backdrop: options.backdrop,
            keyboard: false,
            show: false
        };
        if (options.modaloptions) {
            dialogoption = $.extend({}, dialogoption, options.modaloptions);
        }

        dialog.modal(dialogoption);

        if (options.show) {
            dialog.modal("show");
        }

        return dialog;
    };


    app.utility.displayUserCompetency360 = function () {
        $('.pm-show-competency-360').prop('href', 'Competency360');
    };

    app.utility.logoff = function () {
        localStorage.removeItem('jwt_token');
        window.location.href = '/account/signin';
        
        // jQuery.support.cors = true;

        // if (localStorage.getItem('user_name') === null) {
        //     window.location.href = window.location.origin;
        // }

        // var dParam = JSON.stringify(localStorage.getItem('user_name'));

        // $.ajax(
        //     {
        //         url: app.baseUrl + '/api/user/logout',
        //         type: 'POST',
        //         data: dParam,
        //         dataType: 'json',
        //         contentType: "application/json",
        //         crossDomain: true,
        //         xhrFields: {
        //             withCredentials: false
        //         },
        //         cache: true,
        //         async: true,
        //         beforeSend: function (xhr) {

        //         }
        //     })
        //     .done(function (data, textStatus, jqXHR) {
        //         localStorage.removeItem('user_name');
        //         localStorage.removeItem('access_token');
        //         window.location.href = window.location.origin;
        //     })
        //     .fail(function (jqXHR, textStatus, errorThrown) {
        //         app.MessageBox.invalid(jqXHR.responseText);
        //     });
    };

    app.utility.setFormDisabled = function (formId, value) {
        $("#" + formId + " :input").attr("disabled", value);
    };

    app.utility.onImageError = function (element) {
        $(element).attr('src', app.baseUrl + '/assets/media/misc/no-image.png');
    }

    app.utility.onNoImage = function (element) {
        $(element).attr('src', app.baseUrl + '/assets/media/misc/no-image.png');
    }

    $('#buttonLogout').unbind().click(function () {
        app.utility.logoff();
    });

    app.utility.getInitial = function (name) {
        if (!name) return '';

        // Bersihkan spasi berlebih
        name = $.trim(name);

        // Pisahkan berdasarkan spasi
        var parts = name.split(/\s+/);
        var initials = $.map(parts, function(word) {
            return word.charAt(0).toUpperCase();
        });

        // Kembalikan hasil
        return initials.length > 1 ? initials.slice(0, 2).join('') : initials[0];
    }

    app.utility.getFirstInitial = function (name) {
        if (!name) return '';

        name = $.trim(name); // hapus spasi di awal & akhir
        return name.charAt(0).toUpperCase(); // ambil huruf pertama & kapital
    }

    app.utility.loadImage = function (imageUrl, altText = "") {
        // fallback jika tidak ada image
        const safeUrl = imageUrl && imageUrl.trim() !== ""
            ? imageUrl
            : "/assets/media/misc/no-image.png";

        // gunakan template Metronic symbol
        // tambahkan class js-zoom-image agar mudah attach event
        return `
            <div class="symbol-label js-zoom-image"
                style="background-image: url('${safeUrl}');
                        background-size: cover;
                        background-position: center;
                        width: 45px;
                        height: 45px;
                        border-radius: 6px;"
                data-image="${safeUrl}"
                title="Klik untuk zoom"
                role="button">
            </div>
        `;
    };

    app.utility.loadAvatar = function (element, name) {
        let color = app.bgcolor();
        if (element) {
            return `<img alt="Pic" src="${element}" style="object-fit: cover;" />`;
        } else {
            return `<span class="symbol-label bg-${color} text-inverse-${color} fw-bold">${app.utility.getFirstInitial(name)}</span>`;
        }
    }


    app.utility.removeJSON = function (data, uniqueKey) {
        for (var i = 0; i < data.length; i++) {
            if (data[i].keyId == uniqueKey) {
                data.splice(i, 1);
                break;
            }
        }
        return data;
    }

    app.utility.removeAttachment = function (data, key) {
        for (var i = 0; i < data.length; i++) {
            if (data[i].fileOriginalName == key) {
                data.splice(i, 1);
                break;
            }
        }
        return data;
    }

    app.utility.changeSelect2Color = function (select2Id, oldValue) {
        var changeColor = $('#' + select2Id).next('span');
        var newValue = $('#' + select2Id).val() == null ? '' : $('#' + select2Id).val();
        if (newValue === oldValue) {
            changeColor.find('.select2-selection').removeClass("u-select-change");
        }
        else {
            changeColor.find('.select2-selection').addClass("u-select-change");
        }
    }

    app.utility.defaultGender = function (slRelationship, slGender) {
        $(slRelationship).on("change", function (e) {
            if (['11', '31', '91'].includes($(slRelationship).val())) {      //11 = bapak, 32 = saudara laki-laki, 92 = mertua laki=laki
                $(slGender).val(1).trigger('change');                  //1 = male
            }
            else if (['12', '32', '92'].includes($(slRelationship).val())) { //12 = ibu, 32 = saudara perempuan, 92 = mertua perempuan
                $(slGender).val(2).trigger('change');                  //2 = female
            }
        })
    }

    app.security = {};

    app.security.isAdministrator = function () {
        if (app.profile.roleId === 1 ||
            app.profile.roleId === 7 ||
            app.profile.roleId === 15 ||
            app.profile.roleId === 16 ||
            app.profile.roleId === 17 ||
            app.profile.roleId === 18) {
            return true;
        }

        return false;
    }

    app.security.isFinance = function () {
        if (app.profile.roleId === 1 ||
            app.profile.roleId === 7 ||
            app.profile.roleId === 16) {
            return true;
        }

        return false;
    }


    $(document).ready(function () {
        $.support.cors = true;
        var ajaxRequestList = []; // holds all ajax requests

        //app.profile();
        // default ajax 
        //$.ajaxSetup({
        //    type: "POST",
        //    dataType: "json",
        //    contentType: "application/json;charset=utf-8",
        //    cache: "false", //bimbas 31 jan: set cache to false to make sure in IE API always call to server
        //    crossDomain: true,
        //    xhrFields: {
        //        withCredentials: true
        //    }
        //});
        $(document).ajaxStart(function () {
            //tv.showProgressBar();
            //var x;
        });
        $(document).ajaxStop(function () {
            //tv.hideProgressBar();
            //var x;
        });
        $(document).ajaxSend(function (event, XMLHttpRequest, ajaxOptions) {

            if (ajaxOptions.suppressProgressBar || ajaxOptions.dataType == "iframe json") {
                // TODO: iframe json is to handle jquery upload, maybe we need to revisit this again when we implemented jquery upload
                return;
            }
            else {
                ajaxRequestList.push(XMLHttpRequest);
                //tv.ProgressBar.show();
            }

        });
        $(document).ajaxComplete(function (event, XMLHttpRequest, ajaxOptions) {
            if (ajaxOptions.suppressProgressBar) {
                return;
            }
            else {
                ajaxRequestList = jQuery.grep(ajaxRequestList, function (value) {
                    return value != XMLHttpRequest;
                });
                if (ajaxRequestList.length == 0) {
                    //tv.ProgressBar.hide();
                }
            }
        });

        $(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
            if (typeof jqXHR.responseText !== 'undefined' && jqXHR.responseText.indexOf("session.expired") > 0) {
                //location.reload();
                localStorage.removeItem('jwt_token');
                window.location.href = '/account/signin';
                return;
            }

            if (ajaxSettings.suppressErrorMessageBox) {
                return;
            }

            //var ex = app.Error.getErrorException(jqXHR, thrownError);

            console.log(jqXHR);
            console.log(thrownError);

        });
        window.onerror = function (msg, url, line) {
            // this is to handle if there is an error in application ajax done event
            ajaxRequestList = jQuery.grep(ajaxRequestList, function (value) {
                return value.readyState != 4;
            });
            if (ajaxRequestList.length == 0) {
                //tv.ProgressBar.hide();
            }

            console.log("Global error caught: " + msg + "\nurl: " + url + "\nline: " + line);

            // TODO: Report this error via ajax so you can keep track
            //       of what pages have JS issues

            var suppressErrorAlert = true;
            // If you return true, then error alerts (like in older versions of 
            // Internet Explorer) will be suppressed.
            return suppressErrorAlert;
        };

    });
}(jQuery, app));