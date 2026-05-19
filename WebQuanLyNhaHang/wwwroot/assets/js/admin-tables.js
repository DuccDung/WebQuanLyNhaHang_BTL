(function () {
  const mobileNavTrigger = document.getElementById("mobile-nav-trigger");
  const mobileNavBackdrop = document.getElementById("mobile-nav-backdrop");
  const dashboardSidebar = document.getElementById("dashboard-sidebar");
  const loadingOverlay = document.getElementById("tableLoadingOverlay");
  setupTableCards();
  setupTableQr();
  setupOrderNotification();

  function setupMobileNav() {
    if (!mobileNavTrigger || !mobileNavBackdrop || !dashboardSidebar) {
      return;
    }

    let hideBackdropTimer = 0;

    const syncMobileNav = (isOpen) => {
      mobileNavTrigger.setAttribute("aria-expanded", String(isOpen));

      if (isOpen) {
        window.clearTimeout(hideBackdropTimer);
        mobileNavBackdrop.hidden = false;
        window.requestAnimationFrame(() => {
          document.body.classList.add("is-mobile-nav-open");
        });
        return;
      }

      document.body.classList.remove("is-mobile-nav-open");
      hideBackdropTimer = window.setTimeout(() => {
        if (!document.body.classList.contains("is-mobile-nav-open")) {
          mobileNavBackdrop.hidden = true;
        }
      }, 260);
    };

    mobileNavTrigger.addEventListener("click", () => {
      syncMobileNav(!document.body.classList.contains("is-mobile-nav-open"));
    });

    mobileNavBackdrop.addEventListener("click", () => {
      syncMobileNav(false);
    });

    dashboardSidebar.addEventListener("click", (event) => {
      if (event.target.closest("a")) {
        syncMobileNav(false);
      }
    });

    window.addEventListener("resize", () => {
      if (window.innerWidth > 1080) {
        syncMobileNav(false);
      }
    });

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        syncMobileNav(false);
      }
    });
  }

  function setupAccountMenu() {
    const trigger = document.getElementById("account-trigger");
    const dropdown = document.getElementById("account-dropdown");

    if (!trigger || !dropdown) {
      return;
    }

    trigger.addEventListener("click", () => {
      const isOpen = !dropdown.hidden;
      dropdown.hidden = isOpen;
      trigger.setAttribute("aria-expanded", String(!isOpen));
    });

    document.addEventListener("click", (event) => {
      if (dropdown.hidden || event.target.closest(".account-menu")) {
        return;
      }

      dropdown.hidden = true;
      trigger.setAttribute("aria-expanded", "false");
    });

    document.addEventListener("keydown", (event) => {
      if (event.key !== "Escape") {
        return;
      }

      dropdown.hidden = true;
      trigger.setAttribute("aria-expanded", "false");
    });
  }

  function setupSalesMenu() {
    const salesGroup = document.getElementById("sales-group");
    const salesToggle = document.getElementById("sales-group-toggle");
    const salesSubmenu = document.getElementById("sales-submenu");

    if (!salesGroup || !salesToggle || !salesSubmenu) {
      return;
    }

    salesToggle.addEventListener("click", () => {
      const isOpen = salesGroup.classList.toggle("is-open");
      salesSubmenu.hidden = !isOpen;
      salesToggle.setAttribute("aria-expanded", String(isOpen));
      salesToggle.classList.toggle("is-active", isOpen);
    });
  }

  function setupTableCards() {
    const cards = Array.from(document.querySelectorAll("[data-table-card]"));

    cards.forEach((card) => {
      card.addEventListener("click", (event) => {
        if (event.target.closest("[data-qr-trigger]")) {
          return;
        }

        loadTableComponent(card.getAttribute("data-table-id"));
      });

      card.addEventListener("keydown", (event) => {
        if (event.target.closest("[data-qr-trigger]")) {
          return;
        }

        if (event.key !== "Enter" && event.key !== " ") {
          return;
        }

        event.preventDefault();
        loadTableComponent(card.getAttribute("data-table-id"));
      });
    });
  }

  function setupTableQr() {
    const modal = document.getElementById("qrModal");

    if (!modal) {
      return;
    }

    const title = document.getElementById("qrModalTitle");
    const codeBox = document.getElementById("qrCodeBox");
    const linkText = modal.querySelector("[data-qr-link]");
    const openLink = modal.querySelector("[data-qr-open]");
    const closeButtons = modal.querySelectorAll("[data-qr-close]");
    const qrButtons = Array.from(document.querySelectorAll("[data-qr-trigger]"));

    const closeModal = () => {
      modal.setAttribute("aria-hidden", "true");
    };

    closeButtons.forEach((button) => {
      button.addEventListener("click", closeModal);
    });

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        closeModal();
      }
    });

    qrButtons.forEach((button) => {
      button.addEventListener("click", (event) => {
        event.preventDefault();
        event.stopPropagation();

        const card = button.closest("[data-table-card]");
        const tableId = card?.getAttribute("data-table-id");
        const tableUrl = card?.getAttribute("data-table-url");

        if (!tableId || !tableUrl || !codeBox) {
          return;
        }

        renderQrCode(codeBox, tableUrl);

        if (title) {
          title.textContent = "Bàn " + tableId;
        }

        if (linkText) {
          linkText.textContent = tableUrl;
        }

        if (openLink) {
          openLink.setAttribute("href", tableUrl);
        }

        modal.setAttribute("aria-hidden", "false");
      });
    });
  }

  function renderQrCode(container, url) {
    container.innerHTML = "";

    if (window.QRCode) {
      new window.QRCode(container, {
        text: url,
        width: 196,
        height: 196,
        colorDark: "#111827",
        colorLight: "#ffffff",
        correctLevel: window.QRCode.CorrectLevel.H
      });
      return;
    }

    const image = document.createElement("img");
    image.alt = "QR gọi món";
    image.src = "https://api.qrserver.com/v1/create-qr-code/?size=196x196&data=" + encodeURIComponent(url);
    container.appendChild(image);
  }

  function loadTableComponent(id) {
    if (!id || !window.jQuery) {
      return;
    }

    setTableLoading(true);

    window.jQuery.ajax({
      url: "/Admin/GetFormBuy",
      method: "GET",
      data: { id },
      success: function (response) {
        window.jQuery("#component-FormBuy").html(response);
        window.jQuery(".container-buy").addClass("visible");
        window.jQuery(".overlay").addClass("visible");
      },
      error: function () {
        alert("Không tải được thông tin bàn. Vui lòng thử lại.");
      },
      complete: function () {
        setTableLoading(false);
      }
    });
  }

  function setTableLoading(isLoading) {
    if (!loadingOverlay) {
      return;
    }

    loadingOverlay.hidden = !isLoading;
  }

  function setupOrderNotification() {
    if (!window.signalR) {
      return;
    }

    const connection = new window.signalR.HubConnectionBuilder()
      .withUrl("/chathub")
      .build();

    connection.start().catch(function () {
      console.warn("Không thể kết nối thông báo đơn hàng.");
    });

    connection.on("OderSuccess", function (orderId) {
      playOrderSound();
      fetchOrderNotification(orderId)
        .then(function (order) {
          updateTableCardFromOrder(order);
          showOrderAlert(order);
        })
        .catch(function () {
          window.location.reload();
        });
    });
  }

  function playOrderSound() {
    const audio = document.getElementById("success-sound");

    if (!audio) {
      return;
    }

    audio.currentTime = 0;
    audio.play().catch(function () {
      // Trinh duyet co the chan am thanh neu admin chua thao tac tren trang.
    });
  }

  function fetchOrderNotification(orderId) {
    const url = orderId
      ? "/Admin/LatestOrderNotification?orderId=" + encodeURIComponent(orderId)
      : "/Admin/LatestOrderNotification";

    return fetch(url, {
      headers: {
        "Accept": "application/json"
      }
    }).then(function (response) {
      if (!response.ok) {
        throw new Error("Cannot load latest order");
      }

      return response.json();
    });
  }

  function showOrderAlert(order) {
    const alert = document.getElementById("orderAlert");

    if (!alert || !order) {
      window.location.reload();
      return;
    }

    setOrderAlertText("[data-order-alert-location]", order.locationLabel || "Chưa xác định");
    setOrderAlertText("[data-order-alert-code]", order.orderCode || "--");
    setOrderAlertText("[data-order-alert-customer]", order.customerName || "Khách lẻ");
    setOrderAlertText("[data-order-alert-phone]", order.customerPhone || "Chưa có SĐT");
    setOrderAlertText("[data-order-alert-total]", order.totalLabel || "--");

    const viewButton = alert.querySelector("[data-order-alert-view]");
    const closeButtons = alert.querySelectorAll("[data-order-alert-close]");
    const closeAlert = function (shouldReload) {
      alert.setAttribute("aria-hidden", "true");

      if (shouldReload) {
        window.setTimeout(function () {
          window.location.reload();
        }, 180);
      }
    };

    closeButtons.forEach(function (button) {
      button.onclick = function () {
        closeAlert(true);
      };
    });

    if (viewButton) {
      viewButton.onclick = function () {
        if (order.tableId) {
          closeAlert(false);
          loadTableComponent(order.tableId);
          return;
        }

        window.location.href = "/DonHangs";
      };
    }

    alert.setAttribute("aria-hidden", "false");
  }

  function updateTableCardFromOrder(order) {
    if (!order || !order.tableId) {
      return;
    }

    const card = document.querySelector('[data-table-card][data-table-id="' + String(order.tableId) + '"]');

    if (!card) {
      return;
    }

    card.classList.remove("is-empty", "is-reserved");
    card.classList.add("is-busy");

    const status = card.querySelector(".status-pill");
    if (status) {
      status.className = "restaurant-table-card__status status-pill status-pill--busy";
      status.textContent = "Đang Có Đơn Hàng";
    }

    let divider = card.querySelector(".restaurant-table-card__divider");
    const seats = card.querySelector(".restaurant-table-card__seats");

    if (!divider) {
      divider = document.createElement("span");
      divider.className = "restaurant-table-card__divider";

      if (seats) {
        seats.insertAdjacentElement("afterend", divider);
      } else {
        card.appendChild(divider);
      }
    }

    let meta = card.querySelector(".restaurant-table-card__meta");

    if (!meta) {
      meta = document.createElement("span");
      meta.className = "restaurant-table-card__meta";
      divider.insertAdjacentElement("afterend", meta);
    }

    meta.innerHTML =
      '<span class="restaurant-table-card__meta-line">' +
        '<svg viewBox="0 0 24 24" aria-hidden="true">' +
          '<circle cx="12" cy="12" r="9"></circle>' +
          '<path d="M12 7v5l3 2"></path>' +
        '</svg>' +
        '<span></span>' +
      '</span>' +
      '<span class="restaurant-table-card__meta-line is-money">' +
        '<span class="restaurant-table-card__money-symbol">$</span>' +
        '<span></span>' +
      '</span>';

    const timeText = meta.querySelector(".restaurant-table-card__meta-line:first-child span:last-child");
    const amountText = meta.querySelector(".restaurant-table-card__meta-line.is-money span:last-child");

    if (timeText) {
      timeText.textContent = order.tableTimeLabel || order.orderTime || "";
    }

    if (amountText) {
      amountText.textContent = order.tableAmountLabel || order.totalLabel || "";
    }
  }

  function setOrderAlertText(selector, value) {
    const element = document.querySelector(selector);

    if (element) {
      element.textContent = value;
    }
  }

})();
