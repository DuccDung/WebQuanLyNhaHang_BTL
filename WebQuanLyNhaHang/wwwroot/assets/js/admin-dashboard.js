(function () {
  const payloadNode = document.getElementById("dashboard-payload");
  if (!payloadNode) {
    return;
  }

  let payload = {};

  try {
    payload = JSON.parse(payloadNode.textContent || "{}");
  } catch (error) {
    payload = {};
  }

  const revenueChart = document.querySelector('[data-chart="revenue"]');
  const orderChart = document.querySelector('[data-chart="orders"]');
  const comparisonChart = document.querySelector('[data-chart="comparison"]');

  const revenueSummary = document.querySelector('[data-summary="revenue"]');
  const orderSummary = document.querySelector('[data-summary="orders"]');
  const comparisonSummary = document.querySelector('[data-summary="comparison"]');
  const topProductsBody = document.querySelector("[data-top-products]");
  const mobileNavTrigger = document.getElementById("mobile-nav-trigger");
  const mobileNavBackdrop = document.getElementById("mobile-nav-backdrop");
  const dashboardSidebar = document.getElementById("dashboard-sidebar");

  setupAccountMenu();
  setupMobileNav();
  setupSalesMenu();
  setupRevenuePanel();
  setupOrdersPanel();
  setupComparisonPanel();
  setupProductsPanel();
  normalizePanelTitles();

  bindSelect('[data-series-select="revenue"]', (period) => {
    renderRevenue(period);
  });

  bindSelect('[data-series-select="orders"]', (period) => {
    renderOrders(period);
  });

  bindSelect('[data-series-select="comparison"]', (period) => {
    renderComparison(period);
  });

  bindSelect('[data-series-select="products"]', (period) => {
    renderTopProducts(period);
  });

  function bindSelect(selector, callback) {
    const select = document.querySelector(selector);
    if (!select) {
      return;
    }

    callback(select.value);
    select.addEventListener("change", () => {
      callback(select.value);
    });
  }

  function renderRevenue(period) {
    const series = payload.revenueSeries && payload.revenueSeries[period];
    if (!series || !revenueChart) {
      return;
    }

    revenueChart.innerHTML = buildLineChart({
      chartId: "revenue",
      labels: series.labels || [],
      values: series.values || [],
      lineClass: "chart-line chart-line--orange",
      pointClass: "chart-point chart-point--orange",
      yFormatter: formatCompactCurrency,
      areaColor: "#f97316",
      smooth: true,
      showPoints: false,
      maxLabels: 10,
    });

    if (revenueSummary) {
      renderSummary(revenueSummary, [
      { label: "Tong doanh thu", value: series.totalLabel || "0đ" },
      { label: "Trung binh", value: series.averageLabel || "0đ" },
      { label: "Dinh cao", value: series.peakLabel || "Khong co du lieu" },
      ]);
    }
  }

  function renderOrders(period) {
    const series = payload.orderSeries && payload.orderSeries[period];
    if (!series || !orderChart) {
      return;
    }

    orderChart.innerHTML = buildBarChart({
      labels: series.labels || [],
      values: series.values || [],
      yFormatter: formatCompactCount,
      maxLabels: 10,
      maxSteps: [1, 2, 4, 5, 8, 10],
      minBarWidth: 16,
      maxBarWidth: 24,
      barWidthRatio: 0.76,
      barRadius: 10,
      showTrack: false,
    });

    if (orderSummary) {
      renderSummary(orderSummary, [
        { label: "Tong don", value: series.totalLabel || "0 don" },
        { label: "Trung binh", value: series.averageLabel || "0 / moc" },
        { label: "Dinh cao", value: series.peakLabel || "Khong co du lieu" },
      ]);
    }
  }

  function renderComparison(period) {
    const series = payload.revenueComparisonSeries && payload.revenueComparisonSeries[period];
    if (!series || !comparisonChart) {
      return;
    }

    comparisonChart.innerHTML = buildMultiLineChart({
      labels: series.labels || [],
      firstValues: series.dineInValues || [],
      secondValues: series.takeAwayValues || [],
      yFormatter: formatCompactCurrency,
      maxLabels: period === "daily" ? 8 : 6,
      smooth: true,
      showPoints: false,
    });

    if (comparisonSummary) {
      renderSummary(comparisonSummary, [
      { label: "Tai cho", value: series.dineInLabel || "0đ" },
      { label: "Mang ve", value: series.takeAwayLabel || "0đ" },
      { label: "Ty trong", value: series.totalLabel || "0%" },
      ]);
    }
  }

  function renderTopProducts(period) {
    const products = payload.topProducts && payload.topProducts[period];
    if (!topProductsBody) {
      return;
    }

    if (!products || !products.length) {
      topProductsBody.innerHTML = '<tr class="table-empty"><td colspan="3">Chua co san pham phat sinh trong khoang nay.</td></tr>';
      return;
    }

    topProductsBody.innerHTML = products
      .map((product) => {
        return [
          "<tr>",
          `<td>${escapeHtml(product.name || "Mon chua dat ten")}</td>`,
          `<td class="is-center">${formatCount(product.quantity || 0)}</td>`,
          `<td class="is-center">${formatCurrency(product.revenue || 0)}</td>`,
          "</tr>",
        ].join("");
      })
      .join("");
  }

  function buildLineChart(options) {
    const labels = options.labels || [];
    const values = options.values || [];
    const width = 640;
    const height = 320;
    const padding = { top: 24, right: 24, bottom: 46, left: 64 };
    const plotWidth = width - padding.left - padding.right;
    const plotHeight = height - padding.top - padding.bottom;
    const maxValue = getNiceMax(values);
    const points = buildPoints(values, padding, plotWidth, plotHeight, maxValue);
    const linePath = options.smooth ? buildSmoothPath(points) : buildPath(points);
    const areaPath = buildAreaPath(points, padding.top + plotHeight, options.smooth);
    const xLabels = buildXLabels(labels, points, height - 8, options.maxLabels);
    const yLabels = buildYLabels(maxValue, options.yFormatter, padding, plotHeight);
    const gridLines = buildGridLines(padding, plotWidth, plotHeight);
    const gradientId = `${options.chartId}-gradient`;
    const pointMarkup = options.showPoints === false
      ? ""
      : points
          .map((point) => {
            return `<circle class="${options.pointClass}" cx="${point.x}" cy="${point.y}" r="4"></circle>`;
          })
          .join("");

    return [
      `<svg class="chart-svg" viewBox="0 0 ${width} ${height}" aria-label="Chart">`,
      "<defs>",
      `<linearGradient id="${gradientId}" x1="0" y1="0" x2="0" y2="1">`,
      `<stop offset="0%" stop-color="${options.areaColor}" stop-opacity="0.34"></stop>`,
      `<stop offset="78%" stop-color="${options.areaColor}" stop-opacity="0.08"></stop>`,
      `<stop offset="100%" stop-color="${options.areaColor}" stop-opacity="0"></stop>`,
      "</linearGradient>",
      "</defs>",
      gridLines,
      `<line x1="${padding.left}" y1="${padding.top}" x2="${padding.left}" y2="${padding.top + plotHeight}" class="axis-line"></line>`,
      `<line x1="${padding.left}" y1="${padding.top + plotHeight}" x2="${padding.left + plotWidth}" y2="${padding.top + plotHeight}" class="axis-line"></line>`,
      `<path d="${areaPath}" class="chart-area" fill="url(#${gradientId})"></path>`,
      `<path d="${linePath}" class="${options.lineClass}"></path>`,
      pointMarkup,
      yLabels,
      xLabels,
      "</svg>",
    ].join("");
  }

  function buildBarChart(options) {
    const labels = options.labels || [];
    const values = options.values || [];
    const width = 640;
    const height = 320;
    const padding = { top: 24, right: 24, bottom: 46, left: 64 };
    const plotWidth = width - padding.left - padding.right;
    const plotHeight = height - padding.top - padding.bottom;
    const maxValue = options.maxValue || getNiceMax(values, options.maxSteps);
    const barCount = Math.max(values.length, 1);
    const slotWidth = plotWidth / barCount;
    const barWidth = Math.min(options.maxBarWidth || 28, Math.max(options.minBarWidth || 14, slotWidth * (options.barWidthRatio || 0.62)));
    const barRadius = options.barRadius || 8;
    const gridLines = buildGridLines(padding, plotWidth, plotHeight);
    const yLabels = buildYLabels(maxValue, options.yFormatter, padding, plotHeight);
    const labelPoints = labels.map((_, index) => {
      const x = padding.left + slotWidth * index + slotWidth / 2;
      return {
        x: toFixed(x),
        y: toFixed(height - 8),
      };
    });

    const bars = values
      .map((value, index) => {
        const ratio = maxValue === 0 ? 0 : value / maxValue;
        const barHeight = ratio * plotHeight;
        const x = padding.left + slotWidth * index + (slotWidth - barWidth) / 2;
        const y = padding.top + plotHeight - barHeight;
        return [
          options.showTrack === false ? "" : `<rect class="bar-track" x="${x}" y="${padding.top}" width="${barWidth}" height="${plotHeight}" rx="${barRadius}"></rect>`,
          `<rect class="bar-fill" x="${x}" y="${y}" width="${barWidth}" height="${barHeight}" rx="${barRadius}"></rect>`,
        ].join("");
      })
      .join("");

    const xLabels = buildXLabels(labels, labelPoints, height - 8, options.maxLabels);

    return [
      `<svg class="chart-svg" viewBox="0 0 ${width} ${height}" aria-label="Bar chart">`,
      gridLines,
      `<line x1="${padding.left}" y1="${padding.top}" x2="${padding.left}" y2="${padding.top + plotHeight}" class="axis-line"></line>`,
      `<line x1="${padding.left}" y1="${padding.top + plotHeight}" x2="${padding.left + plotWidth}" y2="${padding.top + plotHeight}" class="axis-line"></line>`,
      bars,
      yLabels,
      xLabels,
      "</svg>",
    ].join("");
  }

  function buildMultiLineChart(options) {
    const labels = options.labels || [];
    const firstValues = options.firstValues || [];
    const secondValues = options.secondValues || [];
    const width = 640;
    const height = 320;
    const padding = { top: 24, right: 24, bottom: 46, left: 64 };
    const plotWidth = width - padding.left - padding.right;
    const plotHeight = height - padding.top - padding.bottom;
    const maxValue = getNiceMax(firstValues.concat(secondValues));
    const firstPoints = buildPoints(firstValues, padding, plotWidth, plotHeight, maxValue);
    const secondPoints = buildPoints(secondValues, padding, plotWidth, plotHeight, maxValue);
    const labelPoints = firstPoints.length ? firstPoints : secondPoints;
    const xLabels = buildXLabels(labels, labelPoints, height - 8, options.maxLabels);
    const yLabels = buildYLabels(maxValue, options.yFormatter, padding, plotHeight);
    const gridLines = buildGridLines(padding, plotWidth, plotHeight);
    const buildSeriesPath = options.smooth === false ? buildPath : buildSmoothPath;
    const firstPointMarkup = options.showPoints === false
      ? ""
      : firstPoints.map((point) => `<circle class="chart-point chart-point--green" cx="${point.x}" cy="${point.y}" r="4"></circle>`).join("");
    const secondPointMarkup = options.showPoints === false
      ? ""
      : secondPoints.map((point) => `<circle class="chart-point chart-point--blue" cx="${point.x}" cy="${point.y}" r="4"></circle>`).join("");

    return [
      `<svg class="chart-svg" viewBox="0 0 ${width} ${height}" aria-label="Comparison chart">`,
      gridLines,
      `<line x1="${padding.left}" y1="${padding.top}" x2="${padding.left}" y2="${padding.top + plotHeight}" class="axis-line"></line>`,
      `<line x1="${padding.left}" y1="${padding.top + plotHeight}" x2="${padding.left + plotWidth}" y2="${padding.top + plotHeight}" class="axis-line"></line>`,
      `<path d="${buildSeriesPath(firstPoints)}" class="chart-line chart-line--green"></path>`,
      `<path d="${buildSeriesPath(secondPoints)}" class="chart-line chart-line--blue"></path>`,
      firstPointMarkup,
      secondPointMarkup,
      yLabels,
      xLabels,
      "</svg>",
    ].join("");
  }

  function buildPoints(values, padding, plotWidth, plotHeight, maxValue) {
    const denominator = Math.max(values.length - 1, 1);
    return values.map((value, index) => {
      const x = padding.left + (plotWidth * index) / denominator;
      const y = padding.top + plotHeight - ((value || 0) / maxValue) * plotHeight;
      return {
        x: toFixed(x),
        y: toFixed(y),
      };
    });
  }

  function buildPath(points) {
    if (!points.length) {
      return "";
    }

    return points
      .map((point, index) => `${index === 0 ? "M" : "L"} ${point.x} ${point.y}`)
      .join(" ");
  }

  function buildSmoothPath(points) {
    if (points.length < 2) {
      return buildPath(points);
    }

    let path = `M ${points[0].x} ${points[0].y}`;

    for (let index = 0; index < points.length - 1; index += 1) {
      const previous = points[index - 1] || points[index];
      const current = points[index];
      const next = points[index + 1];
      const afterNext = points[index + 2] || next;

      const controlPointOneX = toFixed(Number(current.x) + (Number(next.x) - Number(previous.x)) / 6);
      const controlPointOneY = toFixed(Number(current.y) + (Number(next.y) - Number(previous.y)) / 6);
      const controlPointTwoX = toFixed(Number(next.x) - (Number(afterNext.x) - Number(current.x)) / 6);
      const controlPointTwoY = toFixed(Number(next.y) - (Number(afterNext.y) - Number(current.y)) / 6);

      path += ` C ${controlPointOneX} ${controlPointOneY}, ${controlPointTwoX} ${controlPointTwoY}, ${next.x} ${next.y}`;
    }

    return path;
  }

  function buildAreaPath(points, bottom, smooth) {
    if (!points.length) {
      return "";
    }

    const path = smooth ? buildSmoothPath(points) : buildPath(points);
    return `${path} L ${points[points.length - 1].x} ${bottom} L ${points[0].x} ${bottom} Z`;
  }

  function buildGridLines(padding, plotWidth, plotHeight) {
    const lines = [];
    for (let index = 0; index < 5; index += 1) {
      const y = padding.top + (plotHeight * index) / 4;
      lines.push(`<line x1="${padding.left}" y1="${y}" x2="${padding.left + plotWidth}" y2="${y}" class="grid-line"></line>`);
    }
    return lines.join("");
  }

  function buildYLabels(maxValue, formatter, padding, plotHeight) {
    const labels = [];
    for (let index = 0; index < 5; index += 1) {
      const value = maxValue - (maxValue * index) / 4;
      const y = padding.top + (plotHeight * index) / 4 + 4;
      labels.push(`<text x="${padding.left - 14}" y="${y}" class="axis-label">${formatter(value)}</text>`);
    }
    return labels.join("");
  }

  function buildXLabels(labels, points, yPosition, maxLabels) {
    const visibleIndices = labels
      .map((_, index) => index)
      .filter((index) => shouldRenderLabel(index, labels.length, maxLabels));

    // Keep the final label visible while dropping any label whose rendered bounds would collide with it.
    const fittedIndices = [];

    visibleIndices.forEach((index) => {
      const point = points[index];
      if (!point) {
        return;
      }

      if (!fittedIndices.length) {
        fittedIndices.push(index);
        return;
      }

      const previousIndex = fittedIndices[fittedIndices.length - 1];
      const currentBounds = getLabelBounds(labels[index], point.x, index === labels.length - 1);
      const previousBounds = getLabelBounds(labels[previousIndex], points[previousIndex].x, previousIndex === labels.length - 1);

      if (currentBounds.left <= previousBounds.right + 10) {
        if (index === labels.length - 1) {
          fittedIndices[fittedIndices.length - 1] = index;
        }

        return;
      }

      fittedIndices.push(index);
    });

    return fittedIndices
      .map((index) => {
        const cssClass = index === labels.length - 1 ? "axis-label axis-label--x axis-label--end" : "axis-label axis-label--x";
        return `<text x="${points[index].x}" y="${yPosition}" class="${cssClass}">${escapeHtml(labels[index])}</text>`;
      })
      .join("");
  }

  function getLabelBounds(label, x, isEndAligned) {
    const width = estimateLabelWidth(label);
    const anchorX = Number(x);

    if (isEndAligned) {
      return {
        left: anchorX - width,
        right: anchorX,
      };
    }

    return {
      left: anchorX - width / 2,
      right: anchorX + width / 2,
    };
  }

  function estimateLabelWidth(label) {
    return String(label || "").length * 7 + 10;
  }

  function renderSummary(container, items) {
    container.innerHTML = items
      .map((item) => {
        return [
          '<div class="summary-pill">',
          `<p class="summary-pill__label">${escapeHtml(item.label)}</p>`,
          `<p class="summary-pill__value">${escapeHtml(item.value)}</p>`,
          "</div>",
        ].join("");
      })
      .join("");
  }

  function shouldRenderLabel(index, total, maxLabels = 6) {
    if (total <= maxLabels) {
      return true;
    }

    const step = Math.ceil(total / maxLabels);
    return index === total - 1 || index % step === 0;
  }

  function setupRevenuePanel() {
    const revenueSelect = document.querySelector('[data-series-select="revenue"]');
    if (revenueSelect) {
      revenueSelect.value = "daily";
    }

    const revenueTitle = document.querySelector(".panel-card--revenue .panel-card__header h3");
    if (revenueTitle) {
      revenueTitle.textContent = "Doanh thu theo";
    }
  }

  function setupOrdersPanel() {
    const ordersSelect = document.querySelector('[data-series-select="orders"]');
    if (ordersSelect) {
      ordersSelect.value = "daily";
    }

    const ordersTitle = document.querySelector(".panel-card--orders .panel-card__header h3");
    if (ordersTitle) {
      ordersTitle.textContent = "Số Lượng Đơn Hàng";
    }
  }

  function setupComparisonPanel() {
    const comparisonSelect = document.querySelector('[data-series-select="comparison"]');
    if (comparisonSelect) {
      comparisonSelect.value = "daily";
    }
  }

  function setupProductsPanel() {
    const productsSelect = document.querySelector('[data-series-select="products"]');
    if (productsSelect) {
      productsSelect.value = "daily";
    }

    const productsTitle = document.querySelector(".panel-card--products .panel-card__header h3");
    if (productsTitle) {
      productsTitle.textContent = "Sản Phẩm Bán Chạy";
    }
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

  function normalizePanelTitles() {
    const panelTitles = [
      [".panel-card--revenue .panel-card__header h3", "Doanh thu theo"],
      [".panel-card--orders .panel-card__header h3", "Số lượng đơn hàng"],
      [".panel-card--comparison .panel-card__header h3", "So sánh doanh thu tại chỗ và mang về"],
      [".panel-card--products .panel-card__header h3", "Sản phẩm bán chạy"],
    ];

    panelTitles.forEach(([selector, title]) => {
      const element = document.querySelector(selector);
      if (element) {
        element.textContent = title;
      }
    });
  }

  function getNiceMax(values, steps) {
    const maxValue = Math.max.apply(null, values.concat([0]));
    if (maxValue <= 0) {
      return 1;
    }

    const niceSteps = steps && steps.length ? steps : [1, 2, 5, 10];
    const exponent = Math.pow(10, Math.floor(Math.log10(maxValue)));
    const normalized = maxValue / exponent;

    for (let index = 0; index < niceSteps.length; index += 1) {
      if (normalized <= niceSteps[index]) {
        return niceSteps[index] * exponent;
      }
    }

    return niceSteps[niceSteps.length - 1] * exponent;
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

  function formatCurrency(value) {
    return `${formatCount(Math.round(value || 0))}đ`;
  }

  function formatCount(value) {
    return new Intl.NumberFormat("vi-VN").format(Math.round(value || 0));
  }

  function formatCompactCurrency(value) {
    return compactNumber(value, true);
  }

  function formatCompactCount(value) {
    return compactNumber(value, false);
  }

  function compactNumber(value, isCurrency) {
    const safeValue = Number(value || 0);

    if (safeValue >= 1000000000) {
      return `${(safeValue / 1000000000).toFixed(1)}B${isCurrency ? "" : ""}`;
    }

    if (safeValue >= 1000000) {
      return `${(safeValue / 1000000).toFixed(1)}M`;
    }

    if (safeValue >= 1000) {
      return `${(safeValue / 1000).toFixed(0)}K`;
    }

    return String(Math.round(safeValue));
  }

  function escapeHtml(value) {
    return String(value == null ? "" : value)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function toFixed(number) {
    return Number(number).toFixed(2);
  }
})();
