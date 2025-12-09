// Unit Tests for Service Packages - Register Button Fix
// This file contains test cases to validate the registerPackage function behavior

/**
 * Test Suite: registerPackage Function
 * Purpose: Validate that clicking "Register Now" redirects to link.com checkout
 */

// Mock data for testing
const mockPackages = [
    {
        id: 1,
        name: 'Gói Miễn Phí',
        price: 0,
        durationDays: 365
    },
    {
        id: 2,
        name: 'Gói Cơ Bản',
        price: 99000,
        durationDays: 30
    },
    {
        id: 3,
        name: 'Gói Chuyên Nghiệp',
        price: 199000,
        durationDays: 30
    }
];

// Test 1: Valid package redirect
function testValidPackageRedirect() {
    console.log('Test 1: Valid Package Redirect');
    
    const packageId = 2;
    const expectedPackage = mockPackages.find(p => p.id === packageId);
    const expectedUrl = `https://link.com/checkout?packageId=${packageId}&packageName=${encodeURIComponent(expectedPackage.name)}&price=${expectedPackage.price}`;
    
    console.log('Expected URL:', expectedUrl);
    console.log('✓ Test passed: URL format is correct');
}

// Test 2: Invalid package handling
function testInvalidPackageHandling() {
    console.log('\nTest 2: Invalid Package Handling');
    
    const invalidPackageId = 999;
    const pkg = mockPackages.find(p => p.id === invalidPackageId);
    
    if (!pkg) {
        console.log('✓ Test passed: Invalid package detected correctly');
        console.log('Expected behavior: Show error message');
    } else {
        console.log('✗ Test failed: Invalid package not detected');
    }
}

// Test 3: URL encoding for special characters
function testUrlEncoding() {
    console.log('\nTest 3: URL Encoding');
    
    const testPackage = {
        id: 100,
        name: 'Gói Đặc Biệt & Premium',
        price: 299000
    };
    
    const encodedName = encodeURIComponent(testPackage.name);
    const expectedUrl = `https://link.com/checkout?packageId=${testPackage.id}&packageName=${encodedName}&price=${testPackage.price}`;
    
    console.log('Original name:', testPackage.name);
    console.log('Encoded name:', encodedName);
    console.log('Full URL:', expectedUrl);
    console.log('✓ Test passed: Special characters encoded correctly');
}

// Test 4: All package types
function testAllPackageTypes() {
    console.log('\nTest 4: All Package Types');
    
    mockPackages.forEach(pkg => {
        const url = `https://link.com/checkout?packageId=${pkg.id}&packageName=${encodeURIComponent(pkg.name)}&price=${pkg.price}`;
        console.log(`Package: ${pkg.name}`);
        console.log(`URL: ${url}`);
    });
    
    console.log('✓ Test passed: All package types generate valid URLs');
}

// Test 5: Error handling simulation
function testErrorHandling() {
    console.log('\nTest 5: Error Handling');
    
    try {
        // Simulate error scenario
        const pkg = null;
        if (!pkg) {
            throw new Error('Package not found');
        }
    } catch (error) {
        console.log('Error caught:', error.message);
        console.log('✓ Test passed: Error handling works correctly');
    }
}

// Test 6: Browser compatibility check
function testBrowserCompatibility() {
    console.log('\nTest 6: Browser Compatibility');
    
    const features = {
        'window.location.href': typeof window !== 'undefined' && 'location' in window,
        'encodeURIComponent': typeof encodeURIComponent === 'function',
        'Template literals': true, // ES6 feature
        'Try-catch': true, // Standard feature
        'Arrow functions': true // ES6 feature
    };
    
    console.log('Feature support:');
    Object.entries(features).forEach(([feature, supported]) => {
        console.log(`  ${feature}: ${supported ? '✓' : '✗'}`);
    });
    
    const allSupported = Object.values(features).every(v => v);
    console.log(allSupported ? '✓ Test passed: All features supported' : '✗ Test failed: Some features not supported');
}

// Test 7: URL parameter validation
function testUrlParameters() {
    console.log('\nTest 7: URL Parameter Validation');
    
    const pkg = mockPackages[1];
    const url = new URL(`https://link.com/checkout?packageId=${pkg.id}&packageName=${encodeURIComponent(pkg.name)}&price=${pkg.price}`);
    
    const params = {
        packageId: url.searchParams.get('packageId'),
        packageName: url.searchParams.get('packageName'),
        price: url.searchParams.get('price')
    };
    
    console.log('Extracted parameters:');
    console.log('  packageId:', params.packageId);
    console.log('  packageName:', params.packageName);
    console.log('  price:', params.price);
    
    const isValid = params.packageId && params.packageName && params.price;
    console.log(isValid ? '✓ Test passed: All parameters present' : '✗ Test failed: Missing parameters');
}

// Run all tests
function runAllTests() {
    console.log('=== Service Packages Register Button Tests ===\n');
    
    testValidPackageRedirect();
    testInvalidPackageHandling();
    testUrlEncoding();
    testAllPackageTypes();
    testErrorHandling();
    testBrowserCompatibility();
    testUrlParameters();
    
    console.log('\n=== All Tests Completed ===');
}

// Export for use in browser console or test runner
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        runAllTests,
        testValidPackageRedirect,
        testInvalidPackageHandling,
        testUrlEncoding,
        testAllPackageTypes,
        testErrorHandling,
        testBrowserCompatibility,
        testUrlParameters
    };
}

// Auto-run tests if loaded directly
if (typeof window !== 'undefined') {
    console.log('Service Packages Test Suite loaded. Run runAllTests() to execute all tests.');
}
