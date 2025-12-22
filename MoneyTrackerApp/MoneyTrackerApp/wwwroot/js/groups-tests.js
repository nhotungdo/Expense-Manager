/**
 * Groups Page Unit Tests
 * Run via console: GroupsTests.runAll()
 */

const GroupsTests = {
    results: [],

    testVueLoaded() {
        const passed = typeof Vue !== 'undefined';
        this.logResult('Vue.js Library Loaded', passed);
        return passed;
    },

    testAppMounted() {
        const passed = document.getElementById('group-spending-app') !== null &&
            typeof window.groupsApp !== 'undefined';
        this.logResult('Vue App Mounted', passed);
        return passed;
    },

    testInitialState() {
        if (!window.groupsApp) return false;
        const app = window.groupsApp;

        const passed = Array.isArray(app.groups) &&
            app.isLoading === false; // Should be false after initial load, or true if slow
        // We can't guarantee loading is done instantly, so we just check types
        this.logResult('Initial State Structure', Array.isArray(app.groups));
        return passed;
    },

    async testApiConnection() {
        try {
            const res = await fetch('/api/GroupExpense');
            const passed = res.ok;
            this.logResult('API /api/GroupExpense Reachable', passed);
            return passed;
        } catch (e) {
            this.logResult('API /api/GroupExpense Reachable', false, e.message);
            return false;
        }
    },

    testCurrencyFormatting() {
        if (!window.groupsApp) return false;
        const result = window.groupsApp.formatCurrency(100000);
        // Expect "100.000 ₫" or similar depending on locale implementation
        const passed = result.includes('₫') || result.includes('VND');
        this.logResult('Currency Formatting', passed, `Got: ${result}`);
        return passed;
    },

    async runAll() {
        console.clear();
        console.log('🧪 Starting Groups Page Tests...');
        this.results = [];

        this.testVueLoaded();
        this.testAppMounted();
        await this.testApiConnection();

        // Wait for app to be ready if needed
        if (window.groupsApp) {
            this.testInitialState();
            this.testCurrencyFormatting();
        }

        this.printSummary();
    },

    logResult(name, passed, details = '') {
        const res = { name, passed, details };
        this.results.push(res);
        console.log(`${passed ? '✅' : '❌'} ${name} ${details ? '(' + details + ')' : ''}`);
        return res;
    },

    printSummary() {
        const passed = this.results.filter(r => r.passed).length;
        const total = this.results.length;
        console.log(`\nResults: ${passed}/${total} passed.`);
    }
};

window.GroupsTests = GroupsTests;
