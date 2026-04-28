(function () {
  const mobileNavTrigger = document.getElementById("mobile-nav-trigger");
  const mobileNavBackdrop = document.getElementById("mobile-nav-backdrop");
  const dashboardSidebar = document.getElementById("dashboard-sidebar");
  const productsScrollStorageKey = "products-index-scroll-position";
  let pageLoadingElement = null;
  let applyProductFilters = () => {};

  setupLoadingUi();
  setupMobileNav();
  setupCreateModal();
  setupActionModals();
  setupModalEditForms();
  setupModalDeleteForms();
  setupAccountMenu();
  setupSalesMenu();
  setupProductFilters();
  restoreProductsScrollPosition();

  function setupCreateModal() {
    const modal = document.getElementById("create-product-modal");
    const openTrigger = document.querySelector("[data-open-create-modal]");
    const closeTriggers = Array.from(document.querySelectorAll("[data-close-create-modal]"));

    if (!modal) {
      return;
    }

    const fileInput = modal.querySelector("[data-file-input]");
    const fileInputLabel = modal.querySelector("[data-file-input-label]");

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
          : "Chưa chọn tệp ảnh";

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

    const closeModal = closeModalElement;

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
          : "Chưa chọn tệp ảnh";
      });
    });
  }

  function setupModalEditForms() {
    const forms = Array.from(document.querySelectorAll("[data-edit-modal-form]"));

    if (!forms.length || !window.fetch) {
      return;
    }

    forms.forEach((form) => {
      const submitButton = form.querySelector("button[type='submit']");
      const messageBox = form.querySelector("[data-edit-modal-message]");

      form.addEventListener("submit", async (event) => {
        event.preventDefault();

        if (form.dataset.submitting === "true") {
          return;
        }

        form.dataset.submitting = "true";
        form.classList.add("is-loading");
        setButtonLoading(submitButton, true);
        showPageLoading("Đang lưu sản phẩm...");

        setEditFormMessage(messageBox, "");

        try {
          const response = await fetch(form.action, {
            method: form.method || "POST",
            body: new FormData(form),
            headers: {
              "X-Requested-With": "XMLHttpRequest"
            },
            credentials: "same-origin"
          });
          const contentType = response.headers.get("content-type") || "";
          const result = contentType.includes("application/json")
            ? await response.json()
            : null;

          if (!response.ok || !result || !result.success) {
            setEditFormMessage(messageBox, result && result.message
              ? result.message
              : "Không thể lưu thay đổi. Vui lòng thử lại.");
            return;
          }

          updateProductAfterEdit(result.product, form);
          closeModalElement(form.closest(".modal-shell"));
        } catch (error) {
          setEditFormMessage(messageBox, "Không thể kết nối máy chủ. Vui lòng thử lại.");
        } finally {
          form.dataset.submitting = "false";
          form.classList.remove("is-loading");
          setButtonLoading(submitButton, false);
          hidePageLoading();
        }
      });
    });
  }

  function setupModalDeleteForms() {
    const forms = Array.from(document.querySelectorAll("[data-delete-modal-form]"));

    if (!forms.length || !window.fetch) {
      return;
    }

    forms.forEach((form) => {
      const submitButton = form.querySelector("button[type='submit']");

      form.addEventListener("submit", async (event) => {
        event.preventDefault();

        if (form.dataset.submitting === "true") {
          return;
        }

        form.dataset.submitting = "true";
        form.classList.add("is-loading");
        setButtonLoading(submitButton, true);
        showPageLoading("Đang xóa sản phẩm...");

        try {
          const response = await fetch(form.action, {
            method: form.method || "POST",
            body: new FormData(form),
            headers: {
              "X-Requested-With": "XMLHttpRequest"
            },
            credentials: "same-origin"
          });
          const contentType = response.headers.get("content-type") || "";
          const result = contentType.includes("application/json")
            ? await response.json()
            : null;

          if (!response.ok || !result || !result.success) {
            window.alert(result && result.message
              ? result.message
              : "Không thể xóa sản phẩm. Vui lòng thử lại.");
            return;
          }

          closeModalElement(form.closest(".modal-shell"));
          removeProductFromPage(result.productId);
        } catch (error) {
          window.alert("Không thể kết nối máy chủ. Vui lòng thử lại.");
        } finally {
          form.dataset.submitting = "false";
          form.classList.remove("is-loading");
          setButtonLoading(submitButton, false);
          hidePageLoading();
        }
      });
    });
  }

  function setupLoadingUi() {
    ensurePageLoading();
    setupFormLoading();
    setupNavigationLoading();

    window.addEventListener("pageshow", () => {
      hidePageLoading();
      document.querySelectorAll(".is-loading").forEach((element) => {
        element.classList.remove("is-loading");
        element.removeAttribute("aria-busy");

        if ("disabled" in element && element.dataset.loadingDisabled === "true") {
          element.disabled = false;
          delete element.dataset.loadingDisabled;
        }
      });
    });
  }

  function setupFormLoading() {
    document.querySelectorAll("form:not([data-edit-modal-form]):not([data-delete-modal-form])").forEach((form) => {
      form.addEventListener("submit", () => {
        if (form.dataset.submitting === "true") {
          return;
        }

        if (typeof form.checkValidity === "function" && !form.checkValidity()) {
          return;
        }

        form.dataset.submitting = "true";
        form.classList.add("is-loading");
        setButtonLoading(form.querySelector("button[type='submit'], input[type='submit']"), true);
        showPageLoading(resolveFormLoadingMessage(form));
      });
    });
  }

  function setupNavigationLoading() {
    document.addEventListener("click", (event) => {
      if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
        return;
      }

      const clickedElement = event.target instanceof Element
        ? event.target
        : event.target?.parentElement;
      const link = clickedElement?.closest("a[href]");

      if (!link || link.matches("[data-open-create-modal], [data-open-modal]")) {
        return;
      }

      if (link.hasAttribute("download") || (link.target && link.target !== "_self")) {
        return;
      }

      const rawHref = link.getAttribute("href") || "";
      const normalizedHref = rawHref.trim().toLowerCase();

      if (!rawHref || normalizedHref.startsWith("#") || normalizedHref.startsWith("javascript:") || normalizedHref.startsWith("mailto:") || normalizedHref.startsWith("tel:")) {
        return;
      }

      const targetUrl = new URL(rawHref, window.location.href);

      if (targetUrl.origin !== window.location.origin) {
        return;
      }

      if (targetUrl.pathname === window.location.pathname && targetUrl.search === window.location.search && !targetUrl.hash) {
        return;
      }

      showPageLoading("Đang tải dữ liệu...");
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
    const searchInput = document.querySelector("[data-product-search]");
    const categoryFilter = document.querySelector("[data-category-filter]");
    const visibleCount = document.querySelector("[data-visible-count]");
    const totalCount = document.querySelector("[data-total-count]");
    const emptyRow = document.querySelector("[data-empty-row]");

    if (!visibleCount || !totalCount) {
      return;
    }

    applyProductFilters = () => {
      const rows = Array.from(document.querySelectorAll("[data-product-row]"));
      const keyword = normalizeValue(searchInput && "value" in searchInput ? searchInput.value : "");
      const category = normalizeValue(categoryFilter && "value" in categoryFilter ? categoryFilter.value : "");
      let matched = 0;

      totalCount.textContent = formatCount(rows.length);

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
      searchInput.addEventListener("input", applyProductFilters);
    }

    if (categoryFilter) {
      categoryFilter.addEventListener("change", applyProductFilters);
    }

    applyProductFilters();
  }

  function updateProductAfterEdit(product, form) {
    const productInfo = normalizeProductInfo(product);

    if (!productInfo.productId) {
      return;
    }

    updateProductRow(productInfo);
    updateProductPreviewModal(productInfo);
    updateProductDeleteModal(productInfo);

    const fileLabel = form.querySelector("[data-file-input-label]");
    const fileInput = form.querySelector("[data-file-input]");
    if (fileInput) {
      fileInput.value = "";
    }

    if (fileLabel) {
      fileLabel.textContent = productInfo.imagePath ? "Đang dùng ảnh hiện tại" : "Chưa chọn tệp ảnh";
    }

    applyProductFilters();
  }

  function updateProductRow(productInfo) {
    const row = document.querySelector(`[data-product-row][data-product-id="${productInfo.productId}"]`);

    if (!row) {
      return;
    }

    const status = resolveProductStatus(productInfo);
    const typeLabel = resolveProductTypeLabel(productInfo.categoryName);

    row.dataset.name = normalizeValue(productInfo.name);
    row.dataset.category = normalizeValue(productInfo.categoryName);
    setText(row.querySelector("[data-product-name]"), productInfo.displayName);
    setText(row.querySelector("[data-product-description]"), productInfo.displayDescription);
    setText(row.querySelector("[data-product-category-label]"), productInfo.categoryName);
    setText(row.querySelector("[data-product-price]"), formatCurrency(productInfo.price));
    setText(row.querySelector("[data-product-type-label]"), typeLabel);

    const statusElement = row.querySelector("[data-product-status]");
    if (statusElement) {
      statusElement.className = `status-badge ${status.cssClass}`;
      statusElement.textContent = status.label;
    }

    replaceProductMedia(row.querySelector("[data-product-thumb-cell]"), productInfo, "table");

    row.querySelectorAll("[data-open-modal]").forEach((element) => {
      const modalTarget = element.getAttribute("data-open-modal") || "";

      if (modalTarget.includes("preview")) {
        element.setAttribute("aria-label", `Xem chi tiết ${productInfo.displayName}`);
      } else if (modalTarget.includes("edit")) {
        element.setAttribute("aria-label", `Chỉnh sửa ${productInfo.displayName}`);
      } else if (modalTarget.includes("delete")) {
        element.setAttribute("aria-label", `Xóa ${productInfo.displayName}`);
      }
    });
  }

  function updateProductPreviewModal(productInfo) {
    const modal = document.getElementById(`product-preview-${productInfo.productId}`);

    if (!modal) {
      return;
    }

    const status = resolveProductStatus(productInfo);
    const typeLabel = resolveProductTypeLabel(productInfo.categoryName);

    modal.querySelectorAll("[data-preview-name]").forEach((element) => {
      element.textContent = productInfo.displayName;
    });
    setText(modal.querySelector("[data-preview-description]"), productInfo.displayDescription);
    setText(modal.querySelector("[data-preview-category]"), productInfo.categoryName);
    setText(modal.querySelector("[data-preview-type]"), typeLabel);
    setText(modal.querySelector("[data-preview-price]"), formatCurrency(productInfo.price));
    replaceProductMedia(modal.querySelector("[data-preview-media]"), productInfo, "preview");

    const statusElement = modal.querySelector("[data-preview-status]");
    if (statusElement) {
      statusElement.className = status.cssClass;
      statusElement.textContent = status.label;
    }
  }

  function updateProductDeleteModal(productInfo) {
    const modal = document.getElementById(`product-delete-${productInfo.productId}`);

    if (!modal) {
      return;
    }

    setText(modal.querySelector("[data-delete-product-name]"), productInfo.displayName);
  }

  function removeProductFromPage(productId) {
    const row = document.querySelector(`[data-product-row][data-product-id="${productId}"]`);

    if (row) {
      row.classList.add("is-removing");
      window.setTimeout(() => {
        row.remove();
        applyProductFilters();
      }, 180);
    } else {
      applyProductFilters();
    }

    ["preview", "edit", "delete"].forEach((modalName) => {
      document.getElementById(`product-${modalName}-${productId}`)?.remove();
    });
  }

  function normalizeProductInfo(product) {
    const productId = Number(product && product.productId ? product.productId : 0);
    const name = String(product && product.name ? product.name : "").trim();
    const description = String(product && product.description ? product.description : "").trim();
    const categoryName = String(product && product.categoryName ? product.categoryName : "Chưa phân loại").trim();
    const imagePath = String(product && product.imagePath ? product.imagePath : "").trim();
    const rawPrice = product && product.price !== null && product.price !== undefined
      ? Number(product.price)
      : 0;

    return {
      productId,
      name,
      displayName: name || "Sản phẩm chưa đặt tên",
      description,
      displayDescription: description || "Chưa có mô tả cho sản phẩm này.",
      categoryName: categoryName || "Chưa phân loại",
      imagePath,
      price: Number.isFinite(rawPrice) ? rawPrice : 0
    };
  }

  function replaceProductMedia(container, productInfo, variant) {
    if (!container) {
      return;
    }

    const currentMedia = container.querySelector("img, .table-avatar, span");
    const nextMedia = productInfo.imagePath
      ? document.createElement("img")
      : document.createElement("span");

    if (productInfo.imagePath) {
      nextMedia.src = productInfo.imagePath;
      nextMedia.alt = productInfo.displayName;
      if (variant === "table") {
        nextMedia.className = "product-thumb";
      }
    } else {
      nextMedia.textContent = getProductInitial(productInfo.displayName);
      if (variant === "table") {
        nextMedia.className = "table-avatar";
      }
    }

    if (currentMedia) {
      currentMedia.replaceWith(nextMedia);
      return;
    }

    container.prepend(nextMedia);
  }

  function resolveProductStatus(productInfo) {
    if (!productInfo.imagePath) {
      return {
        cssClass: "is-yellow",
        label: "Thiếu ảnh"
      };
    }

    if (!productInfo.price || productInfo.price <= 0) {
      return {
        cssClass: "is-red",
        label: "Thiếu giá"
      };
    }

    if (!productInfo.description) {
      return {
        cssClass: "is-blue",
        label: "Cần mô tả"
      };
    }

    return {
      cssClass: "is-green",
      label: "Sẵn sàng"
    };
  }

  function resolveProductTypeLabel(categoryName) {
    const normalizedCategory = String(categoryName || "").trim().toUpperCase();

    if (["TEA", "JUICE", "HOT DRINK", "SIGNATURE", "BEST MENU"].includes(normalizedCategory)) {
      return "Đồ uống";
    }

    if (["CAKE", "APPETIZERS"].includes(normalizedCategory)) {
      return "Món ăn";
    }

    if (normalizedCategory === "TOPPING") {
      return "Topping";
    }

    return "Sản phẩm";
  }

  function getProductInitial(name) {
    return String(name || "SP").trim().charAt(0).toUpperCase() || "S";
  }

  function setText(element, value) {
    if (element) {
      element.textContent = value;
    }
  }

  function normalizeValue(value) {
    return String(value || "").trim().toLowerCase();
  }

  function isDeleteForm(form) {
    return normalizeValue(form.getAttribute("action")).includes("delete");
  }

  function resolveFormLoadingMessage(form) {
    const action = normalizeValue(form.getAttribute("action"));

    if (isDeleteForm(form)) {
      return "Đang xóa sản phẩm...";
    }

    if (action.includes("create")) {
      return "Đang thêm sản phẩm...";
    }

    return "Đang xử lý dữ liệu...";
  }

  function getProductsScrollContainer() {
    return document.querySelector(".dashboard-scroll");
  }

  function saveProductsScrollPosition() {
    try {
      const scrollContainer = getProductsScrollContainer();
      const scrollTop = scrollContainer
        ? scrollContainer.scrollTop
        : window.scrollY || document.documentElement.scrollTop || 0;

      window.sessionStorage.setItem(productsScrollStorageKey, JSON.stringify({
        path: window.location.pathname,
        scrollTop,
        windowY: window.scrollY || document.documentElement.scrollTop || 0
      }));
    } catch (error) {
      // Session storage can be unavailable in some privacy modes.
    }
  }

  function restoreProductsScrollPosition() {
    let savedPosition = null;

    try {
      const rawValue = window.sessionStorage.getItem(productsScrollStorageKey);
      if (!rawValue) {
        return;
      }

      savedPosition = JSON.parse(rawValue);
      window.sessionStorage.removeItem(productsScrollStorageKey);
    } catch (error) {
      return;
    }

    if (!savedPosition || typeof savedPosition.scrollTop !== "number") {
      return;
    }

    const applyScrollPosition = () => {
      const scrollContainer = getProductsScrollContainer();

      if (scrollContainer) {
        scrollContainer.scrollTop = savedPosition.scrollTop;
        return;
      }

      window.scrollTo({
        top: savedPosition.windowY || savedPosition.scrollTop,
        left: 0,
        behavior: "auto"
      });
    };

    applyScrollPosition();
    window.requestAnimationFrame(applyScrollPosition);
    window.setTimeout(applyScrollPosition, 80);
    window.setTimeout(applyScrollPosition, 240);
  }

  function ensurePageLoading() {
    if (pageLoadingElement) {
      return pageLoadingElement;
    }

    pageLoadingElement = document.querySelector("[data-page-loading]");

    if (pageLoadingElement) {
      return pageLoadingElement;
    }

    pageLoadingElement = document.createElement("div");
    pageLoadingElement.className = "page-loading";
    pageLoadingElement.setAttribute("data-page-loading", "");
    pageLoadingElement.setAttribute("aria-hidden", "true");
    pageLoadingElement.innerHTML = `
      <div class="page-loading__card" role="status" aria-live="polite">
        <span class="page-loading__spinner" aria-hidden="true"></span>
        <span class="page-loading__copy">
          <strong data-page-loading-title>Đang xử lý...</strong>
          <small>Vui lòng chờ trong giây lát</small>
        </span>
      </div>
    `;

    document.body.appendChild(pageLoadingElement);
    return pageLoadingElement;
  }

  function showPageLoading(message) {
    const loadingElement = ensurePageLoading();
    const title = loadingElement.querySelector("[data-page-loading-title]");

    if (title) {
      title.textContent = message || "Đang xử lý...";
    }

    loadingElement.setAttribute("aria-hidden", "false");
    document.body.classList.add("is-page-loading");
  }

  function hidePageLoading() {
    document.body.classList.remove("is-page-loading");

    if (pageLoadingElement) {
      pageLoadingElement.setAttribute("aria-hidden", "true");
    }
  }

  function setButtonLoading(button, isLoading) {
    if (!button) {
      return;
    }

    button.classList.toggle("is-loading", isLoading);
    button.setAttribute("aria-busy", String(isLoading));

    if (!("disabled" in button)) {
      return;
    }

    if (isLoading) {
      button.dataset.loadingDisabled = "true";
      button.disabled = true;
      return;
    }

    if (button.dataset.loadingDisabled === "true") {
      button.disabled = false;
      delete button.dataset.loadingDisabled;
    }

    button.removeAttribute("aria-busy");
  }

  function setEditFormMessage(messageBox, message) {
    if (!messageBox) {
      return;
    }

    messageBox.textContent = message;
    messageBox.classList.toggle("is-visible", Boolean(message));
  }

  function formatCurrency(value) {
    return `${new Intl.NumberFormat("vi-VN").format(Number(value || 0))}đ`;
  }

  function formatCount(value) {
    return new Intl.NumberFormat("vi-VN").format(Number(value || 0));
  }

  function closeModalElement(modal) {
    if (!modal) {
      return;
    }

    modal.classList.remove("is-open");
    modal.setAttribute("aria-hidden", "true");
    syncBodyModalState();
  }

  function syncBodyModalState() {
    document.body.classList.toggle("modal-open", Boolean(document.querySelector(".modal-shell.is-open")));
  }
})();
