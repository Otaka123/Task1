// Toastr configuration and handler
document.addEventListener('DOMContentLoaded', function () {
    // Configure Toastr
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };

    // Handle messages from TempData
    const toastrMessages = document.getElementById('toastr-messages');
    if (toastrMessages) {
        const messages = JSON.parse(toastrMessages.textContent);
        messages.forEach(message => {
            showToast(message.type, message.text);
        });
    }

    // Handle inline messages
    const successMessage = document.getElementById('success-message')?.textContent;
    const errorMessage = document.getElementById('error-message')?.textContent;
    const warningMessage = document.getElementById('warning-message')?.textContent;
    const infoMessage = document.getElementById('info-message')?.textContent;

    if (successMessage) showToast('success', successMessage);
    if (errorMessage) showToast('error', errorMessage);
    if (warningMessage) showToast('warning', warningMessage);
    if (infoMessage) showToast('info', infoMessage);
});

function showToast(type, message) {
    if (!message) return;

    switch (type) {
        case 'success':
            toastr.success(message);
            break;
        case 'error':
            toastr.error(message);
            break;
        case 'warning':
            toastr.warning(message);
            break;
        case 'info':
            toastr.info(message);
            break;
        default:
            toastr.info(message);
    }
}
function initializeToasts() {
    if (typeof toastr === 'undefined') {
        console.warn('Toastr library is not loaded!');
        return;
    }

    // إعدادات Toastr
    toastr.options = {
        positionClass: "toast-top-center",
        closeButton: true,
        progressBar: true,
        timeOut: 5000,
        extendedTimeOut: 2000,
        preventDuplicates: true,
        newestOnTop: true,
        rtl: @(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft ? "true" : "false")
    };
// Global function to show toast from anywhere
window.showToastMessage = function (type, message) {
    showToast(type, message);
};
    document.ready(function () {
        initializeToasts();
    };