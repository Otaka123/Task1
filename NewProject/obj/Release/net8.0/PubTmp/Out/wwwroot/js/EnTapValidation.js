// wwwroot/js/englishTabValidation.js

/**
 * دالة مبسطة للتحقق من تبويب الإنجليزية
 * @param {string} formId - ID الخاص بالـ form
 * @param {string} englishTabId - ID الخاص بتبويب الإنجليزية
 * @param {Object} options - خيارات إضافية
 */
function initializeEnglishTabValidation(formId, englishTabId, options = {}) {
    const config = {
        requiredMessage: 'هذا الحقل مطلوب عند ملء أي حقل في التبويب الإنجليزي',
        notificationMessage: 'لقد قمت بملء أحد الحقول في التبويب الإنجليزي، أصبحت جميع الحقول مطلوبة الآن.',
        enableAllFields: true,
        autoSwitchTab: true,
        ...options
    };

    const $form = $(`#${formId}`);
    const $englishTab = $(`#${englishTabId}`);
    const $englishFields = $englishTab.find('input, textarea, select').not('[type="hidden"]');

    let validationEnabled = false;
    let notificationShown = false;

    // التحقق إذا كان أي حقل إنجليزي مملوء
    function checkAnyFieldFilled() {
        return $englishFields.toArray().some(field => {
            const value = $(field).val();
            return value && value.trim() !== '';
        });
    }

    // التحقق إذا كانت جميع الحقول الإنجليزية مملوءة
    function checkAllFieldsFilled() {
        return $englishFields.toArray().every(field => {
            const value = $(field).val();
            return value && value.trim() !== '';
        });
    }

    // تحديث التحقق
    function updateValidation() {
        const shouldValidate = checkAnyFieldFilled();
        const allFilled = checkAllFieldsFilled();

        if (shouldValidate && !validationEnabled) {
            enableValidation();
        } else if (!shouldValidate && validationEnabled) {
            disableValidation();
        }

        // إخفاء الإشعار إذا كانت جميع الحقول مملوءة
        if (allFilled && notificationShown) {
            hideNotification();
        }
    }

    // تفعيل التحقق
    function enableValidation() {
        $englishFields.each(function () {
            $(this).rules('add', {
                required: true,
                messages: { required: config.requiredMessage }
            });
        });

        validationEnabled = true;

        if (config.autoSwitchTab && !checkAllFieldsFilled()) {
            switchToEnglishTab();
            showNotification();
        }
    }

    // إلغاء التحقق
    function disableValidation() {
        $englishFields.each(function () {
            $(this).rules('remove', 'required');
            clearFieldError($(this));
        });

        validationEnabled = false;
        hideNotification();
    }

    // إزالة رسالة الخطأ من الحقل
    function clearFieldError($field) {
        $field.removeClass('is-invalid is-valid');

        // البحث عن عنصر الخطأ بطرق مختلفة
        const $errorElement = $field.next('.field-validation-error')
            || $field.next('.text-danger')
            || $field.siblings('.field-validation-error')
            || $field.siblings('.text-danger');

        if ($errorElement.length) {
            $errorElement.text('');
            $errorElement.hide();
        }
    }

    // إظهار رسالة الخطأ للحقل
    function showFieldError($field, message) {
        $field.addClass('is-invalid');
        $field.removeClass('is-valid');

        const $errorElement = $field.next('.field-validation-error')
            || $field.next('.text-danger')
            || $field.siblings('.field-validation-error')
            || $field.siblings('.text-danger');

        if ($errorElement.length) {
            $errorElement.text(message);
            $errorElement.show();
        }
    }

    // التبديل إلى تبويب الإنجليزية
    function switchToEnglishTab() {
        const $tabLink = $(`a[href="#${englishTabId}"]`);
        $tabLink.tab('show');
    }

    // إظهار الإشعار
    function showNotification() {
        hideNotification();

        const notification = $(`
            <div class="english-tab-notification mb-3">
                <div class="d-flex align-items-center text-danger">
                    *
                    <span class"m-2">${config.notificationMessage}</span>
                </div>
            </div>
        `);

        $englishTab.prepend(notification.hide().fadeIn(300));
        notificationShown = true;
    }

    // إخفاء الإشعار
    function hideNotification() {
        $('.english-tab-notification').fadeOut(300, function () {
            $(this).remove();
            notificationShown = false;
        });
    }

    // التحقق من الحقل عند الكتابة
    function validateFieldOnInput($field) {
        if (!validationEnabled) return;

        const value = $field.val().trim();

        if (value === '') {
            showFieldError($field, config.requiredMessage);
        } else {
            clearFieldError($field);
            $field.addClass('is-valid');
        }
    }

    // الأحداث
    $englishFields.on('input change', function () {
        const $field = $(this);
        updateValidation();

        if (validationEnabled) {
            validateFieldOnInput($field);
        }
    });

    // حدث keyup محسن لإزالة رسائل الخطأ فوراً
    $englishFields.on('keyup', function () {
        const $field = $(this);
        const value = $field.val().trim();

        if (validationEnabled && value !== '') {
            clearFieldError($field);
            $field.addClass('is-valid');
        }
    });

    // التحقق قبل الإرسال
    $form.on('submit', function (e) {
        updateValidation();

        if (validationEnabled) {
            let isValid = true;

            $englishFields.each(function () {
                const $field = $(this);
                const value = $field.val().trim();

                if (value === '') {
                    isValid = false;
                    showFieldError($field, config.requiredMessage);
                } else {
                    clearFieldError($field);
                    $field.addClass('is-valid');
                }
            });

            if (!isValid) {
                if (config.autoSwitchTab) {
                    switchToEnglishTab();
                    if (!notificationShown) showNotification();

                    // التمرير لأول حقل به خطأ
                    const $firstError = $englishTab.find('.is-invalid').first();
                    if ($firstError.length) {
                        $('html, body').animate({
                            scrollTop: $firstError.offset().top - 100
                        }, 500);
                        $firstError.focus();
                    }
                }
                e.preventDefault();
                return false;
            }
        }

        return true;
    });

    // التهيئة
    $(document).ready(() => {
        setTimeout(updateValidation, 100);
    });

    // الواجهة العامة
    return {
        updateValidation,
        isValidationEnabled: () => validationEnabled,
        getEnglishFields: () => $englishFields,
        showNotification,
        hideNotification,
        enableValidation,
        disableValidation
    };
}

// دالة مساعدة للتهيئة السريعة
window.setupEnglishTabValidation = function (formId, englishTabId, options = {}) {
    return initializeEnglishTabValidation(formId, englishTabId, options);
};