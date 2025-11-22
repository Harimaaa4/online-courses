// home_script.js
document.addEventListener('DOMContentLoaded', function () {
    const form = document.querySelector('.contact-form');
    if (!form) return;

    // Показать сообщение об успехе (если есть)
    if (window.location.search.includes('success')) {
        const msg = document.createElement('div');
        msg.className = 'success-message';
        msg.textContent = 'Спасибо! Ваше сообщение отправлено.';
        form.before(msg);
    }
});