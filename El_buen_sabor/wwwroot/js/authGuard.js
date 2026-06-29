(function () {
    const path = window.location.pathname.toLowerCase();
    const loginPath = "/";

    function clearSession() {
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
    }

    function redirectToLogin() {
        clearSession();
        if (path !== loginPath) {
            window.location.replace(loginPath);
        }
    }

    function parsePayload(token) {
        const parts = token.split(".");
        if (parts.length < 2) {
            return null;
        }

        try {
            const base64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
            const padded = base64.padEnd(base64.length + (4 - base64.length % 4) % 4, "=");
            return JSON.parse(atob(padded));
        } catch {
            return null;
        }
    }

    function readRole(payload) {
        const roleKeys = [
            "role",
            "roles",
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ];

        for (const key of roleKeys) {
            const value = payload[key];
            if (Array.isArray(value) && value.length > 0) {
                return normalizeRole(value[0]);
            }

            if (typeof value === "string" && value.trim()) {
                return normalizeRole(value);
            }
        }

        return "";
    }

    function normalizeRole(role) {
        const value = (role || "").trim().toUpperCase();
        if (value === "ADMIN") return "ADMIN";
        if (value === "CASHIER" || value === "CAJERO") return "CASHIER";
        if (value === "KITCHEN" || value === "COCINA") return "KITCHEN";
        if (value === "WAITRESS" || value === "WAITER" || value === "CAMARERA") return "WAITRESS";
        return value;
    }

    function isExpired(payload) {
        const exp = Number(payload.exp);
        if (!Number.isFinite(exp)) {
            return true;
        }

        return exp <= Math.floor(Date.now() / 1000);
    }

    function homeForRole(role) {
        if (role === "WAITRESS") return "/waiter";
        if (role === "KITCHEN") return "/kitchen";
        return "/DashboardSection";
    }

    function canAccess(role) {
        if (path === loginPath) return true;
        if (path === "/waiter") return role === "WAITRESS" || role === "ADMIN";
        if (path === "/kitchen") return role === "KITCHEN" || role === "ADMIN";
        return role === "ADMIN" || role === "CASHIER";
    }

    const token = localStorage.getItem("token");
    if (!token) {
        redirectToLogin();
        return;
    }

    const payload = parsePayload(token);
    if (!payload || isExpired(payload)) {
        redirectToLogin();
        return;
    }

    const role = readRole(payload);
    if (path === loginPath) {
        window.location.replace(homeForRole(role));
        return;
    }

    if (!canAccess(role)) {
        window.location.replace(homeForRole(role));
    }
})();
