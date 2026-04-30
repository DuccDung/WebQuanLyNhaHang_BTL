(function () {
  const mobileNavTrigger = document.getElementById("mobile-nav-trigger");
  const mobileNavBackdrop = document.getElementById("mobile-nav-backdrop");
  const dashboardSidebar = document.getElementById("dashboard-sidebar");

  setupMobileNav();
  setupAccountMenu();
  setupSalesMenu();
  setupOrderFilters();
  setupExport();
  setupOrderDetailModal();

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

  function setupOrderFilters() {
    const rows = Array.from(document.querySelectorAll("[data-order-row]"));
    const searchInput = document.querySelector("[data-order-search]");
    const statusFilter = document.querySelector("[data-order-status-filter]");
    const dateFilter = document.querySelector("[data-order-date-filter]");
    const dateLabel = dateFilter?.closest(".order-date-filter");
    const dateLabelText = dateLabel?.querySelector("span");
    const emptyRow = document.querySelector("[data-empty-row]");

    if (!rows.length) {
      return;
    }

    const applyFilters = () => {
      const keyword = normalizeValue(searchInput && "value" in searchInput ? searchInput.value : "");
      const status = normalizeValue(statusFilter && "value" in statusFilter ? statusFilter.value : "");
      const date = dateFilter && "value" in dateFilter ? dateFilter.value : "";
      let visibleCount = 0;

      rows.forEach((row) => {
        const rowSearch = normalizeValue(row.getAttribute("data-search"));
        const rowStatus = normalizeValue(row.getAttribute("data-status"));
        const rowDate = row.getAttribute("data-date") || "";
        const isVisible = (!keyword || rowSearch.includes(keyword))
          && (!status || rowStatus === status)
          && (!date || rowDate === date);

        row.hidden = !isVisible;
        if (isVisible) {
          visibleCount += 1;
        }
      });

      if (emptyRow) {
        emptyRow.hidden = visibleCount !== 0;
      }
    };

    if (searchInput) {
      searchInput.addEventListener("input", applyFilters);
    }

    if (statusFilter) {
      statusFilter.addEventListener("change", applyFilters);
    }

    if (dateFilter) {
      dateFilter.addEventListener("change", () => {
        if (dateLabelText) {
          dateLabelText.textContent = dateFilter.value ? formatDateLabel(dateFilter.value) : "Chọn Ngày";
        }

        dateLabel?.classList.toggle("is-active", Boolean(dateFilter.value));
        applyFilters();
      });
    }

    applyFilters();
  }

  function setupExport() {
    const trigger = document.querySelector("[data-export-orders]");

    if (!trigger) {
      return;
    }

    trigger.addEventListener("click", () => {
      const rows = Array.from(document.querySelectorAll("[data-order-row]")).filter((row) => !row.hidden);
      const csvRows = [
        ["Mã ĐH", "Khách hàng", "Bàn", "Thời gian", "Tổng tiền", "Thanh toán", "Trạng thái"]
      ];

      rows.forEach((row) => {
        csvRows.push(Array.from(row.querySelectorAll("td")).slice(0, 7).map((cell) => cleanCsvValue(cell.textContent)));
      });

      const csvContent = csvRows.map((row) => row.map(escapeCsvValue).join(",")).join("\r\n");
      const blob = new Blob(["\uFEFF" + csvContent], { type: "text/csv;charset=utf-8;" });
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");

      link.href = url;
      link.download = `don-hang-${new Date().toISOString().slice(0, 10)}.csv`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    });
  }

  function setupOrderDetailModal() {
    const modal = document.getElementById("order-detail-modal");
    const triggers = Array.from(document.querySelectorAll("[data-order-detail-url]"));

    if (!modal || !triggers.length) {
      return;
    }

    const closeTriggers = Array.from(modal.querySelectorAll("[data-order-modal-close]"));
    const loading = modal.querySelector("[data-detail-loading]");
    const error = modal.querySelector("[data-detail-error]");
    const content = modal.querySelector("[data-detail-content]");
    const status = modal.querySelector("[data-detail-status]");
    const statusActions = modal.querySelector("[data-detail-status-actions]");
    const itemsBody = modal.querySelector("[data-detail-items]");
    const firstCloseButton = modal.querySelector(".order-detail-close");

    triggers.forEach((trigger) => {
      trigger.addEventListener("click", () => {
        const url = trigger.getAttribute("data-order-detail-url");

        if (!url) {
          return;
        }

        openModal();
        loadOrderDetail(url);
      });
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
      document.body.classList.add("is-order-detail-open");
      setModalState("loading");
      window.setTimeout(() => firstCloseButton?.focus(), 0);
    }

    function closeModal() {
      modal.hidden = true;
      modal.setAttribute("aria-hidden", "true");
      document.body.classList.remove("is-order-detail-open");
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

    async function loadOrderDetail(url) {
      try {
        const response = await fetch(url, {
          headers: { "Accept": "application/json" }
        });

        if (!response.ok) {
          throw new Error("Không tải được chi tiết đơn hàng.");
        }

        renderOrderDetail(await response.json());
        setModalState("content");
      } catch (err) {
        setModalState("error");
      }
    }

    function renderOrderDetail(data) {
      setText("[data-detail-code]", data.orderCode || "Chi tiết đơn hàng");
      setText("[data-detail-created-time]", data.createdTime || "Chưa có thời gian");
      setText("[data-detail-customer]", data.customerName || "Khách lẻ");
      setText("[data-detail-phone]", data.customerPhone || "Chưa có SĐT");
      setText("[data-detail-table]", data.tableLabel || "Mang về");
      setText("[data-detail-address]", data.customerAddress || "Chưa có địa chỉ");
      setText("[data-detail-payment]", data.paymentLabel || "Chưa thanh toán");
      setText("[data-detail-promotion]", data.promotionName || "Không áp dụng");
      setText("[data-detail-employee]", data.employeeName || "Chưa phân công");
      setText("[data-detail-time-range]", buildTimeRange(data.timeIn, data.timeOut));
      setText("[data-detail-total]", data.totalAmount || "0đ");

      if (status) {
        status.textContent = data.statusLabel || "Chờ xử lý";
        status.className = `order-detail-status ${data.statusCssClass || "is-pending"}`;
      }

      const items = Array.isArray(data.items) ? data.items : [];
      setText("[data-detail-item-count]", `${items.length} món`);
      renderLineItems(items);
      renderStatusActions(data);
    }

    function renderLineItems(items) {
      if (!itemsBody) {
        return;
      }

      itemsBody.textContent = "";

      if (!items.length) {
        const row = document.createElement("tr");
        const cell = document.createElement("td");

        cell.colSpan = 4;
        cell.textContent = "Đơn hàng chưa có sản phẩm.";
        row.appendChild(cell);
        itemsBody.appendChild(row);
        return;
      }

      items.forEach((item) => {
        const row = document.createElement("tr");
        const productCell = document.createElement("td");
        const product = document.createElement("div");
        const productName = document.createElement("strong");
        const productNote = document.createElement("small");

        product.className = "order-detail-product";
        productName.textContent = item.productName || "Sản phẩm chưa đặt tên";
        productNote.textContent = item.note || "Không có ghi chú";
        product.append(productName, productNote);
        productCell.appendChild(product);

        row.append(
          productCell,
          createTextCell(item.quantity || "0"),
          createTextCell(item.unitPrice || "0đ"),
          createTextCell(item.total || "0đ")
        );

        itemsBody.appendChild(row);
      });
    }

    function renderStatusActions(data) {
      if (!statusActions) {
        return;
      }

      const options = Array.isArray(data.statusOptions) ? data.statusOptions : [];
      statusActions.textContent = "";
      statusActions.hidden = !data.updateStatusUrl || !options.length;

      if (statusActions.hidden) {
        return;
      }

      options.forEach((option) => {
        const button = document.createElement("button");
        button.type = "button";
        button.textContent = option.label || option.value;
        button.dataset.deliveryStatus = option.value || "";
        button.className = option.value === data.statusKey ? "is-active" : "";

        button.addEventListener("click", async () => {
          const nextStatus = button.dataset.deliveryStatus;
          if (!nextStatus) {
            return;
          }

          button.disabled = true;
          try {
            const formData = new FormData();
            formData.append("status", nextStatus);
            const response = await fetch(data.updateStatusUrl, {
              method: "POST",
              body: formData,
              headers: { "Accept": "application/json" }
            });

            if (!response.ok) {
              throw new Error("Khong cap nhat duoc trang thai.");
            }

            const result = await response.json();
            data.statusKey = result.statusKey;
            data.statusLabel = result.statusLabel;
            data.statusCssClass = result.statusCssClass;

            if (status) {
              status.textContent = result.statusLabel;
              status.className = `order-detail-status ${result.statusCssClass || "is-pending"}`;
            }

            renderStatusActions(data);
          } catch (err) {
            button.disabled = false;
          }
        });

        statusActions.appendChild(button);
      });
    }

    function createTextCell(value) {
      const cell = document.createElement("td");
      cell.textContent = value;
      return cell;
    }

    function setText(selector, value) {
      const element = modal.querySelector(selector);

      if (element) {
        element.textContent = value;
      }
    }

    function buildTimeRange(timeIn, timeOut) {
      const hasTimeIn = timeIn && timeIn !== "Chưa có thời gian";
      const hasTimeOut = timeOut && timeOut !== "Chưa có thời gian";

      if (hasTimeIn && hasTimeOut) {
        return `${timeIn} - ${timeOut}`;
      }

      if (hasTimeIn) {
        return `Vào lúc ${timeIn}`;
      }

      if (hasTimeOut) {
        return `Ra lúc ${timeOut}`;
      }

      return "Chưa có thời gian";
    }
  }

  function normalizeValue(value) {
    return String(value || "").trim().toLowerCase();
  }

  function formatDateLabel(value) {
    const [year, month, day] = String(value).split("-");

    if (!year || !month || !day) {
      return "Chọn Ngày";
    }

    return `${day}/${month}/${year}`;
  }

  function cleanCsvValue(value) {
    return String(value || "").replace(/\s+/g, " ").trim();
  }

  function escapeCsvValue(value) {
    const safeValue = String(value || "");

    if (safeValue.includes(",") || safeValue.includes("\"") || safeValue.includes("\n")) {
      return `"${safeValue.replace(/"/g, "\"\"")}"`;
    }

    return safeValue;
  }
})();
