/**
 * Enhanced Loading Bar Integration
 * This script extends the basic loading bar to handle AJAX requests, form submissions, and other async operations
 */

$(document).ready(function() {
    // Ensure loading bar is available
    if (typeof window.pageLoadingBar === 'undefined') {
        console.warn('Loading bar not initialized');
        return;
    }

    const loadingBar = window.pageLoadingBar;

    // Handle AJAX requests
    if (typeof $ !== 'undefined') {
        // Show loading bar on AJAX start
        $(document).ajaxStart(function() {
            loadingBar.start();
        });

        // Hide loading bar on AJAX complete
        $(document).ajaxComplete(function() {
            loadingBar.complete();
        });

        // Handle AJAX errors
        $(document).ajaxError(function() {
            loadingBar.complete();
        });
    }

    // Handle form submissions - but only if validation passes
    $('form').on('submit', function(e) {
        const form = $(this);
        
        // Don't start loading bar immediately - let form validation run first
        // The individual form handlers will start the loading bar if validation passes
        setTimeout(function() {
            // Only start loading bar if the form submission wasn't prevented
            if (!e.isDefaultPrevented() && !form.data('validation-failed')) {
                loadingBar.start();
            }
        }, 10);
    });

    // Handle link clicks that might cause page navigation
    $('a[href]:not([href^="#"]):not([href^="javascript:"]):not([target="_blank"])').on('click', function(e) {
        // Only show loading for same-origin links
        try {
            const link = new URL(this.href, window.location.origin);
            if (link.origin === window.location.origin) {
                loadingBar.start();
            }
        } catch (ex) {
            // Handle invalid URLs gracefully
            console.warn('Invalid URL:', this.href);
        }
    });

    // Handle programmatic navigation (for SPAs)
    if (window.history && window.history.pushState) {
        const originalReplaceState = window.history.replaceState;
        window.history.replaceState = function(...args) {
            originalReplaceState.apply(window.history, args);
            loadingBar.start();
            setTimeout(() => loadingBar.complete(), 100);
        };
    }

    // Global function to manually control loading bar
    window.showLoadingBar = function() {
        loadingBar.start();
    };

    window.hideLoadingBar = function() {
        loadingBar.complete();
    };

    // Helper function for async operations
    window.withLoadingBar = async function(asyncFunction) {
        loadingBar.start();
        try {
            const result = await asyncFunction();
            return result;
        } finally {
            loadingBar.complete();
        }
    };
});

// Additional features for modern browsers
if ('IntersectionObserver' in window) {
    // Show loading when lazy-loaded content is being fetched
    document.addEventListener('DOMContentLoaded', function() {
        const lazyElements = document.querySelectorAll('[data-lazy], [loading="lazy"]');
        if (lazyElements.length > 0) {
            const observer = new IntersectionObserver(function(entries) {
                entries.forEach(function(entry) {
                    if (entry.isIntersecting && window.pageLoadingBar) {
                        window.pageLoadingBar.start();
                        setTimeout(() => window.pageLoadingBar.complete(), 500);
                    }
                });
            });

            lazyElements.forEach(function(element) {
                observer.observe(element);
            });
        }
    });
}

// Handle browser back/forward buttons
window.addEventListener('popstate', function() {
    if (window.pageLoadingBar) {
        window.pageLoadingBar.start();
        setTimeout(() => window.pageLoadingBar.complete(), 200);
    }
});