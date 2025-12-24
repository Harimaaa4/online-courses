document.addEventListener('DOMContentLoaded', function () {

    // Инициализация Bootstrap dropdowns (если используются в других местах)
    var dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'))
    var dropdownList = dropdownElementList.map(function (dropdownToggleEl) {
        return new bootstrap.Dropdown(dropdownToggleEl)
    })

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

    // 2. МОДАЛЬНЫЕ ОКНА
    const modalOverlay = document.getElementById('modal-overlay');
    const loginModal = document.getElementById('login-modal');
    const registerModal = document.getElementById('register-modal');
    const confirmModal = document.getElementById('confirm-email-modal');
    const openLoginBtn = document.getElementById('open-login-modal');
    const openRegisterBtn = document.getElementById('open-register-modal');
    const closeBtns = document.querySelectorAll('[data-close-modal]');
    const switchBtns = document.querySelectorAll('[data-switch-modal]');

    if (modalOverlay) {
        function openModal(modal) {
            if (!modal) return;
            modalOverlay.classList.add('active');
            modal.classList.add('active');
        }
        function closeAllModals() {
            modalOverlay.classList.remove('active');
            document.querySelectorAll('.auth-modal').forEach(m => m.classList.remove('active'));
        }
        function switchModal(targetModalId) {
            const targetModal = document.getElementById(targetModalId);
            if (!targetModal) return;
            document.querySelectorAll('.auth-modal.active').forEach(m => m.classList.remove('active'));
            targetModal.classList.add('active');
        }
        if (openLoginBtn) openLoginBtn.addEventListener('click', () => openModal(loginModal));
        if (openRegisterBtn) openRegisterBtn.addEventListener('click', () => openModal(registerModal));
        closeBtns.forEach(btn => btn.addEventListener('click', closeAllModals));
        modalOverlay.addEventListener('click', (e) => { if (e.target === modalOverlay) closeAllModals(); });
        switchBtns.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const targetId = e.target.dataset.switchModal;
                switchModal(targetId);
            });
        });
    }

    // 3. ГАМБУРГЕР И ТЕМА
    const hamburger = document.getElementById('hamburger-menu');
    const sideMenu = document.getElementById('side-menu');
    if (hamburger && sideMenu) {
        hamburger.addEventListener('click', () => { hamburger.classList.toggle('active'); sideMenu.classList.toggle('active'); });
    }
    const sideLoginBtn = document.getElementById('side-open-login');
    const sideRegisterBtn = document.getElementById('side-open-register');
    if (sideLoginBtn && openLoginBtn) sideLoginBtn.addEventListener('click', () => { openLoginBtn.click(); });
    if (sideRegisterBtn && openRegisterBtn) sideRegisterBtn.addEventListener('click', () => { openRegisterBtn.click(); });

    const themeToggle = document.getElementById('theme-toggle');
    if (themeToggle) {
        function updateThemeIcons() {
            const isDark = document.body.classList.contains('dark-mode');
            const sunIcon = themeToggle.querySelector('.icon-sun');
            const moonIcon = themeToggle.querySelector('.icon-moon');
            if (sunIcon) sunIcon.style.display = isDark ? 'none' : 'inline';
            if (moonIcon) moonIcon.style.display = isDark ? 'inline' : 'none';
        }
        if (localStorage.getItem('theme') === 'dark') document.body.classList.add('dark-mode');
        updateThemeIcons();
        themeToggle.addEventListener('click', () => {
            document.body.classList.toggle('dark-mode');
            localStorage.setItem('theme', document.body.classList.contains('dark-mode') ? 'dark' : 'light');
            updateThemeIcons();
        });
    }

    // 4. ЛОГИКА ФОРМ
    const loginSubmitBtn = document.getElementById('login-submit-btn');
    if (loginSubmitBtn) {
        loginSubmitBtn.addEventListener('click', (e) => {
            e.preventDefault();
            sendAuthRequest('/Home/Login', 'login');
        });
    }
    const registerSubmitBtn = document.getElementById('register-submit-btn');
    if (registerSubmitBtn) {
        registerSubmitBtn.addEventListener('click', (e) => {
            e.preventDefault();
            sendAuthRequest('/Home/Register', 'register');
        });
    }
    const confirmSubmitBtn = document.getElementById('confirm-submit-btn');
    if (confirmSubmitBtn) {
        confirmSubmitBtn.addEventListener('click', (e) => {
            e.preventDefault();
            sendAuthRequest('/Home/ConfirmEmail', 'confirm');
        });
    }

    function sendAuthRequest(url, type) {
        let body = {};
        let errorContainer;
        if (type === 'login') {
            body = { email: document.getElementById('login-email').value, password: document.getElementById('login-password').value };
            errorContainer = document.getElementById('login-error-container');
        } else if (type === 'register') {
            body = {
                login: document.getElementById('register-login').value, email: document.getElementById('register-email').value,
                password: document.getElementById('register-password').value, passwordConfirm: document.getElementById('register-passwordConfirm').value
            };
            errorContainer = document.getElementById('register-error-container');
        } else if (type === 'confirm') {
            body = {
                codeConfirm: document.getElementById('confirm-code').value, generatedCode: document.getElementById('confirm-generated-code').value,
                login: document.getElementById('confirm-login').value, email: document.getElementById('confirm-email').value, password: document.getElementById('confirm-password').value
            };
            errorContainer = document.getElementById('confirm-error-container');
        }
        if (errorContainer) errorContainer.innerHTML = '';

        sendRequest('POST', url, body)
            .then(data => {
                if (type === 'register') {
                    closeAllModals();
                    setTimeout(() => { modalOverlay.classList.add('active'); confirmModal.classList.add('active'); }, 50);
                    document.getElementById('confirm-login').value = data.login; document.getElementById('confirm-email').value = data.email;
                    document.getElementById('confirm-password').value = data.password; document.getElementById('confirm-generated-code').value = data.generatedCode;
                } else {
                    location.reload();
                }
            })
            .catch(err => {
                let formSelector = '#login-modal .auth-form';
                if (type === 'register') formSelector = '#register-modal .auth-form';
                if (type === 'confirm') formSelector = '#confirm-email-modal .auth-form';
                displayErrors(err, errorContainer, document.querySelector(formSelector));
            });
    }

    // 5. КАРУСЕЛЬ
    const carouselWrapper = document.querySelector('.services-wrapper');
    if (carouselWrapper) {
        window.addEventListener('load', () => {
            startInfiniteCarousel(carouselWrapper);
        });
    }

    function startInfiniteCarousel(wrapper) {
        const originalWidth = wrapper.scrollWidth;
        if (originalWidth <= wrapper.clientWidth) return;

        const cards = Array.from(wrapper.children);
        cards.forEach(c => wrapper.appendChild(c.cloneNode(true)));

        const speed = 1;
        const intervalTime = 20;
        let isPaused = false;

        function scroll() {
            if (!isPaused) {
                if (wrapper.scrollLeft >= originalWidth) {
                    wrapper.scrollLeft = 0;
                } else {
                    wrapper.scrollLeft += speed;
                }
            }
        }

        setInterval(scroll, intervalTime);

        wrapper.addEventListener('mouseenter', () => isPaused = true);
        wrapper.addEventListener('mouseleave', () => isPaused = false);
        wrapper.addEventListener('touchstart', () => isPaused = true);
        wrapper.addEventListener('touchend', () => isPaused = false);

        document.querySelectorAll('.arrow').forEach(arrow => {
            arrow.addEventListener('mouseenter', () => isPaused = true);
            arrow.addEventListener('mouseleave', () => isPaused = false);
        });
    }

    // 6. ФИЛЬТРАЦИЯ И ПОИСК
    const applyFiltersBtn = document.getElementById('apply-filters');
    const sortOrderSelect = document.getElementById('sort-order');
    const searchInput = document.getElementById('search-input');

    const priceMin = document.getElementById('price-min');
    const priceMax = document.getElementById('price-max');
    if (priceMin && priceMax) {
        priceMin.addEventListener('input', () => { document.getElementById('price-min-val').textContent = priceMin.value; });
        priceMax.addEventListener('input', () => { document.getElementById('price-max-val').textContent = priceMax.value; });
    }

    function filterCourses() {
        const minPrice = document.getElementById('price-min').value;
        const maxPrice = document.getElementById('price-max').value;
        const selectedLevels = [];
        document.querySelectorAll('.checkbox-container input[type="checkbox"]:checked').forEach(cb => selectedLevels.push(cb.value));
        const sortType = sortOrderSelect ? sortOrderSelect.value : 'default';
        const searchQuery = searchInput ? searchInput.value : '';

        const urlParams = new URLSearchParams(window.location.search);
        const categoryId = urlParams.get('categoryId');
        if (!categoryId) return;

        const filterData = {
            CategoryId: categoryId,
            MinPrice: parseFloat(minPrice),
            MaxPrice: parseFloat(maxPrice),
            Levels: selectedLevels,
            SortType: sortType,
            SearchQuery: searchQuery
        };

        const container = document.querySelector('.courses-list-container');
        if (container) container.style.opacity = '0.5';

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

    if (applyFiltersBtn) {
        applyFiltersBtn.addEventListener('click', (e) => { e.preventDefault(); filterCourses(); });
    }
    if (sortOrderSelect) {
        sortOrderSelect.addEventListener('change', () => filterCourses());
    }
    if (searchInput) {
        searchInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                filterCourses();
            }
        });
    }

    // Отрисовка курсов
    function renderCourses(courses) {
        const container = document.querySelector('.courses-list-container');
        if (!container) return;
        container.innerHTML = '';

        if (courses.length === 0) {
            container.innerHTML = '<p style="width:100%; text-align:center; grid-column: 1/-1;">Курсы не найдены.</p>';
            return;
        }

        courses.forEach(course => {
            const imagePath = course.image ? course.image.replace('~', '') : '';
            const priceFormatted = course.price.toLocaleString('ru-RU', { style: 'currency', currency: 'RUB' });

            const cardHtml = `
                <div class="course-item">
                    <div class="course-img"><img src="${imagePath}" alt="${course.name}" /></div>
                    <div class="course-details">
                        <h3>${course.name}</h3>
                        <div class="course-meta">
                            <span class="level-badge">${course.level}</span>
                            <span class="rating">⭐ ${course.rating}</span>
                        </div>
                        <p class="course-desc">${course.description}</p>
                        <div class="course-footer">
                            <span class="price">${priceFormatted}</span>
                            <a href="/Home/GetCourse/${course.id}" class="button">Подробнее</a>
                        </div>
                    </div>
                </div>`;
            container.insertAdjacentHTML('beforeend', cardHtml);
        });
    }

    // GOOGLE LOGIN
    const googleLoginBtn = document.getElementById('google-login-btn');
    if (googleLoginBtn) {
        googleLoginBtn.addEventListener('click', (e) => {
            e.preventDefault();
            window.location.href = '/Home/AuthenticationGoogle';
        });
    }
});

