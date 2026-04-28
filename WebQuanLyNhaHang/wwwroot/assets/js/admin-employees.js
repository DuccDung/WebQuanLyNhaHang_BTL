(function () {
  const mobileNavTrigger = document.getElementById("mobile-nav-trigger");
  const mobileNavBackdrop = document.getElementById("mobile-nav-backdrop");
  const dashboardSidebar = document.getElementById("dashboard-sidebar");

  setupMobileNav();
  setupAccountMenu();
  setupSalesMenu();
  setupEmployeeFilters();
  setupColumnMenu();
  setupExport();
  setupEmployeeModals();

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

  function setupEmployeeFilters() {
    const rows = Array.from(document.querySelectorAll("[data-employee-row]"));
    const searchInput = document.querySelector("[data-employee-search]");
    const roleFilter = document.querySelector("[data-employee-role-filter]");
    const emptyRow = document.querySelector("[data-empty-row]");

    if (!rows.length) {
      return;
    }

    const applyFilters = () => {
      const keyword = normalizeValue(searchInput && "value" in searchInput ? searchInput.value : "");
      const role = normalizeValue(roleFilter && "value" in roleFilter ? roleFilter.value : "");
      let visibleCount = 0;

      rows.forEach((row) => {
        const rowSearch = normalizeValue(row.getAttribute("data-search"));
        const rowRole = normalizeValue(row.getAttribute("data-role"));
        const isVisible = (!keyword || rowSearch.includes(keyword))
          && (!role || rowRole === role);

        row.hidden = !isVisible;
        if (isVisible) {
          visibleCount += 1;
        }
      });

      if (emptyRow) {
        emptyRow.hidden = visibleCount !== 0;
      }
    };

    searchInput?.addEventListener("input", applyFilters);
    roleFilter?.addEventListener("change", applyFilters);
    applyFilters();
  }

  function setupColumnMenu() {
    const trigger = document.querySelector("[data-column-menu-trigger]");
    const menu = document.querySelector("[data-column-menu]");
    const table = document.querySelector("[data-employees-table]");

    if (!trigger || !menu || !table) {
      return;
    }

    trigger.addEventListener("click", () => {
      const isOpen = !menu.hidden;
      menu.hidden = isOpen;
      trigger.setAttribute("aria-expanded", String(!isOpen));
    });

    menu.querySelectorAll("[data-column-toggle]").forEach((toggle) => {
      toggle.addEventListener("change", () => {
        const column = toggle.getAttribute("data-column-toggle");
        table.classList.toggle(`is-hidden-${column}`, !toggle.checked);
      });
    });

    document.addEventListener("click", (event) => {
      if (event.target.closest(".employee-column-menu")) {
        return;
      }

      menu.hidden = true;
      trigger.setAttribute("aria-expanded", "false");
    });
  }

  function setupExport() {
    const trigger = document.querySelector("[data-export-employees]");

    if (!trigger) {
      return;
    }

    trigger.addEventListener("click", () => {
      const rows = Array.from(document.querySelectorAll("[data-employee-row]")).filter((row) => !row.hidden);
      const csvRows = [
        ["Nhân viên", "Chức vụ", "Liên hệ", "Ngày vào làm", "Lương", "Tên tài khoản", "Mật khẩu", "Trạng thái"]
      ];

      rows.forEach((row) => {
        csvRows.push(Array.from(row.querySelectorAll("td")).slice(0, 8).map((cell) => cleanCsvValue(cell.textContent)));
      });

      const csvContent = csvRows.map((row) => row.map(escapeCsvValue).join(",")).join("\r\n");
      const blob = new Blob(["\uFEFF" + csvContent], { type: "text/csv;charset=utf-8;" });
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");

      link.href = url;
      link.download = `nhan-vien-${new Date().toISOString().slice(0, 10)}.csv`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    });
  }

  function setupEmployeeModals() {
    const modal = document.getElementById("employee-modal");
    const triggers = Array.from(document.querySelectorAll("[data-employee-action]"));

    if (!modal || !triggers.length) {
      return;
    }

    const closeTriggers = Array.from(modal.querySelectorAll("[data-employee-modal-close]"));
    const loading = modal.querySelector("[data-modal-loading]");
    const error = modal.querySelector("[data-modal-error]");
    const viewPanel = modal.querySelector("[data-modal-view]");
    const editForm = modal.querySelector("[data-modal-edit]");
    const deletePanel = modal.querySelector("[data-modal-delete]");
    const deleteSubmit = modal.querySelector("[data-delete-submit]");
    const firstCloseButton = modal.querySelector(".employee-modal__close");
    let activeRow = null;
    let activeUpdateUrl = "";
    let activeDeleteUrl = "";

    triggers.forEach((trigger) => {
      trigger.addEventListener("click", async () => {
        const action = trigger.getAttribute("data-employee-action");
        const detailUrl = trigger.getAttribute("data-employee-detail-url");

        activeRow = trigger.closest("[data-employee-row]");
        activeUpdateUrl = trigger.getAttribute("data-employee-update-url") || "";
        activeDeleteUrl = trigger.getAttribute("data-employee-delete-url") || "";

        if (!action || !detailUrl) {
          return;
        }

        openModal(action);
        await loadEmployeeDetail(detailUrl, action);
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

    editForm?.addEventListener("submit", async (event) => {
      event.preventDefault();

      if (!activeUpdateUrl) {
        return;
      }

      setBusy(editForm.querySelector("[data-edit-submit]"), true, "Đang lưu...");
      clearError();

      try {
        const response = await fetch(activeUpdateUrl, {
          method: "POST",
          body: new FormData(editForm),
          headers: { "Accept": "application/json" }
        });

        const payload = await parseJsonResponse(response);

        if (!response.ok) {
          throw new Error(payload.message || "Không lưu được thay đổi.");
        }

        if (activeRow && payload.row) {
          updateEmployeeRow(activeRow, payload.row);
          applyCurrentFiltersToAll();
          refreshEmployeeStats();
        }

        closeModal();
      } catch (err) {
        showError(err.message || "Không lưu được thay đổi. Vui lòng thử lại.");
      } finally {
        setBusy(editForm.querySelector("[data-edit-submit]"), false);
      }
    });

    deleteSubmit?.addEventListener("click", async () => {
      if (!activeDeleteUrl) {
        return;
      }

      setBusy(deleteSubmit, true, "Đang xóa...");
      clearError();

      try {
        const response = await fetch(activeDeleteUrl, {
          method: "POST",
          headers: { "Accept": "application/json" }
        });

        const payload = await parseJsonResponse(response);

        if (!response.ok) {
          throw new Error(payload.message || "Không xóa được nhân viên.");
        }

        activeRow?.remove();
        applyCurrentFiltersToAll();
        refreshEmployeeStats();
        closeModal();
      } catch (err) {
        showError(err.message || "Không xóa được nhân viên. Vui lòng thử lại.");
      } finally {
        setBusy(deleteSubmit, false);
      }
    });

    function openModal(mode) {
      modal.hidden = false;
      modal.setAttribute("aria-hidden", "false");
      document.body.classList.add("is-employee-modal-open");
      setModalState("loading");
      setModalMode(mode);
      window.setTimeout(() => firstCloseButton?.focus(), 0);
    }

    function closeModal() {
      modal.hidden = true;
      modal.setAttribute("aria-hidden", "true");
      document.body.classList.remove("is-employee-modal-open");
      clearError();
    }

    async function loadEmployeeDetail(url, mode) {
      try {
        const response = await fetch(url, {
          headers: { "Accept": "application/json" }
        });

        const data = await parseJsonResponse(response);

        if (!response.ok) {
          throw new Error(data.message || "Không tải được thông tin nhân viên.");
        }

        renderHeader(data, mode);

        if (mode === "view") {
          renderView(data);
          setModalState("view");
          return;
        }

        if (mode === "edit") {
          renderEdit(data);
          setModalState("edit");
          return;
        }

        renderDelete(data);
        setModalState("delete");
      } catch (err) {
        showError(err.message || "Không tải được thông tin nhân viên.");
        setModalState("error");
      }
    }

    function setModalState(state) {
      if (loading) {
        loading.hidden = state !== "loading";
      }

      if (viewPanel) {
        viewPanel.hidden = state !== "view";
      }

      if (editForm) {
        editForm.hidden = state !== "edit";
      }

      if (deletePanel) {
        deletePanel.hidden = state !== "delete";
      }

      if (error) {
        error.hidden = state !== "error" || !error.textContent.trim();
      }
    }

    function setModalMode(mode) {
      const label = mode === "edit" ? "Chỉnh sửa nhân viên" : mode === "delete" ? "Xác nhận xóa" : "Chi tiết nhân viên";
      setText("[data-modal-mode]", label);
    }

    function renderHeader(data, mode) {
      setText("[data-modal-initial]", data.initial || "NV");
      setText("[data-modal-title]", mode === "delete" ? `Xóa ${data.employeeName || "nhân viên"}` : data.employeeName || "Thông tin nhân viên");
      setText("[data-modal-subtitle]", `${data.roleLabel || "Nhân Viên"} · ${data.account || "Chưa có tài khoản"}`);
      setModalMode(mode);
    }

    function renderView(data) {
      setText("[data-view-role]", data.roleLabel || "Nhân Viên");
      setText("[data-view-status]", data.statusLabel || "Đang Làm");
      setText("[data-view-date]", data.startDateLabel || "Chưa cập nhật");
      setText("[data-view-salary]", data.salaryLabel || "0đ");
      setText("[data-view-phone]", data.phone || "Chưa có SĐT");
      setText("[data-view-email]", data.email || "Chưa có email");
      setText("[data-view-address]", data.address || "Chưa cập nhật địa chỉ");
      setText("[data-view-account]", data.account || "Chưa có tài khoản");
    }

    function renderEdit(data) {
      setInput("[data-edit-id]", data.employeeId || "");
      setInput("[data-edit-name]", data.employeeName || "");
      setInput("[data-edit-date]", data.startDate || "");
      setInput("[data-edit-salary]", data.salaryCoefficient ?? 0);
      setInput("[data-edit-account]", data.account || "");
      setInput("[data-edit-password]", data.password || "");
      setInput("[data-edit-address]", data.address || "");
      renderRoleOptions(data.roles || [], data.roleId);
    }

    function renderDelete(data) {
      setText("[data-modal-subtitle]", `Bạn đang thao tác với tài khoản ${data.account || "chưa có tài khoản"}.`);
    }

    function renderRoleOptions(roles, selectedRoleId) {
      const select = modal.querySelector("[data-edit-role]");

      if (!select) {
        return;
      }

      select.textContent = "";

      roles.forEach((role) => {
        const option = document.createElement("option");
        option.value = role.id;
        option.textContent = role.label;
        option.selected = String(role.id) === String(selectedRoleId || "");
        select.appendChild(option);
      });
    }

    function updateEmployeeRow(row, data) {
      row.setAttribute("data-search", data.searchText || "");
      row.setAttribute("data-role", data.roleKey || "staff");

      setRowText(row, "[data-employee-initial]", data.initial || "N");
      setRowText(row, "[data-employee-name]", data.employeeName || "Nhân viên");
      setRowText(row, "[data-employee-address]", data.address || "Chưa cập nhật địa chỉ");
      setRowText(row, "[data-employee-phone]", data.phone || "Chưa có SĐT");
      setRowText(row, "[data-employee-email]", data.email || "Chưa có email");
      setRowText(row, '[data-column="date"]', data.startDateLabel || "Chưa cập nhật");
      setRowText(row, ".employee-salary", data.salaryLabel || "0đ");
      setRowText(row, '[data-column="account"]', data.account || "");
      setRowText(row, ".employee-password", data.passwordMask || "•••");

      const role = row.querySelector("[data-employee-role]");
      if (role) {
        role.className = `role-pill ${data.roleCssClass || "is-staff"}`;
        role.textContent = data.roleLabel || "Nhân Viên";
      }

      const status = row.querySelector("[data-employee-status]");
      if (status) {
        status.className = `employee-status ${data.statusCssClass || "is-active"}`;
        status.textContent = data.statusLabel || "Đang Làm";
      }
    }

    function applyCurrentFiltersToAll() {
      const rows = Array.from(document.querySelectorAll("[data-employee-row]"));
      const keyword = normalizeValue(document.querySelector("[data-employee-search]")?.value || "");
      const role = normalizeValue(document.querySelector("[data-employee-role-filter]")?.value || "");
      let visibleCount = 0;

      rows.forEach((row) => {
        const rowSearch = normalizeValue(row.getAttribute("data-search"));
        const rowRole = normalizeValue(row.getAttribute("data-role"));
        const isVisible = (!keyword || rowSearch.includes(keyword)) && (!role || rowRole === role);

        row.hidden = !isVisible;

        if (isVisible) {
          visibleCount += 1;
        }
      });

      const emptyRow = document.querySelector("[data-empty-row]");
      if (emptyRow) {
        emptyRow.hidden = visibleCount !== 0;
      }
    }

    function refreshEmployeeStats() {
      const rows = Array.from(document.querySelectorAll("[data-employee-row]"));
      const stats = Array.from(document.querySelectorAll(".employee-stat-card strong"));
      const formatter = new Intl.NumberFormat("vi-VN");

      const counts = [
        rows.length,
        rows.filter((row) => row.getAttribute("data-role") === "manager").length,
        rows.filter((row) => row.getAttribute("data-role") === "kitchen").length,
        rows.filter((row) => row.getAttribute("data-role") === "service").length
      ];

      stats.forEach((stat, index) => {
        stat.textContent = formatter.format(counts[index] || 0);
      });
    }

    function setText(selector, value) {
      const element = modal.querySelector(selector);
      if (element) {
        element.textContent = value;
      }
    }

    function setInput(selector, value) {
      const element = modal.querySelector(selector);
      if (element) {
        element.value = value;
      }
    }

    function setRowText(row, selector, value) {
      const element = row.querySelector(selector);
      if (element) {
        element.textContent = value;
      }
    }

    function showError(message) {
      if (!error) {
        return;
      }

      error.textContent = message;
      error.hidden = false;
    }

    function clearError() {
      if (!error) {
        return;
      }

      error.textContent = "";
      error.hidden = true;
    }

    function setBusy(button, isBusy, busyText) {
      if (!button) {
        return;
      }

      if (isBusy) {
        button.dataset.originalText = button.textContent;
        button.textContent = busyText || "Đang xử lý...";
        button.disabled = true;
        return;
      }

      button.textContent = button.dataset.originalText || button.textContent;
      button.disabled = false;
    }

    async function parseJsonResponse(response) {
      const text = await response.text();
      return text ? JSON.parse(text) : {};
    }
  }

  function normalizeValue(value) {
    return String(value || "").trim().toLowerCase();
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
