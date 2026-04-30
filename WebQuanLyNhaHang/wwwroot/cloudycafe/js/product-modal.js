(function () {
  const dataElement = document.getElementById("cloudy-product-data");
  const backdrop = document.getElementById("product-order-backdrop");
  const modal = backdrop?.querySelector(".product-order-modal");
  const form = document.getElementById("product-order-form");

  if (!dataElement || !backdrop || !modal || !form) {
    return;
  }

  let products = [];
  try {
    products = JSON.parse(dataElement.textContent || "[]");
  } catch {
    products = [];
  }

  const productMap = new Map(products.map((product) => [String(product.id), product]));
  const elements = {
    id: document.getElementById("product-modal-id"),
    image: document.getElementById("product-modal-image"),
    title: document.getElementById("product-modal-title"),
    price: document.getElementById("product-modal-price"),
    description: document.getElementById("product-modal-description"),
    quantity: document.getElementById("product-modal-quantity"),
    quantityInput: document.getElementById("product-modal-quantity-input"),
    conditionInput: document.getElementById("product-modal-condition"),
    noteInput: document.getElementById("product-modal-note"),
    total: document.getElementById("product-modal-total"),
    conditionOptions: document.getElementById("product-condition-options")
  };

  const formatter = new Intl.NumberFormat("vi-VN");
  const state = {
    product: null,
    quantity: 1,
    options: {
      condition: "Đá",
      size: "Size M",
      sugar: "100% đường",
      ice: "100% đá"
    },
    optionPrices: {
      condition: 0,
      size: 0,
      sugar: 0,
      ice: 0
    }
  };

  function currency(value) {
    return `${formatter.format(Math.max(0, Number(value) || 0))}đ`;
  }

  function optionButtons(group) {
    return Array.from(modal.querySelectorAll(`[data-option-group="${group}"]`));
  }

  function setActiveOption(button) {
    const group = button.dataset.optionGroup;
    if (!group) {
      return;
    }

    optionButtons(group).forEach((item) => {
      item.classList.toggle("active", item === button);
      item.setAttribute("aria-pressed", item === button ? "true" : "false");
    });

    state.options[group] = button.dataset.optionLabel || button.textContent.trim();
    state.optionPrices[group] = Number(button.dataset.optionPrice || 0);
    updateTotal();
  }

  function selectedToppings() {
    return Array.from(modal.querySelectorAll("[data-topping-label]:checked")).map((input) => ({
      label: input.dataset.toppingLabel || "",
      price: Number(input.dataset.toppingPrice || 0)
    }));
  }

  function selectedOptionText() {
    return [
      `Loại: ${state.options.condition}`,
      `Size: ${state.options.size}`,
      `Đường: ${state.options.sugar}`,
      `Đá: ${state.options.ice}`
    ].join("; ");
  }

  function updateHiddenInputs() {
    const toppings = selectedToppings();
    elements.quantityInput.value = String(state.quantity);
    elements.conditionInput.value = selectedOptionText();
    elements.noteInput.value = toppings.length
      ? `Topping: ${toppings.map((topping) => topping.label).join(", ")}`
      : "";
  }

  function updateTotal() {
    if (!state.product) {
      return;
    }

    const basePrice = Number(state.product.price || 0);
    const optionTotal = Object.values(state.optionPrices).reduce((sum, value) => sum + value, 0);
    const toppingTotal = selectedToppings().reduce((sum, topping) => sum + topping.price, 0);
    const total = (basePrice + optionTotal + toppingTotal) * state.quantity;
    elements.quantity.textContent = String(state.quantity);
    elements.price.textContent = currency(basePrice);
    elements.total.textContent = currency(total);
    updateHiddenInputs();
  }

  function buildConditionOptions(product) {
    const conditions = Array.isArray(product.conditions) && product.conditions.length
      ? product.conditions
      : ["Đá", "Nóng"];

    elements.conditionOptions.innerHTML = conditions.map((condition, index) => (
      `<button type="button" class="option-choice product-option-choice${index === 0 ? " active" : ""}" data-option-group="condition" data-option-label="${escapeAttribute(condition)}" data-option-price="0" aria-pressed="${index === 0 ? "true" : "false"}">${escapeHtml(condition)}</button>`
    )).join("");

    state.options.condition = conditions[0];
    state.optionPrices.condition = 0;
  }

  function escapeHtml(value) {
    return String(value)
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#039;");
  }

  function escapeAttribute(value) {
    return escapeHtml(value);
  }

  function resetStaticOptions() {
    modal.querySelectorAll(".product-option-choice").forEach((button) => {
      const isDefault = button.matches('[data-option-label="Size M"], [data-option-label="100% đường"], [data-option-label="100% đá"]');
      if (button.dataset.optionGroup !== "condition") {
        button.classList.toggle("active", isDefault);
        button.setAttribute("aria-pressed", isDefault ? "true" : "false");
      }
    });

    modal.querySelectorAll("[data-topping-label]").forEach((input) => {
      input.checked = false;
    });

    state.options.size = "Size M";
    state.options.sugar = "100% đường";
    state.options.ice = "100% đá";
    state.optionPrices.size = 0;
    state.optionPrices.sugar = 0;
    state.optionPrices.ice = 0;
  }

  function openProduct(productId) {
    const product = productMap.get(String(productId));
    if (!product) {
      return;
    }

    state.product = product;
    state.quantity = 1;
    resetStaticOptions();
    buildConditionOptions(product);

    elements.id.value = product.id;
    elements.image.src = product.image || "";
    elements.image.alt = product.name || "Sản phẩm";
    elements.title.textContent = product.name || "Sản phẩm";
    elements.description.textContent = product.description || "Thức uống được chuẩn bị theo lựa chọn của bạn.";

    updateTotal();
    backdrop.hidden = false;
    document.body.classList.add("modal-open");
    modal.querySelector("[data-quantity-action='decrease']").focus();
  }

  function closeModal() {
    backdrop.hidden = true;
    document.body.classList.remove("modal-open");
  }

  document.addEventListener("click", (event) => {
    const explicitButton = event.target.closest(".js-open-product-modal");
    const card = event.target.closest(".js-product-card");
    const opener = explicitButton || card;

    if (!opener) {
      return;
    }

    const productId = opener.dataset.productId || card?.dataset.productId;
    if (!productId) {
      return;
    }

    event.preventDefault();
    openProduct(productId);
  });

  document.addEventListener("keydown", (event) => {
    const card = event.target.closest(".js-product-card");
    if (!card || (event.key !== "Enter" && event.key !== " ")) {
      return;
    }

    event.preventDefault();
    openProduct(card.dataset.productId);
  });

  backdrop.addEventListener("click", (event) => {
    if (event.target === backdrop || event.target.closest("[data-product-modal-close]")) {
      closeModal();
    }
  });

  document.addEventListener("keydown", (event) => {
    if (!backdrop.hidden && event.key === "Escape") {
      closeModal();
    }
  });

  modal.addEventListener("click", (event) => {
    const quantityAction = event.target.closest("[data-quantity-action]");
    if (quantityAction) {
      state.quantity += quantityAction.dataset.quantityAction === "increase" ? 1 : -1;
      state.quantity = Math.max(1, state.quantity);
      updateTotal();
      return;
    }

    const option = event.target.closest("[data-option-group]");
    if (option) {
      setActiveOption(option);
    }
  });

  modal.addEventListener("change", (event) => {
    if (event.target.matches("[data-topping-label]")) {
      updateTotal();
    }
  });

  form.addEventListener("submit", () => {
    updateHiddenInputs();
  });
})();