// =========================================
//   HELPERS (Глобальные функции)
// =========================================

function sendRequest(method, url, body = null) {
    const headers = { 'Content-Type': 'application/json' };
    return fetch(url, {
        method: method,
        body: body ? JSON.stringify(body) : null,
        headers: headers
    }).then(response => {
        if (!response.ok) {
            return response.json()
                .catch(() => response.text().then(text => {
                    throw new Error(text || `Ошибка сервера: ${response.status}`);
                }))
                .then(errObj => {
                    throw errObj;
                });
        }
        return response.text().then(text => text ? JSON.parse(text) : {});
    });
}

function displayErrors(errors, container, form) {
    if (!container) return;
    container.innerHTML = '';
    if (form) form.querySelectorAll('input').forEach(i => i.classList.remove('input-error'));

    if (errors.description) {
        const div = document.createElement('div');
        div.className = 'error-message';
        div.textContent = errors.description;
        container.appendChild(div);
    } else if (Array.isArray(errors)) {
        errors.forEach(e => {
            const div = document.createElement('div');
            div.className = 'error-message';
            div.textContent = e.message || e;
            container.appendChild(div);
            if (form && e.field) {
                const input = form.querySelector(`input[name="${e.field}"]`);
                if (input) input.classList.add('input-error');
            }
        });
    } else if (errors.message) {
        const div = document.createElement('div');
        div.className = 'error-message';
        div.textContent = errors.message;
        container.appendChild(div);
    } else if (typeof errors === 'string') {
        const div = document.createElement('div');
        div.className = 'error-message';
        div.textContent = errors;
        container.appendChild(div);
    }
}

