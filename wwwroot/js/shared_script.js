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

    // 2. МОДАЛЬНЫЕ ОКНА (ИСПРАВЛЕНО ОТОБРАЖЕНИЕ)
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

            // !!! ВАЖНО: Делаем форму видимой !!!
            const form = modal.querySelector('.modal-form');
            if (form) {
                form.classList.remove('form-hidden');
                // Небольшая задержка для плавной анимации (если она есть в CSS)
                setTimeout(() => {
                    form.classList.add('form-visible');
                }, 10);
            }
        }

        // Функция закрытия
        function closeAllModals() {
            modalOverlay.classList.remove('active');
            document.querySelectorAll('.modal').forEach(modal => {
                modal.classList.remove('active');
                const form = modal.querySelector('.modal-form');
                if (form) {
                    form.classList.remove('form-visible');
                    form.classList.add('form-hidden');
                }
            });
        }

        // Функция переключения (Вход <-> Регистрация)
        function switchModal(targetModalId) {
            const targetModal = document.getElementById(targetModalId);
            if (!targetModal) return;

            // Скрываем все текущие
            document.querySelectorAll('.modal.active').forEach(m => {
                m.classList.remove('active');
                const f = m.querySelector('.modal-form');
                if (f) f.classList.remove('form-visible');
            });

            // Показываем целевое
            targetModal.classList.add('active');
            const targetForm = targetModal.querySelector('.modal-form');
            if (targetForm) {
                targetForm.classList.remove('form-hidden');
                setTimeout(() => targetForm.classList.add('form-visible'), 10);
            }
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

    // 4. ЛОГИКА ВХОДА, РЕГИСТРАЦИИ И ПОДТВЕРЖДЕНИЯ
    // (Здесь код для отправки данных на сервер)

    // --- ВХОД ---
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
                    const form = document.querySelector('#login-modal form');
                    if (errorContainer) displayErrors(err, errorContainer, form);
                });
        });
    }

    // --- РЕГИСТРАЦИЯ ---
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

                    // Закрываем регистрацию
                    if (registerModal) {
                        registerModal.classList.remove('active');
                        registerModal.querySelector('.modal-form').classList.remove('form-visible');
                    }

                    // Открываем подтверждение
                    if (confirmModal) openModal(confirmModal);

                    // Заполняем скрытые поля
                    document.getElementById('confirm-login').value = data.login;
                    document.getElementById('confirm-email').value = data.email;
                    document.getElementById('confirm-password').value = data.password;
                    document.getElementById('confirm-generated-code').value = data.generatedCode;
                })
                .catch(err => {
                    const form = document.querySelector('#register-modal form');
                    if (errorContainer) displayErrors(err, errorContainer, form);
                });
        });
    }

    // --- ПОДТВЕРЖДЕНИЕ КОДА ---
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
                    const form = document.querySelector('#confirm-email-modal form');
                    if (errorContainer) displayErrors(err, errorContainer, form);
                });
        });
    }

    // 5. КАРУСЕЛЬ
    const carouselWrapper = document.querySelector('.services-wrapper');
    if (carouselWrapper) {
        let scrollInterval;
        const scrollAmount = 1;
        const scrollSpeed = 30;
        let originalWidth = 0;
        function setupInfiniteScroll() {
            originalWidth = carouselWrapper.scrollWidth;
            const originalCards = Array.from(carouselWrapper.children);
            originalCards.forEach(card => {
                const clone = card.cloneNode(true);
                carouselWrapper.appendChild(clone);
            });
        }
        function startScrolling() {
            clearInterval(scrollInterval);
            scrollInterval = setInterval(() => {
                if (carouselWrapper.scrollLeft >= originalWidth) {
                    carouselWrapper.scrollLeft = carouselWrapper.scrollLeft - originalWidth;
                } else {
                    carouselWrapper.scrollLeft += scrollAmount;
                }
            }, scrollSpeed);
        }
        function stopScrolling() { clearInterval(scrollInterval); }
        setupInfiniteScroll();
        startScrolling();
        carouselWrapper.addEventListener('mouseenter', stopScrolling);
        carouselWrapper.addEventListener('mouseleave', startScrolling);
        const arrows = document.querySelectorAll('.arrow');
        if (arrows) arrows.forEach(arrow => arrow.addEventListener('click', stopScrolling));
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
            return response.json().then(errorData => { throw errorData; })
                .catch(() => { throw new Error('Ошибка сервера ' + response.status); });
        }
        return response.text().then(text => text ? JSON.parse(text) : {});
    });
}

function displayErrors(errors, errorContainer, formElement) {
    errorContainer.innerHTML = '';
    if (formElement) {
        const inputs = formElement.querySelectorAll('input');
        inputs.forEach(input => input.classList.remove('input-error'));
    }
    if (errors.description) {
        const msg = document.createElement('div');
        msg.classList.add('error-message');
        msg.style.color = 'red';
        msg.textContent = errors.description;
        errorContainer.appendChild(msg);
        return;
    }
    if (Array.isArray(errors)) {
        errors.forEach(error => {
            const msg = document.createElement('div');
            msg.classList.add('error-message');
            msg.style.color = 'red';
            msg.textContent = error.message || error;
            errorContainer.appendChild(msg);
        });
    }
}