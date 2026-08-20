/**
 * elearning-api.js
 * Shared API utility for all E-Learning components.
 */
(function ($, app) {
    'use strict';
    app.elearning = app.elearning || {};

    app.elearning.api = async function (path, opts) {
        var token = window.aiaAuth && window.aiaAuth.getToken();
        if (!token) { window.aiaAuth && window.aiaAuth.signOut(); return null; }
        opts = opts || {};
        var isFormData = opts.body instanceof FormData;
        var baseHeaders = { 'Authorization': 'Bearer ' + token };
        if (!isFormData) baseHeaders['Content-Type'] = 'application/json';
        opts.headers = $.extend(baseHeaders, opts.headers || {});
        try {
            var res = await fetch('https://localhost:7089' + path, opts);
            if (res.status === 401) { window.aiaAuth.signOut(); return null; }
            if (res.status === 404) return null;
            if (!res.ok) return null;
            var json = await res.json().catch(function () { return {}; });
            if (json && json.isError) return null;
            return json;
        } catch (err) {
            console.error('[ELearning API Error]', path, err);
            return null;
        }
    };

    app.elearning.unwrap = function (json, fallback) {
        if (json === null || json === undefined) return fallback !== undefined ? fallback : null;
        if (json.content !== undefined) return json.content;
        if (json.data !== undefined) return json.data;
        return json;
    };

    app.elearning.showSkeleton = function (selector, rows) {
        rows = rows || 3;
        var html = '';
        for (var i = 0; i < rows; i++) {
            html += '<div class="el-skeleton-row mb-3" style="height:52px;background:linear-gradient(90deg,#f1f1f4 25%,#e8e8ee 50%,#f1f1f4 75%);background-size:200% 100%;border-radius:8px;animation:el-skeleton-shimmer 1.4s infinite;"></div>';
        }
        $(selector).html(html);
    };

})(jQuery, window.app = window.app || {});
