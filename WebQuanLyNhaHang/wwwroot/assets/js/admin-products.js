(function () {
  const mobileNavTrigger = document.getElementById("mobile-nav-trigger");
  const mobileNavBackdrop = document.getElementById("mobile-nav-backdrop");
  const dashboardSidebar = document.getElementById("dashboard-sidebar");

  setupMobileNav();
  setupCreateModal();
  setupActionModals();
  setupAccountMenu();
  setupSalesMenu();
  setupProductFilters();

  function setupCreateModal() {
    const modal = document.getElementById("create-product-modal");
    const openTrigger = document.querySelector("[data-open-create-modal]");
    const closeTriggers = Array.from(document.querySelectorAll("[data-close-create-modal]"));
    const fileInput = modal.querySelector("[data-file-input]");
    const fileInputLabel = modal.querySelector("[data-file-input-label]");

    if (!modal) {
      return;
    }

    const setModalState = (isOpen) => {
      modal.classList.toggle("is-open", isOpen);
      modal.setAttribute("aria-hidden", String(!isOpen));
      syncBodyModalState();
    };

    if (openTrigger) {
      openTrigger.addEventListener("click", (event) => {
        event.preventDefault();
        setModalState(true);
      });
    }

    closeTriggers.forEach((trigger) => {
      trigger.addEventListener("click", () => {
        setModalState(false);
      });
    });

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape" && modal.classList.contains("is-open")) {
        setModalState(false);
      }
    });

    if (fileInput && fileInputLabel) {
      fileInput.addEventListener("change", () => {
        const fileName = fileInput.files && fileInput.files.length
          ? fileInput.files[0].name
          : "Chua chon tep anh";

        fileInputLabel.textContent = fileName;
      });
    }

    const openOnLoad = modal.getAttribute("data-open-on-load") === "true";
    setModalState(openOnLoad);
  }

  function setupActionModals() {
    const openTriggers = Array.from(document.querySelectorAll("[data-open-modal]"));
    const closeTriggers = Array.from(document.querySelectorAll("[data-close-modal]"));

    const openModal = (modal) => {
      if (!modal) {
        return;
      }

      modal.classList.add("is-open");
      modal.setAttribute("aria-hidden", "false");
      syncBodyModalState();

      const firstInput = modal.querySelector("input:not([type='hidden']), textarea, select, button");
      if (firstInput) {
        window.setTimeout(() => firstInput.focus({ preventScroll: true }), 80);
      }
    };

    const closeModal = (modal) => {
      if (!modal) {
        return;
      }

      modal.classList.remove("is-open");
      modal.setAttribute("aria-hidden", "true");
      syncBodyModalState();
    };

    openTriggers.forEach((trigger) => {
      trigger.addEventListener("click", (event) => {
        event.preventDefault();
        openModal(document.getElementById(trigger.getAttribute("data-open-modal")));
      });
    });

    closeTriggers.forEach((trigger) => {
      trigger.addEventListener("click", () => {
        closeModal(trigger.closest(".modal-shell"));
      });
    });

    document.addEventListener("keydown", (event) => {
      if (event.key !== "Escape") {
        return;
      }

      const activeModal = document.querySelector(".modal-shell.is-open");
      closeModal(activeModal);
    });

    document.querySelectorAll(".product-action-modal [data-file-input]").forEach((input) => {
      const label = input.closest(".field-control--file")?.querySelector("[data-file-input-label]");
      if (!label) {
        return;
      }

      input.addEventListener("change", () => {
        label.textContent = input.files && input.files.length
          ? input.files[0].name
          : "Chua chon tep anh";
      });
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

  function setupProductFilters() {
    const rows = Array.from(document.querySelectorAll("[data-product-row]"));
    const searchInput = document.querySelector("[data-product-search]");
    const categoryFilter = document.querySelector("[data-category-filter]");
    const visibleCount = document.querySelector("[data-visible-count]");
    const totalCount = document.querySelector("[data-total-count]");
    const emptyRow = document.querySelector("[data-empty-row]");

    if (!rows.length || !visibleCount || !totalCount) {
      return;
    }

    totalCount.textContent = formatCount(rows.length);

    const applyFilters = () => {
      const keyword = normalizeValue(searchInput && "value" in searchInput ? searchInput.value : "");
      const category = normalizeValue(categoryFilter && "value" in categoryFilter ? categoryFilter.value : "");
      let matched = 0;

      rows.forEach((row) => {
        const rowName = normalizeValue(row.getAttribute("data-name"));
        const rowCategory = normalizeValue(row.getAttribute("data-category"));
        const matchesKeyword = !keyword || rowName.includes(keyword);
        const matchesCategory = !category || rowCategory === category;
        const isVisible = matchesKeyword && matchesCategory;

        row.hidden = !isVisible;
        if (isVisible) {
          matched += 1;
        }
      });

      visibleCount.textContent = formatCount(matched);
      if (emptyRow) {
        emptyRow.hidden = matched !== 0;
      }
    };

    if (searchInput) {
      searchInput.addEventListener("input", applyFilters);
    }

    if (categoryFilter) {
      categoryFilter.addEventListener("change", applyFilters);
    }

    applyFilters();
  }

  function normalizeValue(value) {
    return String(value || "").trim().toLowerCase();
  }

  function formatCount(value) {
    return new Intl.NumberFormat("vi-VN").format(Number(value || 0));
  }

  function syncBodyModalState() {
    document.body.classList.toggle("modal-open", Boolean(document.querySelector(".modal-shell.is-open")));
  }
})();
