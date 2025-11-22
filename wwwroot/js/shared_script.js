document.addEventListener('DOMContentLoaded', function () {

    // =========================================
    //   1. КОД ДЛЯ АНИМАЦИИ ШАПКИ
    // =========================================
    window.addEventListener('scroll', function () {
        var header = document.getElementById('header-top');
        var maxScroll = 250; // Порог прокрутки

        if (scrollY > maxScroll) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }
    });

    // =========================================
    //   2. КОД ДЛЯ МОДАЛЬНЫХ ОКОН (ГЛАВА 2)
    // =========================================

    // Находим все нужные элементы
    const modalOverlay = document.getElementById('modal-overlay');
    const loginModal = document.getElementById('login-modal');
    const registerModal = document.getElementById('register-modal');

    // !!! ВАЖНО: Эти переменные будут переиспользованы в коде Главы 9
    const openLoginBtn = document.getElementById('open-login-modal');
    const openRegisterBtn = document.getElementById('open-register-modal');

    const closeBtns = document.querySelectorAll('[data-close-modal]');
    const switchBtns = document.querySelectorAll('[data-switch-modal]');

    // Проверяем, что все элементы существуют, чтобы не было ошибок
    if (modalOverlay && loginModal && registerModal && openLoginBtn && openRegisterBtn) {

        // --- ФУНКЦИИ ---
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
        // Функция ПЕРЕКЛЮЧЕНИЯ (например, с Логина на Регистрацию)
        // ОБНОВЛЕНО в Шаге 5 для плавной анимации
        function switchModal(targetModalId) {
            const targetModal = document.getElementById(targetModalId);
            if (!targetModal) return;

            // 1. Находим текущую видимую модалку
            const currentModal = targetModalId === 'login-modal'
                ? registerModal
                : loginModal;

            // 2. Находим формы внутри них
            const currentForm = currentModal.querySelector('.modal-form');
            const targetForm = targetModal.querySelector('.modal-form');

            // 3. Плавно прячем текущую форму
            currentForm.classList.add('form-hidden');
            currentForm.classList.remove('form-visible');

            // 4. Показываем контейнер новой модалки (но форма пока скрыта)
            currentModal.classList.remove('active');
            targetModal.classList.add('active');

            // 5. Готовим новую форму к "выезду"
            targetForm.classList.add('form-before-enter');
            targetForm.classList.remove('form-hidden');

            // 6. Ждем 100мс (чтобы .form-before-enter успел примениться)
            setTimeout(() => {
                // А теперь "включаем" анимацию появления
                targetForm.classList.add('form-visible');
                targetForm.classList.remove('form-before-enter');
            }, 100);

            // 7. Сбрасываем классы у старой формы (на случай, если мы вернемся)
            setTimeout(() => {
                currentForm.classList.remove('form-hidden');
            }, 400); // 400мс = 0.3с анимация + 0.1с задержка
        }

        // --- СЛУШАТЕЛИ СОБЫТИЙ ---
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
            if (e.target === modalOverlay) {
                closeAllModals();
            }
        });
    }
    // --- КОНЕЦ КОДА ГЛАВЫ 2 ---


    // =========================================
    //   ↓↓↓ ВОТ ПРАВИЛЬНОЕ МЕСТО ДЛЯ КОДА ГЛАВЫ 9 ↓↓↓
    // =========================================
    const hamburger = document.getElementById('hamburger-menu');
    const sideMenu = document.getElementById('side-menu');

    if (hamburger && sideMenu) {
        hamburger.addEventListener('click', () => {
            // Переключаем класс .active у иконки
            hamburger.classList.toggle('active');
            // Переключаем класс .active у меню
            sideMenu.classList.toggle('active');
        });
    }

    // Код для кнопок в боковом меню
    const sideLoginBtn = document.getElementById('side-open-login');
    const sideRegisterBtn = document.getElementById('side-open-register');

    // Переиспользуем переменные openLoginBtn и openRegisterBtn,
    // которые были найдены в блоке "КОД ДЛЯ МОДАЛЬНЫХ ОКОН"
    if (sideLoginBtn && openLoginBtn) {
        sideLoginBtn.addEventListener('click', () => {
            // Имитируем клик по главной кнопке "Войти"
            openLoginBtn.click();
            // И сразу закрываем боковое меню
            hamburger.classList.remove('active');
            sideMenu.classList.remove('active');
        });
    }
    if (sideRegisterBtn && openRegisterBtn) {
        sideRegisterBtn.addEventListener('click', () => {
            // Имитируем клик по главной кнопке "Регистрация"
            openRegisterBtn.click();
            // И сразу закрываем боковое меню
            hamburger.classList.remove('active');
            sideMenu.classList.remove('active');
        });
    }
    // --- КОНЕЦ КОДА ГЛАВЫ 9 ---

    // =========================================
    //   ↓↓↓ НОВЫЙ КОД - ПЕРЕКЛЮЧАТЕЛЬ ТЕМЫ ↓↓↓
    // =========================================
    const themeToggle = document.getElementById('theme-toggle');
    if (themeToggle) {
        // Функция для обновления иконок
        function updateThemeIcons() {
            const isDark = document.body.classList.contains('dark-mode');
            themeToggle.querySelector('.icon-sun').style.display = isDark ? 'none' : 'inline';
            themeToggle.querySelector('.icon-moon').style.display = isDark ? 'inline' : 'none';
        }

        // Слушатель клика
        themeToggle.addEventListener('click', () => {
            document.body.classList.toggle('dark-mode');

            // Сохраняем выбор в localStorage
            const isDark = document.body.classList.contains('dark-mode');
            localStorage.setItem('theme', isDark ? 'dark' : 'light');

            // Обновляем иконки
            updateThemeIcons();
        });

        // Устанавливаем правильную иконку при загрузке
        updateThemeIcons();
    }

    // =========================================
    //   ↓↓↓ НОВЫЙ КОД (ШАГ 3) - АВТО-ПРОКРУТКА КАРУСЕЛИ ↓↓↓
    // =========================================
    const carouselWrapper = document.querySelector('.services-wrapper');

    // Проверяем, что мы на странице, где есть эта карусель
    if (carouselWrapper) {
        let scrollInterval;
        const scrollAmount = 1; // Прокручивать по 1px для плавности
        const scrollSpeed = 30; // 30ms - хорошая, плавная скорость
        let originalWidth = 0; // Ширина оригинального контента

        // --- 1. Логика клонирования ---
        function setupInfiniteScroll() {
            // Сохраняем оригинальную ширину ДО клонирования
            // (scrollWidth - это полная ширина контента)
            originalWidth = carouselWrapper.scrollWidth;

            // Находим все оригинальные карточки
            const originalCards = Array.from(carouselWrapper.children);

            // Клонируем каждую карточку и добавляем в конец
            originalCards.forEach(card => {
                const clone = card.cloneNode(true);
                carouselWrapper.appendChild(clone);
            });
        }

        // --- 2. Логика авто-прокрутки ---
        function startScrolling() {
            clearInterval(scrollInterval);

            scrollInterval = setInterval(() => {

                // Проверяем, дошли ли мы до "шва" (начала клонированного контента)
                if (carouselWrapper.scrollLeft >= originalWidth) {

                    // Мы у "шва". 
                    // Мгновенно перескакиваем назад на (текущая позиция - ширина)
                    // Это создает иллюзию бесконечности, т.к. 
                    // (например) 1001px - 1000px = 1px.
                    // Визуально клон A (в 1001px) заменяется на оригинал A (в 1px).
                    carouselWrapper.scrollLeft = carouselWrapper.scrollLeft - originalWidth;

                } else {
                    // Мы не у шва. Просто крутим дальше.
                    carouselWrapper.scrollLeft += scrollAmount;
                }
            }, scrollSpeed);
        }

        function stopScrolling() {
            clearInterval(scrollInterval);
        }

        // --- 3. Запуск ---

        // Сначала клонируем контент
        setupInfiniteScroll();

        // Потом запускаем прокрутку
        startScrolling();

        // Ставим на паузу при наведении мыши
        carouselWrapper.addEventListener('mouseenter', stopScrolling);

        // Возобновляем, когда мышь ушла
        carouselWrapper.addEventListener('mouseleave', startScrolling);

        // Также остановим, если пользователь сам нажал на стрелки
        const arrows = document.querySelectorAll('.arrow');
        if (arrows) {
            arrows.forEach(arrow => {
                arrow.addEventListener('click', stopScrolling);
            });
        }
    }
    // --- КОНЕЦ КОДА ШАГА 3 ---

    // =========================================
    //   3. КОД ДЛЯ FETCH-ЗАПРОСОВ (ГЛАВА 8)
    // =========================================

    // --- Обработчик Входа (Рис. 83, 86, 89) ---
    const loginSubmitBtn = document.getElementById('login-submit-btn');
    if (loginSubmitBtn) {
        loginSubmitBtn.addEventListener('click', () => {
            const requestURL = '/Account/Login';
            const errorContainer = document.getElementById('login-error-container');
            const body = {
                email: document.getElementById('login-email').value,
                password: document.getElementById('login-password').value
            };
            errorContainer.innerHTML = ''; // Очищаем старые ошибки

            sendRequest('POST', requestURL, body)
                .then(data => {
                    console.log('Успешный вход:', data);
                    location.reload(); // Перезагружаем страницу
                })
                .catch(err => {
                    console.log('Ошибка входа:', err);
                    displayErrors(err, errorContainer); // Показываем ошибки
                });
        });
    }

    // --- Обработчик Регистрации (Рис. 91) ---
    const registerSubmitBtn = document.getElementById('register-submit-btn');
    if (registerSubmitBtn) {
        registerSubmitBtn.addEventListener('click', () => {
            const requestURL = '/Account/Register';
            const errorContainer = document.getElementById('register-error-container');
            const body = {
                login: document.getElementById('register-login').value,
                email: document.getElementById('register-email').value,
                password: document.getElementById('register-password').value,
                passwordConfirm: document.getElementById('register-passwordConfirm').value
            };
            errorContainer.innerHTML = ''; // Очищаем старые ошибки

            sendRequest('POST', requestURL, body)
                .then(data => {
                    console.log('Успешная регистрация:', data);
                    location.reload(); // Перезагружаем страницу
                })
                .catch(err => {
                    console.log('Ошибка регистрации:', err);
                    displayErrors(err, errorContainer); // Показываем ошибки
                });
        });
    }
    // --- КОНЕЦ КОДА ГЛАВЫ 8 ---

}); 


