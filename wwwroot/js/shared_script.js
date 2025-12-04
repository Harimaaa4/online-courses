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
    // --- ФИЛЬТРЫ (ОБНОВЛЕНИЕ ЦИФР) ---
    const priceMin = document.getElementById('price-min');
    const priceMax = document.getElementById('price-max');
    const priceMinVal = document.getElementById('price-min-val');
    const priceMaxVal = document.getElementById('price-max-val');

    if (priceMin && priceMax) {
        priceMin.addEventListener('input', () => {
            priceMinVal.textContent = priceMin.value;
        });
        priceMax.addEventListener('input', () => {
            priceMaxVal.textContent = priceMax.value;
        });
    }
    // =========================================
    //   6. ФИЛЬТРАЦИЯ КУРСОВ (ГЛАВА 22-23)
    // =========================================

    const applyFiltersBtn = document.getElementById('apply-filters');
    const sortOrderSelect = document.getElementById('sort-order');

    // Функция сбора данных и отправки запроса
    function filterCourses() {
        // 1. Собираем данные с ползунков цены
        const minPrice = document.getElementById('price-min').value;
        const maxPrice = document.getElementById('price-max').value;

        // 2. Собираем отмеченные чекбоксы (Уровни)
        const selectedLevels = [];
        document.querySelectorAll('.checkbox-container input[type="checkbox"]:checked').forEach(cb => {
            selectedLevels.push(cb.value);
        });

        // 3. Берем сортировку
        const sortType = sortOrderSelect ? sortOrderSelect.value : 'default';

        // 4. Получаем ID категории из URL (он там есть: ?categoryId=...)
        const urlParams = new URLSearchParams(window.location.search);
        const categoryId = urlParams.get('categoryId');

        if (!categoryId) return; // Если мы не на странице списка, выходим

        // 5. Формируем объект фильтра
        const filterData = {
            CategoryId: categoryId,
            MinPrice: parseFloat(minPrice),
            MaxPrice: parseFloat(maxPrice),
            Levels: selectedLevels,
            SortType: sortType
        };

        // 6. Отправляем на сервер
        const container = document.querySelector('.courses-list-container');
        if (container) container.style.opacity = '0.5'; // Эффект загрузки

        sendRequest('POST', '/Home/GetCoursesByFilter', filterData)
            .then(courses => {
                renderCourses(courses);
                if (container) container.style.opacity = '1';
            })
            .catch(err => {
                console.error('Ошибка фильтрации:', err);
                if (container) container.style.opacity = '1';
            });
    }

    // Привязываем функцию к кнопке "Применить"
    if (applyFiltersBtn) {
        applyFiltersBtn.addEventListener('click', (e) => {
            e.preventDefault();
            filterCourses();
        });
    }

    // Привязываем функцию к селекту сортировки (чтобы сразу обновлялось)
    if (sortOrderSelect) {
        sortOrderSelect.addEventListener('change', () => {
            filterCourses();
        });
    }

    // Функция отрисовки карточек (Вспомогательная)
    function renderCourses(courses) {
        const container = document.querySelector('.courses-list-container');
        if (!container) return;

        container.innerHTML = ''; // Очищаем старые

        if (courses.length === 0) {
            container.innerHTML = '<p style="width:100%; text-align:center; grid-column: 1/-1;">Курсы не найдены.</p>';
            return;
        }

        courses.forEach(course => {
            // Создаем HTML карточки вручную (так же, как в ListCourses.cshtml)
            const cardHtml = `
                <div class="course-item">
                    <div class="course-img">
                        <img src="${course.image}" alt="${course.name}" />
                    </div>
                    <div class="course-details">
                        <h3>${course.name}</h3>
                        <div class="course-meta">
                            <span class="level-badge">${course.level}</span>
                            <span class="rating">⭐ ${course.rating}</span>
                        </div>
                        <p class="course-desc">${course.description}</p>
                        <div class="course-footer">
                            <span class="price">${course.price.toLocaleString('ru-RU', { style: 'currency', currency: 'RUB' })}</span>
                            <button class="button">Подробнее</button>
                        </div>
                    </div>
                </div>
            `;
            container.insertAdjacentHTML('beforeend', cardHtml);
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
            // Сначала пробуем прочитать JSON с ошибкой
            return response.json()
                .catch(() => {
                    // Если JSON не прочитался (сервер вернул просто текст или пустоту)
                    throw new Error('Ошибка сервера ' + response.status);
                })
                .then(errorData => {
                    // Если JSON прочитался, бросаем его как ошибку, чтобы поймать в основном коде
                    throw errorData;
                });
        }
        return response.text().then(text => text ? JSON.parse(text) : {});
    });
}
function displayErrors(errors, container, form) {
    console.log('Отображаю ошибки:', errors); // ЭТО ПОКАЖЕТ ОШИБКУ В КОНСОЛИ (F12)

    if (!container) return;
    container.innerHTML = '';

    if (form) {
        const inputs = form.querySelectorAll('input');
        inputs.forEach(input => input.classList.remove('input-error'));
    }

    if (errors.description) {
        const div = document.createElement('div');
        div.className = 'error-message'; // Убедитесь, что тут error-message
        div.textContent = errors.description;
        container.appendChild(div);
    }

    if (Array.isArray(errors)) {
        errors.forEach(e => {
            const div = document.createElement('div');
            div.className = 'error-message'; // И тут error-message
            div.textContent = e.message || e;
            container.appendChild(div);
        });
    }

}
// --- GOOGLE LOGIN ---
const googleLoginBtn = document.getElementById('google-login-btn');
if (googleLoginBtn) {
    googleLoginBtn.addEventListener('click', (e) => {
        e.preventDefault();
        // Просто переходим по ссылке на метод контроллера
        window.location.href = '/Home/AuthenticationGoogle';
    });
}