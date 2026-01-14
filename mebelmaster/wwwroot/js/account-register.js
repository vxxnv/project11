// Динамическая валидация формы регистрации
document.addEventListener('DOMContentLoaded', function () {
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const emailInput = this.querySelector('input[name="Input_Email"]');
            const passwordInput = this.querySelector('input[name="Input_Password"]');
            const confirmPasswordInput = this.querySelector('input[name="Input_ConfirmPassword"]');
            const errorDiv = document.getElementById('customErrors');
            const errorList = document.getElementById('errorList');

            const email = emailInput ? emailInput.value.trim() : '';
            const password = passwordInput ? passwordInput.value.trim() : '';
            const confirmPassword = confirmPasswordInput ? confirmPasswordInput.value.trim() : '';

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
            } else if (password.length < 6) {
                errors.push('• Пароль должен содержать минимум 6 символов');
            }

            // Проверка подтверждения пароля
            if (!confirmPassword) {
                errors.push('• Поле Подтверждение пароля обязательно для заполнения');
            } else if (password !== confirmPassword) {
                errors.push('• Пароли не совпадают');
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

    // Динамические подсказки
    function updateEmailHelp(input) {
        const helpText = document.getElementById('emailHelp');
        if (!input.value) {
            helpText.textContent = 'Введите ваш email адрес';
            helpText.className = 'form-text text-muted small';
        } else if (!validateEmail(input.value)) {
            helpText.textContent = 'Введите корректный email адрес';
            helpText.className = 'form-text text-danger small';
        } else {
            helpText.textContent = 'Email корректен';
            helpText.className = 'form-text text-success small';
        }
    }

    function updatePasswordHelp(input) {
        const helpText = document.getElementById('passwordHelp');
        if (!input.value) {
            helpText.textContent = 'Минимум 6 символов';
            helpText.className = 'form-text text-muted small';
        } else if (input.value.length < 6) {
            helpText.textContent = 'Пароль слишком короткий (минимум 6 символов)';
            helpText.className = 'form-text text-danger small';
        } else {
            helpText.textContent = 'Пароль надежный';
            helpText.className = 'form-text text-success small';
        }
    }

    function updateConfirmPasswordHelp(input) {
        const passwordInput = document.querySelector('input[name="Input_Password"]');
        const password = passwordInput ? passwordInput.value : '';
        const helpText = document.getElementById('confirmPasswordHelp');

        if (!input.value) {
            helpText.textContent = 'Пароли должны совпадать';
            helpText.className = 'form-text text-muted small';
        } else if (input.value !== password) {
            helpText.textContent = 'Пароли не совпадают';
            helpText.className = 'form-text text-danger small';
        } else {
            helpText.textContent = 'Пароли совпадают';
            helpText.className = 'form-text text-success small';
        }
    }

    // Назначаем обработчики событий
    const emailInput = document.querySelector('input[name="Input_Email"]');
    const passwordInput = document.querySelector('input[name="Input_Password"]');
    const confirmPasswordInput = document.querySelector('input[name="Input_ConfirmPassword"]');

    if (emailInput) {
        emailInput.addEventListener('input', function () {
            updateEmailHelp(this);
        });
    }

    if (passwordInput) {
        passwordInput.addEventListener('input', function () {
            updatePasswordHelp(this);
        });
    }

    if (confirmPasswordInput) {
        confirmPasswordInput.addEventListener('input', function () {
            updateConfirmPasswordHelp(this);
        });
    }

    // Подсветка активных полей
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