// تأثيرات تفاعلية مع دعم كامل - إصدار محسن
function initializeIconEffects() {
    const icons = document.querySelectorAll('.icon-bg');

    console.log('تم العثور على أيقونات:', icons.length); // للت debugging

    function applyHoverEffects(element) {
        element.style.transform = 'translateY(-5px) scale(1.1)';
        element.style.boxShadow = '0 10px 20px rgba(0,0,0,0.2)';
        element.style.transition = 'all 0.3s ease';
    }

    function removeHoverEffects(element) {
        element.style.transform = 'translateY(0) scale(1)';
        element.style.boxShadow = 'none';
    }

    icons.forEach(icon => {
        // تأثيرات سطح المكتب
        icon.addEventListener('mouseenter', () => applyHoverEffects(icon));
        icon.addEventListener('mouseleave', () => removeHoverEffects(icon));

        // تأثيرات اللمس للأجهزة المحمولة
        icon.addEventListener('touchstart', function (e) {
            e.preventDefault();
            applyHoverEffects(this);
        });

        icon.addEventListener('touchend', function (e) {
            e.preventDefault();
            removeHoverEffects(this);
        });

        icon.addEventListener('touchmove', function (e) {
            e.preventDefault();
        });
    });
}

// تهيئة عند تحميل الصفحة
document.addEventListener('DOMContentLoaded', function () {
    initializeIconEffects();
});

// إعادة التهيئة عند حدوث تغييرات في DOM (مهم للصفحات الديناميكية)
const observer = new MutationObserver(function (mutations) {
    mutations.forEach(function (mutation) {
        if (mutation.addedNodes.length) {
            initializeIconEffects();
        }
    });
});

observer.observe(document.body, {
    childList: true,
    subtree: true
});

// تصدير الدالة للاستخدام في ملفات أخرى
window.initializeIconEffects = initializeIconEffects;