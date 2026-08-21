(function () {
    const TOKEN_KEY = "aia_access_token";
    const USER_KEY = "aia_user_profile";
    const EXPIRY_KEY = "aia_token_expiry";

    let logoutTimer = null;

    window.aiaAuth = {
        set: function (token, payload, rememberMe) {
            localStorage.setItem(TOKEN_KEY, token);
            localStorage.setItem(USER_KEY, JSON.stringify(payload));
            
            const expiryTimeMs = payload?.exp ? payload.exp * 1000 : Date.now() + 60 * 60 * 1000;
            localStorage.setItem(EXPIRY_KEY, expiryTimeMs.toString());

            this.scheduleExpiryRedirect(expiryTimeMs);
        },

        get: function () {
            const token = localStorage.getItem(TOKEN_KEY);
            const expiry = localStorage.getItem(EXPIRY_KEY);

            if (!token || !expiry) return null;
            if (Date.now() >= parseInt(expiry, 10)) {
                this.clear();
                return null;
            }

            return {
                token: token,
                user: JSON.parse(localStorage.getItem(USER_KEY) || "{}"),
                expiresAt: parseInt(expiry, 10)
            };
        },

        clear: function () {
            if (logoutTimer) clearTimeout(logoutTimer);
            localStorage.removeItem(TOKEN_KEY);
            localStorage.removeItem(USER_KEY);
            localStorage.removeItem(EXPIRY_KEY);
        },

        scheduleExpiryRedirect: function (expiryTimeMs) {
            if (logoutTimer) clearTimeout(logoutTimer);

            const delay = expiryTimeMs - Date.now();
            if (delay <= 0) {
                this.clear();
                window.location.href = "/Account/Login";
                return;
            }

            logoutTimer = setTimeout(() => {
                this.clear();
                window.location.href = "/Account/Login?sessionExpired=true";
            }, delay);
        },

        init: function () {
            const session = this.get();
            if (session) {
                this.scheduleExpiryRedirect(session.expiresAt);
            }

            // Sync auth state across all active tabs in the same browser profile
            window.addEventListener("storage", (event) => {
                if (event.key === TOKEN_KEY && event.newValue === null) {
                    // Logged out in another tab
                    this.clear();
                    window.location.href = "/Account/Login";
                }
            });
        }
    };
    
    window.aiaAuth.init();
})();