// =========================================
//   ГЛОБАЛЬНЫЕ ФУНКЦИИ (ВНЕ DOMContentLoaded)
// =========================================

// --- КОД ДЛЯ СЛАЙДЕРА (ГЛАВА 5) ---
function scrollServices(direction) {
    const wrapper = document.getElementById('servicesWrapper');
    const scrollAmount = 350; // ширина карточки + отступ
    wrapper.scrollBy({
        left: direction * scrollAmount,
        behavior: 'smooth'
    });
}

// --- ФУНКЦИИ ИЗ ГЛАВЫ 8 (FETCH) ---

/**
 * (из Рисунка 84)
 * Отправляет fetch-запрос
 */
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
            // Если ответ не 200 (OK), получаем ошибки
            return response.json().then(errorData => {
                // Если нет JSON, создаем свою ошибку
                if (!errorData) throw new Error('Ошибка сети или сервер недоступен');
                throw errorData; // Бросаем ошибки для .catch()
            });
        }
        // Если все ок, но нет JSON, вернем пустой объект
        return response.text().then(text => text ? JSON.parse(text) : {});
    });
}


/**
 * (из Рисунка 87)
 * Отображает ошибки и подсвечивает поля
 */
function displayErrors(errors, errorContainer, formElement) {
    errorContainer.innerHTML = ''; // Очистить старые сообщения

    // Сначала убираем все старые подсветки
    const inputs = formElement.querySelectorAll('input');
    inputs.forEach(input => input.classList.remove('input-error'));

    if (Array.isArray(errors)) {
        errors.forEach(error => {
            // 1. Показываем сообщение
            const errorMessage = document.createElement('div');
            errorMessage.classList.add('error');
            errorMessage.textContent = error.message; // Используем .message
            errorContainer.appendChild(errorMessage);

            // 2. Подсвечиваем поле
            // Мы ищем по name, так как C# возвращает "Email", "Password"
            const fieldName = error.field.toLowerCase();
            const inputField = formElement.querySelector(`input[name="${fieldName}"]`);
            if (inputField) {
                inputField.classList.add('input-error');
            }
        });
    } else if (typeof errors === 'string') {
        // ... (код для одной ошибки, как и был) ...
    }
}