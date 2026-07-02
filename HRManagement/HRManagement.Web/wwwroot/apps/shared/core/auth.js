(function () {
    const TOKEN_KEY = 'aia_jwt_token';
    const USER_KEY = 'aia_user_info';
    const EXPIRY_KEY = 'aia_auth_expiry';

    function pickStore() {
        if (localStorage.getItem(TOKEN_KEY)) return localStorage;
        if (sessionStorage.getItem(TOKEN_KEY)) return sessionStorage;
        return null;
    }

    function clear() {
        [localStorage, sessionStorage].forEach(store => {
            store.removeItem(TOKEN_KEY);
            store.removeItem(USER_KEY);
            store.removeItem(EXPIRY_KEY);
        });
    }

    function isExpired() {
        const store = pickStore();

        if (!store)
            return true;

        const expiry = parseInt(store.getItem(EXPIRY_KEY) || "0", 10);

        if (!expiry)
            return true;

        if (Date.now() > expiry) {
            clear();
            return true;
        }

        return false;
    }

    function getToken() {
        if (isExpired())
            return null;

        const store = pickStore();

        return store ? store.getItem(TOKEN_KEY) : null;
    }

    function getUserInfo() {

        if (isExpired())
            return null;

        const store = pickStore();

        if (!store)
            return null;

        try {
            return JSON.parse(store.getItem(USER_KEY) || "null");
        }
        catch {
            return null;
        }
    }

    function set(token, payload, rememberMe) {

        const now = Date.now();

        const expiry =
            rememberMe
                ? now + (7 * 24 * 60 * 60 * 1000)
                : now + (60 * 60 * 1000);

        const store = rememberMe
            ? localStorage
            : sessionStorage;

        const other = rememberMe
            ? sessionStorage
            : localStorage;

        store.setItem(TOKEN_KEY, token);
        store.setItem(USER_KEY, JSON.stringify(payload));
        store.setItem(EXPIRY_KEY, expiry.toString());

        other.removeItem(TOKEN_KEY);
        other.removeItem(USER_KEY);
        other.removeItem(EXPIRY_KEY);
    }

    function signOut() {

        clear();

        window.location.replace("/Account/Login");
    }

    window.aiaAuth = {

        getToken,
        getUserInfo,
        set,
        clear,
        signOut,
        isExpired

    };

})();