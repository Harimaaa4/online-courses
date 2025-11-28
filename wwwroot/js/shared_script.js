document.addEventListener('DOMContentLoaded', function () {

    // 1. АНИМАЦИЯ ШАПКИ
    window.addEventListener('scroll', function () {
        var header = document.getElementById('header-top');
        var maxScroll = 250;
        if (scrollY > maxScroll) {
            if (header) header.classList.add('scrolled');
        } else {
            if (header) header.classList.remove('scrolled');
        }
    });

    // 2. МОДАЛЬНЫЕ ОКНА (НОВЫЕ КЛАССЫ: AUTH-MODAL)
    const modalOverlay = document.getElementById('modal-overlay');
    const loginModal = document.getElementById('login-modal');
    const registerModal = document.getElementById('register-modal');
    const confirmModal = document.getElementById('confirm-email-modal');

    const openLoginBtn = document.getElementById('open-login-modal');
    const openRegisterBtn = document.getElementById('open-register-modal');

    const closeBtns = document.querySelectorAll('[data-close-modal]');
    const switchBtns = document.querySelectorAll('[data-switch-modal]');

    if (modalOverlay) {
        // Функция открытия
        function openModal(modal) {
            if (!modal) return;
            modalOverlay.classList.add('active');
            modal.classList.add('active');
        }

        // Функция закрытия (ищем по классу auth-modal)
        function closeAllModals() {
            modalOverlay.classList.remove('active');
            document.querySelectorAll('.auth-modal').forEach(m => m.classList.remove('active'));
        }

        // Переключение
        function switchModal(targetModalId) {
            const targetModal = document.getElementById(targetModalId);
            if (!targetModal) return;

            // Скрываем все активные
            document.querySelectorAll('.auth-modal.active').forEach(m => m.classList.remove('active'));

            // Открываем целевое
            targetModal.classList.add('active');
        }

        // Привязка событий
        if (openLoginBtn) openLoginBtn.addEventListener('click', () => openModal(loginModal));
        if (openRegisterBtn) openRegisterBtn.addEventListener('click', () => openModal(registerModal));

        closeBtns.forEach(btn => btn.addEventListener('click', closeAllModals));

        switchBtns.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const targetId = e.target.dataset.switchModal;
                switchModal(targetId);
            });
        });

        modalOverlay.addEventListener('click', (e) => {
            if (e.target === modalOverlay) closeAllModals();
        });
    }

    // 3. ГАМБУРГЕР И ТЕМА
    const hamburger = document.getElementById('hamburger-menu');
    const sideMenu = document.getElementById('side-menu');
    if (hamburger && sideMenu) {
        hamburger.addEventListener('click', () => {
            hamburger.classList.toggle('active');
            sideMenu.classList.toggle('active');
        });
    }
    const sideLoginBtn = document.getElementById('side-open-login');
    const sideRegisterBtn = document.getElementById('side-open-register');
    if (sideLoginBtn && openLoginBtn) {
        sideLoginBtn.addEventListener('click', () => {
            openLoginBtn.click();
            hamburger.classList.remove('active');
            sideMenu.classList.remove('active');
        });
    }
    if (sideRegisterBtn && openRegisterBtn) {
        sideRegisterBtn.addEventListener('click', () => {
            openRegisterBtn.click();
            hamburger.classList.remove('active');
            sideMenu.classList.remove('active');
        });
    }

    const themeToggle = document.getElementById('theme-toggle');
    if (themeToggle) {
        function updateThemeIcons() {
            const isDark = document.body.classList.contains('dark-mode');
            const sunIcon = themeToggle.querySelector('.icon-sun');
            const moonIcon = themeToggle.querySelector('.icon-moon');
            if (sunIcon) sunIcon.style.display = isDark ? 'none' : 'inline';
            if (moonIcon) moonIcon.style.display = isDark ? 'inline' : 'none';
        }
        if (localStorage.getItem('theme') === 'dark') {
            document.body.classList.add('dark-mode');
        }
        updateThemeIcons();
        themeToggle.addEventListener('click', () => {
            document.body.classList.toggle('dark-mode');
            const isDark = document.body.classList.contains('dark-mode');
            localStorage.setItem('theme', isDark ? 'dark' : 'light');
            updateThemeIcons();
        });
    }

    // 4. ЛОГИКА ФОРМ (Вход, Регистрация)
    const loginSubmitBtn = document.getElementById('login-submit-btn');
    if (loginSubmitBtn) {
        loginSubmitBtn.addEventListener('click', (e) => {
            e.preventDefault();
            const requestURL = '/Home/Login';
            const errorContainer = document.getElementById('login-error-container');
            const body = {
                email: document.getElementById('login-email').value,
                password: document.getElementById('login-password').value
            };
            if (errorContainer) errorContainer.innerHTML = '';

            sendRequest('POST', requestURL, body)
                .then(data => {
                    console.log('Вход успешен:', data);
                    location.reload();
                })
                .catch(err => {
                    // Используем форму с новым классом auth-form
                    const form = document.querySelector('#login-modal .auth-form');
                    if (errorContainer) displayErrors(err, errorContainer, form);
                });
        });
    }

    const registerSubmitBtn = document.getElementById('register-submit-btn');
    if (registerSubmitBtn) {
        registerSubmitBtn.addEventListener('click', (e) => {
            e.preventDefault();
            const requestURL = '/Home/Register';
            const errorContainer = document.getElementById('register-error-container');
            const body = {
                login: document.getElementById('register-login').value,
                email: document.getElementById('register-email').value,
                password: document.getElementById('register-password').value,
                passwordConfirm: document.getElementById('register-passwordConfirm').value
            };
            if (errorContainer) errorContainer.innerHTML = '';

            sendRequest('POST', requestURL, body)
                .then(data => {
                    console.log('Письмо отправлено:', data);
                    closeAllModals();
                    setTimeout(() => {
                        modalOverlay.classList.add('active');
                        confirmModal.classList.add('active');
                    }, 50);

                    document.getElementById('confirm-login').value = data.login;
                    document.getElementById('confirm-email').value = data.email;
                    document.getElementById('confirm-password').value = data.password;
                    document.getElementById('confirm-generated-code').value = data.generatedCode;
                })
                .catch(err => {
                    const form = document.querySelector('#register-modal .auth-form');
                    if (errorContainer) displayErrors(err, errorContainer, form);
                });
        });
    }

    const confirmSubmitBtn = document.getElementById('confirm-submit-btn');
    if (confirmSubmitBtn) {
        confirmSubmitBtn.addEventListener('click', (e) => {
            e.preventDefault();
            const requestURL = '/Home/ConfirmEmail';
            const errorContainer = document.getElementById('confirm-error-container');
            const body = {
                codeConfirm: document.getElementById('confirm-code').value,
                generatedCode: document.getElementById('confirm-generated-code').value,
                login: document.getElementById('confirm-login').value,
                email: document.getElementById('confirm-email').value,
                password: document.getElementById('confirm-password').value
            };
            if (errorContainer) errorContainer.innerHTML = '';

            sendRequest('POST', requestURL, body)
                .then(data => {
                    console.log('Успех:', data);
                    location.reload();
                })
                .catch(err => {
                    const form = document.querySelector('#confirm-email-modal .auth-form');
                    if (errorContainer) displayErrors(err, errorContainer, form);
                });
        });
    }

    // 5. КАРУСЕЛЬ (ИСПРАВЛЕННАЯ АВТОМАТИЧЕСКАЯ ПРОКРУТКА)
    const carouselWrapper = document.querySelector('.services-wrapper');
    if (carouselWrapper) {
        // Ждем полной загрузки страницы (включая картинки), чтобы правильно посчитать ширину
        window.addEventListener('load', () => {
            startInfiniteCarousel(carouselWrapper);
        });
    }

    function startInfiniteCarousel(wrapper) {
        // 1. Запоминаем реальную ширину контента ДО клонирования
        const originalContentWidth = wrapper.scrollWidth;

        // Если контента меньше, чем ширина экрана, крутить не нужно
        if (originalContentWidth <= wrapper.clientWidth) return;

        // 2. Клонируем карточки для эффекта бесконечности
        const originalCards = Array.from(wrapper.children);
        originalCards.forEach(card => {
            const clone = card.cloneNode(true);
            clone.setAttribute('aria-hidden', 'true'); // Для доступности
            wrapper.appendChild(clone);
        });

        // 3. Настройки
        let scrollPos = 0;
        const speed = 1; // Скорость (пикселей за такт)
        const intervalTime = 20; // Частота обновления (мс)
        let isPaused = false;
        let animationId;

        // 4. Функция прокрутки
        function scroll() {
            if (!isPaused) {
                scrollPos += speed;

                // Если прокрутили на ширину оригинального набора -> сбрасываем в начало
                if (scrollPos >= originalContentWidth) {
                    scrollPos = 0;
                    wrapper.scrollLeft = 0; // Мгновенный прыжок назад
                } else {
                    wrapper.scrollLeft = scrollPos;
                }
            }
        }

        // 5. Запуск
        animationId = setInterval(scroll, intervalTime);

        // 6. Пауза при наведении
        wrapper.addEventListener('mouseenter', () => {
            isPaused = true;
        });

        wrapper.addEventListener('mouseleave', () => {
            isPaused = false;
            // Синхронизируем позицию (на случай, если пользователь покрутил колесиком)
            scrollPos = wrapper.scrollLeft;
        });

        // Поддержка тач-устройств
        wrapper.addEventListener('touchstart', () => { isPaused = true; });
        wrapper.addEventListener('touchend', () => {
            isPaused = false;
            scrollPos = wrapper.scrollLeft;
        });
    }
});

// ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ
function sendRequest(method, url, body = null) {
    const headers = { 'Content-Type': 'application/json' };
    return fetch(url, {
        method: method,
        body: body ? JSON.stringify(body) : null,
        headers: headers
    }).then(response => {
        if (!response.ok) {
            return response.json().then(e => { throw e; }).catch(() => { throw new Error(response.status); });
        }
        return response.text().then(text => text ? JSON.parse(text) : {});
    });
}

function displayErrors(errors, container, form) {
    container.innerHTML = '';
    if (form) form.querySelectorAll('input').forEach(i => i.classList.remove('input-error'));
    if (errors.description) {
        const div = document.createElement('div');
        div.className = 'error-message';
        div.textContent = errors.description;
        container.appendChild(div);
    }
    if (Array.isArray(errors)) {
        errors.forEach(e => {
            const div = document.createElement('div');
            div.className = 'error-message';
            div.textContent = e.message || e;
            container.appendChild(div);
        });
    }
}