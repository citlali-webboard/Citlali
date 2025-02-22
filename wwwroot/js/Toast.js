function addToast(title, description, additionalClassName) {
  const toastsContainer = document.getElementById("toasts");
  if (!toastsContainer) {
    console.error("Toasts container not found");
    return;
  }

  const toast = document.createElement("div");
  toast.className = `toast ${additionalClassName}`;

  const closeButton = document.createElement("button");
  closeButton.className = "toast-close-button";
  closeButton.innerHTML = "✖";
  closeButton.onclick = function () {
    toastsContainer.removeChild(toast);
  };

  setTimeout(() => toastsContainer.removeChild(toast), 3000);

  const toastTitle = document.createElement("span");
  toastTitle.className = "toast-title";
  toastTitle.innerText = title;

  const toastDescription = document.createElement("span");
  toastDescription.className = "toast-description";
  toastDescription.innerText = description;

  toast.appendChild(closeButton);
  toast.appendChild(toastTitle);
  toast.appendChild(toastDescription);

  toastsContainer.appendChild(toast);
}

function addNeutralToast(title, description) {
  addToast(title, description, "toast-style-neutral");
}

function addDestructiveToast(title, description) {
  addToast(title, description, "toast-style-destructive");
}
