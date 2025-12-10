/**
 * Automated Test Script for Checkout Page
 * Run this in browser console to test checkout functionality
 */

const CheckoutTests = {
    results: [],
    
    // Test utilities
    assert(condition, testName, message) {
        const result = {
            test: testName,
            passed: condition,
            message: message || (condition ? 'Passed' : 'Failed'),
            timestamp: new Date().toISOString()
        };
        this.results.push(result);
        
        const icon = condition ? '✅' : '❌';
        const color = condition ? 'color: green' : 'color: red';
        console.log(`%c${icon} ${testName}`, color, message);
        
        return condition;
    },
    
    async wait(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    },
    
    // Test 1: Check if all required elements exist
    async testDOMElements() {
        console.log('\n🧪 Test 1: DOM Elements');
        
        const elements = {
            'Loading Overlay': document.getElementById('loadingOverlay'),
            'Confirm Modal': document.getElementById('confirmModal'),
            'Checkout Form': document.getElementById('checkoutForm'),
            'Submit Button': document.getElementById('submitButton'),
            'Package Name': document.getElementById('packageName'),
            'Package Description': document.getElementById('packageDescription'),
            'Subtotal': document.getElementById('subtotal'),
            'Total': document.getElementById('total'),
            'Terms Checkbox': document.getElementById('agreeTerms'),
            'Auto Renew Checkbox': document.getElementById('agreeAutoRenew')
        };
        
        for (const [name, element] of Object.entries(elements)) {
            this.assert(
                element !== null,
                `DOM Element: ${name}`,
                element ? 'Element found' : 'Element not found'
            );
        }
    },
    
    // Test 2: Check if package data is loaded
    async testPackageLoading() {
        console.log('\n🧪 Test 2: Package Loading');
        
        await this.wait(2000); // Wait for package to load
        
        const packageName = document.getElementById('packageName')?.textContent;
        const packageDesc = document.getElementById('packageDescription')?.textContent;
        const subtotal = document.getElementById('subtotal')?.textContent;
        
        this.assert(
            packageName && packageName !== 'Đang tải...',
            'Package Name Loaded',
            `Package: ${packageName}`
        );
        
        this.assert(
            packageDesc && packageDesc !== 'Vui lòng đợi...',
            'Package Description Loaded',
            `Description: ${packageDesc}`
        );
        
        this.assert(
            subtotal && subtotal !== '0 ₫',
            'Package Price Loaded',
            `Price: ${subtotal}`
        );
        
        // Check if selectedPackage global variable exists
        this.assert(
            typeof selectedPackage !== 'undefined' && selectedPackage !== null,
            'Global selectedPackage Variable',
            selectedPackage ? `Package ID: ${selectedPackage.id}` : 'Not set'
        );
    },
    
    // Test 3: Test form validation
    async testFormValidation() {
        console.log('\n🧪 Test 3: Form Validation');
        
        const termsCheckbox = document.getElementById('agreeTerms');
        const autoRenewCheckbox = document.getElementById('agreeAutoRenew');
        
        this.assert(
            termsCheckbox !== null,
            'Terms Checkbox Exists',
            'Found'
        );
        
        this.assert(
            autoRenewCheckbox !== null,
            'Auto Renew Checkbox Exists',
            'Found'
        );
        
        // Set to valid state for other tests
        if (termsCheckbox) termsCheckbox.checked = true;
        if (autoRenewCheckbox) autoRenewCheckbox.checked = true;
    },
    
    // Test 4: Test payment method selection
    async testPaymentMethodSelection() {
        console.log('\n🧪 Test 4: Payment Method Selection');
        
        const vnpayRadio = document.querySelector('input[name="paymentMethod"][value="vnpay"]');
        const momoRadio = document.querySelector('input[name="paymentMethod"][value="momo"]');
        const bankRadio = document.querySelector('input[name="paymentMethod"][value="bank"]');
        
        this.assert(
            vnpayRadio !== null,
            'VNPay Radio Button Exists',
            'Found'
        );
        
        this.assert(
            vnpayRadio.checked,
            'VNPay Selected by Default',
            'Default selection correct'
        );
        
        // Test switching payment methods
        if (momoRadio) {
            momoRadio.checked = true;
            this.assert(
                momoRadio.checked && !vnpayRadio.checked,
                'Switch to MoMo',
                'Successfully switched'
            );
            
            // Switch back to VNPay
            vnpayRadio.checked = true;
        }
    },
    
    // Test 5: Test modal functionality
    async testModalFunctionality() {
        console.log('\n🧪 Test 5: Modal Functionality');
        
        const modal = document.getElementById('confirmModal');
        const initialDisplay = window.getComputedStyle(modal).display;
        
        this.assert(
            initialDisplay === 'none',
            'Modal Initially Hidden',
            `Display: ${initialDisplay}`
        );
        
        // Test showing modal
        if (typeof showConfirmModal === 'function') {
            showConfirmModal();
            await this.wait(500);
            
            const displayAfterShow = window.getComputedStyle(modal).display;
            this.assert(
                displayAfterShow === 'flex',
                'Modal Shows Correctly',
                `Display: ${displayAfterShow}`
            );
            
            // Test closing modal
            if (typeof closeConfirmModal === 'function') {
                closeConfirmModal();
                await this.wait(500);
                
                const displayAfterClose = window.getComputedStyle(modal).display;
                this.assert(
                    displayAfterClose === 'none',
                    'Modal Closes Correctly',
                    `Display: ${displayAfterClose}`
                );
            }
        }
    },
    
    // Test 6: Test loading overlay
    async testLoadingOverlay() {
        console.log('\n🧪 Test 6: Loading Overlay');
        
        const overlay = document.getElementById('loadingOverlay');
        const initialDisplay = window.getComputedStyle(overlay).display;
        
        this.assert(
            initialDisplay === 'none',
            'Loading Overlay Initially Hidden',
            `Display: ${initialDisplay}`
        );
        
        // Test showing overlay
        if (typeof showLoadingOverlay === 'function') {
            showLoadingOverlay();
            await this.wait(500);
            
            const displayAfterShow = window.getComputedStyle(overlay).display;
            this.assert(
                displayAfterShow === 'flex',
                'Loading Overlay Shows Correctly',
                `Display: ${displayAfterShow}`
            );
            
            // Test hiding overlay
            if (typeof hideLoadingOverlay === 'function') {
                hideLoadingOverlay();
                await this.wait(500);
                
                const displayAfterHide = window.getComputedStyle(overlay).display;
                this.assert(
                    displayAfterHide === 'none',
                    'Loading Overlay Hides Correctly',
                    `Display: ${displayAfterHide}`
                );
            }
        }
    },
    
    // Test 7: Test user authentication check
    async testUserAuthentication() {
        console.log('\n🧪 Test 7: User Authentication');
        
        const userId = getUserId();
        const userEmail = getUserEmail();
        const isAuthenticated = localStorage.getItem('isAuthenticated');
        
        this.assert(
            userId !== null,
            'User ID Retrieved',
            `User ID: ${userId}`
        );
        
        this.assert(
            userEmail !== null && userEmail !== '',
            'User Email Retrieved',
            `Email: ${userEmail}`
        );
        
        console.log(`ℹ️ Authentication Status: ${isAuthenticated}`);
    },
    
    // Test 8: Test currency formatting
    async testCurrencyFormatting() {
        console.log('\n🧪 Test 8: Currency Formatting');
        
        const testCases = [
            { input: 99000, expected: '99.000' },
            { input: 199000, expected: '199.000' },
            { input: 1000000, expected: '1.000.000' }
        ];
        
        for (const testCase of testCases) {
            const formatted = formatCurrency(testCase.input);
            this.assert(
                formatted === testCase.expected,
                `Format ${testCase.input}`,
                `Expected: ${testCase.expected}, Got: ${formatted}`
            );
        }
    },
    
    // Test 9: Test responsive design
    async testResponsiveDesign() {
        console.log('\n🧪 Test 9: Responsive Design');
        
        const checkoutWrapper = document.querySelector('.checkout-wrapper');
        const submitButton = document.getElementById('submitButton');
        
        if (checkoutWrapper) {
            const styles = window.getComputedStyle(checkoutWrapper);
            const display = styles.display;
            
            this.assert(
                display === 'grid',
                'Checkout Wrapper Uses Grid Layout',
                `Display: ${display}`
            );
        }
        
        if (submitButton) {
            const styles = window.getComputedStyle(submitButton);
            const width = styles.width;
            
            this.assert(
                width !== '0px',
                'Submit Button Has Width',
                `Width: ${width}`
            );
        }
        
        // Check viewport
        const viewportWidth = window.innerWidth;
        const isMobile = viewportWidth < 768;
        const isTablet = viewportWidth >= 768 && viewportWidth < 1024;
        const isDesktop = viewportWidth >= 1024;
        
        console.log(`ℹ️ Viewport: ${viewportWidth}px (${isMobile ? 'Mobile' : isTablet ? 'Tablet' : 'Desktop'})`);
    },
    
    // Test 10: Test session storage
    async testSessionStorage() {
        console.log('\n🧪 Test 10: Session Storage');
        
        // Test setting values
        sessionStorage.setItem('testKey', 'testValue');
        const retrieved = sessionStorage.getItem('testKey');
        
        this.assert(
            retrieved === 'testValue',
            'Session Storage Works',
            'Can set and retrieve values'
        );
        
        // Check for existing payment data
        const lastTransactionId = sessionStorage.getItem('lastTransactionId');
        const lastPaymentMethod = sessionStorage.getItem('lastPaymentMethod');
        
        console.log(`ℹ️ Last Transaction ID: ${lastTransactionId || 'None'}`);
        console.log(`ℹ️ Last Payment Method: ${lastPaymentMethod || 'None'}`);
        
        // Cleanup
        sessionStorage.removeItem('testKey');
    },
    
    // Run all tests
    async runAll() {
        console.clear();
        console.log('🚀 Starting Checkout Page Tests...\n');
        console.log('═'.repeat(50));
        
        this.results = [];
        const startTime = Date.now();
        
        try {
            await this.testDOMElements();
            await this.testPackageLoading();
            await this.testFormValidation();
            await this.testPaymentMethodSelection();
            await this.testModalFunctionality();
            await this.testLoadingOverlay();
            await this.testUserAuthentication();
            await this.testCurrencyFormatting();
            await this.testResponsiveDesign();
            await this.testSessionStorage();
        } catch (error) {
            console.error('❌ Test execution error:', error);
        }
        
        const endTime = Date.now();
        const duration = ((endTime - startTime) / 1000).toFixed(2);
        
        // Summary
        console.log('\n' + '═'.repeat(50));
        console.log('📊 Test Summary\n');
        
        const passed = this.results.filter(r => r.passed).length;
        const failed = this.results.filter(r => !r.passed).length;
        const total = this.results.length;
        const passRate = ((passed / total) * 100).toFixed(1);
        
        console.log(`Total Tests: ${total}`);
        console.log(`%c✅ Passed: ${passed}`, 'color: green; font-weight: bold');
        console.log(`%c❌ Failed: ${failed}`, 'color: red; font-weight: bold');
        console.log(`Pass Rate: ${passRate}%`);
        console.log(`Duration: ${duration}s`);
        
        if (failed > 0) {
            console.log('\n❌ Failed Tests:');
            this.results.filter(r => !r.passed).forEach(r => {
                console.log(`  - ${r.test}: ${r.message}`);
            });
        }
        
        console.log('\n' + '═'.repeat(50));
        
        // Return results for programmatic access
        return {
            passed,
            failed,
            total,
            passRate,
            duration,
            results: this.results
        };
    },
    
    // Export results as JSON
    exportResults() {
        const summary = {
            timestamp: new Date().toISOString(),
            userAgent: navigator.userAgent,
            viewport: {
                width: window.innerWidth,
                height: window.innerHeight
            },
            results: this.results
        };
        
        const json = JSON.stringify(summary, null, 2);
        console.log('📄 Test Results (JSON):\n', json);
        
        // Copy to clipboard if available
        if (navigator.clipboard) {
            navigator.clipboard.writeText(json).then(() => {
                console.log('✅ Results copied to clipboard!');
            });
        }
        
        return summary;
    }
};

// Auto-run tests when script is loaded
console.log('💡 Checkout Test Suite Loaded!');
console.log('Run tests with: CheckoutTests.runAll()');
console.log('Export results with: CheckoutTests.exportResults()');

// Expose to window for easy access
window.CheckoutTests = CheckoutTests;
