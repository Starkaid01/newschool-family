document.querySelectorAll('[data-touch-stepper]').forEach((stepper) => {
    const panels = Array.from(stepper.querySelectorAll('[data-step-panel]'));
    const pills = Array.from(stepper.querySelectorAll('[data-step-target]'));
    const prevButton = stepper.querySelector('[data-step-action="prev"]');
    const nextButton = stepper.querySelector('[data-step-action="next"]');

    if (panels.length === 0) {
        return;
    }

    let currentIndex = 0;

    function syncStepper() {
        panels.forEach((panel, index) => {
            const isActive = index === currentIndex;
            panel.hidden = !isActive;
            panel.classList.toggle('is-active', isActive);
        });

        pills.forEach((pill, index) => {
            const isActive = index === currentIndex;
            pill.classList.toggle('is-active', isActive);
            pill.setAttribute('aria-pressed', isActive ? 'true' : 'false');
        });

        if (prevButton) {
            prevButton.disabled = currentIndex === 0;
        }

        if (nextButton) {
            const isLast = currentIndex === panels.length - 1;
            nextButton.disabled = isLast;
            nextButton.textContent = isLast ? 'Último passo' : 'Próximo';
        }
    }

    pills.forEach((pill) => {
        pill.addEventListener('click', () => {
            const target = Number.parseInt(pill.getAttribute('data-step-target') || '0', 10);
            if (!Number.isNaN(target)) {
                currentIndex = Math.max(0, Math.min(target, panels.length - 1));
                syncStepper();
            }
        });
    });

    prevButton?.addEventListener('click', () => {
        currentIndex = Math.max(0, currentIndex - 1);
        syncStepper();
    });

    nextButton?.addEventListener('click', () => {
        currentIndex = Math.min(panels.length - 1, currentIndex + 1);
        syncStepper();
    });

    syncStepper();
});
