(function () {
    "use strict";

    if (window.cloudyAdminServiceNotificationsStarted) {
        return;
    }

    window.cloudyAdminServiceNotificationsStarted = true;

    var signalRUrl = "https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/5.0.9/signalr.min.js";
    var soundUrl = "/asset/sound/notify_sound.mp3";
    var containerId = "admin-service-notification-stack";
    var audioId = "admin-service-notification-sound";
    var unlockedAudio = false;

    function ensureStyles() {
        if (document.getElementById("admin-service-notification-style")) {
            return;
        }

        var style = document.createElement("style");
        style.id = "admin-service-notification-style";
        style.textContent = [
            "#admin-service-notification-stack{position:fixed;right:18px;top:18px;z-index:99999;display:grid;gap:10px;max-width:min(360px,calc(100vw - 28px));}",
            ".admin-service-notification{border:1px solid rgba(255,148,24,.28);border-radius:12px;background:#fff;box-shadow:0 18px 45px rgba(15,23,42,.18);padding:14px 14px 13px;color:#1f2933;font-family:Arial,sans-serif;animation:adminServiceNotificationIn .22s ease-out;}",
            ".admin-service-notification__head{display:flex;align-items:flex-start;justify-content:space-between;gap:12px;margin-bottom:8px;}",
            ".admin-service-notification__title{margin:0;color:#111827;font-size:15px;font-weight:700;line-height:1.25;}",
            ".admin-service-notification__time{margin-top:2px;color:#8a8f98;font-size:12px;white-space:nowrap;}",
            ".admin-service-notification__body{display:grid;gap:5px;color:#4b5563;font-size:13px;line-height:1.35;}",
            ".admin-service-notification__body strong{color:#f08300;font-weight:700;}",
            ".admin-service-notification__close{border:0;background:transparent;color:#9ca3af;font-size:20px;line-height:1;cursor:pointer;padding:0 0 0 8px;}",
            "@keyframes adminServiceNotificationIn{from{opacity:0;transform:translateY(-8px)}to{opacity:1;transform:translateY(0)}}"
        ].join("");
        document.head.appendChild(style);
    }

    function ensureContainer() {
        ensureStyles();
        var container = document.getElementById(containerId);
        if (!container) {
            container = document.createElement("div");
            container.id = containerId;
            document.body.appendChild(container);
        }

        return container;
    }

    function ensureAudio() {
        var audio = document.getElementById(audioId);
        if (!audio) {
            audio = document.createElement("audio");
            audio.id = audioId;
            audio.src = soundUrl;
            audio.preload = "auto";
            document.body.appendChild(audio);
        }

        return audio;
    }

    function unlockAudio() {
        if (unlockedAudio) {
            return;
        }

        var audio = ensureAudio();
        audio.volume = 0.85;
        audio.play().then(function () {
            audio.pause();
            audio.currentTime = 0;
            unlockedAudio = true;
        }).catch(function () {
            // Browser may require a real user gesture first; the next click will retry.
        });
    }

    function playSound() {
        var audio = ensureAudio();
        audio.currentTime = 0;
        audio.volume = 0.85;
        audio.play().catch(function () {
            unlockAudio();
        });
    }

    function text(value, fallback) {
        if (value === null || value === undefined || String(value).trim() === "") {
            return fallback || "";
        }

        return String(value).trim();
    }

    function escapeHtml(value) {
        return text(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function buildDetailLines(request) {
        var lines = [
            "<span>Bàn: <strong>" + escapeHtml(text(request.tableCode, "A--")) + "</strong></span>",
            "<span>Khách: " + escapeHtml(text(request.customerName, "Quý khách")) + "</span>"
        ];

        if (request.paymentMethodLabel) {
            lines.push("<span>Thanh toán: " + escapeHtml(request.paymentMethodLabel) + "</span>");
        }

        if (request.ratingLabel) {
            lines.push("<span>Đánh giá: " + escapeHtml(request.ratingLabel) + "</span>");
        }

        if (Array.isArray(request.tags) && request.tags.length) {
            lines.push("<span>Lý do: " + request.tags.map(function (tag) { return escapeHtml(tag); }).join(", ") + "</span>");
        }

        if (request.message) {
            lines.push("<span>Nội dung: " + escapeHtml(request.message) + "</span>");
        }

        if (request.phone) {
            lines.push("<span>SĐT: " + escapeHtml(request.phone) + "</span>");
        }

        return lines.join("");
    }

    function showNotification(request) {
        var container = ensureContainer();
        var item = document.createElement("section");
        item.className = "admin-service-notification";
        item.setAttribute("role", "status");
        item.innerHTML = [
            "<div class=\"admin-service-notification__head\">",
            "<div>",
            "<p class=\"admin-service-notification__title\">" + escapeHtml(text(request.typeLabel, "Yêu cầu mới")) + "</p>",
            "<div class=\"admin-service-notification__body\">" + buildDetailLines(request) + "</div>",
            "</div>",
            "<div class=\"admin-service-notification__time\">" + escapeHtml(text(request.createdAt, "Vừa xong")) + "</div>",
            "<button class=\"admin-service-notification__close\" type=\"button\" aria-label=\"Đóng\">×</button>",
            "</div>"
        ].join("");

        item.querySelector("button").addEventListener("click", function () {
            item.remove();
        });

        container.prepend(item);
        playSound();

        setTimeout(function () {
            item.remove();
        }, 12000);
    }

    function loadSignalR() {
        if (window.signalR) {
            return Promise.resolve();
        }

        return new Promise(function (resolve, reject) {
            var script = document.createElement("script");
            script.src = signalRUrl;
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);
        });
    }

    function startConnection() {
        loadSignalR().then(function () {
            if (!window.signalR || window.cloudyAdminServiceConnection) {
                return;
            }

            var connection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .withAutomaticReconnect()
                .build();

            connection.on("DineInServiceRequested", showNotification);
            connection.start().catch(function (error) {
                console.error("Không kết nối được thông báo realtime admin.", error);
            });

            window.cloudyAdminServiceConnection = connection;
        }).catch(function (error) {
            console.error("Không tải được SignalR cho thông báo admin.", error);
        });
    }

    document.addEventListener("click", unlockAudio, { once: false });
    document.addEventListener("keydown", unlockAudio, { once: false });

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", startConnection);
    } else {
        startConnection();
    }
})();
