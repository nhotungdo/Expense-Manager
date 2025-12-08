// Chart.js Global Configuration for Modern UI
// Optimized for performance and visual appeal

// Set global Chart.js defaults
if (typeof Chart !== 'undefined') {
    Chart.defaults.font.family = "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif";
    Chart.defaults.font.size = 13;
    Chart.defaults.color = '#475569';
    Chart.defaults.borderColor = 'rgba(0, 0, 0, 0.05)';
    Chart.defaults.plugins.legend.display = true;
    Chart.defaults.plugins.tooltip.enabled = true;
    Chart.defaults.responsive = true;
    Chart.defaults.maintainAspectRatio = true;
    
    // Animation settings for better performance
    Chart.defaults.animation.duration = 800;
    Chart.defaults.animation.easing = 'easeInOutQuart';
    
    // Interaction settings
    Chart.defaults.interaction.mode = 'nearest';
    Chart.defaults.interaction.intersect = false;
}

// Modern color schemes
const ChartColors = {
    primary: {
        main: '#10b981',
        light: '#6ee7b7',
        dark: '#059669',
        alpha: (opacity) => `rgba(16, 185, 129, ${opacity})`
    },
    danger: {
        main: '#ef4444',
        light: '#fca5a5',
        dark: '#dc2626',
        alpha: (opacity) => `rgba(239, 68, 68, ${opacity})`
    },
    warning: {
        main: '#f59e0b',
        light: '#fbbf24',
        dark: '#d97706',
        alpha: (opacity) => `rgba(245, 158, 11, ${opacity})`
    },
    info: {
        main: '#3b82f6',
        light: '#93c5fd',
        dark: '#2563eb',
        alpha: (opacity) => `rgba(59, 130, 246, ${opacity})`
    },
    purple: {
        main: '#8b5cf6',
        light: '#c4b5fd',
        dark: '#7c3aed',
        alpha: (opacity) => `rgba(139, 92, 246, ${opacity})`
    },
    gradient: [
        '#ef4444', '#f59e0b', '#10b981', '#3b82f6',
        '#8b5cf6', '#ec4899', '#14b8a6', '#f97316',
        '#06b6d4', '#84cc16', '#a855f7', '#f43f5e'
    ]
};

// Pie Chart Configuration Template
const PieChartConfig = {
    getConfig: (data, labels, options = {}) => ({
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: options.colors || ChartColors.gradient,
                borderWidth: 3,
                borderColor: '#ffffff',
                hoverOffset: 15,
                hoverBorderWidth: 4,
                hoverBorderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: options.legendPosition || 'bottom',
                    labels: {
                        padding: 15,
                        font: {
                            size: 13,
                            family: "'Inter', sans-serif",
                            weight: '500'
                        },
                        usePointStyle: true,
                        pointStyle: 'circle',
                        boxWidth: 12,
                        boxHeight: 12
                    }
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: 'rgba(0, 0, 0, 0.85)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff',
                    borderColor: 'rgba(255, 255, 255, 0.1)',
                    borderWidth: 1,
                    padding: 12,
                    cornerRadius: 8,
                    titleFont: {
                        size: 14,
                        weight: 'bold'
                    },
                    bodyFont: {
                        size: 13
                    },
                    displayColors: true,
                    callbacks: options.tooltipCallbacks || {}
                }
            },
            animation: {
                animateRotate: true,
                animateScale: true,
                duration: 1000,
                easing: 'easeInOutQuart'
            },
            layout: {
                padding: {
                    top: 10,
                    bottom: 10
                }
            }
        }
    }),
    
    // Doughnut variant
    getDoughnutConfig: (data, labels, options = {}) => {
        const config = PieChartConfig.getConfig(data, labels, options);
        config.type = 'doughnut';
        config.options.cutout = options.cutout || '60%';
        return config;
    }
};

// Line Chart Configuration Template
const LineChartConfig = {
    getConfig: (datasets, labels, options = {}) => ({
        type: 'line',
        data: {
            labels: labels,
            datasets: datasets.map((dataset, index) => ({
                label: dataset.label,
                data: dataset.data,
                borderColor: dataset.color || ChartColors.gradient[index],
                backgroundColor: dataset.backgroundColor || `${dataset.color || ChartColors.gradient[index]}20`,
                tension: 0.4,
                fill: dataset.fill !== undefined ? dataset.fill : true,
                borderWidth: 3,
                pointRadius: 5,
                pointHoverRadius: 7,
                pointBackgroundColor: dataset.color || ChartColors.gradient[index],
                pointBorderColor: '#ffffff',
                pointBorderWidth: 2,
                pointHoverBorderWidth: 3
            }))
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false
            },
            plugins: {
                legend: {
                    display: options.showLegend !== undefined ? options.showLegend : true,
                    position: 'top',
                    labels: {
                        padding: 15,
                        font: {
                            size: 13,
                            weight: '500'
                        },
                        usePointStyle: true
                    }
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: 'rgba(0, 0, 0, 0.85)',
                    padding: 12,
                    cornerRadius: 8,
                    titleFont: {
                        size: 14,
                        weight: 'bold'
                    },
                    bodyFont: {
                        size: 13
                    },
                    callbacks: options.tooltipCallbacks || {}
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)',
                        drawBorder: false
                    },
                    ticks: {
                        padding: 10,
                        font: {
                            size: 12
                        },
                        callback: options.yAxisCallback || function(value) {
                            return value;
                        }
                    }
                },
                x: {
                    grid: {
                        display: false,
                        drawBorder: false
                    },
                    ticks: {
                        padding: 10,
                        font: {
                            size: 12
                        }
                    }
                }
            },
            animation: {
                duration: 1000,
                easing: 'easeInOutQuart'
            }
        }
    })
};

// Bar Chart Configuration Template
const BarChartConfig = {
    getConfig: (datasets, labels, options = {}) => ({
        type: 'bar',
        data: {
            labels: labels,
            datasets: datasets.map((dataset, index) => ({
                label: dataset.label,
                data: dataset.data,
                backgroundColor: dataset.color || ChartColors.gradient[index],
                borderColor: dataset.borderColor || 'transparent',
                borderWidth: 0,
                borderRadius: 8,
                borderSkipped: false
            }))
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: options.showLegend !== undefined ? options.showLegend : true,
                    position: 'top',
                    labels: {
                        padding: 15,
                        font: {
                            size: 13,
                            weight: '500'
                        },
                        usePointStyle: true
                    }
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: 'rgba(0, 0, 0, 0.85)',
                    padding: 12,
                    cornerRadius: 8,
                    callbacks: options.tooltipCallbacks || {}
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)',
                        drawBorder: false
                    },
                    ticks: {
                        padding: 10,
                        callback: options.yAxisCallback || function(value) {
                            return value;
                        }
                    }
                },
                x: {
                    grid: {
                        display: false,
                        drawBorder: false
                    },
                    ticks: {
                        padding: 10
                    }
                }
            }
        }
    })
};

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { ChartColors, PieChartConfig, LineChartConfig, BarChartConfig };
}
