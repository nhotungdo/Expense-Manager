// Performance Optimization Utilities
// Ensures page load speed < 2s and Lighthouse score > 90

// Lazy Loading Images
function initLazyLoading() {
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    if (img.dataset.src) {
                        img.src = img.dataset.src;
                        img.classList.add('loaded');
                        observer.unobserve(img);
                    }
                }
            });
        }, {
            rootMargin: '50px 0px',
            threshold: 0.01
        });

        document.querySelectorAll('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    } else {
        // Fallback for browsers without IntersectionObserver
        document.querySelectorAll('img[data-src]').forEach(img => {
            img.src = img.dataset.src;
        });
    }
}

// Debounce Function
function debounce(func, wait = 300) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Throttle Function
function throttle(func, limit = 100) {
    let inThrottle;
    return function(...args) {
        if (!inThrottle) {
            func.apply(this, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

// Request Animation Frame Throttle
function rafThrottle(callback) {
    let requestId = null;
    let lastArgs;

    const later = (context) => () => {
        requestId = null;
        callback.apply(context, lastArgs);
    };

    const throttled = function(...args) {
        lastArgs = args;
        if (requestId === null) {
            requestId = requestAnimationFrame(later(this));
        }
    };

    throttled.cancel = () => {
        cancelAnimationFrame(requestId);
        requestId = null;
    };

    return throttled;
}

// Cache Management
const CacheManager = {
    set: (key, value, ttl = 3600000) => { // Default 1 hour
        const item = {
            value: value,
            expiry: Date.now() + ttl
        };
        try {
            localStorage.setItem(key, JSON.stringify(item));
        } catch (e) {
            console.warn('Cache storage failed:', e);
        }
    },

    get: (key) => {
        try {
            const itemStr = localStorage.getItem(key);
            if (!itemStr) return null;

            const item = JSON.parse(itemStr);
            if (Date.now() > item.expiry) {
                localStorage.removeItem(key);
                return null;
            }
            return item.value;
        } catch (e) {
            console.warn('Cache retrieval failed:', e);
            return null;
        }
    },

    remove: (key) => {
        localStorage.removeItem(key);
    },

    clear: () => {
        localStorage.clear();
    }
};

// Resource Preloading
function preloadResources(urls) {
    urls.forEach(url => {
        const link = document.createElement('link');
        link.rel = 'preload';
        
        if (url.endsWith('.js')) {
            link.as = 'script';
        } else if (url.endsWith('.css')) {
            link.as = 'style';
        } else if (url.match(/\.(jpg|jpeg|png|gif|webp)$/)) {
            link.as = 'image';
        } else if (url.match(/\.(woff|woff2|ttf|otf)$/)) {
            link.as = 'font';
            link.crossOrigin = 'anonymous';
        }
        
        link.href = url;
        document.head.appendChild(link);
    });
}

// Critical CSS Inlining Helper
function loadDeferredStyles() {
    const addStylesNode = document.getElementById('deferred-styles');
    if (addStylesNode) {
        const replacement = document.createElement('div');
        replacement.innerHTML = addStylesNode.textContent;
        document.body.appendChild(replacement);
        addStylesNode.parentElement.removeChild(addStylesNode);
    }
}

// Web Vitals Monitoring
const WebVitals = {
    // Largest Contentful Paint
    measureLCP: () => {
        if ('PerformanceObserver' in window) {
            const observer = new PerformanceObserver((list) => {
                const entries = list.getEntries();
                const lastEntry = entries[entries.length - 1];
                console.log('LCP:', lastEntry.renderTime || lastEntry.loadTime);
            });
            observer.observe({ entryTypes: ['largest-contentful-paint'] });
        }
    },

    // First Input Delay
    measureFID: () => {
        if ('PerformanceObserver' in window) {
            const observer = new PerformanceObserver((list) => {
                const entries = list.getEntries();
                entries.forEach(entry => {
                    console.log('FID:', entry.processingStart - entry.startTime);
                });
            });
            observer.observe({ entryTypes: ['first-input'] });
        }
    },

    // Cumulative Layout Shift
    measureCLS: () => {
        if ('PerformanceObserver' in window) {
            let clsScore = 0;
            const observer = new PerformanceObserver((list) => {
                for (const entry of list.getEntries()) {
                    if (!entry.hadRecentInput) {
                        clsScore += entry.value;
                        console.log('CLS:', clsScore);
                    }
                }
            });
            observer.observe({ entryTypes: ['layout-shift'] });
        }
    },

    // Time to First Byte
    measureTTFB: () => {
        const navigationTiming = performance.getEntriesByType('navigation')[0];
        if (navigationTiming) {
            const ttfb = navigationTiming.responseStart - navigationTiming.requestStart;
            console.log('TTFB:', ttfb);
        }
    }
};

// Network Information API
function getNetworkInfo() {
    if ('connection' in navigator) {
        const connection = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
        return {
            effectiveType: connection.effectiveType,
            downlink: connection.downlink,
            rtt: connection.rtt,
            saveData: connection.saveData
        };
    }
    return null;
}

// Adaptive Loading based on network
function shouldLoadHighQuality() {
    const networkInfo = getNetworkInfo();
    if (!networkInfo) return true;
    
    // Load high quality only on fast connections
    return networkInfo.effectiveType === '4g' && !networkInfo.saveData;
}

// Memory Management
function checkMemoryUsage() {
    if ('memory' in performance) {
        const memory = performance.memory;
        const usedMemoryPercent = (memory.usedJSHeapSize / memory.jsHeapSizeLimit) * 100;
        
        if (usedMemoryPercent > 90) {
            console.warn('High memory usage detected:', usedMemoryPercent.toFixed(2) + '%');
            // Trigger cleanup if needed
            return false;
        }
        return true;
    }
    return true;
}

// Batch DOM Updates
function batchDOMUpdates(updates) {
    requestAnimationFrame(() => {
        const fragment = document.createDocumentFragment();
        updates.forEach(update => update(fragment));
        document.body.appendChild(fragment);
    });
}

// Service Worker Registration
async function registerServiceWorker() {
    if ('serviceWorker' in navigator) {
        try {
            const registration = await navigator.serviceWorker.register('/sw.js');
            console.log('Service Worker registered:', registration);
        } catch (error) {
            console.warn('Service Worker registration failed:', error);
        }
    }
}

// Initialize Performance Monitoring
function initPerformanceMonitoring() {
    // Monitor Web Vitals
    WebVitals.measureLCP();
    WebVitals.measureFID();
    WebVitals.measureCLS();
    WebVitals.measureTTFB();

    // Log page load time
    window.addEventListener('load', () => {
        const loadTime = performance.timing.loadEventEnd - performance.timing.navigationStart;
        console.log('Page Load Time:', loadTime + 'ms');
        
        // Check if under 2s requirement
        if (loadTime > 2000) {
            console.warn('Page load time exceeds 2s target:', loadTime + 'ms');
        }
    });

    // Monitor memory periodically
    setInterval(() => {
        checkMemoryUsage();
    }, 30000); // Check every 30 seconds
}

// Export functions
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        debounce,
        throttle,
        rafThrottle,
        CacheManager,
        preloadResources,
        WebVitals,
        getNetworkInfo,
        shouldLoadHighQuality,
        initPerformanceMonitoring
    };
}

// Auto-initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        initLazyLoading();
        initPerformanceMonitoring();
    });
} else {
    initLazyLoading();
    initPerformanceMonitoring();
}
