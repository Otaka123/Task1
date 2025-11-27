// toastr-config.js
function initializeToasts() {
    if (typeof toastr === 'undefined') {
        console.warn('Toastr library is not loaded!');
        return;
    }

    toastr.options = {
        positionClass: "toast-top-center",
        closeButton: true,
        progressBar: true,
        timeOut: 5000,
        extendedTimeOut: 2000,
        preventDuplicates: true,
        newestOnTop: true,
        rtl: document.dir === 'rtl'
    };

    function showTempDataMessage(type, message) {
        if (!message || typeof toastr === 'undefined') return;

        try {
            const cleanMessage = message
                .replace(/'/g, "\\'")
                .replace(/\r/g, '')
                .replace(/\n/g, ' <br />')
                .replace(/<script /gi, '&lt;script')
                .replace(/<\/script/gi, '&lt;/script');

            toastr[type](cleanMessage);
        } catch (error) {
            console.error('Error displaying toastr message:', error);
        }
    }

    // يمكنك تمرير الرسائل كمعاملات أو استخدام data attributes
}

$(document).ready(function () {
    initializeToasts();
});