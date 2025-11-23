document.addEventListener('DOMContentLoaded', function () {

    // =========================================
    //   1. АНИМАЦИЯ ШАПКИ
    // =========================================
    window.addEventListener('scroll', function () {
        var header = document.getElementById('header-top');
        var maxScroll = 250;
        if (scrollY > maxScroll) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }
    });

    // =========================================
    //   2. МОДАЛЬНЫЕ ОКНА И КНОПКИ
    // =========================================
    const modalOverlay = document.getElementById('modal-overlay');
    const loginModal = document.getElementById('login-modal');
    const registerModal = document.getElementById('register-modal');

    const openLoginBtn = document.getElementById('open-login-modal');
    const openRegisterBtn = document.getElementById('open-register-modal');

    const closeBtns = document.querySelectorAll('[data-close-modal]');
    const switchBtns = document.querySelectorAll('[data-switch-modal]');

    if (modalOverlay && loginModal && registerModal && openLoginBtn && openRegisterBtn) {
        function openModal(modal) {
            if (!modal) return;
            modalOverlay.classList.add('active');
            modal.classList.add('active');
        }
        function closeAllModals() {
            modalOverlay.classList.remove('active');
            loginModal.classList.remove('active');
            registerModal.classList.remove('active');
        }
        function switchModal(targetModalId) {
            const targetModal = document.getElementById(targetModalId);
            if (!targetModal) return;

            const currentModal = targetModalId === 'login-modal' ? registerModal : loginModal;
            const currentForm = currentModal.querySelector('.modal-form');
            const targetForm = targetModal.querySelector('.modal-form');

            currentForm.classList.add('form-hidden');
            currentForm.classList.remove('form-visible');
            currentModal.classList.remove('active');

            targetModal.classList.add('active');
            targetForm.classList.add('form-before-enter');
            targetForm.classList.remove('form-hidden');

            setTimeout(() => {
                targetForm.classList.add('form-visible');
                targetForm.classList.remove('form-before-enter');
            }, 100);

            setTimeout(() => {
                currentForm.classList.remove('form-hidden');
            }, 400);
        }

        openLoginBtn.addEventListener('click', () => openModal(loginModal));
        openRegisterBtn.addEventListener('click', () => openModal(registerModal));
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

    // =========================================
    //   3. ГАМБУРГЕР МЕНЮ
    // =========================================
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

    // =========================================
    //   4. ПЕРЕКЛЮЧАТЕЛЬ ТЕМЫ
    // =========================================
    const themeToggle = document.getElementById('theme-toggle');
    if (themeToggle) {
        function updateThemeIcons() {
            const isDark = document.body.classList.contains('dark-mode');
            const sunIcon = themeToggle.querySelector('.icon-sun');
            const moonIcon = themeToggle.querySelector('.icon-moon');
            if (sunIcon) sunIcon.style.display = isDark ? 'none' : 'inline';
            if (moonIcon) moonIcon.style.display = isDark ? 'inline' : 'none';
        }

        themeToggle.addEventListener('click', () => {
            document.body.classList.toggle('dark-mode');
            const isDark = document.body.classList.contains('dark-mode');
            localStorage.setItem('theme', isDark ? 'dark' : 'light');
            updateThemeIcons();
        });

        // Инициализация при загрузке
        if (localStorage.getItem('theme') === 'dark') {
            document.body.classList.add('dark-mode');
        }
        updateThemeIcons();
    }

    // =========================================
    //   5. КАРУСЕЛЬ (БЕСКОНЕЧНАЯ ПРОКРУТКА)
    // =========================================
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

        function stopScrolling() {
            clearInterval(scrollInterval);
        }

        setupInfiniteScroll();
        startScrolling();

        carouselWrapper.addEventListener('mouseenter', stopScrolling);
        carouselWrapper.addEventListener('mouseleave', startScrolling);

        const arrows = document.querySelectorAll('.arrow');
        if (arrows) {
            arrows.forEach(arrow => arrow.addEventListener('click', stopScrolling));
        }
    }

    // =========================================
    //   6. FETCH-ЗАПРОСЫ (РЕГИСТРАЦИЯ И ВХОД)
    //   !!! ИСПРАВЛЕННАЯ ЧАСТЬ !!!
    // =========================================

    // ВХОД
    const loginSubmitBtn = document.getElementById('login-submit-btn');
    if (loginSubmitBtn) {
        loginSubmitBtn.addEventListener('click', (e) => {
            e.preventDefault(); // Остановка перезагрузки

            const requestURL = '/Home/Login'; // Правильный адрес
            const errorContainer = document.getElementById('login-error-container');
            const body = {
                email: document.getElementById('login-email').value,
                password: document.getElementById('login-password').value
            };
            errorContainer.innerHTML = '';

            sendRequest('POST', requestURL, body)
                .then(data => {
                    console.log('Успешный вход:', data);
                    location.reload();
                })
                .catch(err => {
                    console.log('Ошибка входа:', err);
                    const form = document.querySelector('#login-modal form');
                    displayErrors(err, errorContainer, form);
                });
        });
    }

    // РЕГИСТРАЦИЯ
    const registerSubmitBtn = document.getElementById('register-submit-btn');
    if (registerSubmitBtn) {
        registerSubmitBtn.addEventListener('click', (e) => {
            e.preventDefault(); // Остановка перезагрузки

            const requestURL = '/Home/Register'; // Правильный адрес
            const errorContainer = document.getElementById('register-error-container');
            const body = {
                login: document.getElementById('register-login').value,
                email: document.getElementById('register-email').value,
                password: document.getElementById('register-password').value,
                passwordConfirm: document.getElementById('register-passwordConfirm').value
            };
            errorContainer.innerHTML = '';

            sendRequest('POST', requestURL, body)
                .then(data => {
                    console.log('Успешная регистрация:', data);
                    location.reload();
                })
                .catch(err => {
                    console.log('Ошибка регистрации:', err);
                    const form = document.querySelector('#register-modal form');
                    displayErrors(err, errorContainer, form);
                });
        });
    }
});

// =========================================
//   ГЛОБАЛЬНЫЕ ФУНКЦИИ
// =========================================

function sendRequest(method, url, body = null) {
    const headers = {
        'Content-Type': 'application/json'
    };
    return fetch(url, {
        method: method,
        body: body ? JSON.stringify(body) : null,
        headers: headers
    }).then(response => {
        if (!response.ok) {
            return response.json().then(errorData => {
                if (!errorData) throw new Error('Ошибка сервера');
                throw errorData;
            });
        }
        return response.text().then(text => text ? JSON.parse(text) : {});
    });
}

function displayErrors(errors, errorContainer, formElement) {
    errorContainer.innerHTML = '';
    const inputs = formElement.querySelectorAll('input');
    inputs.forEach(input => input.classList.remove('input-error'));

    // Если errors - это объект с описанием (как мы возвращаем из C#)
    if (errors.description) {
        const errorMessage = document.createElement('div');
        errorMessage.classList.add('error');
        errorMessage.textContent = errors.description;
        errorContainer.appendChild(errorMessage);
        return;
    }

    // Если массив ошибок
    if (Array.isArray(errors)) {
        errors.forEach(error => {
            const errorMessage = document.createElement('div');
            errorMessage.classList.add('error');
            errorMessage.textContent = error.message || error;
            errorContainer.appendChild(errorMessage);
        });
    }
}