// =========================================
//   ФУНКЦИИ ВНЕ DOMContentLoaded
// =========================================

// Функция для стрелок влево/вправо
window.scrollServices = function (direction) {
    const wrapper = document.querySelector('.services-wrapper');
    if (wrapper) {
        const scrollAmount = 330;
        wrapper.scrollBy({
            left: direction * scrollAmount,
            behavior: 'smooth'
        });
    }
};

// =========================================
//   МЕНЮ ПРОФИЛЯ (ПОЛНАЯ ЛОГИКА)
// =========================================

document.addEventListener('click', function (e) {
    const subMenu = document.getElementById("subMenu");
    const userIcon = document.getElementById("user-icon");

    // Если пользователь не авторизован (элементов нет на странице), выходим
    if (!subMenu || !userIcon) return;

    // 1. Если кликнули ТОЧНО ПО ИКОНКЕ (или её содержимому) -> Переключаем
    if (userIcon.contains(e.target)) {
        subMenu.classList.toggle("open-menu");
    }
    // 2. Если кликнули КУДА УГОДНО, но НЕ по меню -> Закрываем
    // (Условие !userIcon.contains(e.target) здесь уже выполняется автоматически благодаря else if)
    else if (!subMenu.contains(e.target)) {
        subMenu.classList.remove("open-menu");
    }
});