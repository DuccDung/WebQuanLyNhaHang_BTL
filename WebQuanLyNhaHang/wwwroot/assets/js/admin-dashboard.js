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

  setupAccountMenu();
  setupSalesMenu();

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

  renderRevenue("yearly");
  renderOrders("yearly");
  renderComparison("yearly");
  renderTopProducts("yearly");

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
    if (!series || !revenueChart || !revenueSummary) {
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
    });

    renderSummary(revenueSummary, [
      { label: "Tong doanh thu", value: series.totalLabel || "0đ" },
      { label: "Trung binh", value: series.averageLabel || "0đ" },
      { label: "Dinh cao", value: series.peakLabel || "Khong co du lieu" },
    ]);
  }

  function renderOrders(period) {
    const series = payload.orderSeries && payload.orderSeries[period];
    if (!series || !orderChart || !orderSummary) {
      return;
    }

    orderChart.innerHTML = buildBarChart({
      labels: series.labels || [],
      values: series.values || [],
      yFormatter: formatCompactCount,
    });

    renderSummary(orderSummary, [
      { label: "Tong don", value: series.totalLabel || "0 don" },
      { label: "Trung binh", value: series.averageLabel || "0 / moc" },
      { label: "Dinh cao", value: series.peakLabel || "Khong co du lieu" },
    ]);
  }

  function renderComparison(period) {
    const series = payload.revenueComparisonSeries && payload.revenueComparisonSeries[period];
    if (!series || !comparisonChart || !comparisonSummary) {
      return;
    }

    comparisonChart.innerHTML = buildMultiLineChart({
      labels: series.labels || [],
      firstValues: series.dineInValues || [],
      secondValues: series.takeAwayValues || [],
      yFormatter: formatCompactCurrency,
    });

    renderSummary(comparisonSummary, [
      { label: "Tai cho", value: series.dineInLabel || "0đ" },
      { label: "Mang ve", value: series.takeAwayLabel || "0đ" },
      { label: "Ty trong", value: series.totalLabel || "0%" },
    ]);
  }

  function renderTopProducts(period) {
    const products = payload.topProducts && payload.topProducts[period];
    if (!topProductsBody) {
      return;
    }

    if (!products || !products.length) {
      topProductsBody.innerHTML = '<tr class="table-empty"><td colspan="4">Chua co san pham phat sinh trong khoang nay.</td></tr>';
      return;
    }

    topProductsBody.innerHTML = products
      .map((product) => {
        return [
          "<tr>",
          `<td>${escapeHtml(product.name || "Mon chua dat ten")}</td>`,
          `<td class="is-right">${formatCount(product.quantity || 0)}</td>`,
          `<td class="is-right">${formatCurrency(product.revenue || 0)}</td>`,
          `<td class="is-right">${escapeHtml(product.shareLabel || "0.0%")}</td>`,
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
    const linePath = buildPath(points);
    const areaPath = buildAreaPath(points, padding.top + plotHeight);
    const xLabels = buildXLabels(labels, points, height - 8);
    const yLabels = buildYLabels(maxValue, options.yFormatter, padding, plotHeight);
    const gridLines = buildGridLines(padding, plotWidth, plotHeight);
    const gradientId = `${options.chartId}-gradient`;
    const pointMarkup = points
      .map((point) => {
        return `<circle class="${options.pointClass}" cx="${point.x}" cy="${point.y}" r="4"></circle>`;
      })
      .join("");

    return [
      `<svg class="chart-svg" viewBox="0 0 ${width} ${height}" aria-label="Chart">`,
      "<defs>",
      `<linearGradient id="${gradientId}" x1="0" y1="0" x2="0" y2="1">`,
      `<stop offset="5%" stop-color="${options.areaColor}" stop-opacity="0.28"></stop>`,
      `<stop offset="95%" stop-color="${options.areaColor}" stop-opacity="0"></stop>`,
      "</linearGradient>",
      "</defs>",
      gridLines,
      `<line x1="${padding.left}" y1="${padding.top}" x2="${padding.left}" y2="${padding.top + plotHeight}" class="axis-line"></line>`,
      `<line x1="${padding.left}" y1="${padding.top + plotHeight}" x2="${padding.left + plotWidth}" y2="${padding.top + plotHeight}" class="axis-line"></line>`,
      `<path d="${areaPath}" fill="url(#${gradientId})"></path>`,
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
    const maxValue = getNiceMax(values);
    const barCount = Math.max(values.length, 1);
    const slotWidth = plotWidth / barCount;
    const barWidth = Math.min(28, Math.max(14, slotWidth * 0.62));
    const gridLines = buildGridLines(padding, plotWidth, plotHeight);
    const yLabels = buildYLabels(maxValue, options.yFormatter, padding, plotHeight);

    const bars = values
      .map((value, index) => {
        const ratio = maxValue === 0 ? 0 : value / maxValue;
        const barHeight = ratio * plotHeight;
        const x = padding.left + slotWidth * index + (slotWidth - barWidth) / 2;
        const y = padding.top + plotHeight - barHeight;
        return [
          `<rect class="bar-track" x="${x}" y="${padding.top}" width="${barWidth}" height="${plotHeight}" rx="8"></rect>`,
          `<rect class="bar-fill" x="${x}" y="${y}" width="${barWidth}" height="${barHeight}" rx="8"></rect>`,
        ].join("");
      })
      .join("");

    const xLabels = labels
      .map((label, index) => {
        if (!shouldRenderLabel(index, labels.length)) {
          return "";
        }

        const x = padding.left + slotWidth * index + slotWidth / 2;
        const cssClass = index === labels.length - 1 ? "axis-label axis-label--x axis-label--end" : "axis-label axis-label--x";
        return `<text x="${x}" y="${height - 8}" class="${cssClass}">${escapeHtml(label)}</text>`;
      })
      .join("");

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
    const xLabels = buildXLabels(labels, firstPoints, height - 8);
    const yLabels = buildYLabels(maxValue, options.yFormatter, padding, plotHeight);
    const gridLines = buildGridLines(padding, plotWidth, plotHeight);

    return [
      `<svg class="chart-svg" viewBox="0 0 ${width} ${height}" aria-label="Comparison chart">`,
      gridLines,
      `<line x1="${padding.left}" y1="${padding.top}" x2="${padding.left}" y2="${padding.top + plotHeight}" class="axis-line"></line>`,
      `<line x1="${padding.left}" y1="${padding.top + plotHeight}" x2="${padding.left + plotWidth}" y2="${padding.top + plotHeight}" class="axis-line"></line>`,
      `<path d="${buildPath(firstPoints)}" class="chart-line chart-line--green"></path>`,
      `<path d="${buildPath(secondPoints)}" class="chart-line chart-line--blue"></path>`,
      firstPoints.map((point) => `<circle class="chart-point chart-point--green" cx="${point.x}" cy="${point.y}" r="4"></circle>`).join(""),
      secondPoints.map((point) => `<circle class="chart-point chart-point--blue" cx="${point.x}" cy="${point.y}" r="4"></circle>`).join(""),
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

  function buildAreaPath(points, bottom) {
    if (!points.length) {
      return "";
    }

    return `${buildPath(points)} L ${points[points.length - 1].x} ${bottom} L ${points[0].x} ${bottom} Z`;
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

  function buildXLabels(labels, points, yPosition) {
    return labels
      .map((label, index) => {
        if (!shouldRenderLabel(index, labels.length)) {
          return "";
        }

        const cssClass = index === labels.length - 1 ? "axis-label axis-label--x axis-label--end" : "axis-label axis-label--x";
        return `<text x="${points[index].x}" y="${yPosition}" class="${cssClass}">${escapeHtml(label)}</text>`;
      })
      .join("");
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

  function shouldRenderLabel(index, total) {
    if (total <= 7) {
      return true;
    }

    const step = Math.ceil(total / 6);
    return index === total - 1 || index % step === 0;
  }

  function getNiceMax(values) {
    const maxValue = Math.max.apply(null, values.concat([0]));
    if (maxValue <= 0) {
      return 1;
    }

    const exponent = Math.pow(10, Math.floor(Math.log10(maxValue)));
    const normalized = maxValue / exponent;

    if (normalized <= 1) {
      return 1 * exponent;
    }
    if (normalized <= 2) {
      return 2 * exponent;
    }
    if (normalized <= 5) {
      return 5 * exponent;
    }
    return 10 * exponent;
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
