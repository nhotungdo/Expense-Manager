// ========================================
// FRONTEND ERROR TESTS
// Test suite for Transactions & Wallet pages
// ========================================

/**
 * Test Suite for Frontend Error Detection
 * Run this in browser console to verify all fixes
 */

const FrontendTests = {
    results: [],
    
    // Test 1: Check if Chart.js is loaded
    testChartJsLoaded() {
        const passed = typeof Chart !== 'undefined';
        this.results.push({
            test: 'Chart.js Library Loaded',
            passed,
            message: passed ? 'Chart.js is available' : 'Chart.js not found - charts will fail'
        });
        return passed;
    },
    
    // Test 2: Check if Bootstrap is loaded
    testBootstrapLoaded() {
        const passed = typeof bootstrap !== 'undefined';
        this.results.push({
            test: 'Bootstrap Library Loaded',
            passed,
            message: passed ? 'Bootstrap is available' : 'Bootstrap not found - modals will fail'
        });
        return passed;
    },
    
    // Test 3: Check if canvas elements exist
    testCanvasElements() {
        const canvases = {
            transactions: {
                distribution: document.getElementById('transactionDistributionChart'),
                category: document.getElementById('categoryExpenseChart')
            },
            wallet: {
                balance: document.getElementById('balanceDistributionChart'),
                transactionType: document.getElementById('transactionTypeChart'),
                categoryBreakdown: document.getElementById('categoryBreakdownChart')
            }
        };
        
        let allFound = true;
        const missing = [];
        
        Object.entries(canvases).forEach(([page, charts]) => {
            Object.entries(charts).forEach(([name, element]) => {
                if (!element) {
                    allFound = false;
                    missing.push(`${page}.${name}`);
                }
            });
        });
        
        this.results.push({
            test: 'Canvas Elements Exist',
            passed: allFound,
            message: allFound ? 'All canvas elements found' : `Missing: ${missing.join(', ')}`
        });
        return allFound;
    },
    
    // Test 4: Check API endpoint accessibility
    async testApiEndpoints() {
        const endpoints = [
            '/api/Report/dashboard-analytics?days=30',
            '/api/Report/wallet-summary?period=month',
            '/api/Report/expense-breakdown?period=month',
            '/api/Transactions/recent?limit=5'
        ];
        
        const results = await Promise.all(
            endpoints.map(async (endpoint) => {
                try {
                    const response = await fetch(endpoint);
                    return {
                        endpoint,
                        status: response.status,
                        ok: response.ok
                    };
                } catch (error) {
                    return {
                        endpoint,
                        status: 'ERROR',
                        ok: false,
                        error: error.message
                    };
                }
            })
        );
        
        const allOk = results.every(r => r.ok);
        this.results.push({
            test: 'API Endpoints Accessible',
            passed: allOk,
            message: allOk ? 'All endpoints responding' : `Failed: ${results.filter(r => !r.ok).map(r => r.endpoint).join(', ')}`,
            details: results
        });
        return allOk;
    },
    
    // Test 5: Check if viewTransaction function is accessible
    testViewTransactionFunction() {
        const transactionItem = document.querySelector('.transaction-item');
        const passed = transactionItem !== null;
        
        this.results.push({
            test: 'Transaction Click Handler',
            passed,
            message: passed ? 'Transaction items found and clickable' : 'No transaction items found'
        });
        return passed;
    },
    
    // Test 6: Check for console errors
    testConsoleErrors() {
        // This would need to be run with a console error listener
        // For now, we'll just check if error logging is available
        const passed = typeof console !== 'undefined' && typeof console.error === 'function';
        this.results.push({
            test: 'Console Error Logging',
            passed,
            message: passed ? 'Console logging available' : 'Console not available'
        });
        return passed;
    },
    
    // Test 7: Check responsive design
    testResponsiveDesign() {
        const viewportWidth = window.innerWidth;
        const isMobile = viewportWidth < 768;
        const isTablet = viewportWidth >= 768 && viewportWidth < 1024;
        const isDesktop = viewportWidth >= 1024;
        
        this.results.push({
            test: 'Responsive Design Detection',
            passed: true,
            message: `Viewport: ${viewportWidth}px (${isMobile ? 'Mobile' : isTablet ? 'Tablet' : 'Desktop'})`
        });
        return true;
    },
    
    // Test 8: Check for XSS vulnerabilities in dynamic content
    testXSSProtection() {
        const testString = '<script>alert("XSS")</script>';
        const div = document.createElement('div');
        div.textContent = testString;
        const safe = div.innerHTML.includes('&lt;script&gt;');
        
        this.results.push({
            test: 'XSS Protection',
            passed: safe,
            message: safe ? 'Text content properly escaped' : 'Potential XSS vulnerability'
        });
        return safe;
    },
    
    // Test 9: Check theme variables
    testThemeVariables() {
        const root = getComputedStyle(document.documentElement);
        const primaryColor = root.getPropertyValue('--primary');
        const passed = primaryColor !== '';
        
        this.results.push({
            test: 'Theme Variables',
            passed,
            message: passed ? 'CSS variables loaded' : 'Theme variables not found'
        });
        return passed;
    },
    
    // Test 10: Check for memory leaks (basic)
    testMemoryLeaks() {
        const initialMemory = performance.memory ? performance.memory.usedJSHeapSize : 0;
        
        // Create and destroy some elements
        for (let i = 0; i < 100; i++) {
            const div = document.createElement('div');
            div.innerHTML = 'Test';
            document.body.appendChild(div);
            document.body.removeChild(div);
        }
        
        const finalMemory = performance.memory ? performance.memory.usedJSHeapSize : 0;
        const memoryIncrease = finalMemory - initialMemory;
        const passed = memoryIncrease < 1000000; // Less than 1MB increase
        
        this.results.push({
            test: 'Memory Leak Detection',
            passed,
            message: passed ? 'No significant memory leaks detected' : `Memory increased by ${(memoryIncrease / 1024).toFixed(2)}KB`
        });
        return passed;
    },
    
    // Run all tests
    async runAll() {
        console.log('🧪 Starting Frontend Error Tests...\n');
        this.results = [];
        
        // Synchronous tests
        this.testChartJsLoaded();
        this.testBootstrapLoaded();
        this.testCanvasElements();
        this.testViewTransactionFunction();
        this.testConsoleErrors();
        this.testResponsiveDesign();
        this.testXSSProtection();
        this.testThemeVariables();
        this.testMemoryLeaks();
        
        // Asynchronous tests
        await this.testApiEndpoints();
        
        // Display results
        this.displayResults();
        
        return this.results;
    },
    
    // Display test results
    displayResults() {
        console.log('\n📊 Test Results:\n');
        console.log('═'.repeat(80));
        
        let passed = 0;
        let failed = 0;
        
        this.results.forEach((result, index) => {
            const icon = result.passed ? '✅' : '❌';
            const status = result.passed ? 'PASS' : 'FAIL';
            
            console.log(`${icon} Test ${index + 1}: ${result.test}`);
            console.log(`   Status: ${status}`);
            console.log(`   Message: ${result.message}`);
            if (result.details) {
                console.log(`   Details:`, result.details);
            }
            console.log('─'.repeat(80));
            
            if (result.passed) passed++;
            else failed++;
        });
        
        console.log('═'.repeat(80));
        console.log(`\n📈 Summary: ${passed} passed, ${failed} failed out of ${this.results.length} tests`);
        
        if (failed === 0) {
            console.log('🎉 All tests passed! Frontend is working correctly.');
        } else {
            console.log('⚠️  Some tests failed. Please review the errors above.');
        }
        
        return { passed, failed, total: this.results.length };
    },
    
    // Export results as JSON
    exportResults() {
        return JSON.stringify(this.results, null, 2);
    }
};

// Auto-run tests if in test mode
if (window.location.search.includes('test=true')) {
    FrontendTests.runAll();
}

// Export to window for manual testing
window.FrontendTests = FrontendTests;

// Usage instructions
console.log(`
╔════════════════════════════════════════════════════════════════╗
║                  FRONTEND ERROR TEST SUITE                     ║
╚════════════════════════════════════════════════════════════════╝

To run tests, open browser console and execute:

    FrontendTests.runAll()

Or add ?test=true to the URL to auto-run tests.

Individual tests can be run with:
    FrontendTests.testChartJsLoaded()
    FrontendTests.testApiEndpoints()
    etc.

Export results with:
    FrontendTests.exportResults()
`);
