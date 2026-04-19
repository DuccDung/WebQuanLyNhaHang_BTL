const form = document.getElementById("login-form");
const nameInput = document.getElementById("name");
const passwordInput = document.getElementById("password");
const submitButton = document.getElementById("login-submit");
const submitText = document.getElementById("login-submit-text");
const spinner = document.getElementById("login-spinner");
const togglePasswordButton = document.getElementById("toggle-password");
const eyeIcon = togglePasswordButton?.querySelector(".icon-eye");
const eyeOffIcon = togglePasswordButton?.querySelector(".icon-eye-off");
const forgotButton = document.getElementById("forgot-password");
const forgotModal = document.getElementById("forgot-modal");
const closeModalButton = document.getElementById("close-modal");
const confirmSupportButton = document.getElementById("confirm-support");

if (form) {
  form.addEventListener("submit", () => {
    setLoading(true);
  });
}

if (togglePasswordButton) {
  togglePasswordButton.addEventListener("click", handleTogglePassword);
}

if (forgotButton) {
  forgotButton.addEventListener("click", openSupportModal);
}

if (closeModalButton) {
  closeModalButton.addEventListener("click", closeSupportModal);
}

if (confirmSupportButton) {
  confirmSupportButton.addEventListener("click", closeSupportModal);
}

if (forgotModal) {
  forgotModal.addEventListener("click", (event) => {
    if (event.target.classList.contains("support-modal__backdrop")) {
      closeSupportModal();
    }
  });
}

document.addEventListener("keydown", (event) => {
  if (event.key === "Escape" && forgotModal && !forgotModal.hidden) {
    closeSupportModal();
  }
});

window.addEventListener("pageshow", () => {
  setLoading(false);
});

function handleTogglePassword() {
  if (!passwordInput || !togglePasswordButton || !eyeIcon || !eyeOffIcon) {
    return;
  }

  const isVisible = passwordInput.type === "text";
  passwordInput.type = isVisible ? "password" : "text";
  togglePasswordButton.setAttribute("aria-label", isVisible ? "Hiện mật khẩu" : "Ẩn mật khẩu");
  togglePasswordButton.setAttribute("aria-pressed", String(!isVisible));
  eyeIcon.hidden = !isVisible;
  eyeOffIcon.hidden = isVisible;
}

function openSupportModal() {
  if (forgotModal) {
    forgotModal.hidden = false;
  }
}

function closeSupportModal() {
  if (forgotModal) {
    forgotModal.hidden = true;
  }
}

function setLoading(isLoading) {
  if (!submitButton || !submitText || !spinner) {
    return;
  }

  if (nameInput) {
    nameInput.readOnly = isLoading;
  }

  if (passwordInput) {
    passwordInput.readOnly = isLoading;
  }

  if (togglePasswordButton) {
    togglePasswordButton.disabled = isLoading;
  }

  submitButton.disabled = isLoading;
  spinner.hidden = !isLoading;
  submitText.textContent = isLoading ? "Đang đăng nhập..." : "Đăng nhập";
}
