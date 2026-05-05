(function () {
  const overlay = document.getElementById("page-loading-overlay");
  const title = document.getElementById("page-loading-title");
  const text = document.getElementById("page-loading-text");

  if (!overlay || !title || !text) {
    return;
  }

  let loadingTimer = null;
  let isVisible = false;

  function clearLoadingTimer() {
    if (loadingTimer) {
      window.clearTimeout(loadingTimer);
      loadingTimer = null;
    }
  }

  function showLoading(message) {
    clearLoadingTimer();

    loadingTimer = window.setTimeout(function () {
      title.textContent = message && message.title ? message.title : "Carregando";
      text.textContent = message && message.text ? message.text : "Aguarde um instante...";
      overlay.setAttribute("aria-hidden", "false");
      document.body.classList.add("page-loading");
      isVisible = true;
    }, 140);
  }

  function hideLoading() {
    clearLoadingTimer();
    overlay.setAttribute("aria-hidden", "true");
    document.body.classList.remove("page-loading");
    isVisible = false;
  }

  function isModifiedClick(event) {
    return event.metaKey || event.ctrlKey || event.shiftKey || event.altKey || event.button !== 0;
  }

  function shouldHandleLink(link) {
    if (!link || !link.href) {
      return false;
    }

    if (link.hasAttribute("download")) {
      return false;
    }

    if (link.target && link.target !== "_self") {
      return false;
    }

    const href = link.getAttribute("href");
    if (!href || href.startsWith("#") || href.startsWith("javascript:")) {
      return false;
    }

    const destination = new URL(link.href, window.location.href);
    if (destination.origin !== window.location.origin) {
      return false;
    }

    const current = new URL(window.location.href);
    if (
      destination.pathname === current.pathname &&
      destination.search === current.search &&
      destination.hash
    ) {
      return false;
    }

    return true;
  }

  document.addEventListener("click", function (event) {
    if (isModifiedClick(event)) {
      return;
    }

    const link = event.target.closest("a");
    if (!shouldHandleLink(link)) {
      return;
    }

    const customTitle = link.dataset.loadingTitle;
    const customText = link.dataset.loadingText;

    showLoading({
      title: customTitle || "Abrindo página",
      text: customText || "Aguarde um instante..."
    });
  });

  document.addEventListener("submit", function (event) {
    const form = event.target;
    if (!(form instanceof HTMLFormElement)) {
      return;
    }

    const multipart = (form.enctype || "").toLowerCase().indexOf("multipart/form-data") >= 0;
    const customTitle = form.dataset.loadingTitle;
    const customText = form.dataset.loadingText;

    showLoading({
      title: customTitle || (multipart ? "Enviando arquivo" : "Processando"),
      text: customText || (multipart ? "Seu arquivo está sendo enviado. Aguarde..." : "Aguarde um instante...")
    });
  });

  window.addEventListener("pageshow", function () {
    hideLoading();
  });

  window.addEventListener("load", function () {
    hideLoading();
  });

  window.addEventListener("beforeunload", function () {
    if (!isVisible) {
      showLoading({
        title: "Carregando",
        text: "Aguarde um instante..."
      });
    }
  });
})();
