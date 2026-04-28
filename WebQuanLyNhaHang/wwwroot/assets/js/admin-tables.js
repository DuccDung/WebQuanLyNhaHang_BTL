(function () {
  const mobileNavTrigger = document.getElementById("mobile-nav-trigger");
  const mobileNavBackdrop = document.getElementById("mobile-nav-backdrop");
  const dashboardSidebar = document.getElementById("dashboard-sidebar");
  const loadingOverlay = document.getElementById("tableLoadingOverlay");

  setupMobileNav();
  setupAccountMenu();
  setupSalesMenu();
  setupTableCards();
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
      card.addEventListener("click", () => {
        loadTableComponent(card.getAttribute("data-table-id"));
      });
    });
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

    connection.on("OderSuccess", function () {
      const audio = document.getElementById("success-sound");

      if (!audio) {
        window.location.reload();
        return;
      }

      audio.play().then(function () {
        audio.onended = function () {
          window.location.reload();
        };
      }).catch(function () {
        window.location.reload();
      });
    });
  }
})();
