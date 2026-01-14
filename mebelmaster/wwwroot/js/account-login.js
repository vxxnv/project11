// Динамическая валидация формы входа
document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const emailInput = this.querySelector('input[name="Input_Email"]');
            const passwordInput = this.querySelector('input[name="Input_Password"]');
            const errorDiv = document.getElementById('customErrors');
            const errorList = document.getElementById('errorList');

            const email = emailInput ? emailInput.value.trim() : '';
            const password = passwordInput ? passwordInput.value.trim() : '';

            // Очищаем предыдущие ошибки
            if (errorList) errorList.innerHTML = '';
            if (errorDiv) errorDiv.classList.add('d-none');

            const errors = [];

            // Проверка email
            if (!email) {
                errors.push('• Поле Email обязательно для заполнения');
            } else if (!validateEmail(email)) {
                errors.push('• Введите корректный email адрес');
            }

            // Проверка пароля
            if (!password) {
                errors.push('• Поле Пароль обязательно для заполнения');
            }

            // Показываем ошибки или отправляем форму
            if (errors.length > 0) {
                if (errorList && errorDiv) {
                    errors.forEach(function (error) {
                        const errorItem = document.createElement('span');
                        errorItem.className = 'd-block';
                        errorItem.textContent = error;
                        errorList.appendChild(errorItem);
                    });
                    errorDiv.classList.remove('d-none');
                }
            } else {
                this.submit();
            }
        });
    }

    function validateEmail(email) {
        // Простая валидация email без регулярных выражений
        if (!email) return false;

        var parts = email.split('@');
        if (parts.length !== 2) return false;

        var localPart = parts[0];
        var domainPart = parts[1];

        if (!localPart || !domainPart) return false;

        // Проверяем наличие точки в доменной части
        if (domainPart.indexOf('.') === -1) return false;

        // Проверяем что после последней точки есть хотя бы 2 символа
        var domainParts = domainPart.split('.');
        var tld = domainParts[domainParts.length - 1];
        if (tld.length < 2) return false;

        return true;
    }

    // Динамические подсказки при фокусе
    document.querySelectorAll('input').forEach(function (input) {
        input.addEventListener('focus', function () {
            const formText = this.parentElement.querySelector('.form-text');
            if (formText && formText.classList.contains('text-muted')) {
                formText.classList.add('text-primary');
            }
        });

        input.addEventListener('blur', function () {
            const formText = this.parentElement.querySelector('.form-text');
            if (formText && formText.classList.contains('text-primary')) {
                formText.classList.remove('text-primary');
            }
        });
    });
});