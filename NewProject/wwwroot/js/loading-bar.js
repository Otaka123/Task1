/**
 * Page Loading Bar Utility
 * Provides a smooth loading bar at the top of the page
 */

class PageLoadingBar {
    constructor(options = {}) {
        this.options = {
            autoStart: true,
            hideDelay: 500,
            minDuration: 500,
            element: null,
            className: 'loading-bar',
            ...options
        };
        
        this.isLoading = false;
        this.startTime = null;
        this.element = null;
        
        this.init();
    }
    
    init() {
        // Create loading bar element if not provided
        if (!this.options.element) {
            this.createLoadingBar();
        } else {
            this.element = this.options.element;
        }
        
        // Auto-start loading if enabled
        if (this.options.autoStart) {
            this.start();
        }
        
        // Set up event listeners
        this.setupEventListeners();
    }
    
    createLoadingBar() {
        // Remove existing loading bar if any
        const existing = document.querySelector('.' + this.options.className);
        if (existing) {
            existing.remove();
        }
        
        // Create new loading bar
        this.element = document.createElement('div');
        this.element.className = this.options.className;
        this.element.innerHTML = '<div class="loading-bar-progress"></div>';
        
        // Insert at the beginning of body
        document.body.insertBefore(this.element, document.body.firstChild);
    }
    
    setupEventListeners() {
        // Start loading when page begins loading
        if (document.readyState === 'loading') {
            this.start();
        }
        
        // Complete loading when page is fully loaded
        if (document.readyState === 'complete') {
            this.complete();
        } else {
            window.addEventListener('load', () => {
                this.complete();
            });
        }
        
        // Handle navigation events for single page applications
        window.addEventListener('beforeunload', () => {
            this.start();
        });
        
        // Handle hash changes
        window.addEventListener('hashchange', () => {
            this.start();
            setTimeout(() => this.complete(), 100);
        });
        
        // Handle history changes (for SPAs)
        if (window.history && window.history.pushState) {
            const originalPushState = window.history.pushState;
            window.history.pushState = function(...args) {
                originalPushState.apply(window.history, args);
                if (window.pageLoadingBar) {
                    window.pageLoadingBar.start();
                    setTimeout(() => window.pageLoadingBar.complete(), 100);
                }
            };
        }
    }
    
    start() {
        if (this.isLoading) return;
        
        this.isLoading = true;
        this.startTime = Date.now();
        
        if (this.element) {
            this.element.classList.remove('hidden', 'complete');
            this.element.classList.add('loading');
        }
    }
    
    complete() {
        if (!this.isLoading) return;
        
        const elapsed = Date.now() - this.startTime;
        const remainingTime = Math.max(0, this.options.minDuration - elapsed);
        
        setTimeout(() => {
            if (this.element) {
                this.element.classList.remove('loading');
                this.element.classList.add('complete');
                
                // Hide after delay
                setTimeout(() => {
                    if (this.element) {
                        this.element.classList.add('hidden');
                    }
                    this.isLoading = false;
                }, this.options.hideDelay);
            }
        }, remainingTime);
    }
    
    reset() {
        this.isLoading = false;
        if (this.element) {
            this.element.classList.remove('loading', 'complete', 'hidden');
        }
    }
    
    destroy() {
        if (this.element && this.element.parentNode) {
            this.element.parentNode.removeChild(this.element);
        }
        this.element = null;
        this.isLoading = false;
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Only initialize if not already exists
    if (!window.pageLoadingBar) {
        window.pageLoadingBar = new PageLoadingBar({
            autoStart: document.readyState === 'loading',
            minDuration: 300,
            hideDelay: 300
        });
    }
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = PageLoadingBar;
}

// Also make it available globally
window.PageLoadingBar = PageLoadingBar;