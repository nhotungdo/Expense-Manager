/**
 * Theme Manager - Quản lý giao diện và màu sắc
 */

class ThemeManager {
    constructor() {
        this.currentTheme = 'light';
        this.currentPrimaryColor = '#10b981';
        this.init();
    }

    init() {
        // Load theme từ localStorage hoặc server
        this.loadTheme();
        
        // Listen cho system theme changes
        if (window.matchMedia) {
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
                if (this.currentTheme === 'auto') {
                    this.applyTheme('auto');
                }
            });
        }
    }

    loadTheme() {
        // Ưu tiên localStorage cho tốc độ
        const savedTheme = localStorage.getItem('theme') || 'light';
        const savedColor = localStorage.getItem('primaryColor') || '#10b981';
        
        this.currentTheme = savedTheme;
        this.currentPrimaryColor = savedColor;
        
        this.applyTheme(savedTheme);
        this.applyPrimaryColor(savedColor);
    }

    async saveTheme(theme) {
        this.currentTheme = theme;
        localStorage.setItem('theme', theme);
        this.applyTheme(theme);
        
        // Sync với server
        try {
            await fetch('/api/settings/theme', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ theme })
            });
        } catch (error) {
            console.error('Failed to save theme to server:', error);
        }
    }

    async savePrimaryColor(color) {
        this.currentPrimaryColor = color;
        localStorage.setItem('primaryColor', color);
        this.applyPrimaryColor(color);
        
        // Sync với server
        try {
            await fetch('/api/settings/primary-color', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ color })
            });
        } catch (error) {
            console.error('Failed to save primary color to server:', error);
        }
    }

    applyTheme(theme) {
        const root = document.documentElement;
        
        // Remove existing theme
        root.removeAttribute('data-theme');
        
        if (theme === 'dark') {
            root.setAttribute('data-theme', 'dark');
        } else if (theme === 'auto') {
            // Detect system preference
            const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
            if (prefersDark) {
                root.setAttribute('data-theme', 'dark');
            }
        }
        // 'light' không cần set attribute
        
        // Dispatch event cho các component khác
        window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
    }

    applyPrimaryColor(color) {
        const root = document.documentElement;
        
        // Convert hex to RGB for alpha variants
        const rgb = this.hexToRgb(color);
        
        // Set CSS variables
        root.style.setProperty('--primary', color);
        root.style.setProperty('--primary-rgb', `${rgb.r}, ${rgb.g}, ${rgb.b}`);
        
        // Generate darker and lighter variants
        const darker = this.adjustBrightness(color, -20);
        const lighter = this.adjustBrightness(color, 40);
        
        root.style.setProperty('--primary-dark', darker);
        root.style.setProperty('--primary-light', lighter);
        
        // Dispatch event
        window.dispatchEvent(new CustomEvent('primaryColorChanged', { detail: { color } }));
    }

    hexToRgb(hex) {
        const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        } : { r: 16, g: 185, b: 129 };
    }

    adjustBrightness(hex, percent) {
        const rgb = this.hexToRgb(hex);
        
        const adjust = (value) => {
            const adjusted = value + (value * percent / 100);
            return Math.max(0, Math.min(255, Math.round(adjusted)));
        };
        
        const r = adjust(rgb.r);
        const g = adjust(rgb.g);
        const b = adjust(rgb.b);
        
        return `#${((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1)}`;
    }

    getTheme() {
        return this.currentTheme;
    }

    getPrimaryColor() {
        return this.currentPrimaryColor;
    }

    // Preset colors
    getPresetColors() {
        return [
            { name: 'Emerald', value: '#10b981', description: 'Xanh lá mặc định' },
            { name: 'Blue', value: '#3b82f6', description: 'Xanh dương' },
            { name: 'Purple', value: '#8b5cf6', description: 'Tím' },
            { name: 'Pink', value: '#ec4899', description: 'Hồng' },
            { name: 'Orange', value: '#f97316', description: 'Cam' },
            { name: 'Red', value: '#ef4444', description: 'Đỏ' },
            { name: 'Teal', value: '#14b8a6', description: 'Xanh ngọc' },
            { name: 'Indigo', value: '#6366f1', description: 'Chàm' }
        ];
    }
}

// Initialize global theme manager
window.themeManager = new ThemeManager();

// Export for modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ThemeManager;
}
