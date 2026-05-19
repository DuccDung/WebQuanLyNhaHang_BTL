(function () {
  const mobileNavTrigger = document.getElementById("mobile-nav-trigger");
  const mobileNavBackdrop = document.getElementById("mobile-nav-backdrop");
  const dashboardSidebar = document.getElementById("dashboard-sidebar");
  let applyCustomerSearch = () => {};
  setupCustomerSearch();
  setupCreateCustomerModal();
  setupCustomerDetailModal();

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

  function setupCustomerSearch() {
    const searchInput = document.querySelector("[data-customer-search]");
    const emptyRow = document.querySelector("[data-empty-row]");

    if (!searchInput) {
      return;
    }

    applyCustomerSearch = () => {
      const rows = Array.from(document.querySelectorAll("[data-customer-row]"));
      const keyword = normalizeValue(searchInput.value);
      let visibleCount = 0;

      rows.forEach((row) => {
        const rowSearch = normalizeValue(row.getAttribute("data-search"));
        const isVisible = !keyword || rowSearch.includes(keyword);

        row.hidden = !isVisible;
        if (isVisible) {
          visibleCount += 1;
        }
      });

      if (emptyRow) {
        emptyRow.hidden = visibleCount !== 0;
      }
    };

    searchInput.addEventListener("input", applyCustomerSearch);
    applyCustomerSearch();
  }

  function setupCreateCustomerModal() {
    const openTrigger = document.querySelector("[data-open-customer-create]");
    const modal = document.getElementById("customer-create-modal");
    const form = document.querySelector("[data-customer-create-form]");
    const alertBox = document.querySelector("[data-customer-create-alert]");
    const submitButton = document.querySelector("[data-customer-create-submit]");
    const tableBody = document.querySelector("[data-customers-table] tbody");
    const emptyRow = document.querySelector("[data-empty-row]");

    if (!openTrigger || !modal || !form || !tableBody) {
      return;
    }

    const openModal = () => {
      clearAlert();
      modal.classList.add("is-open");
      modal.setAttribute("aria-hidden", "false");
      window.requestAnimationFrame(() => {
        form.querySelector("input")?.focus();
      });
    };

    const closeModal = () => {
      modal.classList.remove("is-open");
      modal.setAttribute("aria-hidden", "true");
      setLoading(false);
    };

    openTrigger.addEventListener("click", openModal);

    modal.querySelectorAll("[data-close-customer-create]").forEach((trigger) => {
      trigger.addEventListener("click", closeModal);
    });

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape" && modal.classList.contains("is-open")) {
        closeModal();
      }
    });

    form.addEventListener("submit", async (event) => {
      event.preventDefault();
      clearAlert();
      setLoading(true);

      try {
        const response = await fetch(form.action, {
          method: "POST",
          body: new FormData(form),
          headers: {
            "Accept": "application/json",
            "X-Requested-With": "XMLHttpRequest"
          }
        });
        const data = await parseJsonResponse(response);

        if (!response.ok || !data || data.success === false) {
          showAlert(resolveErrors(data));
          return;
        }

        const customer = data.customer || data.Customer;
        if (customer) {
          const row = createCustomerRow(customer);
          tableBody.insertBefore(row, emptyRow || null);
        }

        updateStats(data.stats || data.Stats);
        form.reset();
        closeModal();
        applyCustomerSearch();
      } catch (error) {
        showAlert(["Không thể tạo khách hàng lúc này. Vui lòng thử lại."]);
      } finally {
        setLoading(false);
      }
    });

    function setLoading(isLoading) {
      if (!submitButton) {
        return;
      }

      submitButton.disabled = isLoading;
      submitButton.classList.toggle("is-loading", isLoading);
    }

    function clearAlert() {
      if (!alertBox) {
        return;
      }

      alertBox.hidden = true;
      alertBox.textContent = "";
    }

    function showAlert(messages) {
      if (!alertBox) {
        return;
      }

      alertBox.hidden = false;
      alertBox.textContent = messages.join(" ");
    }
  }

  function setupCustomerDetailModal() {
    const modal = document.getElementById("customer-detail-modal");

    if (!modal) {
      return;
    }

    const closeTriggers = Array.from(modal.querySelectorAll("[data-close-customer-detail]"));
    const loading = modal.querySelector("[data-customer-detail-loading]");
    const error = modal.querySelector("[data-customer-detail-error]");
    const content = modal.querySelector("[data-customer-detail-content]");
    const closeButton = modal.querySelector(".customer-detail-close");
    const tier = modal.querySelector("[data-customer-detail-tier]");
    const ordersTarget = modal.querySelector("[data-customer-detail-orders]");

    document.addEventListener("click", (event) => {
      const trigger = event.target.closest("[data-customer-detail-url]");

      if (!trigger) {
        return;
      }

      event.preventDefault();

      const url = trigger.getAttribute("data-customer-detail-url");
      if (!url) {
        return;
      }

      openModal();
      loadCustomerDetail(url);
    });

    closeTriggers.forEach((trigger) => {
      trigger.addEventListener("click", closeModal);
    });

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape" && !modal.hidden) {
        closeModal();
      }
    });

    function openModal() {
      modal.hidden = false;
      modal.setAttribute("aria-hidden", "false");
      document.body.classList.add("is-customer-detail-open");
      setModalState("loading");
      window.setTimeout(() => closeButton?.focus(), 0);
    }

    function closeModal() {
      modal.hidden = true;
      modal.setAttribute("aria-hidden", "true");
      document.body.classList.remove("is-customer-detail-open");
    }

    function setModalState(state) {
      if (loading) {
        loading.hidden = state !== "loading";
      }

      if (error) {
        error.hidden = state !== "error";
      }

      if (content) {
        content.hidden = state !== "content";
      }
    }

    async function loadCustomerDetail(url) {
      try {
        const response = await fetch(url, {
          headers: {
            "Accept": "application/json",
            "X-Requested-With": "XMLHttpRequest"
          }
        });

        const data = await parseJsonResponse(response);

        if (data.redirectUrl) {
          throw new Error(data.message || "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
        }

        if (!response.ok) {
          throw new Error(data.message || "Không tải được chi tiết khách hàng.");
        }

        renderCustomerDetail(data);
        setModalState("content");
      } catch (err) {
        setDetailError(err.message || "Không tải được thông tin khách hàng. Vui lòng thử lại sau ít phút.");
        setModalState("error");
      }
    }

    function renderCustomerDetail(data) {
      setText("[data-customer-detail-initial]", data.initial || "K");
      setText("[data-customer-detail-code]", data.customerCode || "KH000");
      setText("[data-customer-detail-name]", data.customerName || "Khách hàng");
      setText("[data-customer-detail-order-count]", data.orderCount || "0");
      setText("[data-customer-detail-total]", data.totalSpent || "0đ");
      setText("[data-customer-detail-last]", data.lastPurchaseLabel || "Chưa có đơn hàng");
      setText("[data-customer-detail-phone]", data.phone || "Chưa có SĐT");
      setText("[data-customer-detail-email]", data.email || "Chưa có email");
      setText("[data-customer-detail-address]", data.address || "Chưa cập nhật địa chỉ");
      setText("[data-customer-detail-account]", data.account || "Chưa có tài khoản");
      setText("[data-customer-detail-password]", data.password || "••••••••");
      setText("[data-customer-detail-photo]", data.photo || "Chưa có ảnh đại diện");

      if (tier) {
        tier.textContent = data.tierLabel || "Mới";
        tier.className = `tier-pill ${data.tierCssClass || "is-new"}`;
      }

      const recentOrders = Array.isArray(data.recentOrders) ? data.recentOrders : [];
      setText("[data-customer-detail-recent-count]", recentOrders.length ? `${recentOrders.length} đơn gần nhất` : "Chưa có đơn");
      renderRecentOrders(recentOrders);
    }

    function renderRecentOrders(orders) {
      if (!ordersTarget) {
        return;
      }

      ordersTarget.textContent = "";

      if (!orders.length) {
        const empty = document.createElement("p");
        empty.className = "customer-detail-orders__empty";
        empty.textContent = "Khách hàng chưa có đơn hàng.";
        ordersTarget.appendChild(empty);
        return;
      }

      orders.forEach((order) => {
        const item = document.createElement("div");
        item.className = "customer-detail-order";

        const main = document.createElement("div");
        const code = document.createElement("strong");
        const meta = document.createElement("span");
        code.textContent = order.orderCode || "DH000";
        meta.textContent = `${order.orderTime || "Chưa có thời gian"} · ${order.tableLabel || "Mang về"}`;
        main.append(code, meta);

        const amount = document.createElement("div");
        const total = document.createElement("strong");
        const payment = document.createElement("span");
        total.textContent = order.totalAmount || "0đ";
        payment.textContent = order.paymentLabel || "Chưa thanh toán";
        amount.append(total, payment);

        item.append(main, amount);
        ordersTarget.appendChild(item);
      });
    }

    function setText(selector, value) {
      const target = modal.querySelector(selector);

      if (target) {
        target.textContent = value;
      }
    }

    function setDetailError(message) {
      if (!error) {
        return;
      }

      const title = error.querySelector("strong") || error.querySelector("h3");
      const copy = error.querySelector("span") || error.querySelector("p");

      if (title) {
        title.textContent = "Không tải được thông tin khách hàng";
      }

      if (copy) {
        copy.textContent = message;
      } else {
        error.textContent = message;
      }
    }
  }

  function createCustomerRow(customer) {
    const row = document.createElement("tr");
    row.setAttribute("data-customer-row", "");
    row.setAttribute("data-search", customer.searchText || "");

    const profileCell = createCell("Khách Hàng");
    const profile = document.createElement("div");
    profile.className = "customer-profile";
    const avatar = document.createElement("span");
    avatar.className = "customer-avatar";
    avatar.textContent = customer.initial || "K";
    const profileCopy = document.createElement("span");
    profileCopy.className = "customer-profile__copy";
    const name = document.createElement("strong");
    name.textContent = customer.customerName || "Khách lẻ";
    const lastPurchase = document.createElement("small");
    lastPurchase.textContent = customer.lastPurchaseLabel || "Chưa có đơn hàng";
    profileCopy.append(name, lastPurchase);
    profile.append(avatar, profileCopy);
    profileCell.append(profile);

    const contactCell = createCell("Liên Hệ");
    const contact = document.createElement("div");
    contact.className = "customer-contact";
    contact.append(
      createIconText(phoneIcon(), customer.phone || "Chưa có SĐT"),
      createIconText(emailIcon(), customer.email || "Chưa có email")
    );
    contactCell.append(contact);

    const addressCell = createCell("Địa Chỉ");
    const address = createIconText(locationIcon(), customer.address || "Chưa cập nhật địa chỉ");
    address.className = "customer-address";
    addressCell.append(address);

    const orderCell = createCell("Đơn Hàng", "is-center");
    const orderCount = document.createElement("strong");
    orderCount.className = "customer-order-count";
    orderCount.textContent = customer.orderCount || "0";
    orderCell.append(orderCount);

    const totalCell = createCell("Tổng Chi Tiêu", "is-right");
    const total = document.createElement("strong");
    total.className = "customer-total";
    total.textContent = customer.totalSpent || "0đ";
    totalCell.append(total);

    const tierCell = createCell("Hạng");
    const tier = document.createElement("span");
    tier.className = `tier-pill ${customer.tierCssClass || "is-new"}`;
    tier.textContent = customer.tierLabel || "Mới";
    tierCell.append(tier);

    const detailCell = createCell("Chi Tiết", "is-right");
    const detail = document.createElement("button");
    detail.className = "customer-detail-button";
    detail.type = "button";
    detail.setAttribute("data-customer-detail-url", customer.detailUrl || "#");
    detail.innerHTML = `${eyeIcon()}<span>Chi Tiết</span>`;
    detailCell.append(detail);

    row.append(profileCell, contactCell, addressCell, orderCell, totalCell, tierCell, detailCell);
    return row;
  }

  function createCell(label, className) {
    const cell = document.createElement("td");
    cell.setAttribute("data-label", label);

    if (className) {
      cell.className = className;
    }

    return cell;
  }

  function createIconText(icon, text) {
    const wrapper = document.createElement("span");
    wrapper.innerHTML = icon;
    wrapper.append(document.createTextNode(text));
    return wrapper;
  }

  function updateStats(stats) {
    if (!stats) {
      return;
    }

    setStat("total", stats.totalCustomers || stats.TotalCustomers);
    setStat("vip", stats.vipCustomers || stats.VipCustomers);
    setStat("regular", stats.regularCustomers || stats.RegularCustomers);
    setStat("new", stats.newCustomers || stats.NewCustomers);
  }

  function setStat(name, value) {
    const target = document.querySelector(`[data-customer-stat="${name}"]`);

    if (target && value !== undefined && value !== null) {
      target.textContent = value;
    }
  }

  function resolveErrors(data) {
    const errors = data && (data.errors || data.Errors);

    if (Array.isArray(errors) && errors.length) {
      return errors;
    }

    if (data && data.message) {
      return [data.message];
    }

    return ["Thông tin chưa hợp lệ. Vui lòng kiểm tra lại các trường bắt buộc."];
  }

  async function parseJsonResponse(response) {
    const contentType = response.headers.get("content-type") || "";
    const text = await response.text();

    if (!text) {
      return {};
    }

    if (contentType.toLowerCase().includes("application/json")) {
      try {
        return JSON.parse(text);
      } catch {
        return { message: "Phản hồi từ server không hợp lệ. Vui lòng thử lại." };
      }
    }

    if (response.redirected || response.url.includes("/Admin/Login")) {
      return {
        message: "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
        redirectUrl: response.url || "/Admin/Login"
      };
    }

    const normalizedText = text.trim().toLowerCase();

    if (normalizedText.startsWith("<!doctype") || normalizedText.startsWith("<html")) {
      return { message: "Server đang trả về trang HTML thay vì dữ liệu khách hàng. Vui lòng tải lại trang rồi thử lại." };
    }

    return { message: text.trim() || "Không thể đọc phản hồi từ server." };
  }

  function phoneIcon() {
    return '<svg viewBox="0 0 24 24" fill="none"><path d="M22 16.9v3a2 2 0 0 1-2.2 2 19.8 19.8 0 0 1-8.6-3.1 19.4 19.4 0 0 1-6-6A19.8 19.8 0 0 1 2.1 4.2 2 2 0 0 1 4.1 2h3a2 2 0 0 1 2 1.7c.1.9.3 1.8.6 2.6a2 2 0 0 1-.5 2.1L8 9.6a16 16 0 0 0 6.4 6.4l1.2-1.2a2 2 0 0 1 2.1-.5c.8.3 1.7.5 2.6.6a2 2 0 0 1 1.7 2Z" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"></path></svg>';
  }

  function emailIcon() {
    return '<svg viewBox="0 0 24 24" fill="none"><rect x="3" y="5" width="18" height="14" rx="2" stroke="currentColor" stroke-width="1.8"></rect><path d="m4 7 8 6 8-6" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"></path></svg>';
  }

  function locationIcon() {
    return '<svg viewBox="0 0 24 24" fill="none"><path d="M12 21s7-5.1 7-11a7 7 0 1 0-14 0c0 5.9 7 11 7 11Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round"></path><circle cx="12" cy="10" r="2.3" stroke="currentColor" stroke-width="1.8"></circle></svg>';
  }

  function eyeIcon() {
    return '<svg viewBox="0 0 24 24" fill="none"><path d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6-10-6-10-6Z" stroke="currentColor" stroke-width="2"></path><circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2"></circle></svg>';
  }

  function normalizeValue(value) {
    return String(value || "")
      .trim()
      .toLowerCase();
  }
})();
